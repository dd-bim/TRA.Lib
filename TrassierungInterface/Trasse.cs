

using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using ScottPlot;
using ScottPlot.TickGenerators.TimeUnits;
using System.Windows.Forms;
using System.Security.Cryptography.Xml;
using System.Reflection.PortableExecutable;
using System.Drawing;
using System.Reflection.Emit;

// AssemblyInfo.cs
[assembly: InternalsVisibleTo("KomponentenTest")]

namespace TrassierungInterface
{
    public interface ICustomLogger { void LogInformation(string message); void LogWarning(string message); void LogError(string message, Exception ex); }

    enum Trassenkennzeichen
    {
        Gerade,
        Kreis,
        Klotoide,
        UB_S_Form, //ÜB S-Form
        Bloss,
        Knick,  //Gerade/Knick R1 Knickwinkel am Ende der Gerade (199.937-200.063Gon)
        KSprung,    //L = Ueberlaenge bzw. Fehllaenge
        S_Form_1f, //S-Form (1f geschw.)
        Bloss_1f    //Bloss (1f geschw.)
    }
    struct Interpolation
    {
        /// <value>Hochwert</value>
        public double[] X;
        /// <value>Rechtswert</value>
        public double[] Y;
        /// <value>Hoehe</value>
        public double[] Z;
        /// <value>Station</value>
        public double[] S;
        /// <value>Richtung</value>
        public double[] T;
        /// <value>Krümmung</value>
        public double[] K;
    }

