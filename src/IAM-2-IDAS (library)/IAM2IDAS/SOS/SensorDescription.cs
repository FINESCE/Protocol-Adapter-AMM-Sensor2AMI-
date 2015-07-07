using IAM2IDAS.SML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IAM2IDAS.SOS
{
    [XmlType("SensorDescription", Namespace = "http://www.opengis.net/sos/1.0")]
    public class SensorDescription
    {
        //[XmlText]
        //public string content;
        //[XmlElement("SensorML", Namespace = "http://www.opengis.net/sensorML/1.0.1")]
        //public SensorML sensorml;
        [XmlElement("System", Namespace = "http://www.opengis.net/sensorML/1.0.1")]
        public SML.System system;
    }
}
