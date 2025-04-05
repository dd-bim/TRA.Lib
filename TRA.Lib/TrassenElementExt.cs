using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ScottPlot.Finance;
using System.Collections.Concurrent;
using System.Windows.Forms;
using Microsoft.Extensions.Logging.Abstractions;
using System.Media;
using System.Xml.Linq;

#if USE_SCOTTPLOT
using SkiaSharp;
using ScottPlot;
#endif

namespace TRA_Lib
{
    public struct Interpolation
    {
        /// <value>Hochwert</value>
        public double[] X;
        /// <value>Rechtswert</value>
        public double[] Y;
        /// <value>Hoehe</value>
        public double[] H;
        /// <value>Station</value>
        public double[] S;
        /// <value>Richtung</value>
        public double[] T;
        /// <value>Krümmung[1/m]</value>
        public double[] K;
        /// <value>Steigung[‰]</value>
        public double[] s;

        public Interpolation(int num = 0)
        {
            X = new double[num];
            Y = new double[num];
            S = new double[num];
            T = new double[num];
            K = new double[num];
            //Only 3D Interpolation
            H = null;
            s = null;
        }
        public readonly bool IsEmpty()
        {
            return (X == null || Y == null || S == null || T == null || K == null);
        }
        public void Concat(Interpolation interp)
        {
            if (interp.IsEmpty()) { return; }
            X = X.Concat(interp.X).ToArray();
            Y = Y.Concat(interp.Y).ToArray();
            S = S.Concat(interp.S).ToArray();
            T = T.Concat(interp.T).ToArray();
            K = K.Concat(interp.K).ToArray();

            if (interp.H != null)
            {
                if (H == null)
                {
                    H = new double[0];
                }
                H = H.Concat(interp.H).ToArray();
            }
            if (interp.s != null)
            {
                if (s == null)
                {
                    s = new double[0];
                }
                s = s.Concat(interp.s).ToArray();
            }
        }
        public static Interpolation Concat(Interpolation[] interp)
        {
            Interpolation Out = new Interpolation(0);
            foreach (Interpolation i in interp)
            {
                Out.Concat(i);
            }
            return Out;
        }
    }

    public class TrassenElementExt : TrassenElement
    {
        /// <value>Trasse this Element belongs to</value>
        public Trasse owner;
        /// <value>Geometry of this element</value>
        TrassenGeometrie TrassenGeometrie;
        public Type GetGeometryType()
        {
            return TrassenGeometrie.GetType();
        }
        /// <value>ID des Elements</value>
        int id;
        /// value>Scale - takes into account a scaling of the element length through scale corrections during the transformation</value> 
        double scale = 1;
        /// <value>Vorgaenger Element</value>
        TrassenElementExt predecessor;
        /// <value>Nachfolger Element</value>
        TrassenElementExt successor;
        /// <value>Interpolationsobjekt</value>
        Interpolation Interpolation;

        /// <value>List of Warnings. Callouts to show on Plot if compiled with SCOTTPLOT</value>
        internal List<GeometryWarning> WarningCallouts = new() { };

