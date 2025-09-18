using Autodesk.Max;
using RedHaloM2B.Materials;
using RedHaloM2B.RedHaloUtils;

namespace RedHaloM2B
{
    partial class Exporter
    {
        public static RedHaloBlendMtl ExportVRayBlendMtl(IMtl inMaterial)
        {
            string materialOriginalName = inMaterial.Name;
            string materialType = inMaterial.ClassName(false);

            string materialName = $"M_{RedHaloTools.CalcMD5FromString(inMaterial.Name)}";

            //Set new name
            inMaterial.Name = materialName;

            RedHaloBlendMtl blendMtl = new()
            {
                Name = materialName,
                SourceName = materialOriginalName,
                Type = "blend_material",

                Layers = [],
            };

            IMtl subMtl = null;
            IColor color = RedHaloCore.Global.Color.Create(0.5, 0.5, 0.5);
            bool enableMtl = false;
            IMtl baseMtl = null;
            float blendAmount = 0f;
            ITexmap texmap;

            baseMtl = RedHaloTools.GetValueByID<IMtl>(inMaterial, 0, 0);

            var pb2 = inMaterial.GetParamBlock(0);

            if (baseMtl == null)
            {
                return null;
            }

            blendMtl.BaseMaterial = baseMtl.Name;

            for (int i = 0; i < pb2.Count(pb2.IndextoID(1)); i++)
            {
                enableMtl = pb2.GetInt(pb2.IndextoID(1), 0, i) == 1;

                if (!enableMtl)
                {
                    continue;
                }

                subMtl = RedHaloTools.GetValueByID<IMtl>(inMaterial, 0, 4 * i + 8);
                color = RedHaloTools.GetValueByID<IColor>(inMaterial, 0, 4 * i + 9);
                texmap = RedHaloTools.GetValueByID<ITexmap>(inMaterial, 0, 4 * i + 10);
                blendAmount = RedHaloTools.GetValueByID<float>(inMaterial, 0, 4 * i + 11) / 100f;

                RedHaloLayer layer = new() { };
                if (subMtl != null)
                {
                    layer.Material = subMtl.Name;
                    layer.Color = [color.R, color.G, color.B];

                    if (texmap != null)
                    {
                        layer.MaskTexmap = MaterialUtils.ExportTexmap(texmap);
                    }

                    layer.MaskAmount = blendAmount;

                    blendMtl.Layers.Add(layer);
                }
            }

            return blendMtl;
        }
    }
}
