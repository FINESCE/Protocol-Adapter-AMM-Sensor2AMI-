using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IAM2IDAS.observations
{
    [XmlType("endPosition", Namespace = "http://www.opengis.net/gml")]
    public class EndPosition
    {
        private DateTime _dateTime;
        [XmlText()]
        public String dateTime
        {
            get
            {
                return _dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
            }
            set
            {
                this._dateTime = DateTime.Parse(value);
            }
        }
    }
}
