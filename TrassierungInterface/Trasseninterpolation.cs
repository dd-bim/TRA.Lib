using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TrassierungInterface
{

    public class InterpolationClass
    {
        private static readonly double sqrt2PI = Math.Sqrt(2 * Math.PI);
        //Fresnel storage
        private static double last_t;
        private static double last_Sa;
        private static double last_Ca;

        /// <summary>
        /// calculates a point along a Line
        /// </summary>
        /// <param name="dist"></param>
        /// <returns></returns>
        public static (double X, double Y, double Z) PointOnLine(double dist)
        {
            return (dist,0.0, 0.0);
        }
        /// <summary>
        /// calculates a point on a circle
        /// </summary>
        /// <param name="dist"></param>
        /// <param name="radius">positive values results in a right turned curve, negative values in left turn</param>
        /// <returns></returns>
        public static (double X, double Y, double Z) PointOnCircle(double dist, double radius)
        {
            int sig = Math.Sign(radius);
            radius = Math.Abs(radius);
            (double X, double Y) = Math.SinCos(dist / radius);
            return (X * radius, sig*((1-Y) * radius), 0.0);
        }
        /// <summary>
        /// calculates a point on a circle
        /// </summary>
        /// <param name="dist"></param>
        /// <param name="radius">positive values results in a right turned curve, negative values in left turn</param>
        /// <returns></returns>
        public static (double X, double Y, double Z) PointOnClothoid(double dist, double radius1, double radius2, double length)
        {
            // Addapted from https://github.com/stefan-urban/pyeulerspiral/blob/master/eulerspiral/eulerspiral.py
            // original Source: https://www.cs.bgu.ac.il/~ben-shahar/ftp/papers/Edge_Completion/2003:Kimia_Frankel_and_Popescu:Euler_Spiral_for_Shape_Completion.pdf Page 165 Eq(6)
            // maybe Helpfull https://math.stackexchange.com/questions/1785816/calculating-coordinates-along-a-clothoid-betwen-2-curves
            // https://www.researchgate.net/publication/292669884_The_Clothoid_Computation_A_Simple_and_Efficient_Numerical_Algorithm
            if (radius1 == 0.0 && radius2 == 0.0)
            {
                //Straight Line
                return PointOnLine(dist);
            }
            else if (radius1 == radius2)
            {
                //Circle
                return PointOnCircle(dist, radius1);
            }
            else
            {
                double curvature1 = radius1 == 0.0 ? 0 : 1 / radius1;
                double curvature2 = radius2 == 0.0 ? 0 : 1 / radius2;
                double gamma = (curvature2 - curvature1) / length;
                
                // Fresnel integrals
                double Ca = new double();
                double Sa = new double();
                double test = curvature1 / Math.Sqrt(Math.PI * Math.Abs(gamma));

                (Sa,Ca) = CalculateFresnel((curvature1 + gamma * dist) / Math.Sqrt(Math.PI * Math.Abs(gamma)),ref last_t,ref last_Sa,ref last_Ca);
                double Cb = new double();
                double Sb = new double();
                (Sb, Cb) = CalculateFresnel(curvature1 / Math.Sqrt(Math.PI * Math.Abs(gamma)));

                // Euler Spiral
                Complex Cs1 = Math.Sqrt(Math.PI / Math.Abs(gamma)) * Complex.Exp(new Complex(0,Math.Pow(curvature1, 2) / 2 / gamma));
                //Complex Cs2 = Math.Sign(gamma) * (Ca - Cb) + new Complex(0, Sa) - new Complex(0, Sb);
                Complex Cs2 = Math.Sign(gamma) * ((Ca - Cb) + new Complex(0, Sa - Sb));
                Complex Cs = Cs1 * Cs2;

                //Tangent at each point
                double theta = gamma * Math.Pow(dist, 2) / 2 + curvature1 * dist;// - 0.5*Math.PI;
                double X = Cs.Real;
                double Y = Cs.Imaginary;
                //return (X * Math.Cos(test) - Y * Math.Sin(test), X * Math.Sin(test) + Y * Math.Cos(test),0.0);
                //return (Ca - Cb, Sa - Sb, 0.0);
                //Debug.WriteLine("Klotoid: " + Cs.Imaginary + " " + Cs.Real);
                return (Cs.Real, Math.Sign(gamma) * Cs.Imaginary, 0.0);
            }

           
        }
        // Implementing the Fresnel integrals using numerical integration
        public static (double S, double C) CalculateFresnel(double x)
        {
            int n = 100000; // Number of steps for the integration
            double step = x / n;
            double sumS = 0.0;
            double sumC = 0.0;
            for (int i = 1; i <= n; i++)
            {
                double t = i * step;
                sumS += Math.Sin(Math.PI * t * t / 2) * step;
                sumC += Math.Cos(Math.PI * t * t / 2) * step;
            }
            return (sumS, sumC);
        }
        public static (double S, double C) CalculateFresnel(double x,ref double t,ref double sumS, ref double sumC)
        {
            int n = 100000; // Number of steps for the integration
            if (t > x) { 
                t = 0.0;
                sumS = 0.0;
                sumC = 0.0;
            } //reset
            double step = (x-t) / n;
            double t_0 = t;
            for (int i = 1; i <= n; i++)
            {
                t = t_0 + i * step;
                sumS += Math.Sin(Math.PI * t * t / 2) * step;
                sumC += Math.Cos(Math.PI * t * t / 2) * step;
            }
            return (sumS, sumC);
        }
    }
   

    internal class Transform2D
    {
        double dx;
        double dy;
        double dt;

        public Transform2D(double dx, double dy, double dt)
        {
            this.dx = dx;
            this.dy = dy;
            this.dt = dt;
        }
        public void Apply(ref double X, ref double Y)
        {
            double x_ = X * Math.Cos(dt) - Y * Math.Sin(dt);
            double y_ = X * Math.Sin(dt) + Y * Math.Cos(dt);
            X = x_ + dx;
            Y = y_ + dy;
        }
    }
}
