using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MCBEWorld.Nbt
{
    internal sealed class NumberArrayTag<T> : INbtArrayTag<T>
        where T : struct
    {
        private T[] source_;

        public NumberArrayTag(NbtTagType type, string name, T[] source)
        {
            Type = type;
            Name = name;

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            source_ = source;
        }

        public T this[int index] => source_[index];

        public string Name { get; }

        public NbtTagType Type { get; }

        public int Count => source_.Length;

        public IEnumerator<T> GetEnumerator()
        {
            foreach (T value in source_)
            {
                yield return value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return source_.GetEnumerator();
        }
    }

    internal static class NumberArrayTag
    {
        public static INbtTag FromReader(NbtTagType type, string name, BinaryReader reader)
        {
            int size = reader.ReadInt32();
            if (size < 0)
            {
                throw new InvalidDataException("Found a negative length for array-type NBT.");
            }

            switch (type)
            {
                // TODO: Determine if this can be made to be faster, for example, by copying
                // the memory out as a byte[], and then doing a second copy as a pointer-copy
                // into the destination.  Alternatively, read them on-demand.
                case NbtTagType.Int32Array:
                    int[] resultIntArray = new int[size];
                    for (int i = 0; i < size; i++)
                    {
                        int next = reader.ReadInt32();
                        resultIntArray[i] = next;
                    }
                    return new NumberArrayTag<int>(type, name, resultIntArray);

                case NbtTagType.Int64Array:
                    long[] resultLongArray = new long[size];
                    for (int i = 0; i < size; i++)
                    {
                        long next = reader.ReadInt64();
                        resultLongArray[i] = next;
                    }
                    return new NumberArrayTag<long>(type, name, resultLongArray);

                case NbtTagType.UInt8Array:
                    byte[] resultByteArray = reader.ReadBytes(size);
                    return new NumberArrayTag<byte>(type, name, resultByteArray);
            }

            throw new ArgumentOutOfRangeException(nameof(type), type, "Unrecognized or unsupported type for numeric integer array.");
        }
    }
}
