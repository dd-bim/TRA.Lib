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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Reflection;

namespace TRA.Tool
{
    public partial class TransformPanel : UserControl
    {
        public TransformPanel()
        {
            InitializeComponent();
            foreach (ETransforms value in Enum.GetValues(typeof(ETransforms)))
            {
                DescriptionAttribute attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(value.GetType().GetField(value.ToString()), typeof(DescriptionAttribute));
                comboBox_Transform.Items.Add(attribute == null ? value.ToString() : attribute.Description);
            }
            comboBox_Transform.SelectedIndex = 0;
        }

        private void TransformPanel_MouseDown(object sender, MouseEventArgs e)
        {
            DoDragDrop(this, DragDropEffects.Move);
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180.0);
        }

        private enum ETransforms
        {
            [Description("DBRef_GK5 -> EGBT22_Local")]
            DBRef_GK5_to_EGBT22_Local,
            [Description("EGBT22_Local -> DBRef_GK5")]
            EGBT22_Local_to_DBRef_GK5,
            [Description("DBRef_GK5 -> ETRS89_UTM33")]
            DBRef_GK5_to_ETRS89_UTM33,
            [Description("ETRS89_UTM33 -> DBRef_GK5")]
            ETRS89_UTM33_to_DBRef_GK5
        }
        private delegate (double Rechts, double Hoch, double H) SingleCoordinateTransform(double InRechts, double InHoch, double InH);
        private delegate bool ArrayCoordinateTransform(double[] InRechts, double[] InHoch, double[] InH, out double[] OutRechts, out double[] OutHoch, out double[] OutH);
        private delegate (double gamma, double k) Single_Gamma_k(double Rechts, double Hoch);
        private delegate bool Array_Gamma_k(double[] Rechts, double[] Hoch, out double[] gamma, out double[] k);

        struct TransformSetup
        {
            public SingleCoordinateTransform singleCoordinateTransform = null;
            public ArrayCoordinateTransform arrayCoordinateTransform = null;
            public Single_Gamma_k singleIn_Gamma_k = null;
            public Array_Gamma_k arrayIn_Gamma_K = null;
            public Single_Gamma_k singleOut_Gamma_k = null;
            public Array_Gamma_k arrayOut_Gamma_K = null;

