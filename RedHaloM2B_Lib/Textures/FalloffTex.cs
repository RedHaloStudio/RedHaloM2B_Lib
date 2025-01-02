using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedHaloM2B.Textures
{
    internal class FalloffTex : BaseTex
    {
        /*
         #color1
        #map1Amount
        #map1
        #map1on
        #color2
        #map2Amount
        #map2
        #map2on
        #type
        #direction
        #node
        #mtlIOROverride
        #ior
        #extrapolateOn
        #nearDistance
        #farDistance
         */

        public IColor Color1 {  get; set; }
        public IColor Color2 { get; set; }
        public float Map1Amout { get; set; }
        public float Map2Amout { get; set;}

        public int Type {  get; set; }
        public int Direction { get; set; }
        public float IOR {  get; set; }
    }
}
