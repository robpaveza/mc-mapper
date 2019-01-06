using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace MCMapper.ConfigurationGenerator.McpeVizCompat
{
    internal sealed class McpeVizXmlReader
    {
        private List<BlockDefinition> blocks_;
        public McpeVizXmlReader(XDocument source)
        {
            blocks_ = source.Root.Element("blocklist").Elements("block").Select(e => BlockDefinition.FromXml(e)).ToList();
        }

        public static McpeVizXmlReader FromFile(string path)
        {
            string xml = File.ReadAllText(path);
            return new McpeVizXmlReader(XDocument.Parse(xml));
        }

        public IReadOnlyList<BlockDefinition> Blocks => blocks_.AsReadOnly();
    }
}
