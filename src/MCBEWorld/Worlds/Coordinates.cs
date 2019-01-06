using System;
using System.Collections.Generic;
using System.Text;

namespace MCBEWorld.Worlds
{
    public class Coordinates
    {
        public Coordinates(int x, int y, int z, Dimension dimension, CoordinatesType type)
        {
            X = x;
            Y = y;
            Z = z;
            Dimension = dimension;
            Type = type;
        }

        public int X { get; }
        public int Y { get; }
        public int Z { get; }
        public CoordinatesType Type { get; }
        public Dimension Dimension { get; }
    }

    public enum CoordinatesType
    {
        /// <summary>
        /// The block location in the world
        /// </summary>
        Block,
        /// <summary>
        /// The chunk coordinates.  To get the block coordinates from this, multiply
        /// the chunk value by 16
        /// </summary>
        Chunk,
        /// <summary>
        /// The region coordinates.  Regions are not used in Minecraft Bedrock Edition, but
        /// we use it to store a bitmap of all discovered areas.  Regions are 64x64 chunk areas,
        /// so to get the chunk coordinates, multiply this chunk value by 64; or to get the
        /// block coordinates, multiply this by 1024.
        /// </summary>
        Region,
    }
}
