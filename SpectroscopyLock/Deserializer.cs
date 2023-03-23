using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ChartTest2
{
    public class Deserializer
    {
        struct header
        {
            public ushort magic;
            public byte formatId;
            public byte batchSize;
            public uint sequenceNumber;
        }

        public OsciData osciData { get; private set; }

        int osciPosition = 0;

        public Deserializer(OsciData osciData)
        {
            this.osciData = osciData;
        }

        /// <summary>
        /// mod(-4,3) = 2
        /// </summary>
        /// <param name="x"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        int mod(int x, int m)
        {
            return (x % m + m) % m;
        }

        public void TransferData(byte[] rawData)
        {
            header h;
            h.magic = BitConverter.ToUInt16(rawData, 0);
            h.formatId = rawData[2];
            h.batchSize = rawData[3];
            h.sequenceNumber = BitConverter.ToUInt32(rawData, 4);
            Console.WriteLine(h.sequenceNumber);

            if (h.magic != 0x57B)
                throw new Exception("wrong magic number");
            if (h.formatId != 1)
                throw new Exception("unsupported formatingg");

            //i think the structure is like this (with batch size 8)
            //array([[ 0,  1,  2,  3,  4,  5,  6,  7, 32, 33, 34, 35, 36, 37, 38, 39, 64, 65, 66, 67, 68, 69, 70, 71],
            //[ 8,  9, 10, 11, 12, 13, 14, 15, 40, 41, 42, 43, 44, 45, 46, 47, 72, 73, 74, 75, 76, 77, 78, 79],
            //[16, 17, 18, 19, 20, 21, 22, 23, 48, 49, 50, 51, 52, 53, 54, 55, 80, 81, 82, 83, 84, 85, 86, 87],
            //[24, 25, 26, 27, 28, 29, 30, 31, 56, 57, 58, 59, 60, 61, 62, 63, 88, 89, 90, 91, 92, 93, 94, 95]])
            // first dimension element 0 and 1 are adc, 1 and 2 are dac
            int batchSize = 8;
            int headerSize = 8;
            for (int iBatch = 0; iBatch < rawData.Length / (batchSize*4*2); iBatch+=4) //2 bytes, 4 arrays
            {
                for (int iFloat = 0; iFloat < batchSize; iFloat++)
                {
                    osciData.adc0[osciPosition] = BitConverter.ToUInt16(rawData, iBatch * batchSize * 2 + iFloat*2 + headerSize) * (5.0f / 2.0f * 4.096f) / (1 << 15);
                }
                for (int iFloat = 0; iFloat < batchSize; iFloat++)
                {
                    osciData.adc1[osciPosition] = BitConverter.ToUInt16(rawData, (iBatch + 1) * batchSize * 2 + iFloat*2 + headerSize) * (5.0f / 2.0f * 4.096f) / (1 << 15);
                }
                for (int iFloat = 0; iFloat < batchSize; iFloat++)
                {
                    osciData.dac0[osciPosition] = (BitConverter.ToUInt16(rawData, (iBatch + 2) * batchSize * 2 + iFloat * 2 + headerSize) ^ (0x8000)) / ((1 << 16) / (4.096f * 5));
                }
                for (int iFloat = 0; iFloat < batchSize; iFloat++)
                {
                    osciData.dac1[osciPosition] = (BitConverter.ToUInt16(rawData, (iBatch + 3) * batchSize * 2 + iFloat*2 + headerSize) ^ (0x8000)) / ((1 << 16) / (4.096f * 5));
                }

                //transfer last batchSize numbers to xy data storage
                for (int iFloat = 0; iFloat < batchSize; iFloat++)
                {
                    int index = mod(-iFloat, osciData.dac0.Length);
                    osciData.setDatapoint(osciData.adc0[index], osciData.dac0[index]);
                }


                osciPosition = (osciPosition+ 1) % osciData.dac0.Length;
            }
        }


    }
}
