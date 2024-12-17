using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TrassierungInterface
{
    public abstract class TrassenGeometrie
    {
        /// <summary>
        /// Calculates local Point on Geometry
        /// </summary>
        /// <param name="s">Distance along Geometry</param>
        /// <returns>local X-Coordinate, local Y-Coordinate, local heading</returns>
        public abstract (double X, double Y, double t, double k) PointAt(double s);
        /// <summary>
        /// Calculates local distance s on Geometry to given Point
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="t">if t is given, intersection is calculated in direction t starting from the given point, otherwise calculation is perpendicular to geometry,</param>
        /// <returns></returns>
        public abstract double sAt(double X, double Y, double t = double.PositiveInfinity);
    }
    public class Gerade : TrassenGeometrie
    {
        public override (double X, double Y, double t, double k) PointAt(double s)
        {
            return (s, 0.0, 0.0, 0.0);
        }

        public override double sAt(double X, double Y, double t = double.PositiveInfinity)
        {
            if (t == 0)// Lines are parallel or identical;
            {
                return double.NegativeInfinity;
            }
            else if (t == double.PositiveInfinity) //t is not used
            {
                return X;
            }

            Vector2 d1 = new Vector2(1, 0);
            Vector2 p2 = new Vector2((float)X, (float)Y);
            Vector2 d2 = new Vector2((float)Math.Cos(t), (float)Math.Sin(t));

            // Solving for t where p1 + t * d1 intersects p2 + u * d2
            double determinant = d1.X * d2.Y - d1.Y * d2.X;
            if (Math.Abs(determinant) < 1e-10) // Lines are parallel or identical; no single intersection point
            {
                return double.NegativeInfinity;
            }
            double t_ = (p2.X * d2.Y - p2.Y * d2.X) / determinant;
            //Vector2 intersection = d1 * (float)t_; //Not nesseccary as d1 is (1,0)
            return t_;
        }
    }
    public class Kreis : TrassenGeometrie
    {
        double radius;

        public Kreis(double radius)
        {
            this.radius = radius;     
        }
        public override (double X, double Y, double t, double k) PointAt(double s)
        {
            int sig = Math.Sign(radius);
            double r = Math.Abs(radius);
            (double X, double Y) = Math.SinCos(s / r);
            return (X * r, sig * ((1 - Y) * r), s/r, 1/radius);
        }

        public override double sAt(double X, double Y, double t = double.PositiveInfinity)
        {
            Vector2 c = new Vector2((float)radius, 0);
            Vector2 point = new Vector2((float)Y,(float)X);
            Vector2 dir;
            if (t == double.PositiveInfinity) //t is not used
            {
                dir = point - c;
            }
            else
            {
                dir = new Vector2((float)Math.Cos(t), (float)Math.Sin(t));
            }
            
            // Calculate quadratic equation coefficients
            double a = dir.X * dir.X + dir.Y * dir.Y;
            double b = 2 * (dir.X * (X - c.X) + dir.Y * (Y - c.Y)); 
            double cValue = (X - c.X) * (X - c.X) + (Y - c.Y) * (Y - c.Y) - radius * radius; 
            
            // Calculate discriminant
            double discriminant = b * b - 4 * a * cValue; 
            if (discriminant < 0) // No real intersection
            {
                return double.NegativeInfinity; 
            }           
            // Calculate t values for intersection points
            double t1 = (-b + Math.Sqrt(discriminant)) / (2 * a); 
            double t2 = (-b - Math.Sqrt(discriminant)) / (2 * a); 
            
            // Calculate nearest intersection point
            Vector2 intersection = point + (float)(Math.Abs(t1) < Math.Abs(t2) ? t1:t2) * dir;
            intersection = intersection - c; //relative to center
            double s_ = radius > 0 ? Math.PI - Math.Atan2(intersection.Y, intersection.X) : Math.Atan2(intersection.Y, intersection.X);
            return s_ * Math.Abs(radius);
        }
    }
    public class Klothoid : TrassenGeometrie
    {
        double r1;
        double r2;
        double length;

        public Klothoid(double r1, double r2, double length)
        {
           this.r1 = r1;
           this.r2 = r2;
           this.length = length;
           CalcConstants();
        }

        //Constant parameters
        double curvature1;
        double curvature2;
        double gamma;
        double Cb;
        double Sb;
        Complex Cs1;
        int dir; //1 ==right turn , -1 == left turn

        //Intermediate results storage
        double last_t;
        double last_Sa;
        double last_Ca;

        void CalcConstants()
        {
            // using absolute values is not the elegant way but, simplfies calculations for sign-combinations of the radii
            curvature1 = r1 == 0.0 ? 0 : Math.Abs(1 / r1);
            curvature2 = r2 == 0.0 ? 0 : Math.Abs(1 / r2);
            if (Math.Sign(r1*r2) == -1)
            {
                gamma = -(curvature2 + curvature1) / length;
            }
            else
            {
                gamma = (curvature2 - curvature1) / length;
            }
            dir = Math.Sign(r1) == 0 ? Math.Sign(r2) : Math.Sign(r1); //Get turning-direction of the clothoid
            (Sb, Cb) = CalculateFresnel(curvature1 / Math.Sqrt(Math.PI * Math.Abs(gamma)));
            // Euler Spiral
            Cs1 = Math.Sqrt(Math.PI / Math.Abs(gamma)) * Complex.Exp(new Complex(0, -Math.Sign(gamma)*(curvature1 * curvature1) / (2 * gamma)));
        }
        public override (double X, double Y, double t, double k) PointAt(double s)
        {
            // Addapted from https://github.com/stefan-urban/pyeulerspiral/blob/master/eulerspiral/eulerspiral.py
            // original Source: https://www.cs.bgu.ac.il/~ben-shahar/ftp/papers/Edge_Completion/2003:Kimia_Frankel_and_Popescu:Euler_Spiral_for_Shape_Completion.pdf Page 165 Eq(6)

            // Fresnel integrals
            double Ca = new double();
            double Sa = new double();
            (Sa, Ca) = CalculateFresnel((curvature1 + gamma * s) / Math.Sqrt(Math.PI * Math.Abs(gamma)), ref last_t, ref last_Sa, ref last_Ca);
                        
            Complex Cs2 = Math.Sign(gamma) * ((Ca - Cb) + new Complex(0, Sa - Sb));
            Complex Cs = Cs1 * Cs2;

            //Tangent at point
            double theta = gamma * Math.Pow(s, 2) / 2 + curvature1 * s;
            return (Cs.Real, dir * Math.Sign(gamma) * Cs.Imaginary, theta, curvature1 + gamma * s);
        }

        // Implementing the Fresnel integrals using numerical integration
        static (double S, double C) CalculateFresnel(double x)
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
        static (double S, double C) CalculateFresnel(double x, ref double t, ref double sumS, ref double sumC)
        {
            int n = 100000; // Number of steps for the integration
            if (t > x)
            {
                t = 0.0;
                sumS = 0.0;
                sumC = 0.0;
            } //reset
            double step = (x - t) / n;
            double t_0 = t;
            for (int i = 1; i <= n; i++)
            {
                t = t_0 + i * step;
                sumS += Math.Sin(Math.PI * t * t / 2) * step;
                sumC += Math.Cos(Math.PI * t * t / 2) * step;
            }
            return (sumS, sumC);
        }

        public override double sAt(double X, double Y, double t = double.PositiveInfinity)
        {
            double threshold = 0.001;
            double delta = 1.0;
            double X_ = 0.0;
            double Y_ = 0.0;
            double t_ = 0.0;
            double k = 0.0;
            double s = 0.0;
            double d = double.PositiveInfinity; //distance between point and normal
            Vector2 v1, v2;
            int maxIterations = 1000;
            int i = 0;
            while (i < maxIterations && d > threshold)
            {
                (X_, Y_, t_, k) = PointAt(s);
                v1 = new Vector2((float)Math.Sin(t_), (float)Math.Cos(t_));
                v2 = new Vector2((float)(X - X_), (float)(Y - Y_));
                //v2 = new Vector2((float)(Y - Y_), (float)(X - X_));
                double scalarCross = v2.X * v1.Y - v2.Y * v1.X;
                if (Math.Sign(scalarCross) != Math.Sign(delta))
                //if (Math.Sign(Vector2.Dot(v2, v1)) != Math.Sign(delta))
                {
                    delta = -0.5 * delta;
                }
                // Compute d by cross product
                
                d = Math.Abs(scalarCross) / (float)v1.Length();
                s = s + delta;
                i++;
            }
            return s;
        }
    }
   

    public class Transform2D
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
        public void Apply(ref double X, ref double Y, ref double T)
        {
            double x_ = X * Math.Cos(dt) - Y * Math.Sin(dt);
            double y_ = X * Math.Sin(dt) + Y * Math.Cos(dt);
            X = x_ + dx;
            Y = y_ + dy;
            T = T + dt;
        }
        public void ApplyInverse(ref double X, ref double Y, ref double T)
        {
            double x_ = X - dx;
            double y_ = Y - dy;
            X = x_ * Math.Cos(-dt) - y_ * Math.Sin(-dt);
            Y = x_ * Math.Sin(-dt) + y_ * Math.Cos(-dt);
            T = T - dt;
        }
    }
}
