using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCBERenderer.Explorer
{
    /// <summary>
    /// Composites a Region into a flat tile.  A region is a 1024x1024-block, or 64x64-chunk, area.  Biomes are a flat, 256-byte array first increasing by 
    /// X-coordinate and then by Z-coordinate, which maps cleanly to X-Y for bitmaps.
    /// </summary>
    internal sealed class BiomeTileCompositor
    {
        private const ulong UNEXPLORED_CHUNK_VALUE = 0xffffffffffffffff;
        private static readonly byte[] EMPTY_BIOME_MAP = Enumerable.Range(0, 256).Select(_ => (byte)255).ToArray();

        /**
         * Suppose you have a 16-element array of 2x2 bitmaps encoded also as just a 4-element array:
         * outer array = [a, b, c, d, e, null, null, f, g, h, i, null, j, k, l, m];
         * a = [1, 2, 3, 4], b = [5, 6, 7, 8], c = [9, 10, 11, 12], etc.
         * and you want to composite these such that the output bitmap is a 4x4 tile of the 2x2 bitmaps:
         *  1   2   5   6   9   10  13  14
         *    a       b       c        d
         *  3   4   7   8   11  12  15  16
         *  17  18  255 255 255 255 20  21
         *    e       null    null     f
         *  19  20  255 255 255 255 22  23
         *  24  25  28  29  32  33  255 255
         *    g       h       i      null
         *  26  27  30  31  34  35  255 255
         *  36  37  40  41  44  45  48  49
         *    j       k       l       m
         *  38  39  42  43  46  47  50  51
         *  In other words, for each source buffer:
         *   destination index starts at row +0, 2, 4, or 6
         */
        /**
         * Reading through this sequentially by origin would be something like:
         * const DEST_ROW_WIDTH = 8;
         * const SRC_ROW_WIDTH = 2;
         * for (int srcBufferIndex = 0; srcBufferIndex < 
         */

        public static unsafe byte[] CompositeChunksIntoRegion(byte[][] biomeChunks)
        {
            byte[] result = new byte[16 * 16 * 64 * 64]; // 1mb array

            // first try: appeared to write image backwards and jagged

            fixed (byte* pOut = result)
            {
                ulong* MAX_PTR = (ulong*)(pOut + 1048576);
                int destRow = 0;
                int destCol = 0; // max of 128 columns
                // Treat as a state machine: 
                for (ulong* dest = (ulong*)pOut; dest < MAX_PTR; dest++)
                {
                    // start of turn: determine which to source from
                    // there are 16 rows per chunk
                    int sourceBufferRow = destRow >> 4;
                    // there are 2 source columns per chunk
                    int sourceBufferCol = destCol >> 1;
                    int sourceBufferIndex = (sourceBufferRow << 6) + sourceBufferCol;
                    byte[] sourceBuffer = biomeChunks[sourceBufferIndex] ?? EMPTY_BIOME_MAP;
                    // Get offset into source buffer 
                    int sourceRow = destRow & 0xf;
                    int sourceCol = destCol & 0x1;
                    int sourceOffset = (sourceRow << 4) | (sourceCol << 3);
                    ulong srcValue = BitConverter.ToUInt64(sourceBuffer, sourceOffset);

                    // now write
                    *dest = srcValue;

                    // end of turn:
                    destCol++;
                    if ((destCol & 0x80) > 0)
                    {
                        destRow++;
                        destCol = 0;
                    }
                }
            }



            return result;
        }
    }
}
