using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using 

IAM2IDAS.SOS;

namespace IAM2IDAS.test
{
    public class TestParsing
    {

        public static RegisterSensor loadFile()
        {
            XmlSerializer ser = new XmlSerializer(typeof(RegisterSensor));
            StreamReader fs = new StreamReader(new FileStream("test/sensorRegistration.xml",FileMode.OpenOrCreate));
            //String s;
            //while ((s=fs.ReadLine())!=null)
            //{
            //    Console.WriteLine(s);
            //}
            RegisterSensor rs = (RegisterSensor)ser.Deserialize(fs);
            fs.Close();
            return rs;
            //XmlDocument doc = new XmlDocument();
            //doc.Load("test/sensorRegistration.xml");
            //System.Console.Write(doc.InnerXml);
        }


   
    }
}
