﻿

using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;

#if USE_SCOTTPLOT
using ScottPlot.Plottables;
using OpenTK.Platform.Windows;
using ScottPlot;
using SkiaSharp;
#endif


// AssemblyInfo.cs
[assembly: InternalsVisibleTo("TRA.Lib_TEST")]

namespace TRA_Lib
{
    public class Trasse
    {
        public static List<Trasse> LoadedTrassen = new List<Trasse> { };
        public string Filename = "";
        public Trasse()
        {
            LoadedTrassen.Add(this);
        }
    }
    public class GRATrasse : Trasse
    {
        public GradientElementExt[] GradientenElemente;
        /// <summary>
        /// Find nearest GradientElement(NW) to milage s
        /// </summary>
        /// <param name="s"></param>
        /// <returns>GradientElement(NW)</returns>
        internal GradientElementExt GetGradientElementFromS(double s)
        {
            if (GradientenElemente == null)
            {
                TrassierungLog.Logger?.LogError("Can not get GradientElement from s, as there are no Gradient Elements loaded for this Trasse. Please add Gradients by calling AssignGRA()", nameof(GetGradientElementFromS)); return null;
            }
            foreach (GradientElementExt element in GradientenElemente)
            {
                if (element.S > s)
                {
                    if (element.Predecessor == null || ((element.S - s) < (s - element.Predecessor.S))) { return element; } //if no successor exists or s is nearer on element than on successor
                    else { return element.Predecessor; }
                }
            }
            return null;
        }

    }
    public class TRATrasse : GRATrasse
    {
        public TrassenElementExt[] Elemente;
        ///<value>Stationierungs/Kilometrierungs Trasse. Used to project coordinates to TrasseS and get Station values S of the mileage(TrasseS).</value>
        TRATrasse TrasseS;
        /// <summary>
        /// Assign a stationing/mileage Trasse. Used to project coordinates to TrasseS and get Station values S of the mileage(TrasseS).
        /// </summary>
        public void AssignTrasseS(TRATrasse TrasseS) 
        { 
            this.TrasseS = TrasseS;
            CalcGradientCoordinates();  
        }

        /// <summary>
        /// Set an array of GradientElements to this Trasse, exisiting GradientElements are overwritten!
        /// </summary>
        /// <param name="gradientElements"></param>
        public void AssignGRA(ref GradientElementExt[] gradientElements)
        {
            GradientenElemente = gradientElements;
            CalcGradientCoordinates();
        }

        /// <summary>
        /// Combine a TRA-Trasse and a GRA-Trasse, by copying GradientenElemente to the TRA-Trasse, exisiting GradientElements are overwritten!
        /// </summary>
        /// <param name="GRATrasse"></param>
        public void AssignGRA(GRATrasse GRATrasse)
        {
            if (GRATrasse != null)
            {
                AssignGRA(ref GRATrasse.GradientenElemente);
            }
        }

        /// <summary>
        /// Estimate coordinates of slopechanges(NW) of the GRA_Data. If a TrasseS is set, a projection to TrasseS is applied.
        /// </summary>
        public void CalcGradientCoordinates()
        {
            if (GradientenElemente == null) return;
            foreach (GradientElementExt gradientElement in GradientenElemente)
            {
                double X, Y, s;
                TrassenElementExt trassenElement;
                if (TrasseS != null)
                {
                    TrassenElementExt trassenElementS = TrasseS.GetTrassenElementFromS(gradientElement.S);
                    if (trassenElementS == null) continue;
                    (X, Y, _) = trassenElementS.GetPointAtS(gradientElement.S);
                    trassenElement = GetElementFromPoint(X, Y);
                    if (trassenElement == null) continue;
                    s = trassenElement.GetSAtPoint(X, Y);
                    if (Double.IsNaN(s)) continue;
                }
                else
                {
                    trassenElement = GetTrassenElementFromS(gradientElement.S);
                    s = gradientElement.S;
                }
                (X, Y, _) = trassenElement.GetPointAtS(s);
                gradientElement.X = X;
                gradientElement.Y = Y;
            }
        }

