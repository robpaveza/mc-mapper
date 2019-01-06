using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBERenderer.Configuration
{
    public struct ColorRgb
    {
        public ColorRgb(int r, int g, int b)
        {
            R = r;
            G = g;
            B = b;
        }

        public static ColorRgb From0x(string s)
        {
            var r = int.Parse(s.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            var g = int.Parse(s.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            var b = int.Parse(s.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);

            return new ColorRgb(r, g, b);
        }

        public ColorRgb Add(int num)
        {
            int original = (R << 16) | (G << 8) | B;
            int result = original + num;
            return new ColorRgb(
                /* R: */ (result & 0xff0000) >> 16,
                /* G: */ (result & 0xff00) >> 8,
                /* B: */ (result & 0xff)
                );
        }

        public int R { get; }
        public int G { get; }
        public int B { get; }

        public Rgba32 ToRgba32(byte alphaValue = 255)
        {
            return new Rgba32((byte)R, (byte)G, (byte)B, alphaValue);
        }

        public string ToHex()
        {
            return $"0x{R:x2}{G:x2}{B:x2}";
        }

        public string ToHash()
        {
            return $"#{R:x2}{G:x2}{B:x2}";
        }
    }
}
