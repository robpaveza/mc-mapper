using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCMapper.TexturePackReader.NetFramework
{
    internal sealed class ImageInspector
    {
        public unsafe int CalculateImageAverageColor(Bitmap sourceImage)
        {
            var data = sourceImage.LockBits(new Rectangle(0, 0, sourceImage.Width, sourceImage.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            // Vanilla textures are 16x16.  If we just accumulate R, G, and B in ints (with a max of ~2bil), we will never overrun
            // e.g., 16 (wide) x 16 (tall) x 256 (possible values) = 65,536.  A 4k resolution - 4096x4096 - only gets to 4.2bil.  So, we should be able
            // to handle up to 2k textures with R, G, and B accumulators of 32-bit ints.

            try
            {
                int rAccum = 0, gAccum = 0, bAccum = 0, aAccum = 0, count = 0;
                int totalPixels = sourceImage.Width * sourceImage.Height;
                uint* pixel = (uint*)data.Scan0.ToPointer();
                for (int pixelNumber = 0; pixelNumber < totalPixels; pixelNumber++)
                {
                    uint currentPixel = pixel[pixelNumber];
                    byte a = (byte)((currentPixel & 0xff000000) >> 24);
                    if (a > 0)
                    {
                        byte r = (byte)((currentPixel & 0xff0000) >> 16);
                        byte g = (byte)((currentPixel & 0xff00) >> 8);
                        byte b = (byte)(currentPixel & 0xff);
                        rAccum += r;
                        gAccum += g;
                        bAccum += b;
                        aAccum += a;
                        count++;
                    }
                }

                int aFinal = aAccum / count;
                int rFinal = rAccum / count;
                int gFinal = gAccum / count;
                int bFinal = bAccum / count;

                return (aFinal << 24) | (rFinal << 16) | (gFinal << 8) | (bFinal);
            }
            finally
            {
                sourceImage.UnlockBits(data);
            }
        }
    }
}
