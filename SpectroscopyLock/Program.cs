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
            //start receiving data
            UDPReceiver udpReceiver = new UDPReceiver();


            OsciData osciData = new OsciData(1000);
            for(int i=0; i<osciData.dac0Rolling.Length; i++)
            {
                osciData.dac0Rolling[i] = i < osciData.dac0Rolling.Length/2? (20.0 * i) / osciData.dac0Rolling.Length: (20.0 * (osciData.dac0Rolling.Length-i)) / osciData.dac0Rolling.Length;
                osciData.adc0Rolling[i] = (5.0 * i) / osciData.dac0Rolling.Length;
            }
            for (int iFloat = 0; iFloat < 1000; iFloat++)
            {
                int index = Deserializer.mod(-iFloat, osciData.dac0Rolling.Length);
                osciData.setDatapoint(osciData.dac0Rolling[index], osciData.adc0Rolling[index]);
            }
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
