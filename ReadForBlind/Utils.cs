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
        private int offset = 120, limit;
        public int width { get; set; }
        public int height { get; set; }
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
        public static void resizeImage(ref WriteableBitmap bmp)
        {
            // TODO: memory management 
            // we have 2 options
            // i) use "using" statement
            // ii) dispose of object "ms" before the method finishes (**check bmp1 as ms is set as it's source )
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

        public Utils(int width, int height)
        {
            this.width = width;
            this.height = height;
            this.offset = 80;
        }

        public int[] GrayScale(int[] pixels)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = ColorToGray(pixels[i]);
            }
            return pixels;
        }

        public int[] Binarize(int[] pixels, int threshold)
        {

            for (int i = 0; i < pixels.Length; i++)
            {
                int color = pixels[i];
                int a = color >> 24;
                int r = (color & 0x00ff0000) >> 16;
                int g = (color & 0x0000ff00) >> 8;
                int b = (color & 0x000000ff);
                //int lumi = (7 * r + 38 * g + 19 * b + 32) >> 6;
                int lumi = (r + g + b) / 3;
                if (lumi < threshold)
                    pixels[i] = EncodeColor(Colors.Black);
                else
                    pixels[i] = EncodeColor(Colors.White);
            }
            return pixels;
        }

        public int[] Bitwise_not(int[] pixels)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i] == EncodeColor(Colors.Black))
                {
                    pixels[i] = EncodeColor(Colors.White);
                }
                else
                {
                    pixels[i] = EncodeColor(Colors.Black);
                }
            }
            return pixels;
        }

        private int ColorToGray(int color)
        {
            int gray = 0;

            int a = color >> 24;
            int r = (color & 0x00ff0000) >> 16;
            int g = (color & 0x0000ff00) >> 8;
            int b = (color & 0x000000ff);

            if ((r == g) && (g == b))
            {
                gray = color;
            }
            else
            {
                // Calculate for the illumination.
                // I =(int)(0.109375*R + 0.59375*G + 0.296875*B + 0.5)
                int i = (7 * r + 38 * g + 19 * b + 32) >> 6;

                gray = ((a & 0xFF) << 24) | ((i & 0xFF) << 16) | ((i & 0xFF) << 8) | (i & 0xFF);
            }
            return gray;
        }

        enum State { 
            Increasing,
            Stable,
            Decreasing
        };

        public int GetThreshold(int[] pixels)
        {
            int[] histo = new int[256];
            for (int i = 0; i < pixels.Length; i++)
            {
                Color c = DecodeColor(pixels[i]);
                int inten = (int)((c.R + c.G + c.B) / 3);
                histo[inten]++;
            }
            for (int i = 0; i < histo.Length; i++)
            {
                histo[i] = histo[i] / 1000;
            }
            List<KeyValuePair<int, int>> points = new List<KeyValuePair<int, int>>();
            State stat = State.Stable;
            int count = 0;
            for (int i = 0; i < histo.Length - 1; i++)
            {
                if (histo[i] > histo[i + 1] && stat != State.Decreasing) {
                    points.Add(new KeyValuePair<int, int>(i, histo[i]));
                    stat = State.Decreasing;
                }
                else if (histo[i] < histo[i + 1] && stat != State.Increasing)
                {
                    points.Add(new KeyValuePair<int, int>(i, histo[i]));
                    stat = State.Increasing;
                }
                else 
                {
                    stat = State.Stable;
                }
            }
            int max = 0, min = 99999, max2 = 0;
            int maxIndex = -1, minIndex = -1, max2Index = -1;
            foreach (var item in points)
            {
                if (max < item.Value) 
                {
                    max2 = max;
                    max2Index = maxIndex;
                    max = item.Value;
                    maxIndex = item.Key;
                }
            }
            int th = max2Index + ((maxIndex + max2Index) / 2);
            return th;
        }

        public Boundaries CheckBoundaries(int[] pixels)
        {
            // check left
            double boundayFactor = 0.1; //10% of the image width/height
            limit = GetIntensity(pixels);
            Boundaries b = new Boundaries();

            b.Bottom = CheckRight(pixels, boundayFactor);
            b.Top = CheckLeft(pixels, boundayFactor);
            b.Right = CheckTop(pixels, boundayFactor);
            b.Left = CheckBottom(pixels, boundayFactor);
            return b;
        }

        private int GetIntensity(int[] pixels)
        {
            int intensity = 0;
            for (int i = 0; i < this.width; i++)
            {
                for (int j = 0; j < this.height; j++)
                {
                    int color = GetPixel(pixels, j, i);
                    Color c = DecodeColor(color);
                    intensity += (int)(c.R + c.G + c.B) / 3;
                }
            }
            return (intensity / (this.width * this.height));
        }

        private bool CheckLeft(int[] bmp, double bf)
        {
            int intensity = 0;
            for (int row = 0; row < this.height; row++)
            {
                int color = GetPixel(bmp, row, 0);
                Color c = DecodeColor(color);
                intensity += (c.R + c.B + c.G) / 3;

            }

            intensity = intensity / this.height;
            if (intensity < offset)
            {
                return false;
            }
            else
            {
                //int col, row;
                //for (col = 0; col < this.width / 2; col++)
                //{
                //    intensity = 0;
                //    for (row = 0; row < this.height; row++)
                //    {
                //        int color = GetPixel(bmp, row, col);
                //        Color c = DecodeColor(color);
                //        intensity += (int)(c.R + c.G + c.B) / 3;
                //        //color = GetPixel(bmp, row, col + 1);
                //        //c = DecodeColor(color);
                //        //intensity += (int)(c.R + c.G + c.B) / 3;
                //    }
                //    intensity /= (this.height);
                //    if (intensity < offset)
                //        return false;
                //}
                return true;
            }
        }

        private bool CheckRight(int[] bmp, double bf)
        {
            int intensity = 0;
            for (int row = 0; row < this.height; row++)
            {
                int color = GetPixel(bmp, row, this.width - 1);
                Color c = DecodeColor(color);
                intensity += (c.R + c.B + c.G) / 3;

            }

            intensity = intensity / this.height;
            if (intensity < offset)
            {
                return false;
            }
            else
            {
                //int col, row;
                //    for (col = 1; col < this.width / 2; col++)
                //    {
                //        intensity = 0;
                //        for (row = 0; row < this.height; row++)
                //        {
                //            int color = GetPixel(bmp, row, this.width - col - 1);
                //            Color c = DecodeColor(color);
                //            intensity += (int)(c.R + c.G + c.B) / 3;
                //            //color = GetPixel(bmp, row, this.width - col);
                //            //c = DecodeColor(color);
                //            //intensity += (int)(c.R + c.G + c.B) / 3;
                //        }
                //        intensity /= (this.height);
                //        if (intensity < offset)
                //            return false;
                //    }
                return true;
            }
        }

        private bool CheckTop(int[] bmp, double bf)
        {
            int intensity = 0;
            for (int col = 0; col < this.width; col++)
            {
                int color = GetPixel(bmp, 0, col);
                Color c = DecodeColor(color);
                intensity += (c.R + c.B + c.G) / 3;
            }
            intensity /= this.height;
            if (intensity < offset)
                return false;
            return true;
        }

        private bool CheckBottom(int[] bmp, double bf)
        {
            int intensity = 0;
            for (int col = 0; col < this.width; col++)
            {
                int color = GetPixel(bmp, this.height - 1, col);
                Color c = DecodeColor(color);
                intensity += (c.R + c.B + c.G) / 3;
            }
            intensity /= this.height;
            if (intensity < offset)
                return false;
            return true;
        }

        public Rect GetCropArea(int[] bmp) { 
            int cutoff = 90;
            double l = GetLeft(bmp,cutoff);
            double r = GetRight(bmp, cutoff);
            double b = GetBottom(bmp, cutoff);
            double t = GetTop(bmp, cutoff);
            Rect rec = new Rect(l, t, (r - l), (b - t));
            return rec;
        }

        public int GetLeft(int[] bmp, int cutoff) 
        {
            for (int i = 0; i < this.width; i++)
            {
                int inten = 0;
                for (int j = 0; j < this.height; j++)
                {
                    int p = GetPixel(bmp, j, i);
                    Color r = DecodeColor(p);
                    inten += (r.R + r.G + r.B) / 3;
                }
                inten /= this.height;
                if (inten > cutoff)
                    return i;
            }
            return 0;
        }

        //public int GetRight(int[] bmp, int cutoff) 
        //{
        //    for (int i = 0; i < this.width; i++)
        //    {
        //        int inten = 0;
        //        for (int j = 0; j < this.height; j++)
        //        {
        //            int p = GetPixel(bmp, this.width - i -1, j);
        //            Color c = DecodeColor(p);
        //            inten += (c.R + c.G + c.B) / 3;
        //        }
        //        inten /= this.height;
        //        if (inten > cutoff)
        //            return (this.width - i);
        //    }
        //    return -1;
        //}

        public int GetRight(int[] bmp, int cutoff)
        {
            for (int i = this.width; i > 0; i--)
            {
                int inten = 0;
                for (int j = 0; j < this.height; j++)
                {
                    int p = GetPixel(bmp, j , i-1);
                    Color c = DecodeColor(p);
                    inten += (c.R + c.G + c.B) / 3;
                }
                inten /= this.height;
                if (inten > cutoff)
                    return (i);
            }
            return 0;
        }

        public int GetTop(int[] bmp, int cutoff)
        {
            for (int i = 0; i < this.height; i++)
            {
                int inten = 0;
                for (int j = 0; j < this.width; j++)
                {
                    Color c = DecodeColor(GetPixel(bmp, i, j));
                    inten += (c.R + c.G + c.B) / 3;
                }
                inten /= this.width;
                if (inten > cutoff)
                    return i;
            }
            return 0;
        }

        public int GetBottom(int[] bmp, int cutoff)
        {
            for (int i = 0; i < this.height; i++)
            {
                int inten = 0;
                for (int j = 0; j < this.width; j++)
                {
                    Color c = DecodeColor(GetPixel(bmp, this.height - i - 1, j));
                    inten += (c.R + c.G + c.B) / 3;
                }
                inten /= this.width;
                if (inten > cutoff)
                    return (this.height - i) ;
            }
            return 0;
        }

        private int GetPixel(int[] pixels, int i, int j)
        {
            return pixels[(this.width * i) + j];
        }

        public class Boundaries
        {
            public bool Left { get; set; }
            public bool Right { get; set; }
            public bool Top { get; set; }
            public bool Bottom { get; set; }
        }

        public int[] Erode(int[] rp)
        {
            int CompareEmptyColor = 0;
            var w = this.width;
            var h = this.height;
            int[] p = new int[rp.Length];
            //rp = p;
            for (int j = 0; j < p.Length; j++)
            {
                p[j] = rp[j];
                rp[j] = 0;
            }
            var empty = CompareEmptyColor;
            int c, cm;
            int i = 0;

            // Erode every pixel
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++, i++)
                {
                    // Middle pixel
                    cm = p[y * w + x];
                    if (cm == empty) { continue; }

                    // Row 0
                    // Left pixel
                    if (x - 2 > 0 && y - 2 > 0)
                    {
                        c = p[(y - 2) * w + (x - 2)];
                        if (c == empty) { continue; }
                    }
                    // Middle left pixel
                    if (x - 1 > 0 && y - 2 > 0)
                    {
                        c = p[(y - 2) * w + (x - 1)];
                        if (c == empty) { continue; }
                    }
                    if (y - 2 > 0)
                    {
                        c = p[(y - 2) * w + x];
                        if (c == empty) { continue; }
                    }
                    if (x + 1 < w && y - 2 > 0)
                    {
                        c = p[(y - 2) * w + (x + 1)];
                        if (c == empty) { continue; }
                    }
                    if (x + 2 < w && y - 2 > 0)
                    {
                        c = p[(y - 2) * w + (x + 2)];
                        if (c == empty) { continue; }
                    }

                    // Row 1
                    // Left pixel
                    if (x - 2 > 0 && y - 1 > 0)
                    {
                        c = p[(y - 1) * w + (x - 2)];
                        if (c == empty) { continue; }
                    }
                    if (x - 1 > 0 && y - 1 > 0)
                    {
                        c = p[(y - 1) * w + (x - 1)];
                        if (c == empty) { continue; }
                    }
                    if (y - 1 > 0)
                    {
                        c = p[(y - 1) * w + x];
                        if (c == empty) { continue; }
                    }
                    if (x + 1 < w && y - 1 > 0)
                    {
                        c = p[(y - 1) * w + (x + 1)];
                        if (c == empty) { continue; }
                    }
                    if (x + 2 < w && y - 1 > 0)
                    {
                        c = p[(y - 1) * w + (x + 2)];
                        if (c == empty) { continue; }
                    }

                    // Row 2
                    if (x - 2 > 0)
                    {
                        c = p[y * w + (x - 2)];
                        if (c == empty) { continue; }
                    }
                    if (x - 1 > 0)
                    {
                        c = p[y * w + (x - 1)];
                        if (c == empty) { continue; }
                    }
                    if (x + 1 < w)
                    {
                        c = p[y * w + (x + 1)];
                        if (c == empty) { continue; }
                    }
                    if (x + 2 < w)
                    {
                        c = p[y * w + (x + 2)];
                        if (c == empty) { continue; }
                    }

                    // Row 3
                    if (x - 2 > 0 && y + 1 < h)
                    {
                        c = p[(y + 1) * w + (x - 2)];
                        if (c == empty) { continue; }
                    }
                    if (x - 1 > 0 && y + 1 < h)
                    {
                        c = p[(y + 1) * w + (x - 1)];
                        if (c == empty) { continue; }
                    }
                    if (y + 1 < h)
                    {
                        c = p[(y + 1) * w + x];
                        if (c == empty) { continue; }
                    }
                    if (x + 1 < w && y + 1 < h)
                    {
                        c = p[(y + 1) * w + (x + 1)];
                        if (c == empty) { continue; }
                    }
                    if (x + 2 < w && y + 1 < h)
                    {
                        c = p[(y + 1) * w + (x + 2)];
                        if (c == empty) { continue; }
                    }

                    // Row 4
                    if (x - 2 > 0 && y + 2 < h)
                    {
                        c = p[(y + 2) * w + (x - 2)];
                        if (c == empty) { continue; }
                    }
                    if (x - 1 > 0 && y + 2 < h)
                    {
                        c = p[(y + 2) * w + (x - 1)];
                        if (c == empty) { continue; }
                    }
                    if (y + 2 < h)
                    {
                        c = p[(y + 2) * w + x];
                        if (c == empty) { continue; }
                    }
                    if (x + 1 < w && y + 2 < h)
                    {
                        c = p[(y + 2) * w + (x + 1)];
                        if (c == empty) { continue; }
                    }
                    if (x + 2 < w && y + 2 < h)
                    {
                        c = p[(y + 2) * w + (x + 2)];
                        if (c == empty) { continue; }
                    }

                    // If all neighboring pixels are processed 
                    // it's clear that the current pixel is not a boundary pixel.
                    rp[i] = cm;
                }
            }

            return rp;
        }

        public void deskew(ref WriteableBitmap bmp)
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

        private int EncodeColor(Color c)
        {
            int color = 0;
            color = color | c.A;
            color = (color << 8) | c.R;
            color = (color << 8) | c.G;
            color = (color << 8) | c.B;
            return color;
        }

        private Color DecodeColor(int color)
        {
            Color c = new Color();
            c.A = (byte)(color >> 24);
            c.R = (byte)((color & 0x00ff0000) >> 16);
            c.G = (byte)((color & 0x0000ff00) >> 8);
            c.B = (byte)(color & 0x000000ff);
            return c;
        }
        public static class MyGlobals
        {
            public static int mode = 0;     // by default flash is off
        }
    }

    
}
