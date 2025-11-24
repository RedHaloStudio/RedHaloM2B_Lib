using Newtonsoft.Json;
using RedHaloM2B.Textures;

namespace RedHaloM2B.Materials
{
    internal class RedHaloDoubleMtl : RedHaloBaseMtl
    {
        [JsonProperty("front_material")]
        public string FrontMaterial { get; set; }

        [JsonProperty("back_material")]
        public string BackMaterial { get; set; }

        [JsonProperty("mask_color")]
        public float[] MaskColor { get; set; } = [0.5f, 0.5f, 0.5f, 1];

        [JsonProperty("mask_texmap")]
        public TexmapInfo MaskTexmap { get; set; }

        [JsonProperty("mix_amount")]
        public float MixAmount { get; set; }
    }
}