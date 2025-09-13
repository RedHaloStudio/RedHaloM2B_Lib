using Newtonsoft.Json;
using RedHaloM2B.Textures;
using System.Collections.Generic;

namespace RedHaloM2B.Materials
{
    public class DiffuseGroup
    {
        [JsonProperty("color")]
        public float[] DiffuseColor { get; set; } = [0.8f, 0.8f, 0.8f, 1f];

        [JsonProperty("texmap")]
        public TexmapInfo DiffuseTexmap { get; set; }

        [JsonProperty("roughness")]
        public float DiffuseRoughness { get; set; }

        [JsonProperty("roughness_texmap")]
        public TexmapInfo DiffuseRoughnessTexmap { get; set; }
    }

    public class MetallicGroup
    {
        [JsonProperty("value")]
        public float Metallic { get; set; } = 0f;

        [JsonProperty("texmap")]
        public TexmapInfo MetallicTexmap { get; set; }
    }

    public class IORGroup
    {
        [JsonProperty("value")]
        public float IOR { get; set; } = 1.50f;

        [JsonProperty("texmap")]
        public TexmapInfo IORTexmap { get; set; }
    }

    public class OpacityGroup
    {
        [JsonProperty("value")]
        public float Opacity { get; set; } = 1f;

        [JsonProperty("texmap")]
        public TexmapInfo OpacityTexmap { get; set; }
    }

    public class SubsurfaceGroup
    {
        [JsonProperty("color")]
        public float[] SubsurfaceColor { get; set; } = new float[4];

        [JsonProperty("color_texmap")]
        public TexmapInfo SubsurfaceColorTexmap { get; set; }

        [JsonProperty("weight")]
        public float SubsurfaceWeight { get; set; }

        [JsonProperty("weight_texmap")]
        public TexmapInfo SubsurfaceWeightTexmap { get; set; }

        [JsonProperty("radius")]
        public float[] SubsurfaceRadius { get; set; } = new float[3];

        [JsonProperty("radius_texmap")]
        public TexmapInfo SubsurfaceRadiusTexmap { get; set; }

        [JsonProperty("scale")]
        public float SubsurfaceScale { get; set; }

        [JsonProperty("scale_texmap")]
        public TexmapInfo SubsurfaceScaleTexmap { get; set; }

        [JsonProperty("anisotropy")]
        public float SubsurfaceAnisotropy { get; set; }

        [JsonProperty("anisotropy_texmap")]
        public TexmapInfo SubsurfaceAnisotropyTexmap { get; set; }
    }

    public class ReflectionGroup
    {
        [JsonProperty("specular")]
        public float Specular { get; set; }

        [JsonProperty("texmap")]
        public TexmapInfo SpecularTexmap { get; set; }

        [JsonProperty("roughness")]
        public float SpecularRoughness { get; set; }

        [JsonProperty("roughness_texmap")]
        public TexmapInfo SpecularRoughnessTexmap { get; set; }
    }

    public class AnisotropicGroup
    {
        [JsonProperty("value")]
        public float Anisotropic { get; set; }

        [JsonProperty("texmap")]
        public TexmapInfo AnisotropicTexmap { get; set; }

        [JsonProperty("rotation")]
        public float AnisotropicRotation { get; set; }

        [JsonProperty("rotation_texmap")]
        public TexmapInfo AnisotropicRotationTexmap { get; set; }
    }

    public class TransmissionGroup
    {
        [JsonProperty("value")]
        public float Refraction { get; set; }

        [JsonProperty("texmap")]
        public TexmapInfo RefractionTexmap { get; set; }

        [JsonProperty("roughness")]
        public float RefractionRoughness { get; set; }

        [JsonProperty("roughness_texmap")]
        public TexmapInfo RefractionRoughnessTexmap { get; set; }
    }

    public class CoatGroup
    {
        [JsonProperty("value")]
        public float Coat { get; set; }

        [JsonProperty("texmap")]
        public TexmapInfo CoatTexmap { get; set; }

        [JsonProperty("roughness")]
        public float CoatRoughness { get; set; }

        [JsonProperty("roughness_texmap")]
        public TexmapInfo CoatRoughnessTexmap { get; set; }

        [JsonProperty("ior")]
        public float CoatIOR { get; set; }

        [JsonProperty("ior_texmap")]
        public TexmapInfo CoatIORTexmap { get; set; }

        [JsonProperty("tint")]
        public float[] CoatTint { get; set; } = new float[4];

        [JsonProperty("tint_texmap")]
        public TexmapInfo CoatTintTexmap { get; set; }

        [JsonProperty("bump")]
        public float CoatBump { get; set; }