    public class Trasse
    {
        public string Filename;
        public TrassenElement[] Elemente;
        static Form Form;
        static ScottPlot.WinForms.FormsPlot Plot2D;
        static ScottPlot.Plottables.Callout selectedS;
        ScottPlot.WinForms.FormsPlot PlotT;
        DataGridView gridView;
        /// <summary>
        /// Iterates backwards over Elements and returns first (and nearest) Element the point is included
        /// </summary>
        /// <param name="X">X coordinate of Point of Interest</param>
        /// <param name="Y">Y coordinate of Point of Interest</param>
        /// <returns></returns>
        internal TrassenElement GetElementFromPoint(double X, double Y)
        {
            foreach (TrassenElement element in Elemente.Reverse())
            {
                Vector2 v1 = new Vector2((float)Math.Cos(element.T), (float)Math.Sin(element.T));
                Vector2 v2 = new Vector2((float)(X - element.Xstart),(float)(Y - element.Ystart));
                if (Vector2.Dot(v1, v2) > 0) { return element; }
            }
            return null;
        }
#if USE_SCOTTPLOT
        public void Plot()
        {
            if (Form == null) {
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
                TabControl tabControl = new TabControl
                {
                    Dock = DockStyle.Fill
                };
                tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
                Form.Controls.Add(splitContainer);
                Form.Controls.Add(BtnPanel);
                BtnPanel.Controls.Add(CheckElementLabels);
                BtnPanel.Controls.Add(CheckShowWarnings);   
                splitContainer.Panel2.Controls.Add(tabControl);
                
            }
            //Add Plot for 2D overview
            PixelPadding padding = new(80, 80, 20, 5);
            if (Plot2D == null) {
                Plot2D = new ScottPlot.WinForms.FormsPlot { Dock = DockStyle.Fill };
                Form.Controls.OfType<SplitContainer>().First<SplitContainer>().Panel1.Controls.Add(Plot2D);         
                Plot2D.Plot.Layout.Fixed(padding);
                //Plot2D.Plot.XLabel("Rechtswert Y[m]");
                Plot2D.Plot.YLabel("Hochwert X[m]");
                Plot2D.MouseDown += Plot2D_MouseClick;
            }
            //Add Plot for heading and curvature
            if (PlotT == null) {
                TabPage tabPage = new TabPage { Text = Filename};
                SplitContainer splitContainer = new SplitContainer
                {
                    Dock = DockStyle.Fill,
                    //Height = 500,
                    Orientation = System.Windows.Forms.Orientation.Horizontal,
                    SplitterDistance = (int)(tabPage.Height * 0.9),
                };
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
                PlotT = new ScottPlot.WinForms.FormsPlot { Dock = DockStyle.Fill };
                padding = new(80, 80, 50, 5);
                PlotT.Plot.Layout.Fixed(padding);
                PlotT.Plot.XLabel("Rechtswert Y[m]");
                PlotT.Plot.Axes.Left.Label.Text = "Heading [rad]";
                PlotT.Plot.Axes.Right.Label.Text = "Curvature [1/m]";
                splitContainer.Panel1.Controls.Add(PlotT);
                splitContainer.Panel2.Controls.Add(gridView);
                tabPage.Controls.Add(splitContainer);
                Form.Controls.OfType<SplitContainer>().First<SplitContainer>().Panel2.Controls.OfType<TabControl>().First<TabControl>().Controls.Add(tabPage);
            }
            Plot2D.Plot.Axes.Link(PlotT, true, false);
            PlotT.Plot.Axes.Link(Plot2D, true, false);                         
            
            foreach (TrassenElement element in Elemente)
            {
                var scatter = Plot2D.Plot.Add.Scatter(element.InterpY, element.InterpX);
                scatter.LegendText = Filename;
                var callout = Plot2D.Plot.Add.Callout(element.ID + "_" + element.KzString,
                    textLocation: new(element.Ystart + Math.Cos(element.T-0.5*Math.PI)*10, element.Xstart + Math.Sin(element.T - 0.5 * Math.PI) * 10),
                    tipLocation: new(element.Ystart, element.Xstart));
                ScottPlot.Color color = scatter.MarkerFillColor;
                callout.LabelBackgroundColor = color;
                callout.LabelBorderColor = color.Lighten();
                callout.ArrowLineColor = color;
                callout.ArrowFillColor = color;
                var scatterT = PlotT.Plot.Add.Scatter(element.InterpY, element.InterpT, color);
                PlotT.Plot.Add.VerticalLine(element.Ystart,2, color);
                var scatterK = PlotT.Plot.Add.ScatterLine(element.InterpY, element.InterpK, color);
                // tell each T and K plot to use a different axis
                scatterT.Axes.YAxis = PlotT.Plot.Axes.Left;
                scatterK.Axes.YAxis = PlotT.Plot.Axes.Right;

                //Raw Data to GridView
                gridView.Rows.Add(element.ID,element.R1,element.R2,element.Ystart, element.Xstart,element.T,element.S,element.KzString,element.L,element.U1,element.U2,element.C);
                //Warnings
                foreach (var warning in element.GetWarnings)
                {
                    Plot2D.Plot.Add.Plottable(warning);
                }
            }
            // Set the axis scale to be equal
            Plot2D.Plot.Axes.SquareUnits();
            Plot2D.Plot.HideLegend();
            Plot2D.Refresh();
            if(!Form.Visible) Form.ShowDialog();
            Form.Update();
        }

        private void CheckElementLabels_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            bool show = box.Checked;
            foreach (var callout in Plot2D.Plot.GetPlottables<ScottPlot.Plottables.Callout>())
            {
                if(callout is not WarningCallout) callout.IsVisible = show;
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
            if (e.Button == MouseButtons.Right)
            {
                Coordinates coordinates = Plot2D.Plot.GetCoordinates(new Pixel(e.X, e.Y));
                TrassenElement element = GetElementFromPoint(coordinates.Y, coordinates.X);
                if (element == null) { return; }
                double s = element.GetSAtPoint(coordinates.Y, coordinates.X);
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
            if(control != null) { label = control.SelectedTab.Text; }
            foreach (var plot in Plot2D.Plot.GetPlottables<ScottPlot.Plottables.Scatter>()) {
                plot.MarkerSize = plot.LegendText == label ? 5 : 0;
            }
            Plot2D.Refresh();
        }
#endif

    }

    public class TrassenElement
    {
        ///TRA-Attribute
       
