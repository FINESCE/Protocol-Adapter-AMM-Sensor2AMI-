using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IAM2IDAS.SML
{
    [XmlType("output", Namespace = "http://www.opengis.net/sensorML/1.0.1")]
    public class Output
    {
        public Output() { }
        public Output(string name)
        { this.name = name; }

        [XmlAttribute("name", Namespace = "http://www.opengis.net/sensorML/1.0.1")]
        public string name;
        [XmlElement("Quantity", Namespace = "http://www.opengis.net/swe/1.0.1")]
        public Quantity quantity;
        [XmlElement("Text", Namespace = "http://www.opengis.net/swe/1.0.1")]
        public Text text;
    }
}
