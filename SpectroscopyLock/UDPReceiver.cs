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
        bool stop = false;

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
        Task UDPListener()
        {
            return Task.Run(() =>
            {
                using (var udpClient = new UdpClient(1883))
                {
                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                    while (!stop)
                    {
                        byte[] receivedResults = udpClient.Receive(ref remoteEndPoint);
                        lastRawData = receivedResults; ///this is the only code that writes to <see cref="lastRawData"/>. Since udp.Receive returns a new array this should be safe/consistent to read at any time, if the reader has a copy of the reference
                    }
                }
            });
        }
    }
}
