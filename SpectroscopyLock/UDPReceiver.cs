using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace LaserLeash
{
    public class UDPReceiver
    {
        private Thread thread;
        public int port;

        ReuseBuffer buffer = new ReuseBuffer(500); //randomly chosen buffer size

        /// <summary>
        /// Starts a continuously running thread that receives and buffers data
        /// </summary>
        public UDPReceiver()
        {
            port = Properties.Settings.Default.StreamPort;
            thread = new Thread(new ThreadStart(() =>
            {
                UdpClient udpClient = null;
                uint lastSequenceNumber = 0;
                uint firstSequenceNumber = 0;
                uint skipped = 0;
                while (!SpectroscopyControlForm.stopped)
                {
                    if (udpClient == null || ((IPEndPoint)udpClient.Client.LocalEndPoint).Port != port)
                    {
                        try
                        {
                            if (udpClient != null)
                            {
                                udpClient.Close();
                                udpClient.Dispose();
                            }
                            udpClient = new UdpClient(port);
                            udpClient.Client.ReceiveTimeout = 5000;
                            SpectroscopyControlForm.WriteLine("Started UDP listener");
                        }
                        catch
                        {
                            SpectroscopyControlForm.WriteLine("Could not start UDP listener");
                            Thread.Sleep(2000);
                            continue;
                        }
                    }

                    try
                    {
                        ReuseBuffer.Frame frame = buffer.getNextBuffer();
                        if (frame == null)
                            continue;
                        udpClient.Client.Receive(frame.data);
                        frame.calcMetadata();

#if DEBUG //calculate skipped frames
                        int skip = (int)(frame.sequenceNumber - lastSequenceNumber) / 22 - 1;
                        lastSequenceNumber = frame.sequenceNumber;
                        if (firstSequenceNumber == 0)
                            firstSequenceNumber = lastSequenceNumber - 1;
                        if (skip > 0 && skip < 1000)
                        {
                            skipped += (uint)skip * 22;
                            Console.WriteLine($"Skipped: {(100.0 * skipped) / (lastSequenceNumber - firstSequenceNumber) : 0.00} %");
                        }
#endif
                    }
                    catch (SocketException)
                    {
                        SpectroscopyControlForm.WriteLine("Timeout receiving data stream");
                    }
                }
                if (udpClient != null)
                {
                    udpClient.Close();
                    udpClient.Dispose();
                }
            }));
        }

        /// <summary>
        /// Starts a continuously running thread that writes received data into the buffer
        /// </summary>
        /// <returns></returns>
        public void Start()
        {
            thread.Start();
        }

        /// <summary>
        /// Transfers data from the buffer to bigger memory and converts from machine units to floats
        /// </summary>
        /// <param name="d"></param>
        /// <param name="memory"></param>
        public void DeserializeTo(Memory memory)
        {
            ReuseBuffer.Frame frame = buffer.getNextFrame();
            if(frame == null)
                return;
            lock(memory)
                Deserializer.Deserialize(frame.data, memory);
        }
    }
}
