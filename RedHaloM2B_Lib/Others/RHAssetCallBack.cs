using Autodesk.Max;
using Autodesk.Max.MaxSDK.AssetManagement;
using Autodesk.Max.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RedHaloM2B.Others
{
    internal class RHAssetEnumCallback : IAssetEnumCallback //AssetEnumCallback
    {
        private List<string> NameList = new();
        private static List<string> PathList = new();
        public List<string> BitmapPaths
        {
            get { return PathList; }
            set { PathList = value; }
        }

        public IntPtr NativePointer => throw new NotImplementedException();

        public void RecordAsset(IAssetUser asset) // Added 'override' to fix CS0114
        {
            if (asset == null)
            {
                Debug.Print($"asset is null");
            }
            Debug.Print($"rcd {asset.FileName}, {asset.FullFilePath}");
            if (!NameList.Contains(asset.FileName))
            {
                NameList.Add(asset.FileName);
                PathList.Add(asset.FullFilePath);
            }
        }

        public bool Equals(IAssetEnumCallback other)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}