using Autodesk.Max;
using RedHaloM2B.Nodes;
using System;
using System.Diagnostics;

namespace RedHaloM2B
{
    partial class Exporter
    {
        public static RedHaloCamera ExportCamera(IINode camera, int cameraIndex)
        {
            float cFov = 35.0f;
            float cClippingNear = 0.1f;
            float cClippingFar = 20f;
            float cShiftX = 0.0f;
            float cShiftY = 0.0f;
            float cSensorWidth = 36.0f;

            string cameraSourceName = camera.Name;
            string cameraName = $"Camera_{cameraIndex:D5}";
            //string cid = Guid.NewGuid().ToString();            
            
            // Set New Name
            camera.Name = cameraName;

            /// For Max Camera
            var maxCamera = camera.ObjectRef.FindBaseObject() as ICameraObject;
            var maxCameraState = RedHaloCore.Global.CameraState.Create();
            maxCamera.EvalCameraState(0, RedHaloCore.Forever, maxCameraState);

            bool hasPB2 = true;
            IIParamBlock2 camParamBlock = null;
            try
            {
                camParamBlock = maxCamera.SubAnim(0) as IIParamBlock2;
            }
            catch (Exception)
            {
                hasPB2 = false;
            }

            var maxSensorWidth = RedHaloCore.Core.RendApertureWidth;

            RedHaloCamera redHaloCamera = new()
            {
                Name = cameraName,
                OriginalName = cameraSourceName
            };

            var cameraMatrix = camera.GetObjectTM(0, RedHaloCore.Forever);
            redHaloCamera.Transform = RedHaloTools.ConvertMatrixToString(cameraMatrix);

            switch (maxCamera.ClassID.PartA)
            {
                // Default Camera
                case 0x00001001:
                // Default target camera
                case 0x00001002:
                    redHaloCamera.Fov = Convert.ToSingle(maxSensorWidth / 2 / Math.Tan(maxCamera.GetFOV(0) / 2.0f));
                    redHaloCamera.ClippingNear = maxCamera.GetClipDist(0, 1);
                    redHaloCamera.ClippingFar = maxCamera.GetClipDist(0, 2);
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

                        //Clip Min
                        camParamBlock.GetValue(camParamBlock.IndextoID(49), 0, ref cClippingNear, RedHaloCore.Forever, 0);
                        redHaloCamera.ClippingNear = cClippingNear;

                        //Clip Max
                        camParamBlock.GetValue(camParamBlock.IndextoID(50), 0, ref cClippingFar, RedHaloCore.Forever, 0);
                        redHaloCamera.ClippingFar = cClippingFar;

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
                        Debug.WriteLine(cFov);
                        redHaloCamera.Fov = cFov;

                        //SensorWidth
                        camParamBlock.GetValue(camParamBlock.IndextoID(23), 0, ref cSensorWidth, RedHaloCore.Forever, 0);
                        redHaloCamera.SensorWidth = cSensorWidth;

                        //Clip Min
                        camParamBlock.GetValue(camParamBlock.IndextoID(5), 0, ref cClippingNear, RedHaloCore.Forever, 0);
                        redHaloCamera.ClippingNear = cClippingNear;

                        //Clip Max
                        camParamBlock.GetValue(camParamBlock.IndextoID(6), 0, ref cClippingFar, RedHaloCore.Forever, 0);
                        redHaloCamera.ClippingFar = cClippingFar;

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

                        //Clip Min
                        camParamBlock.GetValue(camParamBlock.IndextoID(55), 0, ref cClippingNear, RedHaloCore.Forever, 0);
                        redHaloCamera.ClippingNear = cClippingNear;

                        //Clip Max
                        camParamBlock.GetValue(camParamBlock.IndextoID(56), 0, ref cClippingFar, RedHaloCore.Forever, 0);
                        redHaloCamera.ClippingNear = cClippingFar;

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
