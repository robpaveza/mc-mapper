using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MCBEWorld.Nbt
{
    public static class NbtReader
    {
        public static INbtStream Create(Stream source)
        {
            return new NbtStream(source);
        }
    }
}
