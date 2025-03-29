using Newtonsoft.Json;

namespace RedHaloM2B.Materials
{
    public class RedHaloBaseMtl
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("source_name")]
        public string SourceName { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
