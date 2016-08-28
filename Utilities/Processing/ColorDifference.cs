using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Utilities.Processing
{
    static class ColorDifference
    {
        public enum Method
        {
            Binary, // true or false, 0 is false
            Square,
            Dimensional,
            CIE76
        }

        public static double Calculate(Method method, int argb1, int argb2)
        {
            int[] c1 = ColorConversion.ArgbToArray(argb1);
            int[] c2 = ColorConversion.ArgbToArray(argb2);
            return Calculate(method, c1[1], c2[1], c1[2], c2[2], c1[3], c2[3], c1[0], c2[0]);
        }

        public static double Calculate(Method method, int r1, int r2, int g1, int g2, int b1, int b2, int a1 = -1, int a2 = -1)
        {
            switch (method)
            {
                case Method.Binary:
                    return (r1 == r2 && g1 == g2 && b1 == b2 && a1 == a2) ? 0 : 100;
                case Method.CIE76:
                    return CalculateCIE76(r1, r2, g1, g2, b1, b2);
                case Method.Dimensional:
                    if (a1 == -1 || a2 == -1) return Calculate3D(r1, r2, g1, g2, b1, b2);
                    else return Calculate4D(r1, r2, g1, g2, b1, b2, a1, a2);
                case Method.Square:
                    return CalculateSquare(r1, r2, g1, g2, b1, b2, a1, a2);
                default:
                    throw new InvalidOperationException();
            }
        }

        public static double Calculate(Method method, Color c1, Color c2, bool alpha)
        {
            switch (method)
            {
                case Method.Binary:
                    return (c1.R == c2.R && c1.G == c2.G && c1.B == c2.B && (!alpha || c1.A == c2.A)) ? 0 : 100;
                case Method.CIE76:
                    if (alpha) throw new InvalidOperationException();
                    return CalculateCIE76(c1, c2);
                case Method.Dimensional:
                    if (alpha) return Calculate4D(c1, c2);
                    else return Calculate3D(c1, c2);
                case Method.Square:
                    if (alpha) return CalculateSquareAlpha(c1, c2);
                    else return CalculateSquare(c1, c2);
                default:
                    throw new InvalidOperationException();
            }
        }

        // A simple idea, based on on a Square
        public static double CalculateSquare(int argb1, int argb2)
        {
            int[] c1 = ColorConversion.ArgbToArray(argb1);
            int[] c2 = ColorConversion.ArgbToArray(argb2);
            return CalculateSquare(c1[1], c2[1], c1[2], c2[2], c1[3], c2[3]);
        }

        public static double CalculateSquare(Color c1, Color c2)
        {
            return CalculateSquare(c1.R, c2.R, c1.G, c2.G, c1.B, c2.B);
        }

        public static double CalculateSquareAlpha(int argb1, int argb2)
        {
            int[] c1 = ColorConversion.ArgbToArray(argb1);
            int[] c2 = ColorConversion.ArgbToArray(argb2);
            return CalculateSquare(c1[1], c2[1], c1[2], c2[2], c1[3], c2[3], c1[0], c2[0]);
        }

        public static double CalculateSquareAlpha(Color c1, Color c2)
        {
            return CalculateSquare(c1.R, c2.R, c1.G, c2.G, c1.B, c2.B, c1.A, c2.A);
        }

        public static double CalculateSquare(int r1, int r2, int g1, int g2, int b1, int b2, int a1 = -1, int a2 = -1)
        {
            if (a1 == -1 || a2 == -1) return (Math.Abs(r1 - r2) + Math.Abs(g1 - g2) + Math.Abs(b1 - b2)) / 7.65;
            else return (Math.Abs(r1 - r2) + Math.Abs(g1 - g2) + Math.Abs(b1 - b2) + Math.Abs(a1 - a2)) / 10.2;
        }

        // from:http://stackoverflow.com/questions/9018016/how-to-compare-two-colors
        public static double Calculate3D(int argb1, int argb2)
        {
            int[] c1 = ColorConversion.ArgbToArray(argb1);
            int[] c2 = ColorConversion.ArgbToArray(argb2);
            return Calculate3D(c1[1], c2[1], c1[2], c2[2], c1[3], c2[3]);
        }

        public static double Calculate3D(Color c1, Color c2)
        {
            return Calculate3D(c1.R, c2.R, c1.G, c2.G, c1.B, c2.B);
        }

        public static double Calculate3D(int r1, int r2, int g1, int g2, int b1, int b2)
        {
            return Math.Sqrt(Math.Pow(Math.Abs(r1 - r2), 2) + Math.Pow(Math.Abs(g1 - g2), 2) + Math.Pow(Math.Abs(b1 - b2), 2)) / 4.41672955930063709849498817084;
        }

        // Same as above, but made 4D to include alpha channel
        public static double Calculate4D(int argb1, int argb2)
        {
            int[] c1 = ColorConversion.ArgbToArray(argb1);
            int[] c2 = ColorConversion.ArgbToArray(argb2);
            return Calculate4D(c1[1], c2[1], c1[2], c2[2], c1[3], c2[3], c1[0], c2[0]);
        }

        public static double Calculate4D(Color c1, Color c2)
        {
            return Calculate4D(c1.R, c2.R, c1.G, c2.G, c1.B, c2.B, c1.A, c2.A);
        }

        public static double Calculate4D(int r1, int r2, int g1, int g2, int b1, int b2, int a1, int a2)
        {
            return Math.Sqrt(Math.Pow(Math.Abs(r1 - r2), 2) + Math.Pow(Math.Abs(g1 - g2), 2) + Math.Pow(Math.Abs(b1 - b2), 2) + Math.Pow(Math.Abs(a1 - a2), 2)) / 5.1;
        }

        /**
        * Computes the difference between two RGB colors by converting them to the L*a*b scale and
        * comparing them using the CIE76 algorithm { http://en.wikipedia.org/wiki/Color_difference#CIE76}
        */
        public static double CalculateCIE76(int argb1, int argb2)
        {
            return CalculateCIE76(Color.FromArgb(argb1), Color.FromArgb(argb2));
        }

        public static double CalculateCIE76(Color c1, Color c2)
        {
            return CalculateCIE76(c1.R, c2.R, c1.G, c2.G, c1.B, c2.B);
        }

        public static double CalculateCIE76(int r1, int r2, int g1, int g2, int b1, int b2)
        {
            int[] lab1 = ColorConversion.ColorToLab(r1, g1, b1);
            int[] lab2 = ColorConversion.ColorToLab(r2, g2, b2);
            return Math.Sqrt(Math.Pow(lab2[0] - lab1[0], 2) + Math.Pow(lab2[1] - lab1[1], 2) + Math.Pow(lab2[2] - lab1[2], 2)) / 2.55;
        }
    }


    internal static class ColorConversion
    {

        public static int[] ArgbToArray(int argb)
        {
            return new int[] { (argb >> 24), (argb >> 16) & 0xFF, (argb >> 8) & 0xFF, argb & 0xFF };
        }

        public static int[] ColorToLab(int R, int G, int B)
        {
            // http://www.brucelindbloom.com

            double r, g, b, X, Y, Z, fx, fy, fz, xr, yr, zr;
            double Ls, fas, fbs;
            double eps = 216.0f / 24389.0f;
            double k = 24389.0f / 27.0f;

            double Xr = 0.964221f;  // reference white D50
            double Yr = 1.0f;
            double Zr = 0.825211f;

            // RGB to XYZ
            r = R / 255.0f; //R 0..1
            g = G / 255.0f; //G 0..1
            b = B / 255.0f; //B 0..1

            // assuming sRGB (D65)
            if (r <= 0.04045) r = r / 12;
            else r = (float)Math.Pow((r + 0.055) / 1.055, 2.4);

            if (g <= 0.04045) g = g / 12;
            else g = (float)Math.Pow((g + 0.055) / 1.055, 2.4);

            if (b <= 0.04045) b = b / 12;
            else b = (float)Math.Pow((b + 0.055) / 1.055, 2.4);

            X = 0.436052025f * r + 0.385081593f * g + 0.143087414f * b;
            Y = 0.222491598f * r + 0.71688606f * g + 0.060621486f * b;
            Z = 0.013929122f * r + 0.097097002f * g + 0.71418547f * b;

            // XYZ to Lab
            xr = X / Xr;
            yr = Y / Yr;
            zr = Z / Zr;

            if (xr > eps) fx = (float)Math.Pow(xr, 1 / 3.0);
            else fx = (float)((k * xr + 16.0) / 116.0);

            if (yr > eps) fy = (float)Math.Pow(yr, 1 / 3.0);
            else fy = (float)((k * yr + 16.0) / 116.0);

            if (zr > eps) fz = (float)Math.Pow(zr, 1 / 3.0);
            else fz = (float)((k * zr + 16.0) / 116);

            Ls = (116 * fy) - 16;
            fas = 500 * (fx - fy);
            fbs = 200 * (fy - fz);

            int[] lab = new int[3];
            lab[0] = (int)(2.55 * Ls + 0.5);
            lab[1] = (int)(fas + 0.5);
            lab[2] = (int)(fbs + 0.5);
            return lab;
        }

        private static double Difference(Color A, Color B)
        {
            long rmean = ((long)A.R + (long)B.R) / 2;

            long r = (long)A.R - (long)B.R;
            long g = (long)A.G - (long)B.G;
            long b = (long)A.B - (long)B.B;

            return Math.Sqrt((((512 + rmean) * r * r) >> 8) + 4 * g * g + (((767 - rmean) * b * b) >> 8));
        }
    }
}
