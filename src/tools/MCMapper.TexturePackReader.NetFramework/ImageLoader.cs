using DmitryBrant.ImageFormats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCMapper.TexturePackReader.NetFramework
{
    internal sealed class ImageLoader
    {
        private string rootPath_;

        public ImageLoader(string resourcePackPath)
        {
            rootPath_ = resourcePackPath;
        }

        public Bitmap Load(string fileNameBase, Color? tintColor = null)
        {
            string completePath = Path.Combine(rootPath_, "textures", "blocks", fileNameBase);
            Bitmap result = null;
            if (File.Exists(completePath + ".png"))
            {
                result = Image.FromFile(completePath + ".png") as Bitmap;
            }
            else if (File.Exists(completePath + ".tga"))
            {
                result = TgaReader.Load(completePath + ".tga");
            }
            else if (File.Exists(completePath + ".bmp"))
            {
                result = Image.FromFile(completePath + ".bmp") as Bitmap;
            }
            else
            {
                throw new FileNotFoundException("Could not find a resource in the specified location; tried .png, .tga, .bmp.", completePath);
            }

            if (result.Width != result.Height)
            {
                // non-square textures are always vertically oriented
                var replacement = new Bitmap(result.Width, result.Width, result.PixelFormat);
                using (Graphics g = Graphics.FromImage(replacement))
                {
                    g.DrawImage(result, new Rectangle(0, 0, result.Width, result.Width), 0, 0, result.Width, result.Width, GraphicsUnit.Pixel);
                }

                result.Dispose();
                result = replacement;
            }

            if (tintColor.HasValue)
            {
                TintSourceBitmap(result, tintColor.Value);
            }

            return result;
        }

        private unsafe void TintSourceBitmap(Bitmap source, Color tintColor)
        {
            var data = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            try
            {
                uint* pixels = (uint*)data.Scan0.ToPointer();
                int pixelCount = source.Width * source.Height;
                for (int i = 0; i < pixelCount; i++)
                {
                    uint pixel = pixels[i];
                    uint a = (pixel >> 24);
                    uint r = ((pixel & 0xff0000) >> 16);
                    uint g = ((pixel & 0xff00) >> 8);
                    uint b = (pixel & 0xff);
                    if (a > 0)
                    {
                        r = (r * tintColor.R) / 255;
                        g = (g * tintColor.G) / 255;
                        b = (b * tintColor.B) / 255;
                        pixel = (a << 24) | (r << 16) | (g << 8) | b;
                        pixels[i] = pixel;
                    }
                }
            }
            finally
            {
                source.UnlockBits(data);
            }
        }
    }
}