        /// <summary>
        /// search for the TrassenElement at the Stationvalue s. Length of elements is not taken into account, if s is out of covered range first/last element is returned.
        /// </summary>
        /// <param name="s">Stationvalue</param>
        /// <returns>the corresponding Element, returns null if no Elements are loaded</returns>
        internal TrassenElementExt GetTrassenElementFromS(double s)
        {
            if (Elemente == null)
            {
                TrassierungLog.Logger?.LogError("Can not get Element from s, as there are no Elements loaded for this Trasse.", nameof(GetTrassenElementFromS)); return null;
            }
            foreach (TrassenElementExt element in Elemente.Reverse())
            {
                if (s > element.S)
                {
                    return element;
                }
            }
            return Elemente[0];
        }

        /// <summary>
        /// Iterates backwards over Elements and returns first (and nearest) Element the point is included
        /// </summary>
        /// <param name="X">X coordinate of Point of Interest</param>
        /// <param name="Y">Y coordinate of Point of Interest</param>
        /// <returns>returns Element[0] if point is behind first element</returns>
        internal TrassenElementExt GetElementFromPoint(double X, double Y)
        {
            foreach (TrassenElementExt element in Elemente.Reverse())
            {
                Vector2 v1 = new Vector2((float)Math.Cos(element.T), (float)Math.Sin(element.T));
                Vector2 v2 = new Vector2((float)(X - element.Xstart), (float)(Y - element.Ystart));
                if (Vector2.Dot(v1, v2) > 0) { return element; }
            }
            return Elemente[0];
        }
        /// <summary>
        /// Interpolate 2D Points allog the Trasse
        /// </summary>
        /// <param name="delta">distance along geometry between interpolation points</param>
        /// <param name="allowedTolerance">maximal allowed distance between geometry and interpolated polyline, if set to zero this value is ignored</param>
        /// <returns>Interpolation: array of 2D coordinates, along with heading and curvature for each point</returns>
        public Interpolation Interpolate(double delta = 1.0, double allowedTolerance = 0.01)
        {
            Interpolation interpolation = new Interpolation(0);
            foreach (TrassenElementExt element in Elemente)
            {
                interpolation.Concat(element.Interpolate(delta, allowedTolerance));
            }
            return interpolation;
        }

