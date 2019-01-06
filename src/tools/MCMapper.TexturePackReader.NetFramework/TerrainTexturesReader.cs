using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCMapper.TexturePackReader.NetFramework
{
    internal sealed class TerrainTexturesReader
    {
        private static readonly Dictionary<string, string> ManualTint = new Dictionary<string, string>()
        {
            { "redstone_dust_cross", "#ff0000" },
            { "redstone_dust_line", "#ee0000" },
        };
        private string path_;

        public TerrainTexturesReader(string texturePackRoot)
        {
            path_ = Path.Combine(texturePackRoot, "textures", "terrain_texture.json");
        }

        public IEnumerable<TerrainTextureEntry> ReadAllTextureEntries()
        {
            string json = File.ReadAllText(path_);
            JObject root = JObject.Parse(json);
            JObject texData = root["texture_data"] as JObject;

            foreach (var prop in texData.Properties())
            {
                string name = prop.Name;
                var config = prop.Value as JObject;
                var texturesToken = config["textures"];

                TerrainTexture[] textureList;
                if (texturesToken.Type == JTokenType.String)
                {
                    string colorHash = null;
                    ManualTint.TryGetValue(name, out colorHash);
                    textureList = new[]
                    {
                        new TerrainTexture(texturesToken.Value<string>(), colorHash),
                    };
                }
                else if (texturesToken.Type == JTokenType.Object)
                {
                    var textureObj = texturesToken as JObject;
                    textureList = new[]
                    {
                        new TerrainTexture(textureObj["path"].Value<string>(), (textureObj["overlay_color"] ?? textureObj["tint_color"])?.Value<string>()),
                    };
                }
                else if (texturesToken.Type == JTokenType.Array)
                {
                    JArray texArray = texturesToken as JArray;
                    textureList = texArray.Select(token =>
                    {
                        if (token.Type == JTokenType.String)
                        {
                            return new TerrainTexture(token.Value<string>());
                        }
                        else
                        {
                            var textureObj = token as JObject;
                            return new TerrainTexture(textureObj["path"].Value<string>(), (textureObj["overlay_color"] ?? textureObj["tint_color"])?.Value<string>());
                        }
                    }).ToArray();
                }
                else
                {
                    throw new Exception($"Unsupported type for 'textures' {texturesToken.Type}");
                }

                yield return new TerrainTextureEntry(name, textureList);
            }
        }
    }

    [DebuggerDisplay("{TextureReference} ({Textures.Length} texture option(s))")]
    public class TerrainTextureEntry
    {
        public TerrainTextureEntry(string textRefName, TerrainTexture[] textures)
        {
            TextureReference = textRefName;
            Textures = textures;
        }

        public string TextureReference { get; }
        public TerrainTexture[] Textures { get; }
    }

    [DebuggerDisplay("{FileName} tint {ColorHash}")]
    public class TerrainTexture
    {
        public TerrainTexture(string fileName, string colorHash = null)
        {
            FileName = fileName;
            ColorHash = colorHash;
        }

        public string FileName { get; }
        public string ColorHash { get; }

        public Color GetColor()
        {
            if (ColorHash == null)
            {
                throw new InvalidOperationException();
            }

            int r = int.Parse(ColorHash.Substring(1, 2), NumberStyles.HexNumber);
            int g = int.Parse(ColorHash.Substring(3, 2), NumberStyles.HexNumber);
            int b = int.Parse(ColorHash.Substring(5, 2), NumberStyles.HexNumber);
            return Color.FromArgb(255, r, g, b);
        }
    }
}
