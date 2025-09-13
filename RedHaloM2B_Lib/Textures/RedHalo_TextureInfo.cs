using Newtonsoft.Json;
using System.Collections.Generic;

namespace RedHaloM2B.Textures
{
    public class TexmapInfo : BaseTex
    {
        [JsonProperty(propertyName: "params")]
        public Dictionary<string, dynamic> Properties { get; set; } = new Dictionary<string, dynamic>();

        [JsonProperty(propertyName: "subtexmap")]
        public Dictionary<string, TexmapInfo> subTexmapInfo { get; set; } = new Dictionary<string, TexmapInfo>();
        
        public TexmapInfo() { }
    }
}