﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DialogueProgrammer.Serialization.Export
{
    [Serializable()]
    public class ExportSerializedOption
    {
        [XmlElement]
        public int ResponseId { get; set; }
        [XmlElement]
        public int PointToNode { get; set; }
        [XmlElement]
        public string OptionText { get; set; }
    }
}
