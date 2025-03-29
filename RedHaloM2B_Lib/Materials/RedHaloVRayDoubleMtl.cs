using Newtonsoft.Json;
using RedHaloM2B.Textures;

namespace RedHaloM2B.Materials
{
    internal class RedHaloVRayDoubleMtl : RedHaloBaseMtl
    {
        [JsonProperty("front_material")]
        public string FrontMaterial { get; set; }

        [JsonProperty("back_material")]
        public string BackMaterial { get; set; }

        [JsonProperty("mask_color")]
        public string MaskColor { get; set; } = "0.5,0.5,0.5,1";

        [JsonProperty("mask_texmap")]
        public TexmapInfo MaskTexmap { get; set; }

        [JsonProperty("mix_amount")]
        public float MixAmount { get; set; }
    }
}