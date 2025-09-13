using Newtonsoft.Json;
using RedHaloM2B.Nodes;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace RedHaloM2B
{
    public class RedHaloScene
    {
        // Producter
        [JsonProperty("productor")]
        public RedHaloProductor Productor { get; set; }

        // Settings
        [JsonProperty("settings")]
        public RedHaloSettings Settings { get; set; }

        // Environment
        //public RedHaloEnvironment Environment { get; set; }

        // Nodes
        [JsonProperty("geometries")]
        public List<RedHaloGeometry> GeometryList { get; set; }

        // Lights
        [JsonProperty("lights")]
        public List<Nodes.RedHaloLight> LightsList { get; set; }

        // Cameras
        [JsonProperty("cameras")]
        public List<RedHaloCamera> Cameras { get; set; }

        // Materials
        [JsonProperty("materials")]
        public List<dynamic> Materials { get; set; }

        public RedHaloScene()
        {
            GeometryList = [];
            LightsList = [];
            Cameras = [];
            Materials = [];
        }

        public RedHaloScene(string output)
        {
            GeometryList = [];
            LightsList = [];
            Cameras = [];
            Materials = [];
        }

    }
}