        /// <value>Radius am Elementanfang</value>
        double r1;
        /// <value>Radius am Elementende</value>
        double r2;
        /// <value>Rechtswert am Elementanfang</value>
        double y;
        /// <value>Hochwert am Elementanfang</value>
        double x;
        /// <value>Richtung am Elementanfang</value>
        double t;
        /// <value>Station am Elementanfang</value>
        double s;
        /// <value>Kennzeichen des Elements</value>
        Trassenkennzeichen kz;
        TrassenGeometrie TrassenGeometrie;
        /// <value>Laenge des ELements</value>
        double l;
        /// <value>Ueberhoehung am Elementanfang</value>
        double u1;
        /// <value>Ueberhoehung am Elementende</value>
        double u2;
        /// <value>Abstand zur Trasse, i.d.R. 0 nur für Parallelübergangsbögen</value>
        float c;

        /// ergaenzende Attribute

        /// <value>ID des Elements</value>
        int id;
        /// <value>Vorgaenger Element</value>
        TrassenElement predecessor;
        /// <value>Nachfolger Element</value>
        TrassenElement successor;
        /// <value>Gradienten des Trassenelements</value>
        GradientElement[] Gradienten;
        /// <value>Gleisscheren des Trassenelements</value>
        GleisscherenElement[] Gleisscheren;
        /// <value>Interpolationsobjekt</value>
        Interpolation Interpolation;
#if USE_SCOTTPLOT
        List<WarningCallout> WarningCallouts = new() { };
#endif

        /// public
        ///<value>ID des Elements innerhalb der Trasse</value>
        public int ID { get { return id; } }
        /// <value>Radius am Elementanfang</value>
        public double R1 { get { return r1; } }
        /// <value>Radius am Elementende</value>
        public double R2 { get { return r2; } }
        /// <value>Rechtswert am Elementanfang</value>
        public double Ystart { get { return y; } }
        /// <value>Hochwert am Elementanfang</value>
        public double Xstart { get { return x; } }
        /// <value>Rechtswert am Elementende</value>
        public double Yend { get { return successor.y; } }
        /// <value>Hochwert am Elementende</value>
        public double Xend { get { return successor.x; } }
        /// <value>Station am Elementanfang</value>
        public double S { get { return s; } }
        /// <value>Länge des Elements</value>
        public double L { get { return l; } }
        /// <value>Überhöhung am Elementanfang</value>
        public double U1 { get { return u1; } }
        /// <value>Überhöhung am Elementende</value>
        public double U2 { get { return u2; } }
        /// <value>Abstand zu Trasse</value>
        public double C { get { return c; } }
        /// <value>Richtung am Elementanfang</value>
        public double T { get { return t; } }
        public int Kz { get { return (int)kz; } }
        public string KzString { get { return kz.ToString(); } }
        /// <value>Vorgaenger Element</value>
        public TrassenElement Predecessor { get { return predecessor; } }
        /// <value>Hochwert am Elementanfang</value>
        public TrassenElement Successor { get { return successor; } }
        /// <value>X-Koordinaten der Interpolationspunkte</value>
        public double[] InterpX { get { return Interpolation.X == null? new double[0]: Interpolation.X; } }
        /// <value>Y-Koordinaten der Interpolationspunkte</value>
        public double[] InterpY {get { return Interpolation.Y == null ? new double[0] : Interpolation.Y; } }
        /// <value>Richtung der Interpolationspunkte</value>
        public double[] InterpT { get { return Interpolation.T == null ? new double[0] : Interpolation.T; } }
        /// <value>Krümmung der Interpolationspunkte</value>
        public double[] InterpK { get { return Interpolation.K == null ? new double[0] : Interpolation.K; } }
#if USE_SCOTTPLOT
        /// <value>List of Warning Callouts to show on Plot</value>
        public WarningCallout[] GetWarnings { get { return WarningCallouts.ToArray(); } }
#endif

