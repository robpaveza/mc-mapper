using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCMapper.TexturePackReader.NetFramework
{
    [DebuggerDisplay("minecraft:{BlockName}:{BlockData} = {ColorHash}")]
    internal sealed class TextureAnalysis
    {
        public TextureAnalysis(string dataName, int blockData, string colorHash)
        {
            BlockName = dataName;
            BlockData = blockData;
            ColorHash = colorHash;
        }

        public TextureAnalysis(string dataName, int blockData, string colorHash, string biomeName)
            : this(dataName, blockData, colorHash)
        {
            BiomeName = biomeName;
        }

        public string BlockName { get; }
        public int BlockData { get; }
        public string ColorHash { get; }
        public string BiomeName { get; }

        public JObject ToJson()
        {
            var result = new JObject();
            result["block_name"] = BlockName;
            result["data"] = BlockData;
            result["color"] = ColorHash;
            if (BiomeName != null)
            {
                result["biome"] = BiomeName;
            }
            return result;
        }
    }
}
