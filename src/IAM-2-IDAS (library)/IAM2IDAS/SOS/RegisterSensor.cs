using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace IAM2IDAS.SOS
{
    /// <summary>
    /// </summary>
    /// <example>
    ///     <code>
    ///         <sos:RegisterSensor
    ///         service="SOS"
    ///         version="1.0.0"
    ///         xsi:schemaLocation="http://www.opengis.net/sos/1.0
    ///         sosRegisterSensor.xsd"
    ///         xmlns:om="http://www.opengis.net/om/1.0"
    ///         xmlns:swe="http://www.opengis.net/swe/1.0.1"
    ///         xmlns:sml="http://www.opengis.net/sensorML/1.0.1"
    ///         xmlns:sos="http://www.opengis.net/sos/1.0"
    ///         xmlns:xsi="http://www.w3.org/2001/XMLSchema-­‐instance">
    ///             <sos:SensorDescription>
    ///                 <sml:System/>ver 3.1
    ///             </sos:SensorDescription>
    ///             <sos:ObservationTemplate>
    ///                 <om:Observation>
    ///                     <om:samplingTime/>
    ///                     <om:procedure/>
    ///                     <om:observedProperty/>
    ///                     <om:featureOfInterest/>
    ///                     <om:parameter/>
    ///                     <om:result/>
    ///                 </om:Observation>
    ///             </sos:ObservationTemplate>
    ///         </sos:RegisterSensor>
    ///     </code>
    /// </example>
    [XmlRoot("RegisterSensor", Namespace = "http://www.opengis.net/sos/1.0")]
    public class RegisterSensor
    {
        public String service;
        public String version;
        [XmlElement("SensorDescription")]
        public SensorDescription sensorDescription;
        [XmlElement("ObservationTemplate")]
        public ObservationTemplate observationTemplate;
    }
}
