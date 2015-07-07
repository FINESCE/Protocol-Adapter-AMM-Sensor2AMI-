using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IAM2IDAS
{
    [XmlType("ObservableProperty", Namespace = "http://www.opengis.net/swe/1.0.1")]
    public class ObservableProperty
    {
        public ObservableProperty() { }
        public ObservableProperty(string definition)
        { this.definition = definition; }
        [XmlAttribute]
        public string definition;
    }
}
