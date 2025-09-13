using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedHaloM2B.Materials
{
    
    public class RedHaloMaterials
    {
        [JsonProperty("single-materials")]
        public List<RedHaloPBRMtl> PBRMtl { get; set; } = new List<RedHaloPBRMtl>();

        [JsonProperty("light-materials")]
        public List<RedHaloLightMtl> LightMtl { get; set; } = new List<RedHaloLightMtl>();

    }
}
