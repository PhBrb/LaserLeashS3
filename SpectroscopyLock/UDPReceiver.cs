using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ChartTest2
{
    public class UDPReceiver
    {
        /// <summary>
        /// Buffer for last received data
        /// </summary>
        private byte[] lastRawData = new byte[] {};
        
        public int Size { get { return lastRawData.Length; } }

        /// <summary>
        /// Stop the thread
        /// </summary>
        public bool stop = false;

        /// <summary>
        /// Starts a continuously running thread that receives and buffers data
        /// </summary>
        public UDPReceiver()
        {
            UDPListener();
        }

        /// <summary>
        /// Transfers data from the buffer to bigger memory and converts from machine units to floats
        /// </summary>
        /// <param name="d"></param>
        /// <param name="osciData"></param>
        public void TransferData( Memory osciData)
        {
            Deserializer.Deserialize(lastRawData, osciData);
        }

        /// <summary>
        /// Starts continuously running thread that writes received data into the buffer
        /// </summary>
        /// <returns></returns>
        void UDPListener()
        {
            Thread thread = new Thread(new ThreadStart(() =>
            {
                using (var udpClient = new UdpClient(1883))
                {
                    udpClient.Client.ReceiveTimeout = 5000;
                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    uint lastSequenceNumber = 0;
                    uint firstSequenceNumber = 0;
                    uint skipped = 0;
                    while (!stop)
                    {
                        try
                        {
                            byte[] receivedResults = udpClient.Receive(ref remoteEndPoint); //TODO there is udpClient.Client.Receive... which takes in a buffer. could be used to improve memory usage

                            lastRawData = receivedResults; ///this is the only code that writes to <see cref="lastRawData"/>. Since udp.Receive returns a new array this should be safe/consistent to read at any time, if the reader has a copy of the reference

                            uint sequenceNumber = BitConverter.ToUInt32(receivedResults, 4);
                            int skip = (int)(sequenceNumber - lastSequenceNumber) / 22 - 1;
                            if (skip > 0 && skip < 1000)
                            {
                                skipped += (uint)skip * 22;
                                Console.WriteLine("NetworkSkip: " + (float)skipped / (sequenceNumber - firstSequenceNumber));
                            }
                        } catch (System.Net.Sockets.SocketException)
                        {
                            SpectrscopyControlForm.WriteLine("Timeout receiving streamed data");
                        }
                    }
                }
            }));
            thread.Priority = ThreadPriority.Highest;
            thread.Start();
        }
    }
}
