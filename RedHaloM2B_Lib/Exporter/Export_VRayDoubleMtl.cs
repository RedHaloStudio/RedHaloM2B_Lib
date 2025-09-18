using Autodesk.Max;
using RedHaloM2B.Materials;
using RedHaloM2B.RedHaloUtils;

namespace RedHaloM2B
{
    partial class Exporter
    {
        public static RedHaloDoubleMtl ExportVRayDoubleMtl(IMtl inMaterial)
        {
            string materialOriginalName = inMaterial.Name;
            string materialName = $"M_{RedHaloTools.CalcMD5FromString(materialOriginalName)}";

            //Set new name
            inMaterial.Name = materialName;

            var doubleMtl = new RedHaloDoubleMtl
            {
                Name = materialName,
                SourceName = materialOriginalName,
                Type = "double_material",
            };

            ITexmap texmap = null;
            IMtl subMtl = null;
            IColor color = RedHaloCore.Global.Color.Create(0.5, 0.5, 0.5);

            // Front material, otherwise return null
            subMtl = RedHaloTools.GetValueByID<IMtl>(inMaterial, 0, 0);
            if (subMtl == null)
            {
                return null;
            }

            doubleMtl.FrontMaterial = subMtl.Name;

            // Back material
            subMtl = RedHaloTools.GetValueByID<IMtl>(inMaterial, 1, 0);
            if (subMtl != null && RedHaloTools.GetValueByID<int>(inMaterial, 0, 4) == 1)
            {
                doubleMtl.BackMaterial = subMtl.Name;
            }

            // Mask color
            color = RedHaloTools.GetValueByID<IColor>(inMaterial, 0, 3);
            doubleMtl.MaskColor = [color.R, color.G, color.B, 1];

            // Mask texmap
            texmap = RedHaloTools.GetValueByID<ITexmap>(inMaterial, 0, 5);
            if (texmap != null && RedHaloTools.GetValueByID<int>(inMaterial, 0, 7) == 1)
            {
                doubleMtl.MaskTexmap = MaterialUtils.ExportTexmap(texmap);
            }

            // Mix amount
            doubleMtl.MixAmount = RedHaloTools.GetValueByID<float>(inMaterial, 0, 6) / 100f;

            return doubleMtl;
        }
    }
}
