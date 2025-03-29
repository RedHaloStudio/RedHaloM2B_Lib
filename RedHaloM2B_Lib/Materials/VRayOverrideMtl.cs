using Newtonsoft.Json;

namespace RedHaloM2B.Materials
{
    internal class VRayOverrideMtl : RedHaloBaseMtl
    {
        [JsonProperty("base_material")]
        public string BaseMaterial { get; set; }

        [JsonProperty("gi_material")]
        public string GIMaterial { get; set; }

        [JsonProperty("reflection_material")]
        public string ReflectionMaterial { get; set; }

        [JsonProperty("refraction_material")]
        public string RefractionMaterial { get; set; }

        [JsonProperty("shadow_material")]
        public string ShadowMaterial { get; set; }
    }
}
