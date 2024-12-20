
namespace TrassierungInterface
{
    /// <summary>
    /// Base Datastructure for a GRA-File
    /// </summary>
    public class GradientElement
    {
        /// <value>Station NW</value>
        protected double s;
        /// <value>Hoehe NW</value>
        protected double h;
        /// <value>Ausrundungsradius</value>
        protected double r;
        /// <value>Tangentenlaenge</value>
        protected double t;
        /// <value>Punktnummer</value>
        protected int pkt;

        public GradientElement(double s, double h, double r, double t, long pkt)
        {
            this.s = s;
            this.h = h;
            this.r = r;
            this.t = t;
            this.pkt = (int)(pkt / 1000);
        }
        public void print()
        {
            Console.WriteLine("S:" + s + " H:" + h + " R:" + r + " T:" + t + " Pkt:" + pkt);
        }
    }
}