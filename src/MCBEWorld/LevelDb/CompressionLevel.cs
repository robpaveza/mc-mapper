using System;
using System.Collections.Generic;
using System.Text;

namespace MCBEWorld.LevelDb
{
    /// <summary>
    /// DB contents are stored in a set of blocks, each of which holds a
    /// sequence of key,value pairs.  Each block may be compressed before
    /// being stored in a file. The following enum describes which
    /// compression method (if any) is used to compress a block.
    /// </summary>
    public enum CompressionLevel
    {
        NoCompression = 0,
        [Obsolete("Snappy is not supported in LevelDB-MCPE")]
        SnappyCompression = 1,
        Zlib = 2,
    }
}