        /// <summary>
        /// Interpolate 3D Points allog the Trasse using .GRA for elevation interpolation
        /// </summary>
        /// <param name="stationierungsTrasse">optinal TrasseS to get mileage s. If nothing is set either a prvipusly set is used or values s from this Trasse are directly used (depending on the GRA-files this does not match)</param>
        /// <param name="delta">distance along geometry between interpolation points</param>
        /// <param name="allowedTolerance">maximal allowed distance between geometry and interpolated polyline, if set to zero this value is ignored</param>
        /// <returns>Interpolation: array of 3D coordinates, along with heading and curvature for each point</returns>
        public Interpolation Interpolate3D(TRATrasse stationierungsTrasse = null, double delta = 1.0, double allowedTolerance = 0.01)
        {
            Interpolation interp = new Interpolation(0);
            TRATrasse trasseS = stationierungsTrasse != null ? stationierungsTrasse : TrasseS; //if a valid trasse is provided use that one, else try to use a previously assigned
            if (GradientenElemente == null)
            {
                TrassierungLog.Logger?.LogError("Can not calculate Heights for Interpolation as there are no Gradient Elements loaded for this Trasse. Please add Gradients by calling AssignGRA()", nameof(Interpolate3D));
            }
            foreach (TrassenElementExt element in Elemente)
            {
                ref Interpolation Interpolation = ref element.Interpolate(delta, allowedTolerance);
                if (GradientenElemente == null)
                {
                    interp.Concat(Interpolation);
                    continue;
                }
                int num = Interpolation.X.Length;
                Interpolation.H = new double[num];
                Interpolation.s = new double[num];
                for (int i = 0; i < num; i++)
                {
                    double s;
                    if (trasseS != null)  //If TrasseS is set, try projecting coordinate to trasseS, s = NaN if fails
                    {
                        (s,_,_,_) = trasseS.ProjectPoints(Interpolation.X[i], Interpolation.Y[i], element.projections);
                    }
                    else //if no trasseS provided use original value S
                    {
                        s = Interpolation.S[i];
                    }
                    GradientElementExt gradient = GetGradientElementFromS(s);
                    (Interpolation.H[i], Interpolation.s[i]) = (gradient != null ? gradient.GetHAtS(s) : (double.NaN, double.NaN));
                }
                interp.Concat(Interpolation);
            }
            return interp;
        }
        /// <summary>
        /// Project an Array of X,Y coordinates on elements geoemtry
        /// </summary>
        /// <param name="X">array of X-coordinates</param>
        /// <param name="Y">array of Y-coordinates</param>
        /// <returns>mean deviation(distance) from point to geometry</returns>
        public float ProjectPoints(double[] X, double[] Y)
        {
            int num = Math.Min(X.Length, Y.Length);
            float deviation = 0;
            double dist = 0;    
            for (int i =0;i<num; i++)
            {
               (_,dist,_,_) = ProjectPoints(X[i], Y[i]);
               deviation += (float)dist;
            }
            return deviation / num;
        }
        /// <summary>
        /// Projecting a Point on this trasse
        /// </summary>
        /// <param name="X">X-Coordinate</param>
        /// <param name="Y">Y-Coordinate</param>
        /// <returns>s(Milage),distance,X-Coordinate of the projection,Y-Coordinate of the projection</returns>
        internal (double,double,double,double) ProjectPoints(double X, double Y, List<ProjectionArrow> projections = null)
        {
            TrassenElementExt element = GetElementFromPoint(X, Y);
            double s = (element != null ? element.GetSAtPoint(X, Y) : double.NaN);
            double X_, Y_;
            if (element != null && !Double.IsNaN(s))
            {
                (X_, Y_, _) = element.GetPointAtS(s);
#if USE_SCOTTPLOT    
                if (projections == null)
                {
                    projections = element.projections;
                }
                projections.Add(new ProjectionArrow(new Coordinates(Y, X), new Coordinates(Y_, X_)));
#endif
                return (s,Math.Sqrt(Math.Pow(X - X_, 2) + Math.Pow(Y - Y_, 2)), X_, Y_);
            }
            return (double.NaN,double.NaN, double.NaN, double.NaN);

        }

        public void SaveCSV(StreamWriter outputFile)
        {
            CultureInfo info = CultureInfo.CurrentCulture;
            using (outputFile)
            {
                string[] titles = { "R1", "R2", "Y", "X", "T", "S", "Kz", "L", "U1", "U2", "C", "H", "s" };
                outputFile.WriteLine(string.Join(info.TextInfo.ListSeparator, titles));
                foreach (TrassenElementExt ele in Elemente)
                {
                    outputFile.WriteLine(ele.ToString());
                    Interpolation interp = ele.InterpolationResult;
                    if (interp.Y != null)
                    {
                        for (int i = 0; i < ele.InterpolationResult.Y.Length; i++)
                        {
                            string[] values = { "", "", interp.Y[i].ToString(info), interp.X[i].ToString(info), interp.T[i].ToString(info), interp.S[i].ToString(info), interp.K[i].ToString(info) };
                            if (interp.H != null)
                            {
                                values = values.Concat(new string[] { "", "", "", "", interp.H[i].ToString(info), interp.s[i].ToString(info) }).ToArray();
                            }
                            else
                            {
                                values = values.Concat(new string[] { "", "", "", "", "", "" }).ToArray();
                            }
                            outputFile.WriteLine(string.Join(info.TextInfo.ListSeparator, values));
                        }
                    }
                }
            }
        }

#if USE_SCOTTPLOT
        static Form Form;
        /// <value>Plot for a 2D overview of all plotted trassen</value>
        static ScottPlot.WinForms.FormsPlot Plot2D;
        /// <value>List of all Plottables of this element</value>
        List<IPlottable> Plottables = new();
        /// <value>Callout for right-click selected Coordinate and s-projection</value>
        static ProjectionArrow selectedS;
        /// <value>Plot for TRA-Details heading and hurvature </value>
        ScottPlot.WinForms.FormsPlot PlotT;
        /// <value>Plot for GRA-Details elevation and slope </value>
        ScottPlot.WinForms.FormsPlot PlotG;
        /// <value>Table for all loaded Attributes of the TRA-File</value>
        DataGridView gridView;

