using Newtonsoft.Json;

namespace RedHaloM2B.Nodes
{
    public class RedHaloCamera : RedHaloBaseNode
    {
        [JsonProperty("fov")]
        public float Fov { get; set; }

        [JsonProperty("focaldistance")]
        public float FocalDistance { get; set; }

        [JsonProperty("clippingnear")]
        public float ClippingNear { get; set; }

        [JsonProperty("clippingfar")]
        public float ClippingFar { get; set; }

        [JsonProperty("shiftx")]
        public float ShiftX { get; set; }

        [JsonProperty("shifty")]
        public float ShiftY { get; set; }

        [JsonProperty("sensorwidth")]
        public float SensorWidth { get; set; }

        [JsonProperty("cameratype")]
        public int CameraType { get; set; }
    }
}
