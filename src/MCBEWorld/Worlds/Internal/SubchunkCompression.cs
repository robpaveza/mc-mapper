using MCBEWorld.Data;
using MCBEWorld.Nbt;
using MCBEWorld.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MCBEWorld.Worlds.Internal
{
    internal class SubchunkCompression
    {
        private const string BLOCK_PALETTE_NAME_KEY = "name";
        private const string BLOCK_PALETTE_DATA_KEY = "val";

        private class BlockStorageEntry
        {
            public bool NetworkMode;
            public PalettedCompressionType CompressionType;
            public int OffsetToPalette;
            public int OffsetToIndices;
        }

        private const int BLOCKS_PER_SUBCHUNK = 16 * 16 * 16; // 4096
        private const int BLOCKSTORAGE_SIZE_BYTES_PAL1 = 128 * 4; // 128 uint32s
        private const int BLOCKSTORAGE_SIZE_BYTES_PAL2 = 256 * 4;
        private const int BLOCKSTORAGE_SIZE_BYTES_PAL3 = 410 * 4;
        private const int BLOCKSTORAGE_SIZE_BYTES_PAL4 = 512 * 4;
        private const int BLOCKSTORAGE_SIZE_BYTES_PAL5 = 683 * 4;
        private const int BLOCKSTORAGE_SIZE_BYTES_PAL6 = 820 * 4;
        private const int BLOCKSTORAGE_SIZE_BYTES_PAL8 = 1024 * 4;
        private const int BLOCKSTORAGE_SIZE_BYTES_PAL16 = 2048 * 4;
        private const int BLOCKSTORAGE_PADDING_PAL1 = 0;
        private const int BLOCKSTORAGE_PADDING_PAL2 = 0;
        private const int BLOCKSTORAGE_PADDING_PAL3 = 0;
        private const int BLOCKSTORAGE_PADDING_PAL4 = 0;
        private const int BLOCKSTORAGE_PADDING_PAL5 = 0;
        private const int BLOCKSTORAGE_PADDING_PAL6 = 0;
        private const int BLOCKSTORAGE_PADDING_PAL8 = 0;
        private const int BLOCKSTORAGE_PADDING_PAL16 = 0;

        private DataReader source_;
        private PalettedCompressionType type_;
        private int version_;
        private int offsetToPalette_;
        private List<BlockStorageEntry> blockStorages = new List<BlockStorageEntry>();
        private Dictionary<BlockPaletteEntry, uint> eagerBlockPalette;
        private uint[] loadedIndices;

        public SubchunkCompression(DataReader sourceData)
        {
            source_ = sourceData;
            byte version = sourceData.ReadByte();
            if (version > 8)
            {
                throw new InvalidDataException($"Invalid version {version}; expected 0-8");
            }
            version_ = version;

            if (version_ == 8)
            {
                int blockStorageCount = sourceData.ReadByte();
                for (int i = 0; i < 1; i++)
                {
                    byte type = sourceData.ReadByte();
                    bool networkMode = ((type & 0x1) == 1);

                    PalettedCompressionType compressionType = (PalettedCompressionType)((type & (byte)PalettedCompressionType.BIT_MASK) >> 1);
                    type_ = compressionType;

                    if (GetStorageParameters(type_, out int size, out int padding))
                    {
                        offsetToPalette_ = 3 + // version, block storage, compression type
                            size + padding;
                    }
                    else
                    {
                        throw new InvalidDataException($"Unsupported compression type value {type_}");
                    }
                }
            }
            else if (version == 1)
            {
                byte type = sourceData.ReadByte();
                bool networkMode = ((type & 0x1) == 1);

                PalettedCompressionType compressionType = (PalettedCompressionType)((type & (byte)PalettedCompressionType.BIT_MASK) >> 1);
                type_ = compressionType;

                if (GetStorageParameters(type_, out int size, out int padding))
                {
                    offsetToPalette_ = 2 + // version, compression type
                        size + padding;
                }
                else
                {
                    throw new InvalidDataException($"Unsupported compression type value {type_}");
                }
            }
            else
            {
                eagerBlockPalette = new Dictionary<BlockPaletteEntry, uint>();
                uint blockdefIndex = 0;
                byte[] blocks = sourceData.ReadByteArray(BLOCKS_PER_SUBCHUNK);
                byte[] data = sourceData.ReadByteArray(BLOCKS_PER_SUBCHUNK / 2);
                uint[] result = new uint[BLOCKS_PER_SUBCHUNK];

                for (int i = 0; i < BLOCKS_PER_SUBCHUNK; i++)
                {
                    byte block = blocks[i];
                    byte datum = data[i / 2];
                    if ((i & 1) == 1)
                    {
                        datum = unchecked((byte)(datum & 0xf));
                    }
                    else
                    {
                        datum = unchecked((byte)((datum & 0xf0) >> 4));
                    }

                    var def = HistoricalBlockData.Get(block, datum);
                    if (eagerBlockPalette.TryGetValue(def, out uint index))
                    {
                        result[i] = index;
                    }
                    else
                    {
                        eagerBlockPalette.Add(def, blockdefIndex);
                        result[i] = blockdefIndex;
                        blockdefIndex++;
                    }
                }

                loadedIndices = result;
            }
        }

        private static bool GetStorageParameters(PalettedCompressionType type, out int size, out int padding)
        {
            switch (type)
            {
                case PalettedCompressionType.Paletted1:
                    size = BLOCKSTORAGE_SIZE_BYTES_PAL1;
                    padding = BLOCKSTORAGE_PADDING_PAL1;
                    return true;

                case PalettedCompressionType.Paletted2:
                    size = BLOCKSTORAGE_SIZE_BYTES_PAL2;
                    padding = BLOCKSTORAGE_PADDING_PAL2;
                    return true;

                case PalettedCompressionType.Paletted3:
                    size = BLOCKSTORAGE_SIZE_BYTES_PAL3;
                    padding = BLOCKSTORAGE_PADDING_PAL3;
                    return true;

                case PalettedCompressionType.Paletted4:
                    size = BLOCKSTORAGE_SIZE_BYTES_PAL4;
                    padding = BLOCKSTORAGE_PADDING_PAL4;
                    return true;

                case PalettedCompressionType.Paletted5:
                    size = BLOCKSTORAGE_SIZE_BYTES_PAL5;
                    padding = BLOCKSTORAGE_PADDING_PAL5;
                    return true;

                case PalettedCompressionType.Paletted6:
                    size = BLOCKSTORAGE_SIZE_BYTES_PAL6;
                    padding = BLOCKSTORAGE_PADDING_PAL6;
                    return true;

                case PalettedCompressionType.Paletted8:
                    size = BLOCKSTORAGE_SIZE_BYTES_PAL8;
                    padding = BLOCKSTORAGE_PADDING_PAL8;
                    return true;

                case PalettedCompressionType.Paletted16:
                    size = BLOCKSTORAGE_SIZE_BYTES_PAL16;
                    padding = BLOCKSTORAGE_PADDING_PAL16;
                    return true;

                default:
                    size = 0;
                    padding = 0;
                    return false;
            }
        }

        public IEnumerable<uint> GetIndicesIntoPalette()
        {
            if (loadedIndices != null)
            {
                return loadedIndices;
            }

            loadedIndices = CalculatePalettedIndices().ToArray();
            return loadedIndices;
        }

        private IEnumerable<uint> CalculatePalettedIndices()
        {
            source_.GoToStart();
            source_.Seek(3); // go to 3

            uint next = source_.ReadUInt32();
            var span = new NumberSpan(next, type_);
            for (int i = 0; i < BLOCKS_PER_SUBCHUNK;)
            {
                foreach (var index in span.GetIndices(unchecked((uint)(BLOCKS_PER_SUBCHUNK - i))))
                {
                    yield return index;
                    i++;
                }
                next = source_.ReadUInt32();
                span = new NumberSpan(next, type_);
            }
        }

        public IEnumerable<BlockPaletteEntry> GetPalette()
        {
            if (eagerBlockPalette != null)
            {
                return eagerBlockPalette.Keys;
            }

            return CalculatePalette();
        }

        private IEnumerable<BlockPaletteEntry> CalculatePalette()
        {
            source_.GoToStart();
            source_.Seek(offsetToPalette_);

            int paletteSize = source_.ReadInt32();
            while (!source_.AtEnd)
            {
                if (TryGetNextBlockDefinition(source_, out BlockPaletteEntry paletteEntry))
                {
                    yield return paletteEntry;
                }
            }
        }

        private static bool TryGetNextBlockDefinition(DataReader reader, out BlockPaletteEntry result)
        {
            try
            {
                var compound = reader.ReadNbtTag()?.AsDictionary();
                if (compound == null)
                {
                    result = null;
                    return false;
                }

                string name = compound[BLOCK_PALETTE_NAME_KEY].As<string>().Value;
                short value = compound[BLOCK_PALETTE_DATA_KEY].As<short>().Value;

                result = new BlockPaletteEntry(name, value);
                return true;
            }
            catch (Exception ex)
            {
                result = null;
                return false;
            }
        }
    }
}
