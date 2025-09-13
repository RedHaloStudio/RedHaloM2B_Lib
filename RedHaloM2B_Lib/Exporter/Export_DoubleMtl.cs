using Autodesk.Max;
using RedHaloM2B.Materials;
using RedHaloM2B.Nodes;
using RedHaloM2B.RedHaloUtils;
using System.Diagnostics;


namespace RedHaloM2B
{
    partial class Exporter
    {
        public static RedHaloDoubleMtl ExportDoubleMtl(IMtl inMaterial, int materialIndex)
        {
            string materialName = $"material_{materialIndex:D5}";
            string materialOriginalName = inMaterial.Name;

            //Set new name
            inMaterial.Name = materialName;

            var doubleMtl = new RedHaloDoubleMtl
            {
                Name = materialName,
                SourceName = materialOriginalName,
                Type = "DoubleMaterial",
            };

            IMtl subMtl = null;
            IColor color = RedHaloCore.Global.Color.Create(0.5, 0.5, 0.5);
            bool enbleFront = true;
            bool enbleBack = false;


            enbleFront = RedHaloTools.GetValueByID<int>(inMaterial, 0, 2) == 1;
            enbleBack = RedHaloTools.GetValueByID<int>(inMaterial, 0, 3) == 1;

            // 如果前面材质不可见，直接返回
            // Front material
            subMtl = RedHaloTools.GetValueByID<IMtl>(inMaterial, 0, 1);
            if (enbleFront || subMtl == null)
            {
                return null;
            }

            doubleMtl.FrontMaterial = subMtl.Name;

            // Back material
            subMtl = RedHaloTools.GetValueByID<IMtl>(inMaterial, 0, 1);
            if (subMtl != null && enbleBack)
            {
                doubleMtl.BackMaterial = subMtl.Name;
            }

            // Mask color
            // Mix amount
            doubleMtl.MixAmount = RedHaloTools.GetValueByID<float>(inMaterial, 0, 6) / 100f;

            return doubleMtl;
        }
    }
}
