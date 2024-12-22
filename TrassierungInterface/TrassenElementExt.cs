using Microsoft.Extensions.Logging;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrassierungInterface
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
        public void Concat(Interpolation interp)
        {           
            X = X.Concat(interp.X).ToArray();
            Y = Y.Concat(interp.Y).ToArray();            
            S = S.Concat(interp.S).ToArray();
            T = T.Concat(interp.T).ToArray();
            K = K.Concat(interp.K).ToArray();

            if (interp.H != null) { if (H == null) { H = new double[interp.H.Length]; } H = H.Concat(interp.H).ToArray(); }
            if (interp.s != null){ if (s == null) { s = new double[interp.H.Length]; } s = s.Concat(interp.s).ToArray(); }
        }
    }

    public class TrassenElementExt : TrassenElement
    {
        TrassenGeometrie TrassenGeometrie;
        /// <value>ID des Elements</value>
        int id;
        /// <value>Vorgaenger Element</value>
        TrassenElementExt predecessor;
        /// <value>Nachfolger Element</value>
        TrassenElementExt successor;
        /// <value>Gradienten des Trassenelements</value>
        GradientElement[] Gradienten;
        /// <value>Gleisscheren des Trassenelements</value>
        GleisscherenElement[] Gleisscheren;
        /// <value>Interpolationsobjekt</value>
        Interpolation Interpolation;

#if USE_SCOTTPLOT
        List<WarningCallout> WarningCallouts = new() { };
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
        public TrassenElementExt Predecessor { get { return predecessor; } }
        /// <value>Hochwert am Elementanfang</value>
        public TrassenElementExt Successor { get { return successor; } }
        /// <value>Returns Interpolationresult</value>
        public Interpolation InterpolationResult { get { return Interpolation; } }
        ///// <value>X-Koordinaten der Interpolationspunkte</value>
        //public double[] InterpX { get { return Interpolation.X == null ? new double[0] : Interpolation.X; } }
        ///// <value>Y-Koordinaten der Interpolationspunkte</value>
        //public double[] InterpY { get { return Interpolation.Y == null ? new double[0] : Interpolation.Y; } }
        ///// <value>Höhe der Interpolationspunkte</value>
        //public double[] InterpH { get { return Interpolation.H == null ? new double[0] : Interpolation.H; } }
        ///// <value>Richtung der Interpolationspunkte</value>
        //public double[] InterpT { get { return Interpolation.T == null ? new double[0] : Interpolation.T; } }
        ///// <value>Krümmung der Interpolationspunkte</value>
        //public double[] InterpK { get { return Interpolation.K == null ? new double[0] : Interpolation.K; } }
        ///// <value>Steigung der Interpolationspunkte[‰]</value>
        //public double[] InterpSlope { get { return Interpolation.s == null ? new double[0] : Interpolation.s; } }
#if USE_SCOTTPLOT
        /// <value>List of Warning Callouts to show on Plot</value>
        public WarningCallout[] GetWarnings { get { return WarningCallouts.ToArray(); } }
#endif
        public TrassenElementExt(double r1, double r2, double y, double x, double t, double s, int kz, double l, double u1, double u2, float c, int idx, TrassenElementExt predecessor = null, ILogger<TrassenElementExt> logger = null)
            : base(r1,r2,y,x, t, s, kz, l, u1, u2,c)
        {
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
            if (Interpolation.X?.Length > 0 && successor != null)
            {
                //Connectivity
                if (Math.Abs(Interpolation.X.Last() - successor.x) > tolerance && Math.Abs(Interpolation.Y.Last() - successor.y) > tolerance)
                {
                    TrassierungLog.Logger?.LogWarning("Last interpolated Element(ID" + id.ToString() + "_" + kz.ToString() + ") coordinate differs from successors start coordinate by " + Math.Sqrt(Math.Pow(Interpolation.X.Last() - successor.x, 2) + Math.Pow(Interpolation.Y.Last() - successor.y, 2)).ToString());
                    AddWarningCallout("coordinate difference \n" + Math.Sqrt(Math.Pow(Interpolation.X.Last() - successor.x, 2) + Math.Pow(Interpolation.Y.Last() - successor.y, 2)).ToString(), Interpolation.X.Last(), Interpolation.Y.Last());
                }
                //Continuity of Heading
                if (Math.Abs(Interpolation.T.Last() - successor.T) > tolerance)
                {
                    TrassierungLog.Logger?.LogWarning("Last interpolatedElement(ID" + id.ToString() + "_" + kz.ToString() + ") heading differs from successors start heading by " + (Interpolation.T.Last() - successor.T).ToString());
                    AddWarningCallout("heading difference \n" + (Interpolation.T.Last() - successor.T).ToString(), Interpolation.X.Last(), Interpolation.Y.Last());
                }
                //Continuity of Radii(Curvature)
                if (bCheckRadii && Math.Abs(Interpolation.K.Last() == 0 ? 0 : 1 / Interpolation.K.Last() - successor.r1) > tolerance)
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
            WarningCallouts.Add(new WarningCallout(text, X, Y));
#endif
        }

        public ref Interpolation Interpolate(double delta = 1.0)
        {
            Transform2D transform = new Transform2D(x, y, t);
            if (TrassenGeometrie == null) { TrassierungLog.Logger?.LogWarning("No Gemetry for interpolation " + kz.ToString() + "set, maybe not implemented yet", nameof(kz)); return ref Interpolation; }

            int num = (int)Math.Abs(l / delta);
            if (l < 0 && delta > 0) { delta = -delta; } //set delta negative for negative lengths
            Interpolation = new Interpolation(num + 1);

            for (int i = 0; i <= num; i++)
            {
                Interpolation.S[i] = S + i * delta;
                (Interpolation.X[i], Interpolation.Y[i], Interpolation.T[i], Interpolation.K[i]) = TrassenGeometrie.PointAt(i < num ? i * delta : l);
                if (transform != null) { transform.Apply(ref Interpolation.X[i], ref Interpolation.Y[i], ref Interpolation.T[i]); }
            }
            PlausibilityCheck();
            return ref Interpolation;
        }
        
        public double GetSAtPoint(double X, double Y, double T = double.NaN)
        {
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
            (double X, double Y, _, _) = TrassenGeometrie.PointAt(S - s);
            Transform2D transform = new Transform2D(x, y, t);
            double T = 0.0;
            transform.Apply(ref X, ref Y, ref T);
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
            TextCoordinates = new Coordinates(Y + 10, (X != Xlast ? X : X = X - 5) + 10);
            Xlast = X;
            ArrowLineColor = LineColor;
            ArrowFillColor = color;
            TextBorderColor = LineColor;
            TextBackgroundColor = color.Lighten();
        }
    }
#endif
}

