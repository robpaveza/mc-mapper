using System;
using System.Collections.Generic;
using System.Text;

namespace MCBEWorld.Nbt
{
    public interface INbtStream
    {
        bool HasValue { get; }
        bool MoveNext();
        INbtTag CurrentValue { get; }
        void Reset();
    }

    public interface INbtDictionary : INbtStream
    {
        INbtTag this[string key] { get; }
        bool Contains(string key);
    }

    internal interface INbtStreamInternal
    {
        INbtStream CloneAt(long position);
    }

    public interface INbtList
    {
        NbtTagType Subtype { get; }
    }

    public interface INbtList<T> : INbtTag, INbtList, IReadOnlyList<T>
    {
        
    }
}