        public void Plot()
        {
            if (Form == null)
            {
                Form = new() { Width = 800, Height = 500 };
                SplitContainer splitContainer = new SplitContainer
                {
                    Dock = DockStyle.Fill,
                    Orientation = System.Windows.Forms.Orientation.Horizontal,
                    SplitterDistance = (int)(Form.ClientSize.Height * 0.7)
                };
                FlowLayoutPanel BtnPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Bottom,
                    Height = 50,
                    FlowDirection = FlowDirection.LeftToRight,
                    Padding = new(80, 5, 80, 5)
                };
                CheckBox CheckShowWarnings = new CheckBox
                {
                    Text = "Show Warnings",
                    AutoSize = true,
                    Checked = true,
                };
                CheckShowWarnings.CheckedChanged += CheckShowWarnings_CheckedChanged;
                CheckBox CheckElementLabels = new CheckBox
                {
                    Text = "Show Element Labels",
                    AutoSize = true,
                    Checked = true,
                };
                CheckElementLabels.CheckedChanged += CheckElementLabels_CheckedChanged;
                CheckBox CheckProjections = new CheckBox
                {
                    Text = "Show Projections",
                    AutoSize = true,
                    Checked = false,
                };
                CheckProjections.CheckedChanged += CheckProjections_CheckedChanged;
                TabControl tabControl = new TabControl
                {
                    Dock = DockStyle.Fill
                };
                tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
                Form.Controls.Add(splitContainer);
                Form.Controls.Add(BtnPanel);
                BtnPanel.Controls.Add(CheckElementLabels);
                BtnPanel.Controls.Add(CheckShowWarnings);
                BtnPanel.Controls.Add(CheckProjections);
                splitContainer.Panel2.Controls.Add(tabControl);

            }
            //Add Plot for 2D overview
            PixelPadding padding = new(80, 80, 20, 5);
            if (Plot2D == null)
            {
                Plot2D = new ScottPlot.WinForms.FormsPlot { Dock = DockStyle.Fill };
                Form.Controls.OfType<SplitContainer>().First<SplitContainer>().Panel1.Controls.Add(Plot2D);
                Plot2D.Plot.Layout.Fixed(padding);
                //Plot2D.Plot.XLabel("Rechtswert Y[m]");
                Plot2D.Plot.YLabel("Hochwert X[m]");
                Plot2D.MouseDown += Plot2D_MouseClick;
            }
            //Add Plot for heading and curvature
            if (PlotT == null)
            {
                //New Tab for this trasse
                TabPage tabPage = new TabPage { Text = Filename };
                SplitContainer splitContainer = new SplitContainer
                {
                    Dock = DockStyle.Fill,
                    Orientation = System.Windows.Forms.Orientation.Horizontal,
                    SplitterDistance = (int)(tabPage.Height * 0.9),
                };
                //New Table for the raw-data
                gridView = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
                };
                gridView.Columns.AddRange(new DataGridViewColumn[]
                {
                    new DataGridViewTextBoxColumn
                    {
                        HeaderText = "ID",
                        Name = "ID",
                        ValueType = typeof(int),
                    },
                    new DataGridViewTextBoxColumn
                    {
                        HeaderText = "R1",
                        Name = "R1",
                        ToolTipText = "Radius am Elementanfang",
                        ValueType = typeof(double),
                    },
                    new DataGridViewTextBoxColumn
                    {
                        HeaderText = "R2",
                        Name = "R2",
                        ToolTipText = "Radius am Elementende",
                        ValueType = typeof(double),
                    },
                    new DataGridViewTextBoxColumn
                    {
                        HeaderText = "Rechtswert Y",
                        Name = "Y",
                        ToolTipText = "Rechtswert am Elementanfang",
                        ValueType = typeof(double),
                    },
                    new DataGridViewTextBoxColumn
                    {
                        HeaderText = "Hochwert X",
                        Name = "X",
                        ToolTipText = "Hochwert am Elementanfang",
                        ValueType = typeof(double),
                    },
                    new DataGridViewTextBoxColumn
                    {
                        HeaderText = "T",
                        Name = "T",
                        ToolTipText = "Richtung am Elementanfang",
                        ValueType = typeof(double),
                    },
                    new DataGridViewTextBoxColumn
                    {
                        HeaderText = "S",
                        Name = "S",
                        ToolTipText = "Stationswert am Elementanfang",
                        ValueType = typeof(double),
                    },
                    new DataGridViewTextBoxColumn
                    {
                        HeaderText = "Kz",
                        Name = "Kz",
                        ToolTipText = "Kennzeichen des Elements",
                        ValueType = typeof(Trassenkennzeichen),
                    },
                    new DataGridViewTextBoxColumn
                    {
                        HeaderText = "L",
                        Name = "L",
                        ToolTipText = "Länge des Elements",
                        ValueType = typeof(double),
                    },
                    new DataGridViewTextBoxColumn
                    {
                        HeaderText = "U1",
                        Name = "U1",
                        ToolTipText = "Überhöhung am Elementanfang",
                        ValueType = typeof(double),
                    },
                    new DataGridViewTextBoxColumn
                    {
                        HeaderText = "U2",
                        Name = "U2",
                        ToolTipText = "Überhöhung am Elementende",
                        ValueType = typeof(double),
                    },
                    new DataGridViewTextBoxColumn
                    {
                        HeaderText = "C",
                        Name = "C",
                        ToolTipText = "Abstand zur Trasse",
                        ValueType = typeof(double),
                    }
                });
                padding = new(80, 80, 50, 5);
                //Set properties for new Details-Plot (TRA)
                PlotT = new ScottPlot.WinForms.FormsPlot { Dock = DockStyle.Fill };
                PlotT.Plot.Layout.Fixed(padding);
                PlotT.Plot.XLabel("Rechtswert Y[m]");
                PlotT.Plot.Axes.Left.Label.Text = "Heading [rad]";
                PlotT.Plot.Axes.Right.Label.Text = "Curvature [1/m]";

