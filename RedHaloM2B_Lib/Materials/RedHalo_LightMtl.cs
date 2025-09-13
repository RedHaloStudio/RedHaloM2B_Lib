using RedHaloM2B.Textures;
using Newtonsoft.Json;

namespace RedHaloM2B.Materials
{
    public class RedHaloLightMtl : RedHaloBaseMtl
    {
        [JsonProperty("color")]
        public float[] Color { get; set; } = [1, 1, 1, 1];

        [JsonProperty("color_texmap")]
        public TexmapInfo ColorTexmap { get; set; }

        [JsonProperty("strength")]
        public float Strength { get; set; } = 0f;

        [JsonProperty("strength_texmap")]
        public TexmapInfo StrengthTexmap { get; set; }

        [JsonProperty("opacity")]
        public TexmapInfo OpacityTexmap { get; set; }

        [JsonProperty("use_twosides")]
        public bool UseTwoSided { get; set; } = true;

        [JsonProperty("visible_reflect")]
        public bool VisibleReflect { get; set; } = true;

        [JsonProperty("visible_direct")]
        public bool VisibleDirect { get; set; } = true;

        [JsonProperty("visible_refract")]
        public bool VisibleRefract { get; set; } = true;

        [JsonProperty("affect_alpha")]
        public bool AffectAlpha { get; set; }

    }
}
