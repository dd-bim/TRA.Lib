

namespace TRA_Lib
{
    /// <summary>
    /// https://www.akgsoftware.de/docs/de/infravision/b62/akgcad/webhelp/glossary/gl_gleisschere.html
    /// </summary>
    public class GleisscherenElement
    {
        /// <value>Station RE1</value>
        double RE1;
        /// <value>Station RA</value>
        double RA;
        /// <value>Station RE2</value>
        double RE2;
        /// <value>Ueberhoeung1</value>
        double Ueberhoeung1;
        /// <value>Ueberhoeung2</value>
        long Ueberhoeung2;
        /// <value>Gleisscherenkennzeichen</value>
        Trassenkennzeichen Kz;

        public GleisscherenElement(double re1, double ra, double re2, double ueberhoeung1, long ueberhoeung2)
        {
            RE1 = re1;
            RA = ra;
            RE2 = re2;
            Ueberhoeung2 = ueberhoeung2 * 10;
            if (3000 <= ueberhoeung1 && ueberhoeung1 <= 3999) { Kz = Trassenkennzeichen.UB_S_Form; }
            else if (7000 <= ueberhoeung1 && ueberhoeung1 <= 7999) { Kz = Trassenkennzeichen.S_Form_1f; }
            else if (4000 <= ueberhoeung1 && ueberhoeung1 <= 4999) { Kz = Trassenkennzeichen.Bloss; }
            else if (8000 <= ueberhoeung1 && ueberhoeung1 <= 8999) { Kz = Trassenkennzeichen.Bloss_1f; }
            else { Kz = Trassenkennzeichen.Gerade; }
            Ueberhoeung1 = ueberhoeung1 - (int)Kz * 1000;
        }
        public void print()
        {
            Console.WriteLine("RE1:" + RE1 + " RA:" + RA + " RE2:" + RE2 + " Ueberhoeung1:" + Ueberhoeung1 + " Ueberhoeung2:" + Ueberhoeung2 + " Kz:" + Kz);
        }
    }
}
