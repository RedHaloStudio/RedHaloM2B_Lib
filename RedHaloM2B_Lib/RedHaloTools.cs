using Autodesk.Max;
using Autodesk.Max.MaxSDK.Util;
using Newtonsoft.Json;
using RedHaloM2B.RedHaloUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media.Media3D;
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace RedHaloM2B
{
    public class RedHaloTools
    {
        // 取得CPU的核心数量，进行多线程计算
        public static int coreNumber = Environment.ProcessorCount;

        // Textures HashSet
        public static Dictionary<string, string> texturesHash = new Dictionary<string, string>();

        // 不支持的中文路径的文件后缀
        public HashSet<string> unsupportChsFiles = new HashSet<string>
        {
            "exr"
        };

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

        public struct VersionNumber
        {
            public int Major;
            public int Minor;
            public int Revision;
            public int BuildNumber;
        }

        public static VersionNumber GetMaxVersion()
        {
            // https://getcoreinterface.typepad.com/blog/2017/02/querying-the-3ds-max-version.html
#if MAX2022 || MAX2023 || MAX2024 || MAX2025 || MAX2026
            var versionString = ManagedServices.MaxscriptSDK.ExecuteStringMaxscriptQuery("getFileVersion \"$max/3dsmax.exe\"", ManagedServices.MaxscriptSDK.ScriptSource.NotSpecified);
#else
            var versionString = ManagedServices.MaxscriptSDK.ExecuteStringMaxscriptQuery("getFileVersion \"$max/3dsmax.exe\"");
#endif
            var versionSplit = versionString.Split(',');
            int major, minor, revision, buildNumber = 0;
            int.TryParse(versionSplit[0], out major);
            int.TryParse(versionSplit[1], out minor);
            int.TryParse(versionSplit[2], out revision);
            int.TryParse(versionSplit[3], out buildNumber);
            return new VersionNumber { Major = major, Minor = minor, Revision = revision, BuildNumber = buildNumber };
        }

        /// <summary>
        /// Calc MD5 From FileSteam
        /// </summary>
        /// <param name="path">file path</param>
        /// <param name="bufferSize">file stream buffer size</param>
        /// <returns></returns>
        public static string CalcMD5FromFile(string path, int bufferSize = 4096)
        {
            if (File.Exists(path))
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {                    
                    var md5 = MD5.Create();
                    byte[] buffer = new byte[bufferSize];
                    int bytesRead;
                    while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        md5.TransformBlock(buffer, 0, bytesRead, null, 0);
                    }
                    md5.TransformFinalBlock(new byte[0], 0, 0); // 完成计算
                    return BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
                }
            }
            else
            {
                return string.Empty;
            }
        }

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

        public static int AddModifier(uint nodeHandle, IClass_ID mod_id)
        {
            try
            {
                IINode node = RedHaloCore.Core.GetINodeByHandle(nodeHandle);

                IObject obj = node.ObjectRef;
                IIDerivedObject dobj = RedHaloCore.Global.CreateDerivedObject(obj);
                object objMod = RedHaloCore.Core.CreateInstance(SClass_ID.Osm, mod_id as IClass_ID);
                IModifier mod = (IModifier)objMod;

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
        // Calc MD5 From FileSteam
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
            temperature = RedHaloMathUtils.Clamp(temperature, 1000, 40000);

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
                red = RedHaloMathUtils.Clamp(red, 0, 255);
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

            green = RedHaloMathUtils.Clamp(green, 0, 255);

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
                blue = RedHaloMathUtils.Clamp(blue, 0, 255);
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

        //Write JSON file
        public static bool WriteJsonFile<T>(T classType, string FileOutPath)
        {
            try
            {
                using (StreamWriter file = File.CreateText(FileOutPath))
                using (JsonTextWriter writer = new JsonTextWriter(file))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    //serializer.NullValueHandling = NullValueHandling.Ignore;
                    serializer.Serialize(writer, classType);
                }
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
        /// <param name="node"></param>
        public static void GetParams(ICameraObject node)
        {
            for (int p = 0; p < node.NumParamBlocks; p++)
            {
                var pb2 = node.GetParamBlock(p);
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
        public static T GetValueByID<T>(IMtl mtl, short paramBlockID, int paramID)
        {
            Type type = typeof(T);

            var pb2 = mtl.GetParamBlock(paramBlockID);

            if (type == typeof(float))
            {
                var result = 0.0f;
                pb2.GetValue(pb2.IndextoID(paramID), 0, ref result, RedHaloCore.Forever, 0);
                return (T)(Object)result;
            }
            else if (type == typeof(IColor))
            {
                var result = RedHaloCore.Global.Color.Create(0, 0, 0);
                pb2.GetValue(pb2.IndextoID(paramID), 0, result, RedHaloCore.Forever, 0);

                return (T)(Object)result;
            }
            else if (type == typeof(int))
            {
                var result = 0;
                pb2.GetValue(pb2.IndextoID(paramID), 0, ref result, RedHaloCore.Forever, 0);
                return (T)(Object)result;
            }
            else if (type == typeof(ITexmap))
            {
                ITexmap result = pb2.GetTexmap(pb2.IndextoID(paramID), 0, RedHaloCore.Forever, 0);
                return (T)(Object)result;
            }
            else if (type == typeof(IMtl))
            {
                var result = pb2.GetMtl(pb2.IndextoID(paramID), 0, 0);
                return (T)(Object)result;
            }

            return default(T);
        }

        public static T GetValueByID<T>(ITexmap tex, short paramBlockID, int paramID)
        {
            Type type = typeof(T);

            var pb2 = tex.GetParamBlock(paramBlockID);

            if (type == typeof(float))
            {
                var result = 0.0f;
                pb2.GetValue(pb2.IndextoID(paramID), 0, ref result, RedHaloCore.Forever, 0);
                return (T)(Object)result;
            }
            else if (type == typeof(IColor))
            {
                var result = RedHaloCore.Global.Color.Create(0, 0, 0);
                pb2.GetValue(pb2.IndextoID(paramID), 0, result, RedHaloCore.Forever, 0);
                return (T)(Object)result;
            }
            else if (type == typeof(IAColor))
            {
                var result = RedHaloCore.Global.AColor.Create(0, 0, 0, 1);
                pb2.GetValue(pb2.IndextoID(paramID), 0, result, RedHaloCore.Forever, 0);
                return (T)(Object)result;
            }
            else if (type == typeof(int))
            {
                var result = 0;
                pb2.GetValue(pb2.IndextoID(paramID), 0, ref result, RedHaloCore.Forever, 0);
                return (T)(Object)result;
            }
            else if (type == typeof(ITexmap))
            {
                ITexmap result = pb2.GetTexmap(pb2.IndextoID(paramID), 0, RedHaloCore.Forever, 0);
                return (T)(Object)result;
            }
            else if (type == typeof(IPBBitmap))
            {
                IPBBitmap result = pb2.GetBitmap(pb2.IndextoID(paramID), 0, RedHaloCore.Forever, 0);
                return (T)(Object)result;
            }
            else if (type == typeof(string))
            {
                var result = pb2.GetStr(pb2.IndextoID(paramID), 0, RedHaloCore.Forever, 0);
                return (T)(Object)result;
            }
            else if (type == typeof(IReferenceTarget))
            {
                var result = pb2.GetReferenceTarget(pb2.IndextoID(paramID), 0, RedHaloCore.Forever, 0);
                return (T)(Object)result;
            }
            else if (type == typeof(IIParamBlock2))
            {
                var result = pb2.GetParamBlock2(pb2.IndextoID(paramID), 0, RedHaloCore.Forever, 0);
                return (T)(Object)result;
            }
            else if (type == typeof(IPoint3))
            {
                var result = pb2.GetPoint3(pb2.IndextoID(paramID), 0, RedHaloCore.Forever, 0);
                return (T)(Object)result;
            }
            else if (type == typeof(string))
            {
                var result = pb2.GetStr(pb2.IndextoID(paramID), 0, RedHaloCore.Forever, 0);
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

        // Rescale whole scene
        public static void RescaleScene(float fac)
        {
            if (fac != 1f)
            {

#if MAX2022 || MAX2023 || MAX2024
                RedHaloCore.Core.RescaleWorldUnits(fac, false);
#elif MAX2025 || MAX2026
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
                    return ob.OperatorEquals(RedHaloClassID.VRayPlane) ||
                           ob.OperatorEquals(RedHaloClassID.ChaosScatter) ||
                           ob.OperatorEquals(RedHaloClassID.LinkComposite);
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
                    return ob.OperatorEquals(RedHaloClassID.VRayProxy) ||
                           ob.OperatorEquals(RedHaloClassID.CoronaProxy);
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

        // 清理面数为0的物体 [暂时使用maxscript实现]
        public static void CleanEmptyFaceObjects()
        {
            var ms = scripts.mxs_delete_zero_face;
            ScriptsUtilities.ExecuteMaxScriptCommand(ms);
        }

        // 清理不支持的材质 [暂时使用maxscript实现]
        public static void CleanupMaterials()
        {
            var ms = scripts.mxs;
            ScriptsUtilities.ExecuteMaxScriptCommand(ms);
        }

        /// <summary>
        /// 渲染裁切的图片，并返回新图片。<br/>
        /// 暂时解决不了TheManager.Create(bitmapInfo)问题，就用Maxscript暂时实现
        /// </summary>
        /// <param name="inBitmap">IPBBitmap</param>
        /// <param name="clipu">裁切U</param>
        /// <param name="clipv">裁切V</param>
        /// <param name="clipw">裁切宽度</param>
        /// <param name="cliph">裁切高度</param>
        /// <returns></returns>
        public static IBitmapTex RenderAndCropImage(IPBBitmap inBitmap, float clipu, float clipv, float clipw, float cliph)
        {
            // 检查输入的IPBBitmap是否有效
            if (inBitmap == null || inBitmap.Bi == null)
            {
                Debug.Print("Invalid bitmap input.");
                return null;
            }
            // 获取图片的真实路径
            string filename = RedHaloTools.GetActualPath(inBitmap.Bi.Name);

            // 检查文件名是否为空或文件不存在
            if (string.IsNullOrEmpty(filename) ||
                !File.Exists(filename))
                return null;

            // 获取filename的路径和后缀名
            string dirPath = Path.GetDirectoryName(filename); //返回文件所在目录 "d:\test"
            string extension = Path.GetExtension(filename); //扩展名 ".jpg"
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename); // 没有扩展名的文件名 "default"

            string new_prex = Guid.NewGuid().ToString("N").Substring(8, 6);
            // 生成新的文件名
            string newfilename = Path.Combine(dirPath, $"{fileNameWithoutExtension}-{new_prex}{extension}");

            // 获取图片的宽度和高度
            int width = (int)(inBitmap.Bi.Width * clipw);
            int height = (int)(inBitmap.Bi.Height * cliph);

            string script = "bm = undefined\n";
            script += $"bm = Bitmaptexture filename:\"{filename}\" name:\"name_crop\" apply:on clipu:{clipu} clipv:{clipv} clipw:{clipw} cliph:{cliph}\n";
            script += $"new_bm = renderMap bm size:[{width}, {height}] filename:\"{newfilename}\"\n";
            script += @"save new_bm
                       close new_bm
                    ";

            IFPValue result = RedHaloCore.Global.FPValue.Create();
            bool runok = RedHaloCore.Global.ExecuteMAXScriptScript(script, Autodesk.Max.MAXScript.ScriptSource.Embedded, true, result, true);

            if (runok)
            {
                // Load new bitmap
                IBitmapTex newBitmap = RedHaloCore.Global.NewDefaultBitmapTex;
                newBitmap.SetMapName(newfilename, true);

                return newBitmap;
            }

            return null;
        }

        /// <summary>
        /// Cleans up unsupported textures in the specified material by processing its sub-textures.
        /// </summary>
        /// <remarks>This method iterates through all sub-textures of the provided material and processes
        /// each one to ensure compatibility. If a sub-texture is <see langword="null"/>, it is skipped. Scene redraw is
        /// temporarily disabled during the operation to improve performance and is re-enabled after processing is
        /// complete.</remarks>
        /// <param name="currentMtl">The material whose sub-textures will be processed. If <paramref name="currentMtl"/> is <see
        /// langword="null"/>, the method exits without performing any operations.</param>
        public static void CleanupMaterial(IMtl currentMtl)
        {
            RedHaloCore.Core.DisableSceneRedraw();
            if (currentMtl == null)
            {
                return;
            }

            // 遍历当前材质的所有子纹理
            for (int i = 0; i < currentMtl.NumSubTexmaps; i++)
            {
                ITexmap subTexmap = currentMtl.GetSubTexmap(i);
                if (subTexmap == null)
                {
                    continue; // 如果子纹理为空，跳过
                }

                var tex = ProcessTextures(subTexmap, currentMtl, i);
                currentMtl.SetSubTexmap(i, tex);

            }
            RedHaloCore.Core.EnableSceneRedraw();
        }

        public static ITexmap ProcessTextures(ITexmap currentTexmap, IReferenceMaker parentRef, int subIndex)
        {
            if (currentTexmap == null)
            {
                return null; // 如果当前纹理为空，直接返回
            }

            var texmapTypeName = currentTexmap.ClassName(false);
            //Debug.Print($"Processing Texmap: {texmapTypeName}, Name : {currentTexmap.Name}");

            ITexmap effectiveTexmap = null;
            ITexmap tempTexmap = null;
            switch (texmapTypeName)
            {
                case "Bitmap":
                    // 处理 Bitmap 类型的纹理
                    effectiveTexmap = currentTexmap;  
                    var bitmap = GetValueByID<IPBBitmap>(currentTexmap, 0, 13); // 获取位图纹理

                    //var newBitmap = RenderAndCropImage(bitmap, 0.25f, 0.25f, 0.5f, 0.5f); // 裁剪位图，参数可以根据需要调整
                    
                    //if (newBitmap == null)
                    //{
                        //Debug.Print("Failed to create new bitmap texture.");
                        //return currentTexmap; // 如果新位图创建失败，返回当前纹理
                    //}

                    // 计算文件的Hash值，用于比较场景中的纹理是否相同
                    // 如果不相同，使用新纹理，否则使用原纹理
                    //var fileHash = CalcMD5FromFile(newBitmap.Name);

                    //textureHash.Contain(fileHash)

                    break;
                
                case "VRayBitmap":
                    // 处理 Bitmap 类型的纹理
                    effectiveTexmap = currentTexmap;

                    break;
                
                case "CoronaBitmap":
                    // 处理 Bitmap 类型的纹理
                    effectiveTexmap = currentTexmap;
                    
                    break;
                
                case "Map Output Selector": // Advanced Wood Textures
                case "BlendedBoxMap":
                case "Camera_Map_Per_Pixel":
                case "Cellular":
                case "Dent":
                case "Output":
                case "RGB Tint":

                case "VRayMultiSubTex":
                case "VRayTriplanarTex":
                case "VRayUserColor":
                case "VRayUserScalar":

                case "CoronaEdgeMap":
                case "CoronaMultiMap":
                case "CoronaRaySwitch": // Corona Ray Switch Texture
                case "CoronaTriplanar":
                case "CoronaTonemapControl":
                    effectiveTexmap = ProcessTextures(currentTexmap.GetSubTexmap(0), currentTexmap, 0);
                    break;

                case "Checker":
                case "ColorCorrection":
                case "Falloff":
                case "Color Correction":
                case "Color Map":
                case "Composite":
                case "Mix":
                case "Mask":
                case "MultiTile":
                case "RGB Multiply":
                case "Tiles": //Brick Textures
                
                case "VRayBump2Normal":
                case "VRayColor":
                case "VRayColor2Bump":
                case "VRayCompTex":
                case "VRayDirt":
                case "VRayNormalMap":

                case "CoronaAO":
                case "CoronaBumpConverter":                
                case "CoronaColorCorrect":
                case "CoronaFrontBack":
                case "CoronaMix":
                case "CoronaNormal":
                //case "CoronaRaySwitch": //暂时只取第一个纹理
                case "CoronaRoundEdges":
                case "CoronaTileMap":
                case "CoronaWire":
                    for (int i = 0; i < currentTexmap.NumSubTexmaps; i++)
                    {
                        tempTexmap = currentTexmap.GetSubTexmap(i); // 获取当前纹理的子纹理
                        tempTexmap = ProcessTextures(tempTexmap, currentTexmap, i); // 递归处理子纹理
                        currentTexmap.SetSubTexmap(i, tempTexmap); // 更新当前纹理的子纹理
                    }
                    effectiveTexmap = currentTexmap; // 返回当前纹理
                    break;

                case "Gradient":
                    break;
                case "Gradient Ramp":
                    break;

                case "Vertex Color":
                case "VRayEdgesTex":
                case "CoronaColor":
                    effectiveTexmap = currentTexmap; // 直接返回当前纹理
                    break;

                case "VRaySky":
                    break;

                case "CoronaSelect":                    
                    var selectIndex = GetValueByID<int>(currentTexmap, 0, 2);
                    var pb = currentTexmap.GetParamBlock(0);
                    tempTexmap = pb.GetTexmap(pb.IndextoID(1), 0, RedHaloCore.Forever, selectIndex);
                    effectiveTexmap = ProcessTextures(tempTexmap, currentTexmap, 0); // 递归处理选中的纹理
                    break;

                default:
                    effectiveTexmap = null; // 对于不支持的纹理类型，设置为 null
                    break;
            }
            return effectiveTexmap; // 返回处理后的纹理
        }

        /// <summary>
        /// 查看插件是否安装
        /// </summary>
        /// <param name="pluginName">插件名字</param>
        /// <param>USD：USD Importer</param>
        /// <returns></returns>
        public static bool IsPluginInstalled(string pluginName)
        {
            // 提前处理输入参数，如果pluginName为null或空，则认为插件未安装
            if (string.IsNullOrEmpty(pluginName))
            {
                return false;
            }

            var dl = RedHaloCore.Core.DllDir;

            // 处理 RedHaloCore.Core.DllDir 可能为 null 的情况
            if (dl == null)
            {
                return false;
            }

            // 循环遍历插件描述
            for (int i = 0; i < dl.Count; i++)
            {
                var dll = dl.GetDllDescription(i);

                // 处理 GetDllDescription 返回 null 或 Description 属性为 null 的情况
                if (dll == null || string.IsNullOrEmpty(dll.Description))
                {
                    continue; // 跳过当前项
                }

                // 使用 IndexOf 和 StringComparison.OrdinalIgnoreCase 进行不区分大小写的查找
                // 这是比 ToLower().Contains() 更高效且推荐的方式
                if (dll.Description.IndexOf(pluginName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // 找到匹配项，立即返回 true
                    return true;
                }
            }

            // 循环结束都没有找到匹配项，返回 false
            return false;
        }

        /// <summary>
        /// 收集场景中所有位图文件的哈希值和路径
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> CollectAllBitmaps()
        {
            string script = @"(
                mapfiles=#()
                fn addmap mapfile = (
	                local mapfileN = mapfile as name
	                local index = finditem mapfiles mapfileN
	                if index == 0 do append mapfiles mapfileN
                )
                enumeratefiles addmap
                mapfiles
            )";

            IFPValue result = RedHaloCore.Global.FPValue.Create();
            RedHaloCore.Global.ExecuteMAXScriptScript(script, Autodesk.Max.MAXScript.ScriptSource.Embedded, true, result, true);            

            Dictionary<string , string> fileSteamHash = new Dictionary<string, string>();

            for (int i = 0; i < result.STab.Count; i++)
            {
                // 计算文件流的哈希值
                var fileHash = CalcMD5FromFile(result.STab[i]);
                
                // 添加到字典中
                if (fileSteamHash.ContainsKey(fileHash))
                {
                    // 如果哈希值已经存在，则跳过
                    continue;
                }

                fileSteamHash.Add(fileHash, result.STab[i]);
            }

            return fileSteamHash;
        }

        public static void Test()
        {
            //var objs = GetSceneNodes();
            //var camera = objs.Where(m => m.Name == "Corona_Camera007").First();

            //// 创建 Max 相机和相机状态对象
            //var maxCamera = camera.ObjectRef.FindBaseObject() as ICameraObject;
            //maxCamera.EvalCameraState(0, RedHaloCore.Forever, RedHaloCore.Global.CameraState.Create());

            //var pb = maxCamera.GetParamBlock(0);
            //Debug.Print($"{pb.NumParams}");

            //GetParams(maxCamera);

            var actionManager = RedHaloCore.Core.ActionManager;
            for (var actionTableIndex = 0; actionTableIndex < actionManager.NumActionTables; ++actionTableIndex)
            {
                var actionTable = actionManager.GetTable(actionTableIndex);
                //Debug.Print($"{actionTable.Name}");
                if (actionTable.Name == "REDHALO STUDIO")
                {
                    Debug.Print($"REDHALO STUDIO ID: {actionTable.Id.PartA} - {actionTable.Id.PartB}");
                    for (int i = 0; i < actionTable.Count; ++i)
                    {
                        Debug.Print($"  {actionTable[i].MenuText} / {actionTable[i].Id.PartA} / {actionTable[i].Id.PartB}");
                    }
                }
            }
        }

        // 塌陷所有物体
        public static void CollapseObject()
        {
            IINodeTab tabs = RedHaloCore.Global.INodeTab.Create();

            foreach (var item in GetSceneNodes().Where(obj => obj.ObjectRef.FindBaseObject().SuperClassID != SClass_ID.Light))
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

        // Write Log
        public static void WriteLog(string log)
        {
            string logFilename = System.IO.Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "RHM2B_log.log");
            using (StreamWriter w = File.AppendText(logFilename))
            {
                w.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd")} {DateTime.Now.TimeOfDay}]: {log}");
            }
        }

        // Get Actual Path
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

        public static bool CopyFiles(string oldfile, string newPath)
        {
            var fileExt = System.IO.Path.GetExtension(oldfile);
            var newFile = System.IO.Path.Combine(newPath, System.IO.Path.GetFileName(oldfile));
            try
            {
                if (newFile != oldfile)
                {
                    File.Copy(oldfile, newFile, true);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Print error log

                Debug.Print(ex.ToString());
                return false;
            }
        }

        // 导出USD文件
        public static bool ExportUSDFile(string export_file)
        {
            string usdOption = "" +
                "usd_options = USDExporter.createOptions()\n" +
                "              usd_options.Meshes = True\n" +
                "              usd_options.Shapes = True\n" +
                "              usd_options.Cameras = False\n" +
                "              usd_options.Materials = True\n" +
                "              usd_options.FileFormat = #Binary\n" +
                "              usd_options.upAxis = #Z\n" +
                "              usd_options.Normals = #asPrimvar\n" +
                "              usd_options.PreserveEdgeOrientation = True\n" +
                "              usd_options.TimeMode = #animationRange\n" +
                "              USDExporter.UIOptions = usd_options";

            ScriptsUtilities.ExecuteMaxScriptCommand(usdOption);

            var export = RedHaloCore.Core.ExportToFile(export_file, true, 0, null);

            return export;
        }

        // 导出FBX文件
        public static bool ExportFBXFile(string export_file)
        {
            string FbxOption = "" +
                "            pluginManager.loadClass FBXEXPORTER\n" +
                "            FBXExporterSetParam \"SmoothingGroups\" false\n" +
                "            FBXExporterSetParam \"NormalsPerPoly\" false\n" +
                "            FBXExporterSetParam \"TangentSpaceExport\" false\n" +
                "            FBXExporterSetParam \"SmoothMeshExport\" true\n" +
                "            FBXExporterSetParam \"Preserveinstances\" true\n" +
                "            FBXExporterSetParam \"SelectionSetExport\" false\n " +
                "            FBXExporterSetParam \"Triangulate\" false\n" +
                "            FBXExporterSetParam \"Animation\" true\n" +
                "            FBXExporterSetParam \"Removesinglekeys\" true\n" +
                "            FBXExporterSetParam \"Cameras\" false\n" +
                "            FBXExporterSetParam \"Lights\" false\n" +
                "            FBXExporterSetParam \"EmbedTextures\" false\r\n" +
                "            FBXExporterSetParam \"AxisConversionMethod\" \"None\"\n" +
                "            FBXExporterSetParam \"UpAxis\" \"Z\"\n " +
                "            FBXExporterSetParam \"ShowWarnings\" false\n" +
                "            FBXExporterSetParam \"GenerateLog\" false\n " +
                "            FBXExporterSetParam \"ASCII\" false\n" +

#if MAX2026 || MAX2025 || MAX2024 || MAX2023 || MAX2022 || MAX2021 || MAX2020

                "            FBXExporterSetParam \"FileVersion\" \"FBX202000\"\n" +
#elif MAX2019
                "            -- 3dsmax 2019\r\n" +
                "            FBXExporterSetParam \"FileVersion\" \"FBX201900\"\n" +
#elif MAX2018
                "            -- 3dsmax 2018\r\n" +
                "            FBXExporterSetParam \"FileVersion\" \"FBX201800\"\n" +
#elif MAX2017
                "            -- 3dsmax 2017\r\n" +
                "            FBXExporterSetParam \"FileVersion\" \"FBX201700\"\n" +
#else
                "            -- other vesions\r\n" +
                "            FBXExporterSetParam \"FileVersion\" \"FBX201600\"\n" +
#endif

                "        )";

            ScriptsUtilities.ExecuteMaxScriptCommand(FbxOption);

            var export = RedHaloCore.Core.ExportToFile(export_file, true, 0, null);

            return export;
        }
    }
}