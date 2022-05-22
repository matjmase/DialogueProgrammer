using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DialogueProgrammer.Serialization.Project
{
    [Serializable()]
    public class ProjectSerializedMainWindow
    {
        [XmlElement]
        public ProjectSerializedDimensionScaler Canvas { get; set; }

        [XmlArray]
        public ProjectSerializedNode[] Nodes { get; set; }
    }
}
