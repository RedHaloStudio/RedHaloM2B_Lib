using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;
using Autodesk.Max.MaxSDK.AssetManagement;

namespace RedHaloM2B
{
    internal class AssetEnumCallBackExample : IAssetEnumCallback
    {
        public IntPtr NativePointer => throw new NotImplementedException();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool Equals(IAssetEnumCallback other)
        {
            throw new NotImplementedException();
        }
        
        public void RecordAsset(IAssetUser asset)
        {
            Debug.Print(asset.ToString());
        }
    }
}
