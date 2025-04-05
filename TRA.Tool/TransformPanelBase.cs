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
using ScottPlot;
using HarfBuzzSharp;
using System.Collections;

namespace TRA.Tool
{
    public partial class TransformPanelBase : UserControl
    {
        public TransformPanelBase()
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

        internal delegate (double Rechts, double Hoch, double H) SingleCoordinateTransform(double InRechts, double InHoch, double InH);
        internal delegate bool ArrayCoordinateTransform(double[] InRechts, double[] InHoch, double[] InH, out double[] OutRechts, out double[] OutHoch, out double[] OutH);
        internal delegate (double gamma, double k) Single_Gamma_k(double Rechts, double Hoch);
        internal delegate bool Array_Gamma_k(double[] Rechts, double[] Hoch, out double[] gamma, out double[] k);

        internal struct TransformSetup
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
        /// <summary>
        /// Transforms all Elements of a TRA by the given transformSetup
        /// </summary>
        /// <param name="trasse"></param>
        /// <param name="transformSetup"></param>
        private void TrassenTransform(TRATrasse trasse, TransformSetup transformSetup)
        {
            double previousdK = double.NaN; //Scale from previous element
            if (trasse == null) return;
            foreach (TrassenElementExt element in trasse.Elemente.Reverse()) //run reverse for having X/Yend from the successor is already transformed for plausability checks 
            {
                //Transform Interpolation Points
                Interpolation interp = element.InterpolationResult;
                if (!interp.IsEmpty())
                {
                double elementHeight = interp.H != null ? interp.H[0] : double.NaN; //save original height befor transforming
                                                                                    // TODO how to handle trasse without heights (like S) in transformations
                if (interp.H == null) { interp.H = new double[interp.X.Length]; }
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
                    double dT = DegreesToRadians(gamma_o - gamma_i);
                    element.Relocate(hoch, rechts, dT, dK, previousdK, checkBox_RecalcHeading.Checked, checkBox_RecalcLength.Checked);
                    previousdK = dK;
                }
                catch
                {
                }
            }
        }
        internal virtual TransformSetup SetupTransform() { return new TransformSetup(); }
        private void btn_Transform_Click(object sender, EventArgs e)
        {
            LoadingForm loadingForm = null;
            Thread _backgroundThread = new Thread(() =>
            {
                loadingForm = new LoadingForm();
                loadingForm.Show();
                Application.Run(loadingForm);
            });
            _backgroundThread.Start();

            TransformSetup transformSetup = SetupTransform();

            FlowLayoutPanel owner = Parent as FlowLayoutPanel;
            if (owner == null) { return; }
            int idx = owner.Controls.GetChildIndex(this) - 1;

            List<TRATrasse> transform_Trasse = new List<TRATrasse>();
            //Get all TRAs to transform
            while (idx >= 0 && owner.Controls[idx].GetType() != typeof(TransformPanel))
            {
                if (owner.Controls[idx].GetType() == typeof(TrassenPanel))
                {
                    TrassenPanel panel = (TrassenPanel)owner.Controls[idx];
                    if (panel.trasseL != null && !transform_Trasse.Contains(panel.trasseL)) transform_Trasse.Add(panel.trasseL);
                    if (panel.trasseS != null && !transform_Trasse.Contains(panel.trasseS)) transform_Trasse.Add(panel.trasseS);
                    if (panel.trasseR != null && !transform_Trasse.Contains(panel.trasseR)) transform_Trasse.Add(panel.trasseR);
                }
                idx--;
            }
            Task[] tasks = new Task[transform_Trasse.Count()];
            int n = 0;
            foreach (TRATrasse trasse in transform_Trasse)
            {
                int localN = n;
                tasks[localN] = Task.Run(() =>
                {
                    //Transform 
                    TrassenTransform(trasse, transformSetup);
                    //Calc Deviations
                    foreach (TrassenElementExt element in trasse.Elemente)
                    {
                        element.ClearProjections();
                        Interpolation interp = element.InterpolationResult;
                        if (interp.IsEmpty()) continue;
                        float deviation = ((TRATrasse)element.owner).ProjectPoints(interp.X, interp.Y, true);
                        string ownerString = element.owner.Filename + "_" + element.ID;
                        TrassierungLog.Logger?.Log_Async(LogLevel.Information, ownerString + " " + "Deviation to geometry after transform: " + deviation, element);
                    }
                });
                n++;
            }
            Task.WaitAll(tasks);
            foreach (TRATrasse trasse in transform_Trasse)
            {
                //int localN = n;
                //tasks[localN] = Task.Run(() =>
                //{
                //Re-Interpolate and do PlausabilityCheck
                trasse.Interpolate3D();
                //});
                //n++;
            }
            foreach (TRATrasse trasse in transform_Trasse)
            {
                trasse.Plot();
            }

            if (loadingForm != null)
            {
                // Use Invoke to close the form from the background thread
                loadingForm.Invoke(new Action(() =>
                {
                    loadingForm.Close(); // Close the form
                }));
            }
            // Wait for the thread to terminate
            _backgroundThread.Join();
        }

        private void btn_SaveAll_Click(object sender, EventArgs e)
        {
            
        }
    }
}