                //Set properties for new Details-Plot (GRA)
                PlotG = new ScottPlot.WinForms.FormsPlot { Dock = DockStyle.Fill };
                PlotG.Plot.Layout.Fixed(padding);
                PlotG.Plot.XLabel("Rechtswert Y[m]");
                PlotG.Plot.Axes.Left.Label.Text = "Elevation [m]";
                PlotG.Plot.Axes.Right.Label.Text = "Slope [‰]";

                TabControl tabControl = new TabControl
                {
                    Dock = DockStyle.Fill,
                    Alignment = TabAlignment.Left
                };
                TabPage tabPageT = new TabPage { Text = "TRA" };
                tabPageT.Controls.Add(PlotT);
                tabControl.Controls.Add(tabPageT);
                TabPage tabPageG = new TabPage { Text = "GRA" };
                tabPageG.Controls.Add(PlotG);
                tabControl.Controls.Add(tabPageG);
                splitContainer.Panel1.Controls.Add(tabControl);
                splitContainer.Panel2.Controls.Add(gridView);
                tabPage.Controls.Add(splitContainer);
                Form.Controls.OfType<SplitContainer>().First().Panel2.Controls.OfType<TabControl>().First().Controls.Add(tabPage);
            }
            Plot2D.Plot.Axes.Link(PlotT, true, false);
            PlotT.Plot.Axes.Link(Plot2D, true, false);
            Plot2D.Plot.Axes.Link(PlotG, true, false);
            PlotG.Plot.Axes.Link(Plot2D, true, false);

