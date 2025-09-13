using Autodesk.Max;
using RedHaloM2B.Materials;

namespace RedHaloM2B
{
    partial class Exporter
    {
        public static RedHaloOverrideMtl ExportCoronaRaySwitchMtl(IMtl inMaterial, int materialIndex)
        {
            string materialName = $"material_{materialIndex:D5}";
            string materialOriginalName = inMaterial.Name;

            //Set new name
            inMaterial.Name = materialName;

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


            // Base Material / Direct Material
            enbleBaseMtl = RedHaloTools.GetValueByID<int>(inMaterial, 0, 9) == 1;

            if (!enbleBaseMtl)
            {
                return null;
            }

            subMtl = RedHaloTools.GetValueByID<IMtl>(inMaterial, 0, 3);

            if (subMtl == null)
            {
                return null;
            }

            overrideMtl.BaseMaterial = subMtl.Name;

            // GI Material
            subMtl = RedHaloTools.GetValueByID<IMtl>(inMaterial, 0, 0);
            enbleMtl = RedHaloTools.GetValueByID<int>(inMaterial, 0, 1) == 1;
            if (enbleMtl && subMtl != null)
            {
                overrideMtl.GIMaterial = subMtl.Name;
            }

            // Reflection Material
            subMtl = RedHaloTools.GetValueByID<IMtl>(inMaterial, 0, 1);
            enbleMtl = RedHaloTools.GetValueByID<int>(inMaterial, 0, 7) == 1;
            if (enbleMtl && subMtl != null)
            {
                overrideMtl.ReflectionMaterial = subMtl.Name;
            }

            // Refraction Material
            subMtl = RedHaloTools.GetValueByID<IMtl>(inMaterial, 0, 2);
            enbleMtl = RedHaloTools.GetValueByID<int>(inMaterial, 0, 8) == 1;
            if (enbleMtl && subMtl != null)
            {
                overrideMtl.RefractionMaterial = subMtl.Name;
            }

            return overrideMtl;
        }
    }
}