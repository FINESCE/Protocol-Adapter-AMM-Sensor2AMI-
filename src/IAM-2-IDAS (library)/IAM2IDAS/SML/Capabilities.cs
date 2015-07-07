using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IAM2IDAS.SML
{
    //<capabilities>
    // <swe:DataRecord definition="urn:ogc:def:property:OGC:measurementProperties">
    // <gml:description> 
    // The External Temperature Sensor is used to measure temperatures in general 
    // conditions. It is well-suited for air, water, or soil temperature measurements, 
    //and it may be used anywhere a reliable, low-cost temperature sensor is required. 
    // The sensor is epoxy-encapsulated in a vinyl cap. The External Temperature Sensor 
    // uses a precision platinum wire thermistor as a sensor. The thermistor produces a 
    // resistance change proportional to temperature. To ensure accurate readings when 
    // measuring outdoor air temperature, the External Temperature Sensor should be 
    // shielded from direct sunlight and other sources of reflected or radiated heat. 
    //We recommend the use of a Davis Radiation Shield (#7714) or its equivalent for 
    // this purpose.</gml:description>
    // <!-- add EnvironmentalLimit group -->
    //  <swe:field name="resolution" xlink:arcrole="urn:ogc:def:property:OGC:resolution">
    //      <swe:Quantity definition="urn:ogc:def:property:OGC:temperature">
    //          <swe:uom code="cel"/>
 //             <swe:value>0.1</swe:value>
 //         </swe:Quantity>
 //     </swe:field>
 //     <swe:field name="range" xlink:arcrole="urn:ogc:def:property:OGC:dynamicRange">
 //         <swe:QuantityRange definition="urn:ogc:def:property:OGC:temperature">
 //             <swe:uom code="cel"/>
 //             <swe:value>-45 60</swe:value>
 //         </swe:QuantityRange>
 //     </swe:field>
 //     <swe:field name="accuracy" xlink:arcrole="urn:ogc:def:property:OGC:accuracy">
 //         <swe:QuantityRange definition="urn:ogc:def:property:OGC:absoluteAccuracy">
 //             <swe:uom xlink:href="urn:ogc:unit:percent"/>
 //             <swe:value>-0.5 0.5</swe:value>
 //         </swe:QuantityRange>
 //     </swe:field>
 //</swe:DataRecord>
 //</capabilities>
    [XmlType("capabilities", Namespace = "http://www.opengis.net/sensorML/1.0.1")]
    public class Capabilities
    {
        [XmlElement("DataRecord", Namespace = "http://www.opengis.net/swe/1.0.1")]
        public DataRecord datarecord;
    }
}
