using System;
using System.Collections.Generic;
using System.Text;

namespace MCBEWorld.Worlds.Internal
{
    internal class DbKeyDescriptor
    {
        public DbKeyDescriptor(int chunkX, int chunkZ, Dimension dimension, DbKeyType keyType)
        {
            ChunkX = chunkX;
            SubchunkIndex = -1;
            ChunkZ = chunkZ;
            Dimension = dimension;
            KeyType = keyType;
        }

        public DbKeyDescriptor(int chunkX, int chunkY, int chunkZ, Dimension dimension, DbKeyType keyType)
        {
            ChunkX = chunkX;
            SubchunkIndex = chunkY;
            ChunkZ = chunkZ;
            Dimension = dimension;
            KeyType = keyType;
        }

        /// <summary>
        /// Gets the X-coordinate (chunk)
        /// </summary>
        public int ChunkX { get; }
        /// <summary>
        /// Gets the Y-coordinate (subchunk index), 0-15 inclusive.  Will return -1 if 
        /// this is not a subchunk key descriptor.
        /// </summary>
        public int SubchunkIndex { get; }
        /// <summary>
        /// Gets the Z-coordinate (chunk).
        /// </summary>
        public int ChunkZ { get; }

        /// <summary>
        /// Gets the key type.
        /// </summary>
        public DbKeyType KeyType { get; }
        /// <summary>
        /// Gets the related dimension.
        /// </summary>
        public Dimension Dimension { get; }

        public override string ToString()
        {
            if (SubchunkIndex != -1)
            {
                return $"({ChunkX}, {SubchunkIndex}, {ChunkZ}) - {Dimension.Name} - {KeyType}";
            }
            return $"({ChunkX}, {ChunkZ}) - {Dimension.Name} - {KeyType}";
        }

        public Coordinates ToCoordinates()
        {
            return new Coordinates(ChunkX, KeyType == DbKeyType.SubChunkPrefix ? SubchunkIndex : -1, ChunkZ, Dimension, CoordinatesType.Chunk);
        }
    }
}
