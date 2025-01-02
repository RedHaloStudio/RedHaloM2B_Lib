using Autodesk.Max;
using RedHaloM2B.Nodes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace RedHaloM2B
{
    public class ExportScene
    {
        // Export Scene
        public static int SceneExporter(string fileFormat, bool explodeGroup, bool convertToPoly)
        {
            #region GLOBAL VAR

            int index = 0;

            // 临时文件（夹）有：
            // RH_M2B_TEMP
            // RH_M2B_TEMP/Textures 不支持中文路径的贴图文件会复制到些目录
            // %temp%/log.log 日志文件
            // RH_M2B_TEMP/RHM2B_CONTENT.xml  （以下的参数：设置、物体、相机、灯光）
            // RH_M2B_TEMP/RHM2B_material.xml (材质参数，最终会上面文件合并,暂时方案)

            string tempOutDirectory = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "RH_M2B_TEMP");
            string outputFileName = Path.Combine(tempOutDirectory, "RHM2B_CONTENT.xml");            
            string logFilename = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "RHM2B_log.log");
            
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

            // 缩放整个场景（以米为基础单位），匹配Blender单位尺寸
            RedHaloTools.RescaleScene();

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
            
            RedHaloTools.WriteLog("开始设置文件写入");            

            redhaloScene.Settings = new RedHaloSettings
            {
                MetersScale = (float)RedHaloCore.Global.GetSystemUnitScale(5), //5 Meter
                ImageWidth = RedHaloCore.Core.RendWidth,
                ImageHeight = RedHaloCore.Core.RendHeight,
                ImagePixelAspect = RedHaloCore.Core.ImageAspRatio,
                AnimateStart = startFrame,
                AnimateEnd = endFrame,
                FrameRate = 4800 / RedHaloCore.Global.TicksPerFrame, // There are always 4800 ticks per second, this means that ticksPerFrame is dependent on the frames per second rate (ticksPerFrame * frameRate == 4800)
                Gamma = RedHaloCore.Global.GammaMgr.DispGamma,
                LinearWorkflow = 0,
                ExportFormat = fileFormat
            };

            RedHaloTools.WriteLog("设置文件写入完成");
            
            #endregion

            #region EXPORT NODES
            IEnumerable<IINode> staticMesh = RedHaloTools.GetSceneNodes().
                Where(o => 
                o.ObjectRef.FindBaseObject().SuperClassID == SClass_ID.Geomobject || 
                o.ObjectRef.FindBaseObject().SuperClassID == SClass_ID.Shape ||
                o.ObjectRef.FindBaseObject().SuperClassID == SClass_ID.Helper
            );
            
            foreach (var node in staticMesh)
            {
                if (nodeKeys.Contains(node.Name)) {
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

                    string baseobj = $"Mesh_{index:D5}"; //tabs[0].Name;
                    
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

            RedHaloTools.WriteLog($"共导出 {staticMesh.Count()} 个物体");
            // 重置Index
            index = 0;
            nodeKeys.Clear();
            #endregion
            
            #region EXPORT CAMERAS
            // 场景中所有相机物体
            var cameraNodes = RedHaloTools.GetSceneNodes().Where(obj => obj.ObjectRef.FindBaseObject().SuperClassID == SClass_ID.Camera);
            RedHaloCamera redHaloCamera = new();

            foreach (var cam in cameraNodes)
            {
                redHaloCamera = Exporter.ExportCamera(cam, index);
                redhaloScene.cameras.Add(redHaloCamera);
                index++;
            }

            index = 0;
            nodeKeys.Clear();

            RedHaloTools.WriteLog($"共导出 {cameraNodes.Count()} 个相机");
            
            #endregion

            #region EXPORT LIGHTS

            // 场景中所有灯光物体
            var lightNodes = RedHaloTools.GetSceneNodes().Where(obj => obj.ObjectRef.FindBaseObject().SuperClassID == SClass_ID.Light);
            
            foreach (var light in lightNodes)
            {
                //Debug.Print(light.Name);
                //Debug.Print($"0x{light.ObjectRef.ClassID.PartA.ToString("X").PadLeft(8,'0')}, 0x{light.ObjectRef.ClassID.PartB.ToString("X").PadLeft(8, '0')}");

                if (nodeKeys.Contains(light.Name))
                {
                    continue;
                }
                else
                {
                    RedHaloCore.Global.IInstanceMgr.InstanceMgr.GetInstances(light, tabs);

                    string BaseObjectName = $"Light_{index:D5}";

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

            #region WRITE XML FILE
            var writeSucess = RedHaloTools.WriteFile<RedHaloScene>(redhaloScene, outputFileName);
            #endregion

            return 0;
        }
    }
}
