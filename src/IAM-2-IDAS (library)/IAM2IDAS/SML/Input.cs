using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IAM2IDAS.SML
{
    [XmlType("input", Namespace = "http://www.opengis.net/sensorML/1.0.1")]
    public class Input
    {
        public Input() { }
        public Input(string name)
        { this.name = name; }

        [XmlAttribute("name", Namespace = "http://www.opengis.net/sensorML/1.0.1")]
        public string name;
        [XmlElement("ObservableProperty", Namespace = "http://www.opengis.net/swe/1.0.1")]
        public ObservableProperty observableProperty;
        //[XmlElement("Text", Namespace = "http://www.opengis.net/swe/1.0.1")]
        //public Text text;
    }
}
