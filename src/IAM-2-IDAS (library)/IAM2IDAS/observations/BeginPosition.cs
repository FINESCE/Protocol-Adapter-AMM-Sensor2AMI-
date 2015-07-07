using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IAM2IDAS.observations
{
    [XmlType("beginPosition", Namespace = "http://www.opengis.net/gml")]
    public class BeginPosition
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
