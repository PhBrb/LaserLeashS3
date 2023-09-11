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
        /// Stops the thread
        /// </summary>
        public bool stop = false;

        ReuseBuffer buffer = new ReuseBuffer(500); //randomly chosen buffer size

        /// <summary>
        /// Starts a continuously running thread that receives and buffers data
        /// </summary>
        public UDPReceiver()
        {
            Receive();
        }

        /// <summary>
        /// Starts continuously running thread that writes received data into the buffer
        /// </summary>
        /// <returns></returns>
        void Receive()
        {
            Thread thread = new Thread(new ThreadStart(() =>
            {
                using (var udpClient = new UdpClient(1883))
                {
                    udpClient.Client.ReceiveTimeout = 5000;
                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    uint lastSequenceNumber = 0;
                    uint skipped = 0;
                    while (!stop)
                    {
                        try
                        {
                            ReuseBuffer.Frame frame = buffer.getNextBuffer(); 
                            udpClient.Client.Receive(frame.data);

                            frame.calcMetadata();

                            int skip = (int)(frame.sequenceNumber - lastSequenceNumber) / 22 - 1;
                            lastSequenceNumber= frame.sequenceNumber;
                            if (skip > 0 && skip < 1000)
                            {
                                skipped += (uint)skip * 22;
                            }
                        } 
                        catch (SocketException)
                        {
                            SpectrscopyControlForm.WriteLine("Timeout receiving streamed data");
                        }
                    }
                }
            }));
            //thread.Priority = ThreadPriority.Highest;// this needs more checking, there is no point in increasing this if its just burning recources, that could be used elsewhere
            thread.Start();
        }

        /// <summary>
        /// Transfers data from the buffer to bigger memory and converts from machine units to floats
        /// </summary>
        /// <param name="d"></param>
        /// <param name="osciData"></param>
        public void TransferData(Memory osciData)
        {
            ReuseBuffer.Frame frame = buffer.getNextFrame();
            Deserializer.Deserialize(frame.data, osciData);
        }
    }
}