        [JsonProperty("bump_texmap")]
        public TexmapInfo CoatBumpTexmap { get; set; }
    }

    public class SheenGroup
    {
        [JsonProperty("value")]
        public float SheenWeight { get; set; }

        [JsonProperty("color")]
        public float[] SheenColor { get; set; } = [1, 1, 1, 1];

        [JsonProperty("texmap")]
        public TexmapInfo SheenColorTexmap { get; set; }

        [JsonProperty("roughness")]
        public float SheenRoughness { get; set; }

        [JsonProperty("roughness_texmap")]
        public TexmapInfo SheenRoughnessTexmap { get; set; }
    }

    public class EmissionGroup
    {
        [JsonProperty("color")]
        public float[] EmissionColor { get; set; } = [1, 1, 1, 1];

        [JsonProperty("texmap")]
        public TexmapInfo EmissionColorTexmap { get; set; }

        [JsonProperty("strength")]
        public float EmissionStrength { get; set; } = 0f;

        [JsonProperty("strength_texmap")]
        public TexmapInfo EmissionStrengthTexmap { get; set; }
    }

    public class BumpGroup
    {
        [JsonProperty("value")]
        public float Bump { get; set; } = 0.03f;

        [JsonProperty("texmap")]
        public TexmapInfo BumpTexmap { get; set; }
    }

    public class DisplacementGroup
    {
        [JsonProperty("min")]
        public float DisplacementMin { get; set; }

        [JsonProperty("max")]
        public float DisplacementMax { get; set; }

        [JsonProperty("waterlevel")]
        public float DisplacementWaterLevel { get; set; } = 0.5f;

        [JsonProperty("scale")]
        public float DisplacementScale { get; set; }

        [JsonProperty("texmap_amount")]
        public float DisplacementTexmapAmount { get; set; } = 1f;

        [JsonProperty("texmap")]
        public TexmapInfo DisplacementTexmap { get; set; }
    }

    public class ThinFilmGroup
    {
        [JsonProperty("value")]
        public float ThinFilm { get; set; }

        [JsonProperty("texmap")]
        public TexmapInfo ThinFilmTexmap { get; set; }

        [JsonProperty("ior")]
        public float ThinFilmIOR { get; set; } = 1.5f;

        [JsonProperty("ior_texmap")]
        public TexmapInfo ThinFilmIORTexmap { get; set; }
    }

    public class TranslucentGroup
    {
        [JsonProperty("color")]
        public string TranslucentColor { get; set; }

        [JsonProperty("texmap")]
        public TexmapInfo TranslucentTexmap { get; set; }
    }

    public class RedHaloPBRMtl : RedHaloBaseMtl
    {
        [JsonProperty("use_roughness")]
        public bool UseRoughness { get; set; }

        [JsonProperty("diffuse")]
        public DiffuseGroup DiffuseGroup { get; set; } = new DiffuseGroup();

        [JsonProperty("metallic")]
        public MetallicGroup MetallicGroup { get; set; } = new MetallicGroup();

        [JsonProperty("ior")]
        public IORGroup IORGroup { get; set; } = new IORGroup();

        [JsonProperty("opacity")]
        public OpacityGroup OpacityGroup { get; set; } = new OpacityGroup();

        [JsonProperty("subsurface")]
        public SubsurfaceGroup SubsurfaceGroup { get; set; } = new SubsurfaceGroup();

        [JsonProperty("reflection")]
        public ReflectionGroup ReflectionGroup { get; set; } = new ReflectionGroup();

        [JsonProperty("anisotropic")]
        public AnisotropicGroup AnisotropicGroup { get; set; } = new AnisotropicGroup();

        [JsonProperty("transmission")]
        public TransmissionGroup TransmissionGroup { get; set; } = new TransmissionGroup();

        [JsonProperty("coat")]
        public CoatGroup CoatGroup { get; set; } = new CoatGroup();

        [JsonProperty("sheen")]
        public SheenGroup SheenGroup { get; set; } = new SheenGroup();

        [JsonProperty("emission")]
        public EmissionGroup EmissionGroup { get; set; } = new EmissionGroup();

        [JsonProperty("bump")]
        public BumpGroup BumpGroup { get; set; } = new BumpGroup();

        [JsonProperty("displacement")]
        public DisplacementGroup DisplacementGroup { get; set; } = new DisplacementGroup();

        [JsonProperty("thinfilm")]
        public ThinFilmGroup ThinFilmGroup { get; set; } = new ThinFilmGroup();

        [JsonProperty("translucent")]
        public TranslucentGroup TranslucentGroup { get; set; } = new TranslucentGroup();


        public RedHaloPBRMtl()
        {
            UseRoughness = false;
        }
    }

}