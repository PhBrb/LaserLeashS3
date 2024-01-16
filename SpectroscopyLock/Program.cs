using System;
using System.Windows.Forms;
using System.Threading;

namespace LaserLeash
{
    internal static class Program
    {
        public static UDPReceiver udpReceiver;
        static Thread deserializerThread;
        static Thread displayRefreshThread;

        static bool formReady = false;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            //Data flow: udpReceiver -> deserializer -> memory -> oscidisplay -> form -> (user input) -> mqtt
            udpReceiver = new UDPReceiver();
            Memory memory = new Memory(1000000); //5 000 000 is roughly 7s
            OsciDisplay osciDisplay = new OsciDisplay(memory);

            MQTTPublisher mqttPublisher = new MQTTPublisher(Properties.Settings.Default.MQTTServer, Properties.Settings.Default.MQTTPort);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            SpectroscopyControlForm form = new SpectroscopyControlForm(memory, osciDisplay, mqttPublisher);
            form.Shown += new System.EventHandler(Form1_Shown);

            //delay starting the UDP thread, as it accesses the form
            new Thread(new ThreadStart(() =>
            {
                while (!formReady)
                    Thread.Sleep(10);
                udpReceiver.Start();
            })).Start();
            
            //Transfer data from UDP to memory
            deserializerThread = new Thread(new ThreadStart(() =>
            {
                while (!formReady)
                    Thread.Sleep(10);

                while (!SpectroscopyControlForm.stopped)
                {
                    udpReceiver.DeserializeTo(memory);
                }
            }));
            deserializerThread.Start();

            //Refresh the dislayed data
            displayRefreshThread = new Thread(new ThreadStart(() =>
            {
                while(!formReady)
                    Thread.Sleep(10);
                
                while (!SpectroscopyControlForm.stopped)
                {
                    form.RefreshData();
                    Thread.Sleep(50);
                }
            }));
            displayRefreshThread.Start();

            Application.Run(form);
        }

        static void Form1_Shown(object sender, EventArgs e)
        {
            formReady = true;
            ((SpectroscopyControlForm) sender).LoadSettings();
        }
    }
}
