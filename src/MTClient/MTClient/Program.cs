using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;


namespace MTClient
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]

        private static void TimerCallback(Object o)
        {
            String s1;

            // gathering time-driven metering data from DLMS device goes here

            // client date expressed in seconds
            DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime now = DateTime.UtcNow; // or use DateTime.Now
            TimeSpan timeSinceEpoch = now - UnixEpoch;
            long seconds = (long)timeSinceEpoch.TotalSeconds;

            //get name of this client
            string name1 = Environment.MachineName;//Server Name
            Process p = Process.GetCurrentProcess();
            string name3 = Convert.ToString(p.ProcessName) + " (PID: " + Convert.ToString(p.Id) + ")";

            s1 = "TDM (" + name1 + "." + name3 + ") " + DateTime.Now + " (" + Convert.ToString(seconds) + ") >>";
            // show data
            // write data to the local file in APPEND mode
            //Console.WriteLine(s1); // Display the date/time when this method got called
            using (System.IO.StreamWriter w1 = File.AppendText(@".\MTClientlogfile.txt")) w1.WriteLine(s1);
            GC.Collect(); // Force a garbage collection to defragment memory

        } // end TimerCallback


        static void Main()
        {
            long s1, m1, h1, i1;
            DateTime now = DateTime.Now;
            
            s1 = now.Second;
            m1 = now.Minute;
            h1 = now.Hour;
            i1 = (60 - s1) * 1000;
            // hybrid metering contains time-driven events every 5 minutes (TDM)
            System.Threading.Timer t = new System.Threading.Timer(TimerCallback, null, i1, 5000); //timer object every 5,000 milliseconds here

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
