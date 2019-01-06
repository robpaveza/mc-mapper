using MCBEWorld.LevelDb;
using MCBEWorld.Nbt;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MCBEWorld.Worlds.Internal
{
    internal sealed class DbBroker : IDisposable
    {
        private DB db_;
        private string path_;

        public DbBroker(string dbPath)
        {
            path_ = dbPath;
        }

        private void EnsureDbIsOpen()
        {
            if (db_ != null)
            {
                return;
            }

            var opts = new Options()
            {
                CompressionLevel = CompressionLevel.Zlib,
                CreateIfMissing = false,
            };
            db_ = new DB(opts, path_);
        }

        public IEnumerable<CoordinatesOrNamedKey> EnumerateDatabase()
        {
            EnsureDbIsOpen();

            using (var iter = db_.CreateIterator())
            {
                iter.SeekToFirst();
                SortedList<int, Region> overworld = new SortedList<int, Region>();
                SortedList<int, Region> nether = new SortedList<int, Region>();
                SortedList<int, Region> theEnd = new SortedList<int, Region>();

                while (iter.IsValid())
                {
                    var key = iter.Key();
                    if (KeyDescriptorFromKey(key, out DbKeyDescriptor descriptor))
                    {
                        yield return new CoordinatesOrNamedKey(descriptor.ToCoordinates());
                    }
                    else
                    {
                        yield return new CoordinatesOrNamedKey(iter.KeyAsString());
                    }

                    iter.Next();
                }
            }
        }

        public INbtStream GetNbtEntry(string key)
        {
            EnsureDbIsOpen();
            return NbtStreamFromBits(db_.Get(Encoding.ASCII.GetBytes(key)));
        }

        public INbtStream GetNbtEntry(byte[] key)
        {
            EnsureDbIsOpen();
            return NbtStreamFromBits(db_.Get(key));
        }
        private static INbtStream NbtStreamFromBits(byte[] source)
        {
            if (source == null)
            {
                return null;
            }

            var ms = new MemoryStream(source);
            return new NbtStream(ms);
        }

        public unsafe byte[] Query(Dimension dimension, int chunkX, int chunkZ, DbKeyType dataType)
        {
            EnsureDbIsOpen();

            byte[] key;
            if (dimension.Value == 0) // overworld
            {
                key = new byte[9];
                fixed (byte* pKey = key)
                {
                    key[8] = (byte)dataType;
                    *(int*)(pKey) = chunkX;
                    *(((int*)(pKey)) + 1) = chunkZ;
                }
            }
            else
            {
                key = new byte[13];
                fixed (byte* pKey = key)
                {
                    key[12] = (byte)dataType;
                    *(int*)(pKey) = chunkX;
                    *(((int*)(pKey)) + 1) = chunkZ;
                    *(((int*)(pKey)) + 2) = dimension.Value;
                }
            }

            return db_.Get(key);
        }

        public unsafe byte[] QuerySubchunkData(Dimension dimension, int chunkX, int subchunkYIndex, int chunkZ)
        {
            Debug.Assert(subchunkYIndex >= 0 && subchunkYIndex < 16);

            EnsureDbIsOpen();

            byte[] key;
            if (dimension.Value == 0) // overworld
            {
                key = new byte[10];
                fixed (byte* pKey = key)
                {
                    key[8] = (byte)DbKeyType.SubChunkPrefix;
                    key[9] = (byte)subchunkYIndex;
                    *(int*)(pKey) = chunkX;
                    *(((int*)(pKey)) + 1) = chunkZ;
                }
            }
            else
            {
                key = new byte[14];
                fixed (byte* pKey = key)
                {
                    key[12] = (byte)DbKeyType.SubChunkPrefix;
                    key[13] = (byte)subchunkYIndex;
                    *(int*)(pKey) = chunkX;
                    *(((int*)(pKey)) + 1) = chunkZ;
                    *(((int*)(pKey)) + 2) = dimension.Value;
                }
            }

            return db_.Get(key);
        }

        private static bool KeyDescriptorFromKey(byte[] key, out DbKeyDescriptor result)
        {
            result = null;

            if (key.Length < 9)
            {
                return false;
            }

            // Opportunistic bail: Don't examine any areas in The Far Lands, which start at
            // +/- 12,550,824.  These are found in chunks at +/- 784,426,  which means that
            // looking for the presence of 0x00 or 0xff in key[3] and key[7] should always
            // be true for a DbKeyDescriptor, and if that value isn't there, it suggests
            // that we probably have a textual key.
            byte tester = key[3];
            if (tester != 0 && tester != 0xff)
            {
                return false;
            }

            int x = BitConverter.ToInt32(key, 0);
            int z = BitConverter.ToInt32(key, 4);

            if (key.Length == 9)
            {
                // overworld, not a subchunk descriptor
                var keyType = (DbKeyType)key[8];
                if (!keyType.IsValid())
                {
                    return false;
                }
                result = new DbKeyDescriptor(x, z, Dimension.OVERWORLD, keyType);
                return true;
            }
            else if (key.Length == 10)
            {
                // overworld, subchunk descriptor
                var keyType = (DbKeyType)key[8];
                if (!keyType.IsValid())
                {
                    return false;
                }
                result = new DbKeyDescriptor(x, key[9], z, Dimension.OVERWORLD, keyType);
                return true;
            }
            else if (key.Length == 13)
            {
                // nether / end, not a subchunk descriptor
                int dim = BitConverter.ToInt32(key, 8);
                if (dim < 0 || dim > 2)
                {
                    return false;
                }
                var keyType = (DbKeyType)key[12];
                if (!keyType.IsValid())
                {
                    return false;
                }
                result = new DbKeyDescriptor(x, z, Dimension.From(dim), keyType);
                return true;
            }
            else if (key.Length == 14)
            {
                // nether / end, subchunk descriptor
                int dim = BitConverter.ToInt32(key, 8);
                if (dim < 0 || dim > 2)
                {
                    return false;
                }
                var keyType = (DbKeyType)key[12];
                if (!keyType.IsValid())
                {
                    return false;
                }
                result = new DbKeyDescriptor(x, key[13], z, Dimension.From(dim), keyType);
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~DbBroker()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (db_ != null)
            {
                db_.Dispose();
                db_ = null;
            }
        }
    }
}
