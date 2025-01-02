using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RedHaloM2B.Materials
{
    internal class RedHaloPBRMtl : BaseMtl
    {
        #region Diffuse
        [XmlElement("diffusecolor")]
        public string DiffuseColor { get; set; }
        [XmlElement("diffuseroughness")]
        public float DiffuseRoughness { get; set; }
        //[XmlElement("diffusetexture")]
        //public List<BaseTexture> DiffuseTexture { get; set; }
        #endregion

        #region Metallic
        [XmlElement("metallic")]
        public float Metallic { get; set; }
        #endregion

        #region IOR
        [XmlElement("ior")]
        public float IOR { get; set; }
        #endregion

        #region Opacity / Alpha
        [XmlElement("opacity")]
        public float Opacity { get; set; }
        #endregion

        #region Subsurface
        [XmlElement("subsurfaceweight")]
        public float SubsurfaceWeight { get; set; }
        [XmlElement("subsurfaceradius")]
        public string SubsurfaceRadius { get; set; }
        [XmlElement("subsurfacescale")]
        public float SubsurfaceScale { get; set; }
        [XmlElement("subsurfaceanisotropy")]
        public float SubsurfaceAnisotropy { get; set; }
        #endregion

        #region Specular / Reflection
        [XmlElement("specular")]
        public float Specular { get; set; }
        [XmlElement("specularroughness")]
        public float SpecularRoughness { get; set; }
        #endregion

        #region Anisotropic
        [XmlElement("anisotropic")]
        public float Anisotropic { get; set; }
        [XmlElement("anisotropicrotation")]
        public float AnisotropicRotation { get; set; }
        #endregion

        #region Transmission / Refraction
        [XmlElement("transmission")]
        public float Transmission { get; set; }
        [XmlElement("transmissionroughness")]
        public float TransmissionRoughness { get; set; }
        #endregion

        #region Coat
        [XmlElement("coat")]
        public float Coat { get; set; }
        [XmlElement("coatroughness")]
        public float CoatRoughness { get; set; }
        [XmlElement("coatior")]
        public float CoatIOR { get; set; }
        [XmlElement("coattint")]
        public string CoatTint { get; set; }
        #endregion

        #region Sheen
        [XmlElement("sheen")]
        public float Sheen { get; set; }
        [XmlElement("sheenroughness")]
        public float SheenRoughness { get; set; }
        #endregion

        #region Emission
        [XmlElement("emission")]
        public string Emission { get; set; }
        [XmlElement("emissionstrength")]
        public float EmissionStrength { get; set; }
        #endregion

        #region Thin Film
        [XmlElement("thinfilm")]
        public float ThinFilm { get; set; }
        [XmlElement("thinfilmior")]
        public float ThinFilmIOR { get; set; }
        #endregion

        #region Translucent
        [XmlElement("translucentcolor")]
        public string TranslucentColor { get; set; }

        #endregion

        public RedHaloPBRMtl()
        {
            DiffuseColor = "1,1,1,1";
            DiffuseRoughness = 0.5f;
            Metallic = 0;
            IOR = 1.5f;
            Opacity = 1.0f;
            SubsurfaceWeight = 0;
            SubsurfaceRadius = "subsurfaceRadius";
            SubsurfaceScale = 0;
            SubsurfaceAnisotropy = 0;
            Specular = 0;
            SpecularRoughness = 0;
            Anisotropic = 0;
            AnisotropicRotation = 0;
            Transmission = 0;
            TransmissionRoughness = 0;
            Coat = 0;
            CoatRoughness = 0;
            CoatIOR = 1.5f;
            CoatTint = "coatTint";
            Sheen = 0;
            SheenRoughness = 0;
            Emission = "0";
            EmissionStrength = 0;
            ThinFilm = 0;
            ThinFilmIOR = 1.5f;
            TranslucentColor = "translucentColor";
        }

    }
}
