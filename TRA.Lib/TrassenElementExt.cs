using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ScottPlot.Finance;

#if USE_SCOTTPLOT
using SkiaSharp;
using ScottPlot;
#endif

namespace TRA_Lib
{
    /// <summary>
    /// A Collection triggering the Remove Action for all Items on Clearing
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CustomObservableCollection<T> : ObservableCollection<T> 
    {
        protected override void ClearItems() { 
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, this, Count));
            base.ClearItems(); 
        } 
    }
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
        public void Concat(Interpolation interp)
        {
            if (interp.X ==  null || interp.Y == null || interp.S == null || interp.T == null || interp.K == null) { return; }
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
        
    }

    public class TrassenElementExt : TrassenElement
    {
        /// <value>Trasse this Element belongs to</value>
        internal Trasse owner;
        /// <value>Geometry of this element</value>
        TrassenGeometrie TrassenGeometrie;
        /// <value>ID des Elements</value>
        int id;
        /// <value>Vorgaenger Element</value>
        TrassenElementExt predecessor;
        /// <value>Nachfolger Element</value>
        TrassenElementExt successor;
        /// <value>Interpolationsobjekt</value>
        Interpolation Interpolation;

        /// <value>List of Warnings. Callouts to show on Plot if compiled with SCOTTPLOT</value>
        internal CustomObservableCollection<GeometryWarning> WarningCallouts = new() { };

#if USE_SCOTTPLOT
        /// <value>Arrows for visualisation of ProjectionS</value>
        internal List<ProjectionArrow> projections = new() { };
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
        public double Yend { get { return successor != null ? successor.y : y; } }
        /// <value>Hochwert am Elementende</value>
        public double Xend { get { return successor != null ? successor.x : x; } }
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
        public TrassenElementExt Predecessor { get { return predecessor; } }
        /// <value>Hochwert am Elementanfang</value>
        public TrassenElementExt Successor { get { return successor; } }
        /// <value>Returns Interpolationresult</value>
        public Interpolation InterpolationResult { get { return Interpolation; }}

        /// <value>List of Warnings/ Callouts to show on Plot if compiled with SCOTTPLOT</value>
        //public GeometryWarning[] GetWarnings { get { return WarningCallouts.ToArray(); } }
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

        public void Relocate(double x, double y, double deltaGamma = double.NaN, double deltaK_start = double.NaN, double deltaK_end = double.NaN)
        {
            this.x = x;
            this.y = y;
            if (!double.IsNaN(deltaGamma))
            {
                t = t - deltaGamma;
            }
            if (!double.IsNaN(deltaK_start))
            {
                // TODO decide if we need to change the length and if yes, do we want to update the stationvalues s
                l = l * (deltaK_start + deltaK_end) / 2;
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
            }
            PlausibilityCheck();
        }
        /// <summary>
        /// Plausibility Check
        /// </summary>
        public bool PlausibilityCheck(bool bCheckRadii = false)
        {
            double tolerance = 1e-8;

            WarningCallouts.Clear();
            //Radii
            if (kz == Trassenkennzeichen.Gerade && r1 != 0 & r2 != 0) { AddWarningCallout("given Radii are not matching to KZ as it is 'Gerade''", Xstart, Ystart); }
            if (kz == Trassenkennzeichen.Kreis && r1 != r2) { AddWarningCallout("given Radii are not equal for KZ is 'Kreis''", Xstart, Ystart); }
            //Connectivity by Station & Length
            if (predecessor != null)
            {
                if (predecessor.s + predecessor.l != s) { AddWarningCallout("predecessor length missmatch. elements are not connected", Xstart, Ystart); }
            }
            if (successor != null)
            {
                if (s + l != successor.s) { AddWarningCallout("length missmatch. element is not connected to successor", Xend, Yend); }
            }
            //Length and geometrical Length Check
            if(TrassenGeometrie is Gerade)
            {
                double geoL = Math.Sqrt(Math.Pow(Xend-Xstart,2)+Math.Pow(Yend-Ystart,2));
                if (Math.Abs(l- geoL)>tolerance) { AddWarningCallout("length missmatch. elements Length parameter, does not match to Geometry by " + (l - geoL), Xstart, Ystart); }
            }
            //Connectivity & continuity by Interpolation          
            if (Interpolation.X?.Length > 0 && successor != null)
            {
                //Connectivity
                if (Math.Abs(Interpolation.X.Last() - successor.x) > tolerance && Math.Abs(Interpolation.Y.Last() - successor.y) > tolerance)
                {
                    //TrassierungLog.Logger?.LogWarning("Last interpolated Element(ID" + id.ToString() + "_" + kz.ToString() + ") coordinate differs from successors start coordinate by " + Math.Sqrt(Math.Pow(Interpolation.X.Last() - successor.x, 2) + Math.Pow(Interpolation.Y.Last() - successor.y, 2)).ToString());
                    AddWarningCallout("coordinate difference \n" + Math.Sqrt(Math.Pow(Interpolation.X.Last() - successor.x, 2) + Math.Pow(Interpolation.Y.Last() - successor.y, 2)).ToString(), Interpolation.X.Last(), Interpolation.Y.Last());
                }
                //Continuity of Heading
                if (Math.Abs(Interpolation.T.Last() - successor.T) > tolerance)
                {
                    //TrassierungLog.Logger?.LogWarning("Last interpolatedElement(ID" + id.ToString() + "_" + kz.ToString() + ") heading differs from successors start heading by " + (Interpolation.T.Last() - successor.T).ToString());
                    AddWarningCallout("heading difference \n" + (Interpolation.T.Last() - successor.T).ToString(), Interpolation.X.Last(), Interpolation.Y.Last());
                }
                //Continuity of Radii(Curvature)
                if (bCheckRadii && Math.Abs(Interpolation.K.Last() == 0 ? 0 : 1 / Interpolation.K.Last() - successor.r1) > tolerance)
                {
                    //TrassierungLog.Logger?.LogWarning("Last interpolatedElement(ID" + id.ToString() + "_" + kz.ToString() + ") radius differs from successors start radius by " + (Interpolation.K.Last() == 0 ? 0 : 1 / Interpolation.K.Last() - successor.r1).ToString());
                    AddWarningCallout("curvature difference \n" + (Interpolation.K.Last() == 0 ? 0 : 1 / Interpolation.K.Last() - successor.r1).ToString(), Interpolation.X.Last(), Interpolation.Y.Last());
                }
            }
            return true;
        }
        void AddWarningCallout(string text, double X, double Y)
        {
            WarningCallouts.Add(new GeometryWarning(text, X, Y,this));
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
            if (TrassenGeometrie == null) { 
                AddWarningCallout("No Gemetry for interpolation " + kz.ToString() + "set, maybe not implemented yet", Xstart,Ystart); 
                Interpolation = new Interpolation(0);
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
                (x_, y_, t_, k_) = TrassenGeometrie.PointAt(s_);
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
            return TrassenGeometrie.sAt(X, Y, T) + s;
        }
        /// <summary>
        /// Calculates a Point on the Geometry at a given Distance S
        /// </summary>
        /// <param name="S">S as global mileage is reduced by Stationvalue S of the element</param>
        /// <returns>Hochwert X,Rechtswert Y, Heading T</returns>
        public (double, double, double) GetPointAtS(double S)
        {
            if (TrassenGeometrie == null) return (0, 0, 0);// (Double.NaN, Double.NaN, Double.NaN);
            (double X, double Y, double T, _) = TrassenGeometrie.PointAt(S - s);
            Transform2D transform = new Transform2D(x, y, t);
            transform.Apply(ref X, ref Y, ref T);
            return (X, Y, T);
        }

        public override string ToString()
        {
            CultureInfo info = CultureInfo.CurrentCulture;
            string[] values = { r1.ToString(info),r1.ToString(info),y.ToString(info),x.ToString(info),t.ToString(info),s.ToString(info),kz.ToString(),l.ToString(info),u1.ToString(info),u2.ToString(info),c.ToString(info) };
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
            TrassenElementExt trasse = (TrassenElementExt)owner;
            string ownerString = trasse != null && trasse.owner != null ? trasse.owner.Filename + "_" + trasse.ID : null;
            TrassierungLog.Logger?.LogWarning(ownerString + " " + Text, owner) ;
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