        public double MeanProjectionDeviation() { 
            var filteredProjections = projections?.Where(w => w != null);
            return filteredProjections.Sum(w => w.Deviation) / projections.Count; }
        public void ClearProjections() { projections.Clear(); }
        /// <value>Arrows for visualisation of ProjectionS</value>
        internal List<ProjectionArrow> projections = new() { };

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
        public double Yend { get { return successor != null ? successor.y : y; } }
        /// <value>Hochwert am Elementende</value>
        public double Xend { get { return successor != null ? successor.x : x; } }
        /// <value>Station am Elementanfang</value>
        public double S { get { return s; } }
        /// <value>Länge des Elements</value>
        public double L { set { l = value; } get { return l; } }
        /// <value>Überhöhung am Elementanfang</value>
        public double U1 { get { return u1; } }
        /// <value>Überhöhung am Elementende</value>
        public double U2 { get { return u2; } }
        /// <value>Punktnummer</value>
        public string C { get { return c.ToString("F5").Replace(',','.'); } }
        public float Cf { get { return c; } }
        /// <value>Richtung am Elementanfang</value>
        public double T { get { return t; } }
        public int Kz { get { return (int)kz; } }
        public string KzString { get { return kz.ToString(); } }
        public double Scale { get { return scale; } }
        public void ApplyScale() { if(!double.IsNaN(scale)) l = l*scale; }
        /// <value>Vorgaenger Element</value>
        public TrassenElementExt Predecessor { get { return predecessor; } }
        /// <value>Hochwert am Elementanfang</value>
        public TrassenElementExt Successor { get { return successor; } }
        /// <value>Returns Interpolationresult</value>
        public Interpolation InterpolationResult { get { return Interpolation; }}

#if USE_SCOTTPLOT
        public ScottPlot.Color PlotColor;
#endif
        public TrassenElementExt(double r1, double r2, double y, double x, double t, double s, int kz, double l, double u1, double u2, float c, int idx, Trasse owner, TrassenElementExt predecessor = null)
            : base(r1, r2, y, x, t, s, kz, l, u1, u2, c)
        {
            id = idx;
            if (predecessor != null)
            {
                this.predecessor = predecessor;
                predecessor.successor = this;
            }
            this.owner = owner;
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
                    TrassenGeometrie = new Bloss(r1,r2, l);
                    break;
                case Trassenkennzeichen.Knick:
                    break;
                case Trassenkennzeichen.KSprung:
                    TrassenGeometrie = new KSprung(l);
                    break;
                case Trassenkennzeichen.S_Form_1f:
                    break;
                case Trassenkennzeichen.Bloss_1f:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Set a new Transform to a Element
        /// </summary>
        /// <param name="x">new Hochwert</param>
        /// <param name="y">new Rechtswert</param>
        /// <param name="deltaGamma">delta in Heading (Meridiankonvergenz)</param>
        /// <param name="deltaK_start">Scale at Elementstart (Masstabsfaktor)</param>
        /// <param name="deltaK_end">Scale at Elementend (Masstabsfaktor)</param>
        /// <param name="bFitHeading">Fits Heading by rotating the element to have the last interpolationpoint as close as possible to successors start</param>
        /// <param name="bFitLength">Calculate Scale to have the interpolationpoint at L as close as possible to successors start</param>
        public void Relocate(double x = double.NaN, double y = double.NaN, double deltaGamma = double.NaN, double deltaK_start = double.NaN, double deltaK_end = double.NaN,bool bFitHeading = false,bool bFitLength = false)
        {
            if (!double.IsNaN(x))
            {
                this.x = x;
            }
            if (!double.IsNaN(y))
            {
                this.y = y;
            }
            if (!double.IsNaN(deltaGamma))
            {
                t = t - deltaGamma;
            }
            if (!double.IsNaN(deltaK_start))
            {
                scale = (deltaK_start + deltaK_end) / 2;
                if (Double.IsNaN(deltaK_end)) { deltaK_end = deltaK_start; }
                switch (this.TrassenGeometrie)
                {
                    case Gerade:
                        break;
                    case Kreis:
                        r1 = r1 * (deltaK_start + deltaK_end) / 2;
                        r2 = r1;
                        break;
                    case Klothoid:
                        r1 = r1 * deltaK_start;
                        r2 = r2 * deltaK_end;
                        break;
                }
                TrassenGeometrie.updateParameters(l * scale, r1, r2);
            }
            if (bFitHeading)
            {
                double Xi, Yi; //end-coordinates calculated from geometry 
                (Xi, Yi, _) = GetPointAtS(l, true);
                double gammai = Math.Atan2(Xi - Xstart, Yi - Ystart); //heading(Richtungswinkel) from geometry
                double gammat = Math.Atan2(Xend - Xstart, Yend - Ystart); //heading(Richtungswinkel) from element start points
                t = t - (gammat - gammai);
            }
            if (bFitLength)
            {
                //optimize length
                double Xi, Yi, Ti; //end-coordinates calculated from geometry 
                (Xi, Yi, Ti) = GetPointAtS(l, true);
                Transform2D transformAtEnd = new Transform2D(Xi, Yi, Ti);
                double targetX, targetY, targetT;
                targetX = Xend;
                targetY = Yend;
                targetT = 0;
                transformAtEnd.ApplyInverse(ref targetX, ref targetY, ref targetT);
                scale = (l*scale + targetX) / l;
                TrassenGeometrie.updateParameters(l * scale, r1, r2);//Set new parameters to Geometry
            }
            
        }
        /// <summary>
        /// Plausibility Check
        /// </summary>
        public bool PlausibilityCheck(bool bCheckRadii = false)
        {
            WarningCallouts.Clear(); 
            //Radii
            if (kz == Trassenkennzeichen.Gerade && r1 != 0 & r2 != 0) { AddWarningCallout("given Radii are not matching to KZ as it is 'Gerade''", Xstart, Ystart); }
            if (kz == Trassenkennzeichen.Kreis && r1 != r2) { AddWarningCallout("given Radii are not equal for KZ is 'Kreis''", Xstart, Ystart); }
            //Connectivity by Station & Length
            if (predecessor != null)
            {
                if (Math.Abs(predecessor.s + predecessor.l - s) > 1E-9) { AddWarningCallout("predecessor length missmatch. elements are not connected", Xstart, Ystart); }
            }
            if (successor != null)
            {
                if (Math.Abs(s + l - successor.s) > Trassierung.StationMismatchTolerance) { AddWarningCallout("length missmatch. element is not connected to successor", Xend, Yend); }
            }
            //Connectivity & continuity by Interpolation          
            if (Interpolation.X?.Length > 0 && successor != null)
            {
                //Connectivity
                if (Math.Abs(Interpolation.X.Last() - successor.x) > Trassierung.ConnectivityMismatchTolerance && Math.Abs(Interpolation.Y.Last() - successor.y) > Trassierung.ConnectivityMismatchTolerance)
                {
                    //TrassierungLog.Logger?.LogWarning("Last interpolated Element(ID" + id.ToString() + "_" + kz.ToString() + ") coordinate differs from successors start coordinate by " + Math.Sqrt(Math.Pow(Interpolation.X.Last() - successor.x, 2) + Math.Pow(Interpolation.Y.Last() - successor.y, 2)).ToString());
                    AddWarningCallout("coordinate difference \n" + Math.Sqrt(Math.Pow(Interpolation.X.Last() - successor.x, 2) + Math.Pow(Interpolation.Y.Last() - successor.y, 2)).ToString(), Interpolation.X.Last(), Interpolation.Y.Last());
                }
                //Continuity of Heading
                if (Math.Abs(Interpolation.T.Last() - successor.T) > Trassierung.ContinuityOfHeadingTolerance)
                {
                    //TrassierungLog.Logger?.LogWarning("Last interpolatedElement(ID" + id.ToString() + "_" + kz.ToString() + ") heading differs from successors start heading by " + (Interpolation.T.Last() - successor.T).ToString());
                    AddWarningCallout("heading difference \n" + (Interpolation.T.Last() - successor.T).ToString(), Interpolation.X.Last(), Interpolation.Y.Last());
                }
                //Continuity of Radii(Curvature)
                if (bCheckRadii && Math.Abs(Interpolation.K.Last() == 0 ? 0 : 1 / Interpolation.K.Last() - successor.r1) > Trassierung.ContinuityOfCurvatureTolerance)
                {
                    //TrassierungLog.Logger?.LogWarning("Last interpolatedElement(ID" + id.ToString() + "_" + kz.ToString() + ") radius differs from successors start radius by " + (Interpolation.K.Last() == 0 ? 0 : 1 / Interpolation.K.Last() - successor.r1).ToString());
                    AddWarningCallout("curvature difference \n" + (Interpolation.K.Last() == 0 ? 0 : 1 / Interpolation.K.Last() - successor.r1).ToString(), Interpolation.X.Last(), Interpolation.Y.Last());
                }
            }
            return true;
        }
        void AddWarningCallout(string text, double X, double Y)
        {
            WarningCallouts.Add(new GeometryWarning(text, X, Y, this));
        }

        /// <summary>
        /// Interpolate the underlying geometry
        /// </summary>
        /// <param name="delta">distance along geometry between interpolation points</param>
        /// <param name="allowedTolerance">maximal allowed distance between geometry and interpolated polyline, if set to zero this value is ignored</param>
        /// <returns>Interpolation: array of coordinates, along with heading and curvature for each point</returns>
        public ref Interpolation Interpolate(double delta = 1.0, double allowedTolerance = 0.001)
        {
            Transform2D transform = new Transform2D(x, y, t);
            if (TrassenGeometrie == null)
            {
                AddWarningCallout("No Geometry for interpolation " + kz.ToString() + " set, maybe not implemented yet", Xstart,Ystart); 
                Interpolation = new Interpolation();
                return ref Interpolation;
            }
            if (Double.IsNaN(l)) { AddWarningCallout("Length is NaN, no interpolation calculated", Xstart, Ystart); return ref Interpolation; }
            List<double> Xlst = new List<double>();
            List<double> Ylst = new List<double>();
            List<double> Slst = new List<double>();
            List<double> Tlst = new List<double>();
            List<double> Klst = new List<double>();

            double s_ = 0;
            double delta_ = delta;
            bool done = false;
            do
            {
                double x_, y_, t_, k_;
                if (s_ > l) { s_ = l; done = true; }
                (x_, y_, t_, k_) = TrassenGeometrie.PointAt(s_*scale);
                if (transform != null) { transform.Apply(ref x_, ref y_, ref t_); }
                Xlst.Add(x_);
                Ylst.Add(y_);
                Slst.Add(s_);
                Tlst.Add(t_);
                Klst.Add(k_);
                //Calc curvature dependent delta:
                if (k_ != 0 && allowedTolerance != 0)
                {
                    double r = 1 / Math.Abs(k_);
                    delta_ = 2 * r * Math.Acos(1 - (allowedTolerance / r));
                }
                else
                {
                    delta_ = delta;
                }
                s_ = s_ + Math.Min(delta_, delta); //Use smalest delta
            }
            while (!done);
            Interpolation.X = Xlst.ToArray();
            Interpolation.Y = Ylst.ToArray();
            Interpolation.S = Slst.ToArray();
            Interpolation.T = Tlst.ToArray();
            Interpolation.K = Klst.ToArray();

            PlausibilityCheck();
            return ref Interpolation;
        }

        public double GetSAtPoint(double X, double Y, double T = double.NaN)
        {
            if (TrassenGeometrie == null) return (0);
            Transform2D transform = new Transform2D(x, y, t);
            transform.ApplyInverse(ref X, ref Y, ref T);
            TrassenGeometrie copy = (TrassenGeometrie)TrassenGeometrie.Clone(); //Clone Geometry for Multithreading Access
            double output = copy.sAt( X, Y, T)/scale + s;
            return output;
        }
        /// <summary>
        /// Calculates a Point on the Geometry at a given Distance S
        /// </summary>
        /// <param name="S">S as mileage (global by default)</param>
        /// <param name="local">S is used as global mileage by default, so S is reduced by Stationvalue S of the element. if set to true S is directly used as S in element mileage</param>
        /// <returns>Hochwert X,Rechtswert Y, Heading T</returns>
        public (double, double, double) GetPointAtS(double S, bool local = false)
        {
            if (TrassenGeometrie == null) return (0, 0, 0);// (Double.NaN, Double.NaN, Double.NaN);
            (double X, double Y, double T, _) = TrassenGeometrie.PointAt((local == false ? S - s : S)*scale);
            Transform2D transform = new Transform2D(x, y, t);
            transform.Apply(ref X, ref Y, ref T);
            return (X, Y, T);
        }

        public override string ToString()
        {
            CultureInfo info = CultureInfo.CurrentCulture;
            string[] values = { r1.ToString(info),
                r2.ToString(info),
                y.ToString(info),
                x.ToString(info),
                t.ToString(info),
                s.ToString(info),
                kz.ToString(),
                l.ToString(info),
                u1.ToString(info),
                u2.ToString(info),
                C, "","",
                scale.ToString(info),
                MeanProjectionDeviation().ToString(info),
                "\"" + string.Join(Environment.NewLine, WarningCallouts.Select(w => w.Text)) + "\""
                };
            return String.Join(info.TextInfo.ListSeparator, values);
        }
        public void Print()
        {
            Console.WriteLine("R1:" + r1 + " R2:" + r2 + " Y:" + y + " X:" + x + " T:" + t + " S:" + s + " Kz:" + kz.ToString() + " L:" + l + " U1:" + u1 + " U2:" + u2 + " C:" + c);
        }
    }
#if USE_SCOTTPLOT
    public class GeometryWarning : ScottPlot.Plottables.Callout
    {
        static double Xlast; //prevent multiple callouts at same location
        public TrassenElementExt trasse;
        public GeometryWarning(string text, double X, double Y, object owner)
        {
            ScottPlot.Color color = ScottPlot.Color.FromSDColor(System.Drawing.Color.Yellow);
            ScottPlot.Color LineColor = ScottPlot.Color.FromSDColor(System.Drawing.Color.Red);
            Text = text;
            TipCoordinates = new Coordinates(Y, X);
            TextCoordinates = new Coordinates(Y + 10, (X != Xlast ? X : X = X - 5) + 10);
            Xlast = X;
            ArrowLineColor = LineColor;
            ArrowFillColor = color;
            TextBorderColor = LineColor;
            TextBackgroundColor = color.Lighten();
            trasse = (TrassenElementExt)owner;
            string ownerString = trasse != null && trasse.owner != null ? trasse.owner.Filename + "_" + trasse.ID : null;
            TrassierungLog.Logger?.Log_Async(LogLevel.Warning, ownerString + " " + Text, owner);
        }
    }

#else
 public class GeometryWarning 
    {
    public string Text;
    public GeometryWarning(string text, double X, double Y, object owner)
        {
            Text = text;
            TrassenElementExt trasse = (TrassenElementExt)owner;
            string ownerString = trasse != null ? trasse.owner.Filename + "_" + trasse.ID : null;
            TrassierungLog.Logger?.LogWarning(ownerString + " " + Text, owner) ;
        }
    }
#endif
    }

