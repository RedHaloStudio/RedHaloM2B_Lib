using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedHaloM2B.Textures
{
    internal class CheckerTex : BaseTex
    {
        public IColor Color1 { get; set; }
        public IColor Color2 { get; set; }

        public string Map1 {  get; set; }
        public string Map2 { get; set; }
    }
}
