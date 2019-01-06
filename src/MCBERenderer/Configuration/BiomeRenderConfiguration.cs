using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp.PixelFormats;

namespace MCBERenderer.Configuration
{
    public class BiomeRenderConfiguration
    {
        public BiomeRenderConfiguration(int id, string name, ColorRgb color)
        {
            ID = id;
            Name = name;
            Rgba32Color = color.ToRgba32();
        }

        public static BiomeRenderConfiguration FromJson(JObject j)
        {
            int id = j["id"].Value<int>();
            string name = j["name"].Value<string>();
            string colorText = j["color"].Value<string>();
            var color = ColorRgb.From0x(colorText);

            return new BiomeRenderConfiguration(id, name, color);
        }

        public int ID { get; }
        public string Name { get; }
        public Rgba32 Rgba32Color { get; }
    }
}
