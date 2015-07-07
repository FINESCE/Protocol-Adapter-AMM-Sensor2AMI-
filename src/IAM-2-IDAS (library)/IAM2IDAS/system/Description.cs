using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace IAM2IDAS
{
    [XmlType("description", Namespace = "http://www.opengis.net/gml")]
    public class Description
    {
        [XmlText()]
        public String content {get; set;}
    }
}
