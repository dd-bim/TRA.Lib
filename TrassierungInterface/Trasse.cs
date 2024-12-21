

using Microsoft.Extensions.Logging;
using System.Numerics;
using System.Runtime.CompilerServices;
using ScottPlot;
using System.Drawing;
using System.Linq;
using SkiaSharp;
using System.Xml.Linq;
//using OpenTK;

// AssemblyInfo.cs
[assembly: InternalsVisibleTo("KomponentenTest")]

namespace TrassierungInterface
{
    public class Trasse
    {
        public static List<Trasse> LoadedTrassen = new List<Trasse> { };
        public string Filename = "2";
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
                    if (element.Predecessor == null || ((element.S - s) < (s- element.Predecessor.S))) { return element; } //if no successor exists or s is nearer on element than on successor
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
        public TRATrasse SetTrasseS { set { TrasseS = value; CalcGradientCoordinates(); } }

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
            AssignGRA(ref GRATrasse.GradientenElemente);
        }

        /// <summary>
        /// Estimate coordinates of slopechanges(NW) of the GRA_Data. If a TrasseS is set, a projection to TrasseS is applied.
        /// </summary>
        void CalcGradientCoordinates()
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
        /// <returns></returns>
        internal TrassenElementExt GetElementFromPoint(double X, double Y)
        {
            foreach (TrassenElementExt element in Elemente.Reverse())
            {
                Vector2 v1 = new Vector2((float)Math.Cos(element.T), (float)Math.Sin(element.T));
                Vector2 v2 = new Vector2((float)(X - element.Xstart), (float)(Y - element.Ystart));
                if (Vector2.Dot(v1, v2) > 0) { return element; }
            }
            return null;
        }

        public void Interpolate(double delta = 1.0)
        {
            foreach (TrassenElementExt element in Elemente)
            {
                element.Interpolate(delta);
            }
        }
        public void Interpolate3D(TRATrasse stationierungsTrasse = null, double delta = 1.0)
        {
            TRATrasse trasseS = stationierungsTrasse != null ? stationierungsTrasse : TrasseS; //if a valid trasse is provided use that one, else try to use a previously assigned
            foreach (TrassenElementExt element in Elemente)
            {
                ref Interpolation Interpolation = ref element.Interpolate(delta);
                if (GradientenElemente == null)
                {
                    TrassierungLog.Logger?.LogError("Can not calculate Heights for Interpolation as there are no Gradient Elements loaded for this Trasse. Please add Gradients by calling AssignGRA()", nameof(Interpolate3D));
                    return;
                }
                int num = Interpolation.X.Length;
                Interpolation.H = new double[num];
                Interpolation.s = new double[num];
                for (int i = 0; i < num; i++)
                {
                    double s;
                    if (trasseS != null)  //If TrasseS is set, try projecting coordinate to trasseS, s = NaN if fails
                    {
                        TrassenElementExt stationierungsElement = trasseS.GetElementFromPoint(Interpolation.X[i], Interpolation.Y[i]);
                        s = (stationierungsElement != null ? stationierungsElement.GetSAtPoint(Interpolation.X[i], Interpolation.Y[i]) : double.NaN);
                        if (Double.IsNaN(s)) { Interpolation.H[i] = double.NaN; continue; }
#if USE_SCOTTPLOT
                        //Visualisation
                        if (stationierungsElement != null)
                        {
                            double X_, Y_;
                            (X_, Y_, _) = stationierungsElement.GetPointAtS(s);
                            element.projections.Add(new ProjectionArrow(new Coordinates(Interpolation.Y[i], Interpolation.X[i]), new Coordinates(Y_, X_)));
                        }
#endif
                    }
                    else //if no trasseS provided use original value S
                    {
                        s = Interpolation.S[i];
                    }
                    GradientElementExt gradient = GetGradientElementFromS(s);
                    (Interpolation.H[i], Interpolation.s[i]) = (gradient != null ? gradient.GetHAtS(s) : (double.NaN,double.NaN));
                }
            }
        }

#if USE_SCOTTPLOT
        static Form Form;
        /// <value>Plot for a 2D overview of all plotted trassen</value>
        static ScottPlot.WinForms.FormsPlot Plot2D;
        /// <value>Callout for right-click selected Coordinate and s-projection</value>
        static ScottPlot.Plottables.Callout selectedS;
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

