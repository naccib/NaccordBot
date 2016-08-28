using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DiscordBot.Utilities.Processing
{
    public static class ImageUtils
    {
        public static Color AverageColor(this Bitmap bmp)
        {
            int arraySize = bmp.Height * bmp.Width;

            int[] redValues = new int[arraySize];
            int[] greenValues = new int[arraySize];
            int[] blueValues = new int[arraySize];

            int count = 0;

            for (int x = 0; x < bmp.Size.Width; ++x)
            {
                for (int y = 0; y < bmp.Size.Height; ++y)
                {
                    redValues[count] = bmp.GetPixel(x, y).R;
                    greenValues[count] = bmp.GetPixel(x, y).G;
                    blueValues[count] = bmp.GetPixel(x, y).B;

                    count++;
                }
            }

            int r = Average(redValues);
            int g = Average(greenValues);
            int b = Average(blueValues);

            return Color.FromArgb(r, g, b);
        }

        public static int Average(int[] array)
        {
            int result = 0;

            foreach (int elem in array)
                result += elem;

            return result / array.Length;
        }

        public static Bitmap ScaleImage(this Bitmap image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);

            return newImage;
        }

        public static Bitmap resizeImage(this Bitmap image, int new_height, int new_width)
        {
            Bitmap new_image = new Bitmap(new_width, new_height);
            Graphics g = Graphics.FromImage((Image)new_image);
            g.InterpolationMode = InterpolationMode.High;
            g.DrawImage(image, 0, 0, new_width, new_height);
            return new_image;
        }

        public static Bitmap Blend(this Bitmap A, Bitmap B)
        {
            if (A == null || B == null)
                return null;

            if (A.Size != B.Size)
            {
                B = B.resizeImage(A.Height, A.Width);
            }

            Bitmap[] bmSrcs = new Bitmap[2];

            bmSrcs[0] = A;
            bmSrcs[1] = B;

            Bitmap bmDst = new Bitmap(bmSrcs[0].Width, bmSrcs[0].Height);
            for (int y = 0; y < bmSrcs[0].Height; y++)
            {
                for (int x = 0; x < bmSrcs[0].Width; x++)
                {
                    int a = 0, r = 0, g = 0, b = 0, iCount = 0;
                    foreach (Bitmap bmSrc in bmSrcs)
                    {
                        Color colSrc = bmSrc.GetPixel(x, y);
                        // check alpha (transparency): ignore transparent pixels
                        if (colSrc.A > 0)
                        {
                            a += colSrc.A;
                            r = Math.Max(r, colSrc.R);
                            g = Math.Max(g, colSrc.G);
                            b = Math.Max(b, colSrc.B);
                            iCount++;
                        }
                    }
                    Color colDst = Color.FromArgb(iCount > 1 ? (int)Math.Round((double)a / iCount) : a, r, g, b);
                    bmDst.SetPixel(x, y, colDst);
                }
            }

            return bmDst;
        }

        public static Bitmap ReplaceColor(this Bitmap image, Color oldColor, Color newColor)
        {
            Bitmap result = new Bitmap(image.Width, image.Height);

            for (int x = 0; x < result.Width; ++x)
                for (int y = 0; y < result.Height; ++y)
                    result.SetPixel(x, y, result.GetPixel(x, y) == oldColor ? newColor : result.GetPixel(x, y));

            return result;
        }
    }
}
