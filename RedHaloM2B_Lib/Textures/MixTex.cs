using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedHaloM2B.Textures
{
    public class MixTex : BaseTex
    {
        public string Color1 { get; set; }
        public string Color2 { get; set; }
        public float MixAmount { get; set; }

        //public List<T> Map1 { get; set; }
        //public List<BaseTexture> Map2 { get; set; }
        //public List<BaseTexture> MixMap { get; set; }
        public MixTex() { }
    }
}
