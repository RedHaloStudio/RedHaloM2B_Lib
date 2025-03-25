using RedHaloM2B.Materials;
using RedHaloM2B.Nodes;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace RedHaloM2B
{
    [XmlRoot("redhaloscene")]
    public class RedHaloScene
    {
        // Producter
        [XmlElement("productor")]
        public RedHaloProductor Productor { get; set; }

        // Settings
        [XmlElement("settings")]
        public RedHaloSettings Settings { get; set; }

        // Environment
        //public RedHaloEnvironment Environment { get; set; }

        // Nodes
        [XmlArray("geometries")]
        [XmlArrayItem("geometry")]
        public List<RedHaloGeometry> GeometryList { get; set; }

        // Lights
        [XmlArray("lights")]
        [XmlArrayItem("light")]
        public List<Nodes.RedHaloLight> LightsList { get; set; }

        // Cameras
        [XmlArray("cameras")]
        public List<RedHaloCamera> Cameras { get; set; }

        // Materials
        [XmlArray("materials")]
        public List<RedHaloPBRMtl> Materials { get; set; }

        public RedHaloScene()
        {
            GeometryList = [];
            LightsList = [];
            Cameras = [];
            Materials = [];
        }

        public RedHaloScene(string output)
        {
            //OutputFiles = output;
            GeometryList = [];
            LightsList = [];
            Cameras = [];
            Materials = [];
        }

    }
}
