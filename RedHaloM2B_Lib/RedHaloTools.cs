using Autodesk.Max;
using Autodesk.Max.MaxSDK;
using Newtonsoft.Json;
using RedHaloM2B.Materials;
using RedHaloM2B.Textures;
using RedHaloM2B.RedHaloUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Autodesk.Max.MaxSDK.Util;
using Autodesk.Max.MaxSDK.AssetManagement;

namespace RedHaloM2B
{
    public class RedHaloTools
    {
        // 取得CPU的核心数量，进行多线程计算
        public static int coreNumber = Environment.ProcessorCount;

        /// <summary>
        /// Replace all sym use "_"
        /// </summary>
        /// <param name="str"></param>
        /// <returns>String</returns>
        public static string FixSafeName(string str)
        {
            string reg = Regex.Replace(str, "[ \\[ \\] \\^ \\-*×――(^)（^）$%~!@#$…&%￥—+=<>《》!！??？:：•`·、。，；,.;\"‘’“”-]", "_");
            return reg;
        }

        // Create Temp Folder
        public static int CreateTempFolder(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return -1;
            }

            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);  // 删除目录及其所有内容
                }
                catch (Exception)
                {
                    return -2;
                }
            }

            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception)
            {
                return -3;
            }

            return 0;
        }

        // Calc MD5 From string
        public static string CalcMD5FromString(string str)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(str);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }

        // Calc MD5 From File
        public static string CalcMD5FromFile(string path)
        {
            if (Directory.Exists(path))
            {
                FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
                MD5 md5 = MD5.Create();
                byte[] hashBytes = md5.ComputeHash(file);
                file.Close();

                return BitConverter.ToString(hashBytes).Replace("-", "");
            }
            else
            {
                return string.Empty;
            }
        }

        // Get All Nodes
        /// <summary>
        /// Get all objects with children
        /// </summary>
        static IEnumerable<IINode> maxNodeScene
        {
            get { return GetAllNodeRecur(RedHaloCore.Core.RootNode); }
        }

        static IEnumerable<IINode> GetAllNodeRecur(IINode node)
        {
            for (int i = 0; i < node.NumberOfChildren; i++)
            {
                IINode childNode = node.GetChildNode(i);
                yield return childNode;

                foreach (var ch in GetAllNodeRecur(childNode))
                {
                    yield return ch;
                }
            }
        }

        public static List<IINode> GetSceneNodes()
        {
            HashSet<string> nodeNames = new HashSet<string>();
            List<IINode> sceneNodes = new List<IINode>();

            foreach (var node in maxNodeScene)
            {
                string _tmp = RedHaloTools.FixSafeName(node.Name);
                node.Name = _tmp;

                if (nodeNames.Contains(_tmp))
                {
                    string newname = _tmp;
                    RedHaloCore.Core.MakeNameUnique(ref newname);
                    node.Name = newname;
                }
                else
                {
                    nodeNames.Add(_tmp);
                }
                sceneNodes.Add(node);
            }

            return sceneNodes;
        }

        // 塌陷所有物体，保证物体的断开物体的关联
        public static void MakeObjectsUnique()
        {
            var nodes = GetSceneNodes();
            IINodeTab tab = RedHaloCore.Global.INodeTab.Create();
            HashSet<string> objKeys = new HashSet<string>();

            foreach (var node in nodes)
            {
                if (objKeys.Contains(node.Name)) { continue; }
                else
                {
                    RedHaloCore.Global.IInstanceMgr.InstanceMgr.GetInstances(node, tab);

                    if (tab.Count > 1)
                    {
                        RedHaloCore.Global.IInstanceMgr.InstanceMgr.MakeObjectsUnique(tab, 1);
                    }

                    for (int i = 0; i < tab.Count; i++)
                    {
                        objKeys.Add(tab[i].Name);
                    }
                }
            }
        }

        // Get All Files
        public static void GetAllFiles(string path, bool subfolder)
        {

        }

        public static int AddModifier(uint nodeHandle, IClass_ID mod_id)
        {
            try
            {
                IINode node = RedHaloCore.Core.GetINodeByHandle(nodeHandle);

                IObject obj = node.ObjectRef;
                IIDerivedObject dobj = RedHaloCore.Global.CreateDerivedObject(obj);
                object objMod = RedHaloCore.Core.CreateInstance(SClass_ID.Osm, mod_id as IClass_ID);
                IModifier mod = (IModifier)objMod;

                //dobj.AddModifier(mod, null, 0);
                //node.ObjectRef = dobj;

                RedHaloCore.Core.AddModifier(node, mod, 0);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                return -1;
            }

            return 1;
        }

        // Convert Shape to Poly
        public static void ConvertShapeToPoly()
        {
            IEnumerable<IINode> iNodes = GetSceneNodes().Where(o => o.ObjectRef.IsShapeObject == true);

            var tab = RedHaloCore.Global.INodeTab.Create();

            foreach (var iNode in iNodes)
            {
                IShapeObject shp = iNode.ObjectRef as IShapeObject;

                if (shp == null)
                {
                    continue;
                }

                if (shp.Renderable)
                {
                    IClass_ID editpolyMod = RedHaloCore.Global.Class_ID.Create(0x79aa6e1d, 0x71a075b7);

                    IObjectState os = iNode.ObjectRef.Eval(RedHaloCore.Core.Time);
                    IObject obj = os.Obj;

                    var rs = AddModifier(iNode.Handle, editpolyMod);

                    //if (obj.CanConvertToType(RedHaloCore.Global.PolyObjectClassID) == 1)
                    {
                        //var oo = obj.ConvertToType(RedHaloCore.Core.Time, RedHaloCore.Global.TriObjectClassID);
                    }
                }
            }
        }

        // Delete unsupport objects
        public static void DeleteUnsupportObjects()
        {

        }

        // 清理用户属性，blender不支持fbx用户属性
        public static void RemoveUserProperties()
        {

        }

        /// <summary>
        /// Convert Matrix to String [4*4] matrix string
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static string ConvertMatrixToString(IMatrix3 matrix)
        {
            StringBuilder matrixStr = new StringBuilder();
            matrixStr.Append(
                $"{matrix.GetColumn(0).X},{matrix.GetColumn(0).Y},{matrix.GetColumn(0).Z},{matrix.GetColumn(0).W}," +
                $"{matrix.GetColumn(1).X},{matrix.GetColumn(1).Y},{matrix.GetColumn(1).Z},{matrix.GetColumn(1).W}," +
                $"{matrix.GetColumn(2).X},{matrix.GetColumn(2).Y},{matrix.GetColumn(2).Z},{matrix.GetColumn(2).W}," +
                $"0,0,0,1"
            );
            return matrixStr.ToString();
        }

        /// <summary>
        /// Get Color From Kelvin
        /// </summary>
        /// <param name="temperature"></param>
        /// <returns></returns>
        public static /*string*/ IColor GetRgbFromKelvin(float temperature)
        {

            // Temperature must fit between 1000 and 40000 degrees.
            temperature = MathUtils.Clamp(temperature, 1000, 40000);

            // All calculations require temperature / 100, so only do the conversion once.
            temperature /= 100;

            // Compute each color in turn.
            int red, green, blue;

            // First: red.
            if (temperature <= 66)
            {
                red = 255;
            }
            else
            {
                // Note: the R-squared value for this approximation is 0.988.
                red = (int)(329.698727446 * (Math.Pow(temperature - 60, -0.1332047592)));
                red = MathUtils.Clamp(red, 0, 255);
            }

            // Second: green.
            if (temperature <= 66)
            {
                // Note: the R-squared value for this approximation is 0.996.
                green = (int)(99.4708025861 * Math.Log(temperature) - 161.1195681661);
            }
            else
            {
                // Note: the R-squared value for this approximation is 0.987.
                green = (int)(288.1221695283 * (Math.Pow(temperature - 60, -0.0755148492)));
            }

            green = MathUtils.Clamp(green, 0, 255);

            // Third: blue.
            if (temperature >= 66)
            {
                blue = 255;
            }
            else if (temperature <= 19)
            {
                blue = 0;
            }
            else
            {
                // Note: the R-squared value for this approximation is 0.998.
                blue = (int)(138.5177312231 * Math.Log(temperature - 10) - 305.0447927307);
                blue = MathUtils.Clamp(blue, 0, 255);
            }

            IColor color = RedHaloCore.Global.Color.Create(red / 255.0, green / 255.0, blue / 255.0);
            //string color = $"color {red} {green} {blue}";
            return color;
        }

        // Write xml file
        public static bool WriteFile<T>(T classType, string FileOutPath)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                TextWriter writer = new StreamWriter(FileOutPath);
                serializer.Serialize(writer, classType);
                writer.Close();

                return true;
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Get All Params Name
        /// </summary>
        /// <param name="mtl"></param>
        public static void GetParams(IMtl mtl)
        {
            for (int p = 0; p < mtl.NumParamBlocks; p++)
            {
                var pb2 = mtl.GetParamBlock(p);
                for (int d = 0; d < pb2.NumParams; d++)
                {
                    var pid = pb2.IndextoID(d);
                    var def = pb2.GetParamDef(pid);                  

                    Debug.WriteLine($"{p}\t{d}\t{def.Type}\t{def.IntName}");
                }
            }
        }

        /// <summary>
        /// Get All Params Name
        /// </summary>
        /// <param name="tex"></param>
        public static void GetParams(ITexmap tex)
        {
            for (int p = 0; p < tex.NumParamBlocks; p++)
            {
                var pb2 = tex.GetParamBlock(p);
                for (int d = 0; d < pb2.NumParams; d++)
                {
                    var pid = pb2.IndextoID(d);
                    var def = pb2.GetParamDef(pid);
                    Debug.WriteLine($"{p}-{d}\t{def.Type}\t{def.IntName}");
                }
            }
        }

        // 速度快，但是不一定能保证取得的数据是正确的，除非不会修改PB2的ID值
        public static T GetValeByID<T>(IMtl mtl, short paramBlockID, int paramID)
        {
            Type type = typeof(T);

            var pb2 = mtl.GetParamBlock(paramBlockID);

            if (type == typeof(float))
            {
                var result = 0.0f;
                pb2.GetValue(pb2.IndextoID(paramID), 0, ref result, RedHaloCore.Forever, 0);
                return (T)(Object)result;
            }

            if (type == typeof(IColor))
            {
                var result = RedHaloCore.Global.Color.Create(0, 0, 0);
                pb2.GetValue(pb2.IndextoID(paramID), 0, result, RedHaloCore.Forever, 0);

                return (T)(Object)result;
            }

            if (type == typeof(int))
            {
                var result = 0;
                pb2.GetValue(pb2.IndextoID(paramID), 0, ref result, RedHaloCore.Forever, 0);
                return (T)(Object)result;
            }

            if (type == typeof(ITexmap))
            {
                ITexmap result = pb2.GetTexmap(pb2.IndextoID(paramID), 0, RedHaloCore.Forever, 0);
                return (T)(Object)result;
            }

            if (type == typeof(IMtl))
            {
                var result = pb2.GetMtl(pb2.IndextoID(paramID), 0, 0);
                return (T)(Object)result;
            }

            return default(T);
        }

        public static T GetValeByID<T>(ITexmap tex, short paramBlockID, int paramID)
        {
            Type type = typeof(T);

            var pb2 = tex.GetParamBlock(paramBlockID);

            if (type == typeof(float))
            {
                var result = 0.0f;
                pb2.GetValue(pb2.IndextoID(paramID), 0, ref result, RedHaloCore.Forever, 0);
                return (T)(Object)result;
            }

            if (type == typeof(IColor))
            {
                var result = RedHaloCore.Global.Color.Create(0, 0, 0);
                pb2.GetValue(pb2.IndextoID(paramID), 0, result, RedHaloCore.Forever, 0);
                return (T)(Object)result;
            }
            
            if (type == typeof(IAColor))
            {
                var result = RedHaloCore.Global.AColor.Create(0, 0, 0, 1);
                pb2.GetValue(pb2.IndextoID(paramID), 0, result, RedHaloCore.Forever, 0);
                return (T)(Object)result;
            }

            if (type == typeof(int))
            {
                var result = 0;
                pb2.GetValue(pb2.IndextoID(paramID), 0, ref result, RedHaloCore.Forever, 0);
                return (T)(Object)result;
            }

            if (type == typeof(ITexmap))
            {
                ITexmap result = pb2.GetTexmap(pb2.IndextoID(paramID), 0, RedHaloCore.Forever, 0);
                return (T)(Object)result;
            }

            if (type == typeof(IPBBitmap))
            {
                IPBBitmap result = pb2.GetBitmap(pb2.IndextoID(paramID), 0, RedHaloCore.Forever, 0);
                return (T)(Object)result;
            }

            if (type == typeof(string))
            {
                var result = pb2.GetStr(pb2.IndextoID(paramID), 0, RedHaloCore.Forever, 0);
                return (T)(Object)result;
            }

            if (type == typeof(IReferenceTarget))
            {
                var result = pb2.GetReferenceTarget(pb2.IndextoID(paramID), 0, RedHaloCore.Forever, 0);
                return (T)(Object)result;
            }

            if (type == typeof(IIParamBlock2))
            {
                var result = pb2.GetParamBlock2(pb2.IndextoID(paramID), 0, RedHaloCore.Forever, 0);
                return (T)(Object)result;
            }

            return default;
        }

        public static void SetValueByID<T>(IMtl texmap, short paramBlockID, int paramID, ITexmap newTex)
        {
            var pb2 = texmap.GetParamBlock(paramBlockID);

            //pb2.SetValue(paramBlockID, paramID);
            pb2.SetValue(pb2.IndextoID(paramID), 0, newTex, 0);
        }

        public static void SetValueByID<T>(ITexmap texmap, short paramBlockID, int paramID, ITexmap newTex)
        {
            var pb2 = texmap.GetParamBlock(paramBlockID);

            //pb2.SetValue(paramBlockID, paramID);
            pb2.SetValue(pb2.IndextoID(paramID), 0, newTex, 0);
        }

        // 最好用，但是相对耗时
        public static T GetValueByName<T>(IMtl pb, string name)
        {
            Type t = typeof(T);

            for (int p = 0; p < pb.NumParamBlocks; p++)
            {
                var pb2 = pb.GetParamBlock(p);
                for (int d = 0; d < pb2.NumParams; d++)
                {
                    var pid = pb2.IndextoID(d);
                    var def = pb2.GetParamDef(pid);
                    if (def.IntName == name)
                    {
                        if (t == typeof(IColor))
                        {
                            return (T)(Object)pb2.GetColor(pid, 0, 0);
                        }

                        if (t == typeof(float))
                        {
                            return (T)(Object)pb2.GetFloat(pid, 0, 0);
                        }
                    }
                }
            }
            return default(T);
        }

        public enum ProxyMode
        {
            BBOX,
            WIRE,
            POINT,
            FULL
        }
        /// <summary>
        /// Change VRayProxy/CoronaProxy Display mode
        /// </summary>
        /// <param name="node">proxy object</param>
        /// <param name="proxyMode">Display Mode</param>
        public static void ChangeProxyDisplay(IINode node, ProxyMode proxyMode)
        {
            // Set VRayProxy/ CoronaProxy display mode
            var pb = node.ObjectRef.FindBaseObject().GetParamBlock(0);
            int displayMode = 0;
            switch (node.ObjectRef.ClassName(false))
            {
                case "VRayProxy":
                    switch (proxyMode)
                    {
                        case ProxyMode.BBOX:
                            displayMode = 0;
                            break;
                        case ProxyMode.WIRE:
                            displayMode = 2;
                            break;
                        case ProxyMode.POINT:
                            displayMode = 3;
                            break;
                        case ProxyMode.FULL:
                            displayMode = 4;
                            break;
                        default:
                            break;
                    }

                    pb.SetValue(pb.IndextoID(5), 0, displayMode, 0);
                    break;
                case "CProxy":
                    switch (proxyMode)
                    {
                        case ProxyMode.BBOX:
                            displayMode = 0;
                            break;
                        case ProxyMode.WIRE:
                            displayMode = 1;
                            break;
                        case ProxyMode.POINT:
                            displayMode = 2;
                            break;
                        case ProxyMode.FULL:
                            displayMode = 3;
                            break;
                        default:
                            break;
                    }
                    pb.SetValue(pb.IndextoID(13), 0, displayMode, 0);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Get Scene Materials and Make material unique name
        /// </summary>
        public static void GetSceneMaterials()
        {
            IMtlBaseLib sceneMtl = RedHaloCore.Core.SceneMtls;

            List<IMtl> materials = new List<IMtl>();

            for (int i = 0; i < sceneMtl.Count; i++)
            {
                var mtl = sceneMtl[i];
                if (mtl is IMtl material)
                {
                    materials.Add(material);

                    if (material.NumSubMtls > 0)
                    {
                        var subMtls = GetSceneMtlRecure(material);
                        materials.AddRange(subMtls);
                    }
                }
            }

            // 清除重复的材质并修复名字
            List<IMtl> uniqueMaterials = new List<IMtl>();
            HashSet<IMtl> materialSet = new HashSet<IMtl>();

            foreach (var mtl in materials)
            {
                // 修复材质名字
                mtl.Name = RedHaloTools.FixSafeName(mtl.Name);

                if (materialSet.Add(mtl))
                {
                    uniqueMaterials.Add(mtl);
                }
            }

            Dictionary<string, int> nameCounts = new Dictionary<string, int>();

            foreach (var m in uniqueMaterials)
            {
                // 如果名字已经存在，计数器加1
                if (nameCounts.ContainsKey(m.Name))
                {
                    nameCounts[m.Name]++;
                    m.Name = $"{m.Name}_{nameCounts[m.Name]}";
                }
                else
                {
                    nameCounts[m.Name] = 0;
                }
            }
        }

        // 使用递归方法取得材质的子材质
        public static IEnumerable<IMtl> GetSceneMtlRecure(IMtl mtl)
        {
            for (int i = 0; i < mtl.NumSubMtls; i++)
            {
                IMtl childMtl = mtl.GetSubMtl(i);

                if (childMtl != null)
                {
                    yield return childMtl;

                    foreach (var chm in GetSceneMtlRecure(childMtl))
                    {
                        yield return chm;
                    }

                }
            }
        }

        // 整理纹理路径
        public static void CollectTextures()
        {
            //Autodesk.Max.IInterface.EnumAuxFiles(Autodesk.Max.IAssetEnumCallback, uint)
            //AssetEnumCallBackExample callBackExample = new AssetEnumCallBackExample();
            var ast = RedHaloCore.Global.MaxSDK.AssetManagement.IAssetManager.Instance;

        }

        // Rescale whole scene
        public static void RescaleScene()
        {
            var fac = (float)RedHaloCore.Global.GetSystemUnitScale(5); //5 Meter

            if (fac != 1f)
            {
#if MAX2022 || MAX2023 || MAX2024
                RedHaloCore.Core.RescaleWorldUnits(fac, false);
#elif MAX2025
                IINodeTab iNodeTab = RedHaloCore.Global.INodeTab.Create();
                RedHaloCore.Core.RescaleWorldUnits(fac, false, iNodeTab);
#endif

                RedHaloCore.Global.SetSystemUnitInfo(5, 1);
            }
        }

        // 清理物体自定义属性，blender不支持fbx用户属性
        public static void CleanUserProperty()
        {
            foreach (var obj in RedHaloTools.GetSceneNodes())
            {
                obj.SetUserPropBuffer("");
            }
        }

        // 清理不支持的物体
        public static void CleanUnsupportObjects()
        {
            var objs = RedHaloTools.GetSceneNodes()
                .Where(o =>
                {
                    var ob = o.ObjectRef.ClassID;
#if MAX2022
                    return ob.PartA.Equals(RHClassID.cidVRayPlane.PartA) ||
                           ob.PartA.Equals(RHClassID.cidChaosScatter.PartA) ||
                           ob.PartA.Equals(RHClassID.cidLinkComposite.PartA);
#else
                    return ob.OperatorEquals(RHClassID.cidVRayPlane) ||
                           ob.OperatorEquals(RHClassID.cidChaosScatter) ||
                           ob.OperatorEquals(RHClassID.cidLinkComposite);
#endif
                }).ToList(); ;

            foreach (var obj in objs)
            {
                RedHaloCore.Core.DeleteNode(obj, true, true);
            }
        }

        // Remove Proxy's modifier
        public static void removeProxyModifier()
        {
            var objs = RedHaloTools.GetSceneNodes()
                .Where(o =>
                {
                    var ob = o.ObjectRef.ClassID;
#if MAX2022
                    return ob.PartA.Equals(RHClassID.cidVRayProxy.PartA) ||
                           ob.PartA.Equals(RHClassID.cidCoronaProxy.PartA);
#else
                    return ob.OperatorEquals(RHClassID.cidVRayProxy) ||
                           ob.OperatorEquals(RHClassID.cidCoronaProxy);
#endif
                }).ToList();

            foreach (var obj in objs)
            {
                var dobj = obj.ObjectRef as IIDerivedObject;
                if (dobj != null)
                {
                    for (int i = dobj.NumModifiers - 1; i >= 0; i--)
                    {
                        //Debug.Print(i.ToString());
                        dobj.DeleteModifier(i);
                    }
                }
            }
        }

        // 清理面数为0的物体        
        public static void CleanEmptyFaceObjects()
        {
            var objs = GetSceneNodes().Where(
                o =>
                o.ObjectRef.FindBaseObject().SuperClassID == SClass_ID.Geomobject
            );
            var time = RedHaloCore.Core.Time;
            var objList = new List<IINode>();

            IINodeTab iNodeTab = RedHaloCore.Global.INodeTab.Create();

            foreach (var obj in objs)
            {
                IObject ob = obj.EvalWorldState(time, true).Obj;
                int faceNum = 0;
                int vertNum = 0;
                RedHaloCore.Global.GetPolygonCount(time, ob, ref faceNum, ref vertNum);

                if (faceNum == 0)
                {
                    iNodeTab.AppendNode(obj, false, 0);
                }
            }

            RedHaloCore.Core.DeleteNodes(iNodeTab, true, true, true);
        }

        public static IBitmap CreateBitmap(ITexmap tex, ushort width, ushort height, string filename)
        {
            IBitmapInfo bi = RedHaloCore.Global.BitmapInfo.Create();
            bi.SetWidth(width);
            bi.SetHeight(height);
            bi.SetFlags(1 << 1);
            bi.SetType(6);
            bi.SetName(filename);

            IBitmap bitmap = RedHaloCore.Global.TheManager.Create(bi);
            RedHaloCore.Core.RenderTexmap(tex, bitmap, 1.0f, false, false, 1.0f, 0, false);

            bitmap.OpenOutput(bi);
            bitmap.Write(bi, -2000000);
            bitmap.Close(bi, 0);

            return bitmap;
        }

        public static T RemoveUnsupportTexmap<T>(ITexmap texmap)
        {
            Type type = typeof(T);
            var texName = texmap.ClassName(false);
            GetParams(texmap);

            switch (texName)
            {
                case "BlendedBoxMap":

                    break;
                case "Checker":
                    /*
                    0-0,	Float,	    soften
                    0-1,	Rgba,	    color1
                    0-2,	Rgba,	    color2
                    0-3,	Texmap,	    map1
                    0-4,	Texmap,	    map2
                    0-5,	Bool2,	    map1Enabled
                    0-6,	Bool2,	    map2Enabled
                    0-7,	Reftarg,	coords
                    */
                    Debug.Print("++++ CHECKER ++++");


                    break;
                case "Color Correction":

                    break;
                case "Color Map":
                    Debug.Print("++++ Color Map ++++");

                    break;
                case "Composite":

                    break;
                case "Dent":
                    /*
                    0-0,	Texmap,	    map1
                    0-1,	Texmap,	    map2
                    0-2,	Rgba,	    color1
                    0-3,	Rgba,	    color2
                    0-4,	Bool2,	    map1Enabled
                    0-5,	Bool2,	    map2Enabled
                    0-6,	Float,	    size
                    0-7,	Float,	    strength
                    0-8,	Int,	    iterations
                    0-9,	Reftarg,	coords
                    */

                    //GetParams(texmap);
                    var map1Enabled = GetValeByID<int>(texmap, 0, 4);
                    var map2Enabled = GetValeByID<int>(texmap, 0, 5);

                    // Map1
                    var dentMap1 = GetValeByID<ITexmap>(texmap, 0, 0);
                    var dentMap2 = GetValeByID<ITexmap>(texmap, 0, 1);

                    if (dentMap1 != null && map1Enabled == 1)
                    {
                        return (T)(Object)dentMap1;
                    }
                    break;
                case "Falloff":

                    break;
                case "Gradient":

                    break;
                case "Gradient Ramp":

                    break;
                case "Map Output Selector":
                    /*
                    0-0,	Texmap,	sourceMap
                    0-1,	Int,	outputChannelIndex
                    */

                    break;
                case "Mask":
                    /*
                    0-0,	Texmap,	map
                    0-1,	Texmap,	mask
                    0-2,	Bool2,	mapEnabled
                    0-3,	Bool2,	maskEnabled
                    0-4,	Bool2,	maskInverted
                     */

                    break;
                case "Mix":
                    /*
                    0-0,	PcntFrac,	mixAmount
                    0-1,	Float,	    lower
                    0-2,	Float,	    upper
                    0-3,	Bool2,	    useCurve
                    0-4,	Rgba,	    color1
                    0-5,	Rgba,	    color2
                    0-6,	Texmap,	    map1
                    0-7,	Texmap,	    map2
                    0-8,	Texmap,	    mask
                    0-9,	Bool2,	    map1Enabled
                    0-10,	Bool2,	    map2Enabled
                    0-11,	Bool2,	    maskEnabled
                    0-12,	Reftarg,	output 
                    */
                    Debug.Print("++++ MIX ++++");
                    //GetParams(texmap);

                    break;
                case "MultiTile":

                    break;
                case "Noise":

                    break;
                case "Output":
                    /*
                    0-0,	Texmap,	map1
                    0-1,	Bool2,	map1Enabled
                    0-2,	Reftarg,	output
                     */

                    break;
                case "RGB Multiply":
                    /*
                    0-0,	Rgba,	color1
                    0-1,	Rgba,	color2
                    0-2,	Texmap,	map1
                    0-3,	Texmap,	map2
                    0-4,	Bool2,	map1Enabled
                    0-5,	Bool2,	map2Enabled
                    0-6,	Int,	alphaFrom
                     */

                    break;
                case "RGB Tint":
                    /*
                    0-0,	Rgba,	red
                    0-1,	Rgba,	green
                    0-2,	Rgba,	blue
                    0-3,	Texmap,	map1
                    0-4,	Bool2,	map1Enabled
                     */

                    break;
                case "Smoke":

                    break;
                case "Speckle":
                    /*
                    0 - 0,	Float,	    size
                    0 - 1,	Rgba,	    color1
                    0 - 2,	Rgba,	    color2
                    0 - 3,	Texmap,	    map1
                    0 - 4,	Texmap,	    map2
                    0 - 5,	Bool2,	    map1On
                    0 - 6,	Bool2,	    map2On
                    0 - 7,	Reftarg,	coords
                    */
                    break;
                case "Splat":
                    /*
                    0-0,	Float,	    size
                    0-1,	Int,	    iterations
                    0-2,	Float,	    threshold
                    0-3,	Rgba,	    color1
                    0-4,	Rgba,	    color2
                    0-5,	Texmap,	    map1
                    0-6,	Texmap,	    map2
                    0-7,	Bool2,	    map1On
                    0-8,	Bool2,	    map2On
                    0-9,	Reftarg,	coords 
                    */

                    break;
                case "Stucco":

                    break;
                case "Tiles":

                    break;
                case "Vector Displacement":

                    break;
                case "Vertex Color":

                    break;
                case "Waves":

                    break;

                #region VRAY Textures

                case "VRayBitmap":

                    break;
                case "VRayBump2Normal":

                    break;
                case "VRayColor":

                    break;
                case "VRayColor2Bump":

                    break;
                case "VRayCompTex":

                    break;
                case "VRayDirt":

                    break;
                case "VRayEdgesTex":

                    break;
                case "VRayMultiSubTex":

                    break;
                case "VRayNormalMap":

                    break;
                case "VRayOCIO":

                    break;
                case "VRaySoftbox":

                    break;
                case "VRayTriplanarTex":

                    break;
                case "VRayUserColor":

                    break;
                case "VRayUserScalar":

                    break;
                case "VRayUVWRandomizer":

                    break;
                #endregion

                #region Corona Textures
                case "CoronaAO":
                    break;
                case "CoronaBitmap":
                    break;
                case "CoronaBumpConverter":
                    break;
                case "CoronaColor":
                    break;
                case "CoronaColorCorrect":
                    break;
                case "CoronaCurvature":
                    break;
                case "CoronaDistance":
                    break;
                case "CoronaEdgeMap":
                    break;
                case "CoronaFrontBack":
                    break;
                case "CoronaMappingRandomizer":
                    break;
                case "CoronaMix":
                    break;
                case "CoronaMultiMap":
                    break;
                case "CoronaNormal":
                    break;
                case "CoronaRaySwitch":
                    break;
                case "CoronaRoundEdges":
                    break;
                case "CoronaSelect":
                    break;
                case "CoronaTileMap":
                    break;
                case "CoronaTonemapControl":
                    break;
                case "CoronaTriplanar":
                    break;
                case "CoronaWire":
                    break;
                #endregion

                default:
                    break;
            }

            return default;
        }

        public static void CleanupMtl(IMtl mtl)
        {
            var mtlType = mtl.ClassName(false);
            Debug.Print(mtlType);
            //GetParams(mtl);

            switch (mtlType)
            {
                case "VRayBlendMtl":

                    break;
                case "VRay2SidedMtl":
                    /*
                    0-0,	Mtl,	frontMtl
                    0-1,	Mtl,	backMtl
                    0-2,	Float,	
                    0-3,	Rgba,	translucency
                    0-4,	Bool2,	backMtlOn
                    0-5,	Texmap,	texmap_translucency
                    0-6,	Float,	texmap_translucency_multiplier
                    0-7,	Bool2,	texmap_translucency_on
                    0-8,	Bool2,	force1SidedSubMtls
                    0-9,	Bool2,	mult_by_front_diffuse 
                    */
                    var frontMtl = GetValeByID<IMtl>(mtl, 0, 0);

                    break;
                case "VRayOverrideMtl":

                    break;
                case "VrayMtlWrapper":

                    break;
                case "DoubleSided":

                    break;
                case "CoronaRaySwitchMtl":

                    break;
                case "CoronaLayeredMtl":

                    break;
                default:
                    break;
            }
        }

        public static void Test()
        {
            var mats = MaterialUtils.GetSceneMaterials().ToList();
            var mtl = mats.Where(ob => ob.Name == "Mat3d66_15228128_1_16716").First();
            //CleanupMtl(mtl);

            // Export Normal material
            //var mtlpbr = Exporter.ExportMaterial(mtl, 0);

            // Export Light materials
            var mtlpbr = Exporter.ExportLightMaterial(mtl, 0);

            // Mutil-Materials
            //var subMaterial = null;

            string tempOutDirectory = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "RH_M2B_TEMP");
            string outputFileName = Path.Combine(tempOutDirectory, "RHM2B_MATERIAL1.json");

            string json = JsonConvert.SerializeObject(mtlpbr, Formatting.Indented);

            // 使用 JsonTextWriter 将 JSON 写入文件
            using (StreamWriter file = File.CreateText(outputFileName))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(writer, mtlpbr);                
            }
        }

        public static void ExportMtl()
        {
            var b = MaterialUtils.GetSceneMaterials();

            string tempOutDirectory = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "RH_M2B_TEMP");
            string outputFileName = Path.Combine(tempOutDirectory, "RHM2B_MATERIAL.xml");

            RedHaloScene redhaloScene = new(tempOutDirectory);

            var index = 0;
            foreach (var material in b)
            {
                if (material.ClassName(false) != "Multi/Sub-Object")
                {
                    //Debug.Print(material.Name);
                    var redHaloPBRMtl = Exporter.ExportMaterial(material, index);

                    redhaloScene.Materials.Add(redHaloPBRMtl);
                }
                index++;
            }

            #region WRITE XML FILE
            var writeSucess = RedHaloTools.WriteFile<RedHaloScene>(redhaloScene, outputFileName);
            #endregion
        }

        // 塌陷所有物体
        public static void CollapseObject()
        {
            IINodeTab tabs = RedHaloCore.Global.INodeTab.Create();

            foreach (var item in GetSceneNodes())
            {
                RedHaloCore.Global.IInstanceMgr.InstanceMgr.GetInstances(item, tabs);
                IINode firstTab = tabs[0];
                bool shouldCollapse = false;
                if (firstTab.ObjectRef is IIDerivedObject obj && obj.NumModifiers > 0)
                {
                    bool hasModifier = obj.Modifiers.Any(m => m.ClassName(false) == "Skin");

                    if (!hasModifier)
                    {
                        shouldCollapse = true;
                    }
                }

                if (firstTab.ObjectRef is IIBoolObject)
                {
                    shouldCollapse = true;
                }

                if (shouldCollapse)
                {
                    RedHaloCore.Core.CollapseNodeTo(firstTab, 0, true);
                }
            }
        }

        // 炸开所有组，但是如果这个组有关键帧的话，会导致关键帧丢失，
        public static void ExplodeGroup()
        {
            var root = GetSceneNodes().Where(g => g.IsGroupHead);
            IINodeTab iTab = RedHaloCore.Global.INodeTab.Create();
            foreach (var node in root)
            {
                iTab.InsertNode(node, -1, false);
            }
            RedHaloCore.Core.ExplodeNodes(iTab);
        }

        // IColor to string
        public static string IColorToString(IColor color, bool useFourNum = false)
        {
            if (useFourNum)
            {
                return $"{color.R},{color.G},{color.B},1";
            }

            return $"{color.R},{color.G},{color.B}";
        }

        public static string IColorToString(IAColor color)
        {
            return $"{color.R},{color.G},{color.B},{color.A}";
        }

        // Write Log
        public static void WriteLog(string log)
        {
            string logFilename = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "RHM2B_log.log");
            using (StreamWriter w = File.AppendText(logFilename))
            {
                w.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd")} {DateTime.Now.TimeOfDay}]: {log}");
            }
        }

        // Get Path
        public static string GetActualPath(string OriginalPath)
        {
            IIFileResolutionManager fileResolutionManager = RedHaloCore.Global.IFileResolutionManager.Instance;
            IPath Path = RedHaloCore.Global.MaxSDK.Util.Path.Create(OriginalPath);

            if (fileResolutionManager.GetFullFilePath(Path, Autodesk.Max.MaxSDK.AssetManagement.AssetType.BitmapAsset, true))
            {
                return Path.CStr;
            }
            else if (fileResolutionManager.GetFullFilePath(Path, Autodesk.Max.MaxSDK.AssetManagement.AssetType.OtherAsset, true))
            {
                return Path.CStr;
            }
            else if (fileResolutionManager.GetFullFilePath(Path, Autodesk.Max.MaxSDK.AssetManagement.AssetType.PhotometricAsset, true))
            {
                return Path.CStr;
            }
            else if (fileResolutionManager.GetFullFilePath(Path, Autodesk.Max.MaxSDK.AssetManagement.AssetType.XRefAsset, true))
            {
                return Path.CStr;
            }
            else
            {
                return Path.CStr;
            }
        }
    }
}