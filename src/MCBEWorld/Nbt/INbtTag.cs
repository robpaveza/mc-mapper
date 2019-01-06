using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCBEWorld.Nbt
{
    public interface INbtTag
    {
        string Name { get; }
        NbtTagType Type { get; }
    }

    public interface INbtTag<T> : INbtTag
    {
        T Value { get; }
    }

    public interface INbtArrayTag<T> : INbtTag, IReadOnlyList<T>
    {

    }
    
    public static class NbtTagExtensions
    {
        public static INbtTag<T> As<T>(this INbtTag tag)
        {
            if (TagMatchesResultType(tag, typeof(T)))
            {
                return tag as INbtTag<T>;
            }

            return null;
        }

        public static INbtArrayTag<T> AsArray<T>(this INbtTag tag)
            where T : struct
        {
            var list = tag as INbtList;
            if (TagTypeMatchesType(list.Subtype, typeof(T)))
            {
                return list as INbtArrayTag<T>;
            }

            return null;
        }

        public static INbtStream AsStream(this INbtTag tag)
        {
            if (TagMatchesResultType(tag, typeof(INbtStream)))
            {
                return tag as INbtStream;
            }

            return null;
        }

        public static INbtDictionary AsDictionary(this INbtTag tag)
        {
            if (TagMatchesResultType(tag, typeof(INbtDictionary)))
            {
                return tag as INbtDictionary;
            }

            return null;
        }

        private static bool TagMatchesResultType(INbtTag tag, Type type)
        {
            if (tag.Type == NbtTagType.List)
            {
                var interfaceType = type.GetInterfaces().Where(t => t.Name == "INbtTag" && t.GenericTypeArguments.Length > 0).FirstOrDefault();
                if (interfaceType == null)
                {
                    return false;
                }

                var listSubtype = interfaceType.GenericTypeArguments[0];
                var list = tag as INbtList;
                return TagTypeMatchesType(list.Subtype, listSubtype);
            }

            return TagTypeMatchesType(tag.Type, type);
        }

        private static bool TagTypeMatchesType(NbtTagType tagType, Type type)
        {
            switch (tagType)
            {
                case NbtTagType.UInt8:
                    return type == typeof(byte);

                case NbtTagType.Int16:
                    return type == typeof(short);

                case NbtTagType.Int32:
                    return type == typeof(int);

                case NbtTagType.Int64:
                    return type == typeof(long);

                case NbtTagType.Float32:
                    return type == typeof(float);

                case NbtTagType.Float64:
                    return type == typeof(double);

                case NbtTagType.UInt8Array:
                    return type == typeof(byte[]);

                case NbtTagType.Utf8String:
                    return type == typeof(string);

                case NbtTagType.Compound:
                    return typeof(INbtStream).IsAssignableFrom(type);

                case NbtTagType.Int32Array:
                    return type == typeof(int[]);

                case NbtTagType.Int64Array:
                    return type == typeof(long[]);
            }

            return false;
        }
    }
}
