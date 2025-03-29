using Autodesk.Max;
using RedHaloM2B.Materials;
using RedHaloM2B.Nodes;
using RedHaloM2B.RedHaloUtils;
using System.Diagnostics;


namespace RedHaloM2B
{
    partial class Exporter
    {
        public static RedHaloOverrideMtl ExportOverrideMaterial(IMtl inMaterial, int materialIndex)
        {
            string materialName = $"{materialIndex:D5}";
            string materialOriginalName = inMaterial.Name;

            //Set new name
            //inMaterial.Name = materialName;

            var overrideMtl = new RedHaloOverrideMtl
            {
                Name = materialName,
                SourceName = materialOriginalName,
                Type = "OverrideMaterial",
            };
            
            IMtl subMtl = null;
            IColor color = RedHaloCore.Global.Color.Create(0.5, 0.5, 0.5);
            bool enbleBaseMtl = true;
            bool enbleMtl = false;

            RedHaloTools.GetParams(inMaterial);

            if (inMaterial.ClassID.OperatorEquals(RedHaloClassID.VRayOverrideMtl))
            {
                // Base Material
                enbleBaseMtl = RedHaloTools.GetValeByID<int>(inMaterial, 0, 1) == 1;
                subMtl = RedHaloTools.GetValeByID<IMtl>(inMaterial, 0, 0);

                if (!enbleBaseMtl || subMtl == null)
                {
                    return null;
                }

                overrideMtl.BaseMaterial = subMtl.Name;

                // GI Material
                subMtl = RedHaloTools.GetValeByID<IMtl>(inMaterial, 0, 2);
                enbleMtl = RedHaloTools.GetValeByID<int>(inMaterial, 0, 3) == 1;
                if (enbleMtl && subMtl != null)
                {
                    overrideMtl.GIMaterial = subMtl.Name;
                }

                // Reflection Material
                subMtl = RedHaloTools.GetValeByID<IMtl>(inMaterial, 0, 4);
                enbleMtl = RedHaloTools.GetValeByID<int>(inMaterial, 0, 5) == 1;
                if (enbleMtl && subMtl != null)
                {
                    overrideMtl.ReflectionMaterial = subMtl.Name;
                }

                // Refraction Material
                subMtl = RedHaloTools.GetValeByID<IMtl>(inMaterial, 0, 6);
                enbleMtl = RedHaloTools.GetValeByID<int>(inMaterial, 0, 7) == 1;
                if (enbleMtl && subMtl != null)
                {
                    overrideMtl.RefractionMaterial = subMtl.Name;
                }

                // Shadow Material
                subMtl = RedHaloTools.GetValeByID<IMtl>(inMaterial, 0, 8);
                enbleMtl = RedHaloTools.GetValeByID<int>(inMaterial, 0, 9) == 1;
                if (enbleMtl && subMtl != null)
                {
                    overrideMtl.ShadowMaterial = subMtl.Name;
                }
            }
            else if (inMaterial.ClassID.OperatorEquals(RedHaloClassID.CoronaRaySwitchMtl))
            {
                // Base Material / Direct Material
                enbleBaseMtl = RedHaloTools.GetValeByID<int>(inMaterial, 0, 9) == 1;

                if (!enbleBaseMtl)
                {
                    return null; 
                }

                subMtl = RedHaloTools.GetValeByID<IMtl>(inMaterial, 0, 3);

                if(subMtl == null)
                {
                    return null;
                }

                overrideMtl.BaseMaterial = subMtl.Name;

                // GI Material
                subMtl = RedHaloTools.GetValeByID<IMtl>(inMaterial, 0, 0);
                enbleMtl = RedHaloTools.GetValeByID<int>(inMaterial, 0, 1) == 1;
                if(enbleMtl && subMtl != null)
                {
                    overrideMtl.GIMaterial = subMtl.Name;
                }

                // Reflection Material
                subMtl = RedHaloTools.GetValeByID<IMtl>(inMaterial, 0, 1);
                enbleMtl = RedHaloTools.GetValeByID<int>(inMaterial, 0, 7) == 1;
                if (enbleMtl && subMtl != null)
                {
                    overrideMtl.ReflectionMaterial = subMtl.Name;
                }

                // Refraction Material
                subMtl = RedHaloTools.GetValeByID<IMtl>(inMaterial, 0, 2);
                enbleMtl = RedHaloTools.GetValeByID<int>(inMaterial, 0, 8) == 1;
                if (enbleMtl && subMtl != null)
                {
                    overrideMtl.RefractionMaterial = subMtl.Name;
                }
            }

            return overrideMtl;
        }
    }
}
