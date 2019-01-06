using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCMapper.TexturePackReader.NetFramework
{
    internal sealed class BiomesReader
    {
        private string path_;
        public BiomesReader(string texturePackPath)
        {
            path_ = Path.Combine(texturePackPath, "biomes_client.json");
        }

        public Dictionary<string, string> GetBiomeIdTintMap()
        {
            string jsonText = File.ReadAllText(path_);
            var settings = JObject.Parse(jsonText);
            var biomes = settings["biomes"] as JObject;
            return (from p in biomes.Properties()
                    select new
                    {
                        Id = p.Name,
                        WaterTint = (p.Value as JObject)["water_surface_color"].Value<string>(),
                    })
                    .ToDictionary(i => i.Id, i => i.WaterTint);
        }
    }
}
