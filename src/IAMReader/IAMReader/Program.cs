using System;
using System.Globalization;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Xml.Serialization;
using System.Net.Sockets;
using System.Configuration;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.ServiceProcess;
using System.Xml;
using Gurux.Common;
using Gurux.Serial;
using Gurux.DLMS;
using Gurux.DLMS.ManufacturerSettings;
using Gurux.DLMS.Objects;
using Gurux.Net;
using IAM2IDAS;

using System.Web.Script.Serialization;

namespace IAMReader
{
    class Program
    {
		#region Nested classes to support running as service
		public const string ServiceName = "IAMReader";

		public class Service : ServiceBase
		{
			public Service()
			{
				ServiceName = ServiceName;
			}

			protected override void OnStart(string[] args)
			{
				Program.Start(args);
			}

			protected override void OnStop()
			{
				Program.Stop();
			}
		}
		#endregion

		static string sitesMapPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\SitesMap.xml";
		static string logFileName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\LogFile.txt";
		static string rotatedLogFileName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\RotatedLogFile.txt";
        static long maxLogFileSize = 100 * (1024 * 1024); // n * MBytes
		static string readingsXmlFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Readings";
		static string profileXmlFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Profile";
		static volatile bool running = false;
		static String[] args;
		static TextWriter logFile = null;
		static bool keyAvailable = false;

        static string IAMServer = "127.0.0.1"; //address of IAM server

        static void ShowHelp()
        {
            Console.WriteLine("IAMReader v. " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(3) + " (c) 2014");
            Console.WriteLine("");
//            Console.WriteLine("IAMReader /m=lgz /host=1.1.1.1 /p=4059 [/s=]");
			Console.WriteLine(" /a=\t Authentication (None, Low, High).");
			Console.WriteLine(" /host=\t host name.");
			Console.WriteLine(" /l\t Loop time (-1: single run, 0: continuous, 1: 1 minute, 2: 2 minutes, etc...");
			Console.WriteLine(" /m=\t manufacturer identifier.");
			Console.WriteLine(" /p=\t port number or name (Example: 1000).");
			Console.WriteLine(" /pw=\t Password for authentication.");
			Console.WriteLine(" /s=\t start protocol (IEC or DLMS).");
			Console.WriteLine(" /sm=\t SitesMap file.");
			Console.WriteLine(" /sp=\t serial port.");
			Console.WriteLine(" /is=\t IAM server.");
			Console.WriteLine(" /t\t Trace messages.");
			Console.WriteLine("Example:");
            Console.WriteLine("Read LG device using TCP/IP connection.");
            Console.WriteLine("IAMReader /m=lgz /host=1.1.1.1 /p=4059");
            Console.WriteLine("Read LG device using serial port connection.");
            Console.WriteLine("IAMReader /m=lgz /sp=COM1 /s=DLMS");
        }