            gridView.Rows.Clear();           
            PlotT.Plot.Clear();
            PlotG.Plot.Clear();
            //Remove previous Plottables from Plots
            foreach (IPlottable plottable in Plottables)
            {
                Plot2D.Plot.Remove(plottable);
            }
           
            foreach (TrassenElementExt element in Elemente)
            {
                Interpolation interpolation = element.InterpolationResult;
                var scatter = Plot2D.Plot.Add.Scatter(interpolation.Y, interpolation.X);
                Plottables.Add(scatter);
                scatter.LegendText = Filename;
                ScottPlot.Color color = scatter.MarkerFillColor;
                ElementMarker marker = new(element, color);
                Plottables.Add(Plot2D.Plot.Add.Plottable(marker));
                var scatterT = PlotT.Plot.Add.Scatter(interpolation.Y, interpolation.T, color);
                //scatterT.LegendText = "Heading";
                Plottables.Add(PlotT.Plot.Add.VerticalLine(element.Ystart, 2, color));
                var scatterK = PlotT.Plot.Add.ScatterLine(interpolation.Y, interpolation.K, color);
                //scatterK.LegendText = "Curvature";
                // tell each T and K plot to use a different axis
                scatterT.Axes.YAxis = PlotT.Plot.Axes.Left;
                scatterK.Axes.YAxis = PlotT.Plot.Axes.Right;

                if (interpolation.H != null && interpolation.s != null)
                {
                    var scatterH = PlotG.Plot.Add.Scatter(interpolation.Y, interpolation.H, color);
                    //scatterH.LegendText = "Elevation";
                    scatterH.Axes.YAxis = PlotG.Plot.Axes.Left;
                    var scatterSlope = PlotG.Plot.Add.ScatterLine(interpolation.Y, interpolation.s, color);
                    //scatterSlope.LegendText = "Slope";
                    scatterSlope.Axes.YAxis = PlotG.Plot.Axes.Right;
                }

                //Raw Data to GridView
                gridView.Rows.Add(element.ID, element.R1, element.R2, element.Ystart, element.Xstart, element.T, element.S, element.KzString, element.L, element.U1, element.U2, element.C);
                //Warnings
                element.WarningCallouts.CollectionChanged += Warning_CollectionChanged;
                foreach (var warning in element.WarningCallouts)
                {
                    Plot2D.Plot.Add.Plottable(warning);
                }
                //Visualize Projections
                foreach (ProjectionArrow projection in element.projections)
                {
                    projection.IsVisible = false;
                    Plottables.Add(Plot2D.Plot.Add.Plottable(projection));
                }
            }
            if (GradientenElemente != null)
            {
                foreach (GradientElementExt element in GradientenElemente)
                {
                    if (element.Y == double.NaN) continue;
                    var vline = PlotG.Plot.Add.VerticalLine(element.Y);
                    vline.Text = " NW " + element.Pkt.ToString() + " " + element.H.ToString() + "m";
                    vline.LabelRotation = -90;
                    vline.ManualLabelAlignment = Alignment.UpperLeft;
                    vline.LabelFontColor = vline.LabelBackgroundColor;
                    vline.LabelBackgroundColor = ScottPlot.Color.FromHex("#FFFFFF00");
                }
            }
            else
            {
                Plottables.Add(PlotG.Plot.Add.Annotation("No Gradient Data available"));
            }

