﻿

using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

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
    }

    public class Trasse
    {
        public TrassenElement[] Elemente;

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

        /// public
        ///<value>ID des Elements innerhalb der Trasse</value>
        public double ID { get { return id; } }
        /// <value>Rechtswert am Elementanfang</value>
        public double Ystart { get { return y; } }
        /// <value>Hochwert am Elementanfang</value>
        public double Xstart { get { return x; } }
        /// <value>Rechtswert am Elementende</value>
        public double Yend { get { return successor.y; } }
        /// <value>Hochwert am Elementende</value>
        public double Xend { get { return successor.x; } }
        /// <value>Richtung am Elementanfang</value>
        public double T { get { return t; } }
        public int Kz { get { return (int)kz; } }
        /// <value>Vorgaenger Element</value>
        public TrassenElement Predecessor { get { return predecessor; } }
        /// <value>Hochwert am Elementanfang</value>
        public TrassenElement Successor { get { return successor; } }
        /// <value>X-Koordinaten der Interpolationspunkte</value>
        public double[] InterpX { get { return Interpolation.X == null? new double[0]: Interpolation.X; } }
            /// <value>Y-Koordinaten der Interpolationspunkte</value>
        public double[] InterpY {get { return Interpolation.Y == null ? new double[0] : Interpolation.Y; } }

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
        bool PlausibilityCheck()
        {
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
                }
                //Continuity
                if (Math.Abs(Interpolation.T.Last() - successor.T) > tolerance)
                {
                    TrassierungLog.Logger?.LogWarning("Last interpolatedElement(ID" + id.ToString() + "_" + kz.ToString() + ") heading differs from successors start heading by " + (Interpolation.T.Last() - successor.T).ToString());
                }
            }
                return true;
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

            for (int i = 0; i <= num; i++)
            {
                Interpolation.S[i] = i * delta;
                (Interpolation.X[i], Interpolation.Y[i], Interpolation.T[i]) = TrassenGeometrie.PointAt(i < num ? i * delta : l);
                if (transform != null) { transform.Apply(ref Interpolation.X[i], ref Interpolation.Y[i], ref Interpolation.T[i]); }
            }
            PlausibilityCheck();
        }


        public void print()
        {
            Console.WriteLine("R1:" + r1 + " R2:" + r2 + " Y:" + y + " X:" + x + " T:" + t + " S:" + s + " Kz:" + kz.ToString() + " L:" + l + " U1:" + u1 + " U2:" + u2 + " C:" + c);
        }
    }
}
