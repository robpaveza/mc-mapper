using MCBEWorld.Utility;
using System;
using System.Diagnostics;

namespace MCBEWorld.Worlds
{
    /// <summary>
    /// Represents an area of 64x64 chunks, aligned to 64-value areas, to store whether 
    /// the chunk at a given (x, y) position has data.
    /// </summary>
    [DebuggerDisplay("Region at ({x_}, {z_})")]
    public class Region : IEquatable<Region>
    {
        private long[] bitmap_;
        private int x_;
        private int z_;

        public Region(int regionX, int regionZ)
        {
            bitmap_ = new long[64];
            x_ = regionX;
            z_ = regionZ;
        }

        public bool HasData(int chunkX, int chunkZ)
        {
            int x = chunkX & 63;
            int z = chunkZ & 63;

            long bit = 1L << x;
            long value = bitmap_[z];
            return (value & bit) != 0;
        }

        public void SetHasData(int chunkX, int chunkZ)
        {
            /**
             * Any given item in bitmap_ stores a single Z coordinate.  To find the
             * necessary bit, shift 1 << (chunkX & 63).  For example, (0, 5) goes
             * into bitmap_[5] and sets 1 << 0, which is the LSB.
             */
            int x = chunkX & 63;
            int z = chunkZ & 63;

            long bit = 1L << x;
            long value = bitmap_[z];
            value |= bit;
            bitmap_[z] = value;
        }

        // Regions are constrained to +/- 12256, a 14-bit value.  Use outside of these values is
        // permitted, but undetermined behavior may occur.  This allows for room for up to 16 dimensions
        // if we choose to add them later.
        public override int GetHashCode()
        {
            return CalculateKey(x_, z_);
        }

        public override bool Equals(object obj)
        {
            Region other = obj as Region;
            if (other == null)
            {
                return false;
            }

            return Equals(other);
        }

        public bool Equals(Region other)
        {
            return x_ == other.x_ && z_ == other.z_;
        }

        public override string ToString()
        {
            return $"R({x_}, {z_})";
        }

        internal static int CalculateKey(int regionX, int regionZ)
        {
            return ((regionX & 0x3fff) << 14) | (regionZ & 0x3fff);
        }

        internal static int CalculateKeyFromChunk(int chunkX, int chunkZ)
        {
            int regionX = chunkX >> 6;
            int regionZ = chunkZ >> 6;

            return ((regionX & 0x3fff) << 14) | (regionZ & 0x3fff);
        }

        internal static Region CreateContainingChunk(int chunkX, int chunkZ)
        {
            int regionX = chunkX >> 6;
            int regionZ = chunkZ >> 6;

            return new Region(regionX, regionZ);
        }

        public string SerializeToString(BinarySerializationFormat format = BinarySerializationFormat.Base64)
        {
            byte[] buffer = new byte[8 * 64];
            Buffer.BlockCopy(bitmap_, 0, buffer, 0, 8 * 64);

            return BinaryFormatter.Format(format, buffer);
        }

        public int X => x_;
        public int Z => z_;
        public int Key => GetHashCode();
    }
}
