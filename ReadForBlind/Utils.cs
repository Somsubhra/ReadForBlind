using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

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
    }
}
