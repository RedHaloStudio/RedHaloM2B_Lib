using Autodesk.Max;
using Newtonsoft.Json;
using RedHaloM2B.Textures;

namespace RedHaloM2B.Materials
{
    internal class RedHaloLayer
    {
        [JsonProperty("material")]
        public string Material { get; set; }

        [JsonProperty("material_blend_amount")]
        public float MaterialBlend { get; set; }

        [JsonProperty("material_color")]
        public float[] Color { get; set; }

        [JsonProperty("mask_texmap")]
        public TexmapInfo MaskTexmap { get; set; }

        [JsonProperty("mask_texmap_blend_amount")]
        public float MaskAmount { get; set; }

        public RedHaloLayer()
        {
            MaterialBlend = 1f;
            Color = [1f, 1f, 1f]; 

            MaskAmount = 1f;
        }
    }
}
