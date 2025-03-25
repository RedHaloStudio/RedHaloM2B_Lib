using RedHaloM2B.Textures;
using Newtonsoft.Json;

namespace RedHaloM2B.Materials
{
    internal class RedHaloLight
    {
        public string Name { get; set; }
        public string SourceName { get; set; }
        public string ID { get; set; }
        public string Type { get; set; }

        public string Color { get; set; }
        public TexmapInfo ColorTexmap { get; set; }

        public float Strength { get; set; }
        public TexmapInfo StrengthTexmap { get; set; }

        public TexmapInfo OpacityTexmap { get; set; }

        public bool UseTwoSided { get; set; }

        public bool VisibleReflect { get; set; }
        public bool VisibleDirect { get; set; }
        public bool VisibleRefract { get; set; }

        public bool AffectAlpha { get; set; }
    }
}
