using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IAM2IDAS.observations
{
    ///<summary>Indicates a point in time for measurement or observation. 
    ///This moment in time can represent a point in time (gml:TimeInstant) or a time interval (gml:TimePeriod).
    ///</summary>
    ///<example>
    ///The following example shows a description of a point in time:
    ///<code>
    ///<om:samplingTime>
    ///     <gml:TimeInstant>
    ///         <gml:timePosition frame="urn:x-­‐ogc:def:trs:IDAS:1.0:ISO8601">2010­‐04­‐14T11:29:47
    ///         </gml:timePosition>
    ///     </gml:TimeInstant>
    ///</om:samplingTime>
    ///</code>
    ///
    ///The following example describes a time interval:
    ///<code>
    ///<om:samplingTime>
    ///     <gml:TimePeriod>
    ///         <gml:beginPosition frame="urn:x-­‐ogc:def:trs:IDAS:1.0:ISO8601">
    ///             2010‐08-­05T12:23:03Z
    ///         </gml:beginPosition>
    ///         <gml:endPosition frame="urn:x-­‐ogc:def:trs:IDAS:1.0:ISO8601">
    ///             2010­‐08-05T12:23:03Z
    ///         </gml:endPosition>
    ///     </gml:TimePeriod>
    ///</om:samplingTime>
    ///</code>
    /// </example>
    [XmlType("samplingTime", Namespace = "http://www.opengis.net/om/1.0")]
    public class SamplingTime
    {
        [XmlElement("TimeInstant", Namespace = "http://www.opengis.net/gml")]
        public TimeInstant timeInstant;
        public TimePeriod timePeriod;
    }
}
