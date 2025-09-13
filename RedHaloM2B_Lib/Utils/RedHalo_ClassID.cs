using Autodesk.Max;

namespace RedHaloM2B
{
    internal class RedHaloClassID
    {
        public static readonly IClass_ID VRayPlane      = RedHaloCore.Global.Class_ID.Create(0x628140F6, 0x3BDB0E0C);
        public static readonly IClass_ID ChaosScatter   = RedHaloCore.Global.Class_ID.Create(0x63B20069, 0x325E5221);
        public static readonly IClass_ID LinkComposite  = RedHaloCore.Global.Class_ID.Create(0x608B1C8F, 0x2F0B0D2D);
        public static readonly IClass_ID VRayProxy      = RedHaloCore.Global.Class_ID.Create(0x6CF53873, 0x1FF85498);
        public static readonly IClass_ID CoronaProxy    = RedHaloCore.Global.Class_ID.Create(0x2B0264BC, 0x05EC07F5);
        public static readonly IClass_ID OsmSkin        = RedHaloCore.Global.Class_ID.Create(0x0095C723, 0x00015666);
        public static readonly IClass_ID OsmEditPoly    = RedHaloCore.Global.Class_ID.Create(0x79AA6E1D, 0x71A075B7);
        public static readonly IClass_ID Teapot         = RedHaloCore.Global.Class_ID.Create(0xACAD13D3, 0xACAD26D9);
        //public static readonly IClass_ID LinkComposite    = RedHaloCore.Global.Class_ID.Create(0x608B1C8F, 0x2F0B0D2D);

        //#MultiScatter

        // Mutli Material
        public static readonly IClass_ID VRayDoubleMaterial = RedHaloCore.Global.Class_ID.Create(0x6066686A, 0x11731B4B);        
        public static readonly IClass_ID VRayOverrideMtl    = RedHaloCore.Global.Class_ID.Create(0x15D20E6B, 0x54E217EB);
        public static readonly IClass_ID VRayBlendMtl       = RedHaloCore.Global.Class_ID.Create(0x3DA0041B, 0x0F436E31);
        public static readonly IClass_ID VRay2SidedMtl      = RedHaloCore.Global.Class_ID.Create(0x6066686A, 0x11731B4B);

        public static readonly IClass_ID CoronaLayerMtl     = RedHaloCore.Global.Class_ID.Create(0x65486584, 0x8425554E);
        public static readonly IClass_ID CoronaLayeredMtl   = RedHaloCore.Global.Class_ID.Create(0x65486584, 0x8425554E);
        public static readonly IClass_ID CoronaRaySwitchMtl = RedHaloCore.Global.Class_ID.Create(0x6816116A, 0xABE651DE);

        public static readonly IClass_ID DoubleSidedMtl     = RedHaloCore.Global.Class_ID.Create(0x00000210, 0x00000000);
        public static readonly IClass_ID BlendMtl           = RedHaloCore.Global.Class_ID.Create(0x00000250, 0x00000000);
        public static readonly IClass_ID ShellMaterial      = RedHaloCore.Global.Class_ID.Create(0x00000255, 0x00000000);
        public static readonly IClass_ID TopBottomMtl       = RedHaloCore.Global.Class_ID.Create(0x00000100, 0x00000000);

        // Single Material

        public static readonly IClass_ID StandardMaterial   = RedHaloCore.Global.Class_ID.Create(0x00000002, 0x00000000);

        public static readonly IClass_ID VRayMtl            = RedHaloCore.Global.Class_ID.Create(0x37BF3F2F, 0x7034695C);
        
        public static readonly IClass_ID VRayCarPaintMtl    = RedHaloCore.Global.Class_ID.Create(0x38312C74, 0x47280883);
        public static readonly IClass_ID VRayCarPaintMtl2   = RedHaloCore.Global.Class_ID.Create(0x56BC47E2, 0x03E008C7);

        public static readonly IClass_ID CoronaLegacyMtl    = RedHaloCore.Global.Class_ID.Create(0x70BE6506, 0x448931DD);
        public static readonly IClass_ID CoronaPhysicalMtl  = RedHaloCore.Global.Class_ID.Create(0x6912AB89, 0x87151720);

        // Light Material
        public static readonly IClass_ID VRayLightMtl       = RedHaloCore.Global.Class_ID.Create(0x7CCF263E, 0x3F5B39B9);
        public static readonly IClass_ID CoronaLightMtl     = RedHaloCore.Global.Class_ID.Create(0x53B7717A, 0x500D5093);

        // Light Objects
        public static readonly IClass_ID VrayLight          = RedHaloCore.Global.Class_ID.Create(0x3C5575A1, 0x5FD602DF);

    }
}
