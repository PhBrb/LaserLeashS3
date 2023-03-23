using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace ChartTest2
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //start receiving data
            UDPReceiver udpReceiver = new UDPReceiver();


            OsciData osciData = new OsciData(1000);
            for(int i=0; i<osciData.dac0.Length; i++)
            {
                osciData.dac0[i] = i < osciData.dac0.Length/2? (20.0 * i) / osciData.dac0.Length: (20.0 * (osciData.dac0.Length-i)) / osciData.dac0.Length;
                osciData.adc0[i] = (5.0 * i) / osciData.dac0.Length;
            }
            Deserializer osciWriter = new Deserializer(osciData);

            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 form = new Form1(osciWriter, udpReceiver);

            //transfer data from udp to osci memory
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        osciWriter.TransferData(udpReceiver.b.lastRawData);
                    }
                    catch
                    {

                    }
                    Thread.Sleep(1);
                }
            });

            //refresh osci display
            Task.Run(() =>
            {
                while (true)
                {
                    form.OnNewData();
                    Thread.Sleep(10);
                }
            });

            Application.Run(form);
        }
    }
}
