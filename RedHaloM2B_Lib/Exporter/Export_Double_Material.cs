using Autodesk.Max;
using RedHaloM2B.Materials;
using RedHaloM2B.Nodes;
using RedHaloM2B.RedHaloUtils;
using System.Diagnostics;


namespace RedHaloM2B
{
    partial class Exporter
    {
        public static RedHaloDoubleMtl ExportDoubleMaterial(IMtl inMaterial, int materialIndex)
        {
            string materialName = $"{materialIndex:D5}";
            string materialOriginalName = inMaterial.Name;

            //Set new name
            inMaterial.Name = materialName;

            var doubleMtl = new RedHaloDoubleMtl
            {
                Name = materialName,
                SourceName = materialOriginalName,
                Type = "DoubleMaterial",
            };

            //Debug.Print($"{inMaterial.ClassID.PartA.ToString("X")}");
            //Debug.Print($"{inMaterial.ClassID.PartB.ToString("X")}");

            ITexmap texmap = null;
            IMtl subMtl = null;
            IColor color = RedHaloCore.Global.Color.Create(0.5, 0.5, 0.5);
            bool enbleFront = true;
            bool enbleBack = false;

            if (inMaterial.ClassID.OperatorEquals(RedHaloClassID.StandardMaterial))
            {
                enbleFront = RedHaloTools.GetValeByID<int>(inMaterial, 0, 2) == 1;
                enbleBack = RedHaloTools.GetValeByID<int>(inMaterial, 0, 3) == 1;

                // 如果前面材质不可见，直接返回
                // Front material
                subMtl = RedHaloTools.GetValeByID<IMtl>(inMaterial, 0, 1);
                if (enbleFront || subMtl == null)
                {
                    return null;
                }
                
                doubleMtl.FrontMaterial = subMtl.Name;

                // Back material
                subMtl = RedHaloTools.GetValeByID<IMtl>(inMaterial, 0, 1);
                if (subMtl != null && enbleBack)
                {
                    doubleMtl.BackMaterial = subMtl.Name;
                }

                // Mask color

                // Mix amount
                doubleMtl.MixAmount = RedHaloTools.GetValeByID<float>(inMaterial, 0, 6) / 100f;
            }
            else if (inMaterial.ClassID.OperatorEquals(RedHaloClassID.VRayDoubleMaterial))
            {
                // Front material, otherwise return null
                subMtl = RedHaloTools.GetValeByID<IMtl>(inMaterial, 0, 0);
                if (subMtl == null) {
                    return null;
                }

                doubleMtl.FrontMaterial = subMtl.Name;

                // Back material
                subMtl = RedHaloTools.GetValeByID<IMtl>(inMaterial, 1, 0);
                if (subMtl != null && RedHaloTools.GetValeByID<int>(inMaterial, 0, 4) == 1)
                {
                    doubleMtl.BackMaterial = subMtl.Name;
                }
                
                // Mask color
                color = RedHaloTools.GetValeByID<IColor>(inMaterial, 0, 3);
                doubleMtl.MaskColor = RedHaloTools.IColorToString(color, true);

                // Mask texmap
                texmap = RedHaloTools.GetValeByID<ITexmap>(inMaterial, 0, 5);
                if (texmap != null && RedHaloTools.GetValeByID<int>(inMaterial, 0, 7) == 1)
                {
                    doubleMtl.MaskTexmap = MaterialUtils.ExportTexmap(texmap);
                }

                // Mix amount
                doubleMtl.MixAmount = RedHaloTools.GetValeByID<float>(inMaterial, 0, 6) / 100f;

            }

            return doubleMtl;
        }
    }
}
