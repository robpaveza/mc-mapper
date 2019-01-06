using System;
using System.Collections.Generic;
using System.Text;

namespace MCBEWorld.Worlds
{
    public sealed class BlockPaletteEntry
    {
        public BlockPaletteEntry(string name, int data)
        {
            Name = name;
            Data = data;
        }

        public string Name { get; }
        public int Data { get; }
    }
}
