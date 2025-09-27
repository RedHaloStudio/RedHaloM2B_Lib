using Autodesk.Max;
using RedHaloM2B.Materials;
using RedHaloM2B.RedHaloUtils;
using System;

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

        public static RedHaloPBRMtl ExportMaterial(IMtl material)
        {
            string sourceName = material.Name;
            string md5 = RedHaloTools.CalcMD5FromString(sourceName);
            //string newName = $"material_{materialIndex:D5}";
            string newName = $"M_{md5}";

            // set new name
            material.Name = newName;

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
                Type = material.ClassName(false),
            };
            var materialType = material.ClassName(false);
            switch (material.ClassName(false))
            {
                case "VRayMtl":
                    // Use Roughness
                    PBRMtl.UseRoughness = Convert.ToBoolean(RedHaloTools.GetValueByID<int>(material, 1, 10));

                    maxClr = RedHaloTools.GetValueByID<IColor>(material, 0, 3);
                    reflectClr = RedHaloTools.GetValueByID<IColor>(material, 0, 9);
                    refractClr = RedHaloTools.GetValueByID<IColor>(material, 0, 27);
                    sss_on = RedHaloTools.GetValueByID<int>(material, 0, 44);

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
                    refractFogClr = RedHaloTools.GetValueByID<IColor>(material, 0, 31);

                    if (RedHaloCore.Global.RGBtoHSV(refractFogClr)[1] > 0.01 && sss_on != 6)
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
                    lockIOR = RedHaloTools.GetValueByID<int>(material, 0, 20);

                    if (lockIOR == 1)
                    {
                        ior = RedHaloTools.GetValueByID<float>(material, 0, 30);
                    }
                    else
                    {
                        ior = RedHaloTools.GetValueByID<float>(material, 0, 17);
                    }

                    // 如果IOR值小于1，设置为1
                    if (ior < 1)
                    {
                        ior = 1;
                    }

                    // 是否使用Fresnel反射
                    useIOR = RedHaloTools.GetValueByID<int>(material, 0, 13);
                    if (useIOR == 0)
                    {
                        ior = 2;
                    }

                    #region Diffuse
                    PBRMtl.DiffuseGroup = new DiffuseGroup
                    {
                        DiffuseColor = new[] { maxClr.R, maxClr.G, maxClr.B, 1 },
                        DiffuseRoughness = RedHaloTools.GetValueByID<float>(material, 0, 14)
                    };

                    // Diffuse Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 67);
                    if (texmap != null)
                    {
                        PBRMtl.DiffuseGroup.DiffuseTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    // Diffuse Roughness
                    PBRMtl.DiffuseGroup.DiffuseRoughness = RedHaloTools.GetValueByID<float>(material, 0, 4);

                    // Diffuse Roughness Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 68);
                    if (texmap != null)
                    {
                        PBRMtl.DiffuseGroup.DiffuseRoughnessTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    #region Roughness / Specular / Reflection

                    // 反射颜色的亮度值作为 Specular Level的值
                    PBRMtl.ReflectionGroup.SpecularLevel = RedHaloCore.Global.RGBtoHSV(reflectClr)[2];

                    // 反射的高光值作为 Roughness
                    PBRMtl.ReflectionGroup.Roughness = RedHaloTools.GetValueByID<float>(material, 0, 10);

                    // texmap_reflection 3-3
                    // texmap_reflection_on 3-4
                    // texmap_reflectionGlossiness 3-12
                    // texmap_reflectionGlossiness_on 3-13
                    // texmap_roughness 3-41
                    // texmap_roughness_on 3-42


                    // Roughness Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 70);
                    if (texmap != null)
                    {
                        PBRMtl.ReflectionGroup.SpecularLevelTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    // Specular Level Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 71);
                    if (texmap != null)
                    {
                        PBRMtl.ReflectionGroup.RoughnessTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    #endregion
                    
                    #region Metallic

                    /*
                     * 如果IOR值高于6，设置Metallic为真,在Blender里面测试过，当高于5.5的时候，和金属材质的效果差不多
                     */
                    metallic = RedHaloTools.GetValueByID<float>(material, 0, 18);
                    if (RedHaloTools.GetValueByID<float>(material, 0, 17) > 6)
                    {
                        metallic = 1.0f;
                        ior = 1.6f;
                    }
                    PBRMtl.MetallicGroup.Metallic = metallic;

                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 73);
                    if (texmap != null)
                    {
                        PBRMtl.MetallicGroup.MetallicTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion
                    
                    #region Opacity

                    PBRMtl.OpacityGroup.Opacity = RedHaloTools.GetValueByID<float>(material, 3, 40) / 100.0f;
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 3, 38);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 3, 39) == 1)
                    {
                        PBRMtl.OpacityGroup.OpacityTexmap = MaterialUtils.ExportTexmap(texmap);
                        texmap = null;
                    }

                    #endregion
                    
                    #region Subsurface

                    if (sss_on == 6)
                    {
                        maxClr = RedHaloTools.GetValueByID<IColor>(material, 0, 50);
                        PBRMtl.SubsurfaceGroup.SubsurfaceColor = [maxClr.R, maxClr.G, maxClr.B, 1];
                        texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 79);
                        if (texmap != null)
                        {
                            PBRMtl.SubsurfaceGroup.SubsurfaceColorTexmap = MaterialUtils.ExportTexmap(texmap);
                        }

                        PBRMtl.SubsurfaceGroup.SubsurfaceWeight = RedHaloTools.GetValueByID<float>(material, 0, 45);
                        texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 80);
                        if (texmap != null)
                        {
                            PBRMtl.SubsurfaceGroup.SubsurfaceWeightTexmap = MaterialUtils.ExportTexmap(texmap);
                        }

                        PBRMtl.SubsurfaceGroup.SubsurfaceRadius = [refractFogClr.R, refractFogClr.G, refractFogClr.B];
                        texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 77);
                        if (texmap != null)
                        {
                            PBRMtl.SubsurfaceGroup.SubsurfaceRadiusTexmap = MaterialUtils.ExportTexmap(texmap);
                        }


                        PBRMtl.SubsurfaceGroup.SubsurfaceScale = RedHaloTools.GetValueByID<float>(material, 0, 33);
                        texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 78);
                        if (texmap != null && RedHaloTools.GetValueByID<int>(material, 3, 25) == 1)
                        {
                            PBRMtl.SubsurfaceGroup.SubsurfaceScaleTexmap = MaterialUtils.ExportTexmap(texmap);
                        }
                    }
                    #endregion
                    
                    #region Anisotropic
                    PBRMtl.AnisotropicGroup.Anisotropic = RedHaloTools.GetValueByID<float>(material, 1, 1);
                    PBRMtl.AnisotropicGroup.AnisotropicRotation = (float)(RedHaloTools.GetValueByID<float>(material, 1, 2) % 360 / 360);

                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 1, 12);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 3, 45) == 1)
                    {
                        PBRMtl.AnisotropicGroup.AnisotropicTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 1, 13);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 3, 48) == 1)
                    {
                        PBRMtl.AnisotropicGroup.AnisotropicRotationTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    #region Transmission                    

                    PBRMtl.TransmissionGroup.Refraction = RedHaloCore.Global.RGBtoHSV(refractClr)[2];

                    // Transmission Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 74);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 3, 7) == 1)
                    {
                        PBRMtl.TransmissionGroup.RefractionTexmap = MaterialUtils.ExportTexmap(texmap);
                        texmap = null;
                    }

                    PBRMtl.TransmissionGroup.RefractionRoughness = RedHaloTools.GetValueByID<float>(material, 0, 28);
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 75);
                    if (texmap != null)
                    {
                        PBRMtl.TransmissionGroup.RefractionRoughnessTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    #endregion

                    #region Coat
                    PBRMtl.CoatGroup.Coat = RedHaloTools.GetValueByID<float>(material, 0, 55);
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 3, 74);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 3, 75) == 1)
                    {
                        PBRMtl.CoatGroup.CoatTexmap = MaterialUtils.ExportTexmap(texmap);
                        texmap = null;
                    }

                    PBRMtl.CoatGroup.CoatRoughness = RedHaloTools.GetValueByID<float>(material, 0, 56);
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 3, 77);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 3, 78) == 1)
                    {
                        PBRMtl.CoatGroup.CoatRoughnessTexmap = MaterialUtils.ExportTexmap(texmap);
                        texmap = null;
                    }

                    PBRMtl.CoatGroup.CoatIOR = RedHaloTools.GetValueByID<float>(material, 0, 57);
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 3, 80);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 3, 81) == 1)
                    {
                        PBRMtl.CoatGroup.CoatIORTexmap = MaterialUtils.ExportTexmap(texmap);
                        texmap = null;
                    }

                    maxClr = RedHaloTools.GetValueByID<IColor>(material, 0, 54);
                    PBRMtl.CoatGroup.CoatTint = [maxClr.R, maxClr.G, maxClr.B, 1];
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 3, 71);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 3, 72) == 1)
                    {
                        PBRMtl.CoatGroup.CoatTintTexmap = MaterialUtils.ExportTexmap(texmap);
                        texmap = null;
                    }

                    PBRMtl.CoatGroup.CoatBump = RedHaloTools.GetValueByID<float>(material, 0, 60);
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 3, 83);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 3, 84) == 1)
                    {
                        PBRMtl.CoatGroup.CoatBumpTexmap = MaterialUtils.ExportTexmap(texmap);
                        texmap = null;
                    }

                    #endregion
                    
                    #region Sheen
                    maxClr = RedHaloTools.GetValueByID<IColor>(material, 0, 52);

                    PBRMtl.SheenGroup.SheenWeight = RedHaloCore.Global.RGBtoHSV(maxClr)[2];

                    PBRMtl.SheenGroup.SheenColor = [maxClr.R, maxClr.G, maxClr.B, 1];
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 3, 65);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 3, 66) == 1)
                    {
                        PBRMtl.SheenGroup.SheenColorTexmap = MaterialUtils.ExportTexmap(texmap);
                        texmap = null;
                    }

                    PBRMtl.SheenGroup.SheenRoughness = RedHaloTools.GetValueByID<float>(material, 0, 53);
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 88);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 3, 69) == 1)
                    {
                        PBRMtl.SheenGroup.SheenRoughnessTexmap = MaterialUtils.ExportTexmap(texmap);
                        texmap = null;
                    }
                    #endregion
                    
                    #region Emission

                    maxClr = RedHaloTools.GetValueByID<IColor>(material, 0, 5);
                    PBRMtl.EmissionGroup.EmissionColor = [maxClr.R, maxClr.G, maxClr.B, 1];
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 3, 56);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 3, 57) == 1)
                    {
                        PBRMtl.EmissionGroup.EmissionColorTexmap = MaterialUtils.ExportTexmap(texmap);
                        texmap = null;
                    }

                    PBRMtl.EmissionGroup.EmissionStrength = RedHaloTools.GetValueByID<float>(material, 0, 7);
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 3, 58);
                    if (texmap != null)
                    {
                        PBRMtl.EmissionGroup.EmissionColorTexmap = MaterialUtils.ExportTexmap(texmap);
                        texmap = null;
                    }

                    #endregion

                    #region BUMP
                    PBRMtl.BumpGroup.Bump = RedHaloTools.GetValueByID<float>(material, 0, 102);

                    // Bump Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 100);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 101) == 1)
                    {
                        PBRMtl.BumpGroup.BumpTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    #region DISPLACEMENT
                    PBRMtl.DisplacementGroup.DisplacementMax = RedHaloTools.GetValueByID<float>(material, 0, 84);
                    PBRMtl.DisplacementGroup.DisplacementMin = RedHaloTools.GetValueByID<float>(material, 0, 83);

                    enabled = Convert.ToBoolean(RedHaloTools.GetValueByID<int>(material, 0, 85));
                    if (enabled)
                    {
                        PBRMtl.DisplacementGroup.DisplacementWaterLevel = RedHaloTools.GetValueByID<float>(material, 0, 86);
                    }
                    else
                    {
                        PBRMtl.DisplacementGroup.DisplacementWaterLevel = 0.5f;
                    }

                    // Displacement Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 87);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 88) == 1)
                    {
                        PBRMtl.DisplacementGroup.DisplacementTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    #endregion

                    #region ThinFilm

                    if (RedHaloTools.GetValueByID<int>(material, 0, 61) == 1)
                    {
                        PBRMtl.ThinFilmGroup.ThinFilm = RedHaloTools.GetValueByID<float>(material, 0, 62);
                        texmap = RedHaloTools.GetValueByID<ITexmap>(material, 3, 86);
                        if (texmap != null && RedHaloTools.GetValueByID<int>(material, 3, 87) == 1)
                        {
                            PBRMtl.ThinFilmGroup.ThinFilmTexmap = MaterialUtils.ExportTexmap(texmap);
                            texmap = null;
                        }
                    }
                    else
                    {
                        PBRMtl.ThinFilmGroup.ThinFilm = 0;
                    }

                    PBRMtl.ThinFilmGroup.ThinFilmIOR = RedHaloTools.GetValueByID<float>(material, 0, 64);
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 3, 89);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 3, 90) == 1)
                    {
                        PBRMtl.ThinFilmGroup.ThinFilmIORTexmap = MaterialUtils.ExportTexmap(texmap);
                        texmap = null;
                    }

                    #endregion
                    
                    #region TranslucentColor

                    #endregion

                    break;

                case "CoronaLegacyMtl":
                    var CR_Legacy_Diffuse_Color = RedHaloTools.GetValueByID<IColor>(material, 0, 0);
                    var CR_Legacy_Diffuse_Level = RedHaloTools.GetValueByID<float>(material, 0, 6);
                    var CR_Legacy_Diffuse_Color_Final = CR_Legacy_Diffuse_Color.MultiplyBy(CR_Legacy_Diffuse_Level);

                    var CR_Legacy_Reflect_IOR = RedHaloTools.GetValueByID<float>(material, 0, 11);
                    var CR_Legacy_Refract_IOR = RedHaloTools.GetValueByID<float>(material, 0, 62);

                    var CR_Legacy_Reflect_Level = RedHaloTools.GetValueByID<float>(material, 0, 7);
                    var CR_Legacy_Reflect_Color = RedHaloTools.GetValueByID<IColor>(material, 0, 9);
                    var CR_Legacy_Reflect_Final = CR_Legacy_Reflect_Color.MultiplyBy(CR_Legacy_Reflect_Level);

                    var CR_Legacy_Refract_Level = RedHaloTools.GetValueByID<float>(material, 0, 8);
                    var CR_Legacy_Refract_Color = RedHaloTools.GetValueByID<IColor>(material, 0, 27);
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
                    PBRMtl.DiffuseGroup.DiffuseColor = new[] { CR_Legacy_Diffuse_Color_Final.R, CR_Legacy_Diffuse_Color_Final.G, CR_Legacy_Diffuse_Color_Final.B, 1 };

                    // Diffuse Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 12);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 30) == 1)
                    {
                        PBRMtl.DiffuseGroup.DiffuseTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    #region Specular / Reflection / Roughness
                    // 反射颜色 * 反射系数
                    // 反射颜色的亮度值作为 Specular Level 值
                    maxClr = RedHaloTools.GetValueByID<IColor>(material, 0, 9).MultiplyBy(RedHaloTools.GetValueByID<float>(material, 0, 7));
                    PBRMtl.ReflectionGroup.SpecularLevel = RedHaloCore.Global.RGBtoHSV(maxClr)[2];

                    // 反射的高光值作为Roughness
                    PBRMtl.ReflectionGroup.Roughness = RedHaloTools.GetValueByID<float>(material, 0, 10);

                    // Specular Level Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 13);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 31) == 1)
                    {
                        PBRMtl.ReflectionGroup.SpecularLevelTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    // Roughness Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 14);
                    if(texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 33) == 1)
                    {
                        PBRMtl.ReflectionGroup.RoughnessTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    #endregion

                    #region Metallic
                    PBRMtl.MetallicGroup.Metallic = CR_Legacy_MetallicValue;
                    #endregion

                    #region IOR
                    // 默认获取折射IOR
                    PBRMtl.IORGroup.IOR = CR_Legacy_IOR; // CR_Legacy_Refract_IOR;

                    // IOR Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 23);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 40) == 1)
                    {
                        PBRMtl.IORGroup.IORTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    #region Opacity
                    maxClr = RedHaloTools.GetValueByID<IColor>(material, 0, 3).MultiplyBy(RedHaloTools.GetValueByID<float>(material, 0, 9));
                    PBRMtl.OpacityGroup.Opacity = RedHaloCore.Global.RGBtoHSV(maxClr)[2];

                    // Opacity Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 17);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 35) == 1)
                    {
                        PBRMtl.OpacityGroup.OpacityTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    #region Subsurface
                    sss_on = RedHaloTools.GetValueByID<int>(material, 0, 125);
                    if (RedHaloTools.GetValueByID<int>(material, 0, 125) == 1)
                    {
                        PBRMtl.SubsurfaceGroup.SubsurfaceWeight = RedHaloTools.GetValueByID<float>(material, 0, 109);

                        maxClr = RedHaloTools.GetValueByID<IColor>(material, 0, 50);
                        PBRMtl.SubsurfaceGroup.SubsurfaceRadius = [maxClr.R, maxClr.G, maxClr.B];

                        PBRMtl.SubsurfaceGroup.SubsurfaceScale = RedHaloTools.GetValueByID<float>(material, 0, 33);
                    }
                    #endregion

                    #region Anisotropic
                    PBRMtl.AnisotropicGroup.Anisotropic = RedHaloTools.GetValueByID<float>(material, 0, 67);
                    PBRMtl.AnisotropicGroup.AnisotropicRotation = (float)(RedHaloTools.GetValueByID<float>(material, 0, 68) / 360.0);

                    // Anisotropic Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 21);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 38) == 1)
                    {
                        PBRMtl.AnisotropicGroup.AnisotropicTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    // anisiotropic rotation texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 22);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 39) == 1)
                    {
                        PBRMtl.AnisotropicGroup.AnisotropicRotationTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    #region Transmission / Refraction
                    maxClr = RedHaloTools.GetValueByID<IColor>(material, 0, 2).MultiplyBy(RedHaloTools.GetValueByID<float>(material, 0, 8));
                    PBRMtl.TransmissionGroup.Refraction = RedHaloCore.Global.RGBtoHSV(maxClr)[2];
                    #endregion

                    #region Coat NOT SUPPORTED
                    #endregion 

                    #region Sheen NOT SUPPORTED
                    #endregion

                    #region EMISSION
                    maxClr = RedHaloTools.GetValueByID<IColor>(material, 0, 5);
                    PBRMtl.EmissionGroup.EmissionColor = new[] { maxClr.R, maxClr.G, maxClr.B, 1 };

                    PBRMtl.EmissionGroup.EmissionStrength = RedHaloTools.GetValueByID<float>(material, 0, 11);

                    // Emission Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 29);
                    if (texmap != null &&
                        RedHaloTools.GetValueByID<int>(material, 0, 45) == 1)
                    {
                        PBRMtl.EmissionGroup.EmissionColorTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    #region BUMP
                    PBRMtl.BumpGroup.Bump = RedHaloTools.GetValueByID<float>(material, 0, 52);

                    // Bump Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 18);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 36) == 1)
                    {
                        PBRMtl.BumpGroup.BumpTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    #region DISPLACEMENT
                    PBRMtl.DisplacementGroup.DisplacementMax = RedHaloTools.GetValueByID<float>(material, 0, 76);
                    PBRMtl.DisplacementGroup.DisplacementMin = RedHaloTools.GetValueByID<float>(material, 0, 74);

                    enabled = Convert.ToBoolean(RedHaloTools.GetValueByID<int>(material, 0, 98));
                    if (enabled)
                    {
                        PBRMtl.DisplacementGroup.DisplacementWaterLevel = RedHaloTools.GetValueByID<float>(material, 0, 75);
                    }
                    else
                    {
                        PBRMtl.DisplacementGroup.DisplacementWaterLevel = 0.5f;
                    }

                    // Displacement Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 25);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 42) == 1)
                    {
                        PBRMtl.DisplacementGroup.DisplacementTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    #endregion

                    #region ThinFilm NOT SUPPORTED
                    #endregion

                    #region TranslucentColor

                    #endregion

                    break;

                case "\rCoronaPhysicalMtl":
                case "CoronaPhysicalMtl":
                    var useRoughness = !Convert.ToBoolean(RedHaloTools.GetValueByID<int>(material, 0, 124));
                    PBRMtl.UseRoughness = useRoughness;

                    var useMetallic = false;
                    useMetallic = RedHaloTools.GetValueByID<int>(material, 0, 5) == 1;

                    var useThinMode = false;
                    useThinMode = RedHaloTools.GetValueByID<int>(material, 0, 34) == 1;

                    #region DIFFUSE
                    var CP_Diffuse_Color = RedHaloTools.GetValueByID<IColor>(material, 0, 0);
                    var CP_Diffuse_Level = RedHaloTools.GetValueByID<float>(material, 0, 1);
                    var CP_Diffuse_Color_Final = CP_Diffuse_Color.MultiplyBy(CP_Diffuse_Level);

                    PBRMtl.DiffuseGroup.DiffuseColor = new[] { CP_Diffuse_Color_Final.R, CP_Diffuse_Color_Final.G, CP_Diffuse_Color_Final.B, 1 };
                    // Diffuse Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 2);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 3) == 1)
                    {
                        PBRMtl.DiffuseGroup.DiffuseTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    #endregion

                    #region REFLECTION / SPECULAR / GLOSSINESS / ROUGHNESS
                    var CP_Reflect_Level = RedHaloTools.GetValueByID<float>(material, 0, 12);
                    PBRMtl.ReflectionGroup.Roughness = CP_Reflect_Level;

                    // Reflection Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 13);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 14) == 1)
                    {
                        PBRMtl.ReflectionGroup.RoughnessTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    #region METALLIC
                    PBRMtl.MetallicGroup.Metallic = useMetallic ? 1.0f : 0.0f;
                    #endregion

                    #region IOR
                    PBRMtl.IORGroup.IOR = RedHaloTools.GetValueByID<float>(material, 0, 24);

                    // IOR Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 25);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 26) == 1)
                    {
                        PBRMtl.IORGroup.IORTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    #region OPACITY
                    maxClr = RedHaloTools.GetValueByID<IColor>(material, 0, 6).MultiplyBy(RedHaloTools.GetValueByID<float>(material, 0, 7));
                    PBRMtl.OpacityGroup.Opacity = RedHaloCore.Global.RGBtoHSV(maxClr)[2];

                    // Opacity Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 8);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 9) == 1)
                    {
                        PBRMtl.OpacityGroup.OpacityTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    #region SUBSURFACE
                    /* 
                     * Max和Blender中名称的对应名称（MAX --  BLENDER）
                     * sssAmount -- Subsurface Weight
                     * sssRadius -- Subsurface Scale
                     * sssScatterColor -- Subsurface Radius
                    */
                    PBRMtl.SubsurfaceGroup.SubsurfaceWeight = RedHaloTools.GetValueByID<float>(material, 0, 71);

                    // Subsurface Weight Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 72);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 73) == 1)
                    {
                        PBRMtl.SubsurfaceGroup.SubsurfaceWeightTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    maxClr = RedHaloTools.GetValueByID<IColor>(material, 0, 79);
                    // PBRMtl.SubsurfaceColor = RedHaloTools.IColorToString(maxClr);
                    PBRMtl.SubsurfaceGroup.SubsurfaceRadius = [maxClr.R, maxClr.G, maxClr.B];

                    // Subsurface Color Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 80);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 81) == 1)
                    {
                        PBRMtl.SubsurfaceGroup.SubsurfaceScaleTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    PBRMtl.SubsurfaceGroup.SubsurfaceScale = RedHaloTools.GetValueByID<float>(material, 0, 75);

                    // Subsurface Scale Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 76);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 77) == 1)
                    {
                        PBRMtl.SubsurfaceGroup.SubsurfaceRadiusTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    #endregion

                    #region ANISOTROPIC
                    PBRMtl.AnisotropicGroup.Anisotropic = RedHaloTools.GetValueByID<float>(material, 0, 16);
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 17);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 18) == 1)
                    {
                        PBRMtl.AnisotropicGroup.AnisotropicTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    PBRMtl.AnisotropicGroup.AnisotropicRotation = RedHaloTools.GetValueByID<float>(material, 0, 20);
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 21);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 22) == 1)
                    {
                        PBRMtl.AnisotropicGroup.AnisotropicRotationTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    #endregion

                    #region REFRACTION / TRANSMISSION
                    if (!useMetallic)
                    {
                        PBRMtl.TransmissionGroup.Refraction = RedHaloTools.GetValueByID<float>(material, 0, 28);

                        // Refraction Texmap
                        texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 29);
                        if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 30) == 1)
                        {
                            PBRMtl.TransmissionGroup.RefractionTexmap = MaterialUtils.ExportTexmap(texmap);
                        }
                    }
                    #endregion

                    #region COAT
                    PBRMtl.CoatGroup.Coat = RedHaloTools.GetValueByID<float>(material, 0, 36);
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 37);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 38) == 1)
                    {
                        PBRMtl.CoatGroup.CoatTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    PBRMtl.CoatGroup.CoatRoughness = RedHaloTools.GetValueByID<float>(material, 0, 44);
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 45);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 46) == 1)
                    {
                        PBRMtl.CoatGroup.CoatRoughnessTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    PBRMtl.CoatGroup.CoatIOR = RedHaloTools.GetValueByID<float>(material, 0, 40);
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 41);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 42) == 1)
                    {
                        PBRMtl.CoatGroup.CoatIORTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    maxClr = RedHaloTools.GetValueByID<IColor>(material, 0, 115);
                    PBRMtl.CoatGroup.CoatTint = [maxClr.R, maxClr.G, maxClr.B, 1];
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 116);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 117) == 1)
                    {
                        PBRMtl.CoatGroup.CoatTintTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    PBRMtl.CoatGroup.CoatBump = RedHaloTools.GetValueByID<float>(material, 0, 121);
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 119);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 120) == 1)
                    {
                        PBRMtl.CoatGroup.CoatBumpTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    #endregion

                    #region SHEEN
                    PBRMtl.SheenGroup.SheenWeight = RedHaloTools.GetValueByID<float>(material, 0, 48);

                    maxClr = RedHaloTools.GetValueByID<IColor>(material, 0, 52);
                    PBRMtl.SheenGroup.SheenColor = [maxClr.R, maxClr.G, maxClr.B, 1];

                    // Sheen Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 53);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 54) == 1)
                    {
                        PBRMtl.SheenGroup.SheenColorTexmap = MaterialUtils.ExportTexmap(texmap);
                    }


                    PBRMtl.SheenGroup.SheenRoughness = RedHaloTools.GetValueByID<float>(material, 0, 56);
                    // Sheen Roughness Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 57);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 58) == 1)
                    {
                        PBRMtl.SheenGroup.SheenRoughnessTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    #endregion

                    #region EMISSION
                    maxClr = RedHaloTools.GetValueByID<IColor>(material, 0, 89);
                    PBRMtl.EmissionGroup.EmissionColor = new[] { maxClr.R, maxClr.G, maxClr.B, 1 };
                    PBRMtl.EmissionGroup.EmissionStrength = RedHaloTools.GetValueByID<float>(material, 0, 90);

                    // Emission Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 91);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 92) == 1)
                    {
                        PBRMtl.EmissionGroup.EmissionColorTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    #region THIN FILM NOT SUPPORTED

                    #endregion

                    #region TranslucentColor
                    if (useThinMode)
                    {
                        //PBRMtl.TranslucentColor = RedHaloTools.IColorToString(RedHaloTools.GetValueByID<IColor>(material, 0, 97), true);
                    }
                    #endregion

                    #region BUMP
                    PBRMtl.BumpGroup.Bump = RedHaloTools.GetValueByID<float>(material, 0, 102);

                    // Bump Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 100);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 101) == 1)
                    {
                        PBRMtl.BumpGroup.BumpTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    #region DISPLACEMENT
                    PBRMtl.DisplacementGroup.DisplacementMax = RedHaloTools.GetValueByID<float>(material, 0, 84);
                    PBRMtl.DisplacementGroup.DisplacementMin = RedHaloTools.GetValueByID<float>(material, 0, 83);

                    enabled = Convert.ToBoolean(RedHaloTools.GetValueByID<int>(material, 0, 85));

                    if (enabled)
                    {
                        PBRMtl.DisplacementGroup.DisplacementWaterLevel = RedHaloTools.GetValueByID<float>(material, 0, 86);
                    }
                    else
                    {
                        PBRMtl.DisplacementGroup.DisplacementWaterLevel = 0.5f;
                    }

                    // Displacement Texmap
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 87);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 88) == 1)
                    {
                        PBRMtl.DisplacementGroup.DisplacementTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    break;

                case "Standard (Legacy)":
                case "StandardMaterial":
                    IStdMat2 stdMat = material as IStdMat2;

                    var shaderType = RedHaloTools.GetValueByID<int>(material, 0, 0);
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
                    maxClr = stdMat.GetDiffuse(0, false);
                    PBRMtl.DiffuseGroup.DiffuseColor = [maxClr.R, maxClr.G, maxClr.B, 1];

                    // Diffuse Texmap
                    texmap = stdMat.GetSubTexmap(1);
                    if (texmap != null && stdMat.MapEnabled(1))
                    {
                        PBRMtl.DiffuseGroup.DiffuseTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    #region REFLECTION / SPECULAR / GLOSSINESS / ROUGHNESS               
                    //var sp_clr = stdMat.GetSpecular(0, false);                    
                    PBRMtl.ReflectionGroup.SpecularLevel = stdMat.GetShinStr(0); // Specular level
                    PBRMtl.ReflectionGroup.Roughness = stdMat.GetShininess(0); // Glossiness

                    // Specular textures
                    texmap = stdMat.GetSubTexmap(3);
                    if (texmap != null && stdMat.MapEnabled(3))
                    {
                        PBRMtl.ReflectionGroup.SpecularLevelTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    // Specular Roughness
                    texmap = stdMat.GetSubTexmap(4);
                    if (texmap != null && stdMat.MapEnabled(4))
                    {
                        PBRMtl.ReflectionGroup.RoughnessTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    #endregion

                    #region METALLIC

                    #endregion

                    #region OPACITY
                    PBRMtl.OpacityGroup.Opacity = stdMat.GetOpacity(0);

                    texmap = stdMat.GetSubTexmap(6);
                    if (texmap != null && stdMat.MapEnabled(6))
                    {
                        PBRMtl.OpacityGroup.OpacityTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    #region ANISOTROPIC
                    if (shaderType == 0)
                    {

                    }
                    #endregion

                    #region EMISSION                    
                    maxClr = stdMat.GetSelfIllumColor(0);
                    if (stdMat.GetSelfIllumColorOn(0, false))
                    {
                        PBRMtl.EmissionGroup.EmissionStrength = 1.0f;
                        PBRMtl.EmissionGroup.EmissionColor = [maxClr.R, maxClr.G, maxClr.B, 1];
                    }
                    else
                    {
                        PBRMtl.EmissionGroup.EmissionStrength = stdMat.GetSelfIllum(0);
                        PBRMtl.EmissionGroup.EmissionColor = [1, 1, 1, 1];
                    }
                    #endregion

                    #region TranslucentColor

                    #endregion

                    #region BUMP
                    PBRMtl.BumpGroup.Bump = stdMat.GetTexmapAmt(8, 0) / 10;

                    texmap = stdMat.GetSubTexmap(8);
                    if (texmap != null && stdMat.MapEnabled(8))
                    {
                        PBRMtl.BumpGroup.BumpTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    #region DISPLACEMENT
                    PBRMtl.DisplacementGroup.DisplacementMax = stdMat.GetTexmapAmt(11, 0);

                    texmap = stdMat.GetSubTexmap(11);
                    if (texmap != null && stdMat.MapEnabled(11))
                    {
                        PBRMtl.DisplacementGroup.DisplacementTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    break;

                case "VRayCarPaintMtl":
                case "VRayCarPaintMtl2":

                    #region DIFFUSE
                    maxClr = RedHaloTools.GetValueByID<IColor>(material, 0, 0);
                    PBRMtl.DiffuseGroup.DiffuseColor = [maxClr.R, maxClr.G, maxClr.B, 1];

                    // Diffuse texture
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 41);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 42) == 1)
                    {
                        PBRMtl.DiffuseGroup.DiffuseTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    #region SPECULAR / REFLECTION
                    PBRMtl.ReflectionGroup.Roughness = RedHaloTools.GetValueByID<float>(material, 0, 2);

                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 44);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 45) == 1)
                    {
                        PBRMtl.ReflectionGroup.RoughnessTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    PBRMtl.ReflectionGroup.Roughness = RedHaloTools.GetValueByID<float>(material, 0, 4);
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 47);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 48) == 1)
                    {
                        PBRMtl.ReflectionGroup.SpecularLevelTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    #region COAT

                    PBRMtl.CoatGroup.Coat = RedHaloTools.GetValueByID<float>(material, 0, 28);
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 68);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 69) == 1)
                    {
                        PBRMtl.CoatGroup.CoatTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    maxClr = RedHaloTools.GetValueByID<IColor>(material, 0, 26);
                    PBRMtl.CoatGroup.CoatTint = [maxClr.R, maxClr.G, maxClr.B, 1];
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 65);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 66) == 1)
                    {
                        PBRMtl.CoatGroup.CoatTintTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    PBRMtl.CoatGroup.CoatRoughness = RedHaloTools.GetValueByID<float>(material, 0, 32);
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 74);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 75) == 1)
                    {
                        PBRMtl.CoatGroup.CoatRoughnessTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    PBRMtl.CoatGroup.CoatIOR = RedHaloTools.GetValueByID<float>(material, 0, 30);
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 71);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 72) == 1)
                    {
                        PBRMtl.CoatGroup.CoatIORTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    PBRMtl.CoatGroup.CoatBump = RedHaloTools.GetValueByID<float>(material, 0, 79);
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 77);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 78) == 1)
                    {
                        PBRMtl.CoatGroup.CoatBumpTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    #endregion

                    #region BUMP
                    PBRMtl.BumpGroup.Bump = RedHaloTools.GetValueByID<float>(material, 0, 55);
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 53);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 54) == 1)
                    {
                        PBRMtl.BumpGroup.BumpTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    #endregion

                    break;

                default:
                    break;
            }

            return PBRMtl;
        }
    }
}