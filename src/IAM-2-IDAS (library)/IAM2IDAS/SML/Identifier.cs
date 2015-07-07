using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IAM2IDAS.SML
{
    [XmlType("identifier", Namespace = "http://www.opengis.net/sensorML/1.0.1")]
    public class Identifier
    {
        [XmlAttribute]
        public string name;

        [XmlElement("Term")]
        public Term term;
    }
}
