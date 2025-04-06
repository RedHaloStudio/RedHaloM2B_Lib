using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RedHaloM2B.Materials;
using RedHaloM2B.Textures;

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
