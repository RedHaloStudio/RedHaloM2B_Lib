using RedHaloM2B.Materials;
using System.Collections.Generic;

namespace RedHaloM2B
{
    //[XmlRoot("redhaloscene")]
    public class RedHaloSceneA
    {

        // Materials
        //[XmlArray("materials")]
        public List<RedHaloPBRMtl> Materials { get; set; }

        public RedHaloSceneA() { }
        public RedHaloSceneA(string output)
        {
            Materials = new List<RedHaloPBRMtl>();
        }
    }
}
