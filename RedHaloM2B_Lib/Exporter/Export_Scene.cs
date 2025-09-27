using Autodesk.Max;
using RedHaloM2B.Nodes;
using RedHaloM2B.RedHaloUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Media.Media3D;

namespace RedHaloM2B
{
    public class ExportScene
    {
        public static Dictionary<string, string> mapHash { get; } = new Dictionary<string, string>();

        /// <summary>
        /// <c>导出场景</c>
        /// </summary>
        /// <param name="fileFormat">导出模型格式</param>
        /// <param name="explodeGroup">是否炸开组</param>
        /// <param name="convertToPoly">是否转成Polygon</param>
        /// <param name="scaleScene">是否缩放场景</param>
        /// <returns>
        /// <param>0. 正常无问题</param>
        /// <param>1. 文件创建出错</param>
        /// <param>2. 导出文件失败</param>
        /// <param>3. 导出模型失败</param>
        /// <param>4. 导出模型失败</param>
        /// </returns>
        public static int SceneExporter(string fileFormat, bool explodeGroup, bool convertToPoly, bool scaleScene = true)
        {
            #region GLOBAL VAR
            const string baseFileName = "RHM2B_MODEL";
            const string usdExtension = ".usd";
            const string fbxExtension = ".fbx";

            int index = 0;

            // 临时文件（夹）有：
            // RH_M2B_TEMP
            // RH_M2B_TEMP/Textures 不支持中文路径的贴图文件会复制到此目录下
            // %temp%/RHM2B_LOG.log 日志文件
            // RH_M2B_TEMP/RHM2B_CONTENT.xml  （以下的参数：设置、物体、相机、灯光、材质参数）

            string systemTempDirectory = Environment.GetEnvironmentVariable("TEMP");
            string tempOutDirectory = Path.Combine(systemTempDirectory, "RH_M2B_TEMP");
            string outputFileName = Path.Combine(tempOutDirectory, "RHM2B_CONTENT.json");
            string logFilename = Path.Combine(systemTempDirectory, "RHM2B_LOG.log");

            var appSettings = new AppSettings()
            {
                OutputPath = tempOutDirectory,
                LogPath = systemTempDirectory,
            };

            GlobalSettings.OutputPath = appSettings.OutputPath;
            GlobalSettings.LogPath = appSettings.LogPath;
            GlobalSettings.ExportFormat = fileFormat;
            GlobalSettings.SceneScale = (float)RedHaloCore.Global.GetSystemUnitScale(5); //5 Meter

            // 尝试删除log文件
            File.Delete(logFilename);

            //使用HashSet过滤重复物体
            HashSet<string> nodeKeys = new HashSet<string>();

            IINodeTab tabs = RedHaloCore.Global.INodeTab.Create();

            var dirInfo = RedHaloTools.CreateTempFolder(tempOutDirectory);
            if (dirInfo != 0)
            {
                RedHaloTools.WriteLog($"创建临时文件夹失败，检查一下文件写入权限");
                return 1;
            }
            Directory.CreateDirectory(Path.Combine(tempOutDirectory, "Textures"));

            int startFrame = RedHaloCore.Core.AnimRange.Start / RedHaloCore.Global.TicksPerFrame;
            int endFrame = RedHaloCore.Core.AnimRange.End / RedHaloCore.Global.TicksPerFrame;

            RedHaloTools.WriteLog($"文件 ：{RedHaloCore.Core.CurFilePath}");

            #endregion

            RedHaloScene redhaloScene = new RedHaloScene(tempOutDirectory);

            #region 准备工作

            // 删除面数为0的物体
            //RedHaloTools.CleanEmptyFaceObjects();

            // 可渲染样条线转为网格物体
            RedHaloTools.ConvertShapeToPoly();

            // 塌陷所有物体，以防缩放物体时，发生不必要的变形, 蒙皮修改器也会消失
            RedHaloTools.CollapseObject();

            // 清理自定义属性，blender不支持fbx的自定义属性
            RedHaloTools.CleanUserProperty();

            // 清理不支持的物体
            RedHaloTools.CleanUnsupportObjects();

            // 删除代理物体上的修改器，避免导出时出现错误
            RedHaloTools.removeProxyModifier();

            // 是否解组
            if (explodeGroup)
            {
                RedHaloTools.ExplodeGroup();
            }
            /*
            // 图片的Hash值，字典
            Dictionary<string, string> texHash = RedHaloTools.CollectAllBitmaps();

            foreach (var item in texHash)
            {
                if (mapHash.ContainsKey(item.Key))
                {
                    continue;
                }

                mapHash.Add(item.Key, item.Value);
            }
            */

            // 获取场景中所有的贴图

            #endregion

            #region PRODUCTOR
            redhaloScene.Productor = new RedHaloProductor
            {
                Host = "3ds Max",
                Version = RedHaloCore.Global.GetMaxVerFromMxsMaxFileVer(EMxsMaxFileVersion.Current).ToString(),
                ExportVersion = "RedHaloExport 1.0",
                File = RedHaloCore.Core.CurFilePath,
                Renderer = RedHaloCore.Core.GetCurrentRenderer(false).ClassName(false),
                BuildTime = DateTime.Now.ToString(),
            };
            #endregion

            #region SETTINGS

            redhaloScene.Settings = new RedHaloSettings
            {
                WorldUnit = GlobalSettings.SceneScale, //(float)RedHaloCore.Global.GetSystemUnitScale(5), //5 Meter
                ImageWidth = RedHaloCore.Core.RendWidth,
                ImageHeight = RedHaloCore.Core.RendHeight,
                ImagePixelAspect = RedHaloCore.Core.ImageAspRatio,
                AnimateStart = startFrame,
                AnimateEnd = endFrame,
                FrameRate = 4800 / RedHaloCore.Global.TicksPerFrame, // There are always 4800 ticks per second, this means that ticksPerFrame is dependent on the frames per second rate (ticksPerFrame * frameRate == 4800)

#if   MAX2022 || MAX2023
                Gamma = RedHaloCore.Global.GammaMgr.DispGamma,
#elif MAX2024 || MAX2025 || MAX2026
                Gamma = RedHaloCore.Global.GammaMgr.DisplayGamma,
#else
                Gamma = 1.0f,
#endif

                LinearWorkflow = 0,
                ExportFormat = fileFormat,
                OutputPath = tempOutDirectory,
            };

            // 设置日志文件
            #endregion

            // 缩放整个场景（以米为基础单位），匹配Blender单位尺寸
            if (scaleScene)
            {
                RedHaloTools.RescaleScene(redhaloScene.Settings.WorldUnit);
            }
            
            RedHaloTools.WriteLog($"场景缩放比例 ：{redhaloScene.Settings.WorldUnit}");

            var sceneNodes = RedHaloTools.GetSceneNodes();

            #region EXPORT NODES

            IEnumerable<IINode> staticMesh = sceneNodes.Where(o =>
            {
                var ob = o.ObjectRef.FindBaseObject();
                return ob.SuperClassID == SClass_ID.Geomobject ||
                ob.SuperClassID == SClass_ID.Shape ||
                ob.SuperClassID == SClass_ID.Helper;
            });

            foreach (var node in staticMesh)
            {
                if (nodeKeys.Contains(node.Name))
                {
                    continue;
                }
                else
                {
                    RedHaloCore.Global.IInstanceMgr.InstanceMgr.GetInstances(node, tabs);

                    // 断开所有物体的关联属性
                    if (tabs.Count > 1)
                    {
                        // 修改代理物体的显示模式
                        RedHaloTools.ChangeProxyDisplay(tabs[0], RedHaloTools.ProxyMode.BBOX);
                        RedHaloCore.Global.IInstanceMgr.InstanceMgr.MakeObjectsUnique(tabs, 1);
                    }

                    string baseobj = $"mesh_{index:D5}"; //tabs[0].Name;

                    for (int i = 0; i < tabs.Count; i++)
                    {
                        var nd = tabs[i];

                        try
                        {
                            RedHaloGeometry redHaloGeometry = Exporter.ExportGeometry(nd, index);
                            redHaloGeometry.BaseObject = baseobj;
                            redhaloScene.GeometryList.Add(redHaloGeometry);
                        }
                        catch (Exception)
                        {
                            throw;
                        }

                        nodeKeys.Add(nd.Name);
                        index++;
                    }
                    // 修改代理物体的显示模式为完整模式，方便转换
                    RedHaloTools.ChangeProxyDisplay(tabs[0], RedHaloTools.ProxyMode.FULL);
                }
            }
            
            RedHaloTools.WriteLog($"场景中共有 {staticMesh.Count()} 个物体，导出 {staticMesh.Count()} 个");
            // 重置Index
            index = 0;
            nodeKeys.Clear();
            #endregion

            #region EXPORT CAMERAS
            // 场景中所有相机物体
            var cameraNodes = sceneNodes.Where(obj => obj.ObjectRef.FindBaseObject().SuperClassID == SClass_ID.Camera);
            RedHaloCamera redHaloCamera = new();

            foreach (var cam in cameraNodes)
            {
                redHaloCamera = Exporter.ExportCamera(cam, index);
                redhaloScene.Cameras.Add(redHaloCamera);
                index++;
            }

            index = 0;
            nodeKeys.Clear();

            RedHaloTools.WriteLog($"共导出 {cameraNodes.Count()} 个相机");

            #endregion

            #region EXPORT LIGHTS

            // 场景中所有灯光物体
            var lightNodes = sceneNodes.Where(obj => obj.ObjectRef.FindBaseObject().SuperClassID == SClass_ID.Light);

            foreach (var light in lightNodes)
            {
                if (nodeKeys.Contains(light.Name))
                {
                    continue;
                }
                else
                {
                    RedHaloCore.Global.IInstanceMgr.InstanceMgr.GetInstances(light, tabs);

                    string BaseObjectName = $"light_{index:D5}";

                    for (int i = 0; i < tabs.Count; i++)
                    {
                        try
                        {
                            RedHaloLight redhaloLight = Exporter.ExportLights(tabs[i], index);
                            redhaloLight.BaseObject = BaseObjectName;
                            redhaloScene.LightsList.Add(redhaloLight);
                        }
                        catch (Exception e)
                        {
                            Debug.Print(e.ToString());
                        }

                        nodeKeys.Add(tabs[i].Name);
                        index++;
                    }
                }
            }
            index = 0;
            nodeKeys.Clear();

            RedHaloTools.WriteLog($"共导出 {lightNodes.Count()} 个灯光");

            #endregion

            #region EXPORT MATERIALS

            RedHaloTools.WriteLog($"=========导出材质开始=========");
            // 清理场景中不支持的材质
            RedHaloTools.CleanupMaterials();
            
            var materials = MaterialUtils.GetSceneMaterials().ToList();

            var basePbrMaterialType = new HashSet<string> {
                   "VRayMtl", "CoronaPhysicalMtl","\rCoronaPhysicalMtl", "CoronaLegacyMtl", "Standard (Legacy)", "StandardMaterial", "VRayCarPaintMtl", "VRayCarPaintMtl2"
            };

            var lightMaterialType = new HashSet<string> {
                   "VRayLightMtl", "CoronaLightMtl"
            };
            
            var multiMaterialType = new HashSet<string> {
                   "VRayBlendMtl", "CoronaLayerMtl", "Blend", "VRay2SidedMtl", "VRayOverrideMtl", "CoronaRaySwitchMtl", "Double Sided"
            };

            // 普通材质
            List<IMtl> singleMaterials = materials.Where(m => basePbrMaterialType.Contains(m.ClassName(false))).ToList();
            materials.RemoveAll(m => basePbrMaterialType.Contains(m.ClassName(false)));

            // 灯光材质
            List<IMtl> LightMaterials = materials.Where(m => lightMaterialType.Contains(m.ClassName(false))).ToList();
            materials.RemoveAll(m => lightMaterialType.Contains(m.ClassName(false)));

            // 复合材质/多层材质
            List<IMtl> multiMaterials = materials.Where(m => multiMaterialType.Contains(m.ClassName(false))).ToList();
            materials.RemoveAll(m => multiMaterialType.Contains(m.ClassName(false)));

            // Single Material
            foreach (var material in singleMaterials)
            {
                // 清理不支持的纹理
                RedHaloTools.CleanupMaterial(material);
                var materialType = material.ClassName(false);
                var originalName = material.Name;

                try
                {
                    var redHaloPBRMtl = Exporter.ExportMaterial(material);
                    if (redHaloPBRMtl != null)
                    {
                        redHaloPBRMtl.Type = "pbr_material";
                        redhaloScene.Materials.Add(redHaloPBRMtl);
                    }

                    //if (materialType == "CoronaSelectMtl")
                    //{
                    //    var selectIndex = RedHaloTools.GetValueByID<int>(material, 0, 2);
                    //    var pb = material.GetParamBlock(0);
                    //    var sub = pb.GetMtl(pb.IndextoID(1), 0, RedHaloCore.Forever, selectIndex);
                    //}
                }
                catch (Exception ex)
                {
                    RedHaloTools.WriteLog($"错误材质名 ：{material.Name}，原始材质名：{originalName}, 类型：{materialType}");
                    RedHaloTools.WriteLog($"ERROR INFO:\n{ex.Message}");
                }
            }

            // Light Material
            foreach (var material in LightMaterials)
            {
                // 清理不支持的纹理
                RedHaloTools.CleanupMaterial(material);

                var materialType = material.ClassName(false);
                var originalName = material.Name;

                try
                {
                    var redHaloLightMtl = Exporter.ExportLightMaterial(material);
                    if (redHaloLightMtl != null)
                    {
                        redHaloLightMtl.Type = "light_material";
                        redhaloScene.Materials.Add(redHaloLightMtl);
                    }

                }
                catch (Exception ex)
                {
                    RedHaloTools.WriteLog($"错误材质名 ：{material.Name}，原始材质名：{originalName}, 类型：{materialType}");
                    RedHaloTools.WriteLog($"ERROR INFO:\n{ex.Message}");
                }
            }

            // mutli materials
            foreach (var material in multiMaterials)
            {
                var materialType = material.ClassName(false);

                //RedHaloTools.WriteLog($"材质 ：{material.Name}，类型：{materialType}");

                try
                {
                    if (materialType == "VRayBlendMtl")
                    {
                        var redHaloMtl = Exporter.ExportVRayBlendMtl(material);
                        if (redHaloMtl != null)
                        {
                            redhaloScene.Materials.Add(redHaloMtl);
                        }
                    }
                    else if (materialType == "CoronaLayerMtl")
                    {
                        var redHaloMtl = Exporter.ExportCoronaLayerMtl(material);
                        if (redHaloMtl != null)
                        {
                            redhaloScene.Materials.Add(redHaloMtl);
                        }
                    }
                    //else if (materialType == "Blend")
                    else if (materialType == "Double Sided")
                    {
                        var redHaloMtl = Exporter.ExportDoubleMtl(material);
                        if (redHaloMtl != null)
                        {
                            redhaloScene.Materials.Add(redHaloMtl);
                        }
                    }
                    else if (materialType == "VRay2SidedMtl")
                    {
                        var redHaloMtl = Exporter.ExportVRayDoubleMtl(material);
                        if (redHaloMtl != null)
                        {
                            redhaloScene.Materials.Add(redHaloMtl);
                        }
                    }
                    else if (materialType == "VRayOverrideMtl")
                    {
                        var redHaloMtl = Exporter.ExportVRayOverrideMtl(material);
                        if (redHaloMtl != null)
                        {
                            redhaloScene.Materials.Add(redHaloMtl);
                        }
                    }
                    else if (materialType == "CoronaRaySwitchMtl")
                    {
                        var redHaloMtl = Exporter.ExportCoronaRaySwitchMtl(material);
                        if (redHaloMtl != null)
                        {
                            redhaloScene.Materials.Add(redHaloMtl);
                        }
                    }
                }catch (Exception ex)
                {
                    RedHaloTools.WriteLog($"错误材质名 ：{material.Name}，类型：{materialType}");
                    RedHaloTools.WriteLog($"ERROR INFO:\n{ex.Message}");
                }
            }

            #endregion
            
            RedHaloTools.WriteLog($"=========导出材质结束=========");

            #region WRITE XML FILE / JSON FILE
            //var writeSucess = RedHaloTools.WriteFile<RedHaloScene>(redhaloScene, outputFileName);
            var writeSucess = RedHaloTools.WriteJsonFile<RedHaloScene>(redhaloScene, outputFileName);
            if (!writeSucess)
            {
                RedHaloTools.WriteLog($"导出文件失败，请检查文件权限");
                return 2;
            }
            else
            {
                RedHaloTools.WriteLog($"导出文件成功，文件路径：{outputFileName}");
            }
            #endregion

            #region EXPORT MODELS

            if (writeSucess)
            {
                string outputModelFileName = "";
                if (redhaloScene.Settings.ExportFormat == "USD")
                {
                    if (RedHaloTools.IsPluginInstalled("USD Importer"))
                    {
                        outputModelFileName = Path.Combine(redhaloScene.Settings.OutputPath, baseFileName + usdExtension);
                        RedHaloTools.ExportUSDFile(outputModelFileName);
                    }
                }
                else
                {
                    outputModelFileName = Path.Combine(redhaloScene.Settings.OutputPath, baseFileName + fbxExtension);
                    // 其他格式的导出
                    RedHaloTools.ExportFBXFile(outputModelFileName);
                }


                RedHaloTools.WriteLog($"导出模型成功，文件路径：{outputModelFileName}");
            }
            else
            {
                RedHaloTools.WriteLog($"导出模型失败，请检查文件权限");
                return 3;
            }
            #endregion

            return 0;
        }
    }
}
