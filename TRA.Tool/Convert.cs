//This file is an excerpt from egbt22lib to provide Array conversions as a fallback if TRA.Tool is compiled without egbt22lib
using System;
using System.Collections.Generic;

#if !USE_EGBT22LIB
namespace egbt22lib
{
    public static class Convert
    {
        #region arrays

        /// <summary>
        ///     Processes arrays of coordinates (x, y, z) using a provided calculation function
        ///     and returns the resulting arrays.
        /// </summary>
        /// <param name="xin">Input array of x-coordinates.</param>
        /// <param name="yin">Input array of y-coordinates.</param>
        /// <param name="zin">Input array of z-coordinates.</param>
        /// <param name="calc">
        ///     A function that takes x, y, and z coordinates as inputs and returns a tuple
        ///     containing the transformed x, y, and z coordinates.
        /// </param>
        /// <returns>
        ///     A tuple containing three arrays: the resulting x-coordinates, y-coordinates, and z-coordinates
        ///     after applying the provided calculation function.
        /// </returns>
        public static (double[] x, double[] y, double[] z) CalcArrays3(double[] xin, double[] yin, double[] zin,
            Func<double, double, double, (double x, double y, double z)> calc)
        {
            int n = xin.Length;
            double[] x = new double[n];
            double[] y = new double[n];
            double[] z = new double[n];
            for (int i = 0; i < n; i++) (x[i], y[i], z[i]) = calc(xin[i], yin[i], zin[i]);
            return (x, y, z);
        }

        /// <summary>
        ///     Processes an array of 3D points using a provided calculation function
        ///     and returns the resulting array of transformed points.
        /// </summary>
        /// <param name="points">A jagged array where each inner array represents a 3D point with x, y, and z coordinates.</param>
        /// <param name="calc">
        ///     A function that takes x, y, and z coordinates as input and returns a tuple
        ///     containing the transformed x, y, and z coordinates.
        /// </param>
        /// <returns>
        ///     A jagged array of transformed 3D points, where each inner array contains the x, y, and z coordinates
        ///     resulting from applying the provided calculation function.
        /// </returns>
        public static double[][] CalcArray3(double[][] points,
            Func<double, double, double, (double x, double y, double z)> calc)
        {
            int n = points.Length;
            double[][] xyz = new double[n][];
            for (int i = 0; i < n; i++)
            {
                (double x, double y, double z) = calc(points[i][0], points[i][1], points[i][2]);
                xyz[i] = new[] { x, y, z };
            }

            return xyz;
        }

        public static (double[] gamma, double[] k, bool[] isInsideBBox, bool[] isInHeightRange) CalcArrays3(double[] xin, double[] yin, double[] zin,
            Func<double, double, double, (double gamma, double k, bool isInsideBBox, bool isInHeightRange)> calc)
        {
            int n = xin.Length;
            double[] gamma = new double[n];
            double[] k = new double[n];
            bool[] isInsideBBox = new bool[n];
            bool[] isInHeightRange = new bool[n];
            for (int i = 0; i < n; i++) (gamma[i], k[i], isInsideBBox[i], isInHeightRange[i]) = calc(xin[i], yin[i], zin[i]);
            return (gamma, k, isInsideBBox, isInHeightRange);
        }


        public static (double[] gamma, double[] k, bool[] isInsideBBox, bool[] isInHeightRange) CalcArray3(double[][] points,
            Func<double, double, double, (double gamma, double k, bool isInsideBBox, bool isInHeightRange)> calc)
        {
            int n = points.Length;
            double[] gamma = new double[n];
            double[] k = new double[n];
            bool[] isInsideBBox = new bool[n];
            bool[] isInHeightRange = new bool[n];
            for (int i = 0; i < n; i++)
            {
                (gamma[i], k[i], isInsideBBox[i], isInHeightRange[i]) = calc(points[i][0], points[i][1], points[i][2]);
            }

            return (gamma, k, isInsideBBox, isInHeightRange);
        }
 
        public static (bool[] isInsideBBox, bool[] isInHeightRange) CalcArrays3(double[] xin, double[] yin, double[] zin,
            Func<double, double, double, (bool isInsideBBox, bool isInHeightRange)> calc)
        {
            int n = xin.Length;
            bool[] isInsideBBox = new bool[n];
            bool[] isInHeightRange = new bool[n];
            for (int i = 0; i < n; i++) (isInsideBBox[i], isInHeightRange[i]) = calc(xin[i], yin[i], zin[i]);
            return (isInsideBBox, isInHeightRange);
        }

