using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using IAM2IDAS.SML;

namespace IAM2IDAS
{
    [XmlType("Quantity", Namespace = "http://www.opengis.net/swe/1.0.1")]
    public class Quantity
    {
        public Quantity() { }
        public Quantity(string definition)
        { this.definition = definition; }

        [XmlAttribute]
        public string definition;
        [XmlElement("uom")]
        public Uom uom;
        [XmlElement("value", Namespace = "http://www.opengis.net/swe/1.0.1")]
        public String value;
    }
}
