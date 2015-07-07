using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MTServer
{
    class Program
    {

        private static void TimerCallback(Object o)
        {
            String s1;

            // time-driven metering could be handled here
            // do polling of every client
            // get data from clients

            //convert the date in seconds as requested by Engineering
            DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime now = DateTime.UtcNow; // or use DateTime.Now
            TimeSpan timeSinceEpoch = now - UnixEpoch;
            long seconds = (long)timeSinceEpoch.TotalSeconds;

            s1 = "TDM>> timer-driven DLMS metering at " + DateTime.Now + " (" + Convert.ToString(seconds) + " s)";
         
            // show data and write data to the local file in APPEND mode
            Console.WriteLine(s1); // Display the date/time when this method got called
            using (System.IO.StreamWriter w1 = File.AppendText(@".\MTlogfile.txt")) w1.WriteLine(s1);
            GC.Collect(); // Force a garbage collection to defragment memory

            // calculate local totales and control variables (e.g. intensities etc.)

            // add here the code sending to FIWARE GENERIC ENABLER
            // use the s1 structure containing the DMLS data

/* SECURITY SENSITIVE PART GOES HERE
METER = metering data (at the moment, retrieved every 5 mins)
LOAD = load profile (at the moment, retrieved every 15 mins)
A1-Authentication - GET) https: 1.1.1.1/metering-input
A2-SEND as POST) https: 1.1.1.1/metering-input
B1-AUTENTICAZIONE - GET) http: 1.1.1.1:8080/metering-input
B2-SEND as POST) http: 1.1.1.1:8080/metering-input

To check that data have been passed to the ContextBroker and then forwarded to the BigData:
http: 1.1.1.1/rest2cosmos/rest/restHive2Cosmos/meterTest
http: 1.1.1.1/rest2cosmos/rest/restHive2Cosmos/loadTest
*/


        } // end TimerCallback

        static void Main(string[] args)
        {
            int counter = 0;
            int h1, m1, s1, i1 = 0;
            
            DateTime now = DateTime.Now;
            s1 = now.Second;
            m1 = now.Minute;
            h1 = now.Hour;
            i1 = (60 - s1)*1000;

            // hybrid metering contains time-driven events every 5 minutes (TDM)
            Timer t = new Timer(TimerCallback, null, i1, 5000); //timer object every 5,000 milliseconds here

            // asynchronous event listener goes here (EDM)
            TcpListener serverSocket = new TcpListener(IPAddress.Parse("127.0.0.1"), 8888);
            TcpClient   clientSocket = default(TcpClient);
            serverSocket.Start();
            Console.WriteLine(">> " + "Multithread IAM server listens on 127.0.0.1:8888\r\n---------------------------------------------------");
            counter = 0;
            while (true )
            {
                try
                {
                    counter += 1; // new client added
                    clientSocket = serverSocket.AcceptTcpClient(); // accept client socket connection
                    Console.WriteLine(" >> " + "Client No:" + Convert.ToString(counter) + " started on " + clientSocket.Client.RemoteEndPoint);
                    handleClient client = new handleClient();
                    client.startClient(clientSocket, Convert.ToString(counter));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(">> " + ex.ToString());
                    clientSocket.Close();
                    serverSocket.Stop();
                    Console.WriteLine(">> " + "exit");
                    Console.ReadLine();
                }
            }

            // add cleanup code here but make it reachable !!!

        } //end main

    } //end class Program



    //Class to handle each client request separatly
    public class handleClient
    {
        TcpClient clientSocket;
        string clNo;

        public void startClient(TcpClient inClientSocket, string clineNo){
            this.clientSocket = inClientSocket;
            this.clNo = clineNo;
            Thread ctThread = new Thread(doChat);
            ctThread.Start();
        } //end startClient

        private void doChat(){
            int requestCount = 0;
            int l1 = 0;
            int l2 = 0;
            byte[] bytesFrom = new byte[10025];
            string dataFromClient = null;
            Byte[] sendBytes = null;
            string serverResponse = null;
            string rCount = null;
            String s1;

            requestCount = 0;

            while (clientSocket.Connected) //when one client shuts down, the clientSocket becomes "Disposed" (exception)
            {
                try
                {
                    requestCount = requestCount + 1;
                    NetworkStream networkStream = clientSocket.GetStream();
                    l1 = (int)clientSocket.ReceiveBufferSize;
                    if (l1 > 0) 
                    {
                        networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                        dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                        l2 = dataFromClient.IndexOf("$");
                        if (l2 > 0) 
                           {
                            dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                            s1 = "EDM from client " + clNo + " >> " + dataFromClient;
                            //
                            // now you got the message form IAMReader. Put your own code to dialogate with FINESCE services
                            // in this build, we removed our project-specific code because containing sensitive items
                            //
                            Console.WriteLine(s1);
                            //write to file
                            using (System.IO.StreamWriter w1 = File.AppendText(@".\MTlogfile.txt"))
                                w1.WriteLine(s1);
                            GC.Collect(); // Force a garbage collection to defragment memory
                            //
                           }
                        rCount = Convert.ToString(requestCount);
                        serverResponse = "Server to client(" + clNo + ") " + rCount;
                        sendBytes = Encoding.ASCII.GetBytes(serverResponse);
                        if (sendBytes.Length > 0)
                        {
                            networkStream.Write(sendBytes, 0, sendBytes.Length);
                            networkStream.Flush();
                        }
                        Console.WriteLine(" >> " + serverResponse);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" >> one remote client was shut down.");
                    //Console.WriteLine(" >> " + ex.ToString() + "\r\n");
                    //handle here clients shutting down
                    //Console.WriteLine(" >> Client " + clientSocket.Client.RemoteEndPoint + " exit");
                    //clientSocket.Close();
                    //GC.Collect();
                    //Console.ReadLine();
                }
            }
        } //end doChat


    } //end handleClient

} //end namespace

