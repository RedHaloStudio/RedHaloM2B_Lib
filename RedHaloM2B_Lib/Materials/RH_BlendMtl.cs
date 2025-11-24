using Newtonsoft.Json;
using System.Collections.Generic;

namespace RedHaloM2B.Materials
{
    internal class RedHaloBlendMtl : RedHaloBaseMtl
    {
        [JsonProperty("base_material")]
        public string BaseMaterial { get; set; }

        [JsonProperty("layers")]
        public List<RedHaloLayer> Layers { get; set; }
    }
}
