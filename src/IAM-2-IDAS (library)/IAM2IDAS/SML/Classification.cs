﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace IAM2IDAS.SML
{
    [XmlType("classification", Namespace = "http://www.opengis.net/sensorML/1.0.1")]
    public class Classification
    {
        [XmlElement("ClassifierList", Namespace="http://www.opengis.net/sensorML/1.0.1")]
        public ClassifierList classifierList;
    }
}
