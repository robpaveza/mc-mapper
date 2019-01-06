using System;
using System.Collections.Generic;
using System.Text;

namespace MCBEWorld.Worlds.Internal
{
    internal enum SerializationType : byte
    {
        Persistence = 0,
        Network = 0x80,
        BIT_MASK = 0x80,
    }
}
