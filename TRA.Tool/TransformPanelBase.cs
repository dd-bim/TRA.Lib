﻿using System;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TRA.Tool
{
    public partial class TransformPanelBase : BasePanel
    {
        public TransformPanelBase()
        {
            InitializeComponent();
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180.0);
        }

        internal struct TransformSetup
        {
            public Func<double, double, (double x, double y)> ConvertFunc;
            public Func<double, double, (double gamma, double k, bool IsInside)> GammaK_From;
            public Func<double, double, (double gamma, double k, bool IsInside)> GammaK_To;
            public string Target_CRS;
            public TransformSetup()
            {
            }
        }

        public static double[][] CalcArray2(double[][] points, Func<double, double, (double x, double y)> calc)
        {
            int n = points[0].Length;
            double[][] xy = new double[2][];
            xy[0] = new double[n];
            xy[1] = new double[n];
            for (int i = 0; i < n; i++)
            {
                var (x, y) = calc(points[0][i], points[1][i]);
                xy[0][i] = x;
                xy[1][i] = y;
            }
            return xy;
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
            trasse.CRS_Name = transformSetup.Target_CRS; //Store new CRS of this trasse
            HashSet<TrassenElementExt> elementsOutsideSource = new HashSet<TrassenElementExt>();
            HashSet<TrassenElementExt> elementsOutsideTarget = new HashSet<TrassenElementExt>();
            HashSet<TrassenElementExt> elementsExeedingPPM = new HashSet<TrassenElementExt>();
            foreach (TrassenElementExt element in trasse.Elemente.Reverse()) //run reverse for having X/Yend from the successor is already transformed for plausability checks 
            {
                //Transform Interpolation Points
                Interpolation interp = element.InterpolationResult;
                if (!interp.IsEmpty())
                {
                    if (interp.H == null) { interp.H = new double[interp.X.Length]; }
                    try
                    {
                        //Transform is done in 2D. Normal Heights are not transformed, but the X/Y coordinates are transformed.
                        double[][] points = { interp.Y, interp.X};
                        double[] gamma_from, k_from, gamma_to, k_to = new double[interp.X.Length];
                        bool[] inside_from, inside_to = new bool[interp.X.Length];
                    
                        (gamma_from, k_from, inside_from) = egbt22lib.Convert.CalcArrays2(points[0], points[1],transformSetup.GammaK_From);
                        points = CalcArray2(points, transformSetup.ConvertFunc);
                        (gamma_to, k_to, inside_to) = egbt22lib.Convert.CalcArrays2(points[0], points[1], transformSetup.GammaK_To);
                        //Workaround to set values in place
                        for (int i = 0; i < interp.X.Length; i++)
                        {
                            interp.Y[i] = points[0][i];
                            interp.X[i] = points[1][i];
                            interp.H[i] = interp.H[i];
                            interp.T[i] = interp.T[i] - DegreesToRadians(gamma_to[i] - gamma_from[i]);
                        }
                        if (inside_from.Contains(false))
                        {
                            elementsOutsideSource.Add(element);
                            TrassierungLog.Logger?.Log_Async(LogLevel.Warning, trasse.Filename + ":Element#" + element.ID + " one or more InterpolationPoint coordinate is outside valid range of SourceCRS");

                        }
                        if (inside_to.Contains(false))
                        {
                            elementsOutsideTarget.Add(element);
                            TrassierungLog.Logger?.Log_Async(LogLevel.Warning, trasse.Filename + ":Element#" + element.ID + " one or more InterpolationPoint coordinate is outside valid range of SourceCRS");
                        }
                        //Check for PPM greather 10 
                        //if(Math.Abs(k_to.First()-1)*1000000 > 10 || Math.Abs(k_to.Last() - 1) * 1000000 > 10) //we only check first and last point for simplicity
                        //{
                        //    elementsExeedingPPM.Add(element);
                        //    TrassierungLog.Logger?.Log_Async(LogLevel.Warning, trasse.Filename + ":Element#" + element.ID + " scale exceeds 10ppm (" + Math.Max(Math.Abs(k_to.First() - 1) * 1000000, Math.Abs(k_to.Last() - 1) * 1000000) + ")");
                        //}
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
                    bool inside_from, inside_to;
                    (gamma_i, k_i, inside_from) = transformSetup.GammaK_From(element.Ystart, element.Xstart);
                    (rechts, hoch) = transformSetup.ConvertFunc(element.Ystart, element.Xstart);
                    (gamma_o, k_o, inside_to) = transformSetup.GammaK_To(rechts, hoch);
                    double dK = (k_o / k_i);
                    double dT = DegreesToRadians(gamma_o - gamma_i);
                    element.Relocate(hoch, rechts, dT, dK, previousdK, checkBox_RecalcHeading.Checked, checkBox_RecalcLength.Checked);
                    previousdK = dK;
                    if (!inside_from)
                    {
                        elementsOutsideSource.Add(element);
                    }      
                    if (!inside_to)
                    {
                        elementsOutsideTarget.Add(element);
                    }
                    //Check for PPM greather 10 
                    //if (Math.Abs(k_o - 1) * 1000000 > 10)
                    //{
                    //    elementsExeedingPPM.Add(element);
                    //}
                }
                catch
                {
                }
            }
            //Message if elements are outside CRS-BBox;
            if (elementsOutsideSource.Count > 0)
            {
                MessageBox.Show(trasse.Filename + ": has Elements outside SourceCRS BoundingBox:\n" + string.Join(", ", elementsOutsideSource.Select(p => p.ID)) + "\n (see log for more information)", "Transform: " + trasse.Filename);
            }
            if (elementsOutsideTarget.Count > 0 || elementsExeedingPPM.Count > 0)
            {
                MessageBox.Show(trasse.Filename + ": has Elements outside TargetCRS BoundingBox:\n" + string.Join(", ", elementsOutsideTarget.Select(p => p.ID)) + 
                    //"\n Element-Scale exceeding 10ppm:\n" + string.Join(", ", elementsExeedingPPM.Select(p => p.ID)) +
                    "\n(see log for more information)", "Transform: " + trasse.Filename);
            }
            //Set Heading to End element as this is only an empty Geometry and we started to iterate reverse heading could not be calculated. Set heading from the second last element
            double heading = 0;
            (_,_,heading) = trasse.Elemente[^2].GetPointAtS(trasse.Elemente[^2].L, true);
            trasse.Elemente.Last().T = heading;
            //Try Removing unnecessary KSprung-Elements.This can happen if previous scale was saved to TRA using KSprung, and inverted Transform was applied. Else Update
            foreach (TrassenElementExt element in trasse.Elemente)
            {
                if (element.Successor != null && element.Successor.GetGeometryType() == typeof(KSprung) && element.Successor.Successor != null)
                {
                    if (Math.Abs(element.S + element.L * element.Scale - element.Successor.Successor.S) < Trassierung.StationMismatchTolerance)
                    {
                        //Replace KSprung
                        element.ApplyScale(); //Apply Scale to pre-KSprung element
                        element.Successor.L = 0; //Set Length of KSprung to 0 (we delete it later by filtering on zero)
                        element.Successor = element.Successor.Successor; //move successor to KSprung-successor
                        if (element.Successor.Successor != null) element.Successor.Successor.Predecessor = element; //if there is a successor update also its predecessor 
                        element.Relocate(bFitHeading: true); //we changed Length so it makes sense to update Heading.
                        element.PlausibilityCheck(); // Rerun Check as we changed Length and Heading Parameters
                    }
                    else
                    {
                        element.Successor.L = element.Successor.Successor.S - element.S - element.L * element.Scale; //if there is still a deviation in Sation-values update this in KSprung-Length
                        element.Successor.T = element.Successor.Successor.T;
                    }
                }
            }
            trasse.Elemente = trasse.Elemente.Where(x => !(x.GetGeometryType() == typeof(KSprung) && x.L == 0)).ToArray(); //Remove KSprung elements of length 0
            //Try Removing unnecessary Scale.This can happen if previous scale was saved applied for saving, and inverted Transform was applied.
            foreach (TrassenElementExt element in trasse.Elemente)
            {
                if (element.Successor != null)
                {
                    if (Math.Abs(element.S + element.L * element.Scale - element.Successor.S) < Trassierung.StationMismatchTolerance)
                    {
                        element.ApplyScale(); //Apply Scale
                    }
                }
            }     
        }
        internal virtual TransformSetup GetTransformSetup() { return new TransformSetup(); }

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

            FlowLayoutPanel owner = Parent as FlowLayoutPanel;
            if (owner == null) { return; }
            int idx = owner.Controls.GetChildIndex(this) - 1;

            List<TRATrasse> transform_Trasse = new List<TRATrasse>();
            //Get all TRAs to transform
            while (idx >= 0 && owner.Controls[idx].GetType() != typeof(TransformPanelBase))
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
                    TrassenTransform(trasse, GetTransformSetup());
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
    }
}
