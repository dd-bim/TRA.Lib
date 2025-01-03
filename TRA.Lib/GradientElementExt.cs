using Microsoft.Extensions.Logging;


namespace TRA_Lib
{
    /// <summary>
    /// Extended GradientElement providing functionality for calculating heights defined by the base <seealso cref="GradientElement"/>
    /// </summary>
    /// <remarks> For caluclations see Gruber, Franz Josef ; Joeckel, Rainer ; Austen, Gerrit: Formelsammlung für das Vermessungswesen. Berlin Heidelberg New York: Springer-Verlag, 2014 S.152.</remarks>

    public class GradientElementExt : GradientElement
    {
        Trasse owner;
        /// <value>ID des Elements</value>
        int id;
        /// <value>Vorgaenger Element</value>
        GradientElementExt predecessor;
        /// <value>Nachfolger Element</value>
        GradientElementExt successor;
        /// <value>Hochwert</value>
        public double X = double.NaN;
        /// <value>Rechtswert</value>
        public double Y = double.NaN;
        /// <value>Längsneigung[‰] (vom predecessor kommend)</value>
        double s1;
        /// <value>Längsneigung[‰] (zum successor)</value>
        double s2;

        //Constants
        /// <value>Höhe am Ausrundungsanfang</value>
        double h_A;
        /// <value>Stationswert am Ausrundungsanfang</value>
        double x_A;
        /// <value>Höhe am Ausrundungsende</value>
        double h_E;
        /// <value>Stationswert am Ausrundungsende</value>
        double x_E;

        //public
        /// <value>Station am NW</value>
        public double S { get { return s; } }
        /// <value>Höhe am NW</value>
        public double H { get { return h; } }
        /// <value>Punktnummer am NW</value>
        public double Pkt { get { return pkt; } }
        /// <value>ID des Elements</value>
        public double ID { get { return id; } }
        /// <value>Vorgaenger Element</value>
        public GradientElementExt Predecessor { get { return predecessor; } }
        /// <value>Hochwert am Elementanfang</value>
        public GradientElementExt Successor { get { return successor; } }

        public GradientElementExt(double s, double h, double r, double t, long pkt, int idx, Trasse owner, GradientElementExt predecessor = null) : base(s, h, r, t, pkt)
        {
            id = idx;
            if (predecessor != null)
            {
                this.predecessor = predecessor;
                predecessor.successor = this;
                s1 = (h - predecessor.h) / (S - predecessor.S) * 1000;
                predecessor.s2 = s1;
                predecessor.CalcConstants();
            }
            this.owner = owner;
        }
        
        public bool PlausibilityCheck(bool bCheckRadii = false)
        {
            double tolerance = 0.00000001;
            //Tangent length
            double T_ = (Math.Abs(predecessor.r) / 100) * ((predecessor.s2 - predecessor.s1) / 2);
            if (Math.Abs(t - T_) > tolerance) { TrassierungLog.Logger?.LogWarning("Calculated TangentLength differs from provided one in file"); }
            return true;
        }
        void CalcConstants()
        {
            h_A = h - t * (s1 / 1000);
            x_A = S - t;
            h_E = h + t * (s2 / 1000);
            x_E = S + t;
        }

        /// <summary>
        /// Calculates Heigth at mileage s
        /// </summary>
        /// <param name="s"></param>
        /// <returns>Height and Slope at s</returns>
        public (double, double) GetHAtS(double s)
        {
            if (s < S)
            {
                if (s <= x_A)
                {
                    return (h_A + s1 * (s - x_A) / 1000, s1); //s is before start of "Ausrundung"
                }
                else
                {
                    return (h_A + (s1 / 1000) * (s - x_A) + (r != 0 ? Math.Pow(s - x_A, 2) / (2 * r) : 0),
                        r != 0 ? ((s - x_A + (s1 * r) / 1000) / r) * 1000 : s1);
                }
            }
            else
            {
                if (s >= x_E)
                {
                    return (h_E + s2 * (s - x_E) / 1000, s2); //s is behind end of "Ausrundung"
                }
                return (h_E + (s2 / 1000) * (s - x_E) + (r != 0 ? Math.Pow(s - x_E, 2) / (2 * r) : 0),
                    r != 0 ? ((s - x_A + (s1 * r) / 1000) / r) * 1000 : s2);
            }
        }
    }
}
