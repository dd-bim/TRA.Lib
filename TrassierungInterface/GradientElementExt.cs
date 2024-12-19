using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrassierungInterface
{
    public class GradientElementExt : GradientElement
    {
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
        /// <value>Längsneigung (vom predecessor kommend)</value>
        double s1;
        /// <value>Längsneigung (zum successor)</value>
        double s2;
        /// <value>Auf die Horizontale reduzierte Tangentenlänge</value>
        double T_;

        //Constants
        /// <value>Höhe am Ausrundungsanfang</value>
        double h_A;
        /// <value>Stationswert am Ausrundungsanfang</value>
        double x_A;
        /// <value>Stationswert am Ausrundungsende</value>
        double x_E;

        //public
        /// <value>Station am NW</value>
        public double S { get { return s; } }
        /// <value>Höhe am NW</value>
        public double H { get { return h; } }
        /// <value>Punktnummer am NW</value>
        public double Pkt { get { return pkt; } }
        /// <value>Vorgaenger Element</value>
        public GradientElementExt Predecessor { get { return predecessor; } }
        /// <value>Hochwert am Elementanfang</value>
        public GradientElementExt Successor { get { return successor; } }

        public GradientElementExt(double s, double h, double r, double t, long pkt, int idx, GradientElementExt predecessor = null) : base(s, h, r, t, pkt)
        {
            id = idx;
            if (predecessor != null)
            {
                this.predecessor = predecessor;
                predecessor.successor = this;
                s1 = (base.h - predecessor.h) / (S - predecessor.S) * 100;
                predecessor.s2 = s1;
                predecessor.CalcConstants();
            }
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
            h_A = h - t * (s1 / 100);
            x_A = S - t;
            x_E = S + t;
        }
        
        public double GetHAtS(double s)
        {
            if (s < S)
            {
                if (s <= x_A) { return h_A + s1 * (S - s) / 100; }
                return h_A + (s1 / 100) * (s - S) + (r > 0?Math.Pow(s - S, 2) / (2 * r):0);
            }
            else
            {
                if (s >= x_E) { return h_A + s2 * (s - S) / 100; }
                return h_A + (s2 / 100) * (S - s) + (r != 0 ? Math.Pow(S-s, 2) / (2 * r) : 0);
            }
        }
    }
}
