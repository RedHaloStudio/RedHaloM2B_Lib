using Newtonsoft.Json;
using RedHaloM2B.Textures;

namespace RedHaloM2B.Materials
{
    public class RedHaloPBRMtl
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("source_name")]
        public string SourceName { get; set; }

        [JsonProperty("type")]
        public string MaterialType { get; set; }

        [JsonProperty("use_roughness")]
        public bool UseRoughness { get; set; }

        #region Diffuse
        [JsonProperty("diffuse_color")]
        public string DiffuseColor { get; set; }

        [JsonProperty("diffuse_texmap")]
        public TexmapInfo DiffuseTexmap { get; set; }

        [JsonProperty("diffuse_roughness")]
        public float DiffuseRoughness { get; set; }

        [JsonProperty("diffuse_roughness_texmap")]
        public TexmapInfo DiffuseRoughnessTexmap { get; set; }
        #endregion

        #region Metallic
        [JsonProperty("metallic")]
        public float Metallic { get; set; }

        [JsonProperty("metallic_texmap")]
        public TexmapInfo MetallicTexmap { get; set; }
        #endregion

        #region IOR
        [JsonProperty("ior")]
        public float IOR { get; set; }

        [JsonProperty("ior_texmap")]
        public TexmapInfo IORTexmap { get; set; }
        #endregion

        #region Opacity / Alpha
        [JsonProperty("opacity")]
        public float Opacity { get; set; }

        [JsonProperty("opacity_texmap")]
        public TexmapInfo OpacityTexmap { get; set; }
        #endregion

        #region Subsurface
        [JsonProperty("sss_color")]
        public string SubsurfaceColor { get; set; }

        [JsonProperty("ss_color_texmap")]
        public TexmapInfo SubsurfaceColorTexmap { get; set; }

        [JsonProperty("sss_weight")]
        public float SubsurfaceWeight { get; set; }

        [JsonProperty("sss_weight_texmap")]
        public TexmapInfo SubsurfaceWeightTexmap { get; set; }

        [JsonProperty("sss_radius")]
        public string SubsurfaceRadius { get; set; }

        [JsonProperty("sss_radius_texmap")]
        public TexmapInfo SubsurfaceRadiusTexmap { get; set; }

        [JsonProperty("sss_scale")]
        public float SubsurfaceScale { get; set; }

        [JsonProperty("sss_scale_texmap")]
        public TexmapInfo SubsurfaceScaleTexmap { get; set; }

        [JsonProperty("sss_anisotropy")]
        public float SubsurfaceAnisotropy { get; set; }

        [JsonProperty("sss_anisotropy_texmap")]
        public TexmapInfo SubsurfaceAnisotropyTexmap { get; set; }

        #endregion

        #region Specular / Reflection
        [JsonProperty("specular")]
        public float Specular { get; set; }

        [JsonProperty("specular_texmap")]
        public TexmapInfo SpecularTexmap { get; set; }

        [JsonProperty("specular_roughness")]
        public float SpecularRoughness { get; set; }

        [JsonProperty("specular_roughness_texmap")]
        public TexmapInfo SpecularRoughnessTexmap { get; set; }

        #endregion

        #region Anisotropic
        [JsonProperty("anisotropic")]
        public float Anisotropic { get; set; }

        [JsonProperty("anisotropic_texmap")]
        public TexmapInfo AnisotropicTexmap { get; set; }

        [JsonProperty("anisotropic_rotation")]
        public float AnisotropicRotation { get; set; }

        [JsonProperty("anisotropic_rotation_texmap")]
        public TexmapInfo AnisotropicRotationTexmap { get; set; }

        #endregion

        #region Transmission / Refraction
        [JsonProperty("transmission")]
        public float Refraction { get; set; }

        [JsonProperty("transmission_texmap")]
        public TexmapInfo RefractionTexmap { get; set; }

        [JsonProperty("transmission_roughness")]
        public float RefractionRoughness { get; set; }

        [JsonProperty("transmission_roughness_texmap")]
        public TexmapInfo RefractionRoughnessTexmap { get; set; }

        #endregion

        #region Coat
        [JsonProperty("coat")]
        public float Coat { get; set; }

        [JsonProperty("coat_texmap")]
        public TexmapInfo CoatTexmap { get; set; }

        [JsonProperty("coat_roughness")]
        public float CoatRoughness { get; set; }

        [JsonProperty("coat_roughness_texmap")]
        public TexmapInfo CoatRoughnessTexmap { get; set; }

        [JsonProperty("coat_ior")]
        public float CoatIOR { get; set; }

        [JsonProperty("coat_ior_texmap")]
        public TexmapInfo CoatIORTexmap { get; set; }

        [JsonProperty("coat_tint")]
        public string CoatTint { get; set; }

        [JsonProperty("coat_tint_texmap")]
        public TexmapInfo CoatTintTexmap { get; set; }

        [JsonProperty("coat_bump")]
        public float CoatBump { get; set; }

        [JsonProperty("coat_bump_texmap")]
        public TexmapInfo CoatBumpTexmap { get; set; }

        #endregion

        #region Sheen
        [JsonProperty("sheen_weight")]
        public float SheenWeight { get; set; }

        [JsonProperty("sheen_color")]
        public string SheenColor { get; set; }

        [JsonProperty("sheen_color_texmap")]
        public TexmapInfo SheenColorTexmap { get; set; }

        [JsonProperty("sheen_roughness")]
        public float SheenRoughness { get; set; }

        [JsonProperty("sheen_roughness_texmap")]
        public TexmapInfo SheenRoughnessTexmap { get; set; }

        #endregion

        #region Emission
        [JsonProperty("emission_color")]
        public string EmissionColor { get; set; }

        [JsonProperty("emission_color_texmap")]
        public TexmapInfo EmissionColorTexmap { get; set; }

        [JsonProperty("emission_strength")]
        public float EmissionStrength { get; set; }

        [JsonProperty("emission_strength_texmap")]
        public TexmapInfo EmissionStrengthTexmap { get; set; }

        #endregion

        #region Bump
        [JsonProperty("bump")]
        public float Bump { get; set; }

        [JsonProperty("bump_texmap")]
        public TexmapInfo BumpTexmap { get; set; }

        #endregion

        #region Displacement
        //[JsonProperty("displacement")]
        //public float Displacement { get; set; }

        [JsonProperty("displacement_min")]
        public float DisplacementMin { get; set; }

        [JsonProperty("displacement_max")]
        public float DisplacementMax { get; set; }

        [JsonProperty("displacement_waterlevel")]
        public float DisplacementWaterLevel { get; set; }

        [JsonProperty("displacement_scale")]
        public float DisplacementScale { get; set; }

        [JsonProperty("displacement_texmap_amount")]
        public float DisplacementTexmapAmount { get; set; }

        [JsonProperty("displacement_texmap")]
        public TexmapInfo DisplacementTexmap { get; set; }

        #endregion

        #region Thin Film
        [JsonProperty("thinfilm")]
        public float ThinFilm { get; set; }

        [JsonProperty("thinfilm_texmap")]
        public TexmapInfo ThinFilmTexmap { get; set; }

        [JsonProperty("thinfilm_ior")]
        public float ThinFilmIOR { get; set; }

        [JsonProperty("thinfilm_ior_texmap")]
        public TexmapInfo ThinFilmIORTexmap { get; set; }

        #endregion

        #region Translucent
        [JsonProperty("translucent_color")]
        public string TranslucentColor { get; set; }

        [JsonProperty("translucent_texmap")]
        public TexmapInfo TranslucentTexmap { get; set; }

        #endregion


        #region TestArea
        public float[] floats = new float[4];
        #endregion

        public RedHaloPBRMtl()
        {
            UseRoughness = false;

            Bump = 0.1f;
        }
    }
}