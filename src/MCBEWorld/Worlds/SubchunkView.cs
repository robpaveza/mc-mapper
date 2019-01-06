using System;
using System.Collections.Generic;
using System.Text;

namespace MCBEWorld.Worlds
{
    public sealed class SubchunkView
    {
        private uint[] indices_;
        private BlockPaletteEntry[] palette_;

        public SubchunkView(uint[] indices, BlockPaletteEntry[] palette)
        {
            indices_ = indices;
            palette_ = palette;
        }

        public IReadOnlyList<BlockPaletteEntry> GetPalette()
        {
            return palette_;
        }

        public int PaletteLength => palette_.Length;

        public uint this[int x, int y, int z]
        {
            get
            {
                return GetAt(x, y, z);
            }
        }

        public uint GetAt(int x, int y, int z)
        {
            if (x < 0 || x > 15)
            {
                throw new ArgumentOutOfRangeException("x");
            }
            if (y < 0 || y > 15)
            {
                throw new ArgumentOutOfRangeException("y");
            }
            if (z < 0 || z > 15)
            {
                throw new ArgumentOutOfRangeException("z");
            }

            return indices_[x * 256 + z * 16 + y];
        }

        public uint[] GetXZPlane(uint y)
        {
            if (y > 15)
            {
                throw new ArgumentOutOfRangeException("y");
            }

            uint[] plane = new uint[256];
            uint curIndex = y;
            for (int i = 0; i < 256; i++)
            {
                plane[i] = indices_[curIndex];
                curIndex += 256;
                if ((i & 15) == 15)
                {
                    curIndex += 16;
                }
                curIndex &= 0xfff; // mask to 4095
            }

            return plane;
        }
    }
}
