using Autodesk.Max;
using Autodesk.Max.IGameObject;
using RedHaloM2B.Materials;
using RedHaloM2B.RedHaloUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

namespace RedHaloM2B
{
    partial class Exporter
    {
        public static RedHaloLightMtl ExportLightMaterial(IMtl material, int materialIndex)
        {
            string sourceName = material.Name;
            string newName = $"material_light_{materialIndex:D4}";

            IColor maxClr = RedHaloCore.Global.Color.Create(0.8, 0.8, 0.8);
            ITexmap texmap = null;

            RedHaloLightMtl lightMtl = new()
            {
                Name = newName,
                SourceName = sourceName,
                Type = material.ClassName(false),
            };
            Debug.Print($"{material.ClassName(false)}");

            switch (material.ClassName(false))
            {
                case "VRayLightMtl":
                    #region COLOR
                    maxClr = RedHaloTools.GetValueByID<IColor>(material, 0, 0);

                    lightMtl.Color = [maxClr.R, maxClr.G, maxClr.B, 1]; //RedHaloTools.IColorToString(maxClr, true);
                    
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 2);
                    if(texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 3) == 1)
                    {
                        lightMtl.ColorTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    #region STRENGTH
                    // VRayLightMtl && CoronaLightMtl not texture of strength
                    lightMtl.Strength = RedHaloTools.GetValueByID<float>(material, 0, 2);
                    #endregion

                    #region OPACITY
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 7);
                    if(texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 8) == 1)
                    {
                        lightMtl.OpacityTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    #region OTHERS
                    lightMtl.UseTwoSided = RedHaloTools.GetValueByID<int>(material, 0, 4) == 1;
                    #endregion

                    break;

                case "CoronaLightMtl":
                    #region COLOR
                    maxClr = RedHaloTools.GetValueByID<IColor>(material, 0, 3);

                    lightMtl.Color = [maxClr.R, maxClr.G, maxClr.B, 1];

                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 2);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 1) == 1)
                    {
                        lightMtl.ColorTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    #region STRENGTH
                    // VRayLightMtl && CoronaLightMtl not texture of strength
                    lightMtl.Strength = RedHaloTools.GetValueByID<float>(material, 0, 0);
                    #endregion

                    #region OPACITY
                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 10);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 11) == 1)
                    {
                        lightMtl.OpacityTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    #region OTHERS
                    lightMtl.UseTwoSided = RedHaloTools.GetValueByID<int>(material, 0, 17) == 1;
                    lightMtl.VisibleReflect = RedHaloTools.GetValueByID<int> (material, 0, 7) == 1;
                    lightMtl.VisibleDirect = RedHaloTools.GetValueByID<int>(material, 0, 8) == 1;
                    lightMtl.VisibleRefract = RedHaloTools.GetValueByID<int>(material, 0, 9) == 1;
                    lightMtl.AffectAlpha = RedHaloTools.GetValueByID<int>(material, 0, 4) == 1;
                    #endregion

                    break;
                
                default:
                    break;
            }

            return lightMtl;
            }
        }
}
 