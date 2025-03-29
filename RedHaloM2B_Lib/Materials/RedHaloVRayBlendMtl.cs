using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RedHaloM2B.Materials;
using RedHaloM2B.Textures;

namespace RedHaloM2B.Materials
{
    internal class RedHaloVRayBlendMtl : RedHaloBaseMtl
    {
        [JsonProperty("base_material")]
        public string BaseMaterial { get; set; }

        #region Material1
        [JsonProperty("material1")]
        public string Material1 { get; set; }

        [JsonProperty("material1_color")]
        public string Material1Color { get; set; }

        [JsonProperty("material1_texmap")]
        public TexmapInfo Material1Texmap { get; set; }

        [JsonProperty("material1_blend_amount")]
        public float Material1BlendAmout { get; set; }
        #endregion

        #region Material2
        [JsonProperty("material2")]
        public string Material2 { get; set; }

        [JsonProperty("material2_color")]
        public string Material2Color { get; set; }

        [JsonProperty("material2_texmap")]
        public TexmapInfo Material2Texmap { get; set; }

        [JsonProperty("material2_blend_amount")]
        public float Material2BlendAmout { get; set; }
        #endregion

        #region Material3
        [JsonProperty("material3")]
        public string Material3 { get; set; }

        [JsonProperty("material3_color")]
        public string Material3Color { get; set; }

        [JsonProperty("material3_texmap")]
        public TexmapInfo Material3Texmap { get; set; }

        [JsonProperty("material3_blend_amount")]
        public float Material3BlendAmout { get; set; }
        #endregion

        #region Material4
        [JsonProperty("material4")]
        public string Material4 { get; set; }

        [JsonProperty("material4_color")]
        public string Material4Color { get; set; }

        [JsonProperty("material4_texmap")]
        public TexmapInfo Material4Texmap { get; set; }

        [JsonProperty("material4_blend_amount")]
        public float Material4BlendAmout { get; set; }
        #endregion

        #region Material5
        [JsonProperty("material5")]
        public string Material5 { get; set; }

        [JsonProperty("material5_color")]
        public string Material5Color { get; set; }

        [JsonProperty("material5_texmap")]
        public TexmapInfo Material5Texmap { get; set; }

        [JsonProperty("material5_blend_amount")]
        public float Material5BlendAmout { get; set; }
        #endregion

        #region Material6
        [JsonProperty("material6")]
        public string Material6 { get; set; }

        [JsonProperty("material6_color")]
        public string Material6Color { get; set; }

        [JsonProperty("material6_texmap")]
        public TexmapInfo Material6Texmap { get; set; }

        [JsonProperty("material6_blend_amount")]
        public float Material6BlendAmout { get; set; }
        #endregion

        #region Material7
        [JsonProperty("material7")]
        public string Material7 { get; set; }

        [JsonProperty("material7_color")]
        public string Material7Color { get; set; }

        [JsonProperty("material7_texmap")]
        public TexmapInfo Material7Texmap { get; set; }

        [JsonProperty("material7_blend_amount")]
        public float Material7BlendAmout { get; set; }
        #endregion

        #region Material8
        [JsonProperty("material8")]
        public string Material8 { get; set; }

        [JsonProperty("material8_color")]
        public string Material8Color { get; set; }

        [JsonProperty("material8_texmap")]
        public TexmapInfo Material8Texmap { get; set; }

        [JsonProperty("material8_blend_amount")]
        public float Material8BlendAmout { get; set; }
        #endregion

        #region Material9
        [JsonProperty("material9")]
        public string Material9 { get; set; }

        [JsonProperty("material9_color")]
        public string Material9Color { get; set; }

        [JsonProperty("material9_texmap")]
        public TexmapInfo Material9Texmap { get; set; }

        [JsonProperty("material9_blend_amount")]
        public float Material9BlendAmout { get; set; }
        #endregion
    }
}
