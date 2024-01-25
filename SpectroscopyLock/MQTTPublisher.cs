using System.Threading;
using MQTTnet.Client;
using System.Text;
using MQTTnet;

namespace LaserLeash
{
    public class MQTTPublisher
    {
        IMqttClient mqttClient;
        string stabilizerID { get => Properties.Settings.Default.StabilizerID; }
        int stabilizerChannel { get => Properties.Settings.Default.Channel; }

        /// <summary>
        /// Creates a connection to an MQTT server
        /// </summary>
        /// <param name="server"></param>
        /// <param name="port"></param>
        public MQTTPublisher(string server, int port)
        {
            connect(server, port);

            //format with . instead of , as decimal separator
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-GB");
        }

        public void connect(string server, int port)
        {
            mqttClient = new MqttFactory().CreateMqttClient();
            MqttClientOptions options = new MqttClientOptionsBuilder()
                                     .WithTcpServer(server, port).Build();
            mqttClient.ConnectAsync(options, CancellationToken.None);

            SpectroscopyControlForm.WriteLine("Starting MQTT client");
        }

        public void disconnect()
        {
            if (mqttClient.IsConnected)
            {
                mqttClient.DisconnectAsync();
            }
            mqttClient.Dispose();
        }

        private void send(string path, double value)
        {
            if (mqttClient.IsConnected)
            {
                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(path)
                    .WithPayload($"{value:0.####}")
                    .Build();

                mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
                SpectroscopyControlForm.WriteLine($"Sent double {value:0.####} to {path}");
            } else
            {
                SpectroscopyControlForm.WriteLine("MQTT client not connected");
            }
        }

        private void send(string path, string value)
        {
            if (mqttClient.IsConnected)
            {
                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(path)
                    .WithPayload(Encoding.ASCII.GetBytes(value))
                    .Build();

                mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
                SpectroscopyControlForm.WriteLine($"Sent string {value} to {path}");
            }
            else
            {
                SpectroscopyControlForm.WriteLine("MQTT client not connected");
            }
        }


        public void sendScanAmplitude(double amplitude)
        {
            send($"dt/sinara/dual-iir/{stabilizerID}/settings/signal_generator/{stabilizerChannel}/amplitude", amplitude);
        }

        public void sendScanSymmetry(double symmetry)
        {
            send($"dt/sinara/dual-iir/{stabilizerID}/settings/signal_generator/{stabilizerChannel}/symmetry", symmetry);
        }

        public void sendScanOffset(double offset, double ymin, double ymax)
        {
            sendPID(offset, "[0.0,0.0,0.0,-0.0,-0.0]", ymin, ymax);
        }

        public void sendScanFrequency(double frequency)
        {
            send($"dt/sinara/dual-iir/{stabilizerID}/settings/signal_generator/{stabilizerChannel}/frequency", frequency);
        }

        public void sendPID(double offset, string iir, double ymin, double ymax)
        {
            send($"dt/sinara/dual-iir/{stabilizerID}/settings/iir_ch/{stabilizerChannel}/0",
                $"{{\"ba\":{iir},\"y_min\":{UnitConvert.YVToMU(ymin)},\"y_max\":{UnitConvert.YVToMU(ymax)},\"y_offset\":{UnitConvert.YVToMU(offset)}}}");
        }

        public void sendSignal()
        {
            send($"dt/sinara/dual-iir/{stabilizerID}/settings/signal_generator/{stabilizerChannel}/signal", $"\"Triangle\"");
        }

        public void sendPhase(double phase)
        {
            send($"dt/sinara/dual-iir/{stabilizerID}/settings/pounder/out_channel/{stabilizerChannel}/dds/phase_offset", phase);
        }

        public void sendModulationAmplitude(double amplitude)
        {
            send($"dt/sinara/dual-iir/{stabilizerID}/settings/pounder/out_channel/{stabilizerChannel}/dds/amplitude", amplitude);
        }

        public void sendDemodulationAmplitude(double amplitude)
        {
            send($"dt/sinara/dual-iir/{stabilizerID}/settings/pounder/in_channel/{stabilizerChannel}/dds/amplitude", amplitude);
        }

        public void sendModulationFrequencyMHz(double frequency)
        {
            send($"dt/sinara/dual-iir/{stabilizerID}/settings/pounder/out_channel/{stabilizerChannel}/dds/frequency", frequency*1000000);
        }
        public void sendDemodulationFrequencyMHz(double frequency)
        {
            send($"dt/sinara/dual-iir/{stabilizerID}/settings/pounder/in_channel/{stabilizerChannel}/dds/frequency", frequency*1000000);
        }

        public void sendModulationAttenuation(double attenuation)
        {
            send($"dt/sinara/dual-iir/{stabilizerID}/settings/pounder/out_channel/{stabilizerChannel}/attenuation", attenuation);
        }

        public void sendDemodulationAttenuation(double attenuation)
        {
            send($"dt/sinara/dual-iir/{stabilizerID}/settings/pounder/in_channel/{stabilizerChannel}/attenuation", attenuation);
        }

        public void sendStreamTarget(string ip, string port)
        {
            send($"dt/sinara/dual-iir/{stabilizerID}/settings/stream_target",
                $"{{\"ip\":[{ip.Replace('.', ',')}],\"port\":{port}}}"); //IP is an array, so replace . with ,
        }
    }
}