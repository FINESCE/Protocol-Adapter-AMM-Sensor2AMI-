using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IAM2IDAS
{
    [XmlType("timePosition", Namespace = "http://www.opengis.net/gml")]
    public class TimePosition
    {
        [XmlAttribute]
        public string frame;
        
        [XmlIgnore]
        public DateTime dateTime;

        [XmlText]
        public string formattedDateTime
        {
            get { return dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ"); }
            set{this.formattedDateTime=value;}

        } 

    }
}
