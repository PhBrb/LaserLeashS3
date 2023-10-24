using System.Threading;
using MQTTnet.Client;
using System.Text;
using MQTTnet;

namespace ChartTest2
{

    public class MQTTPublisher
    {

        IMqttClient mqttClient;
        string ID { get => Properties.Settings.Default.StabilizerID; }
        int Channel { get => Properties.Settings.Default.Channel; }

        public MQTTPublisher(string server, int port)
        {
            MqttClientOptions options = new MqttClientOptionsBuilder()
                                     .WithTcpServer(server, port).Build();
            mqttClient = new MqttFactory().CreateMqttClient();
            mqttClient.ConnectAsync(options, CancellationToken.None);
            SpectrscopyControlForm.WriteLine("Starting MQTT client");

            //format with . instead of , as decimal separator
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-GB");
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
                SpectrscopyControlForm.WriteLine($"Sent double {value:0.####} to {path}");
            } else
            {
                SpectrscopyControlForm.WriteLine("MQTT client not connected");
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
                SpectrscopyControlForm.WriteLine($"Sent string {value} to {path}");
            }
            else
            {
                SpectrscopyControlForm.WriteLine("MQTT client not connected");
            }
        }


        public void sendScanAmplitude(double amplitude)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/signal_generator/{Channel}/amplitude", amplitude);
        }

        public void sendScanSymmetry(double symmetry)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/signal_generator/{Channel}/symmetry", symmetry);
        }

        public void sendScanOffset(double offset, double ymin, double ymax)
        {
            sendPID(offset, "[0.0,0.0,0.0,-0.0,-0.0]", ymin, ymax);
        }

        public void sendScanFrequency(double frequency)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/signal_generator/{Channel}/frequency", frequency);
        }

        public void sendPID(double offset, string iir, double ymin, double ymax)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/iir_ch/{Channel}/0",
                $"{{\"ba\":{iir},\"y_min\":{UnitConvert.YVToMU(ymin)},\"y_max\":{UnitConvert.YVToMU(ymax)},\"y_offset\":{UnitConvert.YVToMU(offset)}}}");
        }

        public void sendSignal()
        {
            /*var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic($"dt/sinara/dual-iir/{ID}/settings/signal_generator/0/signal")
                    .WithPayload($"\"Triangle\"")
                    .Build();
            mqttClient.PublishAsync(applicationMessage, CancellationToken.None);*/
            send($"dt/sinara/dual-iir/{ID}/settings/signal_generator/{Channel}/signal", $"\"Triangle\"");
        }

        public void sendPhase(double phase)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/pounder/out_channel/{Channel}/dds/phase_offset", phase);
        }

        public void sendModulationAmplitude(double amplitude)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/pounder/out_channel/{Channel}/dds/amplitude", amplitude);
        }

        public void sendDemodulationAmplitude(double amplitude)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/pounder/in_channel/{Channel}/dds/amplitude", amplitude);
        }

        public void sendModulationFrequencyMHz(double frequency)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/pounder/out_channel/{Channel}/dds/frequency", frequency*1000000);
        }
        public void sendDemodulationFrequencyMHz(double frequency)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/pounder/in_channel/{Channel}/dds/frequency", frequency*1000000);
        }

        public void sendModulationAttenuation(double attenuation)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/pounder/out_channel/{Channel}/attenuation", attenuation);
        }

        public void sendDemodulationAttenuation(double attenuation)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/pounder/in_channel/{Channel}/attenuation", attenuation);
        }

        public void sendStreamTarget(string ip, string port)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/stream_target",
                $"{{\"ip\":[{ip.Replace('.', ',')}],\"port\":{port}}}"); //IP is sent as an array, so replace . with ,
        }
    }
}