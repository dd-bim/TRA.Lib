

namespace TRA_Lib
{
    public enum Trassenkennzeichen
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
    public class TrassenElement
    {
        ///TRA-Attribute
        /// <value>Radius am Elementanfang</value>
        protected double r1;
        /// <value>Radius am Elementende</value>
        protected double r2;
        /// <value>Rechtswert am Elementanfang</value>
        protected double y;
        /// <value>Hochwert am Elementanfang</value>
        protected double x;
        /// <value>Richtung am Elementanfang</value>
        protected double t;
        /// <value>Station am Elementanfang</value>
        protected double s;
        /// <value>Kennzeichen des Elements</value>
        protected Trassenkennzeichen kz;
        /// <value>Laenge des ELements</value>
        protected double l;
        /// <value>Ueberhoehung am Elementanfang</value>
        protected double u1;
        /// <value>Ueberhoehung am Elementende</value>
        protected double u2;
        /// <value>Abstand zur Trasse, i.d.R. 0 nur für Parallelübergangsbögen</value>
        protected float c;

        public TrassenElement(double r1, double r2, double y, double x, double t, double s, int kz, double l, double u1, double u2, float c)
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
        }
    }
}
