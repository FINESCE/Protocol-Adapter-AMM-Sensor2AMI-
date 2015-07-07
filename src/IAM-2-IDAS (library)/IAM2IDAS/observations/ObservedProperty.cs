using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IAM2IDAS
{
    /// <summary>
    /// Indicates the property or physical phenomenon for which it is providing a value.
    /// Must match one of the inputs of the description fields and use the URN as reference.
    /// </summary>
    /// <example>
    ///     <code>
    ///         <om:observedProperty xlink:href="urn:x-­‐ogc:def:phenomenon:IDAS:1.0:temperature"/>
    ///     </code>
    /// </example>
    [XmlType("observedProperty", Namespace = "http://www.opengis.net/om/1.0")]
    public class ObservedProperty
    {
        [XmlAttribute("href", Namespace = "http://www.w3.org/1999/xlink")]
        public string href;
    }
}
