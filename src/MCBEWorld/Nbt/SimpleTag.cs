using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MCBEWorld.Nbt
{
    [DebuggerDisplay("{Type}: {Name} = {Value}")]
    internal sealed class SimpleTag<T> : INbtTag<T>
    {
        public SimpleTag(string name, NbtTagType type, T value)
        {
            Name = name;
            Type = type;
            Value = value;
        }

        public string Name { get; }

        public T Value { get; }

        public NbtTagType Type { get; }
    }

    internal static class SimpleTag
    {
        public static INbtTag FromReader(NbtTagType type, string name, BinaryReader reader)
        {
            switch (type)
            {
                case NbtTagType.Compound:
                case NbtTagType.EndTag:
                case NbtTagType.Int32Array:
                case NbtTagType.Int64Array:
                case NbtTagType.UInt8Array:
                case NbtTagType.List:
                    throw new ArgumentException($"Unsupported type '{type}' for simple tag.", nameof(type));

                case NbtTagType.Float32:
                    float floatValue = reader.ReadSingle();
                    return new SimpleTag<float>(name, type, floatValue);

                case NbtTagType.Float64:
                    double doubleValue = reader.ReadDouble();
                    return new SimpleTag<double>(name, type, doubleValue);

                case NbtTagType.Int16:
                    short shortValue = reader.ReadInt16();
                    return new SimpleTag<short>(name, type, shortValue);

                case NbtTagType.Int32:
                    int intValue = reader.ReadInt32();
                    return new SimpleTag<int>(name, type, intValue);

                case NbtTagType.Int64:
                    long longValue = reader.ReadInt64();
                    return new SimpleTag<long>(name, type, longValue);

                case NbtTagType.UInt8:
                    byte byteValue = reader.ReadByte();
                    return new SimpleTag<byte>(name, type, byteValue);

                case NbtTagType.Utf8String:
                    ushort stringLength = reader.ReadUInt16();
                    byte[] stringBytes = reader.ReadBytes(stringLength);
                    string stringValue = Encoding.UTF8.GetString(stringBytes);
                    return new SimpleTag<string>(name, type, stringValue);

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "Unrecognized or unsupported type of NBT data.");
            }
        }
    }
}
