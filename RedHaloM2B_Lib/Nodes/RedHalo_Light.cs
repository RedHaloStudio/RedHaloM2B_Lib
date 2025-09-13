using Newtonsoft.Json;
using System.Xml.Serialization;

namespace RedHaloM2B.Nodes
{
    public class RedHaloLight : RedHaloBaseNode
    {
        [JsonProperty("color")]
        public float[] Color { get; set; }

        [JsonProperty("strength")]
        public float Strength { get; set; }

        [JsonProperty("type")]
        public string LightType { get; set; }

        [JsonProperty("width")]
        public float Width { get; set; }

        [JsonProperty("length")]
        public float Length { get; set; }

        [JsonProperty("ies")]
        public string IES { get; set; }

        [JsonProperty("angle")]
        public float Angle { get; set; }

        [JsonProperty("angleblend")]
        public float AngleBlend { get; set; }

        [JsonProperty("directional")]
        public float Directional { get; set; }

        [JsonProperty("portal")]
        public bool Portal { get; set; }

        [JsonProperty("diffuse")]
        public bool Diffuse { get; set; }

        [JsonProperty("specular")]
        public bool Specular { get; set; }

        [JsonProperty("reflection")]
        public bool Reflection { get; set; }

        [JsonProperty("shadow")]
        public bool Shadow { get; set; }

        [JsonProperty("invisible")]
        public bool Invisible { get; set; }

        [JsonProperty("volume")]
        public bool Volume { get; set; }
    }
}
