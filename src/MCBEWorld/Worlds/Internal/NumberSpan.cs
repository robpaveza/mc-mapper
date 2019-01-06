using System;
using System.Collections.Generic;
using System.Linq;

namespace MCBEWorld.Worlds.Internal
{
    /// <summary>
    /// Supports reading a bit-packed subchunk byte array into palette indices.
    /// </summary>
    internal class NumberSpan
    {
        private uint n_;
        private PalettedCompressionType type_;

        public NumberSpan(uint n, PalettedCompressionType type)
        {
            n_ = n;
            type_ = type;
        }

        private const uint PAL1_WIDTH = 1;
        private const int PAL1_FIRST_OFFSET = 0;
        private static readonly uint[] PAL1_MASKS = ConstructPal1Masks().ToArray();

        private static IEnumerable<uint> ConstructPal1Masks()
        {
            var accum = 1u;
            for (int i = 0; i < 32; i++)
            {
                yield return accum;
                accum <<= 1;
            }
        }

        private const uint PAL2_WIDTH = 2;
        private const int PAL2_FIRST_OFFSET = 0;
        private const uint PAL2_15 = 0xc0000000;
        private const uint PAL2_14 = 0x30000000;
        private const uint PAL2_13 = 0x0c000000;
        private const uint PAL2_12 = 0x03000000;
        private const uint PAL2_11 = 0x00c00000;
        private const uint PAL2_10 = 0x00300000;
        private const uint PAL2_9 = 0x000c0000;
        private const uint PAL2_8 = 0x00030000;
        private const uint PAL2_7 = 0x0000c000;
        private const uint PAL2_6 = 0x00003000;
        private const uint PAL2_5 = 0x00000c00;
        private const uint PAL2_4 = 0x00000300;
        private const uint PAL2_3 = 0x000000c0;
        private const uint PAL2_2 = 0x00000030;
        private const uint PAL2_1 = 0x0000000c;
        private const uint PAL2_0 = 0x00000003;
        private static readonly uint[] PAL2_MASKS = new[]
        {
            PAL2_0, PAL2_1, PAL2_2, PAL2_3, PAL2_4, PAL2_5, PAL2_6, PAL2_7,
            PAL2_8, PAL2_9, PAL2_10, PAL2_11, PAL2_12, PAL2_13, PAL2_14, PAL2_15,
        };

        private const uint PAL3_WIDTH = 3;
        private const int PAL3_FIRST_OFFSET = 0;
        private const uint PAL3_9 = 0x38000000;
        private const uint PAL3_8 = 0x07000000;
        private const uint PAL3_7 = 0x00e00000;
        private const uint PAL3_6 = 0x001c0000;
        private const uint PAL3_5 = 0x00038000;
        private const uint PAL3_4 = 0x00007000;
        private const uint PAL3_3 = 0x00000e00;
        private const uint PAL3_2 = 0x000001c0;
        private const uint PAL3_1 = 0x00000038;
        private const uint PAL3_0 = 0x00000007;
        private static readonly uint[] PAL3_MASKS = new[]
        {
            PAL3_0, PAL3_1, PAL3_2, PAL3_3, PAL3_4,
            PAL3_5, PAL3_6, PAL3_7, PAL3_8, PAL3_9,
        };

        private const uint PAL4_WIDTH = 4;
        private const int PAL4_FIRST_OFFSET = 0;
        private const uint PAL4_7 = 0xf0000000;
        private const uint PAL4_6 = 0x0f000000;
        private const uint PAL4_5 = 0x00f00000;
        private const uint PAL4_4 = 0x000f0000;
        private const uint PAL4_3 = 0x0000f000;
        private const uint PAL4_2 = 0x00000f00;
        private const uint PAL4_1 = 0x000000f0;
        private const uint PAL4_0 = 0x0000000f;
        private static readonly uint[] PAL4_MASKS = new[]
        {
            PAL4_0, PAL4_1, PAL4_2, PAL4_3,
            PAL4_4, PAL4_5, PAL4_6, PAL4_7,
        };

        private const uint PAL5_WIDTH = 5;
        private const int PAL5_FIRST_OFFSET = 0;
        private const uint PAL5_5 = 0x3e000000;
        private const uint PAL5_4 = 0x01f00000;
        private const uint PAL5_3 = 0x000f8000;
        private const uint PAL5_2 = 0x00007c00;
        private const uint PAL5_1 = 0x000003e0;
        private const uint PAL5_0 = 0x0000001f;
        private static readonly uint[] PAL5_MASKS = new[]
        {
            PAL5_0, PAL5_1, PAL5_2, PAL5_3, PAL5_4, PAL5_5,
        };

