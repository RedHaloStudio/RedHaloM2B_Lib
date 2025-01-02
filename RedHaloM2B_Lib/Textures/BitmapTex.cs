using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedHaloM2B.Textures
{
    internal class BitmapTex : BaseTex
    {
        public string Filename {  get; set; }

        public float ClipU {  get; set; }
        public float ClipV { get; set; }
        public float ClipW {  get; set; }
        public float ClipH {  get; set; }
        public int AlphaSource { get; set; }

        public string ImageWrap {  get; set; }

        public float UScale {  get; set; }
        public float VScale { get; set; }

        public float UOffset {  get; set; }
        public float VOffset { get; set; }

        public float WAngle {  get; set; }
        public float VAngle { get; set; }
        public float UAngle { get; set; }

        // 贴图类型：纹理/环境
        public int MappingType {  get; set; }

        // 贴图方式
        public int Mapping {  get; set; }

        public int MonoOutput { get; set; }
        public int RGBOutput {  get; set; }

        public int PreMultAlpha {  get; set; }
    }
}
