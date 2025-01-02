using RedHaloM2B.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RedHaloM2B
{
    [XmlRoot("redhaloscene")]
    public class RedHaloScene
    {
        // Producter
        [XmlElement("productor")]
        public RedHaloProductor Productor {  get; set; }
        
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
        public List<RedHaloLight> LightsList { get; set; }

        // Cameras
        [XmlArray("cameras")]
        public List<RedHaloCamera> cameras { get; set; }        

        public RedHaloScene() { }        
        public RedHaloScene(string output)
        {
            //OutputFiles = output;
            GeometryList = new List<RedHaloGeometry>();
            LightsList = new List<RedHaloLight>();
            cameras = new List<RedHaloCamera>();
        }
        
    }
}
