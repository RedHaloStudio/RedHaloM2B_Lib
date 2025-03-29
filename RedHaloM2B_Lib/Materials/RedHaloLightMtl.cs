using RedHaloM2B.Textures;
using Newtonsoft.Json;
using RedHaloM2B.Nodes;

namespace RedHaloM2B.Materials
{
    internal class RedHaloLightMtl : RedHaloBaseMtl
    {
        //public string Name { get; set; }
        //public string SourceName { get; set; }
        //public string ID { get; set; }
        //public string Type { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("color_array")]
        public float[] ColorArray { get; set; }

        [JsonProperty("color_texmap")]
        public TexmapInfo ColorTexmap { get; set; }

        [JsonProperty("strength")]
        public float Strength { get; set; }

        [JsonProperty("strength_texmap")]
        public TexmapInfo StrengthTexmap { get; set; }

        [JsonProperty("opacity")]
        public TexmapInfo OpacityTexmap { get; set; }

        [JsonProperty("use_twosides")]
        public bool UseTwoSided { get; set; }

        [JsonProperty("visible_reflect")]
        public bool VisibleReflect { get; set; }

        [JsonProperty("visible_direct")]
        public bool VisibleDirect { get; set; }

        [JsonProperty("visible_refract")]
        public bool VisibleRefract { get; set; }

        [JsonProperty("affect_alpha")]
        public bool AffectAlpha { get; set; }

        public RedHaloLightMtl() {
            Color = "1,1,1,1";
            ColorArray = new[] { 1f, 1f, 1f };

            UseTwoSided = true;
            VisibleReflect = true;
            VisibleDirect = true;
            VisibleRefract = true;
        }
    }
}