        public static (bool[] isInsideBBox, bool[] isInHeightRange) CalcArray3(double[][] points,
            Func<double, double, double, (bool isInsideBBox, bool isInHeightRange)> calc)
        {
            int n = points.Length;
            bool[] isInsideBBox = new bool[n];
            bool[] isInHeightRange = new bool[n];
            for (int i = 0; i < n; i++)
            {
                (isInsideBBox[i], isInHeightRange[i]) = calc(points[i][0], points[i][1], points[i][2]);
            }

            return (isInsideBBox, isInHeightRange);
        }

        /// <summary>
        ///     Processes arrays of 2D coordinates (x, y) using a provided calculation function
        ///     and returns the resulting arrays.
        /// </summary>
        /// <param name="xin">Input array of x-coordinates.</param>
        /// <param name="yin">Input array of y-coordinates.</param>
        /// <param name="calc">
        ///     A function that takes x and y coordinates as inputs and returns a tuple
        ///     containing the transformed x and y coordinates.
        /// </param>
        /// <returns>
        ///     A tuple containing two arrays: the resulting x-coordinates and y-coordinates
        ///     after applying the provided calculation function.
        /// </returns>
        public static (double[] x, double[] y) CalcArrays2(double[] xin, double[] yin,
            Func<double, double, (double x, double y)> calc)
        {
            int n = xin.Length;
            double[] x = new double[n];
            double[] y = new double[n];
            for (int i = 0; i < n; i++) (x[i], y[i]) = calc(xin[i], yin[i]);
            return (x, y);
        }

        /// <summary>
        ///     Processes an array of coordinate pairs using a provided calculation function
        ///     and returns the resulting array of transformed coordinate pairs.
        /// </summary>
        /// <param name="points">Input array of points, where each point is an array containing two coordinates (x, y).</param>
        /// <param name="calc">
        ///     A function that takes two inputs, x and y coordinates, and returns a tuple
        ///     with transformed x and y coordinates.
        /// </param>
        /// <returns>
        ///     A jagged array where each inner array contains the transformed x and y coordinates
        ///     for the corresponding input point.
        /// </returns>
        public static double[][] CalcArray2(double[][] points, Func<double, double, (double x, double y)> calc)
        {
            int n = points.Length;
            double[][] xy = new double[n][];
            for (int i = 0; i < n; i++)
            {
                (double x, double y) = calc(points[i][0], points[i][1]);
                xy[i] = new[] { x, y };
            }

            return xy;
        }

        public static (double[] gamma, double[] k, bool[] isInsideBBox) CalcArrays2(double[] xin, double[] yin,
            Func<double, double, (double gamma, double k, bool isInsideBBox)> calc)
        {
            int n = xin.Length;
            double[] gamma = new double[n];
            double[] k = new double[n];
            bool[] isInsideBBox = new bool[n];
            for (int i = 0; i < n; i++) (gamma[i], k[i], isInsideBBox[i]) = calc(xin[i], yin[i]);
            return (gamma, k, isInsideBBox);
        }

        public static (double[] gamma, double[] k, bool[] isInsideBBox) CalcArray2(double[][] points, Func<double, double, (double gamma, double k, bool isInsideBBox)> calc)
        {
            int n = points.Length;
            double[] gamma = new double[n];
            double[] k = new double[n];
            bool[] isInsideBBox = new bool[n];
            for (int i = 0; i < n; i++)
            {
                (gamma[i], k[i], isInsideBBox[i]) = calc(points[i][0], points[i][1]);
            }

            return (gamma, k, isInsideBBox);
        }

        public static bool[] CalcArrays2(double[] xin, double[] yin,
            Func<double, double, bool> calc)
        {
            int n = xin.Length;
            bool[] isInsideBBox = new bool[n];
            for (int i = 0; i < n; i++) isInsideBBox[i] = calc(xin[i], yin[i]);
            return isInsideBBox;
        }

        public static bool[] CalcArray2(double[][] points, Func<double, double, bool> calc)
        {
            int n = points.Length;
            bool[] isInsideBBox = new bool[n];
            for (int i = 0; i < n; i++)
            {
                isInsideBBox[i] = calc(points[i][0], points[i][1]);
            }

            return isInsideBBox;
        }

        #endregion
    }
}
#endif