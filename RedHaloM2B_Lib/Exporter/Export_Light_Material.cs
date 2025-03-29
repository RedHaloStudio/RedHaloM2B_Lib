using Autodesk.Max;
using RedHaloM2B.Materials;
using RedHaloM2B.RedHaloUtils;
using System;
using System.Diagnostics;
using System.Windows.Media;

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
                    maxClr = RedHaloTools.GetValeByID<IColor>(material, 0, 0);

                    lightMtl.Color = RedHaloTools.IColorToString(maxClr, true);
                    lightMtl.ColorArray = new[] { maxClr.R, maxClr.G, maxClr.B }; 
                    
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 2);
                    if(texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 3) == 1)
                    {
                        lightMtl.ColorTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    #region STRENGTH
                    // VRayLightMtl && CoronaLightMtl not texture of strength
                    lightMtl.Strength = RedHaloTools.GetValeByID<float>(material, 0, 2);
                    #endregion

                    #region OPACITY
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 7);
                    if(texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 8) == 1)
                    {
                        lightMtl.OpacityTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    #region OTHERS
                    lightMtl.UseTwoSided = RedHaloTools.GetValeByID<int>(material, 0, 4) == 1;
                    #endregion

                    break;

                case "CoronaLightMtl":
                    #region COLOR
                    maxClr = RedHaloTools.GetValeByID<IColor>(material, 0, 3);

                    lightMtl.Color = RedHaloTools.IColorToString(maxClr, true);
                    lightMtl.ColorArray = new[] { maxClr.R, maxClr.G, maxClr.B };

                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 2);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 1) == 1)
                    {
                        lightMtl.ColorTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    #region STRENGTH
                    // VRayLightMtl && CoronaLightMtl not texture of strength
                    lightMtl.Strength = RedHaloTools.GetValeByID<float>(material, 0, 0);
                    #endregion

                    #region OPACITY
                    texmap = RedHaloTools.GetValeByID<ITexmap>(material, 0, 10);
                    if (texmap != null && RedHaloTools.GetValeByID<int>(material, 0, 11) == 1)
                    {
                        lightMtl.OpacityTexmap = MaterialUtils.ExportTexmap(texmap);
                    }
                    #endregion

                    #region OTHERS
                    lightMtl.UseTwoSided = RedHaloTools.GetValeByID<int>(material, 0, 17) == 1;
                    lightMtl.VisibleReflect = RedHaloTools.GetValeByID<int> (material, 0, 7) == 1;
                    lightMtl.VisibleDirect = RedHaloTools.GetValeByID<int>(material, 0, 8) == 1;
                    lightMtl.VisibleRefract = RedHaloTools.GetValeByID<int>(material, 0, 9) == 1;
                    lightMtl.AffectAlpha = RedHaloTools.GetValeByID<int>(material, 0, 4) == 1;
                    #endregion

                    break;
                
                default:
                    break;
            }

            return lightMtl;
        }
    }
}
 