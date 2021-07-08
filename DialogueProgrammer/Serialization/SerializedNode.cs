using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DialogueProgrammer.Serialization
{
    [Serializable()]
    public class SerializedNode
    {
        [XmlElement]
        public int NodeId { get; set; }
        [XmlElement]
        public string DialogueText { get; set; }
        [XmlArray]
        public SerializedOption[] Options { get; set; }
    }
}
