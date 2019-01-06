using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCMapper.TexturePackReader.NetFramework
{
    internal sealed class BlocksJsonReader
    {
        private static readonly HashSet<string> BiomeDependentColorBlocks = new HashSet<string>
        {
            "water",
            "flowing_water"
        };

        private static readonly HashSet<string> IgnoredBlocks = new HashSet<string>()
        {
            "format_version",
            "air"
        };

        private string path_;
        public BlocksJsonReader(string texturePackRoot)
        {
            path_ = Path.Combine(texturePackRoot, "blocks.json");
        }

        public IEnumerable<BlocksJsonEntry> ReadAllBlockEntries()
        {
            string json = File.ReadAllText(path_);
            JObject obj = JObject.Parse(json);
            foreach (var prop in obj.Properties())
            {
                string name = prop.Name;
                if (IgnoredBlocks.Contains(name))
                {
                    continue;
                }

                JObject settings = prop.Value as JObject;
                var textureToken = settings["textures"];
                string textureNameRef;
                if (textureToken.Type == JTokenType.String)
                {
                    textureNameRef = textureToken.Value<string>();
                }
                else
                {
                    JObject textureComponents = textureToken as JObject;
                    textureNameRef = textureComponents["up"].Value<string>();
                }

                bool isBiomeDependent = BiomeDependentColorBlocks.Contains(name);

                yield return new BlocksJsonEntry(name, textureNameRef, isBiomeDependent);
            }
        }
    }

    [DebuggerDisplay("{BlockDataName}: {TextureReference}")]
    internal sealed class BlocksJsonEntry
    {
        public BlocksJsonEntry(string name, string texRef, bool isBiomeDependent)
        {
            BlockDataName = name;
            TextureReference = texRef;
            IsBiomeDependent = isBiomeDependent;
        }

        public string BlockDataName { get; }
        public string TextureReference { get; }
        public bool IsBiomeDependent { get; }
    }
}
