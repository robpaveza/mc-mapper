using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MCMapper.ConfigurationGenerator
{
    /// <summary>
    /// Reads runtime_id_table.json
    /// </summary>
    /// <remarks>
    /// Tommo published this gist which contains the set of blocks and their corresponding
    /// RuntimeIDs: https://gist.github.com/Tomcc/ad971552b024c7619e664d0377e48f58
    /// We will use these RuntimeIDs to communicate between the backend and the frontend.
    /// </remarks>
    internal static class RuntimeIdTableReader
    {
        public static IReadOnlyList<RuntimeIdTableEntry> Load()
        {
            string jsonText = File.ReadAllText("runtime_id_table.json");
            return ReadTable(jsonText);
        }

        public static IReadOnlyList<RuntimeIdTableEntry> ReadTable(string jsonText)
        {
            JArray obj = JArray.Parse(jsonText);
            return obj.Select(token => RuntimeIdTableEntry.FromJson(token as JObject)).ToList();
        }
    }

    internal class RuntimeIdTableEntry
    {
        private int? id_;
        public RuntimeIdTableEntry(int id, int data, string name, int rid)
            : this(data, name, rid)
        {

        }

        public RuntimeIdTableEntry(int data, string name, int rid)
        {
            Data = data;
            Name = name;
            RuntimeId = rid;
        }

        public static RuntimeIdTableEntry FromJson(JObject jsonObject)
        {
            int data = jsonObject["data"].Value<int>();
            string name = jsonObject["name"].Value<string>();
            int rid = jsonObject["runtimeID"].Value<int>();

            int? id = jsonObject["data"]?.Value<int>();
            if (id.HasValue)
            {
                return new RuntimeIdTableEntry(id.Value, data, name, rid);
            }

            return new RuntimeIdTableEntry(data, name, rid);
        }

        public int RuntimeId { get; }
        public string Name { get; }
        public int Data { get; }
        public bool HasLegacyBlockId => id_.HasValue;
        public int LegacyBlockId => id_.Value;
    }
}
