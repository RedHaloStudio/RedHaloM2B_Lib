using System.Xml.Serialization;

namespace RedHaloM2B.Materials
{
    public class BaseMtl
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("id")]
        public string ID { get; set; }

        [XmlAttribute("source_name")]
        public string SourceName { get; set; }

        [XmlAttribute("type")]
        public string MaterialType { get; set; }
    }
}
