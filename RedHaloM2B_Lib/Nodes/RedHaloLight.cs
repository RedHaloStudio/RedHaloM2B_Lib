using System.Xml.Serialization;

namespace RedHaloM2B.Nodes
{
    [XmlRoot("light")]
    public class RedHaloLight : RedHaloBaseNode
    {
        [XmlAttribute("color")]
        public string Color { get; set; }

        [XmlAttribute("strength")]
        public float Strength { get; set; }

        [XmlAttribute("type")]
        public string LightType { get; set; }

        [XmlAttribute("width")]
        public float Width { get; set; }

        [XmlAttribute("length")]
        public float Length { get; set; }

        [XmlAttribute("ies")]
        public string IES { get; set; }

        [XmlAttribute("angle")]
        public float Angle { get; set; }

        [XmlAttribute("angleblend")]
        public float AngleBlend { get; set; }

        [XmlAttribute("directional")]
        public float Directional { get; set; }

        [XmlAttribute("portal")]
        public bool Portal { get; set; }

        [XmlAttribute("diffuse")]
        public bool Diffuse { get; set; }

        [XmlAttribute("specular")]
        public bool Specular { get; set; }

        [XmlAttribute("reflection")]
        public bool Reflection { get; set; }

        [XmlAttribute("shadow")]
        public bool Shadow { get; set; }

        [XmlAttribute("invisible")]
        public bool Invisible { get; set; }

        [XmlAttribute("volume")]
        public bool Volume { get; set; }
    }
}
