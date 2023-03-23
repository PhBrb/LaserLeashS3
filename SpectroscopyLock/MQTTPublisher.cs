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

        public void sendAmplitude(double amplitude)
        {
            if (mqttClient.IsConnected)
            {
                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("dt/sinara/dual-iir/04-91-62-d2-60-2f/settings/signal_generator/0/amplitude")
                    .WithPayload($"{amplitude:0.####}")
                    .Build();

                mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
                Console.WriteLine($"Sent amplitude {amplitude:0.####}");
            }
        }

        public void sendOffset(double offset)
        {
            if (mqttClient.IsConnected)
            {
                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("dt/sinara/dual-iir/04-91-62-d2-60-2f/settings/signal_generator/0/offset")
                    .WithPayload($"{offset:0.####}")
                    .Build();

                mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
                Console.WriteLine($"Sent offset{offset:0.####}");
            }
        }

        public void sendSweepFrequency(double frequency)
        {
            if (mqttClient.IsConnected)
            {
                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("dt/sinara/dual-iir/04-91-62-d2-60-2f/settings/signal_generator/0/frequency")
                    .WithPayload($"{frequency:0.####}")
                    .Build();

                mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
                Console.WriteLine("MQTT application message is published");
            }
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
                Console.WriteLine("sent pid");
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


    }
}