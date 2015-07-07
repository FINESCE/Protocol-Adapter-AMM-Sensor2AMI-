using IAM2IDAS.observations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IAM2IDAS.SOS
{
    [XmlType("ObservationTemplate", Namespace = "http://www.opengis.net/sos/1.0")]
    public class ObservationTemplate
    {
        [XmlElement("Observation", Namespace = "http://www.opengis.net/om/1.0")]
        public Observation observation;
    }
}
