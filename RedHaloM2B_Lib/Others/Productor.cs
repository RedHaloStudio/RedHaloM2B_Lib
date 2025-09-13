using Newtonsoft.Json;

namespace RedHaloM2B
{
    public class RedHaloProductor
    {
        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("export_version")]
        public string ExportVersion { get; set; }

        [JsonProperty("file")]
        public string File { get; set; }

        [JsonProperty("renderer")]
        public string Renderer { get; set; }

        [JsonProperty("build_time")]
        public string BuildTime { get; set; }

        [JsonProperty("active_camera")]
        public string ActiveCameraID { get; set; }
    }
}
