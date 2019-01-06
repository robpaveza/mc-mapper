using MCBERenderer.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace MCMapper.ConfigurationGenerator.McpeVizCompat
{
    internal sealed class BlockDefinition
    {
        public BlockDefinition(int id, string friendly, string[] dataNames, ColorRgb color, IReadOnlyList<BlockVariantDefinition> variants)
        {
            Id = id;
            FriendlyName = friendly;
            DataNames = dataNames;
            Color = color;
            Variants = variants;
        }

        public static BlockDefinition FromXml(XElement e)
        {
            int id = int.Parse(e.Attribute("id").Value.Substring(2), NumberStyles.HexNumber);
            string fname = e.Attribute("name")?.Value ?? $"Unknown block id={id}";
            string dname = e.Attribute("uname")?.Value ?? $"minecraft:unknown_block_{id}";
            string[] dataNames = dname.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            string color = e.Attribute("color")?.Value ?? "0xff00ff";
            var parsedColor = ColorRgb.From0x(color);
            var variants = e.Elements("blockvariant").Select(el => BlockVariantDefinition.FromXml(el, dname, parsedColor)).ToList();

            return new BlockDefinition(id, fname, dataNames, parsedColor, variants);
        }

        public bool NameMatches(string lookingFor)
        {
            return DataNames.Contains(lookingFor);
        }

        // id=
        public int Id { get; }
        // name=
        public string FriendlyName { get; }
        // uname =
        public string[] DataNames { get; }
        // color =
        public ColorRgb Color { get; }

        public IReadOnlyList<BlockVariantDefinition> Variants { get; }
    }

    internal sealed class BlockVariantDefinition
    {
        public BlockVariantDefinition(int data, string friendlyName, ColorRgb color, bool wasColorInferred)
        {
            BlockData = data;
            FriendlyName = friendlyName;
            Color = color;
            IsColorInferred = wasColorInferred;
        }

        public static BlockVariantDefinition FromXml(XElement e, string parentBlockDataName, ColorRgb parentBlockColor)
        {
            int data = int.Parse(e.Attribute("blockdata").Value.Substring(2), NumberStyles.HexNumber);
            string customColor = e.Attribute("color")?.Value;
            ColorRgb resultColor;
            if (customColor != null)
            {
                resultColor = ColorRgb.From0x(customColor);
            }
            else
            {
                resultColor = parentBlockColor.Add(data);
            }
            string fname = e.Attribute("name")?.Value ?? $"Unknown variant ({parentBlockDataName} variant {data})";

            return new BlockVariantDefinition(data, fname, resultColor, customColor == null);
        }
        // blockdata=
        public int BlockData { get; }
        // name=
        public string FriendlyName { get; }
        // color=, optional
        public ColorRgb Color { get; }
        // if the color is not explicit, this property is true (gives us a chance to customize)
        public bool IsColorInferred { get; }
    }
}