        public TrassenElement(double r1, double r2, double y, double x, double t, double s, int kz, double l, double u1, double u2, float c, int idx, TrassenElement predecessor = null, ILogger<TrassenElement> logger = null)
        {
            this.r1 = r1;
            this.r2 = r2;
            this.y = y;
            this.x = x;
            this.t = t;
            this.s = s;
            if (Enum.IsDefined(typeof(Trassenkennzeichen), kz))
            {
                this.kz = (Trassenkennzeichen)kz;
            }
            else
            {
                throw new ArgumentException("KZ is no valid index for a Trassenkennzeichen", nameof(kz));
            }
            this.l = l;
            this.u1 = u1;
            this.u2 = u2;
            this.c = c;

            id = idx;
            if (predecessor != null)
            {
                this.predecessor = predecessor;
                predecessor.successor = this;
            }
            PlausibilityCheck();

            switch (this.kz)
            {
                case Trassenkennzeichen.Gerade:
                    TrassenGeometrie = new Gerade();
                    break;
                case Trassenkennzeichen.Kreis:
                    TrassenGeometrie = new Kreis(r1);
                    break;
                case Trassenkennzeichen.Klotoide:
                    TrassenGeometrie = new Klothoid(r1, r2, l);
                    break;
                case Trassenkennzeichen.UB_S_Form:
                    break;
                case Trassenkennzeichen.Bloss:
                    break;
                case Trassenkennzeichen.Knick:
                    break;
                case Trassenkennzeichen.KSprung:
                    TrassenGeometrie = new Gerade();
                    break;
                case Trassenkennzeichen.S_Form_1f:
                    break;
                case Trassenkennzeichen.Bloss_1f:
                    break;
                default:
                    break;
            }
        }

        public void Relocate(double x, double y, double t)
        {
            this.x = x;
            this.y = y;
            this.t = t;
            PlausibilityCheck();
        }
        /// <summary>
        /// Plausibility Check
        /// </summary>
        public bool PlausibilityCheck(bool bCheckRadii = false)
        {
            WarningCallouts.Clear();
            //Radii
            if (kz == Trassenkennzeichen.Gerade && r1 != 0 & r2 != 0) { TrassierungLog.Logger?.LogWarning("given Radii are not matching to KZ as it is 'Gerade''", nameof(kz)); }
            if (kz == Trassenkennzeichen.Kreis && r1 != r2) { TrassierungLog.Logger?.LogWarning("given Radii are not equal for KZ is 'Kreis''", nameof(kz)); }
            //Connectivity by Station & Length
            if (predecessor != null)
            {
                if (predecessor.s + predecessor.l != s) { TrassierungLog.Logger?.LogWarning("predecessor length missmatch. elements are not connected", nameof(l)); }
            }
            if (successor != null)
            {
                if (s + l != successor.s) { TrassierungLog.Logger?.LogWarning("length missmatch. element is not connected to successor", nameof(l)); }
            }
            //Connectivity & continuity by Interpolation
            double tolerance = 0.00000001;
            if (Interpolation.X?.Length >0 && successor != null)
            {
                //Connectivity
                if (Math.Abs(Interpolation.X.Last() - successor.x) > tolerance && Math.Abs(Interpolation.Y.Last() - successor.y) > tolerance){
                    TrassierungLog.Logger?.LogWarning("Last interpolated Element(ID" + id.ToString() + "_" + kz.ToString() + ") coordinate differs from successors start coordinate by " + Math.Sqrt(Math.Pow(Interpolation.X.Last() - successor.x,2) + Math.Pow(Interpolation.Y.Last() - successor.y,2)).ToString());
                    AddWarningCallout("coordinate difference \n" + Math.Sqrt(Math.Pow(Interpolation.X.Last() - successor.x, 2) + Math.Pow(Interpolation.Y.Last() - successor.y, 2)).ToString(), Interpolation.X.Last(), Interpolation.Y.Last());
                }
                //Continuity of Heading
                if (Math.Abs(Interpolation.T.Last() - successor.T) > tolerance)
                {
                    TrassierungLog.Logger?.LogWarning("Last interpolatedElement(ID" + id.ToString() + "_" + kz.ToString() + ") heading differs from successors start heading by " + (Interpolation.T.Last() - successor.T).ToString());
                    AddWarningCallout("heading difference \n" + (Interpolation.T.Last() - successor.T).ToString(), Interpolation.X.Last(), Interpolation.Y.Last());
                }
                //Continuity of Radii(Curvature)
                if (bCheckRadii && Math.Abs(Interpolation.K.Last() == 0?0:1/ Interpolation.K.Last() - successor.r1) > tolerance)
                {
                    TrassierungLog.Logger?.LogWarning("Last interpolatedElement(ID" + id.ToString() + "_" + kz.ToString() + ") radius differs from successors start radius by " + (Interpolation.K.Last() == 0 ? 0 : 1 / Interpolation.K.Last() - successor.r1).ToString());
                    AddWarningCallout("curvature difference \n" + (Interpolation.K.Last() == 0 ? 0 : 1 / Interpolation.K.Last() - successor.r1).ToString(), Interpolation.X.Last(), Interpolation.Y.Last());
                }
            }
            return true;
        }
        void AddWarningCallout(string text, double X, double Y)
        {
#if USE_SCOTTPLOT
            WarningCallouts.Add(new WarningCallout(text,X, Y));
#endif
        }

