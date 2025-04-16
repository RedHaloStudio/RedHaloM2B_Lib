using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedHaloM2B.Utils
{
    internal class CropParameters
    {
        public bool applyCropping {get; set; }
        public float horizontalStart { get; set; }
        public float verticalStart { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
    }
}
