using MCBERenderer.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCBEMapper.Data
{
    public class Configuration
    {
        private string worldPath_;
        private Dictionary<int, BiomeRenderConfiguration> biomeConfig_;

        public Configuration(string worldPath, string biomeConfigJsonText)
        {
            worldPath_ = worldPath;
            biomeConfig_ = new Dictionary<int, BiomeRenderConfiguration>();
            JArray biomeArray = JArray.Parse(biomeConfigJsonText);
            foreach (var token in biomeArray)
            {
                BiomeRenderConfiguration entry = BiomeRenderConfiguration.FromJson(token as JObject);
                biomeConfig_.Add(entry.ID, entry);
            }
        }

        public Dictionary<int, BiomeRenderConfiguration> BiomeRenderingConfiguration => biomeConfig_;
        public string WorldPath => worldPath_;
    }
}
