using LowDistortionProjection;
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
        SpatialReference SRS_Source = Srs.Get(5685); //DB_Ref
        public TransformPanel()
        {
            InitializeComponent();
            
            //set default out SRS:
            tb_SRS.Text = @"PROJCS[""unnamed"",GEOGCS[""ETRS89"",DATUM[""European_Terrestrial_Reference_System_1989"",SPHEROID[""GRS 1980"",6378137,298.257222101,AUTHORITY[""EPSG"",""7019""]],AUTHORITY[""EPSG"",""6258""]],PRIMEM[""Greenwich"",0,AUTHORITY[""EPSG"",""8901""]],UNIT[""degree"",0.0174532925199433,AUTHORITY[""EPSG"",""9122""]],AUTHORITY[""EPSG"",""4258""]],PROJECTION[""Transverse_Mercator""],PARAMETER[""latitude_of_origin"",50.8247],PARAMETER[""central_meridian"",13.9027],PARAMETER[""scale_factor"",1.0000346],PARAMETER[""false_easting"",10000],PARAMETER[""false_northing"",50000],UNIT[""metre"",1,AUTHORITY[""EPSG"",""9001""]],AXIS[""Easting"",EAST],AXIS[""Northing"",NORTH]]";
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
            SpatialReference SRS_Target;
            //Get Target SRS
            int epsg;
            if (int.TryParse(tb_SRS.Text, out epsg))
            {
                SRS_Target = Srs.Get(epsg);
            }
            else
            {
                SRS_Target = Srs.Get(tb_SRS.Text);
            }
            //For testing:
            //int baseSrsEPSG = 4258;
            //double lat = 50.8247;
            //double lon = 13.9027;
            //double k = 1.0000346;
            //double falseEasting = 10000.0;
            //double falseNorthing = 50000.0;
            //SRS_Target = Srs.GetLocalTMSystemWithScale(in baseSrsEPSG, in lat, in lon, in k, in falseEasting, in falseNorthing);
            //string argout = """";
            //SRS_Target.ExportToWkt(out argout,new string[] { });
            //Get TRA-Files to transform
            FlowLayoutPanel owner = Parent as FlowLayoutPanel;
            if (owner == null) { return; }
            int idx = owner.Controls.GetChildIndex(this) - 1;
            
            while (idx >= 0 && owner.Controls[idx].GetType() != typeof(TransformPanel))
            {
                if (owner.Controls[idx].GetType() == typeof(TrassenPanel))
                {
                    TrassenPanel panel = (TrassenPanel)owner.Controls[idx];
                    IEnumerable<TrassenElementExt> elements = Enumerable.Empty<TrassenElementExt>();
                    if(panel.trasseL != null ) { elements = elements.Concat(panel.trasseL.Elemente); }
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
                                egbt22lib.Convert.DBRef_GK5_Gamma_k(points[0], points[1],out gamma_i,out k_i);
                                egbt22lib.Convert.DBRef_GK5_to_EGBT22_Local(points[0], points[1], points[2],out points[0], out points[1]);
                                egbt22lib.Convert.EGBT22_Local_Gamma_k(points[0], points[1], out gamma_o, out k_o);
                                //Workaround to set values in place
                                for (int i = 0; i < interp.X.Length; i++)
                                {
                                    interp.X[i] = points[1][i];
                                    interp.Y[i] = points[0][i];
                                    interp.H[i] = points[2][i];
                                    interp.T[i] = interp.T[i]-DegreesToRadians(gamma_o[i] - gamma_i[i]);
                                }
                            }
                            catch { 
                            }
                        }
                        //Transform Element
                        try
                        {
                            //Srs.ConvertInPlace(ref coordinate, SRS_Source, SRS_Target);
                            double gamma_i, k_i,gamma_o,k_o;
                            double rechts, hoch;
                            (gamma_i,k_i) = egbt22lib.Convert.DBRef_GK5_Gamma_k(element.Ystart, element.Xstart);
                            (rechts, hoch) = egbt22lib.Convert.DBRef_GK5_to_EGBT22_Local(element.Ystart, element.Xstart, Double.IsNaN(elementHeight) ? 0.0 : elementHeight);
                            (gamma_o,k_o) = egbt22lib.Convert.EGBT22_Local_Gamma_k(rechts, hoch);
                            double dK = (k_i / k_o);

                            element.Relocate(hoch, rechts, DegreesToRadians(gamma_o - gamma_i), dK,previousdK);
                            previousdK = element.ID == 0 ? double.NaN : dK; //reset prviousScale for next Element
                        }
                        catch {
                        }
                    }

                    if (panel.trasseL != null) { 
                        foreach (TrassenElementExt element in panel.trasseL.Elemente)
                        {
                            Interpolation interp = element.InterpolationResult;
                            float deviation = panel.trasseL.ProjectPoints(interp.X, interp.Y);
                            string ownerString = panel.trasseL.Filename + "_" + element.ID;
                            TrassierungLog.Logger?.LogInformation(ownerString + " " + "Deviation to geometry after transform: " + deviation);
                        }
                        panel.trasseL.CalcGradientCoordinates();
                        panel.trasseL.Plot();
                    }
                    if (panel.trasseS != null) {
                        foreach (TrassenElementExt element in panel.trasseS.Elemente)
                        {
                            Interpolation interp = element.InterpolationResult;
                            float deviation = panel.trasseS.ProjectPoints(interp.X, interp.Y);
                            string ownerString = panel.trasseS.Filename + "_" + element.ID;
                            TrassierungLog.Logger?.LogInformation(ownerString + " " + "Deviation to geometry after transform: " + deviation);
                        }
                        panel.trasseS.Plot();
                    }
                    if (panel.trasseR != null) { 
                        foreach (TrassenElementExt element in panel.trasseR.Elemente)
                        {
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
