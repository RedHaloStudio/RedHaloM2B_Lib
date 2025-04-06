using Autodesk.Max;
using RedHaloM2B.Materials;
using RedHaloM2B.Nodes;
using RedHaloM2B.RedHaloUtils;
using System;
using System.Diagnostics;
using System.Linq;


namespace RedHaloM2B
{
    partial class Exporter
    {
        public static RedHaloBlendMtl ExportBlendMaterial(IMtl inMaterial, int materialIndex)
        {
            string materialName = $"{materialIndex:D5}";
            string materialOriginalName = inMaterial.Name;

            //Set new name
            //inMaterial.Name = materialName;

            RedHaloBlendMtl blendMtl = new()
            {
                Name = materialName,
                SourceName = materialOriginalName,
                Type = "BlendMaterial",

                Layers = [],
            };
            
            IMtl subMtl = null;
            IColor color = RedHaloCore.Global.Color.Create(0.5, 0.5, 0.5);
            bool enableMtl = false;
            IMtl baseMtl = null;
            float blendAmount = 0f;
            ITexmap texmap;

            if (inMaterial.ClassID.OperatorEquals(RedHaloClassID.VRayBlendMtl))
            {
                baseMtl = RedHaloTools.GetValeByID<IMtl>(inMaterial, 0, 0);

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
                    
                    subMtl = RedHaloTools.GetValeByID<IMtl>(inMaterial, 0, 4 * i + 8);
                    color = RedHaloTools.GetValeByID<IColor>(inMaterial, 0, 4 * i + 9);
                    texmap = RedHaloTools.GetValeByID<ITexmap>(inMaterial, 0, 4 * i + 10);
                    blendAmount = RedHaloTools.GetValeByID<float>(inMaterial, 0, 4 * i + 11) / 100f;

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
            }
            else if (inMaterial.ClassID.OperatorEquals(RedHaloClassID.CoronaLayerMtl))
            {
                baseMtl = RedHaloTools.GetValeByID<IMtl>(inMaterial, 0, 0);
                if (baseMtl == null)
                {
                    return null;
                }

                blendMtl.BaseMaterial = baseMtl.Name;

                var NumberParamBlocks = inMaterial.NumParamBlocks;

                var pb2 = inMaterial.GetParamBlockByID(0);

                // CoronaLayerMtl 共有10个图层，只取前9个，兼容VRayBlendMtl
                for (int i = 0; i < 9; i++)
                {
                    RedHaloLayer layer = new();

                    // layerOn id is 4
                    var layerOnID = pb2.IndextoID(4);
                    var layerOn = pb2.GetInt(layerOnID, 0, i) == 1;

                    // Layer MtlTab ID is 1
                    var mtlID = pb2.IndextoID(1);
                    IMtl SubLayer = pb2.GetMtl(mtlID, 0, i);

                    if (SubLayer == null || !layerOn)
                    {
                        continue;
                    }
                    layer.Material = SubLayer.Name;

                    // Material Blend Amount
                    layer.MaterialBlend = pb2.GetFloat(pb2.IndextoID(3), 0, i);// / 100f;


                    // Mask
                    // MaskOn
                    var maskOnID = pb2.IndextoID(5);
                    var maskOn = pb2.GetInt(maskOnID, 0, i) == 1;
                    // MaskAmount
                    var maskAmountID = pb2.IndextoID(6);
                    // MaskTexmap
                    var maskTexmapID = pb2.IndextoID(2);
                    ITexmap maskTexmap = pb2.GetTexmap(maskTexmapID, 0, i);

                    if (maskTexmap != null || !maskOn)
                    {
                        layer.MaskTexmap = MaterialUtils.ExportTexmap(maskTexmap);
                        layer.MaskAmount = pb2.GetFloat(maskAmountID, 0, i);
                    }

                    blendMtl.Layers.Add(layer);
                }
            }

            return blendMtl;
        }
    }
}
