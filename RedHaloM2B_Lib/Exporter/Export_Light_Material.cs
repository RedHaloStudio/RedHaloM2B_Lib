using Autodesk.Max;
using RedHaloM2B.Materials;
using RedHaloM2B.RedHaloUtils;

namespace RedHaloM2B
{
    partial class Exporter
    {
        public static RedHaloLightMtl ExportLightMaterial(IMtl material)
        {
            string sourceName = material.Name;
            string newName = $"M_{RedHaloTools.CalcMD5FromString(sourceName)}";

            IColor maxClr = RedHaloCore.Global.Color.Create(0.8, 0.8, 0.8);
            ITexmap texmap = null;

            RedHaloLightMtl lightMtl = new()
            {
                Name = newName,
                SourceName = sourceName,
                Type = material.ClassName(false),
            };

            switch (material.ClassName(false))
            {
                case "VRayLightMtl":
                    #region COLOR
                    maxClr = RedHaloTools.GetValueByID<IColor>(material, 0, 0);

                    lightMtl.Color = [maxClr.R, maxClr.G, maxClr.B, 1]; //RedHaloTools.IColorToString(maxClr, true);

                    texmap = RedHaloTools.GetValueByID<ITexmap>(material, 0, 2);
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 3) == 1)
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
                    if (texmap != null && RedHaloTools.GetValueByID<int>(material, 0, 8) == 1)
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
                    lightMtl.VisibleReflect = RedHaloTools.GetValueByID<int>(material, 0, 7) == 1;
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
