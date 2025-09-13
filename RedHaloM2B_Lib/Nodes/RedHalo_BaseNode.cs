using Newtonsoft.Json;
using System.Xml.Serialization;

namespace RedHaloM2B
{
    public class RedHaloBaseNode
    {
        [JsonProperty("name", Order = -8)]
        public string Name { get; set; }

        [JsonProperty("original_name", Order = -7)]
        public string OriginalName { get; set; }

        [JsonProperty("id", Order = -6)]
        public string ID { get; set; }

        [JsonProperty("base_object", Order = -5)]
        public string BaseObject { get; set; }


        [JsonProperty("matrix", Order = -4)]
        public string Transform { get; set; }

        [JsonProperty("pivotoffset", Order = -3)]
        public float[] PivotOffset { get; set; } = new float[4];

        [JsonProperty("layer", Order = -2)]
        public string layer { get; set; } = "0";

    }
}
