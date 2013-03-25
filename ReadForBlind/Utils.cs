using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Effects;
using System.Windows.Media.Animation;
using System.Diagnostics;
using System.Windows;

namespace ReadForBlind
{
    public class Utils
    {
        /// <summary>
        /// The Hawaii Application Id.
        /// </summary>
        public const string HawaiiApplicationId = "50be8f73-9a12-457c-af36-a982afe9756c";

        public static byte[] imageToByte(WriteableBitmap bmp)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bmp.SaveJpeg(ms, bmp.PixelWidth, bmp.PixelHeight, 0, 100);
                byte[] buffer = new byte[ms.Length];

                long seekPosition = ms.Seek(0, SeekOrigin.Begin);
                int bytesRead = ms.Read(buffer, 0, buffer.Length);
                seekPosition = ms.Seek(0, SeekOrigin.Begin);

                return buffer;
            }
        }

        // call by reference
        public static void resizeImage( ref WriteableBitmap bmp )
        {
            // TODO: memory management 
            // we have 2 options
            // i) use "using" statement
            // ii) dispose of object "ms" before the method finishes (**check bmp as ms is set as it's source )
            MemoryStream ms = new MemoryStream();
            int h, w;
            if (bmp.PixelWidth > bmp.PixelHeight)
            {
                double aspRatio = bmp.PixelWidth / (double)bmp.PixelHeight;
                double hh, ww;
                hh = (640.0 / aspRatio);
                ww = hh * aspRatio;
                h = (int)hh;
                w = (int)ww;
            }
            else
            {
                double aspRatio = bmp.PixelHeight / (double)bmp.PixelWidth;
                double hh, ww;
                hh = (480.0 / aspRatio);
                ww = hh * aspRatio;
                h = (int)hh;
                w = (int)ww;
            }
            bmp.SaveJpeg(ms, w, h, 0, 100);
            bmp.SetSource(ms);
        }

        public static void deskew(ref WriteableBitmap bmp)
        {
            Deskew sk = new Deskew(bmp);
            double skewAngle = -1 * sk.GetSkewAngle();
            bmp = WriteableBitmapExtensions.RotateFree(bmp, skewAngle);
        }

        public class Deskew
        {
            // Representation of a line in the image.
            public class HougLine
            {
                // Count of points in the line.
                public int Count;
                // Index in Matrix.
                public int Index;
                // The line is represented as all x,y that solve y*cos(alpha)-x*sin(alpha)=d
                public double Alpha;
                public double d;
            }
            // The Bitmap
            WriteableBitmap cBmp;
            // The range of angles to search for lines
            double cAlphaStart = -20;
            double cAlphaStep = 0.2;
            int cSteps = 40 * 5;
            // Precalculation of sin and cos.
            double[] cSinA;
            double[] cCosA;
            // Range of d
            double cDMin;
            double cDStep = 1;
            int cDCount;
            // Count of points that fit in a line.

            int[] cHMatrix;
            // Calculate the skew angle of the image cBmp.
            public double GetSkewAngle()
            {
                Deskew.HougLine[] hl = null;
                int i = 0;
                double sum = 0;
                int count = 0;

                // Hough Transformation
                Calc();
                // Top 20 of the detected lines in the image.
                hl = GetTop(20);
                // Average angle of the lines
                for (i = 0; i <= 19; i++)
                {
                    sum += hl[i].Alpha;
                    count += 1;
                }
                return sum / count;
            }

            // Calculate the Count lines in the image with most points.
            private HougLine[] GetTop(int Count)
            {
                HougLine[] hl = null;
                int i = 0;
                int j = 0;
                HougLine tmp = null;
                int AlphaIndex = 0;
                int dIndex = 0;

                hl = new HougLine[Count + 1];
                for (i = 0; i <= Count - 1; i++)
                {
                    hl[i] = new HougLine();
                }
                for (i = 0; i <= cHMatrix.Length - 1; i++)
                {
                    if (cHMatrix[i] > hl[Count - 1].Count)
                    {
                        hl[Count - 1].Count = cHMatrix[i];
                        hl[Count - 1].Index = i;
                        j = Count - 1;
                        while (j > 0 && hl[j].Count > hl[j - 1].Count)
                        {
                            tmp = hl[j];
                            hl[j] = hl[j - 1];
                            hl[j - 1] = tmp;
                            j -= 1;
                        }
                    }
                }
                for (i = 0; i <= Count - 1; i++)
                {
                    dIndex = hl[i].Index / cSteps;
                    AlphaIndex = hl[i].Index - dIndex * cSteps;
                    hl[i].Alpha = GetAlpha(AlphaIndex);
                    hl[i].d = dIndex + cDMin;
                }
                return hl;
            }
            public Deskew(WriteableBitmap bmp)
            {
                cBmp = bmp;
            }
            // Hough Transforamtion:
            private void Calc()
            {
                int x = 0;
                int y = 0;
                int hMin = cBmp.PixelHeight / 4;
                int hMax = cBmp.PixelHeight * 3 / 4;

                Init();
                for (y = hMin; y <= hMax; y++)
                {
                    for (x = 1; x <= cBmp.PixelWidth - 2; x++)
                    {
                        // Only lower edges are considered.
                        if (IsBlack(x, y))
                        {
                            if (!IsBlack(x, y + 1))
                            {
                                Calc(x, y);
                            }
                        }
                    }
                }
            }
            // Calculate all lines through the point (x,y).
            private void Calc(int x, int y)
            {
                int alpha = 0;
                double d = 0;
                int dIndex = 0;
                int Index = 0;

                for (alpha = 0; alpha <= cSteps - 1; alpha++)
                {
                    d = y * cCosA[alpha] - x * cSinA[alpha];
                    dIndex = CalcDIndex(d);
                    Index = dIndex * cSteps + alpha;
                    try
                    {
                        cHMatrix[Index] += 1;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
                }
            }
            private int CalcDIndex(double d)
            {
                return Convert.ToInt32(d - cDMin);
            }
            private bool IsBlack(int x, int y)
            {
                Color c = default(Color);
                double luminance = 0;

                c = cBmp.GetPixel(x, y);
                luminance = (c.R * 0.299) + (c.G * 0.587) + (c.B * 0.114);
                return luminance < 140;
            }
            private void Init()
            {
                int i = 0;
                double angle = 0;

                // Precalculation of sin and cos.
                cSinA = new double[cSteps];
                cCosA = new double[cSteps];
                for (i = 0; i <= cSteps - 1; i++)
                {
                    angle = GetAlpha(i) * Math.PI / 180.0;
                    cSinA[i] = Math.Sin(angle);
                    cCosA[i] = Math.Cos(angle);
                }
                // Range of d:
                cDMin = -cBmp.PixelWidth;
                cDCount = (int)(2 * (cBmp.PixelWidth + cBmp.PixelHeight) / cDStep);
                cHMatrix = new int[cDCount * cSteps + 1];
            }

            public double GetAlpha(int Index)
            {
                return cAlphaStart + Index * cAlphaStep;
            }
        }
    }
}
