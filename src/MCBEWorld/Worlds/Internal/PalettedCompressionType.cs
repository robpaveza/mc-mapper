using System;
using System.Collections.Generic;
using System.Text;

namespace MCBEWorld.Worlds.Internal
{
    internal enum PalettedCompressionType : byte
    {
        Paletted1 = 1, // 32 blocks per word
        Paletted2 = 2, // 16 bpw
        Paletted3 = 3, // 10 bpw, 2 bits padding
        Paletted4 = 4, // 8 bpw
        Paletted5 = 5, // 6 bpw, 2 bits padding
        Paletted6 = 6, // 5 bpw, 2 bits padding
        Paletted8 = 8, // 4 bpw
        Paletted16 = 16, // 2 bpw

        BIT_MASK = 0xfe,
    }
}
