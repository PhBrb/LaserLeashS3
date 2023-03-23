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
        public class Buffer
        {
            public byte[] lastRawData = new byte[] {0x7C, 0x5, 1, 8, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        }


        Task thread;
        bool stop = false;
        public Buffer b;

        public UDPReceiver()
        {
            b = new Buffer();

            thread = UDPListener();
        }

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
                        b.lastRawData = receivedResults;
                    }
                }
            });
        }
    }
}
