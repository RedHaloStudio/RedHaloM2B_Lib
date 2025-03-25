using System.Xml.Serialization;

namespace RedHaloM2B
{
    [XmlRoot("settings")]
    public class RedHaloSettings
    {
        [XmlElement("meters_scale")]
        public float MetersScale { get; set; }

        [XmlElement("image_width")]
        public int ImageWidth { get; set; }

        [XmlElement("image_height")]
        public int ImageHeight { get; set; }

        [XmlElement("image_pixel_aspect")]
        public float ImagePixelAspect { get; set; }

        [XmlElement("start")]
        public int AnimateStart { get; set; }

        [XmlElement("end")]
        public int AnimateEnd { get; set; }

        [XmlElement("frame_rate")]
        public float FrameRate { get; set; }

        [XmlElement("gamma")]
        public float Gamma { get; set; }

        [XmlElement("linear_workflow")]
        public int LinearWorkflow { get; set; }

        [XmlElement("export_format")]
        public string ExportFormat { get; set; }
    }
}