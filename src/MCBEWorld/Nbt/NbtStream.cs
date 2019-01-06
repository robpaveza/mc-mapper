using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MCBEWorld.Nbt
{
    internal class NbtStream : INbtStream
    {
        private Stream source_;
        private BinaryReader reader_;
        private INbtTag currentTag_;
        private long startPos_;

        public NbtStream(Stream source)
        {
            if (!source.CanSeek)
            {
                throw new ArgumentException("Source stream must be seekable.", nameof(source));
            }
            source_ = source;
            startPos_ = source.Position;
            reader_ = new BinaryReader(source, Encoding.UTF8);
        }

        public bool HasValue => currentTag_ != null;

        public INbtTag CurrentValue => currentTag_;

        public void Reset()
        {
            source_.Seek(startPos_, SeekOrigin.Begin);
        }

        public bool MoveNext()
        {
            if (source_.Position >= source_.Length)
            {
                return false;
            }

            long before = source_.Position;
            try
            {
                NbtTagType nextTagType = (NbtTagType)reader_.ReadByte();
                if (nextTagType == NbtTagType.EndTag)
                {
                    currentTag_ = EndTag.Instance;
                    return source_.Position < source_.Length;
                }

                ushort nextTagNameLength = reader_.ReadUInt16();
                string nextTagName = "";
                if (nextTagNameLength > 0)
                {
                    byte[] utf8TagName = reader_.ReadBytes(nextTagNameLength);
                    nextTagName = Encoding.UTF8.GetString(utf8TagName);
                }

                switch (nextTagType)
                {
                    case NbtTagType.Float32:
                    case NbtTagType.Float64:
                    case NbtTagType.Int16:
                    case NbtTagType.Int32:
                    case NbtTagType.Int64:
                    case NbtTagType.UInt8:
                    case NbtTagType.Utf8String:
                        currentTag_ = SimpleTag.FromReader(nextTagType, nextTagName, reader_);
                        return true;

                    case NbtTagType.Int32Array:
                    case NbtTagType.Int64Array:
                    case NbtTagType.UInt8Array:
                        currentTag_ = NumberArrayTag.FromReader(nextTagType, nextTagName, reader_);
                        return true;

                    case NbtTagType.List:
                        currentTag_ = ListTag.FromReaderAndStream(nextTagName, reader_, this);
                        return true;

                    case NbtTagType.Compound:
                        currentTag_ = NbtCompoundTag.FromReader(nextTagName, this);
                        return true;

                    default:
                        Debug.WriteLine($"Unrecognized NBT tag {((int)nextTagType):x2} at position 0x{before:x}");
                        throw new Exception($"Unrecognized NBT tag {((int)nextTagType):x2} at position 0x{before:x}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
                source_.Seek(before, SeekOrigin.Begin);
                return false;
            }
        }
    }
}
