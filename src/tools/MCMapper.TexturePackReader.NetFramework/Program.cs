using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCMapper.TexturePackReader.NetFramework
{
    class Program
    {
        const string VANILLA_PACK_DOWNLOAD_PATH = @"C:\Users\Rob\Downloads\Vanilla_Resource_Pack_1.8.0";
        // The point of this application is to produce a map of minecraft block names, e.g., minecraft:cobblestone_wall, and data values, 
        // e.g., 1 (this tuple equals a Mossy Cobblestone Wall), to an RGBA color value.  It should be calculated as the average of the 
        // block's terrain top texture's pixel colors which are not transparent.
        static void Main(string[] args)
        {
            ProcessTexturePack(VANILLA_PACK_DOWNLOAD_PATH);
        }
        
        private static void ProcessTexturePack(string packLocation)
        {
            var loader = new ImageLoader(packLocation);
            var inspector = new ImageInspector();

            var blocksReader = new BlocksJsonReader(packLocation);
            var blocksDictionary = blocksReader.ReadAllBlockEntries().ToDictionary(b => b.BlockDataName);
            var texturesReader = new TerrainTexturesReader(packLocation);
            var textures = texturesReader.ReadAllTextureEntries().ToDictionary(t => t.TextureReference);
            var biomesReader = new BiomesReader(packLocation);
            var biomes = biomesReader.GetBiomeIdTintMap();

            List<TextureAnalysis> result = new List<TextureAnalysis>();
            foreach (string blockName in blocksDictionary.Keys)
            {
                var block = blocksDictionary[blockName];
                string textRef = block.TextureReference;
                var textureInfo = textures[textRef];
                for (int i = 0; i < textureInfo.Textures.Length; i++)
                {
                    Console.WriteLine($"Processing minecraft:{blockName}/{i}");
                    var texture = textureInfo.Textures[i];
                    string blockFileName = texture.FileName.Substring(texture.FileName.LastIndexOf('/') + 1);
                    Color? tintColor = null;
                    if (texture.ColorHash != null)
                    {
                        tintColor = texture.GetColor();
                    }
                    else if (block.IsBiomeDependent)
                    {
                        foreach (string key in biomes.Keys)
                        {
                            tintColor = ParseHashColor(biomes[key]);
                            using (var tex = loader.Load(blockFileName, tintColor))
                            {
                                var averageColor = Color.FromArgb(inspector.CalculateImageAverageColor(tex));
                                var analysis = new TextureAnalysis(blockName, i, FormatColor(averageColor), key);
                                result.Add(analysis);
                            }
                        }
                        continue;
                    }

                    using (var tex = loader.Load(blockFileName, tintColor))
                    {
                        var averageColor = Color.FromArgb(inspector.CalculateImageAverageColor(tex));
                        var analysis = new TextureAnalysis(blockName, i, FormatColor(averageColor));
                        result.Add(analysis);
                    }
                }
            }

            JArray output = new JArray(
                result.Select(s => s.ToJson())
            );
            string outputJson = output.ToString(Formatting.Indented);
            File.WriteAllText("texpack-colors.json", outputJson);
        }

        private static string FormatColor(Color c)
        {
            return $"#{c.R:x2}{c.G:x2}{c.B:x2}";
        }

        private static Color ParseHashColor(string hashColor)
        {
            int r = int.Parse(hashColor.Substring(1, 2), NumberStyles.HexNumber);
            int g = int.Parse(hashColor.Substring(3, 2), NumberStyles.HexNumber);
            int b = int.Parse(hashColor.Substring(5, 2), NumberStyles.HexNumber);

            return Color.FromArgb(255, r, g, b);
        }

        //private static void TestLoads()
        //{
        //    var loader = new ImageLoader(@"C:\Users\Rob\Downloads\Vanilla_Resource_Pack_1.8.0");
        //    var inspector = new ImageInspector();

        //    using (var water = loader.Load("water_still_grey", DEFAULT_WATER_COLOR))
        //    {
        //        water.Save("water.png");
        //        Console.WriteLine($"Average was: {inspector.CalculateImageAverageColor(water):x2}");
        //    }

        //    Console.ReadLine();
        //}
    }
}
