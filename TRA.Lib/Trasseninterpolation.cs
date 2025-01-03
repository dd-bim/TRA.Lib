using Microsoft.Extensions.Logging;
using System.Numerics;

namespace TRA_Lib
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
        /// <param name="t">if t is given, intersection is calculated in direction t starting from the given point, otherwise calculation is perpendicular to geometry</param>
        /// <returns></returns>
        public abstract double sAt(double X, double Y, double t = double.NaN);
    }
    public class Gerade : TrassenGeometrie
    {
        public override (double X, double Y, double t, double k) PointAt(double s)
        {
            return (s, 0.0, 0.0, 0.0);
        }

        public override double sAt(double X, double Y, double t = double.NaN)
        {
            if (t == 0)// Lines are parallel or identical;
            {
                return double.NaN;
            }
            else if (Double.IsNaN(t)) //t is not used
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
            if (r == 0) { return (s, 0.0, 0.0, 0.0); } //Gerade
            (double X, double Y) = Math.SinCos(s / r);
            return (X * r, sig * ((1 - Y) * r), s / r, 1 / radius);
        }

        public override double sAt(double X, double Y, double t = double.NaN)
        {
            if (radius == 0) return new Gerade().sAt(X, Y, t); //use this calculation if radius is 0;

            Vector2 c = new Vector2(0, (float)radius);
            Vector2 point = new Vector2((float)X, (float)Y);
            Vector2 dir;
            if (Double.IsNaN(t)) //t is not used
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
                return double.NaN;
            }
            // Calculate t values for intersection points
            double t1 = (-b + Math.Sqrt(discriminant)) / (2 * a);
            double t2 = (-b - Math.Sqrt(discriminant)) / (2 * a);

            // Calculate nearest intersection point
            Vector2 intersection = point + (float)(Math.Abs(t1) < Math.Abs(t2) ? t1 : t2) * dir;
            intersection = intersection - c; //relative to center
            double s_ = radius > 0 ? Math.PI - Math.Atan2(intersection.X, intersection.Y) : Math.Atan2(intersection.X, intersection.Y);
            return s_ * Math.Abs(radius);
        }
    }
    public class Klothoid : TrassenGeometrie
    {
        double r1;
        double r2;
        double length;

        /// <summary>
        /// Define Clothoid
        /// </summary>
        /// <param name="r1">First radius NaN is interpreted as zero(straight)</param>
        /// <param name="r2">Second radius NaN is interpreted as zero(straight)</param>
        /// <param name="length">Length of the Clothoid. NaN and 0 is results in stragiht line (radii have no effect)</param>
        public Klothoid(double r1, double r2, double length)
        {
            this.r1 = Double.IsNaN(r1) ? 0 : r1; //interpreting NaN radius as zero (straight line)
            this.r2 = Double.IsNaN(r2) ? 0 : r2; //interpreting NaN radius as zero (straight line)
            this.length = Double.IsNaN(length) ? 0 : length;
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
            if (Math.Sign(r1 * r2) == -1)
            {
                gamma = length != 0 ? -(curvature2 + curvature1) / length : 0;
            }
            else
            {
                gamma = length != 0 ? (curvature2 - curvature1) / length : 0;
            }
            dir = Math.Sign(r1) == 0 ? Math.Sign(r2) : Math.Sign(r1); //Get turning-direction of the clothoid
            (Sb, Cb) = CalculateFresnel(curvature1 / Math.Sqrt(Math.PI * Math.Abs(gamma)));
            // Euler Spiral
            Cs1 = Math.Sqrt(Math.PI / Math.Abs(gamma)) * Complex.Exp(new Complex(0, -Math.Sign(gamma) * (curvature1 * curvature1) / (2 * gamma)));
        }
        public override (double X, double Y, double t, double k) PointAt(double s)
        {
            if (r1 == 0.0 && r2 == 0.0) { return new Gerade().PointAt(s); }
            if (r1 == r2) { return new Kreis(r1).PointAt(s); }

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
                double sin, cos;
                (sin, cos) = Math.SinCos(Math.PI * t * t / 2);
                sumS += sin * step;
                sumC += cos * step;
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
                double sin, cos;
                (sin, cos) = Math.SinCos(Math.PI * t * t / 2);
                sumS += sin * step;
                sumC += cos * step;
            }
            return (sumS, sumC);
        }

        public override double sAt(double X, double Y, double t = double.NaN)
        {
            if (Double.IsNaN(X) || Double.IsNaN(Y)) return double.NaN;
            double threshold = 0.00001;
            double delta = 1.0;
            double X_ = 0.0;
            double Y_ = 0.0;
            double t_ = 0.0;
            double k = 0.0;
            double s = 0.0;
            double d = double.PositiveInfinity; //distance between point and normal
            Vector2 v1, v2;
            Vector2 vt = new();
            int prevSign = 0;
            if (!Double.IsNaN(t))
            {
                double x, y;
                (x, y) = Math.SinCos(t);
                vt = new Vector2((float)y, (float)x);
                prevSign = Math.Sign(X * vt.Y - Y * vt.X);
            }

            int maxIterations = 1000;
            int i = 0;
            while (i < maxIterations && d > threshold)
            {
                (X_, Y_, t_, k) = PointAt(s);
                v2 = new Vector2((float)(X - X_), (float)(Y - Y_)); //vector from current position to Point of interest
                if (Double.IsNaN(t))
                {
                    v1 = new Vector2((float)Math.Sin(t_), (float)Math.Cos(t_)); //normal at current position
                    double scalarCross = v2.X * v1.Y - v2.Y * v1.X;
                    if (Math.Sign(scalarCross) != Math.Sign(delta))
                    {
                        delta = -0.5 * delta;
                    }
                    // Compute d by cross product               
                    d = Math.Abs(scalarCross) / (float)v1.Length();
                }
                else
                {
                    double scalarCross = v2.X * vt.Y - v2.Y * vt.X;
                    if (Math.Sign(scalarCross) != prevSign)
                    {
                        delta = -0.5 * delta;
                        prevSign = Math.Sign(scalarCross);
                    }
                    double ang = Math.Acos(Vector2.Dot(vt, v2 / v2.Length()));
                    d = (ang > 0.5 * Math.PI ? Math.PI - ang : ang) * v2.Length();
                }
                s = s + delta;
                i++;
            }
            if (i == maxIterations)
            {
                TrassierungLog.Logger?.LogWarning("Could not Interpolate a valid solution on Clothoid geometry", this);
                return double.NaN;
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
