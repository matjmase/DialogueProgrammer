using DialogueProgrammer.Serialization.Export;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DialogueProgrammer.Serialization.Project
{
    [Serializable()]
    public class ProjectSerializedNode : ExportSerializedNode
    {
        [XmlElement]
        public double CanvasLeft { get; set; }
        [XmlElement]
        public double CanvasTop { get; set; }
    }
}
