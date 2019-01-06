using System;
using System.Collections.Generic;
using System.Text;

namespace MCBEWorld.Data
{
    /// <summary>
    /// When implemented by a renderer or application, facilitates an agreed-upon fast path
    /// for block data to be serialized to the front-end.
    /// </summary>
    public interface IBlockDataSerializationConfiguration
    {
        int LookupLegacyFormatBlock(byte blockId, byte blockData);
        int LookupBlockByName(string name, byte blockData);
    }
}
