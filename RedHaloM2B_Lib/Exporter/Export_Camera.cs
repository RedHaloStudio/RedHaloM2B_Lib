using Autodesk.Max;
using RedHaloM2B.Nodes;
using System;

namespace RedHaloM2B
{
    partial class Exporter
    {
        public static RedHaloCamera ExportCamera(IINode camera)
        {
            // 缓存一些常量，避免重复读取
            var foreverTime = RedHaloCore.Forever;
            var maxCameraState = RedHaloCore.Global.CameraState.Create();
            var maxSensorWidth = RedHaloCore.Core.RendApertureWidth;

            int cEnabledClipping = 0;

            float cFov = 50.0f;
            float cClippingNear = 0.05f;
            float cClippingFar = 1000f;
            float cShiftX = 0.0f;
            float cShiftY = 0.0f;
            float cSensorWidth = 36.0f;

            // 相机名称设置
            string cameraSourceName = camera.Name;
            string md5 = RedHaloTools.CalcMD5FromString(cameraSourceName);
            //string cameraName = $"camera_{cameraIndex:D5}";
            string cameraName = $"C_{md5}";
            camera.Name = cameraName;

            // 创建 Max 相机和相机状态对象
            var maxCamera = camera.ObjectRef.FindBaseObject() as ICameraObject;
            maxCamera?.EvalCameraState(0, foreverTime, maxCameraState);

            // 获取 ParamBlock2，如果没有则设置 flag
            IIParamBlock2 camParamBlock = null;
            bool hasPB2 = TryGetParamBlock(maxCamera, out camParamBlock);

            // Helper Method: 获取 ParamBlock2
            bool TryGetParamBlock(ICameraObject maxCamera, out IIParamBlock2 paramBlock)
            {
                paramBlock = null;
                try
                {
                    paramBlock = maxCamera?.SubAnim(0) as IIParamBlock2;
                    return paramBlock != null;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            // 获取相机矩阵
            var cameraMatrix = camera.GetObjectTM(0, foreverTime);

            // 创建 RedHaloCamera 对象
            RedHaloCamera redHaloCamera = new RedHaloCamera
            {
                Name = cameraName,
                OriginalName = cameraSourceName,
                Transform = RedHaloTools.ConvertMatrixToString(cameraMatrix),
            };

            switch (maxCamera.ClassID.PartA)
            {
                // Default Camera
                case 0x00001001:
                // Default target camera
                case 0x00001002:
                    redHaloCamera.Fov = Convert.ToSingle(maxSensorWidth / 2 / Math.Tan(maxCamera.GetFOV(0) / 2.0f));

                    if (maxCamera.ManualClip != 0)
                    {
                        redHaloCamera.ClippingNear = maxCamera.GetClipDist(0, 1);
                        redHaloCamera.ClippingFar = maxCamera.GetClipDist(0, 2);
                    }

                    redHaloCamera.SensorWidth = maxSensorWidth;
                    redHaloCamera.CameraType = maxCamera.IsOrtho ? 0 : 1;
                    break;

                // Physical Camera
                case 0x46697218:
                    if (hasPB2)
                    {
                        //Fov
                        camParamBlock.GetValue(camParamBlock.IndextoID(7), 0, ref cFov, RedHaloCore.Forever, 0);
                        redHaloCamera.Fov = cFov;

                        //SensorWidth
                        camParamBlock.GetValue(camParamBlock.IndextoID(6), 0, ref cSensorWidth, RedHaloCore.Forever, 0);
                        redHaloCamera.SensorWidth = cSensorWidth;

                        // Clipping On
                        camParamBlock.GetValue(camParamBlock.IndextoID(48), 0, ref cEnabledClipping, RedHaloCore.Forever, 0);

                        if (cEnabledClipping != 0)
                        {
                            //Clip Min
                            camParamBlock.GetValue(camParamBlock.IndextoID(49), 0, ref cClippingNear, RedHaloCore.Forever, 0);
                            redHaloCamera.ClippingNear = cClippingNear;

                            //Clip Max
                            camParamBlock.GetValue(camParamBlock.IndextoID(50), 0, ref cClippingFar, RedHaloCore.Forever, 0);
                            redHaloCamera.ClippingFar = cClippingFar;
                        }

                        //Shift X
                        camParamBlock.GetValue(camParamBlock.IndextoID(40), 0, ref cShiftX, RedHaloCore.Forever, 0);
                        redHaloCamera.ShiftX = cShiftX;

                        //Shift Y
                        camParamBlock.GetValue(camParamBlock.IndextoID(41), 0, ref cShiftY, RedHaloCore.Forever, 0);
                        redHaloCamera.ShiftY = cShiftY;

                        //Use Focus
                        //redHaloCamera.useFocus = camParamBlock.GetBool(camParamBlock.IndextoID(12), 0, 0);

                        //Mode
                        redHaloCamera.CameraType = 0;
                    }
                    break;

                // Corona Camera
                case 0xA5843284:
                    if (hasPB2)
                    {
                        //Fov
                        camParamBlock.GetValue(camParamBlock.IndextoID(21), 0, ref cFov, RedHaloCore.Forever, 0);
                        redHaloCamera.Fov = cFov;

                        //SensorWidth
                        camParamBlock.GetValue(camParamBlock.IndextoID(23), 0, ref cSensorWidth, RedHaloCore.Forever, 0);
                        redHaloCamera.SensorWidth = cSensorWidth;

                        // Camera Clipping
                        camParamBlock.GetValue(camParamBlock.IndextoID(4), 0, ref cEnabledClipping, RedHaloCore.Forever, 0);
                        if (cEnabledClipping != 0)
                        {
                            //Clip Min
                            camParamBlock.GetValue(camParamBlock.IndextoID(5), 0, ref cClippingNear, RedHaloCore.Forever, 0);
                            redHaloCamera.ClippingNear = cClippingNear;

                            //Clip Max
                            camParamBlock.GetValue(camParamBlock.IndextoID(6), 0, ref cClippingFar, RedHaloCore.Forever, 0);
                            redHaloCamera.ClippingFar = cClippingFar;
                        }

                        //Shift X
                        camParamBlock.GetValue(camParamBlock.IndextoID(75), 0, ref cShiftX, RedHaloCore.Forever, 0);
                        redHaloCamera.ShiftX = cShiftX;

                        //Shift Y
                        camParamBlock.GetValue(camParamBlock.IndextoID(73), 0, ref cShiftY, RedHaloCore.Forever, 0);
                        redHaloCamera.ShiftY = cShiftY;

                        //Use Focus
                        //redHaloCamera.useFocus = camParamBlock.GetBool(camParamBlock.IndextoID(12), 0, 0);

                        //Mode
                        redHaloCamera.CameraType = 0;

                    }
                    break;

                // Vray Camera
                case 0x405E3FEE:
                    if (hasPB2)
                    {
                        //Fov
                        camParamBlock.GetValue(camParamBlock.IndextoID(10), 0, ref cFov, RedHaloCore.Forever, 0);
                        redHaloCamera.Fov = cFov;

                        //SensorWidth
                        camParamBlock.GetValue(camParamBlock.IndextoID(9), 0, ref cSensorWidth, RedHaloCore.Forever, 0);
                        redHaloCamera.SensorWidth = cSensorWidth;

                        // Clip On
                        camParamBlock.GetValue(camParamBlock.IndextoID(54), 0, ref cEnabledClipping, RedHaloCore.Forever, 0);

                        if (cEnabledClipping != 0)
                        {
                            //Clip Min
                            camParamBlock.GetValue(camParamBlock.IndextoID(55), 0, ref cClippingNear, RedHaloCore.Forever, 0);
                            redHaloCamera.ClippingNear = cClippingNear;

                            //Clip Max
                            camParamBlock.GetValue(camParamBlock.IndextoID(56), 0, ref cClippingFar, RedHaloCore.Forever, 0);
                            redHaloCamera.ClippingNear = cClippingFar;
                        }

                        //Shift X
                        camParamBlock.GetValue(camParamBlock.IndextoID(38), 0, ref cShiftX, RedHaloCore.Forever, 0);
                        redHaloCamera.ShiftX = cShiftX;

                        //Shift Y
                        camParamBlock.GetValue(camParamBlock.IndextoID(36), 0, ref cShiftY, RedHaloCore.Forever, 0);
                        redHaloCamera.ShiftY = cShiftY;

                        //Use Focus
                        //redHaloCamera.useFocus = camParamBlock.GetBool(camParamBlock.IndextoID(12), 0, 0);

                        //Mode
                        redHaloCamera.CameraType = 0;
                    }
                    break;

                default:
                    break;
            }

            return redHaloCamera;
        }
    }
}
