using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IAM2IDAS.SML
{
    [XmlType("Term", Namespace = "http://www.opengis.net/sensorML/1.0.1")]
    public class Term
    {
        [XmlAttribute]
        public string definition;

        [XmlElement]
        public SMLValue value;
    }
}
