using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gurux.Common;
using Gurux.DLMS;
using Gurux.Net;
using Gurux.Serial;
using System.IO.Ports;
using Gurux.DLMS.ManufacturerSettings;
using Gurux.DLMS.Objects;
using System.IO;

namespace IAMReader
{
    class GXCommunicatation
    {
        public bool Trace = false;
        public Gurux.DLMS.GXDLMSClient Client;
        int WaitTime = 10000;
        IGXMedia Media;
        bool InitializeIEC;
        GXManufacturer Manufacturer;
        HDLCAddressType HDLCAddressing;        

        /// <summary>
        /// Constructor.
        /// </summary>
        public GXCommunicatation(Gurux.DLMS.GXDLMSClient dlms, IGXMedia media, bool initializeIEC, Gurux.DLMS.Authentication authentication, string password)
        {
            Client = dlms;
            Media = media;
            InitializeIEC = initializeIEC;
            Client.Authentication = authentication;
            Client.Password = ASCIIEncoding.ASCII.GetBytes(password);            
            //Delete trace file if exists.
            if (File.Exists("trace.txt"))
            {
                File.Delete("trace.txt");
            }
        }

        public void Close()
        {
            if (Media != null && Client != null)
            {
                try
                {
                    //Console.WriteLine("Disconnecting from the meter.");
                    ReadDLMSPacket(Client.DisconnectRequest());
                    Media.Close();
                }
                catch
                {

                }
                Media = null;
                Client = null;
            }
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        /// <remarks>
        /// Close connection to the media and send disconnect message.
        /// </remarks>
        ~GXCommunicatation()
        {
            Close();
        }

        public void UpdateManufactureSettings(string id)
        {
            if (Manufacturer.Identification == "LGZ")   //TW
            {
                Manufacturer.UseIEC47 = true;
                Manufacturer.UseLogicalNameReferencing = true;
            }
            if (Manufacturer != null && string.Compare(Manufacturer.Identification, id, true) != 0)
            {
                throw new Exception(string.Format("Manufacturer type does not match. Manufacturer is {0} and it should be {1}.", id, Manufacturer.Identification));
            }            
            Client.InterfaceType = InterfaceType.General;
            Client.UseLogicalNameReferencing = Manufacturer.UseLogicalNameReferencing;
            //If network media is used check is manufacturer supporting IEC 62056-47
            GXServerAddress server = Manufacturer.GetServer(HDLCAddressing);
            if (this.Media is GXNet && Manufacturer.UseIEC47)
            {
                Client.InterfaceType = InterfaceType.Net;
                Client.ClientID = Convert.ToUInt16(Manufacturer.GetAuthentication(Client.Authentication).ClientID);
                //Client.ServerID = Convert.ToUInt16(server.PhysicalAddress);
                Client.ServerID = 1;
            }
            else
            {
                if (HDLCAddressing == HDLCAddressType.Custom)
                {
                    Client.ClientID = Manufacturer.GetAuthentication(Client.Authentication).ClientID;
                }
                else
                {
                    GXAuthentication auth = Manufacturer.GetAuthentication(Client.Authentication);
                    if (auth == null)
                    {
                        Client.ClientID = (byte)41;
                    }
                    else
                    {
                        Client.ClientID = (byte)(Convert.ToByte(auth.ClientID) << 1 | 0x1);
                    }
                }                                
                Client.ServerID = GXManufacturer.CountServerAddress(HDLCAddressing, server.Formula, server.PhysicalAddress, server.LogicalAddress);
            }
        }

        void InitSerial()
        {
            GXSerial serial = Media as GXSerial;
            byte Terminator = (byte)0x0A;
            if (serial != null && InitializeIEC)
            {
                serial.BaudRate = 300;
                serial.DataBits = 7;
                serial.Parity = Parity.Even;
                serial.StopBits = StopBits.One;
            }
            Media.Open();
            //Query device information.
            if (Media != null && InitializeIEC)
            {
                string data = "/?!\r\n";
                if (Trace)
                {
                    Console.WriteLine("IEC sending:" + data);
                }
                ReceiveParameters<string> p = new ReceiveParameters<string>()
                {
                    Eop = Terminator,
                    WaitTime = WaitTime
                };
                lock (Media.Synchronous)
                {
                    WriteTrace("<- " + DateTime.Now.ToLongTimeString() + "\t" + GXCommon.ToHex(ASCIIEncoding.ASCII.GetBytes(data), true));
                    Media.Send(data, null);
                    if (!Media.Receive(p))
                    {
                        //Try to move away from mode E.
                        try
                        {
                            ReadDLMSPacket(Client.DisconnectRequest());
                        }
                        catch (Exception)
                        {
                        }
                        data = (char)0x01 + "B0" + (char)0x03;
                        Media.Send(data, null);
                        p.Count = 1;
                        if (!Media.Receive(p))
                        {
                        }
                        data = "Failed to receive reply from the device in given time.";
                        Console.WriteLine(data);
                        throw new Exception(data);
                    }
                    WriteTrace("-> " + DateTime.Now.ToLongTimeString() + "\t" + GXCommon.ToHex(ASCIIEncoding.ASCII.GetBytes(p.Reply), true));
                    //If echo is used.
                    if (p.Reply == data)
                    {
                        p.Reply = null;
                        if (!Media.Receive(p))
                        {
                            //Try to move away from mode E.
                            ReadDLMSPacket(Client.DisconnectRequest());
                            if (serial != null)
                            {
                                data = (char)0x01 + "B0" + (char)0x03;
                                Media.Send(data, null);
                                p.Count = 1;
                                Media.Receive(p);
                                serial.BaudRate = 9600;
                                data = (char)0x01 + "B0" + (char)0x03 + "\r\n";
                                Media.Send(data, null);
                                p.Count = 1;
                                Media.Receive(p);
                            }

                            data = "Failed to receive reply from the device in given time.";
                            Console.WriteLine(data);
                            throw new Exception(data);
                        }
                        WriteTrace("-> " + DateTime.Now.ToLongTimeString() + "\t" + GXCommon.ToHex(ASCIIEncoding.ASCII.GetBytes(p.Reply), true));
                    }
                }
                Console.WriteLine("IEC received: " + p.Reply);
                if (p.Reply[0] != '/')
                {
                    p.WaitTime = 100;
                    Media.Receive(p);
                    throw new Exception("Invalid responce.");
                }
                string manufactureID = p.Reply.Substring(1, 3);
                UpdateManufactureSettings(manufactureID);
                char baudrate = p.Reply[4];
                int BaudRate = 0;
                switch (baudrate)
                {
                    case '0':
                        BaudRate = 300;
                        break;
                    case '1':
                        BaudRate = 600;
                        break;
                    case '2':
                        BaudRate = 1200;
                        break;
                    case '3':
                        BaudRate = 2400;
                        break;
                    case '4':
                        BaudRate = 4800;
                        break;
                    case '5':
                        BaudRate = 9600;
                        break;
                    case '6':
                        BaudRate = 19200;
                        break;
                    default:
                        throw new Exception("Unknown baud rate.");
                }
                Console.WriteLine("BaudRate is : " + BaudRate.ToString());
                //Send ACK
                //Send Protocol control character
                byte controlCharacter = (byte)'2';// "2" HDLC protocol procedure (Mode E)
                //Send Baudrate character
                //Mode control character 
                byte ModeControlCharacter = (byte)'2';//"2" //(HDLC protocol procedure) (Binary mode)
                //Set mode E.
                byte[] arr = new byte[] { 0x06, controlCharacter, (byte)baudrate, ModeControlCharacter, 13, 10 };
                Console.WriteLine("Moving to mode E.", arr);
                lock (Media.Synchronous)
                {
                    WriteTrace("<- " + DateTime.Now.ToLongTimeString() + "\t" + GXCommon.ToHex(arr, true));
                    Media.Send(arr, null);
                    p.Reply = null;
                    if (!Media.Receive(p))
                    {
                        //Try to move away from mode E.
                        ReadDLMSPacket(Client.DisconnectRequest());
                        data = "Failed to receive reply from the device in given time.";
                        Console.WriteLine(data);
                        throw new Exception(data);
                    }
                    WriteTrace("-> " + DateTime.Now.ToLongTimeString() + "\t" + GXCommon.ToHex(ASCIIEncoding.ASCII.GetBytes(p.Reply), true));
                    Console.WriteLine("Received: " + p.Reply);
                    if (serial != null)
                    {
                        serial.BaudRate = BaudRate;
                        serial.DataBits = 8;
                        serial.Parity = Parity.None;
                        serial.StopBits = StopBits.One;
                        System.Threading.Thread.Sleep(300);
                        serial.ResetSynchronousBuffer();
                    }
                }
            }
        }

        void InitNet()
        {
            Media.Open();
        }

        public void InitializeConnection(GXManufacturer man)
        {
            Manufacturer = man;
            UpdateManufactureSettings(man.Identification);
            if (Media is GXSerial)
            {
                Console.WriteLine("Initializing serial connection.");
                InitSerial();                
            }
            else if (Media is GXNet)
            {
                //Console.WriteLine("Initializing Network connection.");
                InitNet();
                //Some Electricity meters need some time before first message can be send.
                System.Threading.Thread.Sleep(500);
            }
            else
            {                
                throw new Exception("Unknown media type.");
            }
            byte[] data, reply = null;
            data = Client.SNRMRequest();
            if (data != null)
            {
                if (Trace)
                {
                    Console.WriteLine("Send SNRM request." + BitConverter.ToString(data));
                }
                reply = ReadDLMSPacket(data);
                if (Trace)
                {
                    Console.WriteLine("Parsing UA reply." + BitConverter.ToString(reply));
                }
                //Has server accepted client.
                Client.ParseUAResponse(reply);
                Console.WriteLine("Parsing UA reply succeeded.");
            }
            //Generate AARQ request.
            //Split requests to multible packets if needed. 
            //If password is used all data might not fit to one packet.
            foreach (byte[] it in Client.AARQRequest(null))
            {
                if (Trace)
                {
                    Console.WriteLine("Send AARQ request", BitConverter.ToString(it));
                }
                reply = ReadDLMSPacket(it);
            }
            if (Trace)
            {
                Console.WriteLine("Parsing AARE reply" + BitConverter.ToString(reply));
            }
            //Parse reply.
            Client.ParseAAREResponse(reply);
            //Get challenge Is HSL authentication is used.
            if (Client.IsAuthenticationRequired)
            {
                foreach (byte[] it in Client.GetApplicationAssociationRequest())
                {
                    reply = ReadDLMSPacket(it);
                }
                Client.ParseApplicationAssociationResponse(reply);
            }
            //Console.WriteLine("Parsing AARE reply succeeded.");
        }

        /// <summary>
        /// Read attribute value.
        /// </summary>
        public object Read(GXDLMSObject it, int attributeIndex)
        {            
            byte[] reply = ReadDataBlock(Client.Read(it.Name, it.ObjectType, attributeIndex)[0]);
            //Update data type.
            if (it.GetDataType(attributeIndex) == DataType.None)
            {
                it.SetDataType(attributeIndex, Client.GetDLMSDataType(reply));
            }
            return Client.UpdateValue(reply, it, attributeIndex);            
        }

        /// <summary>
        /// Write attribute value.
        /// </summary>
        public void Write(GXDLMSObject it, int attributeIndex)
        {
            ReadDataBlock(Client.Write(it, attributeIndex));                        
        }

        /// <summary>
        /// Method attribute value.
        /// </summary>
        public void Method(GXDLMSObject it, int attributeIndex, object value, DataType type)
        {
            ReadDataBlock(Client.Method(it, attributeIndex, value, type)[0]);                        
        }        

        /// <summary>
        /// Read Profile Generic Columns by entry.
        /// </summary>
        public object[] ReadRowsByEntry(GXDLMSObject it, int index, int count)
        {
            byte[] reply = ReadDataBlock(Client.ReadRowsByEntry(it.Name, index, count));
            return (object[]) Client.UpdateValue(reply, it, 2);
        }

        /// <summary>
        /// Read Profile Generic Columns by range.
        /// </summary>
        public object[] ReadRowsByRange(GXDLMSObject it, DateTime start, DateTime end, List<GXKeyValuePair<GXDLMSObject, GXDLMSCaptureObject>> columns)
        {
            GXDLMSObject col = columns[0].Key;
            byte[] reply = ReadDataBlock(Client.ReadRowsByRange(it.Name, col.LogicalName, col.ObjectType, col.Version, start, end));
            return (object[])Client.UpdateValue(reply, it, 2);
        }

        /// <summary>
        /// Read data type of selected attribute index.
        /// </summary>
        public DataType GetDLMSDataType(GXDLMSObject it, int attributeIndex)
        {
            byte[] reply = ReadDataBlock(Client.Read(it.Name, it.ObjectType, attributeIndex)[0]);
            return Client.GetDLMSDataType(reply);
        }        

        /// <summary>
        /// Read Association View from the meter.
        /// </summary>
        public GXDLMSObjectCollection GetAssociationView()
        {
            byte[] reply = ReadDataBlock(Client.GetObjectsRequest());
            return Client.ParseObjects(reply, false/*true*/);
        }

        void WriteTrace(string line)
        {
            if (Trace)
            {
                Console.WriteLine(line);
            }
            using (TextWriter writer = new StreamWriter(File.Open("trace.txt", FileMode.Append)))
            {
                writer.WriteLine(line);
            }
        }



        /// <summary>
        /// Read DLMS Data from the device.
        /// </summary>
        /// <param name="data">Data to send.</param>
        /// <returns>Received data.</returns>
        public byte[] ReadDLMSPacket(byte[] data)
        {
            if (data == null)
            {
                return null;
            }
            object eop = (byte)0x7E;
            //In network connection terminator is not used.
            if (Client.InterfaceType == InterfaceType.Net && Media is GXNet)
            {
                eop = null;
            }
            int pos = 0;
            bool succeeded = false;
            ReceiveParameters<byte[]> p = new ReceiveParameters<byte[]>()
            {
                AllData = true,
                Eop = eop,
                Count = 5,
                WaitTime = WaitTime,
            };
            lock (Media.Synchronous)
            {
                while (!succeeded && pos != 3)
                {
                    WriteTrace("<- " + DateTime.Now.ToLongTimeString() + "\t" + GXCommon.ToHex(data, true));
                    Media.Send(data, null);
                    succeeded = Media.Receive(p);
                    if (!succeeded)
                    {
                        //If Eop is not set read one byte at time.
                        if (p.Eop == null)
                        {
                            p.Count = 1;
                        }
                        //Try to read again...
                        if (++pos != 3)
                        {
                            System.Diagnostics.Debug.WriteLine("Data send failed. Try to resend " + pos.ToString() + "/3");
                            continue;
                        }                        
                        throw new Exception("Failed to receive reply from the device in given time.");
                    }
                }
                //Loop until whole Cosem packet is received.                
                while (!Client.IsDLMSPacketComplete(p.Reply))
                {
                    //If Eop is not set read one byte at time.
                    if (p.Eop == null)
                    {
                        p.Count = 1;
                    }
                    if (!Media.Receive(p))
                    {
                        //Try to read again...
                        if (++pos != 3)
                        {
                            System.Diagnostics.Debug.WriteLine("Data send failed. Try to resend " + pos.ToString() + "/3");
                            continue;
                        }
                        throw new Exception("Failed to receive reply from the device in given time.");
                    }
                }
            }
            WriteTrace("-> " + DateTime.Now.ToLongTimeString() + "\t" + GXCommon.ToHex(p.Reply, true));
            object errors = Client.CheckReplyErrors(data, p.Reply);
            if (errors != null)
            {
                object[,] arr = (object[,])errors;
                int error = (int)arr[0, 0];
                throw new GXDLMSException(error);
            }
            return p.Reply;
        }

        public void UpdateImage(GXDLMSImageTransfer target, byte[] data, string Identification)
        {
            //Check that image transfer ia enabled.
            byte[] reply = ReadDataBlock(Client.Read(target, 5));
            Client.UpdateValue(reply, target, 5);
            if (!target.ImageTransferEnabled)
            {
                throw new Exception("Image transfer is not enabled");
            }

            //Step 1: The client gets the ImageBlockSize.
            reply = ReadDataBlock(Client.Read(target, 2));
            Client.UpdateValue(reply, target, 2);

            // Step 2: The client initiates the Image transfer process.
            ReadDataBlock(target.ImageTransferInitiate(Client, Identification, data.Length));           
            // Step 3: The client transfers ImageBlocks.
            int ImageBlockCount;
            ReadDataBlock(target.ImageBlockTransfer(Client, data, out ImageBlockCount));
            //Step 4: The client checks the completeness of the Image in 
            //each server individually and transfers any ImageBlocks not (yet) transferred;
            Client.UpdateValue(reply, target, 2);

            // Step 5: The Image is verified;
            ReadDataBlock(target.ImageVerify(Client));
            // Step 6: Before activation, the Image is checked;

            //Get list to imaages to activate.
            reply = ReadDataBlock(Client.Read(target, 7));
            Client.UpdateValue(reply, target, 7);
            bool bFound = false;
            foreach (GXDLMSImageActivateInfo it in target.ImageActivateInfo)
            {
                if (it.Identification == Identification)
                {
                    bFound = true;
                    break;
                }
            }

            //Read image transfer status.
            reply = ReadDataBlock(Client.Read(target, 6));
            Client.UpdateValue(reply, target, 6);
            if (target.ImageTransferStatus != ImageTransferStatus.VerificationSuccessful)
            {
                throw new Exception("Image transfer status is " + target.ImageTransferStatus.ToString());
            }

            if (!bFound)
            {
                throw new Exception("Image not found.");
            }

            //Step 7: Activate image.
            ReadDataBlock(target.ImageActivate(Client));
        }

        public byte[] ReadDataBlock(byte[][] data)
        {
            byte[] reply = null;
            foreach (byte[] it in data)
            {
                reply = ReadDataBlock(it);
            }
            return reply;
        }

        /// <summary>
        /// Read data block from the device.
        /// </summary>
        /// <param name="data">data to send</param>
        /// <param name="text">Progress text.</param>
        /// <param name="multiplier"></param>
        /// <returns>Received data.</returns>
        public byte[] ReadDataBlock(byte[] data)
        {
            byte[] reply = ReadDLMSPacket(data);
            byte[] allData = null;
            RequestTypes moredata = Client.GetDataFromPacket(reply, ref allData);
            int maxProgress = Client.GetMaxProgressStatus(allData);
            while (moredata != 0)
            {
                while ((moredata & RequestTypes.Frame) != 0)
                {
                    data = Client.ReceiverReady(RequestTypes.Frame);                    
                    reply = ReadDLMSPacket(data);
                    RequestTypes tmp = Client.GetDataFromPacket(reply, ref allData);
                    if (!Trace)
                    {
                        Console.Write("-");
                    }
                    //If this was last frame.
                    if ((tmp & RequestTypes.Frame) == 0)
                    {
                        moredata &= ~RequestTypes.Frame;
                        break;
                    }
                }
                if ((moredata & RequestTypes.DataBlock) != 0)
                {
                    //Send Receiver Ready.
                    data = Client.ReceiverReady(RequestTypes.DataBlock);
                    reply = ReadDLMSPacket(data);
                    moredata = Client.GetDataFromPacket(reply, ref allData);
                    if (!Trace)
                    {
                        Console.Write("+");
                    }
                }
            }            
            return allData;
        }
    }
}
