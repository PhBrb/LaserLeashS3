using System.Threading.Tasks;
using System.Threading;
using System;
using MQTTnet.Client;
using MQTTnet.Server;

namespace MQTTnet.Samples.Client
{

    public class MQTTPublisher
    {

        IMqttClient mqttClient;

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
                Console.WriteLine("MQTT application message is published");
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
                Console.WriteLine("MQTT application message is published");
            }
        }

        public void sendScanAmplitude(double amplitude)
        {
            send("dt/sinara/dual-iir/04-91-62-d2-60-2f/settings/signal_generator/0/amplitude", amplitude);
        }

        public void sendScanOffset(double offset)
        {
            send("dt/sinara/dual-iir/04-91-62-d2-60-2f/settings/signal_generator/0/offset", offset);
        }

        public void sendScanFrequency(double frequency)
        {
            send("dt/sinara/dual-iir/04-91-62-d2-60-2f/settings/signal_generator/0/frequency", frequency);
        }

        public void sendPID()
        {
            if (mqttClient.IsConnected)
            {
                //var applicationMessage = new MqttApplicationMessageBuilder()
                //    .WithTopic("dt/sinara/dual-iir/04-91-62-d2-60-2f/settings/signal_generator/0/frequency")
                //    .WithPayload($"{frequency:0.####}")
                //    .Build();

                //mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

            }
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
            send("dt/sinara/dual-iir/04-91-62-d2-60-2f/settings/pounder/out_channel/0/dds/phase_offset", phase);
        }

        public void sendModulationAmplitude(double amplitude)
        {
            send("dt/sinara/dual-iir/04-91-62-d2-60-2f/settings/pounder/out_channel/0/dds/amplitude", amplitude);
        }

        public void sendDemodulationAmplitude(double amplitude)
        {
            send("dt/sinara/dual-iir/04-91-62-d2-60-2f/settings/pounder/in_channel/0/dds/amplitude", amplitude);
        }

        public void sendModulationFrequency(double frequency)
        {
            send("dt/sinara/dual-iir/04-91-62-d2-60-2f/settings/pounder/out_channel/0/dds/frequency", frequency);
        }
        public void sendDemodulationFrequency(double frequency)
        {
            send("dt/sinara/dual-iir/04-91-62-d2-60-2f/settings/pounder/in_channel/0/dds/frequency", frequency);
        }

        public void sendModulationAttenuation(double attenuation)
        {
            send("dt/sinara/dual-iir/04-91-62-d2-60-2f/settings/pounder/out_channel/0/attenuation", attenuation);
        }

        public void sendDemodulationAttenuation(double attenuation)
        {
            send("dt/sinara/dual-iir/04-91-62-d2-60-2f/settings/pounder/in_channel/0/attenuation", attenuation);
        }


    }
}