            foreach (TrassenElementExt element in Elemente)
            {
                var scatter = Plot2D.Plot.Add.Scatter(element.InterpY, element.InterpX);
                scatter.LegendText = Filename;
                ScottPlot.Color color = scatter.MarkerFillColor;
                ElementMarker marker = new(element, color);
                Plot2D.Plot.Add.Plottable(marker);
               
                var scatterT = PlotT.Plot.Add.Scatter(element.InterpY, element.InterpT, color);
                PlotT.Plot.Add.VerticalLine(element.Ystart, 2, color);
                var scatterK = PlotT.Plot.Add.ScatterLine(element.InterpY, element.InterpK, color);
                // tell each T and K plot to use a different axis
                scatterT.Axes.YAxis = PlotT.Plot.Axes.Left;
                scatterK.Axes.YAxis = PlotT.Plot.Axes.Right;

                var scatterH = PlotG.Plot.Add.Scatter(element.InterpY, element.InterpH, color);
                scatterH.Axes.YAxis = PlotG.Plot.Axes.Left;
                var scatterSlope = PlotG.Plot.Add.ScatterLine(element.InterpY, element.InterpSlope, color);
                scatterSlope.Axes.YAxis = PlotG.Plot.Axes.Right;

                //Raw Data to GridView
                gridView.Rows.Add(element.ID, element.R1, element.R2, element.Ystart, element.Xstart, element.T, element.S, element.KzString, element.L, element.U1, element.U2, element.C);
                //Warnings
                foreach (var warning in element.GetWarnings)
                {
                    Plot2D.Plot.Add.Plottable(warning);
                }
                //Visualize Projections
                foreach (ProjectionArrow projection in element.projections)
                {
                    projection.IsVisible = false;
                    Plot2D.Plot.Add.Plottable(projection);
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
                PlotG.Plot.Add.Annotation("No Gradient Data available");
            }

            // Set the axis scale to be equal
            Plot2D.Plot.Axes.SquareUnits();
            Plot2D.Plot.HideLegend();
            Plot2D.Refresh();
            if (!Form.Visible) Form.ShowDialog();
            Form.Update();
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
            foreach (var warning in Plot2D.Plot.GetPlottables<WarningCallout>())
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
                double s = element.GetSAtPoint(coordinates.Y, coordinates.X);
                if (Double.IsNaN(s)) return;
                (double X, double Y, _) = element.GetPointAtS(s);
                if (selectedS == null)
                {
                    selectedS = Plot2D.Plot.Add.Callout(s.ToString(), Y + 10, X + 10, Y, X);
                }
                else
                {
                    selectedS.TextCoordinates = coordinates;
                    selectedS.TipCoordinates = new Coordinates(Y, X);
                    selectedS.Text = s.ToString();
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
        public Coordinates Start { get; set; }
        public Coordinates End { get; set; }

        public override LabelStyle LabelStyle { get; set; } = new() { FontSize = 14 };
        double t;
        public ElementMarker(TrassenElementExt element, ScottPlot.Color Color)
        {
            Start = new Coordinates(element.Ystart - Math.Sin(element.T + 0.5 * Math.PI) * 10, element.Xstart - Math.Cos(element.T + 0.5 * Math.PI) * 10);
            End = new Coordinates(element.Ystart + Math.Sin(element.T + 0.5 * Math.PI) * 10, element.Xstart + Math.Cos(element.T + 0.5 * Math.PI) * 10);
            this.Color = Color;
            this.LabelText = element.ID.ToString() + "_" + element.KzString; ;
            this.LabelRotation = (float)(element.T*(180/Math.PI));
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
}
