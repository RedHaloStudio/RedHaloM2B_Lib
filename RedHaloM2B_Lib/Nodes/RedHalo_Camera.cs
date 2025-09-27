using Newtonsoft.Json;

namespace RedHaloM2B.Nodes
{
    public class RedHaloCamera : RedHaloBaseNode
    {
        [JsonProperty("fov")]
        public float Fov { get; set; } = 50f;

        [JsonProperty("focaldistance")]
        public float FocalDistance { get; set; }

        [JsonProperty("clippingnear")]
        public float ClippingNear { get; set; } = 0.05f;

        [JsonProperty("clippingfar")]
        public float ClippingFar { get; set; } = 1000f;

        [JsonProperty("shiftx")]
        public float ShiftX { get; set; }

        [JsonProperty("shifty")]
        public float ShiftY { get; set; }

        [JsonProperty("sensorwidth")]
        public float SensorWidth { get; set; } = 36f;

        [JsonProperty("cameratype")]
        public int CameraType { get; set; }
    }
}
