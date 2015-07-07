using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IAM2IDAS.SML
{
    [XmlType("DataRecord", Namespace = "http://www.opengis.net/swe/1.0.1")]
    public class DataRecord
    {
        [XmlElement("description", Namespace = "http://www.opengis.net/gml")]
        public string description;
        [XmlElement("field", Namespace = "http://www.opengis.net/swe/1.0.1")]
        public Field[] field;
    }
}
