using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using MQTTnet.Samples.Client;

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

            //Data flow: udpReceiver -> osciWriter -> osciData -> form -> (user input) mqtt
            UDPReceiver udpReceiver = new UDPReceiver();
            OsciData osciData = new OsciData(5000);
            Deserializer osciWriter = new Deserializer(osciData);
            
            MQTTPublisher mqtt = new MQTTPublisher();
            mqtt.connect();


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 form = new Form1(osciWriter, udpReceiver, mqtt);

            //transfer data from udp to osci memory
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        if(udpReceiver.b.lastRawData.Length > 0)
                            osciWriter.TransferData(udpReceiver.b.lastRawData);
                    }
                    catch (ArgumentException e)
                    {
                        Console.WriteLine("Couldnt convert data");
                    }
                    //Thread.Sleep(1);
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
