using System.Threading.Tasks;
using System.Threading;
using System;
using MQTTnet.Client;
using MQTTnet.Server;
using System.Text;
using ChartTest2;

namespace MQTTnet.Samples.Client
{

    public class MQTTPublisher
    {

        IMqttClient mqttClient;
        string ID = "04-91-62-d2-60-2f";

        public MQTTPublisher()
        {
            var mqttFactory = new MqttFactory();
            mqttClient = mqttFactory.CreateMqttClient();


            //format with . instead of , as decimal separator
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-GB");
        }

        public void connect()
        {
            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer("localhost")
                .Build();

            mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
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
                SpectrscopyControlForm.WriteLine($"Sent  double {value:0.####} to {path}");
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
            send($"dt/sinara/dual-iir/{ID}/settings/signal_generator/0/amplitude", amplitude);
        }

        public void sendScanSymmetry(double symmetry)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/signal_generator/0/symmetry", symmetry);
        }

        public void sendScanOffset(double offset, double ymin, double ymax)
        {
            sendPID(offset, "[0.0,0.0,0.0,-0.0,-0.0]", ymin, ymax);
        }

        public void sendScanFrequency(double frequency)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/signal_generator/0/frequency", frequency);
        }

        public void sendPID(double offset, string iir, double ymin, double ymax)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/iir_ch/0/0",
                $"{{\"ba\":{iir},\"y_min\":{UnitConvert.YVToMU(ymin)},\"y_max\":{UnitConvert.YVToMU(ymax)},\"y_offset\":{UnitConvert.YVToMU(offset)}}}");
        }


        public void sendPhase(double phase)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/pounder/out_channel/0/dds/phase_offset", phase);
        }

        public void sendModulationAmplitude(double amplitude)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/pounder/out_channel/0/dds/amplitude", amplitude);
        }

        public void sendDemodulationAmplitude(double amplitude)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/pounder/in_channel/0/dds/amplitude", amplitude);
        }

        public void sendModulationFrequencyMHz(double frequency)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/pounder/out_channel/0/dds/frequency", frequency*1000000);
        }
        public void sendDemodulationFrequencyMHz(double frequency)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/pounder/in_channel/0/dds/frequency", frequency*1000000);
        }

        public void sendModulationAttenuation(double attenuation)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/pounder/out_channel/0/attenuation", attenuation);
        }

        public void sendDemodulationAttenuation(double attenuation)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/pounder/in_channel/0/attenuation", attenuation);
        }

        public void sendStreamTarget(string ip, string port)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/stream_target",
                $"{{\"ip\":[{ip.Replace('.', ',')}],\"port\":{port}}}"); //IP is sent as an array, so replay . wiht ,
        }

        public void setStabilizerID(string id)
        {
            ID = id;
        }
    }
}