        private const uint PAL6_WIDTH = 6;
        private const int PAL6_FIRST_OFFSET = 0;
        private const uint PAL6_4 = 0x3f000000;
        private const uint PAL6_3 = 0x00fc0000;
        private const uint PAL6_2 = 0x0003f000;
        private const uint PAL6_1 = 0x00000fc0;
        private const uint PAL6_0 = 0x0000003f;
        private static readonly uint[] PAL6_MASKS = new[]
        {
            PAL6_0, PAL6_1, PAL6_2, PAL6_3, PAL6_4,
        };

        private const uint PAL8_WIDTH = 8;
        private const int PAL8_FIRST_OFFSET = 0;
        private const uint PAL8_3 = 0xff000000;
        private const uint PAL8_2 = 0x00ff0000;
        private const uint PAL8_1 = 0x0000ff00;
        private const uint PAL8_0 = 0x000000ff;
        private static readonly uint[] PAL8_MASKS = new[]
        {
            PAL8_0, PAL8_1, PAL8_2, PAL8_3,
        };

        private const uint PAL16_WIDTH = 16;
        private const int PAL16_FIRST_OFFSET = 0;
        private const uint PAL16_1 = 0xffff0000;
        private const uint PAL16_0 = 0x0000ffff;
        private static readonly uint[] PAL16_MASKS = new[]
        {
            PAL16_0,
            PAL16_1,
        };

        private static bool GetIndexReaderParameters(PalettedCompressionType type, out uint[] masks, out uint width, out uint count, out int initialOffset)
        {
            switch (type)
            {
                case PalettedCompressionType.Paletted1:
                    masks = PAL1_MASKS;
                    width = PAL1_WIDTH;
                    initialOffset = PAL1_FIRST_OFFSET;
                    count = 32;
                    return true;

                case PalettedCompressionType.Paletted2:
                    masks = PAL2_MASKS;
                    width = PAL2_WIDTH;
                    initialOffset = PAL2_FIRST_OFFSET;
                    count = 16;
                    return true;

                case PalettedCompressionType.Paletted3:
                    masks = PAL3_MASKS;
                    width = PAL3_WIDTH;
                    initialOffset = PAL3_FIRST_OFFSET;
                    count = 10;
                    return true;

                case PalettedCompressionType.Paletted4:
                    masks = PAL4_MASKS;
                    width = PAL4_WIDTH;
                    initialOffset = PAL4_FIRST_OFFSET;
                    count = 8;
                    return true;

                case PalettedCompressionType.Paletted5:
                    masks = PAL5_MASKS;
                    width = PAL5_WIDTH;
                    initialOffset = PAL5_FIRST_OFFSET;
                    count = 6;
                    return true;

                case PalettedCompressionType.Paletted6:
                    masks = PAL6_MASKS;
                    width = PAL6_WIDTH;
                    initialOffset = PAL6_FIRST_OFFSET;
                    count = 5;
                    return true;

                case PalettedCompressionType.Paletted8:
                    masks = PAL8_MASKS;
                    width = PAL8_WIDTH;
                    initialOffset = PAL8_FIRST_OFFSET;
                    count = 4;
                    return true;

                case PalettedCompressionType.Paletted16:
                    masks = PAL16_MASKS;
                    width = PAL16_WIDTH;
                    initialOffset = PAL16_FIRST_OFFSET;
                    count = 2;
                    return true;

                default:
                    masks = null;
                    width = 0;
                    initialOffset = 0;
                    count = 0;
                    return false;
            }
        }

        public IEnumerable<uint> GetIndices(uint max = 32)
        {
            if (GetIndexReaderParameters(type_, out uint[] masks, out uint width, out uint count, out int initialOffset))
            {
                max = Math.Min(count, max);
                for (int index = 0; index < max; index++)
                {
                    uint mask = masks[index];
                    uint valueBeforeTransform = n_ & mask;
                    uint result = valueBeforeTransform >> initialOffset;
                    initialOffset += unchecked((int)width);
                    yield return result;
                }
            }
        }
    }
}