            public TransformSetup()
            {
            }
        }
        private void TrassenTransform(TRATrasse trasse, TransformSetup transformSetup)
        {
            double previousdK = double.NaN;
            foreach (TrassenElementExt element in trasse.Elemente.Reverse()) //run reverse for having X/Yend from the successor is already transformed for plausability checks 
            {
                //Transform Interpolation Points
                Interpolation interp = element.InterpolationResult;
                if (interp.X == null) break;
                double elementHeight = interp.H != null ? interp.H[0] : double.NaN; //save original height befor transforming
                                                                                    // TODO how to handle trasse without heights (like S) in transformations
                if (interp.H == null) { interp.H = new double[interp.X.Length]; }
                if (interp.X.Length > 0 && interp.H != null)
                {
                    try
                    {
                        double[][] points = { interp.Y, interp.X, interp.H };
                        double[] zeros = new double[interp.Y.Length];
                        double[] gamma_i, k_i, gamma_o, k_o = new double[interp.X.Length];
                        transformSetup.arrayIn_Gamma_K(points[0], points[1], out gamma_i, out k_i);
                        // TODO transform interpolation points also at zero level?
                        //egbt22lib.Convert.DBRef_GK5_to_EGBT22_Local_Ell(points[0], points[1], points[2], out points[0], out points[1],out points[2]);
                        transformSetup.arrayCoordinateTransform(points[0], points[1], zeros, out points[0], out points[1], out zeros);
                        transformSetup.arrayOut_Gamma_K(points[0], points[1], out gamma_o, out k_o);
                        //Workaround to set values in place
                        for (int i = 0; i < interp.X.Length; i++)
                        {
                            interp.X[i] = points[1][i];
                            interp.Y[i] = points[0][i];
                            interp.H[i] = interp.H[i] + zeros[i]; //TODO is that expected behaviour
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
                    (gamma_i, k_i) = transformSetup.singleIn_Gamma_k(element.Ystart, element.Xstart);
                    (rechts, hoch, _) = transformSetup.singleCoordinateTransform(element.Ystart, element.Xstart, 0.0);
                    (gamma_o, k_o) = transformSetup.singleOut_Gamma_k(rechts, hoch);
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
        }
        private void btn_Transform_Click(object sender, EventArgs e)
        {
            TransformSetup transformSetup = new TransformSetup();
            ETransforms eTransform = (ETransforms)comboBox_Transform.SelectedIndex;
            //Get Target SRS
            switch (eTransform)
            {
                case ETransforms.DBRef_GK5_to_EGBT22_Local:
                    transformSetup.singleCoordinateTransform = egbt22lib.Convert.DBRef_GK5_to_EGBT22_Local_Ell;
                    transformSetup.arrayCoordinateTransform = egbt22lib.Convert.DBRef_GK5_to_EGBT22_Local_Ell;
                    transformSetup.singleIn_Gamma_k = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    transformSetup.arrayIn_Gamma_K = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    transformSetup.singleOut_Gamma_k = egbt22lib.Convert.EGBT22_Local_Gamma_k;
                    transformSetup.arrayOut_Gamma_K = egbt22lib.Convert.EGBT22_Local_Gamma_k;
                    break;
                case ETransforms.EGBT22_Local_to_DBRef_GK5:
                    transformSetup.singleCoordinateTransform = egbt22lib.Convert.EGBT22_Local_to_DBRef_GK5_Ell;
                    transformSetup.arrayCoordinateTransform = egbt22lib.Convert.EGBT22_Local_to_DBRef_GK5_Ell;
                    transformSetup.singleIn_Gamma_k = egbt22lib.Convert.EGBT22_Local_Gamma_k;
                    transformSetup.arrayIn_Gamma_K = egbt22lib.Convert.EGBT22_Local_Gamma_k;
                    transformSetup.singleOut_Gamma_k = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    transformSetup.arrayOut_Gamma_K = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    break;
                case ETransforms.DBRef_GK5_to_ETRS89_UTM33:
                    transformSetup.singleCoordinateTransform = egbt22lib.Convert.DBRef_GK5_to_ETRS89_UTM33_Ell;
                    transformSetup.arrayCoordinateTransform = egbt22lib.Convert.DBRef_GK5_to_ETRS89_UTM33_Ell;
                    transformSetup.singleIn_Gamma_k = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    transformSetup.arrayIn_Gamma_K = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    transformSetup.singleOut_Gamma_k = egbt22lib.Convert.ETRS89_UTM33_Gamma_k;
                    transformSetup.arrayOut_Gamma_K = egbt22lib.Convert.ETRS89_UTM33_Gamma_k;
                    break;
                case ETransforms.ETRS89_UTM33_to_DBRef_GK5:
                    transformSetup.singleCoordinateTransform = egbt22lib.Convert.ETRS89_UTM33_to_DBRef_GK5_Ell;
                    transformSetup.arrayCoordinateTransform = egbt22lib.Convert.ETRS89_UTM33_to_DBRef_GK5_Ell;
                    transformSetup.singleIn_Gamma_k = egbt22lib.Convert.ETRS89_UTM33_Gamma_k;
                    transformSetup.arrayIn_Gamma_K = egbt22lib.Convert.ETRS89_UTM33_Gamma_k;
                    transformSetup.singleOut_Gamma_k = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    transformSetup.arrayOut_Gamma_K = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    break;
                default:
                    transformSetup.singleCoordinateTransform = egbt22lib.Convert.DBRef_GK5_to_EGBT22_Local_Ell;
                    transformSetup.arrayCoordinateTransform = egbt22lib.Convert.DBRef_GK5_to_EGBT22_Local_Ell;
                    transformSetup.singleIn_Gamma_k = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    transformSetup.arrayIn_Gamma_K = egbt22lib.Convert.DBRef_GK5_Gamma_k;
                    transformSetup.singleOut_Gamma_k = egbt22lib.Convert.EGBT22_Local_Gamma_k;
                    transformSetup.arrayOut_Gamma_K = egbt22lib.Convert.EGBT22_Local_Gamma_k;
                    break;
            }

            FlowLayoutPanel owner = Parent as FlowLayoutPanel;
            if (owner == null) { return; }
            int idx = owner.Controls.GetChildIndex(this) - 1;

            while (idx >= 0 && owner.Controls[idx].GetType() != typeof(TransformPanel))
            {
                if (owner.Controls[idx].GetType() == typeof(TrassenPanel))
                {
                    TrassenPanel panel = (TrassenPanel)owner.Controls[idx];
                    TrassenTransform(panel.trasseL, transformSetup);
                    TrassenTransform(panel.trasseS, transformSetup);
                    TrassenTransform(panel.trasseR, transformSetup);
                   
                    // Transform Gradient Elements
                    //IEnumerable<GradientElementExt> GRAelements = Enumerable.Empty<GradientElementExt>();
                    //if (panel.gradientL != null) { GRAelements = GRAelements.Concat(panel.gradientL.GradientenElemente); }
                    //if (panel.gradientR != null) { GRAelements = GRAelements.Concat(panel.gradientR.GradientenElemente); }
                    //foreach (GradientElementExt element in GRAelements)
                    //{
                    //    //Transform GradientElement
                    //    try
                    //    {
                    //        double H;
                    //        (_, _, H) = transformSetup.singleCoordinateTransform(element.Y, element.X, element.H);
                    //        element.Relocate(H);
                    //    }
                    //    catch
                    //    {
                    //    }
                    //}

                    //Calc Deviations
                    IEnumerable<TrassenElementExt> elements = Enumerable.Empty<TrassenElementExt>();
                    if (panel.trasseL != null) { elements = elements.Concat(panel.trasseL.Elemente); }
                    if (panel.trasseS != null) { elements = elements.Concat(panel.trasseS.Elemente); }
                    if (panel.trasseR != null) { elements = elements.Concat(panel.trasseR.Elemente); }

                    Task[] tasks = new Task[elements.Count()];
                    int n = 0;
                    foreach (TrassenElementExt element in elements)
                    {
                        int localN = n;
                        tasks[localN] = Task.Run(() =>
                        {
                            element.ClearProjections();
                            Interpolation interp = element.InterpolationResult;
                            float deviation = ((TRATrasse)element.owner).ProjectPoints(interp.X, interp.Y);
                            string ownerString = element.owner.Filename + "_" + element.ID;
                            TrassierungLog.Logger?.Log_Async(LogLevel.Information, ownerString + " " + "Deviation to geometry after transform: " + deviation, element);
                        });
                        n++;
                    }
                    Task.WaitAll(tasks);
                    if (panel.trasseL != null)
                    {
                        panel.trasseL.CalcGradientCoordinates();
                        panel.trasseL.Plot();
                    }
                    if (panel.trasseS != null)
                    {
                        panel.trasseS.Plot();
                    }
                    if (panel.trasseR != null)
                    {
                        panel.trasseR.CalcGradientCoordinates();
                        panel.trasseR.Plot();
                    }
                }
                idx--;
            }
        }
    }
}
