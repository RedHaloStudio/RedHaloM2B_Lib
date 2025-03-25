using Autodesk.Max;
using RedHaloM2B.Materials;
using RedHaloM2B.Utils;
using System;
using System.Diagnostics;
using System.Windows.Media;

namespace RedHaloM2B
{
    partial class Exporter
    {

        public enum stdMtlShaderType
        {
            ANISOTROPIC = 0,
            BLINN = 1,
            METAL = 2,
            MULTI_LAYER = 3,
            OREN_NAYAR = 4,
            PHONG = 5,
            STRAUSS = 6,
            TRANSLUCENT = 7
        }

        public static RedHaloPBRMtl ExportMaterial(IMtl material, int materialIndex)
        {
            string sourceName = material.Name;
            string newName = $"material_{materialIndex:D4}";

            IColor maxClr = RedHaloCore.Global.Color.Create(0.8, 0.8, 0.8);
            IColor reflectClr = RedHaloCore.Global.Color.Create(0.8, 0.8, 0.8);
            IColor refractClr = RedHaloCore.Global.Color.Create(0.8, 0.8, 0.8);
            IColor refractFogClr = RedHaloCore.Global.Color.Create(1, 1, 1);
            //IColor reflect
            int sss_on = 0;
            float ior = 1.6f;
            float metallic = 0.0f;
            ITexmap texmap = null;

            bool enabled = false;

            RedHaloPBRMtl PBRMtl = new()
            {
                Name = newName,
                SourceName = sourceName,
                MaterialType = material.ClassName(false),
            };
            Debug.Print($"{material.ClassName(false)}");

            switch (material.ClassName(false))
            {
                case "VRayMtl":
                    // Use Roughness
                    PBRMtl.UseRoughness = Convert.ToBoolean(RedHaloTools.GetValeByID<int>(material, 1, 10));                    
                    
                    maxClr = RedHaloTools.GetValeByID<IColor>(material, 0, 3);
                    reflectClr = RedHaloTools.GetValeByID<IColor>(material, 0, 9);
                    refractClr = RedHaloTools.GetValeByID<IColor>(material, 0, 27);
                    sss_on = RedHaloTools.GetValeByID<int>(material, 0, 44);

                    // 判断是不是玻璃,如果是玻璃，设置为白色
                    // 条件1：漫反射颜色小于0.08，反射颜色大于0.93，折射颜色大于0.93
                    // 条件2： 折射颜色不是白色，说明是有色玻璃，设置漫反射颜色为折射颜色
                    if (RedHaloCore.Global.RGBtoHSV(maxClr)[2] < 0.08 &&
                        RedHaloCore.Global.RGBtoHSV(reflectClr)[2] > 0.93 &&
                        RedHaloCore.Global.RGBtoHSV(refractClr)[2] > 0.93)
                    {
                        maxClr.White();
                        refractClr.White();
                    }

                    if (RedHaloCore.Global.RGBtoHSV(reflectClr)[1] > 0.1)
                    {
                        maxClr = reflectClr;
                        reflectClr.White();
                    }

                    /*
                     * 判断是不是有色玻璃
                     * 如果折射颜色不是白色，说明是有色玻璃，设置漫反射颜色为折射颜色
                    */
                    refractFogClr = RedHaloTools.GetValeByID<IColor>(material, 0, 31);

                    if(RedHaloCore.Global.RGBtoHSV(refractFogClr)[1] > 0.01 && sss_on != 6)
                    {
                        maxClr = refractFogClr;
                        refractFogClr.White();
                    }

                    /*                
                     * LockIOR开启的话，使用折射的IOR，否则就使用反射的IOR
                     */
                    var lockIOR = 0;
                    var useIOR = 0;
                    // 是否锁定IOR
                    lockIOR = RedHaloTools.GetValeByID<int>(material, 0, 20);                    

                    if (lockIOR == 1)
                    {
                        ior = RedHaloTools.GetValeByID<float>(material, 0, 30);
                    }
                    else
                    {
                        ior = RedHaloTools.GetValeByID<float>(material, 0, 17);
                    }

                    // 如果IOR值小于1，设置为1
                    if (ior < 1)
                    {
                        ior = 1;
                    }
                    
                    // 是否使用Fresnel反射
                    useIOR = RedHaloTools.GetValeByID<int>(material, 0, 13);
                    if (useIOR == 0)
                    {
                        ior = 2;
                    }
                    
                    #region Diffuse
                    PBRMtl.DiffuseColor = $"{maxClr.R},{maxClr.G},{maxClr.B},1";

                    PBRMtl.DiffuseRoughness = RedHaloTools.GetValeByID<float>(material, 0, 14);

                    // Diffuse Texmap
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 67);
                    if (texmap != null)
                    {
                        PBRMtl.DiffuseTexmap = MaterialUtils.GetTexmap(texmap);
                    }

                    // Diffuse Roughness
                    PBRMtl.DiffuseRoughness = RedHaloTools.GetValeByID<float>(material, 0, 4);

                    // Diffuse Roughness Texmap
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 68);
                    if (texmap != null)
                    {
                        PBRMtl.DiffuseRoughnessTexmap = MaterialUtils.GetTexmap(texmap);
                    }
                    #endregion

