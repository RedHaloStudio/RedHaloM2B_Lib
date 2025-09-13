using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedHaloM2B.Utils
{
    internal class RH_ParamID
    {
        // Standard Bitmap Parameters
        public const int Std_ClipU = 0;
        public const int Std_ClipV = 1;
        public const int Std_ClipW = 2;
        public const int Std_ClipH = 3;
        public const int Std_CroppingApply = 6;
        public const int Std_MonoOutput = 9;
        public const int Std_RgbOutput = 10;
        public const int Std_AlphaSource = 11;
        public const int Std_PremultAlpha = 12;
        public const int Std_UVGen = 14;
        public const int Std_Filename = 16;

        // VRayBitmap Parameters
        public const int VRay_Filename = 0;
        public const int VRay_OverallMult = 1;
        public const int VRay_RenderMult = 2;
        public const int VRay_EnvironType = 3;
        public const int VRay_BitmapInfo = 13;
        public const int VRay_CroppingApply = 18;
        public const int VRay_ClipU = 20;
        public const int VRay_ClipV = 21;
        public const int VRay_ClipW = 22;
        public const int VRay_ClipH = 23;
        public const int VRay_RgbOutput = 24;
        public const int VRay_MonoOutput = 25;
        public const int VRay_AlphaSource = 26;
        public const int VRay_UVGen = 29;
    }
}
