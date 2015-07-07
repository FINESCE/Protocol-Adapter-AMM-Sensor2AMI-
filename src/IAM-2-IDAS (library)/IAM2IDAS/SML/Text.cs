using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IAM2IDAS.SML
{
    [XmlType("Text", Namespace = "http://www.opengis.net/swe/1.0.1")]
    public class Text
    {
        public Text() { }
        public Text(string content) {
            this.content = content;
        }
        [XmlElement("value", Namespace = "http://www.opengis.net/swe/1.0.1")]
        public string content;
    }
}
