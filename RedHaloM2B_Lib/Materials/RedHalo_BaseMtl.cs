using Newtonsoft.Json;

namespace RedHaloM2B.Materials
{
    public class RedHaloBaseMtl
    {
        [JsonProperty("name", Order = -5)]
        public string Name { get; set; }

        [JsonProperty("id", Order = -4)]
        public string ID { get; set; }

        [JsonProperty("source_name", Order = -3)]
        public string SourceName { get; set; }

        [JsonProperty("type", Order = -2)]
        public string Type { get; set; }
    }
}
