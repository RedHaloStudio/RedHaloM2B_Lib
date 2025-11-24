using Newtonsoft.Json;

namespace RedHaloM2B.Textures
{
    public class BaseTex
    {
        [JsonProperty("name", Order = -3)]
        public string Name { get; set; }

        [JsonProperty("type", Order = -2)]
        public string Type { get; set; }

        public BaseTex() { }
    }
}