            // Set the axis scale to be equal
            Plot2D.Plot.Axes.SquareUnits();
            Plot2D.Plot.HideLegend();
            Plot2D.Refresh();
            PlotT.Refresh();
            PlotG.Refresh();
            if (!Form.Visible) Form.Show();// ShowDialog();
            Form.Update();
        }

        public void UpdatePlot()
        {
            if (gridView != null)
            {
                gridView.Rows.Clear();
                foreach (TrassenElementExt element in Elemente)
                {
                    gridView.Rows.Add(element.ID, element.R1, element.R2, element.Ystart, element.Xstart, element.T, element.S, element.KzString, element.L, element.U1, element.U2, element.C);
                }
            }

        }
        private void Warning_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) 
        { 
            switch (e.Action) 
            { 
                case NotifyCollectionChangedAction.Add: 
                    foreach(var item in e.NewItems)
                    {
                        Plot2D.Plot.Add.Plottable(item as GeometryWarning);
                    }
                    break; 
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        Plot2D.Plot.Remove(item as GeometryWarning);
                    }
                    break; 
                case NotifyCollectionChangedAction.Replace:
                    int i = 0;
                    foreach (var item in e.OldItems)
                    {
                        Plot2D.Plot.Remove(item as GeometryWarning);
                        Plot2D.Plot.Add.Plottable(e.NewItems[i] as GeometryWarning);
                        i++;
                    }
                    break; 
                case NotifyCollectionChangedAction.Move: 
                    break; 
                case NotifyCollectionChangedAction.Reset:       
                    break; 
            }
            Plot2D.Refresh();
        }
        private void CheckProjections_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            bool show = box.Checked;
            foreach (ProjectionArrow arrow in Plot2D.Plot.GetPlottables<ProjectionArrow>())
            {
                arrow.IsVisible = show;
            }
            Plot2D.Refresh();
        }

        private void CheckElementLabels_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            bool show = box.Checked;
            foreach (var callout in Plot2D.Plot.GetPlottables<ElementMarker>())
            {
                callout.IsVisible = show;
            }
            Plot2D.Refresh();
        }

        private void CheckShowWarnings_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            bool show = box.Checked;
            foreach (var warning in Plot2D.Plot.GetPlottables<GeometryWarning>())
            {
                warning.IsVisible = show;
            }
            Plot2D.Refresh();
        }
        
        private void Plot2D_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && Control.ModifierKeys.HasFlag(Keys.Control))
            {
                Coordinates coordinates = Plot2D.Plot.GetCoordinates(new Pixel(e.X, e.Y));
                TrassenElementExt element = GetElementFromPoint(coordinates.Y, coordinates.X);
                if (element == null) { return; }
                double s = element.GetSAtPoint(coordinates.Y, coordinates.X,0);
                if (Double.IsNaN(s)) return;
                (double X, double Y, double H) = element.GetPointAtS(s);
                coordinates = new Coordinates(Y + 10 * Math.Sin(H + 0.5 * Math.PI), X + 10 * Math.Cos(H + 0.5 * Math.PI));
                if (selectedS == null)
                {
                    //selectedS = Plot2D.Plot.Add.Callout(s.ToString(), Y + 10, X + 10, Y, X);
                    selectedS = new ProjectionArrow(coordinates, new Coordinates(Y, X));
                    Plot2D.Plot.Add.Plottable(selectedS);
                }
                else
                {
                    selectedS.Base = coordinates;
                    selectedS.Tip = new Coordinates(Y, X);
                    //selectedS.Text = s.ToString();
                }
                Plot2D.Refresh();
            }
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            String label = "";
            TabControl control = (TabControl)sender;
            if (control != null) { label = control.SelectedTab.Text; }
            foreach (var plot in Plot2D.Plot.GetPlottables<ScottPlot.Plottables.Scatter>())
            {
                plot.MarkerSize = plot.LegendText == label ? 5 : 0;
            }
            Plot2D.Refresh();
        }
#endif
    }
