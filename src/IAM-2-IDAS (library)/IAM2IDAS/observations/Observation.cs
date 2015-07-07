using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace IAM2IDAS.observations
{
//V1.0.0
//    <om:Observation
//      xsi:schemaLocation="http://www.opengis.net/om/1.0
//      observation.xsd"
//      xmlns:gml="http://www.opengis.net/gml"
//      xmlns:xlink="http://www.w3.org/1999/xlink"
//      xmlns:swe=http://www.opengis.net/swe/1.0.1
//      xmlns:om="http://www.opengis.net/om/1.0"
//      xmlns:xsi="http://www.w3.org/2001/XMLSchema-­‐instance">
//      <om:samplingTime/>
//      <om:procedure/>
//      <om:observedProperty/>
//      <om:featureOfInterest/>
//      <om:parameter/>
//      <om:result/>
//  </om:Observation>

    [XmlType("Observation", Namespace = "http://www.opengis.net/om/1.0")]
    public class Observation
    {
        //<om:samplingTime/>
        [XmlElement]
        public SamplingTime samplingTime;
        //<om:procedure/>
        [XmlElement]
        public Procedure procedure;
        //<om:observedProperty/>
        [XmlElement]
        public ObservedProperty observedProperty;
        //<om:featureOfInterest/>
        [XmlElement]
        public FeatureOfInterest featureOfInterest;
        //<om:parameter/>
        [XmlElement]
        public Parameter parameter;
        //<om:result/>
        [XmlElement]
        public Result result;
    }
}
