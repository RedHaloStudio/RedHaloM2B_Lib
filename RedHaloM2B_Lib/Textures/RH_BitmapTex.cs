namespace RedHaloM2B.Textures
{
    public class BitmapTex : BaseTex
    {
        public string filename { get; set; }
        public string name { get; set; }

        public float clipU { get; set; }
        public float clipV { get; set; }
        public float clipW { get; set; }
        public float clipH { get; set; }

        public string imageWrap { get; set; }

        public int alphaSource { get; set; }

        public float uScale { get; set; }
        public float vScale { get; set; }
        public float uOffset { get; set; }
        public float vOffset { get; set; }

        public float wAngle { get; set; }
        public float vAngle { get; set; }
        public float uAngle { get; set; }

        // 贴图类型：纹理/环境
        public string mappingType { get; set; }

        // 贴图方式
        public int mapping { get; set; }
        public int monoOutput { get; set; }
        public int rgbOutput { get; set; }
        public int premultAlpha { get; set; }

        public BitmapTex() { }
    }
}
