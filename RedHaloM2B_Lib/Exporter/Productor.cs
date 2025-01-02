using System.Xml.Serialization;

namespace RedHaloM2B
{
    [XmlRoot("producer")]
    public class RedHaloProductor
    {
        [XmlElement("host")]
        public string Host { get; set; }

        [XmlElement("version")]
        public string Version { get; set; }

        [XmlElement("export_version")]
        public string ExportVersion { get; set; }

        [XmlElement("file")]
        public string File { get; set; }

        [XmlElement("renderer")]
        public string Renderer { get; set; }

        [XmlElement("build_time")]
        public string BuildTime { get; set; }

        [XmlElement("active_camera")]
        public string ActiveCameraID { get; set; }
    }
}
