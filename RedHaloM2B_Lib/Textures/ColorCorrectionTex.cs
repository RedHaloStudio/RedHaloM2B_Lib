using Autodesk.Max;
using System.Xml.Serialization;

namespace RedHaloM2B.Textures
{
    internal class ColorCorrectionTex : BaseTex
    {
        [XmlIgnore]
        public IColor Color {  get; set; }
        public float Hue {  get; set; }
        public float Saturation { get; set; }
        public float Brightness {  get; set; }
        public float Contrast {  get; set; }
        public float Gamma {  get; set; }

        public string Map {  get; set; } // 不定类型
    }
}