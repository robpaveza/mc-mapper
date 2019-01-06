using MCBEWorld.Worlds;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace MCBEWorld.Data
{
    internal sealed class HistoricalBlockData
    {
        private const string HBD_RESOURCE_NAME = "MCBEWorld.Data.Blocks.json";
        private static Dictionary<ushort, BlockPaletteEntry> definitions = new Dictionary<ushort, BlockPaletteEntry>();
        private static Lazy<Dictionary<int, string>> table = new Lazy<Dictionary<int, string>>(LazilyLoad);

        private static Dictionary<int, string> LazilyLoad()
        {
            var asm = Assembly.GetExecutingAssembly();
            using (var stream = asm.GetManifestResourceStream(HBD_RESOURCE_NAME))
            using (var reader = new StreamReader(stream))
            {
                string json = reader.ReadToEnd();
                var array = JArray.Parse(json);

                var result = new Dictionary<int, string>();
                foreach (var descendant in array)
                {
                    var idToken = descendant["id"];
                    if (idToken == null)
                    {
                        continue;
                    }

                    int id = idToken.Value<int>();
                    if (result.ContainsKey(id))
                    {
                        continue;
                    }

                    string name = descendant["name"].Value<string>();
                    result.Add(id, name);
                }

                return result;
            }
        }


        public static BlockPaletteEntry Get(byte id, byte data)
        {
            ushort key = unchecked((ushort)((id << 8) | data));
            if (definitions.TryGetValue(key, out BlockPaletteEntry result))
            {
                return result;
            }
            string name = table.Value[id];
            var definition = new BlockPaletteEntry(name, data);
            definitions.Add(key, definition);
            return definition;
        }
    }
}
