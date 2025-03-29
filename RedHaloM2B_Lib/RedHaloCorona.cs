using Autodesk.Max.Plugins;
using Autodesk.Max;
using RedHaloM2B.RedHaloUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedHaloM2B;
using RedHaloM2B.Materials;

namespace RedHaloM2B
{
    internal class RedHaloExporter
    {
        public static void DumpCoronaMaterial(RedHaloPBRMtl PBRMtl, IMtl material)
        {
            ITexmap texmap = null;
            IColor maxClr = RedHaloCore.Global.Color.Create(0.8, 0.8, 0.8);
            int sss_on = 0;
            bool enabled = false;

            var CR_Legacy_Diffuse_Color = RedHaloTools.GetValeByID<IColor>(material, 0, 0);
            var CR_Legacy_Diffuse_Level = RedHaloTools.GetValeByID<float>(material, 0, 6);
            var CR_Legacy_Diffuse_Color_Final = CR_Legacy_Diffuse_Color.MultiplyBy(CR_Legacy_Diffuse_Level);

            var CR_Legacy_Reflect_IOR = RedHaloTools.GetValeByID<float>(material, 0, 11);
            var CR_Legacy_Refract_IOR = RedHaloTools.GetValeByID<float>(material, 0, 62);

            var CR_Legacy_Reflect_Level = RedHaloTools.GetValeByID<float>(material, 0, 7);
            var CR_Legacy_Reflect_Color = RedHaloTools.GetValeByID<IColor>(material, 0, 9);
            var CR_Legacy_Reflect_Final = CR_Legacy_Reflect_Color.MultiplyBy(CR_Legacy_Reflect_Level);

            var CR_Legacy_Refract_Level = RedHaloTools.GetValeByID<float>(material, 0, 8);
            var CR_Legacy_Refract_Color = RedHaloTools.GetValeByID<IColor>(material, 0, 27);
            var CR_Legacy_Refract_Final = CR_Legacy_Refract_Color.MultiplyBy(CR_Legacy_Refract_Level);

            var CR_Legacy_IOR = 1.52f;
            var CR_Legacy_MetallicValue = 0.0f;

            // 判断是不是金属材质
            // 如果反射IOR大于6，设置为金属材质
            if (CR_Legacy_Reflect_IOR > 6)
            {
                CR_Legacy_MetallicValue = 1.0f;
                if (RedHaloCore.Global.RGBtoHSV(CR_Legacy_Reflect_Color)[2] > 0.02)
                {
                    CR_Legacy_Diffuse_Color = CR_Legacy_Reflect_Color;
                    CR_Legacy_Reflect_Color.White();
                }
                CR_Legacy_IOR = 1.52f;
            }
            else
            {
                CR_Legacy_MetallicValue = 0.0f;
                CR_Legacy_IOR = CR_Legacy_Refract_IOR;
            }

            // 判断是不是有色玻璃,如果是玻璃，设置为白色
            // 条件1：漫反射颜色小于0.08，反射颜色大于0.93，折射颜色大于0.93
            // 条件2： 反射颜色不是白色，说明是有色玻璃，设置漫反射颜色为折射颜色

            if (RedHaloCore.Global.RGBtoHSV(CR_Legacy_Diffuse_Color_Final)[2] < 0.08 &&
                RedHaloCore.Global.RGBtoHSV(CR_Legacy_Reflect_Final)[2] > 0.93 &&
                RedHaloCore.Global.RGBtoHSV(CR_Legacy_Refract_Final)[2] > 0.93)
            {
                CR_Legacy_Diffuse_Color.White();
                CR_Legacy_Refract_Color.White();
            }

            #region Diffuse
            PBRMtl.DiffuseColor = RedHaloTools.IColorToString(CR_Legacy_Diffuse_Color_Final, true);

            // Diffuse Texmap
            texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 12);
            if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 30) == 1)
            {
                PBRMtl.DiffuseTexmap = MaterialUtils.ExportTexmap(texmap);
            }
            #endregion

            #region Specular / Reflection
            maxClr = RedHaloTools.GetValeByID<IColor>(material, 0, 9).MultiplyBy(RedHaloTools.GetValeByID<float>(material, 0, 7));
            PBRMtl.Specular = RedHaloCore.Global.RGBtoHSV(maxClr)[2];

            PBRMtl.SpecularRoughness = RedHaloTools.GetValeByID<float>(material, 0, 10);

            // Specular Texmap
            texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 13);
            if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 31) == 1)
            {
                PBRMtl.SpecularTexmap = MaterialUtils.ExportTexmap(texmap);
            }

            #endregion

            #region Metallic
            PBRMtl.Metallic = CR_Legacy_MetallicValue;
            #endregion

            #region IOR
            // 默认获取折射IOR
            PBRMtl.IOR = CR_Legacy_IOR; // CR_Legacy_Refract_IOR;

            // IOR Texmap
            texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 23);
            if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 40) == 1)
            {
                PBRMtl.IORTexmap = MaterialUtils.ExportTexmap(texmap);
            }
            #endregion

            #region Opacity
            maxClr = RedHaloTools.GetValeByID<IColor>(material, 0, 3).MultiplyBy(RedHaloTools.GetValeByID<float>(material, 0, 9));
            PBRMtl.Opacity = RedHaloCore.Global.RGBtoHSV(maxClr)[2];

            // Opacity Texmap
            texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 17);
            if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 35) == 1)
            {
                PBRMtl.OpacityTexmap = MaterialUtils.ExportTexmap(texmap);
            }
            #endregion

            #region Subsurface
            sss_on = RedHaloTools.GetValeByID<int>(material, 0, 125);
            if (RedHaloTools.GetValeByID<int>(material, 0, 125) == 1)
            {
                PBRMtl.SubsurfaceWeight = RedHaloTools.GetValeByID<float>(material, 0, 109);

                maxClr = RedHaloTools.GetValeByID<IColor>(material, 0, 50);
                PBRMtl.SubsurfaceRadius = $"{maxClr.R},{maxClr.G},{maxClr.B}";

                PBRMtl.SubsurfaceScale = RedHaloTools.GetValeByID<float>(material, 0, 33);
            }
            #endregion

            #region Anisotropic
            PBRMtl.Anisotropic = RedHaloTools.GetValeByID<float>(material, 0, 67);
            PBRMtl.AnisotropicRotation = (float)(RedHaloTools.GetValeByID<float>(material, 0, 68) / 360.0);

            // Anisotropic Texmap
            texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 21);
            if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 38) == 1)
            {
                PBRMtl.AnisotropicTexmap = MaterialUtils.ExportTexmap(texmap);
            }

            // anisiotropic rotation texmap
            texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 22);
            if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 39) == 1)
            {
                PBRMtl.AnisotropicRotationTexmap = MaterialUtils.ExportTexmap(texmap);
            }
            #endregion

            #region Transmission / Refraction
            maxClr = RedHaloTools.GetValeByID<IColor>(material, 0, 2).MultiplyBy(RedHaloTools.GetValeByID<float>(material, 0, 8));
            PBRMtl.Refraction = RedHaloCore.Global.RGBtoHSV(maxClr)[2];
            #endregion

            #region Coat NOT SUPPORTED
            #endregion

            #region Sheen NOT SUPPORTED
            #endregion

            #region EMISSION
            maxClr = RedHaloTools.GetValeByID<IColor>(material, 0, 5);
            PBRMtl.EmissionColor = RedHaloTools.IColorToString(maxClr, true);

            PBRMtl.EmissionStrength = RedHaloTools.GetValeByID<float>(material, 0, 11);

            // Emission Texmap
            texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 29);
            if (texmap != null &&
                RedHaloTools.GetValeByID<int>(material, 0, 45) == 1)
            {
                PBRMtl.EmissionColorTexmap = MaterialUtils.ExportTexmap(texmap);
            }
            #endregion

            #region BUMP
            PBRMtl.Bump = RedHaloTools.GetValeByID<float>(material, 0, 52);

            // Bump Texmap
            texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 18);
            if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 36) == 1)
            {
                PBRMtl.BumpTexmap = MaterialUtils.ExportTexmap(texmap);
            }
            #endregion

            #region DISPLACEMENT
            PBRMtl.DisplacementMax = RedHaloTools.GetValeByID<float>(material, 0, 76);
            PBRMtl.DisplacementMin = RedHaloTools.GetValeByID<float>(material, 0, 74);

            enabled = Convert.ToBoolean(RedHaloTools.GetValeByID<int>(material, 0, 98));
            if (enabled)
            {
                PBRMtl.DisplacementWaterLevel = RedHaloTools.GetValeByID<float>(material, 0, 75);
            }
            else
            {
                PBRMtl.DisplacementWaterLevel = 0.5f;
            }

            // Displacement Texmap
            texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 25);
            if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 42) == 1)
            {
                PBRMtl.DisplacementTexmap = MaterialUtils.ExportTexmap(texmap);
            }

            #endregion

            #region ThinFilm NOT SUPPORTED
            #endregion

            #region TranslucentColor

            #endregion
        }
    }
}
