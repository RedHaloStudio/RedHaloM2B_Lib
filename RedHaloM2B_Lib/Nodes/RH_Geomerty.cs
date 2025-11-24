using Newtonsoft.Json;

namespace RedHaloM2B.Nodes
{
    public class RedHaloGeometry : RedHaloBaseNode
    {
        [JsonProperty("renderable")]
        public bool Renderable { get; set; }

        [JsonProperty("visiblecamera")]
        public bool VisibleCamera { get; set; }

        [JsonProperty("visiblespecular")]
        public bool VisibleSpecular { get; set; }

        [JsonProperty("visiblevolume")]
        public bool VisibleVolume { get; set; }

        [JsonProperty("castshadow")]
        public bool CastShadow { get; set; }
    }
}