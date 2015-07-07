using IAM2IDAS.observations;
using IAM2IDAS.SML;
using IAM2IDAS.SOS;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace IAM2IDAS
{
    public class IDASClient
    {
        private string serviceUrl;

        public IDASClient(string serviceUrl)
        {
            this.serviceUrl = serviceUrl;
        }

        public String registerMeter(string meterId)
        {
            RegisterSensor rs = new RegisterSensor();
            rs.sensorDescription = new SensorDescription();

            rs.sensorDescription.system = new SML.System();
            rs.sensorDescription.system.id = "meter:"+meterId;
            
            rs.sensorDescription.system.identification = new Identification();
            rs.sensorDescription.system.identification.identifierList = new IdentifierList();
            rs.sensorDescription.system.identification.identifierList.identifier = new Identifier[1];
            rs.sensorDescription.system.identification.identifierList.identifier[0] = new Identifier();
            rs.sensorDescription.system.identification.identifierList.identifier[0].term = new Term();
            rs.sensorDescription.system.identification.identifierList.identifier[0].term.definition = "urn:x-ogc:def:identifier:IDAS:1.0:localIdentifier";
            rs.sensorDescription.system.identification.identifierList.identifier[0].term.value = new SMLValue();
            rs.sensorDescription.system.identification.identifierList.identifier[0].term.value.content = "meter:"+meterId;

            rs.sensorDescription.system.inputs = new Inputs();
            rs.sensorDescription.system.inputs.inputList = new InputList();
            rs.sensorDescription.system.inputs.inputList.input = new Input[10];
            rs.sensorDescription.system.inputs.inputList.input[0]=new Input("upstreamActivePower");
            rs.sensorDescription.system.inputs.inputList.input[0].observableProperty = new ObservableProperty("urn:x-ogc:def:phenomenon:FINESCE:1.0:upstreamActivePower");
            rs.sensorDescription.system.inputs.inputList.input[1]=new Input("downstreamActivePower");
            rs.sensorDescription.system.inputs.inputList.input[1].observableProperty = new ObservableProperty("urn:x-ogc:def:phenomenon:FINESCE:1.0:downstreamActivePower");  
            rs.sensorDescription.system.inputs.inputList.input[2]=new Input("RP_Q1");
            rs.sensorDescription.system.inputs.inputList.input[2].observableProperty = new ObservableProperty("urn:x-ogc:def:phenomenon:FINESCE:1.0:RP_Q1");
            rs.sensorDescription.system.inputs.inputList.input[3]=new Input("RP_Q2");
            rs.sensorDescription.system.inputs.inputList.input[3].observableProperty = new ObservableProperty("urn:x-ogc:def:phenomenon:FINESCE:1.0:RP_Q2");
            rs.sensorDescription.system.inputs.inputList.input[4] = new Input("RP_Q3");
            rs.sensorDescription.system.inputs.inputList.input[4].observableProperty = new ObservableProperty("urn:x-ogc:def:phenomenon:FINESCE:1.0:RP_Q3");
            rs.sensorDescription.system.inputs.inputList.input[5] = new Input("RP_Q4");
            rs.sensorDescription.system.inputs.inputList.input[5].observableProperty = new ObservableProperty("urn:x-ogc:def:phenomenon:FINESCE:1.0:RP_Q4");
            rs.sensorDescription.system.inputs.inputList.input[6] = new Input("isConcentrator");
            rs.sensorDescription.system.inputs.inputList.input[6].observableProperty = new ObservableProperty("urn:x-ogc:def:phenomenon:FINESCE:1.0:isConcentrator");
            rs.sensorDescription.system.inputs.inputList.input[7] = new Input("currentTime");
            rs.sensorDescription.system.inputs.inputList.input[7].observableProperty = new ObservableProperty("urn:x-ogc:def:phenomenon:FINESCE:1.0:currentTime");
            rs.sensorDescription.system.inputs.inputList.input[8] = new Input("meterID");
            rs.sensorDescription.system.inputs.inputList.input[8].observableProperty = new ObservableProperty("urn:x-ogc:def:phenomenon:FINESCE:1.0:meterID");
            rs.sensorDescription.system.inputs.inputList.input[9] = new Input("meterType");
            rs.sensorDescription.system.inputs.inputList.input[9].observableProperty = new ObservableProperty("urn:x-ogc:def:phenomenon:FINESCE:1.0:meterType");

            rs.sensorDescription.system.outputs = new Outputs();
            rs.sensorDescription.system.outputs.outputList = new OutputList();
            rs.sensorDescription.system.outputs.outputList.output=new Output[10];
            rs.sensorDescription.system.outputs.outputList.output[0]=new Output("upstreamActivePower");
            rs.sensorDescription.system.outputs.outputList.output[0].quantity = new Quantity("urn:x-ogc:def:phenomenon:IDAS:1.0:power");
            rs.sensorDescription.system.outputs.outputList.output[0].quantity.uom = new Uom("kW");
            rs.sensorDescription.system.outputs.outputList.output[1]=new Output("downstreamActivePower");
            rs.sensorDescription.system.outputs.outputList.output[1].quantity = new Quantity("urn:x-ogc:def:phenomenon:IDAS:1.0:power");
            rs.sensorDescription.system.outputs.outputList.output[1].quantity.uom = new Uom("kW");
            rs.sensorDescription.system.outputs.outputList.output[2] = new Output("RP_Q1");
            rs.sensorDescription.system.outputs.outputList.output[2].quantity = new Quantity("urn:x-ogc:def:phenomenon:IDAS:1.0:reactivePower");
            rs.sensorDescription.system.outputs.outputList.output[2].quantity.uom = new Uom("kVAr");
            rs.sensorDescription.system.outputs.outputList.output[3] = new Output("RP_Q2");
            rs.sensorDescription.system.outputs.outputList.output[3].quantity = new Quantity("urn:x-ogc:def:phenomenon:IDAS:1.0:reactivePower");
            rs.sensorDescription.system.outputs.outputList.output[3].quantity.uom = new Uom("kVAr");
            rs.sensorDescription.system.outputs.outputList.output[4] = new Output("RP_Q3");
            rs.sensorDescription.system.outputs.outputList.output[4].quantity = new Quantity("urn:x-ogc:def:phenomenon:IDAS:1.0:reactivePower");
            rs.sensorDescription.system.outputs.outputList.output[4].quantity.uom = new Uom("kVAr");
            rs.sensorDescription.system.outputs.outputList.output[5] = new Output("RP_Q4");
            rs.sensorDescription.system.outputs.outputList.output[5].quantity = new Quantity("urn:x-ogc:def:phenomenon:IDAS:1.0:reactivePower");
            rs.sensorDescription.system.outputs.outputList.output[5].quantity.uom = new Uom("kVAr");
            rs.sensorDescription.system.outputs.outputList.output[6] = new Output("isConcentrator");
            rs.sensorDescription.system.outputs.outputList.output[6].text = new Text();
            rs.sensorDescription.system.outputs.outputList.output[7] = new Output("currentTime");
            rs.sensorDescription.system.outputs.outputList.output[7].text = new Text();
            rs.sensorDescription.system.outputs.outputList.output[8] = new Output("meterID");
            rs.sensorDescription.system.outputs.outputList.output[8].text = new Text();
            rs.sensorDescription.system.outputs.outputList.output[9] = new Output("meterType");
            rs.sensorDescription.system.outputs.outputList.output[9].text = new Text();

            rs.observationTemplate = new ObservationTemplate();
            rs.observationTemplate.observation = new Observation();
            rs.observationTemplate.observation.samplingTime = new SamplingTime();
            rs.observationTemplate.observation.observedProperty = new ObservedProperty();
            rs.observationTemplate.observation.procedure = new Procedure();
            rs.observationTemplate.observation.featureOfInterest = new FeatureOfInterest();
            rs.observationTemplate.observation.parameter = new Parameter();
            rs.observationTemplate.observation.result = new Result();

            XmlSerializer ser = new XmlSerializer(typeof(RegisterSensor));

            Utf8StringWriter textWriter = new Utf8StringWriter();

            ser.Serialize(textWriter, rs);
            return sendMessage(textWriter.ToString());
        
        }

        public String registerLoad(string meterId)
        {
            RegisterSensor rs = new RegisterSensor();
            rs.sensorDescription = new SensorDescription();

            rs.sensorDescription.system = new SML.System();
            rs.sensorDescription.system.id = "load:"+meterId;

            rs.sensorDescription.system.identification = new Identification();
            rs.sensorDescription.system.identification.identifierList = new IdentifierList();
            rs.sensorDescription.system.identification.identifierList.identifier = new Identifier[1];
            rs.sensorDescription.system.identification.identifierList.identifier[0] = new Identifier();
            rs.sensorDescription.system.identification.identifierList.identifier[0].term = new Term();
            rs.sensorDescription.system.identification.identifierList.identifier[0].term.definition = "urn:x-ogc:def:identifier:IDAS:1.0:localIdentifier";
            rs.sensorDescription.system.identification.identifierList.identifier[0].term.value = new SMLValue();
            rs.sensorDescription.system.identification.identifierList.identifier[0].term.value.content = "load:"+meterId;

            rs.sensorDescription.system.inputs = new Inputs();
            rs.sensorDescription.system.inputs.inputList = new InputList();
            rs.sensorDescription.system.inputs.inputList.input = new Input[14];
            rs.sensorDescription.system.inputs.inputList.input[0] = new Input("sampleNumber");
            rs.sensorDescription.system.inputs.inputList.input[0].observableProperty = new ObservableProperty("urn:x-ogc:def:phenomenon:FINESCE:1.0:sampleNumber");
            rs.sensorDescription.system.inputs.inputList.input[1] = new Input("loadTime");
            rs.sensorDescription.system.inputs.inputList.input[1].observableProperty = new ObservableProperty("urn:x-ogc:def:phenomenon:FINESCE:1.0:loadTime");
            rs.sensorDescription.system.inputs.inputList.input[2] = new Input("downstreamActivePowerEEA");
            rs.sensorDescription.system.inputs.inputList.input[2].observableProperty = new ObservableProperty("urn:x-ogc:def:phenomenon:FINESCE:1.0:downstreamActivePowerEEA");
            rs.sensorDescription.system.inputs.inputList.input[3] = new Input("reactiveInductivePowerEEI");
            rs.sensorDescription.system.inputs.inputList.input[3].observableProperty = new ObservableProperty("urn:x-ogc:def:phenomenon:FINESCE:1.0:reactiveInductivePowerEEI");
            rs.sensorDescription.system.inputs.inputList.input[4] = new Input("reactiveCapacitivePowerEEC");
            rs.sensorDescription.system.inputs.inputList.input[4].observableProperty = new ObservableProperty("urn:x-ogc:def:phenomenon:FINESCE:1.0:reactiveCapacitivePowerEEC");
            rs.sensorDescription.system.inputs.inputList.input[5] = new Input("upstreamActivePowerEUA");
            rs.sensorDescription.system.inputs.inputList.input[5].observableProperty = new ObservableProperty("urn:x-ogc:def:phenomenon:FINESCE:1.0:upstreamActivePowerEUA");
            rs.sensorDescription.system.inputs.inputList.input[6] = new Input("reactiveInductivePowerEUI");
            rs.sensorDescription.system.inputs.inputList.input[6].observableProperty = new ObservableProperty("urn:x-ogc:def:phenomenon:FINESCE:1.0:reactiveInductivePowerEUI");
            rs.sensorDescription.system.inputs.inputList.input[7] = new Input("reactiveCapacitivePowerEUC");
            rs.sensorDescription.system.inputs.inputList.input[7].observableProperty = new ObservableProperty("urn:x-ogc:def:phenomenon:FINESCE:1.0:reactiveCapacitivePowerEUC");
            rs.sensorDescription.system.inputs.inputList.input[8] = new Input("tariffType");
            rs.sensorDescription.system.inputs.inputList.input[8].observableProperty = new ObservableProperty("urn:x-ogc:def:phenomenon:FINESCE:1.0:tariffType");
            rs.sensorDescription.system.inputs.inputList.input[9] = new Input("integrationPeriodRef");
            rs.sensorDescription.system.inputs.inputList.input[9].observableProperty = new ObservableProperty("urn:x-ogc:def:phenomenon:FINESCE:1.0:integrationPeriodRef");
            rs.sensorDescription.system.inputs.inputList.input[10] = new Input("currentTime");
            rs.sensorDescription.system.inputs.inputList.input[10].observableProperty = new ObservableProperty("urn:x-ogc:def:phenomenon:FINESCE:1.0:currentTime");
            rs.sensorDescription.system.inputs.inputList.input[11] = new Input("meterID");
            rs.sensorDescription.system.inputs.inputList.input[11].observableProperty = new ObservableProperty("urn:x-ogc:def:phenomenon:FINESCE:1.0:meterID");
            rs.sensorDescription.system.inputs.inputList.input[12] = new Input("meterType");
            rs.sensorDescription.system.inputs.inputList.input[12].observableProperty = new ObservableProperty("urn:x-ogc:def:phenomenon:FINESCE:1.0:meterType");
            rs.sensorDescription.system.inputs.inputList.input[13] = new Input("isConcentrator");
            rs.sensorDescription.system.inputs.inputList.input[13].observableProperty = new ObservableProperty("urn:x-ogc:def:phenomenon:FINESCE:1.0:isConcentrator");

            rs.sensorDescription.system.outputs = new Outputs();
            rs.sensorDescription.system.outputs.outputList = new OutputList();
            rs.sensorDescription.system.outputs.outputList.output = new Output[14];
            rs.sensorDescription.system.outputs.outputList.output[0] = new Output("sampleNumber");
            rs.sensorDescription.system.outputs.outputList.output[0].text = new Text();
            rs.sensorDescription.system.outputs.outputList.output[1] = new Output("loadTime");
            rs.sensorDescription.system.outputs.outputList.output[1].text = new Text();
            rs.sensorDescription.system.outputs.outputList.output[2] = new Output("downstreamActivePowerEEA");
            rs.sensorDescription.system.outputs.outputList.output[2].quantity = new Quantity("urn:x-ogc:def:phenomenon:IDAS:1.0:power");
            rs.sensorDescription.system.outputs.outputList.output[2].quantity.uom = new Uom("kW");
            rs.sensorDescription.system.outputs.outputList.output[3] = new Output("reactiveInductivePowerEEI");
            rs.sensorDescription.system.outputs.outputList.output[3].quantity = new Quantity("urn:x-ogc:def:phenomenon:IDAS:1.0:reactivePower");
            rs.sensorDescription.system.outputs.outputList.output[3].quantity.uom = new Uom("kVAr");
            rs.sensorDescription.system.outputs.outputList.output[4] = new Output("reactiveCapacitivePowerEEC");
            rs.sensorDescription.system.outputs.outputList.output[4].quantity = new Quantity("urn:x-ogc:def:phenomenon:IDAS:1.0:reactivePower");
            rs.sensorDescription.system.outputs.outputList.output[4].quantity.uom = new Uom("kVAr");
            rs.sensorDescription.system.outputs.outputList.output[5] = new Output("upstreamActivePowerEUA");
            rs.sensorDescription.system.outputs.outputList.output[5].quantity = new Quantity("urn:x-ogc:def:phenomenon:IDAS:1.0:power");
            rs.sensorDescription.system.outputs.outputList.output[5].quantity.uom = new Uom("kW");
            rs.sensorDescription.system.outputs.outputList.output[6] = new Output("reactiveInductivePowerEUI");
            rs.sensorDescription.system.outputs.outputList.output[6].quantity = new Quantity("urn:x-ogc:def:phenomenon:IDAS:1.0:reactivePower");
            rs.sensorDescription.system.outputs.outputList.output[6].quantity.uom = new Uom("kVAr");
            rs.sensorDescription.system.outputs.outputList.output[7] = new Output("reactiveCapacitivePowerEUC");
            rs.sensorDescription.system.outputs.outputList.output[7].quantity = new Quantity("urn:x-ogc:def:phenomenon:IDAS:1.0:reactivePower");
            rs.sensorDescription.system.outputs.outputList.output[7].quantity.uom = new Uom("kVAr");
            rs.sensorDescription.system.outputs.outputList.output[8] = new Output("tariffType");
            rs.sensorDescription.system.outputs.outputList.output[8].text = new Text();
            rs.sensorDescription.system.outputs.outputList.output[9] = new Output("integrationPeriodRef");
            rs.sensorDescription.system.outputs.outputList.output[9].text = new Text();
            rs.sensorDescription.system.outputs.outputList.output[10] = new Output("currentTime");
            rs.sensorDescription.system.outputs.outputList.output[10].text = new Text();
            rs.sensorDescription.system.outputs.outputList.output[11] = new Output("meterID");
            rs.sensorDescription.system.outputs.outputList.output[11].text = new Text(); 
            rs.sensorDescription.system.outputs.outputList.output[12] = new Output("meterType");
            rs.sensorDescription.system.outputs.outputList.output[12].text = new Text();
            rs.sensorDescription.system.outputs.outputList.output[13] = new Output("isConcentrator");
            rs.sensorDescription.system.outputs.outputList.output[13].text = new Text();

            rs.observationTemplate = new ObservationTemplate();
            rs.observationTemplate.observation = new Observation();
            rs.observationTemplate.observation.samplingTime = new SamplingTime();
            rs.observationTemplate.observation.observedProperty = new ObservedProperty();
            rs.observationTemplate.observation.procedure = new Procedure();
            rs.observationTemplate.observation.featureOfInterest = new FeatureOfInterest();
            rs.observationTemplate.observation.parameter = new Parameter();
            rs.observationTemplate.observation.result = new Result();

            XmlSerializer ser = new XmlSerializer(typeof(RegisterSensor));

            Utf8StringWriter textWriter = new Utf8StringWriter();

            ser.Serialize(textWriter, rs);
            return sendMessage(textWriter.ToString());

        }


        public void insertMeterObservation(string sensorId, 
                                            long samplingTime,
                                            double upstreamActivePower,
                                            double downstreamActivePower,
                                            double reactivePowerQ1,
                                            double reactivePowerQ2,
                                            double reactivePowerQ3,
                                            double reactivePowerQ4)
        {
            XmlSerializer ser = new XmlSerializer(typeof(InsertObservation));
            Utf8StringWriter textWriter = new Utf8StringWriter();

            InsertObservation io = new InsertObservation();
            io.assignedSensorId = "meter:" + sensorId;
            io.observation = new Observation();
            io.observation.samplingTime = new SamplingTime();
            io.observation.samplingTime.timeInstant = new TimeInstant();
            io.observation.samplingTime.timeInstant.timePosition = new TimePosition();
            io.observation.samplingTime.timeInstant.timePosition.frame = "urn:x-ogc:def:trs:IDAS:1.0:ISO8601";
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            io.observation.samplingTime.timeInstant.timePosition.dateTime = epoch.AddSeconds(samplingTime);
            io.observation.procedure = new Procedure();
            io.observation.procedure.href = "meter:" + sensorId;
            io.observation.observedProperty = new ObservedProperty();
            io.observation.observedProperty.href = "urn:x-ogc:def:phenomenon:FINESCE:1.0:upstreamActivePower";
            io.observation.featureOfInterest = new FeatureOfInterest();
            io.observation.parameter = new Parameter();
            io.observation.result = new Result();
            io.observation.result.quantity = new Quantity("urn:x-ogc:def:phenomenon:IDAS:1.0:power");
            io.observation.result.quantity.uom = new Uom("kW");
            io.observation.result.quantity.value = upstreamActivePower.ToString("G", CultureInfo.InvariantCulture);

            textWriter = new Utf8StringWriter();
            ser.Serialize(textWriter, io);
            sendMessage(textWriter.ToString());

            //downstreamActivePower
            io.observation.observedProperty.href = "urn:x-ogc:def:phenomenon:FINESCE:1.0:downstreamActivePower";
            io.observation.result.quantity.value = downstreamActivePower.ToString("G", CultureInfo.InvariantCulture);

            textWriter = new Utf8StringWriter();
            ser.Serialize(textWriter, io);
            sendMessage(textWriter.ToString());

            //reactivePowerQ1
            io.observation.observedProperty.href = "urn:x-ogc:def:phenomenon:FINESCE:1.0:RP_Q1";
            io.observation.result = new Result();
            io.observation.result.quantity = new Quantity("urn:x-ogc:def:phenomenon:IDAS:1.0:reactivePower");
            io.observation.result.quantity.uom = new Uom("kVAr");
            io.observation.result.quantity.value = reactivePowerQ1.ToString("G", CultureInfo.InvariantCulture);

            textWriter = new Utf8StringWriter();
            ser.Serialize(textWriter, io);
            sendMessage(textWriter.ToString());

            //reactivePowerQ2
            io.observation.observedProperty.href = "urn:x-ogc:def:phenomenon:FINESCE:1.0:RP_Q2";
            io.observation.result = new Result();
            io.observation.result.quantity = new Quantity("urn:x-ogc:def:phenomenon:IDAS:1.0:reactivePower");
            io.observation.result.quantity.uom = new Uom("kVAr");
            io.observation.result.quantity.value = reactivePowerQ2.ToString("G", CultureInfo.InvariantCulture);

            textWriter = new Utf8StringWriter();
            ser.Serialize(textWriter, io);
            sendMessage(textWriter.ToString());

            //reactivePowerQ3
            io.observation.observedProperty.href = "urn:x-ogc:def:phenomenon:FINESCE:1.0:RP_Q3";
            io.observation.result = new Result();
            io.observation.result.quantity = new Quantity("urn:x-ogc:def:phenomenon:IDAS:1.0:reactivePower");
            io.observation.result.quantity.uom = new Uom("kVAr");
            io.observation.result.quantity.value = reactivePowerQ3.ToString("G", CultureInfo.InvariantCulture);

            textWriter = new Utf8StringWriter();
            ser.Serialize(textWriter, io);
            sendMessage(textWriter.ToString());

            //reactivePowerQ4
            io.observation.observedProperty.href = "urn:x-ogc:def:phenomenon:FINESCE:1.0:RP_Q4";
            io.observation.result = new Result();
            io.observation.result.quantity = new Quantity("urn:x-ogc:def:phenomenon:IDAS:1.0:reactivePower");
            io.observation.result.quantity.uom = new Uom("kVAr");
            io.observation.result.quantity.value = reactivePowerQ4.ToString("G", CultureInfo.InvariantCulture);

            textWriter = new Utf8StringWriter();
            ser.Serialize(textWriter, io);
            sendMessage(textWriter.ToString());

            //isConcentrator
            io.observation.observedProperty.href = "urn:x-ogc:def:phenomenon:FINESCE:1.0:isConcentrator";
            io.observation.result = new Result();
            io.observation.result.text = new Text("false");

            textWriter = new Utf8StringWriter();
            ser.Serialize(textWriter, io);
            sendMessage(textWriter.ToString());

            //meterID
            io.observation.observedProperty.href = "urn:x-ogc:def:phenomenon:FINESCE:1.0:meterID";
            io.observation.result = new Result();
            io.observation.result.text = new Text(sensorId);

            textWriter = new Utf8StringWriter();
            ser.Serialize(textWriter, io);
            sendMessage(textWriter.ToString());

            //meterType
            io.observation.observedProperty.href = "urn:x-ogc:def:phenomenon:FINESCE:1.0:meterType";
            io.observation.result = new Result();
            io.observation.result.text = new Text("METER");

            textWriter = new Utf8StringWriter();
            ser.Serialize(textWriter, io);
            sendMessage(textWriter.ToString());

        }

        public void insertLoadObservation(String sensorId,
                                            String sampleNumber,
                                           long loadTime,
                                           double downstreamActivePowerEEA,
                                            double reactiveInductivePowerEEI,
                                            double reactiveCapacitivePowerEEC,
                                            double upstreamActivePowerEUA,
                                            double reactiveInductivePowerEUI,
                                            double reactiveCapaciticePowerEUC,
                                            String tariffType,
                                            String integrationPeriodRef,
                                            long currentTime
                                            )
        {
            XmlSerializer ser = new XmlSerializer(typeof(InsertObservation));
            Utf8StringWriter textWriter = new Utf8StringWriter();

            InsertObservation io = new InsertObservation();
            io.assignedSensorId = "load:" + sensorId;
            io.observation = new Observation();
            io.observation.samplingTime = new SamplingTime();
            io.observation.samplingTime.timeInstant = new TimeInstant();
            io.observation.samplingTime.timeInstant.timePosition = new TimePosition();
            io.observation.samplingTime.timeInstant.timePosition.frame = "urn:x-ogc:def:trs:IDAS:1.0:ISO8601";
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            io.observation.samplingTime.timeInstant.timePosition.dateTime = epoch.AddSeconds(currentTime);//ho messo il currentTime. è giusto?
            io.observation.procedure = new Procedure();
            io.observation.procedure.href = "load:" + sensorId;

            //sampleNumber
            io.observation.observedProperty = new ObservedProperty();
            io.observation.observedProperty.href = "urn:x-ogc:def:phenomenon:FINESCE:1.0:sampleNumber";
            io.observation.featureOfInterest = new FeatureOfInterest();
            io.observation.parameter = new Parameter();
            io.observation.result = new Result();
            io.observation.result.text = new Text(sampleNumber);

            textWriter = new Utf8StringWriter();
            ser.Serialize(textWriter, io);
            sendMessage(textWriter.ToString());

            //loadTime
            io.observation.observedProperty.href = "urn:x-ogc:def:phenomenon:FINESCE:1.0:loadTime";
            io.observation.result = new Result();
            io.observation.result.text = new Text(loadTime.ToString());

            textWriter = new Utf8StringWriter();
            ser.Serialize(textWriter, io);
            sendMessage(textWriter.ToString());

            //downstreamActivePowerEEA
            io.observation.observedProperty.href = "urn:x-ogc:def:phenomenon:FINESCE:1.0:downstreamActivePowerEEA";
            io.observation.result = new Result();
            io.observation.result.quantity = new Quantity("urn:x-ogc:def:phenomenon:IDAS:1.0:power");
            io.observation.result.quantity.uom = new Uom("kW");
            io.observation.result.quantity.value = downstreamActivePowerEEA.ToString("G", CultureInfo.InvariantCulture);

            textWriter = new Utf8StringWriter();
            ser.Serialize(textWriter, io);
            sendMessage(textWriter.ToString());

            //reactiveInductivePowerEEI
            io.observation.observedProperty.href = "urn:x-ogc:def:phenomenon:FINESCE:1.0:reactiveInductivePowerEEI";
            io.observation.result = new Result();
            io.observation.result.quantity = new Quantity("urn:x-ogc:def:phenomenon:IDAS:1.0:reactivePower");
            io.observation.result.quantity.uom = new Uom("kVAr");
            io.observation.result.quantity.value = reactiveInductivePowerEEI.ToString("G", CultureInfo.InvariantCulture);

            textWriter = new Utf8StringWriter();
            ser.Serialize(textWriter, io);
            sendMessage(textWriter.ToString());

            //reactiveCapacitivePowerEEC
            io.observation.observedProperty.href = "urn:x-ogc:def:phenomenon:FINESCE:1.0:reactiveCapacitivePowerEEC";
            io.observation.result = new Result();
            io.observation.result.quantity = new Quantity("urn:x-ogc:def:phenomenon:IDAS:1.0:reactivePower");
            io.observation.result.quantity.uom = new Uom("kVAr");
            io.observation.result.quantity.value = reactiveCapacitivePowerEEC.ToString("G", CultureInfo.InvariantCulture);

            textWriter = new Utf8StringWriter();
            ser.Serialize(textWriter, io);
            sendMessage(textWriter.ToString());

            //upstreamActivePowerEUA
            io.observation.observedProperty.href = "urn:x-ogc:def:phenomenon:FINESCE:1.0:upstreamActivePowerEUA";
            io.observation.result = new Result();
            io.observation.result.quantity = new Quantity("urn:x-ogc:def:phenomenon:IDAS:1.0:power");
            io.observation.result.quantity.uom = new Uom("kW");
            io.observation.result.quantity.value = upstreamActivePowerEUA.ToString("G", CultureInfo.InvariantCulture);

            textWriter = new Utf8StringWriter();
            ser.Serialize(textWriter, io);
            sendMessage(textWriter.ToString());

            //reactiveInductivePowerEUI
            io.observation.observedProperty.href = "urn:x-ogc:def:phenomenon:FINESCE:1.0:reactiveInductivePowerEUI";
            io.observation.result = new Result();
            io.observation.result.quantity = new Quantity("urn:x-ogc:def:phenomenon:IDAS:1.0:reactivePower");
            io.observation.result.quantity.uom = new Uom("kVAr");
            io.observation.result.quantity.value = reactiveInductivePowerEUI.ToString("G", CultureInfo.InvariantCulture);

            textWriter = new Utf8StringWriter();
            ser.Serialize(textWriter, io);
            sendMessage(textWriter.ToString());
           
            //reactiveCapaciticePowerEUC
            io.observation.observedProperty.href = "urn:x-ogc:def:phenomenon:FINESCE:1.0:reactiveCapaciticePowerEUC";
            io.observation.result = new Result();
            io.observation.result.quantity = new Quantity("urn:x-ogc:def:phenomenon:IDAS:1.0:reactivePower");
            io.observation.result.quantity.uom = new Uom("kVAr");
            io.observation.result.quantity.value = reactiveCapaciticePowerEUC.ToString("G", CultureInfo.InvariantCulture);

            textWriter = new Utf8StringWriter();
            ser.Serialize(textWriter, io);
            sendMessage(textWriter.ToString());
           
            //tariffType
            io.observation.observedProperty.href = "urn:x-ogc:def:phenomenon:FINESCE:1.0:tariffType";
            io.observation.result = new Result();
            io.observation.result.text = new Text(tariffType);

            textWriter = new Utf8StringWriter();
            ser.Serialize(textWriter, io);
            sendMessage(textWriter.ToString());
            
            //integrationPeriodRef
            io.observation.observedProperty.href = "urn:x-ogc:def:phenomenon:FINESCE:1.0:integrationPeriodRef";
            io.observation.result = new Result();
            io.observation.result.text = new Text(integrationPeriodRef);

            textWriter = new Utf8StringWriter();
            ser.Serialize(textWriter, io);
            sendMessage(textWriter.ToString());

            //currentTime
            io.observation.observedProperty.href = "urn:x-ogc:def:phenomenon:FINESCE:1.0:currentTime";
            io.observation.result = new Result();
            io.observation.result.text = new Text(currentTime.ToString());

            textWriter = new Utf8StringWriter();
            ser.Serialize(textWriter, io);
            sendMessage(textWriter.ToString());

            //meterID
            io.observation.observedProperty.href = "urn:x-ogc:def:phenomenon:FINESCE:1.0:meterID";
            io.observation.result = new Result();
            io.observation.result.text = new Text(sensorId);

            textWriter = new Utf8StringWriter();
            ser.Serialize(textWriter, io);
            sendMessage(textWriter.ToString());

            //meterType
            io.observation.observedProperty.href = "urn:x-ogc:def:phenomenon:FINESCE:1.0:meterType";
            io.observation.result = new Result();
            io.observation.result.text = new Text("LOAD");

            textWriter = new Utf8StringWriter();
            ser.Serialize(textWriter, io);
            sendMessage(textWriter.ToString());

            //isConcentrator
            io.observation.observedProperty.href = "urn:x-ogc:def:phenomenon:FINESCE:1.0:isConcentrator";
            io.observation.result = new Result();
            io.observation.result.text = new Text("false");

            textWriter = new Utf8StringWriter();
            ser.Serialize(textWriter, io);
            sendMessage(textWriter.ToString());           
         
        }

        public String sendMessage(string message)
        {
            //console.writeline(message);
            //return "";
            WebRequest wr = WebRequest.Create(serviceUrl);
            wr.ContentType = "application/xml";
            wr.Method = "POST";
            Stream content = wr.GetRequestStream();
            byte[] byteArray = Encoding.UTF8.GetBytes (message);
            content.Write(byteArray, 0, byteArray.Length);
            content.Close();
            WebResponse response = wr.GetResponse();

            //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            
            Stream responseContent = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseContent);
            string responseFromServer = reader.ReadToEnd();
            //Console.WriteLine(responseFromServer);
            reader.Close();
            responseContent.Close();
            response.Close();
            //XmlSerializer serializer=new XmlSerializer(typeof());
            return responseFromServer;

      }

    }
}
