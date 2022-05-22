using DialogueProgrammer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DialogueProgrammer.Serialization.Project
{
    [Serializable()]
    public class ProjectSerializedDimensionScaler
    {
        [XmlElement]
        public double BaseWidth { get; set; }
        [XmlElement]
        public double BaseHeight { get; set; }  
        [XmlElement]
        public double ScalerWidth { get; set; }
        [XmlElement]
        public double ScalerHeight { get; set; }

        #region Conversions

        public DimensionScalerVM ToDimensionScaler()
        { 
            return new DimensionScalerVM() { BaseWidth = BaseWidth, BaseHeight = BaseHeight, ScalerWidth = ScalerWidth, ScalerHeight = ScalerHeight};
        }

        #endregion
    }
}
