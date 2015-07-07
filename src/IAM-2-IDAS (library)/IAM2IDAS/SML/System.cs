using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace IAM2IDAS.SML
{
    /// <summary>
    /// Any resource that is defined in the domain of Ambient Intelligence Platform should be described
    /// using SensorML within one sml:System element. This item is considered the more general
    /// model to describe any resources because it allows including components that do not have its
    /// own entity outside.
    /// However, you can choose to use this element for individual descriptions of resources (sensors
    /// and actuators).
    /// The following paragraph provides an example of tags among which it is described the System
    /// element.
    /// <example> 
    ///     <code>
    ///         <sml:System
    ///         gml:id="f81d4fae-­‐7dec-­‐11d0-­‐a765-­‐00a0c91e6bf6"
    ///         xsi:schemaLocation="http://www.opengis.net/sensorML/1.0.1
    ///         system.xsd"
    ///         xmlns:xlink="http://www.w3.org/1999/xlink"
    ///         xmlns:swe="http://www.opengis.net/swe/1.0"
    ///         xmlns:gml="http://www.opengis.net/gml"
    ///         xmlns:sml="http://www.opengis.net/sensorML/1.0.1"
    ///         xmlns:xsi=http://www.w3.org/2001/XMLSchema-­‐instance
    ///         Descripción
    ///         </sml:System>
    ///     </code>
    /// </summary>
    [XmlType("System", Namespace = "http://www.opengis.net/sensorML/1.0.1")]
    public class System
    {
        [XmlAttribute("id", Namespace = "http://www.opengis.net/gml")]
        public string id;
        //[XmlElement("name", Namespace = "http://www.opengis.net/gml")]
        //public string name;
        //[XmlElement("description", Namespace = "http://www.opengis.net/gml")]
        //public string description;
        [XmlElement("identification")]
        public Identification identification;
        //[XmlElement("classification")]
        //public Classification classification;
        //[XmlElement("capabilities")]
        //public Capabilities capabilities;
        //[XmlElement("position")]
        //public Position position;
        [XmlElement("inputs")]
        public Inputs inputs;
        [XmlElement("outputs")]
        public Outputs outputs;
    }
}
