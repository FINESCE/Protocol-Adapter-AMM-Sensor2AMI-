using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IAM2IDAS.observations
{
    /// <summary>
    /// This element indicates the measurement resource origin. It must be a previously described resource.
    /// If the measure or observation origin resource is recorded as a System element, then the
    /// om:procedure element coincides with the registered element.
    /// If the measure or observation origin resource is a component of an action described as System,
    /// then the om:procedure field contains a reference to the component.
    /// </summary>
    /// <example>
    ///     <code>
    ///         <om:procedure xlink:href="WeatherStation_1.12345678"/>
    ///     </code>
    /// </example>
    [XmlType("procedure", Namespace = "http://www.opengis.net/om/1.0")]
    public class Procedure
    {
        [XmlAttribute("href",Namespace="http://www.w3.org/1999/xlink")]
        public string href;
    }
}
