using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IAM2IDAS.SML
{
    [XmlType("value", Namespace = "http://www.opengis.net/swe/1.0.1")]
    public class Value
    {
        [XmlText]
        public string value;
    }
}