        public void Interpolate(double delta = 1.0)
        {
            Transform2D transform = new Transform2D(x, y, t);
            if (TrassenGeometrie == null) { TrassierungLog.Logger?.LogWarning("No Gemetry for interpolation " + kz.ToString() + "set, maybe not implemented yet", nameof(kz)); return; }

            int num = (int)Math.Abs(l / delta);
            if (l < 0 && delta > 0) { delta = -delta; } //set delta negative for negative lengths
            Interpolation.X = new double[num + 1];
            Interpolation.Y = new double[num + 1];
            Interpolation.Z = new double[num + 1];
            Interpolation.S = new double[num + 1];
            Interpolation.T = new double[num + 1];
            Interpolation.K = new double[num + 1];

            for (int i = 0; i <= num; i++)
            {
                Interpolation.S[i] = i * delta;
                (Interpolation.X[i], Interpolation.Y[i], Interpolation.T[i], Interpolation.K[i]) = TrassenGeometrie.PointAt(i < num ? i * delta : l);
                if (transform != null) { transform.Apply(ref Interpolation.X[i], ref Interpolation.Y[i], ref Interpolation.T[i]); }
            }
            PlausibilityCheck();
        }
        public double GetSAtPoint(double X, double Y, double T = double.PositiveInfinity)
        {
            Transform2D transform = new Transform2D(x, y, t);
            transform.ApplyInverse(ref X,ref Y,ref T);
            return TrassenGeometrie.sAt(X, Y, T)+s;
        }

        public (double, double, double) GetPointAtS(double S)
        {
            (double X, double Y,_,_) = TrassenGeometrie.PointAt(S-s);
            Transform2D transform = new Transform2D(x, y, t);
            double T = 0.0;
            transform.Apply(ref X, ref Y,ref T);
            return (X, Y, T);
        }

        public void print()
        {
            Console.WriteLine("R1:" + r1 + " R2:" + r2 + " Y:" + y + " X:" + x + " T:" + t + " S:" + s + " Kz:" + kz.ToString() + " L:" + l + " U1:" + u1 + " U2:" + u2 + " C:" + c);
        }
    }
#if USE_SCOTTPLOT
    public class WarningCallout : ScottPlot.Plottables.Callout
    {
        static double Xlast; //prevent multiple callouts at same location
        public WarningCallout(string text, double X, double Y) 
        {
            ScottPlot.Color color = ScottPlot.Color.FromSDColor(System.Drawing.Color.Yellow);
            ScottPlot.Color LineColor = ScottPlot.Color.FromSDColor(System.Drawing.Color.Red);
            Text = text;
            TipCoordinates = new Coordinates(Y, X);
            TextCoordinates = new Coordinates(Y + 10, (X != Xlast?X:X = X-5) + 10);
            Xlast = X;
            ArrowLineColor = LineColor;
            ArrowFillColor = color;
            TextBorderColor = LineColor;
            TextBackgroundColor = color.Lighten();
        }
    }
#endif
}
