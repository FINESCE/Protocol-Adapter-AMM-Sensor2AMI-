using IAM2IDAS.observations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IAM2IDAS.SOS
{
    /// <example>
    ///     <code>
    ///         <sos:InsertObservation
    ///         service="SOS"
    ///         version="1.0.0"
    ///         xsi:schemaLocation="http://www.opengis.net/sos/1.0
    ///         sosInsert.xsd"
    ///         xmlns:om="http://www.opengis.net/om/1.0"
    ///         xmlns:sos="http://www.opengis.net/sos/1.0"
    ///         xmlns:xsi="http://www.w3.org/2001/XMLSchema-­‐instance">
    ///             <sos:AssignedSensorId>WeatherStation_1</sos:AssignedSensorId>
    ///             <om:Observation>
    ///                 <om:samplingTime/>
    ///                 <om:procedure/>
    ///                 <om:observedProperty/>
    ///                 <om:featureOfInterest/>
    ///                 <om:result/>
    ///             </om:Observation>
    ///         </sos:InsertObservation>
    ///     </code>
    /// </example>
    [XmlRoot("InsertObservation", Namespace = "http://www.opengis.net/sos/1.0")]
    public class InsertObservation
    {
        [XmlElement("AssignedSensorId")]
        public string assignedSensorId;
        [XmlElement("Observation", Namespace = "http://www.opengis.net/om/1.0")]
        public Observation observation;
    }
}
