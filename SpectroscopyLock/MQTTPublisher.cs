using System.Threading.Tasks;
using System.Threading;
using System;
using MQTTnet.Client;
using MQTTnet.Server;
using System.Text;

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


            //make sure strings get formated with . instead of , as decimal separator
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
                Console.WriteLine($"Sent double {value:0.####} to {path}");
            }
        }

        private void send(string path, int value)
        {
            if (mqttClient.IsConnected)
            {
                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(path)
                    .WithPayload($"{value}")
                    .Build();

                mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
                Console.WriteLine($"Sent int {value} to {path}");
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
                Console.WriteLine($"Sent string {value} to {path}");
            }
        }


        public void sendScanAmplitude(double amplitude)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/signal_generator/0/amplitude", amplitude);
        }

        public void sendScanOffset(double offset)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/signal_generator/0/offset", offset);
        }

        public void sendScanFrequency(double frequency)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/signal_generator/0/frequency", frequency);
        }

        public void sendPID()
        {
            //add printing of topic and payload to miniconf.py to find out what parameters get sent. (might not be needed, the iir_coefficients script  gives some logging info)
            //use eg python iir_coefficients.py --v -b 127.0.0.1 --prefix 04-91-62-d2-60-2f --no-discover --c 0 --sample-period 0.1 --x-offset 0 --y-min 0 --y-offset 0 pid --Kii 0 --Ki 0.01 --Kp 1 --Kd 0 --Kdd 0
            send($"dt/sinara/dual-iir/{ID}/settings/iir_ch/0/0", "{\"ba\":[1.0062831853071796,-1.0,0.0,1.0,-0.0],\"y_min\":0,\"y_max\":32767,\"y_offset\":0}");
        }

        public void sendPIDOff()
        {
            //add printing of topic and payload to miniconf.py to find out what parameters get sent. (might not be needed, the iir_coefficients script gives some logging info)
            //use eg python iir_coefficients.py --v -b 127.0.0.1 --prefix 04-91-62-d2-60-2f --no-discover --c 0 --sample-period 0.1 --x-offset 0 --y-min 0 --y-offset 0 pid --Kii 0 --Ki 0.01 --Kp 1 --Kd 0 --Kdd 0
            send($"dt/sinara/dual-iir/{ID}/settings/iir_ch/0/0", "{\"ba\":[0.0,0.0,0.0,-0.0,-0.0],\"y_min\":0,\"y_max\":32767,\"y_offset\":0}");
        }

        public bool isConnected()
        {
            return mqttClient.IsConnected;
        }


        public void disconnect()
        {
            mqttClient.DisconnectAsync();
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

        public void sendModulationFrequency(double frequency)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/pounder/out_channel/0/dds/frequency", frequency);
        }
        public void sendDemodulationFrequency(double frequency)
        {
            send($"dt/sinara/dual-iir/{ID}/settings/pounder/in_channel/0/dds/frequency", frequency);
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
            send($"dt/sinara/dual-iir/{ID}/settings/stream_target",  $"{{\"ip\":[{ip}],\"port\":{port}}}");
        }

        public void setStabilizerID(string id)
        {
            ID = id;
        }
    }
}