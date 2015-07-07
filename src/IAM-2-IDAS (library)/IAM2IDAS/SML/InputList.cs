using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IAM2IDAS.SML
{
    [XmlType("InputList", Namespace = "http://www.opengis.net/sensorML/1.0.1")]
    public class InputList
    {
        [XmlElement("input", Namespace = "http://www.opengis.net/sensorML/1.0.1")]
        public Input[] input;
    }
}
