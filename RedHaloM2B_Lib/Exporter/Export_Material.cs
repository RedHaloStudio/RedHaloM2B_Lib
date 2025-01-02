using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedHaloM2B.Materials;

namespace RedHaloM2B
{
    partial class Exporter
    {
        public static RedHaloPBRMtl ExportMaterial(IMtl material, int materialIndex)
        {
            var sourceName = material.Name;
            var mid = Guid.NewGuid().ToString();

            RedHaloPBRMtl PBRMtl = new RedHaloPBRMtl();
            PBRMtl.Name = sourceName;
            PBRMtl.ID = mid;
            PBRMtl.MaterialType = material.ClassName(false);

            IColor maxClr = RedHaloCore.Global.Color.Create(0.9, 0.9, 0.9);
            IColor refClr = RedHaloCore.Global.Color.Create(0.5, 0.5, 0.5);

            Debug.Print(sourceName);
            //Debug.WriteLine(material.ClassName(false));
            switch (material.ClassName(false))
            {
                case "VRayMtl":
                    try
                    {
                        #region Diffuse
                        maxClr = RedHaloTools.GetValeByID<IColor>(material, (short)0, 3);
                        PBRMtl.DiffuseColor = $"{maxClr.R},{maxClr.G},{maxClr.B},1";

                        PBRMtl.DiffuseRoughness = RedHaloTools.GetValeByID<float>(material, (short)0, 14);
                        #endregion

                        #region Specular
                        maxClr = RedHaloTools.GetValeByID<IColor>(material, (short)0, 9);
                        PBRMtl.Specular = RedHaloCore.Global.RGBtoHSV(maxClr)[2];

                        PBRMtl.SpecularRoughness = RedHaloTools.GetValeByID<float>(material, (short)0, 10);
                        #endregion

                        #region Metallic
                        PBRMtl.Metallic = RedHaloTools.GetValeByID<float>(material, (short)0, 18);
                        #endregion

                        #region IOR
                        // 默认获取折射IOR
                        PBRMtl.IOR = RedHaloTools.GetValeByID<float>(material, (short)0, 30);
                        #endregion

                        #region Opacity

                        #endregion

                        #region Subsurface
                        var sss_on = RedHaloTools.GetValeByID<int>(material, (short)0, 44);
                        if (sss_on == 6)
                        {
                            PBRMtl.SubsurfaceWeight = RedHaloTools.GetValeByID<float>(material, (short)0, 45);

                            maxClr = RedHaloTools.GetValeByID<IColor>(material, (short)0, 50);
                            PBRMtl.SubsurfaceRadius = $"{maxClr.R},{maxClr.G},{maxClr.B}";

                            PBRMtl.SubsurfaceScale = RedHaloTools.GetValeByID<float>(material, (short)0, 33);
                        }
                        #endregion

                        #region Anisotropic
                        PBRMtl.Anisotropic = RedHaloTools.GetValeByID<float>(material, (short)1, 1);
                        PBRMtl.AnisotropicRotation = (float)(RedHaloTools.GetValeByID<float>(material, (short)1, 2) % 360 / 360);
                        #endregion

                        #region Transmission
                        maxClr = RedHaloTools.GetValeByID<IColor>(material, (short)0, 27);
                        PBRMtl.Transmission = RedHaloCore.Global.RGBtoHSV(maxClr)[2];
                        #endregion

                        #region Coat
                        PBRMtl.Coat = RedHaloTools.GetValeByID<float>(material, (short)0, 55);
                        PBRMtl.CoatRoughness = RedHaloTools.GetValeByID<float>(material, (short)0, 56);
                        PBRMtl.CoatIOR = RedHaloTools.GetValeByID<float>(material, (short)0, 57);

                        maxClr = RedHaloTools.GetValeByID<IColor>(material, (short)0, 54);
                        PBRMtl.CoatTint = $"{maxClr.R},{maxClr.G},{maxClr.B},1";
                        #endregion

                        #region Sheen
                        maxClr = RedHaloTools.GetValeByID<IColor>(material, (short)0, 52);

                        PBRMtl.SheenRoughness = RedHaloTools.GetValeByID<float>(material, (short)0, 53);
                        #endregion

                        #region Emission
                        maxClr = RedHaloTools.GetValeByID<IColor>(material, (short)0, 5);
                        PBRMtl.Emission = $"{maxClr.R},{maxClr.G},{maxClr.B},1";

                        PBRMtl.EmissionStrength = RedHaloTools.GetValeByID<float>(material, (short)0, 7);
                        #endregion

                        #region ThinFilm
                        PBRMtl.ThinFilm = RedHaloTools.GetValeByID<int>(material, (short)0, 61) == 1 ? RedHaloTools.GetValeByID<float>(material, (short)0, 62) : 0f; // VRay thin Film min. Max is 63
                        PBRMtl.ThinFilmIOR = RedHaloTools.GetValeByID<float>(material, (short)0, 64);
                        #endregion

                        #region TranslucentColor

                        #endregion

                    }
                    catch (Exception e)
                    {
                        Debug.Print(e.ToString());
                    }
                    break;
                case "VRay2SidedMtl":

                    break;
                case "CoronaLegacyMtl":
                    #region Diffuse
                    maxClr = RedHaloTools.GetValeByID<IColor>(material, (short)0, 0).MultiplyBy(RedHaloTools.GetValeByID<float>(material, (short)0, 6));
                    PBRMtl.DiffuseColor = $"{maxClr.R},{maxClr.G},{maxClr.B},1";

                    PBRMtl.DiffuseRoughness = 0.0f;
                    #endregion

                    #region Specular
                    maxClr = RedHaloTools.GetValeByID<IColor>(material, (short)0, 9).MultiplyBy(RedHaloTools.GetValeByID<float>(material, (short)0, 7));
                    PBRMtl.Specular = RedHaloCore.Global.RGBtoHSV(maxClr)[2];

                    PBRMtl.SpecularRoughness = RedHaloTools.GetValeByID<float>(material, (short)0, 10);
                    #endregion

                    #region Metallic
                    PBRMtl.Metallic = 0;
                    #endregion

                    #region IOR
                    // 默认获取折射IOR
                    PBRMtl.IOR = RedHaloTools.GetValeByID<float>(material, (short)0, 62);
                    #endregion

                    #region Opacity

                    #endregion

                    #region Subsurface
                    //var sss_on = RedHaloTools.GetValeByID<int>(material, (short)0, 125);
                    if (RedHaloTools.GetValeByID<int>(material, (short)0, 125) == 1)
                    {
                        PBRMtl.SubsurfaceWeight = RedHaloTools.GetValeByID<float>(material, (short)0, 109);

                        //maxClr = RedHaloTools.GetValeByID<IColor>(material, (short)0, 50);
                        //PBRMtl.SubsurfaceRadius = $"{maxClr.R},{maxClr.G},{maxClr.B}";

                        //PBRMtl.SubsurfaceScale = RedHaloTools.GetValeByID<float>(material, (short)0, 33);
                    }
                    #endregion

                    #region Anisotropic
                    PBRMtl.Anisotropic = RedHaloTools.GetValeByID<float>(material, (short)0, 67);
                    PBRMtl.AnisotropicRotation = (float)(RedHaloTools.GetValeByID<float>(material, (short)0, 68) / 360.0);
                    #endregion

                    #region Transmission
                    maxClr = RedHaloTools.GetValeByID<IColor>(material, (short)0, 2).MultiplyBy(RedHaloTools.GetValeByID<float>(material, (short)0, 8));
                    PBRMtl.Transmission = RedHaloCore.Global.RGBtoHSV(maxClr)[2];
                    #endregion

                    #region Coat
                    PBRMtl.Coat = 0;
                    PBRMtl.CoatRoughness = 0.03f;
                    PBRMtl.CoatIOR = 1.5f;
                    PBRMtl.CoatTint = $"1,1,1,1";
                    #endregion

                    #region Sheen
                    PBRMtl.Sheen = 0;
                    PBRMtl.SheenRoughness = 0.5f;
                    #endregion

                    #region Emission
                    maxClr = RedHaloTools.GetValeByID<IColor>(material, (short)0, 5);
                    PBRMtl.Emission = $"{maxClr.R},{maxClr.G},{maxClr.B},1";

                    PBRMtl.EmissionStrength = RedHaloTools.GetValeByID<float>(material, (short)0, 11);
                    #endregion

                    #region ThinFilm
                    PBRMtl.ThinFilm = 0f; // VRay thin Film min. Max is 63
                    PBRMtl.ThinFilmIOR = 1.6f;
                    #endregion

                    #region TranslucentColor

                    #endregion
                    break;

                default:
                    break;
            }
            #region Diffuse

            #endregion

            #region Specular

            #endregion

            #region Metallic

            #endregion

            #region IOR

            #endregion

            #region Opacity

            #endregion

            #region Subsurface

            #endregion

            #region Anisotropic

            #endregion

            #region Transmission

            #endregion
            // Coat
            // Sheen
            // Emission
            // ThinFilm
            // TranslucentColor
            return PBRMtl;
        }
    }
}
