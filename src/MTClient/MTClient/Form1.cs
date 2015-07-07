using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace MTClient
{
    //[DllImport("user32.dll")]
    //public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

    //[DllImport("user32.dll")]
    //private static extern IntPtr GetForegroundWindow();

    public partial class Form1 : Form
    {
        System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();
        NetworkStream serverStream; 

        public Form1() {
            InitializeComponent();
        }


        public void msg(string mesg){
            textBox1.Text = textBox1.Text + Environment.NewLine + " >> " + mesg;
        }


        private void button1_Click_1(object sender, EventArgs e)
        {
            String iammeter1, iamload1;

            Random rnd = new Random();
            
            // hybrid metering data object (being aggregated from the individual EDM messages)
            int  id   = 1;                 //concentrator ID
            long ts0  = 0;                 //time previous
            long ts1  = 1;                 //time current
            long dt   = (ts1 - ts0);       //dt
            long ea   = 1;                 //energy accumulated so far (since the beginning)
            long ep   = 1;                 //energy previous
            long ec   = 1;                 //energy current
            long en   = 1;                 //energy next
            long de   = (ec-ep);           //delta energy
            long p1   = (ec-ep)/(ts1-ts0); //averaged pseudo-measurement of Power
            long unb1 = 0;                 //unbalance of energy of the current time step (stateless)
            long unbm = 0;                 //unbalance accumulated so far (stateful)
            long crc0 = 0;                 // counter of the intencity of messages
            long crc1 = 0;                 // counter of the intencity of messages
            

            //load object
            int meterid = 1; //MeterId
            int uapower = rnd.Next(0, 100); // number between 0 and 100
            int dapower = rnd.Next(0, 100); // number between 0 and 100
            int rpower1 = rnd.Next(0, 100); // number between 0 and 100
            int rpower2 = rnd.Next(0, 100); // number between 0 and 100
            int rpower3 = rnd.Next(0, 100); // number between 0 and 100
            int rpower4 = rnd.Next(0, 100); // number between 0 and 100

            //meter object
            int daPowerEEA = rnd.Next(0, 100); // number between 0 and 100
            int ripowerEEI = rnd.Next(0, 100); // number between 0 and 100
            int rcpowerEEC = rnd.Next(0, 100); // number between 0 and 100
            int uapowerEUA = rnd.Next(0, 100); // number between 0 and 100
            int ripowerEUI = rnd.Next(0, 100); // number between 0 and 100
            int rcpowerEUC = rnd.Next(0, 100); // number between 0 and 100
            String tariffType = "T1";
            String periodref = "Q";
            long loadtime    = 0; // by IAM client
            long currenttime = 0; // by IAM server

            
            //convert the date in seconds as requested by Engineering
            DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan timeSinceEpoch = DateTime.UtcNow - UnixEpoch;
            long seconds = (long) timeSinceEpoch.TotalSeconds;
            //Instant instant = clock.Now;
            //Duration duration = instant - Instant.UnixEpoch;
            // Nodal time exposes TotalXyz as long. Go from ticks here
            //double seconds = ((double)duration.TotalTicks) / NodaConstants.TicksPerSecond;
            //long ticks = DateTime.UtcNow.Ticks % TimeSpan.TicksPerSecond;
            
            //EDM data
            iammeter1 = "<meter isConcentrator=\u0022false\u0022 " + "meterId=\u0022"+ "000000" + "\u0022>" +
                        "<currentTime>"             + Convert.ToString(seconds) + "</currentTime>" +
                        "<upsteamActivePower>"      + Convert.ToString(uapower) + "</upsteamActivePower>" +
                        "<downstreamActivePower>"   + Convert.ToString(dapower) + "</downstreamActivePower>" + 
                        "<reactivePowerQ1>"         + Convert.ToString(rpower1) + "</reactivePowerQ1>" +
                        "<reactivePowerQ2>"         + Convert.ToString(rpower2) + "</reactivePowerQ2>" +
                        "<reactivePowerQ3>"         + Convert.ToString(rpower3) + "</reactivePowerQ3>" +
                        "<reactivePowerQ4>"         + Convert.ToString(rpower4) + "</reactivePowerQ4>" + "</meter>";

            //TDM data
            iamload1 = "<load isConcentrator=\u0022"   + "false" + "\u0022 meterId=" + "000000" + ">" +
                        "<sampleNumber>"               + "1"          + "</sampleNumber>" +
                        "<loadTime>"                   + "1401887914" + "</loadTime>" +
                        "<downstreamActivePowerEEA>"   + "11.0"       + "</downstreamActivePowerEEA>" +
                        "<reactiveInductivePowerEEI>"  + "21.0"       + "</reactiveInductivePowerEEI>" +
                        "<reactiveCapacitivePowerEEC>" + "31.0"       + "</reactiveCapacitivePowerEEC>" +
                        "<upstreamActivePowerEUA>"     + "4.0"        + "</upstreamActivePowerEUA>" +
                        "<reactiveInductivePowerEUI>"  + "5.0"        + "</reactiveInductivePowerEUI>" +
                        "<reactiveCapacitivePowerEUC>" + "6.0"        + "</reactiveCapacitivePowerEUC>" + 
                        "<tariffType>"                 + "T1"         + "</tariffType>" +
                        "<integrationPeriodRef>"       + "Q"          + "</integrationPeriodRef>" +
                        "<currentTime>"                + "1401887499" + "</currentTime>" + "</load>";


            NetworkStream serverStream = clientSocket.GetStream();
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(iammeter1 + " $");
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();
            byte[] inStream = new byte[10025];
            serverStream.Read(inStream, 0, (int)clientSocket.ReceiveBufferSize);
            string returndata = System.Text.Encoding.ASCII.GetString(inStream);
            msg("Data from Server : " + returndata);
        }

        private void Form1_Load_1(object sender, EventArgs e){
            msg("Client Started");

            //get name of this client
            string name1 = Environment.MachineName;//Server Name
            Process p = Process.GetCurrentProcess();
            string name3 = Convert.ToString(p.ProcessName) + " (PID: " + Convert.ToString(p.Id) + ")";
            
            clientSocket.Connect("127.0.0.1", 8888);
            label1.Text = "Client Socket " + name1 + "." + name3 + " - Server Connected.";
         
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label2.Text = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            //send METER data every 5  minutes 
            //send LOAD data every  15 minutes
        } 

    }
        //

}
