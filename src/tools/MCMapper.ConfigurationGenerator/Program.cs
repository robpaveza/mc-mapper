using MCBERenderer.Configuration;
using MCMapper.ConfigurationGenerator.McpeVizCompat;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MCMapper.ConfigurationGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var reader = McpeVizCompat.McpeVizXmlReader.FromFile("mcpe_viz.xml");
            var mcpeConfigList = reader.Blocks;
            var ridTable = RuntimeIdTableReader.Load();
            var texpackColors = TexpackReader.Load();
            List<OutputEntry> resultList = new List<OutputEntry>();

            int numOfUnknownBlocks = 0;
            ColorRgb unknownColor = new ColorRgb(255, 0, 255);
            HashSet<string> observedColors = new HashSet<string>();

            foreach (var ridEntry in ridTable)
            {
                OutputEntry result = new OutputEntry();
                result.DataName = ridEntry.Name;
                result.DataValue = ridEntry.Data;
                if (ridEntry.HasLegacyBlockId)
                {
                    result.LegacyId = ridEntry.LegacyBlockId;
                }
                result.RuntimeId = ridEntry.RuntimeId;

                BlockDefinition block = null;
                bool lookedForVariant = false;
                BlockVariantDefinition blockVariant = null;
                if (ridEntry.HasLegacyBlockId)
                {
                    block = mcpeConfigList.Where(bd => bd.Id == ridEntry.LegacyBlockId).FirstOrDefault();
                    if (block != null && block.Variants.Count > 0)
                    {
                        lookedForVariant = true;
                        blockVariant = block.Variants.Where(v => v.BlockData == ridEntry.Data).FirstOrDefault();
                    }
                }
                else
                {
                    block = mcpeConfigList.Where(bd => bd.NameMatches(ridEntry.Name)).FirstOrDefault();
                    if (block != null && block.Variants.Count > 0)
                    {
                        lookedForVariant = true;
                        blockVariant = block.Variants.Where(v => v.BlockData == ridEntry.Data).FirstOrDefault();
                    }
                }

                if (block == null)
                {
                    result.FriendlyName = $"Unknown block {ridEntry.Name}";
                    result.Color = unknownColor.Add(-numOfUnknownBlocks).ToHash();
                    result.InferredColor = true;
                    numOfUnknownBlocks++;
                    result.Todo = $"NO MATCHED BLOCK FOUND: {ridEntry.Name}";
                }
                else
                {
                    if (lookedForVariant)
                    {
                        if (blockVariant == null)
                        {
                            // it's a variant block that couldn't be located
                            result.FriendlyName = $"{block.FriendlyName} (unknown variant; data = {ridEntry.Data})";
                            result.Color = block.Color.Add(ridEntry.Data).ToHash();
                            result.InferredColor = true;
                            result.Todo = $"BLOCK MATCHED BUT VARIANT MISSING; {ridEntry.Name} variant ${ridEntry.Data}";
                        }
                        else
                        {
                            // completely good block
                            result.FriendlyName = blockVariant.FriendlyName;
                            result.Color = blockVariant.Color.ToHash();
                            result.InferredColor = blockVariant.IsColorInferred;
                        }
                    }
                    else
                    {
                        if (ridEntry.Data > 0)
                        {
                            result.FriendlyName = $"{block.FriendlyName} (unlabeled variant {ridEntry.Data})";
                            result.Color = block.Color.Add(ridEntry.Data).ToHash();
                            result.InferredColor = true;
                            result.Todo = $"Block variant not included in source config: {ridEntry.Name} data {ridEntry.Data}";
                        }
                        else
                        {
                            // no variant, so completely good block
                            result.FriendlyName = block.FriendlyName;
                            result.Color = block.Color.ToHash();
                            result.InferredColor = false;
                        }
                    }
                }
                if (observedColors.Contains(result.Color))
                {
                    Console.WriteLine($"Duplicate color {result.Color} (for {result.DataName}, {result.DataValue})");
                }
                else
                {
                    observedColors.Add(result.Color);
                }
                resultList.Add(result);
            }

            JArray outputArray = new JArray(resultList.Select(oe => oe.Serialize()));
            string outputJson = outputArray.ToString(Formatting.Indented);
            File.WriteAllText("blocks-config.json", outputJson);

            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }
}
