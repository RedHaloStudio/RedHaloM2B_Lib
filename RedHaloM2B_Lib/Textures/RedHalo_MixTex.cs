using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedHaloM2B.Textures
{
    public class MixTex
    {
        public string name {  get; set; }
        public string type { get; set; }

        public float mixAmount { get; set; }

        public object map1 { get; set; }
        public object map2 { get; set; }
        public object maskMap {  get; set; }
    }
}
