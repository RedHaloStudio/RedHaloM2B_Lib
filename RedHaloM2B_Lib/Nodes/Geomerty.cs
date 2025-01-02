using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RedHaloM2B.Nodes
{
    [XmlRoot("node")]
    public class RedHaloGeometry : BaseNode
    {
        [XmlAttribute("renderable")]
        public bool Renderable { get; set; }

        [XmlAttribute("visiblecamera")]
        public bool VisibleCamera { get; set; }

        [XmlAttribute("visiblespecular")]
        public bool VisibleSpecular { get; set; }

        [XmlAttribute("visiblevolume")]
        public bool VisibleVolume { get; set; }

        [XmlAttribute("castshadow")]
        public bool CastShadow { get; set; }
    }
}