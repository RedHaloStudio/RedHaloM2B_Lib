using RedHaloM2B.Textures;
using Newtonsoft.Json;
using RedHaloM2B.Nodes;

namespace RedHaloM2B.Materials
{
    internal class RedHaloLightMtl
    {
        public string Name { get; set; }
        public string SourceName { get; set; }
        public string ID { get; set; }
        public string Type { get; set; }

        public string Color { get; set; }
        public float[] ColorArray { get; set; }
        public TexmapInfo ColorTexmap { get; set; }

        public float Strength { get; set; }
        public TexmapInfo StrengthTexmap { get; set; }

        public TexmapInfo OpacityTexmap { get; set; }

        public bool UseTwoSided { get; set; }

        public bool VisibleReflect { get; set; }
        public bool VisibleDirect { get; set; }
        public bool VisibleRefract { get; set; }

        public bool AffectAlpha { get; set; }

        public RedHaloLightMtl() {
            Color = "1,1,1,1";
            ColorArray = new[] { 1f, 1f, 1f };

            UseTwoSided = true;
            VisibleReflect = true;
            VisibleDirect = true;
            VisibleRefract = true;
        }
    }
}
