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

            //Data flow: udpReceiver -> deserializer -> memory -> oscidisplay -> form -> (user input) -> mqtt
            UDPReceiver udpReceiver = new UDPReceiver();
            Memory memory = new Memory(1000000); //5000000 is roughly 7s
            OsciDisplay osciDisplay = new OsciDisplay(memory);
            
            MQTTPublisher mqtt = new MQTTPublisher();
            mqtt.connect();


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            SpectrscopyControlForm form = new SpectrscopyControlForm(memory, osciDisplay, mqtt);

            //transfer data from udp to osci memory
            new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    if (udpReceiver.Size > 0)
                        udpReceiver.TransferData(memory);
                }
            })).Start();

            //refresh osci display
            new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(1000);//TODO https://stackoverflow.com/questions/18675771/wait-until-form-is-finished-loading , also do samplesOnDisplayText.Value = osciDisplay.getSize(); there 
                while (true)
                {
                    form.OnNewData();
                    Thread.Sleep(100);
                }
            })).Start();

            Application.Run(form);
        }
    }
}
