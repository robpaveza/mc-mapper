using MCBEWorld.Nbt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MCBEWorld.Utility
{
    internal sealed class DataReader
    {
        private Stream source_;
        private BinaryReader reader_;

        public DataReader(Stream source, BinaryReader reader)
        {
            if (!source.CanSeek)
            {
                throw new ArgumentException(nameof(source));
            }
        }

        public byte ReadByte() => reader_.ReadByte();
        public byte[] ReadByteArray(int count) => reader_.ReadBytes(count);
        public int ReadInt32() => reader_.ReadInt32();
        public uint ReadUInt32() => reader_.ReadUInt32();

        public bool Seek(long position) => source_.Seek(position, SeekOrigin.Begin) == position;
        public void GoToStart() => source_.Seek(0, SeekOrigin.Begin);
        public bool AtEnd => source_.Position == source_.Length;

        public INbtTag ReadNbtTag()
        {
            var reader = NbtReader.Create(source_);
            if (reader.MoveNext())
            {
                return reader.CurrentValue;
            }

            return null;
        }
    }
}
