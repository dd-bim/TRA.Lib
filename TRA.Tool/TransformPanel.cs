using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using OSGeo.OSR;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using TRA_Lib;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;
using System.Diagnostics;

namespace TRA.Tool
{
    public partial class TransformPanel : UserControl
    {
        public TransformPanel()
        {
            InitializeComponent();
        }

        private void TransformPanel_MouseDown(object sender, MouseEventArgs e)
        {
            DoDragDrop(this, DragDropEffects.Move);
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180.0);
        }

        private void btn_Transform_Click(object sender, EventArgs e)
        {
            //Get Target SRS

            FlowLayoutPanel owner = Parent as FlowLayoutPanel;
            if (owner == null) { return; }
            int idx = owner.Controls.GetChildIndex(this) - 1;

            while (idx >= 0 && owner.Controls[idx].GetType() != typeof(TransformPanel))
            {
                if (owner.Controls[idx].GetType() == typeof(TrassenPanel))
                {
                    TrassenPanel panel = (TrassenPanel)owner.Controls[idx];
                    IEnumerable<TrassenElementExt> elements = Enumerable.Empty<TrassenElementExt>();
                    if (panel.trasseL != null) { elements = elements.Concat(panel.trasseL.Elemente); }
                    if (panel.trasseS != null) { elements = elements.Concat(panel.trasseS.Elemente); }
                    if (panel.trasseR != null) { elements = elements.Concat(panel.trasseR.Elemente); }
                    double previousdK = double.NaN;
                    foreach (TrassenElementExt element in elements.Reverse()) //run reverse for having X/Yend from the successor is already transformed for plausability checks 
                    {
                        //Transform Interpolation Points
                        Interpolation interp = element.InterpolationResult;
                        double elementHeight = interp.H != null ? interp.H[0] : double.NaN; //save original height befor transforming
                        // TODO how to handle trasse without heights (like S) in transformations
                        if (interp.H == null) { interp.H = new double[interp.X.Length]; }
                        if (interp.X.Length > 0 && interp.H != null)
                        {
                            try
                            {
                                double[][] points = { interp.Y, interp.X, interp.H };
                                //Srs.ConvertInPlace(ref points, SRS_Source, SRS_Target);
                                double[] gamma_i, k_i, gamma_o, k_o = new double[interp.X.Length];
                                egbt22lib.Convert.DBRef_GK5_Gamma_k(points[0], points[1], out gamma_i, out k_i);
                                egbt22lib.Convert.DBRef_GK5_to_EGBT22_Local(points[0], points[1], points[2], out points[0], out points[1]);
                                egbt22lib.Convert.EGBT22_Local_Gamma_k(points[0], points[1], out gamma_o, out k_o);
                                //Workaround to set values in place
                                for (int i = 0; i < interp.X.Length; i++)
                                {
                                    interp.X[i] = points[1][i];
                                    interp.Y[i] = points[0][i];
                                    interp.H[i] = points[2][i];
                                    interp.T[i] = interp.T[i] - DegreesToRadians(gamma_o[i] - gamma_i[i]);
                                }
                            }
                            catch
                            {
                            }
                        }
                        //Transform Element
                        try
                        {
                            double gamma_i, k_i, gamma_o, k_o;
                            double rechts, hoch;
                            (gamma_i, k_i) = egbt22lib.Convert.DBRef_GK5_Gamma_k(element.Ystart, element.Xstart);
                            (rechts, hoch) = egbt22lib.Convert.DBRef_GK5_to_EGBT22_Local(element.Ystart, element.Xstart, Double.IsNaN(elementHeight) ? 0.0 : elementHeight);
                            (gamma_o, k_o) = egbt22lib.Convert.EGBT22_Local_Gamma_k(rechts, hoch);
                            double dK = (k_o / k_i);
                            element.Relocate(hoch, rechts, DegreesToRadians(gamma_o - gamma_i), dK, previousdK);
                            previousdK = element.ID == 0 ? double.NaN : dK; //As we iterate reverse(!) over all elements of all Trasses, we need to reset previousScale for next Trasse
                            if (checkBox_RecalcHeading.Checked) //recalculate a optimized Heading.
                            {
                                double Xi, Yi; //end-coordinates calculated from geometry 
                                (Xi, Yi, _) = element.GetPointAtS(element.L, true);
                                double gammai = Math.Atan2(Xi - element.Xstart, Yi - element.Ystart); //heading(Richtungswinkel) from geometry
                                double gammat = Math.Atan2(element.Xend - element.Xstart, element.Yend - element.Ystart); //heading(Richtungswinkel) from element start points
                                element.Relocate(deltaGamma: gammat - gammai);
                            }
                        }
                        catch
                        {
                        }
                    }

                    if (panel.trasseL != null)
                    {
                        foreach (TrassenElementExt element in panel.trasseL.Elemente)
                        {
                            element.ClearProjections();
                            Interpolation interp = element.InterpolationResult;
                            float deviation = panel.trasseL.ProjectPoints(interp.X, interp.Y);
                            string ownerString = panel.trasseL.Filename + "_" + element.ID;
                            TrassierungLog.Logger?.LogInformation(ownerString + " " + "Deviation to geometry after transform: " + deviation);
                        }
                        panel.trasseL.CalcGradientCoordinates();
                        panel.trasseL.Plot();
                    }
                    if (panel.trasseS != null)
                    {
                        foreach (TrassenElementExt element in panel.trasseS.Elemente)
                        {
                            element.ClearProjections();
                            Interpolation interp = element.InterpolationResult;
                            float deviation = panel.trasseS.ProjectPoints(interp.X, interp.Y);
                            string ownerString = panel.trasseS.Filename + "_" + element.ID;
                            TrassierungLog.Logger?.LogInformation(ownerString + " " + "Deviation to geometry after transform: " + deviation);
                        }
                        panel.trasseS.Plot();
                    }
                    if (panel.trasseR != null)
                    {
                        foreach (TrassenElementExt element in panel.trasseR.Elemente)
                        {
                            element.ClearProjections();
                            Interpolation interp = element.InterpolationResult;
                            float deviation = panel.trasseR.ProjectPoints(interp.X, interp.Y);
                            string ownerString = panel.trasseR.Filename + "_" + element.ID;
                            TrassierungLog.Logger?.LogInformation(ownerString + " " + "Deviation to geometry after transform: " + deviation);
                        }
                        panel.trasseR.CalcGradientCoordinates();
                        panel.trasseR.Plot();
                    }
                }
                idx--;
            }
        }
    }
}
