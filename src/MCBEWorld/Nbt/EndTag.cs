using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MCBEWorld.Nbt
{
    [DebuggerDisplay("<end tag>")]
    internal class EndTag : INbtTag
    {
        internal static readonly EndTag Instance = new EndTag();

        private EndTag() { }

        public string Name => "";

        public NbtTagType Type => NbtTagType.EndTag;
    }
}
