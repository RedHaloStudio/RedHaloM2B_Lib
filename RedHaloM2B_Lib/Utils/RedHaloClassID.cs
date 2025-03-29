using Autodesk.Max;

namespace RedHaloM2B
{
    internal class RedHaloClassID
    {
        public static readonly IClass_ID VRayPlane = RedHaloCore.Global.Class_ID.Create(0x628140F6, 0x3BDB0E0C);
        public static readonly IClass_ID ChaosScatter = RedHaloCore.Global.Class_ID.Create(0x63B20069, 0x325E5221);
        public static readonly IClass_ID LinkComposite = RedHaloCore.Global.Class_ID.Create(0x608B1C8F, 0x2F0B0D2D);
        public static readonly IClass_ID VRayProxy = RedHaloCore.Global.Class_ID.Create(0x6CF53873, 0x1FF85498);
        public static readonly IClass_ID CoronaProxy = RedHaloCore.Global.Class_ID.Create(0x2B0264BC, 0x05EC07F5);
        public static readonly IClass_ID OsmSkin = RedHaloCore.Global.Class_ID.Create(0x0095C723, 0x00015666);
        public static readonly IClass_ID OsmEditPoly = RedHaloCore.Global.Class_ID.Create(0x79AA6E1D, 0x71A075B7);
        public static readonly IClass_ID Teapot = RedHaloCore.Global.Class_ID.Create(0xACAD13D3, 0xACAD26D9);
        //public static readonly IClass_ID LinkComposite    = RedHaloCore.Global.Class_ID.Create(0x608B1C8F, 0x2F0B0D2D);

        //#MultiScatter

        public static readonly IClass_ID StandardMaterial = RedHaloCore.Global.Class_ID.Create(0x00000210, 0x00000000);
        public static readonly IClass_ID VRayDoubleMaterial = RedHaloCore.Global.Class_ID.Create(0x6066686A, 0x11731B4B);

    }
}
