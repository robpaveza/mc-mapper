using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MCBEWorld.Nbt
{
    internal sealed class ListTag<T> : INbtList<T>, INbtArrayTag<T>
    {
        private T[] items_;
        public ListTag(NbtTagType subtype, string name, T[] items)
        {
            Subtype = subtype;
            Name = name;
            items_ = items;
        }

        public T this[int index] => items_[index];

        public string Name { get; }

        public NbtTagType Type => NbtTagType.List;

        public NbtTagType Subtype { get; }

        public int Count => items_.Length;

        public IEnumerator<T> GetEnumerator()
        {
            foreach (T item in items_)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items_.GetEnumerator();
        }
    }

    internal static class ListTag
    {
        private delegate T ReadT<T>();
        public static INbtTag FromReaderAndStream(string name, BinaryReader reader, INbtStream stream)
        {
            NbtTagType type = (NbtTagType)reader.ReadByte();
            int count = reader.ReadInt32();
            switch (type)
            {
                case NbtTagType.Compound:
                    return CompoundListFrom(name, count, stream);

                case NbtTagType.Float32:
                    return new ListTag<float>(type, name, ReadArray(count, reader.ReadSingle));

                case NbtTagType.Float64:
                    return new ListTag<double>(type, name, ReadArray(count, reader.ReadDouble));

                case NbtTagType.Int16:
                    return new ListTag<short>(type, name, ReadArray(count, reader.ReadInt16));

                case NbtTagType.Int32:
                    return new ListTag<int>(type, name, ReadArray(count, reader.ReadInt32));

                case NbtTagType.Int64:
                    return new ListTag<long>(type, name, ReadArray(count, reader.ReadInt64));

                case NbtTagType.UInt8:
                    return ByteListFrom(name, reader, count);

                case NbtTagType.Utf8String:
                    return StringListFrom(name, reader, count);

                case NbtTagType.EndTag:
                    return VoidListFrom(name, reader, count);

                default:
                    throw new NotSupportedException($"Unsupported type {type} subtype of List");
            }
        }

        private delegate T Producer<T>();
        private static T[] ReadArray<T>(int count, Producer<T> producer)
        {
            T[] result = new T[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = producer();
            }
            return result;
        }

        private static INbtList<string> StringListFrom(string name, BinaryReader reader, int count)
        {
            string[] result = new string[count];
            for (int i = 0; i < count; i++)
            {
                ushort strlen = reader.ReadUInt16();
                byte[] stringBits = reader.ReadBytes(strlen);
                result[i] = Encoding.UTF8.GetString(stringBits);
            }

            return new ListTag<string>(NbtTagType.Utf8String, name, result);
        }

        private static INbtList<INbtTag> VoidListFrom(string name, BinaryReader reader, int count)
        {
            if (count != 0)
            {
                throw new InvalidDataException($"Expected 0-length array of TAG_End but found {count}-length.");
            }

            return new ListTag<INbtTag>(NbtTagType.EndTag, name, new INbtTag[0]);
        }

        private static INbtList<INbtStream> CompoundListFrom(string name, int count, INbtStream source)
        {
            INbtStream[] result = new INbtStream[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = NbtCompoundTag.FromReader($"{name}[{i}]", source).AsStream();
            }

            return new ListTag<INbtStream>(NbtTagType.Compound, name, result);
        }

        private static INbtList<byte> ByteListFrom(string name, BinaryReader reader, int count)
        {
            return new ListTag<byte>(NbtTagType.UInt8, name, reader.ReadBytes(count));
        }
    }
}
