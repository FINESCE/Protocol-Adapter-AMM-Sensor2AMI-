using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IAM2IDAS.observations
{
    [XmlType("timePeriod", Namespace = "http://www.opengis.net/gml")]
    public class TimePeriod
    {
        public BeginPosition beginPosition;
        public EndPosition endPosition;
    }
}
