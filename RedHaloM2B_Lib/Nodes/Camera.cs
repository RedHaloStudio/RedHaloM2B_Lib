using System.Xml.Serialization;

namespace RedHaloM2B.Nodes
{
    [XmlRoot("camera")]
    public class RedHaloCamera : BaseNode
    {
        [XmlAttribute("fov")]
        public float Fov { get; set; }

        [XmlAttribute("focaldistance")]
        public float FocalDistance { get; set; }

        [XmlAttribute("clippingnear")]
        public float ClippingNear { get; set; }

        [XmlAttribute("clippingfar")]
        public float ClippingFar { get; set; }

        [XmlAttribute("shiftx")]
        public float ShiftX { get; set; }

        [XmlAttribute("shifty")]
        public float ShiftY { get; set; }

        [XmlAttribute("sensorwidth")]
        public float SensorWidth { get; set; }

        [XmlAttribute("cameratype")]
        public int CameraType { get; set; }
    }
}