#if USE_SCOTTPLOT
    class ProjectionArrow : ScottPlot.Plottables.Arrow
    {
        public ProjectionArrow(Coordinates pos, Coordinates tip) : base()
        {
            Base = pos;
            Tip = tip;
            ArrowFillColor = Colors.LightGray;
            ArrowLineWidth = 0;
            ArrowWidth = 3;
            ArrowheadLength = 20;
            ArrowheadAxisLength = 20;
            ArrowheadWidth = 7;
        }
    }
    class ElementMarker : LabelStyleProperties, IPlottable, IHasLine, IHasMarker, IHasLegendText
    {
        public Coordinates Start { 
            get {
                return new Coordinates(element.Ystart - Math.Sin(element.T + 0.5 * Math.PI) * 10, element.Xstart - Math.Cos(element.T + 0.5 * Math.PI) * 10);
            }
        }
        public Coordinates End
        {
            get
            {
                return new Coordinates(element.Ystart + Math.Sin(element.T + 0.5 * Math.PI) * 10, element.Xstart + Math.Cos(element.T + 0.5 * Math.PI) * 10);
            }
        }
        TrassenElementExt element;
        public override LabelStyle LabelStyle { get; set; } = new() { FontSize = 14 };
        double t;
        public ElementMarker(TrassenElementExt element, ScottPlot.Color Color)
        {
            this.element = element;
            this.Color = Color;
            this.LabelText = element.ID.ToString() + "_" + element.KzString; ;
            this.LabelRotation = (float)(element.T * (180 / Math.PI));
            LabelAlignment = Alignment.LowerLeft;
        }

        public MarkerStyle MarkerStyle { get; set; } = new() { Size = 0 };
        public MarkerShape MarkerShape { get => MarkerStyle.Shape; set => MarkerStyle.Shape = value; }
        public float MarkerSize { get => MarkerStyle.Size; set => MarkerStyle.Size = value; }
        public ScottPlot.Color MarkerFillColor { get => MarkerStyle.FillColor; set => MarkerStyle.FillColor = value; }
        public ScottPlot.Color MarkerLineColor { get => MarkerStyle.LineColor; set => MarkerStyle.LineColor = value; }
        public ScottPlot.Color MarkerColor { get => MarkerStyle.MarkerColor; set => MarkerStyle.MarkerColor = value; }
        public float MarkerLineWidth { get => MarkerStyle.LineWidth; set => MarkerStyle.LineWidth = value; }

        public string LegendText { get; set; } = string.Empty;
        public bool IsVisible { get; set; } = true;
        public IAxes Axes { get; set; } = new Axes();
        public IEnumerable<LegendItem> LegendItems => LegendItem.Single(this, LegendText, LineStyle, MarkerStyle);

        public LineStyle LineStyle { get; set; } = new() { Width = 3 };
        public float LineWidth { get => LineStyle.Width; set => LineStyle.Width = value; }
        public LinePattern LinePattern { get => LineStyle.Pattern; set => LineStyle.Pattern = value; }
        public ScottPlot.Color LineColor { get => LineStyle.Color; set => LineStyle.Color = value; }

        public ScottPlot.Color Color
        {
            get => LineStyle.Color;
            set
            {
                LineStyle.Color = value;
                MarkerStyle.FillColor = value;
                LabelFontColor = value;
            }
        }

        public AxisLimits GetAxisLimits()
        {
            CoordinateRect boundingRect = new(Start, End);
            return new AxisLimits(boundingRect);
        }

        public virtual void Render(RenderPack rp)
        {
            CoordinateLine line = new(Start, End);
            PixelLine pxLine = Axes.GetPixelLine(line);

            using SKPaint paint = new();
            Drawing.DrawMarker(rp.Canvas, paint, Axes.GetPixel(Start), MarkerStyle);
            Drawing.DrawMarker(rp.Canvas, paint, Axes.GetPixel(End), MarkerStyle);
            Drawing.DrawLine(rp.Canvas, paint, pxLine, LineStyle);
            LabelStyle.Render(rp.Canvas, pxLine.Center, paint);
        }
    }
#endif
}
