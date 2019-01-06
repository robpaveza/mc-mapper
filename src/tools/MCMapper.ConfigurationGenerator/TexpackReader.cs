using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MCMapper.ConfigurationGenerator
{
    internal static class TexpackReader
    {
        public static IReadOnlyList<TexpackEntry> Load()
        {
            string jsonText = File.ReadAllText("texpack-colors.json");
            return ReadTable(jsonText);
        }

        public static IReadOnlyList<TexpackEntry> ReadTable(string jsonText)
        {
            JArray obj = JArray.Parse(jsonText);
            return obj.Select(t => TexpackEntry.FromJson(t as JObject)).ToList();
        }
    }

    internal class TexpackEntry
    {
        public TexpackEntry(string blockName, int data, string colorHash)
        {
            BlockName = blockName;
            Data = data;
            ColorHash = colorHash;
        }

        public TexpackEntry(string blockName, int data, string colorHash, string biomeName) 
            : this(blockName, data, colorHash)
        {
            BiomeName = biomeName;
        }

        public string BlockName { get; }
        public int Data { get; }
        public string ColorHash { get; }
        public string BiomeName { get; }

        public static TexpackEntry FromJson(JObject source)
        {
            string name = source["block_name"].Value<string>();
            int data = source["data"].Value<int>();
            string colorHash = source["color"].Value<string>();
            string biome = source["biome"]?.Value<string>();

            if (biome != null)
            {
                return new TexpackEntry(name, data, colorHash, biome);
            }

            return new TexpackEntry(name, data, colorHash);
        }
    }
}
