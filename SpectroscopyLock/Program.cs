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

        static bool formReady = false;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            //Data flow: udpReceiver -> deserializer -> memory -> oscidisplay -> form -> (user input) -> mqtt
            UDPReceiver udpReceiver = new UDPReceiver();
            Memory memory = new Memory(1000000); //5 000 000 is roughly 7s
            OsciDisplay osciDisplay = new OsciDisplay(memory);
            
            MQTTPublisher mqtt = new MQTTPublisher();
            mqtt.connect();


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            SpectrscopyControlForm form = new SpectrscopyControlForm(memory, osciDisplay, mqtt);
            form.Shown += new System.EventHandler(Form1_Shown);

            //transfer data from udp to osci memory
            new Thread(new ThreadStart(() =>
            {
                while (!form.stopped)
                {
                    udpReceiver.TransferData(memory);
                }
                udpReceiver.stop = true;
            })).Start();

            //refresh osci display
            new Thread(new ThreadStart(() =>
            {
                while(!formReady)
                    Thread.Sleep(10);
                
                while (!form.stopped)
                {
                    form.OnNewData();
                    Thread.Sleep(100);
                }
            })).Start();

            Application.Run(form);
        }

        static void Form1_Shown(object sender, EventArgs e)
        {
            formReady = true;
        }
    }
}
