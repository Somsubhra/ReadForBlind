using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadForBlind
{
    // If WriteableBitmap.Pixels are passed to these methods, then they are call by reference
    // so you should avoid the return value
    // Applies to all the methods

    // Plus need to remove one file from Utils & UtilsArray
    // But not now
    class UtilsArray
    {
        public static int[] GrayScale(int[] pixels)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = ColorToGray(pixels[i]);
            }
            return pixels;
        }

        public static int[] Binarize(int[] pixels, int threshold)
        {
            
            for (int i = 0; i < pixels.Length; i++)
            {
                int color = pixels[i];
                int a = color >> 24;
                int r = (color & 0x00ff0000) >> 16;
                int g = (color & 0x0000ff00) >> 8;
                int b = (color & 0x000000ff);
                int lumi = (7 * r + 38 * g + 19 * b + 32) >> 6;
                if (lumi < threshold)
                    pixels[i] = 0;
                else
                    pixels[i] = ~0 ;
            }
            return pixels;
        }

        public static int[] Bitwise_not(int[] pixels)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i] == 0)
                    pixels[i] = ~0;
                else
                    pixels[i] = 0;
            }
            return pixels;
        }

        private static int ColorToGray(int color)
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

        public static int[] Erode(int[] rp, int width, int height)
        {
            int CompareEmptyColor = 0;
            var w = width;
            var h = height;
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
    }
}
