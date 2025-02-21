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

        internal double length;
        internal double r1;
        internal double r2;
        public void updateParameters(double length, double r1, double r2)
        {
            this.length = length;
            this.r1 = r1;
            this.r2 = r2;
        }
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

        public Kreis(double radius)
        {
            this.r1 = radius;
        }
        public override (double X, double Y, double t, double k) PointAt(double s)
        {
            int sig = Math.Sign(r1);
            double r = Math.Abs(r1);
            if (r == 0) { return (s, 0.0, 0.0, 0.0); } //Gerade
            (double X, double Y) = Math.SinCos(s / r);
            return (X * r, sig * ((1 - Y) * r), s / r1, 1 / r1);
        }

        public override double sAt(double X, double Y, double t = double.NaN)
        {
            if (r1 == 0) return new Gerade().sAt(X, Y, t); //use this calculation if radius is 0;

            Vector2 c = new Vector2(0, (float)r1);
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
            double cValue = (X - c.X) * (X - c.X) + (Y - c.Y) * (Y - c.Y) - r1 * r1;

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
            double s_ = r1 > 0 ? Math.PI - Math.Atan2(intersection.X, intersection.Y) : Math.Atan2(intersection.X, intersection.Y);
            return s_ * Math.Abs(r1);
        }
    }
    public class Klothoid : TrassenGeometrie
    {
        /// <summary>
        /// Define Clothoid
        /// </summary>
        /// <param name="r1">First radius NaN is interpreted as zero(straight)</param>
        /// <param name="r2">Second radius NaN is interpreted as zero(straight)</param>
        /// <param name="length">Length of the Clothoid. NaN and 0 is results in straight line (radii have no effect)</param>
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
            double theta = gamma * s * s * 0.5 + curvature1 * s;
            return (Cs.Real, dir * Math.Sign(gamma) * Cs.Imaginary, dir * theta, dir* (curvature1 + gamma * s));
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
            v1 = new Vector2();
            if (!Double.IsNaN(t))
            {
                double x, y;
                (x, y) = Math.SinCos(t);
                v1 = new Vector2((float)y, (float)x);
                delta = Math.Sign(X * v1.Y - Y * v1.X)*delta;
            }

            int maxIterations = 1000;
            int i = 0;
            while (i < maxIterations && d > threshold)
            {
                (X_, Y_, t_, k) = PointAt(s);
                v2 = new Vector2((float)(X - X_), (float)(Y - Y_)); //vector from current position to Point of interest
                if (Double.IsNaN(t))
                {
                    v1 = new Vector2((float)Math.Cos(t_), (float)Math.Sin(t_)); //normal at current position
                    double vectorDot = Vector2.Dot(v1,v2) / v2.Length();
                    if (Math.Sign(vectorDot) != Math.Sign(delta))
                    {
                        delta = -0.5 * delta;
                    }
                    d = Math.Abs(vectorDot);
                }
                else
                {
                    double scalarCross = v2.X * v1.Y - v2.Y * v1.X;
                    double vectorDot = 1 - (Vector2.Dot(v2, v1) / v2.Length()); //Result is reached when both vectors are parallel
                    if (Math.Sign(scalarCross) != Math.Sign(delta))
                    {
                        delta = -0.5 * delta;
                    }
                    d = Math.Abs(vectorDot);
                }
                s = s + delta;
                i++;
            }
            if (i == maxIterations)
            {
                TrassierungLog.Logger?.LogWarning("Could not Interpolate a valid solution on Clothoid geometry" + d, this);
                return double.NaN;
            }
            return s;
        }
    }

    public class Bloss : TrassenGeometrie
    {

        /// <summary>
        /// Define Bloss
        /// </summary>
        /// <param name="r1">radius at the begin of Bloss. radius NaN is interpreted as zero(straight)</param>
        /// <param name="r2">radius at the end of Bloss. radius NaN is interpreted as zero(straight)</param>
        /// <param name="length">Length of the Bloss. NaN and 0 results in straight line (radii have no effect)</param>
        public Bloss(double r1, double r2, double length)
        {
            this.r1 = Double.IsNaN(r1) ? 0 : r1; //interpreting NaN radius as zero (straight line)
            this.r2 = Double.IsNaN(r2) ? 0 : r2; //interpreting NaN radius as zero (straight line)
            this.length = Double.IsNaN(length) ? 0 : length;
            CalcConstants();
        }

        //Constant parameters
        double radius;
        double curvature1, curvature2;
        int dir; //1 ==right turn , -1 == left turn

        //Intermediate results storage
        double last_t;
        double last_Sa;
        double last_Ca;

        void CalcConstants()
        {
            // using absolute values is not the elegant way but, simplfies calculations
            radius = Math.Abs(r2);
            curvature1 = r1 == 0.0 ? 0 : Math.Abs(1 / r1);
            curvature2 = r2 == 0.0 ? 0 : Math.Abs(1 / r2);
            dir = Math.Sign(r2); //Get turning-direction of the Bloss
        }
        public override (double X, double Y, double t, double k) PointAt(double s)
        {
            if (r2 == 0.0) { return new Gerade().PointAt(s); }

            // Fresnel integrals
            double Ca = new double();
            double Sa = new double();
        //https://pwayblog.com/2017/05/15/bloss-rectangular-coordinates/
        //(Sa, Ca) = CalculateFresnel(s, ref last_t, ref last_Sa, ref last_Ca);
        https://dgk.badw.de/fileadmin/user_upload/Files/DGK/docs/b-314.pdf

            //https://mediatum.ub.tum.de/doc/1295100/document.pdf:
            //Berechnung mit Polynom Formel (3.29):
            //Ca = s;
            //Sa = curvature2 * (Math.Pow(s, 4) / (4 * Math.Pow(length, 2)) - Math.Pow(s, 5) / (10 * Math.Pow(length, 3))); //https://pwayblog.com/2015/02/22/bloss-like-a-boss/ gleichung 1
            //Berechnung mit FresnelIntegral Formel (3.33):
            //(Sa, Ca) = CalculateFresnel(s, ref last_t, ref last_Sa, ref last_Ca);
            //Berechnung mit Taylorreihe Formal (3.36):
            double k = 0;
            double t = 0;
            if (r1 != 0)
            {
                double x0, y0, t0;
                t0 = TangentAtS(length, Math.Abs(r1));
                (x0, y0) = CalculateTaylor(length, Math.Abs(r1));
                Transform2D transform0 = new Transform2D(x0, -y0, t0);
                double x_, y_, t_;
                t_ = TangentAtS(length - s, Math.Abs(r1));
                (x_, y_) = CalculateTaylor(length - s, Math.Abs(r1));
                Transform2D transform_ = new Transform2D(x_, -y_, t_);
                (Sa, Ca) = CalculateTaylor(s);
                t = TangentAtS(s);
                Ca = 0;
                double t_out = t-(-t0 + t_); //TODO Test with additional datasets (differnet left/right curvature)
                transform_.Apply(ref Sa, ref Ca, ref t);
                transform0.ApplyInverse(ref Sa, ref Ca, ref t);
                t = t_out;
                //TODO Curvature Fehler in file:///C:/HTW/Trassierung/Infos/Charakterisierung%20von%20Einzelfehlern%20im%20Eisenbahnoberbau%20aus%20Messfahrten.pdf Formel 2.5 - Addition von k_anf fehlt (vgl. 2.4)?!?
                k = (1 / Math.Abs(r1))+(1 / Math.Abs(r2) - 1 / Math.Abs(r1)) * (3 * Math.Pow(s / length, 2) - 2 * Math.Pow(s / length, 3));
            }
            else
            {
                (Sa, Ca) = CalculateTaylor(s);
                t = TangentAtS(s);
                k = (3 * Math.Pow(s, 2)) / (radius * Math.Pow(length, 2)) -  (2 * Math.Pow(s, 3)) / (radius * Math.Pow(length, 3));
            }
            return (Ca, Sa * dir, t*dir, k*dir);
        }
        double TangentAtS(double s, double r = double.NaN)
        {
            if(s == 0) return 0;
            r = double.IsNaN(r) ? r = radius : r;
            return Math.Pow(s, 3) / (r * Math.Pow(length, 2))
                    - Math.Pow(s, 4) / (2 * r * Math.Pow(length, 3));
        }

        int binomialCoeffizient(int n, int k)
        {
            if (k > n - k)
                k = n - k;
            int res = 1;
            for (int i = 0; i < k; ++i)
            {
                res *= (n - i);
                res /= (i + 1);
            }
            return res;
        }
        long fakultaet(int n)
        {
            if (n == 0)
                return 1;
            return n * fakultaet(n - 1);
        }

        (double S, double C) CalculateTaylor(double x, double r = double.NaN)
        {
            if (x == 0) return (0.0, 0.0);
            int n = 4; // Number of steps for the integration
            double k, t, Fi;
            double S = 0;
            double C = 0;
            double R = double.IsNaN(r) ? radius : r;
            for (int i = 0; i <= n; i++)
            {
                int m = i * 2;
                double am = 0;
                for (int j = 0; j <= m; j++)
                {
                    am += binomialCoeffizient(m, j) * Math.Pow(1 / (R * length * length), m - j) * Math.Pow(-1 / (2 * R * length * length * length), j) * (Math.Pow(x, j + 1) / (3 * m + j + 1));
                }
                C += Math.Pow(-1, m / 2) / fakultaet(m) * am * Math.Pow(x, 3 * m);
                m = m + 1;
                am = 0;
                for (int j = 0; j <= m; j++)
                {
                    am += binomialCoeffizient(m, j) * Math.Pow(1 / (R * length * length), m - j) * Math.Pow(-1 / (2 * R * length * length * length), j) * (Math.Pow(x, j + 1) / (3 * m + j + 1));
                }
                S += Math.Pow(-1, (m - 1) / 2) / fakultaet(m) * am * Math.Pow(x, 3 * m);
            }
            return (S, C);
        }
        (double S, double C) CalculateFresnel(double x)
        {
            //https://dgk.badw.de/fileadmin/user_upload/Files/DGK/docs/b-314.pdf gleichung 4.8
            int n = 100000; // Number of steps for the integration
            double k, t, Fi;
            t = 0;
            double S = 0;
            double C = 0;
            double step = x / n;
            for (int i = 1; i <= n; i++)
            {
                Fi = 3 * Math.Pow(x / length, 2) - 2 * Math.Pow(x / length, 3);
                k = curvature1 + Fi * (curvature2 - curvature1);
                t += k * step;
                double sin, cos;
                (sin, cos) = Math.SinCos(t);
                S += sin * step;
                C += cos * step;
            }
            return (S, C);
        }

        (double S, double C) CalculateFresnel(double x, ref double t, ref double sumS, ref double sumC)
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
                (sin, cos) = Math.SinCos(
                    Math.Pow(x, 3) / (radius * Math.Pow(length, 2))
                    - Math.Pow(x, 4) / (2 * radius * Math.Pow(length, 3))
                    );
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
            v1 = new Vector2();
            if (!Double.IsNaN(t))
            {
                double x, y;
                (x, y) = Math.SinCos(t);
                v1 = new Vector2((float)y, (float)x);
                delta = Math.Sign(X * v1.Y - Y * v1.X) * delta;
            }

            int maxIterations = 1000;
            int i = 0;
            while (i < maxIterations && d > threshold)
            {
                (X_, Y_, t_, k) = PointAt(s);
                v2 = new Vector2((float)(X - X_), (float)(Y - Y_)); //vector from current position to Point of interest
                if (Double.IsNaN(t))
                {
                    v1 = new Vector2((float)Math.Cos(t_), (float)Math.Sin(t_)); //normal at current position
                    double vectorDot = Vector2.Dot(v1, v2) / v2.Length();
                    if (Math.Sign(vectorDot) != Math.Sign(delta))
                    {
                        delta = -0.5 * delta;
                    }
                    d = Math.Abs(vectorDot);
                }
                else
                {
                    double scalarCross = v2.X * v1.Y - v2.Y * v1.X;
                    double vectorDot = 1 - (Vector2.Dot(v2, v1) / v2.Length()); //Result is reached when both vectors are parallel
                    if (Math.Sign(scalarCross) != Math.Sign(delta))
                    {
                        delta = -0.5 * delta;
                    }
                    d = Math.Abs(vectorDot);
                }
                s = s + delta;
                i++;
            }
            if (i == maxIterations)
            {
                TrassierungLog.Logger?.LogWarning("Could not Interpolate a valid solution on Clothoid geometry" + d, this);
                return double.NaN;
            }
            return s;
        }
    }

    public class KSprung : TrassenGeometrie
    {
        double length;
        public KSprung(double l)
        {
            length = l;
        }

        public override (double X, double Y, double t, double k) PointAt(double s)
        {
            return (0, 0, 0, 0);
        }

        public override double sAt(double X, double Y, double t = double.NaN)
        {
            return length;
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