        static void Trace(TextWriter writer, string text)
        {
            try
            {
                string msg = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fff   ") + text;
                writer.Write(msg);
                Console.Write(msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Trace error: " + ex.Message);
            }
        }
        
        static void TraceLine(TextWriter writer, string text)
        {
            try
            {
                string msg = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fff   ") + text;
                writer.WriteLine(msg);
                Console.WriteLine(msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine("TraceLine error: " + ex.Message);
            }
        }


		static void PublishReading(IAM.DLMSSite site, DateTime timestamp, double activePowerP, double activePowerN, double reactivePowerQ1, double reactivePowerQ2, double reactivePowerQ3, double reactivePowerQ4, TextWriter logFile)
        {
            string ts = timestamp.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
			TraceLine(logFile, "Avg. Active Power @t " + ts + " is " + activePowerP.ToString("0.##") + " W");

			DataSet ds = new DataSet("MeterData");
			ds.Tables.Add("Reading");
			ds.Tables[0].Columns.Add("Timestamp", typeof(DateTime));
			ds.Tables[0].Columns.Add("Site", typeof(String));
			ds.Tables[0].Columns.Add("Period", typeof(int));
			ds.Tables[0].Columns.Add("UpstreamActivePower", typeof(Double));
			ds.Tables[0].Columns.Add("DownstreamActivePower", typeof(Double));
			ds.Tables[0].Columns.Add("Q1ReactivePower", typeof(Double));
			ds.Tables[0].Columns.Add("Q2ReactivePower", typeof(Double));
			ds.Tables[0].Columns.Add("Q3ReactivePower", typeof(Double));
			ds.Tables[0].Columns.Add("Q4ReactivePower", typeof(Double));
			DataRow row = ds.Tables[0].NewRow();
			row["Timestamp"] = timestamp;
			row["Site"] = site.Name;
			row["Period"] = site.DataProcessing.PowerPeriod;
			row["UpstreamActivePower"] = activePowerN;
			row["DownstreamActivePower"] = activePowerP;
			row["Q1ReactivePower"] = reactivePowerQ1;
			row["Q2ReactivePower"] = reactivePowerQ2;
			row["Q3ReactivePower"] = reactivePowerQ3;
			row["Q4ReactivePower"] = reactivePowerQ4;
			ds.Tables[0].Rows.Add(row);
			ds.WriteXml(readingsXmlFile + site.Name + ".xml");

		}

		static void PublishEnergy(IAM.DLMSSite site, DateTime timestamp, double activeEnergyP, double activeEnergyN, double reactiveEnergyQ1, double reactiveEnergyQ2, double reactiveEnergyQ3, double reactiveEnergyQ4, double ratio, TextWriter logFile)
		{
			DataSet ds = new DataSet("MeterData");
			ds.Tables.Add("Reading");
			ds.Tables[0].Columns.Add("Timestamp", typeof(DateTime));
			ds.Tables[0].Columns.Add("Site", typeof(String));
			ds.Tables[0].Columns.Add("Period", typeof(int));
			ds.Tables[0].Columns.Add("UpstreamActiveEnergy", typeof(Double));
			ds.Tables[0].Columns.Add("DownstreamActiveEnergy", typeof(Double));
			ds.Tables[0].Columns.Add("Q1ReactiveEnergy", typeof(Double));
			ds.Tables[0].Columns.Add("Q2ReactiveEnergy", typeof(Double));
			ds.Tables[0].Columns.Add("Q3ReactiveEnergy", typeof(Double));
			ds.Tables[0].Columns.Add("Q4ReactiveEnergy", typeof(Double));
			ds.Tables[0].Columns.Add("Ratio", typeof(Double));
			DataRow row = ds.Tables[0].NewRow();
			row["Timestamp"] = timestamp;
			row["Site"] = site.Name;
			row["Period"] = site.DataProcessing.EnergyPeriod;
			row["UpstreamActiveEnergy"] = activeEnergyN;
			row["DownstreamActiveEnergy"] = activeEnergyP;
			row["Q1ReactiveEnergy"] = reactiveEnergyQ1;
			row["Q2ReactiveEnergy"] = reactiveEnergyQ2;
			row["Q3ReactiveEnergy"] = reactiveEnergyQ3;
			row["Q4ReactiveEnergy"] = reactiveEnergyQ4;
			row["Ratio"] = ratio; 
			ds.Tables[0].Rows.Add(row);
			ds.WriteXml(profileXmlFile + site.Name + ".xml");

			StringWriter sw = new StringWriter();
			ds.WriteXml(sw);
			string data = sw.ToString();
			data = data.Replace("\r\n", "");
            // data = data + "$";
			postDataToServer(IAMServer, data);
		}


		static void PublishPower(IAM.DLMSSite site, DateTime timestamp, double activePowerP, double activePowerN, double reactivePowerQ1, double reactivePowerQ2, double reactivePowerQ3, double reactivePowerQ4, double ratio, TextWriter logFile)
		{
            //here we prepare the xml to send to the IAM server

            DataSet ds = new DataSet("MeterData");
			ds.Tables.Add("Reading");
			ds.Tables[0].Columns.Add("Timestamp", typeof(DateTime));
			ds.Tables[0].Columns.Add("Site", typeof(String));
			ds.Tables[0].Columns.Add("Period", typeof(int));
			ds.Tables[0].Columns.Add("UpstreamActivePower", typeof(Double));
			ds.Tables[0].Columns.Add("DownstreamActivePower", typeof(Double));
			ds.Tables[0].Columns.Add("Q1ReactivePower", typeof(Double));
			ds.Tables[0].Columns.Add("Q2ReactivePower", typeof(Double));
			ds.Tables[0].Columns.Add("Q3ReactivePower", typeof(Double));
			ds.Tables[0].Columns.Add("Q4ReactivePower", typeof(Double));
			ds.Tables[0].Columns.Add("Ratio", typeof(Double));
			DataRow row = ds.Tables[0].NewRow();
			row["Timestamp"] = timestamp;
			row["Site"] = site.Name;
			row["Period"] = site.DataProcessing.PowerPeriod;
			row["UpstreamActivePower"] = activePowerN;
			row["DownstreamActivePower"] = activePowerP;
			row["Q1ReactivePower"] = reactivePowerQ1;
			row["Q2ReactivePower"] = reactivePowerQ2;
			row["Q3ReactivePower"] = reactivePowerQ3;
			row["Q4ReactivePower"] = reactivePowerQ4;
			row["Ratio"] = ratio;
			ds.Tables[0].Rows.Add(row);
			ds.WriteXml(readingsXmlFile + site.Name + ".xml");

			StringWriter sw = new StringWriter();
			ds.WriteXml(sw);
			string data = sw.ToString();
			data = data.Replace("\r\n", "");
            //
			postDataToServer(IAMServer, data);
            //


			// To be removed: for testing only
//			JavaScriptSerializer TheSerializer = new JavaScriptSerializer();
//			var TheJson = TheSerializer.Serialize(ds.Tables[0].Rows[0]);
			var list = new List<Dictionary<string, object>>();

			foreach (DataRow r in ds.Tables[0].Rows)
			{
				var dict = new Dictionary<string, object>();

				foreach (DataColumn col in ds.Tables[0].Columns)
				{
					dict[col.ColumnName] = r[col];
				}
				list.Add(dict);
			}
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			string theJson = serializer.Serialize(list);
			theJson = Regex.Replace(theJson, @"\""\\/Date\((-?\d+)\)\\/\""", "new Date($1)");
			System.IO.File.WriteAllText(readingsXmlFile + site.Name + ".json", theJson);
		}

		static void postDataToServer(string host, string data)
		{
            // this function sends the data to the IDAS GENERIC ENABLER (server)
// use here your IPs and your keys
            IDASClient c = new IDASClient("http://1.1.1.1:8002/idas/sml?apikey=place_here_your_secretkey");
            //register meter object first in order to make an entity id meter:0125
            c.registerMeter("0999");
            //register load object in order to make an entity id load:0125
            c.registerLoad("0999");

            //ONLY after registration it is possible to insert observations
            c.insertMeterObservation("0999", (long)1415888468, 12.1, 12.2, 13.1, 13.2, 13.3, 13.4);
            c.insertLoadObservation("0999", "2", (long)1415888468, 23.1, 23.2, 23.3, 23.4, 23.5, 23.6, "T1", "IP1", (long)1415888468);
            //TraceLine(logFile, string.Format("IAM posted data to ISDAS.");

			// now sending data to IAM Server
            // alternatively, the same message can be sent to ORION here
            Socket client = null;
			try
			{
				string[] hostParam = host.Split(new char [] { ':' });

				int port = 8888; // ex port 8888
				if (hostParam.Length > 1)
				{
					int.TryParse(hostParam[1], out port);
				}

				IPAddress ipAddress;
				if (!IPAddress.TryParse(host, out ipAddress)) //host can be supplied through the command line
				{
					IPHostEntry ipHostInfo = Dns.GetHostEntry(hostParam[0]);
					ipAddress = ipHostInfo.AddressList[0];
				}
				IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

				// Create a TCP/IP socket.
				client = new Socket(AddressFamily.InterNetwork,
					SocketType.Stream, ProtocolType.Tcp);

				// Connect to the remote endpoint.
				client.BeginConnect(remoteEP,
					new AsyncCallback(ConnectCallback), Tuple.Create(client, data));
			}
			catch (Exception ex)
			{
				TraceLine(logFile, ex.Message);
				if (client != null)
				{
					// Release the socket.
					client.Shutdown(SocketShutdown.Both);
					client.Close();
				}
			}
		}

		private static void ConnectCallback(IAsyncResult ar)
		{
			Tuple<Socket, string> state = (Tuple<Socket, string>)ar.AsyncState;

			// Retrieve the socket from the state object.
			Socket client = (Socket)state.Item1;
			string data = state.Item2;
			
			try
			{

				// Complete the connection.
				client.EndConnect(ar);

				// Convert the string data to byte data using ASCII encoding.
				byte[] byteData = Encoding.ASCII.GetBytes(data);

				// Begin sending the data to the remote device.
				client.BeginSend(byteData, 0, byteData.Length, 0,
					new AsyncCallback(SendCallback), Tuple.Create(client, byteData));
			}
			catch (Exception e)
			{
				TraceLine(logFile, e.Message);

				// Release the socket.
				if (client.Connected)
				{
					client.Shutdown(SocketShutdown.Both);
					client.Close();
				}
			}
		}

		private static void SendCallback(IAsyncResult ar)
		{
			Tuple<Socket, byte[]> state = (Tuple<Socket, byte[]>)ar.AsyncState;

			// Retrieve the socket from the state object.
			Socket client = (Socket)state.Item1;

			// Retrieve the byte data
			byte[] byteData = (byte[])state.Item2;

			try
			{
				// Complete sending the data to the remote device.
				int bytesSent = client.EndSend(ar);

				// Check if all data sent
				if (bytesSent < byteData.Length)
				{
					// Begin sending the reminder of data to the remote device.
					client.BeginSend(byteData, bytesSent, byteData.Length - bytesSent, 0,
						new AsyncCallback(SendCallback), Tuple.Create(client, byteData));
				}
				else
				{
					TraceLine(logFile, string.Format("Posted {0} bytes to IAM server: {1}.", bytesSent, client.RemoteEndPoint.ToString()));

					// Create the state object for receive.
					StateObject stateRx = new StateObject();
					stateRx.workSocket = client;

					// Begin receiving the data from the remote device.
					client.BeginReceive(stateRx.buffer, 0, StateObject.BufferSize, 0,
						new AsyncCallback(ReceiveCallback), stateRx);
					
				}
			}
			catch (Exception e)
			{
				TraceLine(logFile, e.Message);

				if (client.Connected)
				{
					// Release the socket.
					client.Shutdown(SocketShutdown.Both);
					client.Close();
				}
			}
		}

		private static void ReceiveCallback(IAsyncResult ar)
		{
			// Retrieve the state object and the client socket 
			// from the asynchronous state object.
			StateObject state = (StateObject)ar.AsyncState;
			Socket client = state.workSocket;

			try
			{

				// Read data from the remote device.
				int bytesRead = client.EndReceive(ar);

				if (bytesRead > 0)
				{
					// There might be more data, so store the data received so far.
					state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
					TraceLine(logFile, string.Format("Reply from IAM server: '{0}'.", state.sb.ToString()));

//					// Get the rest of the data.
//					client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
//						new AsyncCallback(ReceiveCallback), state);
//				}
//				else
//				{
					// Release the socket.
					client.Shutdown(SocketShutdown.Both);
					client.Close();
				}
			}
			catch (Exception e)
			{
				TraceLine(logFile, e.Message);
				if (client.Connected)
				{
					// Release the socket.
					client.Shutdown(SocketShutdown.Both);
					client.Close();
				}
			}
		}


		static void PublishProfile(IAM.DLMSSite site, DateTime timestamp, StringBuilder sb, TextWriter logFile)
        {
            TraceLine(logFile, "Read profile:");
            string [] split;

            split = sb.ToString().Split(new string [] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (string s in split)
            {
                string msg = new string(' ', 26) + s;
                logFile.WriteLine(msg);
                Console.WriteLine(msg);
            }

			DataSet ds = new DataSet("MeterData");
			ds.Tables.Add("Profile");
			ds.Tables[0].Columns.Add("Timestamp", typeof(DateTime));
			ds.Tables[0].Columns.Add("Site", typeof(String));
			ds.Tables[0].Columns.Add("Period", typeof(int));
			ds.Tables[0].Columns.Add("UpstreamActivePower", typeof(Double));
			ds.Tables[0].Columns.Add("DownstreamActivePower", typeof(Double));
			ds.Tables[0].Columns.Add("Q1ReactivePower", typeof(Double));
			ds.Tables[0].Columns.Add("Q2ReactivePower", typeof(Double));
			ds.Tables[0].Columns.Add("Q3ReactivePower", typeof(Double));
			ds.Tables[0].Columns.Add("Q4ReactivePower", typeof(Double));

			if (split.Length < 2)
				return;

			for (int i = 1; i < split.Length; i++)
			{
				string [] s1 = split[i - 1].Split(new char [] {'|'});
				string [] s2 = split[i].Split(new char[] { '|' });

				double activePowerP1 = 0;
				double activePowerN1 = 0;
				double activePowerP2 = 0;
				double activePowerN2 = 0;

				double.TryParse(s1[2], out activePowerP1);
				double.TryParse(s2[2], out activePowerP2);
				double.TryParse(s1[3], out activePowerN1);
				double.TryParse(s2[3], out activePowerN2);
				DateTime dt = DateTime.Now;

				if (DateTime.TryParse(s2[0], out dt))
				{ 
					DataRow row = ds.Tables[0].NewRow();
					row["Timestamp"] = dt;
					row["Site"] = site.Name;
					row["Period"] = site.DataProcessing.PowerPeriod;
					row["UpstreamActivePower"] = activePowerN2 - activePowerN1;
					row["DownstreamActivePower"] = activePowerP2 - activePowerP1;
					row["Q1ReactivePower"] = 0;
					row["Q2ReactivePower"] = 0;
					row["Q3ReactivePower"] = 0;
					row["Q4ReactivePower"] = 0;
					ds.Tables[0].Rows.Add(row);
				}
			}

			if (ds.Tables[0].Rows.Count > 0)
				ds.WriteXml(profileXmlFile + site.Name + ".xml");
        }

		static void Main(string[] cmdLineArgs)
		{
            // 2 modalities are allowed, running as service or as instance of the application
			
            if (!Environment.UserInteractive) // running as service
				using (var service = new Service())
					ServiceBase.Run(service);
            else // running as console app
			{	
				args = cmdLineArgs;
				running = true;
				Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e)
				{
					e.Cancel = true;
					keyAvailable = true;
				};

				MainProcessing(null);
			}
		}


		private static void MainProcessing(object state)
		{
			IGXMedia media = null;
			GXCommunicatation comm = null;
			Gurux.Net.GXNet net = null;

			if ((!Environment.UserInteractive) && (args.Length == 0))	// when in service mode, get arguments from commandline if failed from onStart method
				args = Environment.CommandLine.Split(new char[] { ' ' });

			if (!Environment.UserInteractive && !running)
				return;

			try
			{
				logFile = new StreamWriter(File.Open(logFileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
				logFile.WriteLine("\n");

				//Handle command line parameters.
				String id = "lgz", host = "", port = "4095", pw = "";
				String lt = "-1";
				bool trace = false, iec = true;
				int loopTime = -1;
				Authentication auth = Authentication.None;
				foreach (string it in args)
				{
					String item = it.Trim().ToLower();
					if (string.Compare(item, "/u", true) == 0)//Update
					{
						//Get latest manufacturer settings from Gurux web server.
						GXManufacturerCollection.UpdateManufactureSettings();
					}
					else if (item.StartsWith("/m="))//Manufacturer
					{
						id = item.Replace("/m=", "");
					}
					else if (item.StartsWith("/host=")) //Host
					{
						host = item.Replace("/host=", "");
					}
					else if (item.StartsWith("/p="))// TCP/IP Port
					{
						media = new Gurux.Net.GXNet();
						port = item.Replace("/p=", "");
					}
					else if (item.StartsWith("/sp="))//Serial Port
					{
						port = item.Replace("/sp=", "");
						media = new GXSerial();
					}
					else if (item.StartsWith("/t"))//Are messages traced.
					{
						trace = true;
					}
					else if (item.StartsWith("/s="))//Start
					{
						String tmp = item.Replace("/s=", "");
						iec = string.Compare(tmp, "dlms", true) != 0;
					}
					else if (item.StartsWith("/a="))//Authentication
					{
						auth = (Authentication)Enum.Parse(typeof(Authentication), it.Trim().Replace("/a=", ""));
					}
					else if (item.StartsWith("/pw="))//Password
					{
						pw = it.Trim().Replace("/pw=", "");
					}
					else if (item.StartsWith("/l="))// loop time
					{
						lt = item.Replace("/l=", "");
					}
					else if (item.StartsWith("/h"))// help
					{
						ShowHelp();
						return;
					}
					else if (item.StartsWith("/sm=")) //sitemap
					{
						sitesMapPath = it.Trim().Replace("/sm=", "");
					}
					else if (item.StartsWith("/is=")) //IAM server 
					{
						IAMServer = it.Trim().Replace("/is=", "");
					}
				}
				if (media == null)
					media = new Gurux.Net.GXNet();

				int.TryParse(lt, out loopTime);

				Version version = Assembly.GetExecutingAssembly().GetName().Version;
				
				if (!Environment.UserInteractive)
					TraceLine(logFile, string.Format("IAM Reader OPEN SOURCE EXAMPLE Service ver. {0} started with loop time={1} min.", version.ToString(3), loopTime));
				else
					TraceLine(logFile, string.Format("IAM Reader OPEN SOURCE EXAMPLE ver. {0} started with loop time={1} min.", version.ToString(3), loopTime));

				logFile.Flush();

				//Initialize connection settings.
				if (media is GXSerial)
				{
					GXSerial serial = media as GXSerial;
					serial.PortName = port;
					if (iec)
					{
						serial.BaudRate = 300;
						serial.DataBits = 7;
						serial.Parity = System.IO.Ports.Parity.Even;
						serial.StopBits = System.IO.Ports.StopBits.One;
					}
					else
					{
						serial.BaudRate = 9600;
						serial.DataBits = 8;
						serial.Parity = System.IO.Ports.Parity.None;
						serial.StopBits = System.IO.Ports.StopBits.One;
					}
				}
				else if (media is GXNet)
				{
					/*Gurux.Net.GXNet*/
					net = media as GXNet;
					net.Port = Convert.ToInt32(port);
					net.HostName = host;
					net.Protocol = Gurux.Net.NetworkType.Tcp;
				}
				else
				{
					throw new Exception("Unknown media type.");
				}

				//Update manufacturer debended settings.
				GXManufacturerCollection Manufacturers = new GXManufacturerCollection();
				GXManufacturerCollection.ReadManufacturerSettings(Manufacturers);
				GXManufacturer man = Manufacturers.FindByIdentification(id);
				if (man == null)
				{
					throw new Exception("Unknown manufacturer: " + id);
				}
				GXDLMSClient dlms = new GXDLMSClient();
				//Update Obis code list so we can get right descriptions to the objects.
				dlms.ObisCodes = man.ObisCodes;

				List<IAM.DLMSSite> DlmsSites = null;
				IAM.ArrayOfDLMSSite aods = null;

				XmlSerializer y = new XmlSerializer(typeof(IAM.ArrayOfDLMSSite));
				if (File.Exists(sitesMapPath))
				{
					try
					{
						using (Stream stream = File.Open(sitesMapPath, FileMode.Open))
						{
							TraceLine(logFile, "Get configured sites map from file.");
							aods = y.Deserialize(stream) as IAM.ArrayOfDLMSSite;
							DlmsSites = aods.DLMSSite;
							stream.Close();
						}

					}
					catch (Exception ex)
					{
						throw ex;
					}
				}
				else
				{
					TraceLine(logFile, "Error. SitesMap file not found.");
					return;
				}

				List<Type> extraTypes = new List<Type>(Gurux.DLMS.GXDLMSClient.GetObjectTypes());
				extraTypes.Add(typeof(GXDLMSAttributeSettings));
				extraTypes.Add(typeof(GXDLMSAttribute));
				//TraceLine(logFile, "Memory32: " + GC.GetTotalMemory(true));
				XmlSerializer x = new XmlSerializer(typeof(GXDLMSObjectCollection), extraTypes.ToArray());
				//You can save association view, but make sure that it is not change.
				//Save Association view to the cache so it is not needed to retrieve every time.
				extraTypes.Clear();

				foreach (IAM.DLMSSite site in DlmsSites) //
				{
					site.DataProcessing.EnergyPeriodDateTime = DateTime.MinValue;
					site.DataProcessing.PowerPeriodDateTime = DateTime.MinValue;
				}

				//bool keyAvailable = false;
				DateTime startTime;
				FileInfo infoFileSite = new FileInfo(sitesMapPath);
				DateTime lastWriteMapFile = infoFileSite.LastWriteTime;

				do
				{
					try
					{
						FileInfo f = new FileInfo(logFileName); //output log file
						if (f.Length > maxLogFileSize)
						{
							logFile.Flush();
							logFile.Close();
							File.Delete(rotatedLogFileName);
							File.Move(logFileName, rotatedLogFileName);
							logFile = new StreamWriter(File.Open(logFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite));
						}

						infoFileSite.Refresh();
						if (infoFileSite.LastWriteTime != lastWriteMapFile)
						{
							try
							{
								using (Stream stream = File.Open(sitesMapPath, FileMode.Open))
								{
									TraceLine(logFile, "Get updated sites map from file.");
									aods = y.Deserialize(stream) as IAM.ArrayOfDLMSSite;
									DlmsSites = aods.DLMSSite;
									stream.Close();

									lastWriteMapFile = infoFileSite.LastWriteTime;
								}

							}
							catch (Exception ex)
							{
								throw ex;
							}

						}
						
						startTime = DateTime.Now;
						TraceLine(logFile, "Start new iteration");
						foreach (IAM.DLMSSite site in DlmsSites) //cycle for each remote meter (listed in the sitemap.xml)
						{
							TraceLine(logFile, "Now processing: " + site.Name); //current remote meter
							if (!site.Enabled)
							{
								TraceLine(logFile, "Disabled: skip to next");  
								continue;
							}
							try
							{
								host = site.Device.Host; //ip address of the current remote meter
								port = site.Device.Port;
								if (site.Device.Manufacturer != "")
									id = site.Device.Manufacturer.ToLower();
								if (site.Device.Protocol != "")
									iec = site.Device.Protocol.ToLower() == "dlms";
								if (media is GXNet)
								{
									/*Gurux.Net.GXNet*/
									net = media as GXNet;
									net.Port = Convert.ToInt32(port);
									net.HostName = host;
									net.Protocol = Gurux.Net.NetworkType.Tcp;
								}
								comm = new GXCommunicatation(dlms, media, iec, auth, pw);
								comm.Trace = trace;
								TraceLine(logFile, "Initializing Network connection.");
								comm.InitializeConnection(man);
							}
							catch (Exception ex)
							{
								TraceLine(logFile, "Error. " + ex.Message);
								if (comm != null)
								{
									comm.Close();
								}
								continue;
							}
							GXDLMSObjectCollection objects = null;

							string meterPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +  "\\" +host.Replace('.', '_') + "_" + port.ToString() + ".xml";

							if (File.Exists(meterPath)) //list of OBIS objects for remote meter
							{
								try
								{
									using (Stream stream = File.Open(meterPath, FileMode.Open))
									{
										TraceLine(logFile, "Get available objects from the cache.");
										objects = x.Deserialize(stream) as GXDLMSObjectCollection;
										stream.Close();
									}

								}
								catch (Exception ex)
								{
									if (File.Exists(meterPath))
									{
										File.Delete(meterPath);
									}
									throw ex;
								}
							}
							else
							{
								TraceLine(logFile, "Get available objects from the device.");
								objects = comm.GetAssociationView();
								
								// Save to file 
								try
								{
									using (Stream stream = File.Open(meterPath, FileMode.Create))
									{
										TextWriter writer = new StreamWriter(stream);
										x.Serialize(writer, objects);
										writer.Close();
										stream.Close();
									}
									TraceLine(logFile, "Available objects saved to the cache.");

								}
								catch (Exception ex)
								{
									if (File.Exists(meterPath))
									{
										File.Delete(meterPath);
									}
									throw ex;
								}

							}

                            // start remote meter reading
							try 
							{
								int EPeriod = 15;	// energy integration period
								if (site.DataProcessing.EnergyPeriod != 0)
									EPeriod = site.DataProcessing.EnergyPeriod;
								else
									site.DataProcessing.EnergyPeriod = EPeriod;

								//if (EPeriod <= 0)
								//    EPeriod = 15;

								int PPeriod = 15;	// power integration period
								if (site.DataProcessing.PowerPeriod != 0)
									PPeriod = site.DataProcessing.PowerPeriod;
								else
									site.DataProcessing.PowerPeriod = PPeriod;

								//if (PPeriod <= 0)
								//    PPeriod = 15;

								DateTime now = DateTime.Now;
								DateTime nowRnd = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute / EPeriod * EPeriod, 0);

								if (site.DataProcessing.EnergyPeriodDateTime == DateTime.MinValue)
								{
									site.DataProcessing.EnergyPeriodDateTime = now;
									TraceLine(logFile, "Initialize energy period from null.");
								}

								// default OBIS codes for energies
								string AEP = "1.0.1.8.0.255";
								string AEM = "1.0.2.8.0.255";	
								string REP = "1.0.3.8.0.255";
								string REM = "1.0.4.8.0.255";

								if (!string.IsNullOrEmpty(site.ArrayOfOBIS.DataObjects.ActiveEnergyP)) //plus = absorbed energy
									AEP = site.ArrayOfOBIS.DataObjects.ActiveEnergyP;
								if (!string.IsNullOrEmpty(site.ArrayOfOBIS.DataObjects.ActiveEnergyM)) //minus = supplied energy
									AEM = site.ArrayOfOBIS.DataObjects.ActiveEnergyM;
                                if (!string.IsNullOrEmpty(site.ArrayOfOBIS.DataObjects.ReactiveEnergyP)) //plus = absorbed energy
									REP = site.ArrayOfOBIS.DataObjects.ReactiveEnergyP;
                                if (!string.IsNullOrEmpty(site.ArrayOfOBIS.DataObjects.ReactiveEnergyM)) //minus = supplied energy
									REM = site.ArrayOfOBIS.DataObjects.ReactiveEnergyM;

								GXDLMSObject aepObj = objects.FindByLN(ObjectType.Register, AEP);
								if (aepObj == null)
									throw new System.InvalidOperationException(string.Format("Error: Active Energy Positive Object not defined for site {0}.", site.Name));
								GXDLMSObject aemObj = objects.FindByLN(ObjectType.Register, AEM);
								if (aemObj == null)
									throw new System.InvalidOperationException(string.Format("Active Energy Negative Object not defined for site {0}.", site.Name));
								GXDLMSObject repObj = objects.FindByLN(ObjectType.Register, REP);
								if (repObj == null)
									throw new System.InvalidOperationException(string.Format("Reactive Energy Positive Object not defined for site {0}.", site.Name));
								GXDLMSObject remObj = objects.FindByLN(ObjectType.Register, REM);
								if (remObj == null)
									throw new System.InvalidOperationException(string.Format("Reactive Energy Negative Object not defined for site {0}.", site.Name));
								
								Regex regex = new Regex(@"^Scaler:(.*) Unit:(.*)$");
								double scaler = 1;

								object value = comm.Read(aepObj, 2);
								string scalerAttr = (string)comm.Read(aepObj, 3);
								Match match = regex.Match(scalerAttr);
								if (match.Groups.Count >= 2)
									scaler = double.Parse(match.Groups[1].Value);
								double ActiveEnergyP = double.Parse(value.ToString()) * scaler;
								TraceLine(logFile, "Positive Active Energy reading completed.");

								value = comm.Read(aemObj, 2);
								scalerAttr = (string)comm.Read(aemObj, 3);
								match = regex.Match(scalerAttr);
								if (match.Groups.Count >= 2)
									scaler = double.Parse(match.Groups[1].Value);
								double ActiveEnergyM = double.Parse(value.ToString()) * scaler;
								TraceLine(logFile, "Negative Active Energy reading completed.");

								value = comm.Read(repObj, 2);
								scalerAttr = (string)comm.Read(repObj, 3);
								match = regex.Match(scalerAttr);
								if (match.Groups.Count >= 2)
									scaler = double.Parse(match.Groups[1].Value);
								double ReactiveEnergyP = double.Parse(value.ToString()) * scaler;
								TraceLine(logFile, "Positive Reactive Energy reading completed.");

								value = comm.Read(remObj, 2);
								scalerAttr = (string)comm.Read(remObj, 3);
								match = regex.Match(scalerAttr);
								if (match.Groups.Count >= 2)
									scaler = double.Parse(match.Groups[1].Value);
								double ReactiveEnergyM = double.Parse(value.ToString()) * scaler;
								TraceLine(logFile, "Negative Reactive Energy reading completed.");

                                // end of remote meter reading

								if (site.DataProcessing.ActiveEnergyPStart == 0)
									site.DataProcessing.ActiveEnergyPStart = ActiveEnergyP;
								site.DataProcessing.ActiveEnergyPCurrent = ActiveEnergyP;

								if (site.DataProcessing.ActiveEnergyMStart == 0)
									site.DataProcessing.ActiveEnergyMStart = ActiveEnergyM;
								site.DataProcessing.ActiveEnergyMCurrent = ActiveEnergyM;

								if (site.DataProcessing.ReactiveEnergyPStart == 0)
									site.DataProcessing.ReactiveEnergyPStart = ReactiveEnergyP;
								site.DataProcessing.ReactiveEnergyPCurrent = ReactiveEnergyP;

								if (site.DataProcessing.ReactiveEnergyMStart == 0)
									site.DataProcessing.ReactiveEnergyMStart = ReactiveEnergyM;
								site.DataProcessing.ReactiveEnergyMCurrent = ReactiveEnergyM;

								DateTime tStart = new DateTime(site.DataProcessing.EnergyPeriodDateTime.Year, site.DataProcessing.EnergyPeriodDateTime.Month, site.DataProcessing.EnergyPeriodDateTime.Day, site.DataProcessing.EnergyPeriodDateTime.Hour, site.DataProcessing.EnergyPeriodDateTime.Minute / EPeriod * EPeriod, 0);
								TimeSpan ts = now.Subtract(tStart);

								if (EPeriod > 0)		// if energy recording enabled
								{
									if (((ts.Hours * 60) + ts.Minutes) >= EPeriod)  // if end of period
									{
										ts = now.Subtract(site.DataProcessing.EnergyPeriodDateTime);
										double ratio = (double)((ts.Hours * 3600) + (ts.Minutes * 60) + ts.Seconds) / (EPeriod * 60);
										if (ratio > 0.9) ratio = 1;
										ratio = Math.Round(ratio, 2);
										TraceLine(logFile, string.Format("End of integration energy period (ratio = {0})", ratio));

										double deltaActiveEnergyP = site.DataProcessing.ActiveEnergyPCurrent - site.DataProcessing.ActiveEnergyPStart;
										double deltaActiveEnergyM = site.DataProcessing.ActiveEnergyMCurrent - site.DataProcessing.ActiveEnergyMStart;

										double deltaReactiveEnergyP = site.DataProcessing.ReactiveEnergyPCurrent - site.DataProcessing.ReactiveEnergyPStart;
										double deltaReactiveEnergyM = site.DataProcessing.ReactiveEnergyMCurrent - site.DataProcessing.ReactiveEnergyMStart;

										if (deltaActiveEnergyP > deltaActiveEnergyM)	// quadrants prevalent: I or IV
										{
											PublishEnergy(site, nowRnd, deltaActiveEnergyP, deltaActiveEnergyM, deltaReactiveEnergyP, 0, 0, deltaReactiveEnergyM, ratio, logFile);
										}
										else  // quadrants prevalent: II or III
										{
											PublishEnergy(site, nowRnd, deltaActiveEnergyP, deltaActiveEnergyM, 0, deltaReactiveEnergyP, deltaReactiveEnergyM, 0, ratio, logFile);
										}

										site.DataProcessing.EnergyPeriodDateTime = now;
										site.DataProcessing.ActiveEnergyPStart = ActiveEnergyP;
										site.DataProcessing.ActiveEnergyMStart = ActiveEnergyM;
										site.DataProcessing.ReactiveEnergyPStart = ReactiveEnergyP;
										site.DataProcessing.ReactiveEnergyMStart = ReactiveEnergyM;
									}
								}

								if (site.DataProcessing.ActivePowerPStart == 0)
									site.DataProcessing.ActivePowerPStart = ActiveEnergyP;
								site.DataProcessing.ActivePowerPCurrent = ActiveEnergyP;

								if (site.DataProcessing.ActivePowerMStart == 0)
									site.DataProcessing.ActivePowerMStart = ActiveEnergyM;
								site.DataProcessing.ActivePowerMCurrent = ActiveEnergyM;

								if (site.DataProcessing.ReactivePowerPStart == 0)
									site.DataProcessing.ReactivePowerPStart = ReactiveEnergyP;
								site.DataProcessing.ReactivePowerPCurrent = ReactiveEnergyP;

								if (site.DataProcessing.ReactivePowerMStart == 0)
									site.DataProcessing.ReactivePowerMStart = ReactiveEnergyM;
								site.DataProcessing.ReactivePowerMCurrent = ReactiveEnergyM;

								tStart = new DateTime(site.DataProcessing.PowerPeriodDateTime.Year, site.DataProcessing.PowerPeriodDateTime.Month, site.DataProcessing.PowerPeriodDateTime.Day, site.DataProcessing.PowerPeriodDateTime.Hour, site.DataProcessing.PowerPeriodDateTime.Minute / PPeriod * PPeriod, 0);
								ts = now.Subtract(tStart);

								if (PPeriod > 0)		// if power recording enabled
								{
									if (((ts.Hours * 60) + ts.Minutes) >= PPeriod)  // if end of period
									{
										ts = now.Subtract(site.DataProcessing.PowerPeriodDateTime);
										double ratio = (double)((ts.Hours * 3600) + (ts.Minutes * 60) + ts.Seconds) / (PPeriod * 60);
										if (ratio > 0.9) ratio = 1;
										ratio = Math.Round(ratio, 2);
										TraceLine(logFile, string.Format("End of integration power period (ratio = {0})", ratio));

										double deltaActivePowerP = (site.DataProcessing.ActivePowerPCurrent - site.DataProcessing.ActivePowerPStart) / PPeriod * 60;
										double deltaActivePowerM = (site.DataProcessing.ActivePowerMCurrent - site.DataProcessing.ActivePowerMStart) / PPeriod * 60;

										double deltaReactivePowerP = (site.DataProcessing.ReactivePowerPCurrent - site.DataProcessing.ReactivePowerPStart) / PPeriod * 60;
										double deltaReactivePowerM = (site.DataProcessing.ReactivePowerMCurrent - site.DataProcessing.ReactivePowerMStart) / PPeriod * 60;

                                        // since ZXF meter has no capability to supply Q1-Q4 (but only 2 quadrants), we calculate Q1, Q2, Q3, Q4 here

										if (deltaActivePowerP > deltaActivePowerM)	// quadrants prevalent: I or IV
										{
											PublishPower(site, nowRnd, deltaActivePowerP, deltaActivePowerM, deltaReactivePowerP, 0, 0, deltaReactivePowerM, ratio, logFile);
										}
										else  // quadrants prevalent: II or III
										{
											PublishPower(site, nowRnd, deltaActivePowerP, deltaActivePowerM, 0, deltaReactivePowerP, deltaReactivePowerM, 0, ratio, logFile);
										}

                                        // 

										site.DataProcessing.PowerPeriodDateTime = now;
										site.DataProcessing.ActivePowerPStart = ActiveEnergyP;
										site.DataProcessing.ActivePowerMStart = ActiveEnergyM;
										site.DataProcessing.ReactivePowerPStart = ReactiveEnergyP;
										site.DataProcessing.ReactivePowerMStart = ReactiveEnergyM;
									}
								}
							}
							catch (Exception ex)
							{
								TraceLine(logFile, "Error. " + ex.Message);
								if (comm != null)
								{
									comm.Close();
								}
								continue;
							}
					
#if ZERO
							foreach (GXDLMSObject it in objects)
							{
								if (!string.IsNullOrEmpty(site.ArrayOfOBIS.DataObjects.ActiveEnergyP))		// if is requested the ActiveEnergy+ obis code
								{


									if (it.LogicalName == AEP)
									{
										try
										{
											object value = comm.Read(it, 2);
											string scalerAttr = (string)comm.Read(it, 3);

											Regex regex = new Regex(@"^Scaler:(.*) Unit:(.*)$");
											Match match = regex.Match(scalerAttr);

											double scaler = 1;
											if (match.Groups.Count >= 2)
												scaler = double.Parse(match.Groups[1].Value);

											double Energy = double.Parse(value.ToString()) * scaler;

											// First time initialize the value of initial energy
											if (site.DataProcessing.ActiveEnergyPStart == 0)
											{
												site.DataProcessing.ActiveEnergyPStart = Energy;
												TraceLine(logFile, "Initial ActiveEnergy+ : " + Energy + " Wh");
											}
										}
										catch (Exception ex)
										{
											TraceLine(logFile, "Error. Failed to get ActiveEnergy+: " + ex.Message);
											//Continue reading.
										}


									}
								}


		
								// Read meter inst. values
								if (!string.IsNullOrEmpty(site.ArrayOfOBIS.DataObjects.ActivePower))
								{
									try
									{

										if (it.LogicalName == site.ArrayOfOBIS.DataObjects.ActivePower)
										{
											//Console.WriteLine(it.Name);
											object value = comm.Read(it, 2);
											string scalerAttr = (string)comm.Read(it, 3);

											Regex regex = new Regex(@"^Scaler:(.*) Unit:(.*)$");
											Match match = regex.Match(scalerAttr);

											double scaler = 1;
											if (match.Groups.Count >= 2)
												scaler = double.Parse(match.Groups[1].Value);

											double activePower = double.Parse(value.ToString()) * scaler;
											TraceLine(logFile, "Active Power: " + activePower + " W");

											int period = 15;
											if (site.DataProcessing.Period != 0)
											{
												period = site.DataProcessing.Period;
											}
											DateTime now = DateTime.Now;
											DateTime nowRnd = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute / period * period, 0);

											if (site.DataProcessing.IntervalDateTime.Ticks == 0)
											{
												site.DataProcessing.IntervalDateTime = nowRnd;
												site.DataProcessing.Count = 0;
												site.DataProcessing.Sum = 0;
												TraceLine(logFile, "Initialize period from null.");
											}
											else
											{
												long elapsedTicks = now.Ticks - site.DataProcessing.IntervalDateTime.Ticks;
												TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);
												if (elapsedSpan.TotalMinutes >= period)  // if end of period
												{
													// Round date to minute multiple of "period"
													if (site.DataProcessing.Count > 0)    // compute avg quantities
													{
														TraceLine(logFile, "End of inst. readings integration period.");
														double avgActivePower = site.DataProcessing.Sum / site.DataProcessing.Count;
														site.DataProcessing.IntervalDateTime = nowRnd;
														site.DataProcessing.Count = 0;
														site.DataProcessing.Sum = 0;
														//                                                    PublishReading(site, site.DataProcessing.IntervalDateTime.AddMinutes(period), avgActivePower, logFile);
														PublishReading(site, site.DataProcessing.IntervalDateTime, avgActivePower, 0, 0, 0, 0, 0, logFile);
													}
													else
													{
														TraceLine(logFile, "Zero counts period: initialize only.");
														site.DataProcessing.IntervalDateTime = nowRnd;
														site.DataProcessing.Count = 0;
														site.DataProcessing.Sum = 0;
													}
												}
												site.DataProcessing.Sum += activePower;
												site.DataProcessing.Count++;
											}
										}
									}
									catch (Exception ex)
									{
										TraceLine(logFile, "Error. Failed to get inst. readings: " + ex.Message);
										//Continue reading.
									}
								}

								//  Read load profiles
								if (string.IsNullOrEmpty(site.ArrayOfOBIS.GenericProfileObjects.LoadProfile))
									continue;

								if (it.LogicalName == site.ArrayOfOBIS.GenericProfileObjects.LoadProfile)
								{
									//Console.WriteLine(it.Name);
									try
									{
										int period = 15;
										if (site.DataProcessing.ProfilePeriod != 0)
										{
											period = site.DataProcessing.ProfilePeriod;
										}

										DateTime now = DateTime.Now;
										DateTime nowRnd = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute / period * period, 0);
										long elapsedTicks = now.Ticks - site.DataProcessing.ProfileDateTime.Ticks;
										TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);
										double totalMinutes = elapsedSpan.TotalMinutes;
										int maxDays = 1;
										if (totalMinutes > 1440 * maxDays)
										{  // max one week
											totalMinutes = 1440 * maxDays;
											site.DataProcessing.ProfileDateTime = nowRnd.AddMinutes(-totalMinutes);
										}
										if (totalMinutes >= period)  // if end of period
										{
											//Read from Profile Generic.                    
											object[] rows = comm.ReadRowsByRange(it, site.DataProcessing.ProfileDateTime, nowRnd, (it as GXDLMSProfileGeneric).CaptureObjects);
											StringBuilder sb = new StringBuilder();
											foreach (object[] row in rows)
											{
												object dto = row[0];
												DateTime dt;
												if (DateTime.TryParse(dto.ToString(), out dt))
												{
													dt = dt.ToLocalTime();
													row[0] = dt;
												}


												foreach (object cell in row)
												{
													if (cell is byte[])
													{
														sb.Append(GXCommon.ToHex((byte[])cell, true));
													}
													else
													{
														sb.Append(Convert.ToString(cell));
													}
													sb.Append(" | ");
												}
												sb.Append("\r\n");
											}
											site.DataProcessing.ProfileDateTime = nowRnd;
											PublishProfile(site, site.DataProcessing.ProfileDateTime, sb, logFile);
										}
									}
									catch (Exception ex)
									{
										TraceLine(logFile, "Error. Failed to get load profile: " + ex.Message);
										//Continue reading.
									}
								}
							}
#endif				
							try
							{
								using (Stream stream = File.Open(sitesMapPath, FileMode.Create))
								{
									TraceLine(logFile, "Save site map to file.");
									TextWriter writer = new StreamWriter(stream);
									y.Serialize(writer, aods);
									//                            aods = y.Deserialize(stream) as IAM.ArrayOfDLMSSite;
									stream.Close();

									infoFileSite.Refresh();
									lastWriteMapFile = infoFileSite.LastWriteTime;
								}

							}
							catch (Exception ex)
							{
								throw ex;
							}

							TraceLine(logFile, "Disconnecting from the meter.");
							comm.Close();
							objects.Clear();
						}
					}
					catch (Exception ex)
					{
						TraceLine(logFile, ex.Message);
						if (comm != null)
						{
							comm.Close();
						}
						// Continue reading
					}

					logFile.Flush();

					if (loopTime > 0)   // wait until next loop time
					{
						DateTime when = DateTime.Now;
						int nextMinutes = (when.Minute / loopTime) * loopTime + loopTime;
						DateTime nextTime = new DateTime(when.Year, when.Month, when.Day);
						nextTime = nextTime.AddHours(when.Hour);
						nextTime = nextTime.AddMinutes(nextMinutes);
						int waitSeconds = (int)(new TimeSpan(nextTime.Ticks - when.Ticks).TotalSeconds);
						waitSeconds++;
						TraceLine(logFile, "Waiting for next loop (in "+waitSeconds+" sec.)...\r\n");
						logFile.Flush();
						do
						{
							Thread.Sleep(1000);
							waitSeconds--;

							//if (Environment.UserInteractive)
							//    keyAvailable = Console.KeyAvailable;

						} while ((waitSeconds > 0) && !keyAvailable && running);
					}
					Console.WriteLine("");
					
					//if (Environment.UserInteractive)
					//    keyAvailable = Console.KeyAvailable;

				} while (!keyAvailable && (loopTime != -1) && running);

				if (keyAvailable)
				{
					TraceLine(logFile, "Program interrupted by user.");
				}
				else if (!running)
				{
					TraceLine(logFile, "Service stopped.");
				}
				else
				{
					TraceLine(logFile, "Single run ended.");
					if (!Environment.UserInteractive)
					{
						logFile.Flush();
						logFile.Close();
						Environment.Exit(0);
					}
				}
			}
			catch (Exception ex)
			{
				if (logFile != null)
					TraceLine(logFile, ex.Message);

				if (comm != null)
				{
					comm.Close();
				}
				Console.WriteLine("Error. " + ex.Message);
			}
			finally
			{
				logFile.Flush();
				logFile.Close();

				if (comm != null)
				{
					comm.Close();
				}
				Console.WriteLine("Now stop.");
			}
		}

		private static void Start(string[] cmdLineArgs)
		{
			// onstart code here
			args = cmdLineArgs;
			running = true;

			// run application code in a thread
			ThreadPool.QueueUserWorkItem(new WaitCallback(MainProcessing));
		}

		private static void Stop()
		{
			// onstop code here
			running = false;
			TraceLine(logFile, "Service stopped.");
			logFile.Flush();
			logFile.Close();
		}

		// State object for receiving data from remote device.
		public class StateObject
		{
			// Client socket.
			public Socket workSocket = null;
			// Size of receive buffer.
			public const int BufferSize = 256;
			// Receive buffer.
			public byte[] buffer = new byte[BufferSize];
			// Received data string.
			public StringBuilder sb = new StringBuilder();

		}
	}

}
