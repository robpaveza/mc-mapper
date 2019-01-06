using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCMapper.ConfigurationGenerator
{

    internal class OutputEntry
    {
        public string DataName { get; set; }
        public string FriendlyName { get; set; }
        public int? LegacyId { get; set; }
        public int DataValue { get; set; }
        public int RuntimeId { get; set; }
        public string Color { get; set; }
        public bool InferredColor { get; set; }
        public string Todo { get; set; }

        public JObject Serialize()
        {
            JObject result = new JObject();
            // These are the three possible selector values: either dname+data or legacy_id+data
            result["dname"] = DataName;
            if (LegacyId.HasValue)
            {
                result["legacy_id"] = LegacyId.Value;
            }
            result["data"] = DataValue;
            // output value when serializing blocks for rendering
            result["runtime_id"] = RuntimeId;
            // friendly name
            result["name"] = FriendlyName;
            result["color"] = Color;
            if (InferredColor)
            {
                result["inferred_color"] = true;
            }

            if (Todo != null)
            {
                result["todo"] = Todo;
            }

            return result;
        }
    }
}
