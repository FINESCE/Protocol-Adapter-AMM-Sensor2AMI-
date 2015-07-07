using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IAM2IDAS
{
    [XmlType("uom", Namespace = "http://www.opengis.net/swe/1.0.1")]
    public class Uom
    {
        public Uom() { }
        public Uom(string code)
        { this.code = code; }

        //[XmlAttribute("href", Namespace = "http://www.w3.org/1999/xlink")]
        //public string href;
        [XmlAttribute("code", Namespace = "http://www.opengis.net/swe/1.0.1")]
        public String code;
    }
}
