using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RedHaloM2B
{
    public class BaseNode
    {
        //[XmlAttribute("id")]
        //public string ID { get; set; }
        
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("original_name")]
        public string OriginalName {  get; set; }

        //[XmlAttribute("parentid")]
        //public string ParentId {  get; set; }

        [XmlAttribute("base_object")]
        public string BaseObject { get; set; }

        [XmlAttribute("transform")]
        public string Transform {  get; set; }
        
        [XmlAttribute("pivotoffset")]
        public string PivotOffset {  get; set; }
        
    }
}
