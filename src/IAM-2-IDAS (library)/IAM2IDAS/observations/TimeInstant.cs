using System.Xml.Serialization;

namespace IAM2IDAS
{
    [XmlType("TimeInstant", Namespace = "http://www.opengis.net/gml")]
    public class TimeInstant
    {
        [XmlElement("timePosition")]
        public TimePosition timePosition;
    }
}
