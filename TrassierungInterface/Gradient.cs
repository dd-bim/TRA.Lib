using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrassierungInterface
{
    public class GradientElement
    {
        /// <value>Station NW</value>
        double S;
        /// <value>Hoehe NW</value>
        double H;
        /// <value>Ausrundungsradius</value>
        double R;
        /// <value>Tangentenlaenge</value>
        double T;
        /// <value>Punktnummer</value>
        long Pkt;

        public GradientElement(double s, double h, double r, double t, long pkt)
        {
            S = s;
            H = h;
            R = r;
            T = t;
            Pkt = pkt;
        }
        public void print()
        {
            Console.WriteLine("S:" + S + " H:" + H + " R:" + R + " T:" + T + " Pkt:" + Pkt);
        }
    }
}