using System;
using System.Collections.Generic;
using System.Text;

namespace MCBEWorld.Nbt
{
    public enum NbtTagType : byte
    {
        EndTag = 0,
        UInt8 = 1,
        Int16 = 2,
        Int32 = 3,
        Int64 = 4,
        Float32 = 5,
        Float64 = 6,
        UInt8Array = 7,
        Utf8String = 8,
        List = 9,
        Compound = 10,
        Int32Array = 11,
        Int64Array = 12,
    }
}