                    #region Specular / Reflection
                    PBRMtl.Specular = RedHaloCore.Global.RGBtoHSV(reflectClr)[2];

                    PBRMtl.SpecularRoughness = RedHaloTools.GetValeByID<float>(material, 0, 10);

                    // Specular / Reflection Texmap
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 70);
                    if (texmap != null)
                    {
                        PBRMtl.SpecularTexmap = MaterialUtils.GetTexmap(texmap);
                    }

                    // SpecularRoughness / Reflection Texmap
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 71);
                    if (texmap != null)
                    {
                        PBRMtl.SpecularRoughnessTexmap = MaterialUtils.GetTexmap(texmap);
                    }

                    #endregion

                    #region Metallic

                    /*
                     * 如果IOR值高于6，设置Metallic为真,在Blender里面测试过，当高于5.5的时候，和金属材质的效果差不多
                     */
                    metallic = RedHaloTools.GetValeByID<float>(material, 0, 18);
                    if (RedHaloTools.GetValeByID<float>(material, 0, 17) > 6)
                    {
                        metallic = 1.0f;
                        ior = 1.6f;
                    }
                    PBRMtl.Metallic = metallic;                     

                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 73);
                    if (texmap != null)
                    {
                        PBRMtl.MetallicTexmap = MaterialUtils.GetTexmap(texmap);
                    }
                    #endregion

                    #region Opacity

                    PBRMtl.Opacity = RedHaloTools.GetValeByID<float>(material, 3, 40) / 100.0f;
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 3, 38);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 3, 39) == 1)
                    {
                        PBRMtl.OpacityTexmap = MaterialUtils.GetTexmap(texmap);
                        texmap = null;
                    }

                    #endregion

                    #region Subsurface

                    if (sss_on == 6)
                    {
                        PBRMtl.SubsurfaceColor = RedHaloTools.IColorToString(RedHaloTools.GetValeByID<IColor>(material, 0, 50));
                        texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 79);
                        if (texmap != null )
                        {
                            PBRMtl.SubsurfaceColorTexmap = MaterialUtils.GetTexmap(texmap);
                        }

                        PBRMtl.SubsurfaceWeight = RedHaloTools.GetValeByID<float>(material, 0, 45);
                        texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 80);
                        if (texmap != null)
                        {
                            PBRMtl.SubsurfaceWeightTexmap = MaterialUtils.GetTexmap(texmap);
                        }

                        PBRMtl.SubsurfaceRadius = RedHaloTools.IColorToString(refractFogClr);
                        texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 77);
                        if (texmap != null)
                        {
                            PBRMtl.SubsurfaceRadiusTexmap = MaterialUtils.GetTexmap(texmap);
                        }


                        PBRMtl.SubsurfaceScale = RedHaloTools.GetValeByID<float>(material, 0, 33);
                        texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 78);
                        if (texmap != null && RedHaloTools.GetValeByID<int>(material, 3, 25) == 1)
                        {
                            PBRMtl.SubsurfaceScaleTexmap = MaterialUtils.GetTexmap(texmap);
                        }
                    }
                    #endregion

                    #region Anisotropic
                    PBRMtl.Anisotropic = RedHaloTools.GetValeByID<float>(material, 1, 1);
                    PBRMtl.AnisotropicRotation = (float)(RedHaloTools.GetValeByID<float>(material, 1, 2) % 360 / 360);

                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 1, 12);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 3, 45) == 1)
                    {
                        PBRMtl.AnisotropicTexmap = MaterialUtils.GetTexmap(texmap);
                    }

                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 1, 13);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 3, 48) == 1)
                    {
                        PBRMtl.AnisotropicRotationTexmap = MaterialUtils.GetTexmap(texmap);
                    }
                    #endregion

                    #region Transmission                    

                    PBRMtl.Refraction = RedHaloCore.Global.RGBtoHSV(refractClr)[2];

                    // Transmission Texmap
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 74);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 3, 7) == 1)
                    {
                        PBRMtl.RefractionTexmap = MaterialUtils.GetTexmap(texmap);
                        texmap = null;
                    }

                    PBRMtl.RefractionRoughness = RedHaloTools.GetValeByID<float>(material, 0, 28);
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 75);
                    if (texmap != null)
                    {
                        PBRMtl.RefractionRoughnessTexmap = MaterialUtils.GetTexmap(texmap);
                    }

                    #endregion

                    #region Coat
                    PBRMtl.Coat = RedHaloTools.GetValeByID<float>(material, 0, 55);
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 3, 74);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 3, 75) == 1)
                    {
                        PBRMtl.CoatTexmap = MaterialUtils.GetTexmap(texmap);
                        texmap = null;
                    }

                    PBRMtl.CoatRoughness = RedHaloTools.GetValeByID<float>(material, 0, 56);
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 3, 77);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 3, 78) == 1)
                    {
                        PBRMtl.CoatRoughnessTexmap = MaterialUtils.GetTexmap(texmap);
                        texmap = null;
                    }

                    PBRMtl.CoatIOR = RedHaloTools.GetValeByID<float>(material, 0, 57);
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 3, 80);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 3, 81) == 1)
                    {
                        PBRMtl.CoatIORTexmap = MaterialUtils.GetTexmap(texmap);
                        texmap = null;
                    }

                    PBRMtl.CoatTint = RedHaloTools.IColorToString(RedHaloTools.GetValeByID<IColor>(material, 0, 54));
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 3, 71);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 3, 72) == 1)
                    {
                        PBRMtl.CoatTintTexmap = MaterialUtils.GetTexmap(texmap);
                        texmap = null;
                    }

                    PBRMtl.CoatBump = RedHaloTools.GetValeByID<float>(material, 0, 60);
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 3, 83);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 3, 84) == 1)
                    {
                        PBRMtl.CoatBumpTexmap = MaterialUtils.GetTexmap(texmap);
                        texmap = null;
                    }

                    #endregion

                    #region Sheen
                    maxClr = RedHaloTools.GetValeByID<IColor>(material, 0, 52);

                    PBRMtl.SheenWeight = RedHaloCore.Global.RGBtoHSV(maxClr)[2];                    

                    PBRMtl.SheenColor = RedHaloTools.IColorToString(maxClr, true);
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 3, 65);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 3, 66) == 1)
                    {
                        PBRMtl.SheenColorTexmap = MaterialUtils.GetTexmap(texmap);
                        texmap = null;
                    }

                    PBRMtl.SheenRoughness = RedHaloTools.GetValeByID<float>(material, 0, 53);
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 88);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 3, 69) == 1)
                    {
                        PBRMtl.SheenRoughnessTexmap = MaterialUtils.GetTexmap(texmap);
                        texmap = null;
                    }
                    #endregion

                    #region Emission

                    maxClr = RedHaloTools.GetValeByID<IColor>(material, 0, 5);
                    PBRMtl.EmissionColor = RedHaloTools.IColorToString(maxClr, true);
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 3, 56);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 3, 57) == 1)
                    {
                        PBRMtl.EmissionColorTexmap = MaterialUtils.GetTexmap(texmap);
                        texmap = null;
                    }

                    PBRMtl.EmissionStrength = RedHaloTools.GetValeByID<float>(material, 0, 7);
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 3, 58);
                    if (texmap != null)
                    {
                        PBRMtl.EmissionColorTexmap = MaterialUtils.GetTexmap(texmap);
                        texmap = null;
                    }

                    #endregion

                    #region ThinFilm

                    if (RedHaloTools.GetValeByID<int>(material, 0, 61) == 1)
                    {
                        PBRMtl.ThinFilm = RedHaloTools.GetValeByID<float>(material, 0, 62);
                        texmap = RedHaloTools.GetValeByID<ITexmap>(material, 3, 86);
                        if (texmap != null && RedHaloTools.GetValeByID<int>(material, 3, 87) == 1)
                        {
                            PBRMtl.ThinFilmTexmap = MaterialUtils.GetTexmap(texmap);
                            texmap = null;
                        }
                    }
                    else
                    {
                        PBRMtl.ThinFilm = 0;
                    }

                    PBRMtl.ThinFilmIOR = RedHaloTools.GetValeByID<float>(material, 0, 64);
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 3, 89);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 3, 90) == 1)
                    {
                        PBRMtl.ThinFilmIORTexmap = MaterialUtils.GetTexmap(texmap);
                        texmap = null;
                    }
                    
                    #endregion

                    #region TranslucentColor

                    #endregion

                    break;
                
                case "VRay2SidedMtl":

                    break;
                
                case "CoronaLegacyMtl":                    
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
                        PBRMtl.DiffuseTexmap = MaterialUtils.GetTexmap(texmap);
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
                        PBRMtl.SpecularTexmap = MaterialUtils.GetTexmap(texmap);
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
                        PBRMtl.IORTexmap = MaterialUtils.GetTexmap(texmap);
                    }
                    #endregion

                    #region Opacity
                    maxClr = RedHaloTools.GetValeByID<IColor>(material, 0, 3).MultiplyBy(RedHaloTools.GetValeByID<float>(material, 0, 9));
                    PBRMtl.Opacity = RedHaloCore.Global.RGBtoHSV(maxClr)[2];

                    // Opacity Texmap
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 17);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 35) == 1)
                    {
                        PBRMtl.OpacityTexmap = MaterialUtils.GetTexmap(texmap);
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
                        PBRMtl.AnisotropicTexmap = MaterialUtils.GetTexmap(texmap);
                    }

                    // anisiotropic rotation texmap
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 22);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 39) == 1)
                    {
                        PBRMtl.AnisotropicRotationTexmap = MaterialUtils.GetTexmap(texmap);
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
                        PBRMtl.EmissionColorTexmap = MaterialUtils.GetTexmap(texmap);
                    }
                    #endregion

                    #region BUMP
                    PBRMtl.Bump = RedHaloTools.GetValeByID<float>(material, 0, 52);

                    // Bump Texmap
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 18);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 36) == 1)
                    {
                        PBRMtl.BumpTexmap = MaterialUtils.GetTexmap(texmap);
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
                        PBRMtl.DisplacementTexmap = MaterialUtils.GetTexmap(texmap);
                    }

                    #endregion

                    #region ThinFilm NOT SUPPORTED
                    #endregion

                    #region TranslucentColor

                    #endregion

                    break;

                case "\rCoronaPhysicalMtl":
                case "CoronaPhysicalMtl":
                    var useRoughness = !Convert.ToBoolean(RedHaloTools.GetValeByID<int>(material, 0, 124));
                    PBRMtl.UseRoughness = useRoughness;

                    var useMetallic = false;
                    useMetallic = RedHaloTools.GetValeByID<int>(material, 0, 5) == 1;

                    var useThinMode = false;
                    useThinMode = RedHaloTools.GetValeByID<int>(material, 0, 34) == 1;

                    #region DIFFUSE
                    var CP_Diffuse_Color = RedHaloTools.GetValeByID<IColor>(material, 0, 0);
                    var CP_Diffuse_Level = RedHaloTools.GetValeByID<float>(material, 0, 1);
                    var CP_Diffuse_Color_Final = CP_Diffuse_Color.MultiplyBy(CP_Diffuse_Level);

                    PBRMtl.DiffuseColor = RedHaloTools.IColorToString(CP_Diffuse_Color_Final, true);
                    // Diffuse Texmap
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 2);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 3) == 1)
                    {
                        PBRMtl.DiffuseTexmap = MaterialUtils.GetTexmap(texmap);
                    }

                    #endregion

                    #region REFLECTION / SPECULAR / GLOSSINESS / ROUGHNESS
                    var CP_Reflect_Level = RedHaloTools.GetValeByID<float>(material, 0, 12);
                    PBRMtl.Specular = CP_Reflect_Level;

                    // Reflection Texmap
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 13);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 14) == 1)
                    {
                        PBRMtl.SpecularTexmap = MaterialUtils.GetTexmap(texmap);
                    }
                    #endregion

                    #region METALLIC
                    PBRMtl.Metallic = useMetallic ? 1.0f : 0.0f;
                    #endregion

                    #region IOR
                    PBRMtl.IOR = RedHaloTools.GetValeByID<float>(material, 0, 24);

                    // IOR Texmap
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 25);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 26) == 1)
                    {
                        PBRMtl.IORTexmap = MaterialUtils.GetTexmap(texmap);
                    }
                    #endregion

                    #region OPACITY
                    maxClr = RedHaloTools.GetValeByID<IColor>(material, 0, 6).MultiplyBy(RedHaloTools.GetValeByID<float>(material, 0, 7));
                    PBRMtl.Opacity = RedHaloCore.Global.RGBtoHSV(maxClr)[2];

                    // Opacity Texmap
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 8);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 9) == 1)
                    {
                        PBRMtl.OpacityTexmap = MaterialUtils.GetTexmap(texmap);
                    }
                    #endregion

                    #region SUBSURFACE
                    /* 
                     * Max和Blender中名称的对应名称（MAX --  BLENDER）
                     * sssAmount -- Subsurface Weight
                     * sssRadius -- Subsurface Scale
                     * sssScatterColor -- Subsurface Radius
                    */
                    PBRMtl.SubsurfaceWeight = RedHaloTools.GetValeByID<float>(material, 0, 71);

                    // Subsurface Weight Texmap
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 72);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 73) == 1)
                    {
                        PBRMtl.SubsurfaceWeightTexmap = MaterialUtils.GetTexmap(texmap);
                    }

                    maxClr = RedHaloTools.GetValeByID<IColor>(material, 0, 79);
                    // PBRMtl.SubsurfaceColor = RedHaloTools.IColorToString(maxClr);
                    PBRMtl.SubsurfaceRadius = RedHaloTools.IColorToString(maxClr);

                    // Subsurface Color Texmap
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 80);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 81) == 1)
                    {
                        PBRMtl.SubsurfaceScaleTexmap = MaterialUtils.GetTexmap(texmap);
                    }

                    PBRMtl.SubsurfaceScale = RedHaloTools.GetValeByID<float>(material, 0, 75);

                    // Subsurface Scale Texmap
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 76);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 77) == 1)
                    {
                        PBRMtl.SubsurfaceRadiusTexmap = MaterialUtils.GetTexmap(texmap);
                    }

                    #endregion

                    #region ANISOTROPIC
                    PBRMtl.Anisotropic = RedHaloTools.GetValeByID<float>(material, 0, 16);
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 17);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 18) == 1)
                    {
                        PBRMtl.AnisotropicTexmap = MaterialUtils.GetTexmap(texmap);
                    }

                    PBRMtl.AnisotropicRotation = RedHaloTools.GetValeByID<float>(material, 0, 20);
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 21);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 22) == 1)
                    {
                        PBRMtl.AnisotropicRotationTexmap = MaterialUtils.GetTexmap(texmap);
                    }

                    #endregion

                    #region REFRACTION / TRANSMISSION
                    if (!useMetallic)
                    {
                        PBRMtl.Refraction = RedHaloTools.GetValeByID<float>(material, 0, 28);

                        // Refraction Texmap
                        texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 29);
                        if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 30) == 1)
                        {
                            PBRMtl.RefractionTexmap = MaterialUtils.GetTexmap(texmap);
                        }
                    }
                    #endregion

                    #region COAT
                    PBRMtl.Coat = RedHaloTools.GetValeByID<float>(material, 0, 36);
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 37);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 38) == 1)
                    {
                        PBRMtl.CoatTexmap = MaterialUtils.GetTexmap(texmap);
                    }

                    PBRMtl.CoatRoughness = RedHaloTools.GetValeByID<float>(material, 0, 44);
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 45);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 46) == 1)
                    {
                        PBRMtl.CoatRoughnessTexmap = MaterialUtils.GetTexmap(texmap);
                    }

                    PBRMtl.CoatIOR = RedHaloTools.GetValeByID<float>(material, 0, 40);
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 41);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 42) == 1)
                    {
                        PBRMtl.CoatIORTexmap = MaterialUtils.GetTexmap(texmap);
                    }

                    PBRMtl.CoatTint = RedHaloTools.IColorToString(RedHaloTools.GetValeByID<IColor>(material, 0, 115));
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 116);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 117) == 1)
                    {
                        PBRMtl.CoatTintTexmap = MaterialUtils.GetTexmap(texmap);
                    }

                    PBRMtl.CoatBump = RedHaloTools.GetValeByID<float>(material, 0, 121);
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 119);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 120) == 1)
                    {
                        PBRMtl.CoatBumpTexmap = MaterialUtils.GetTexmap(texmap);
                    }

                    #endregion

                    #region SHEEN
                    PBRMtl.SheenWeight = RedHaloTools.GetValeByID<float>(material, 0, 48);
                    PBRMtl.SheenColor = RedHaloTools.IColorToString(RedHaloTools.GetValeByID<IColor>(material, 0, 52), true);

                    // Sheen Texmap
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 53);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 54) == 1)
                    {
                        PBRMtl.SheenColorTexmap = MaterialUtils.GetTexmap(texmap);
                    }


                    PBRMtl.SheenRoughness = RedHaloTools.GetValeByID<float>(material, 0, 56);
                    // Sheen Roughness Texmap
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 57);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 58) == 1)
                    {
                        PBRMtl.SheenRoughnessTexmap = MaterialUtils.GetTexmap(texmap);
                    }

                    #endregion

                    #region EMISSION
                    PBRMtl.EmissionColor = RedHaloTools.IColorToString(RedHaloTools.GetValeByID<IColor>(material, 0, 89), true);
                    PBRMtl.EmissionStrength = RedHaloTools.GetValeByID<float>(material, 0, 90);

                    // Emission Texmap
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 91);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 92) == 1)
                    {
                        PBRMtl.EmissionColorTexmap = MaterialUtils.GetTexmap(texmap);
                    }
                    #endregion

                    #region THIN FILM NOT SUPPORTED

                    #endregion

                    #region TranslucentColor
                    if(useThinMode)
                    {
                        //PBRMtl.TranslucentColor = RedHaloTools.IColorToString(RedHaloTools.GetValeByID<IColor>(material, 0, 97), true);
                    }
                    #endregion

                    #region BUMP
                    PBRMtl.Bump = RedHaloTools.GetValeByID<float>(material, 0, 102);

                    // Bump Texmap
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 100);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 101) == 1)
                    {
                        PBRMtl.BumpTexmap = MaterialUtils.GetTexmap(texmap);
                    }
                    #endregion

                    #region DISPLACEMENT
                    PBRMtl.DisplacementMax = RedHaloTools.GetValeByID<float>(material, 0, 84);
                    PBRMtl.DisplacementMin = RedHaloTools.GetValeByID<float>(material, 0, 83);

                    enabled = Convert.ToBoolean(RedHaloTools.GetValeByID<int>(material, 0, 85));

                    if (enabled)
                    {
                        PBRMtl.DisplacementWaterLevel = RedHaloTools.GetValeByID<float>(material, 0, 86);
                    }
                    else
                    {
                        PBRMtl.DisplacementWaterLevel = 0.5f;
                    }

                    // Displacement Texmap
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 87);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 88) == 1)
                    {
                        PBRMtl.DisplacementTexmap = MaterialUtils.GetTexmap(texmap);
                    }
                    #endregion

                    break;

                case "VRayLightMtl":                    
                    //RedHaloTools.GetParams(material);
                    break;
                case "CoronaLightMtl":
                    //RedHaloTools.GetParams(material);

                    break;

                case "Standard (Legacy)":
                case "StandardMaterial":
                    IStdMat2 stdMat = material as IStdMat2;

                    var shaderType = RedHaloTools.GetValeByID<int>(material, 0, 0);
                    var std_specular_color = stdMat.GetSpecular(0);

                    /*
                     * 0 - Ambient Color
                     * 1 - Diffuse Color
                     * 2 - Specular Color
                     * 3 - Specular Level
                     * 4 - Glossiness
                     * 5 - Self-Illumination
                     * 6 - Opacity
                     * 7 - Filter Color
                     * 8 - Bump
                     * 9 - Reflection
                     * 10 - Refraction
                     * 11 - Displacement
                     * 12 - 23 None
                    */

                    #region DIFFUSE
                    maxClr = material.GetDiffuse(0, false);
                    PBRMtl.DiffuseColor = RedHaloTools.IColorToString(maxClr, true);
                    
                    // Diffuse Texmap
                    texmap = stdMat.GetSubTexmap(1);
                    if (texmap != null && stdMat.MapEnabled(1))
                    {
                        PBRMtl.DiffuseTexmap = MaterialUtils.GetTexmap(texmap);
                    }

                    #endregion

                    #region REFLECTION / SPECULAR / GLOSSINESS / ROUGHNESS
                    //PBRMtl.Specular = stdMat.GetSpecular(0, false);
                    #endregion

                    #region METALLIC

                    #endregion

                    #region OPACITY
                    PBRMtl.Opacity =stdMat.GetOpacity(0);

                    texmap = stdMat.GetSubTexmap(6);
                    if(texmap != null && stdMat.MapEnabled(6))
                    {
                        PBRMtl.OpacityTexmap = MaterialUtils.GetTexmap(texmap);
                    }
                    #endregion

                    #region ANISOTROPIC
                    if(shaderType == 0)
                    {
                       
                    }
                    #endregion

                    #region EMISSION                    
                    maxClr = stdMat.GetSelfIllumColor(0);
                    if(stdMat.GetSelfIllumColorOn(0, false))
                    {
                        PBRMtl.EmissionStrength = 1.0f;
                        PBRMtl.EmissionColor = RedHaloTools.IColorToString(maxClr, true);
                    }
                    else
                    {
                        PBRMtl.EmissionStrength = stdMat.GetSelfIllum(0);
                        PBRMtl.EmissionColor = "1,1,1,1";
                    }
                    #endregion

                    #region TranslucentColor

                    #endregion

                    #region BUMP

                    #endregion

                    #region DISPLACEMENT

                    #endregion

                    break;

                case "VRayCarPaintMtl":
                case "VRayCarPaintMtl2":

                    break;
                default:
                    break;
            }

            return PBRMtl;
        }
    }
}
