using MCBEWorld.Nbt;
using MCBEWorld.Utility;
using MCBEWorld.Worlds.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MCBEWorld.Worlds
{
    [DebuggerDisplay("chunk: {Dimension.Name}, ({X}, {Z})")]
    public sealed class ChunkView
    {
        private byte[] biomes_;
        private byte[] heightMap_;
        private byte[][] subchunkData_;
        private byte[] version_;
        private byte[] entity_;
        private byte[] blockEntity_;
        private byte[] blockExtraData_;
        private byte[] pendingTicks_;
        private byte[] finalizedState_;
        private byte[] borderBlocks_;
        private byte[] hardcodedSpawners_;

        internal ChunkView(
            Dimension dimension,
            int x,
            int z,
            byte[] biomes = null,
            byte[] heightMap = null,
            byte[][] subchunkData = null,
            byte[] version = null,
            byte[] entities = null,
            byte[] blockEntities = null,
            byte[] blockExtraData = null,
            byte[] pendingTicks = null,
            byte[] finalizedState = null,
            byte[] borderBlocks = null,
            byte[] hardcodedSpawners = null
        )
        {
            Dimension = dimension;
            X = x;
            Z = z;

            biomes_ = biomes;
            heightMap_ = heightMap;
            subchunkData_ = subchunkData;
            version_ = version;
            entity_ = entities;
            blockEntity_ = blockEntities;
            blockExtraData_ = blockExtraData;
            pendingTicks_ = pendingTicks;
            finalizedState_ = finalizedState;
            borderBlocks_ = borderBlocks;
            hardcodedSpawners_ = hardcodedSpawners;
        }

        public Dimension Dimension { get; }
        public int X { get; }
        public int Z { get; }

        public bool HasBiomeData => biomes_ != null;
        public bool HasHeightmap => heightMap_ != null;
        public bool HasSubchunks => subchunkData_ != null;
        public bool HasVersion => version_ != null;
        public bool HasEntities => entity_ != null;
        public bool HasBlockEntities => blockEntity_ != null;
        public bool HasExtraBlockData => blockExtraData_ != null;
        public bool HasPendingTicksList => pendingTicks_ != null;
        public bool HasFinalizedState => finalizedState_ != null;
        public bool HasBorderBlocks => borderBlocks_ != null;
        public bool HasHardcodedSpawners => hardcodedSpawners_ != null;

        public SubchunkView GetSubchunkView(int yIndex)
        {
            if (subchunkData_ == null)
            {
                throw new InvalidOperationException("Subchunks were not loaded into this chunk view.");
            }
            
            if (yIndex < 0 || yIndex > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(yIndex), "Expected y-index of 0-15");
            }

            if (yIndex >= subchunkData_.Length)
            {
                return null;
            }

            byte[] subchunk = subchunkData_[yIndex];
            using (var ms = new MemoryStream(subchunk))
            using (var br = new BinaryReader(ms))
            {
                var reader = new DataReader(ms, br);
                var decompressor = new SubchunkCompression(reader);
                var indices = decompressor.GetIndicesIntoPalette().ToArray();
                var palette = decompressor.GetPalette().ToArray();

                return new SubchunkView(indices, palette);
            }
        }

        public byte[] GetBiomesArea()
        {
            if (biomes_ == null)
            {
                throw new InvalidOperationException("Biomes were not loaded into this chunk view, or biome data are not present.");
            }

            return biomes_;
        }

        public IReadOnlyList<INbtDictionary> GetEntities()
        {
            if (entity_ == null)
            {
                throw new InvalidOperationException("Entities were not loaded into this chunk view, or no entities are present.");
            }

            return DictionaryTagListFromBuffer(entity_);
        }

        public IReadOnlyList<INbtDictionary> GetBlockEntities()
        {
            if (blockEntity_ == null)
            {
                throw new InvalidOperationException("Block entities were not loaded into this chunk view, or no block entities are present.");
            }

            return DictionaryTagListFromBuffer(blockEntity_);
        }

        public IReadOnlyList<INbtDictionary> GetExtraBlockData()
        {
            if (blockExtraData_ == null)
            {
                throw new InvalidOperationException("Block extra data are not loaded into this chunk view, or there were no extra data present.");
            }

            return DictionaryTagListFromBuffer(blockExtraData_);
        }

        public INbtDictionary GetPendingTicksList()
        {
            if (pendingTicks_ == null)
            {
                throw new InvalidOperationException("Pending ticks have not been loaded into this chunk view, or there were none present.");
            }

            return DictionaryTagFromBuffer(pendingTicks_);
        }

        private static INbtDictionary DictionaryTagFromBuffer(byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer))
            {
                var reader = NbtReader.Create(ms);
                if (!reader.MoveNext())
                {
                    return null;
                }

                return reader.CurrentValue.AsDictionary();
            }
        }

        private static List<INbtDictionary> DictionaryTagListFromBuffer(byte[] buffer)
        {
            List<INbtDictionary> result = new List<INbtDictionary>();
            using (var ms = new MemoryStream(buffer))
            {
                while (ms.Position < ms.Length)
                {
                    var reader = NbtReader.Create(ms);
                    if (!reader.MoveNext())
                    {
                        break;
                    }

                    result.Add(reader.CurrentValue.AsDictionary());
                }
            }

            return result;
        }

        public int Version
        {
            get
            {
                if (version_ == null)
                {
                    throw new InvalidOperationException("Version not loaded into this chunk view or it was unavailable.");
                }

                return version_[0];
            }
        }
    }
}
