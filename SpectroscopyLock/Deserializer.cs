using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
        public static int mod(int x, int m)
        {
            return (x % m + m) % m;
        }

        public void TransferData(byte[] rawData)
        {
            //rawData = File.ReadAllBytes(1027017276 + "packet.bytes");

            header h;
            h.magic = BitConverter.ToUInt16(rawData, 0);
            h.formatId = rawData[2];
            h.batchSize = rawData[3];
            h.sequenceNumber = BitConverter.ToUInt32(rawData, 4);

            if (h.magic != 0x57B)
                throw new ArgumentOutOfRangeException("wrong magic number");
            if (h.formatId != 1)
                throw new ArgumentOutOfRangeException("unsupported formating");

            //i think the structure is like this (with batch size 8)
            //array([[ 0,  1,  2,  3,  4,  5,  6,  7, 32, 33, 34, 35, 36, 37, 38, 39, 64, 65, 66, 67, 68, 69, 70, 71],
            //[ 8,  9, 10, 11, 12, 13, 14, 15, 40, 41, 42, 43, 44, 45, 46, 47, 72, 73, 74, 75, 76, 77, 78, 79],
            //[16, 17, 18, 19, 20, 21, 22, 23, 48, 49, 50, 51, 52, 53, 54, 55, 80, 81, 82, 83, 84, 85, 86, 87],
            //[24, 25, 26, 27, 28, 29, 30, 31, 56, 57, 58, 59, 60, 61, 62, 63, 88, 89, 90, 91, 92, 93, 94, 95]])
            // first dimension element 0 and 1 are adc, 1 and 2 are dac
            int batchSize = 8;
            int headerSize = 8;
            int iPos = 0;
            //TODO: if this gets refactored, use a byterunner instead of calculating which byte to read. avoids bugs when calculating the position.
            for (int iBatch = 0; iBatch < rawData.Length / (batchSize*4*2); iBatch++) //2 bytes, 4 arrays
            {
                for (int iFloat = 0; iFloat < batchSize; iFloat++)
                {
                    iPos = iBatch * batchSize * 2 * 4 + iFloat * 2 + headerSize;
                    osciData.adc0Rolling[osciPosition + iFloat] = BitConverter.ToInt16(rawData, iPos)  / ((1 << 16) / (4.096f * 5)); //TODO: is this correct? The python script uses the DAC conversion factor also for the ADC, and ignores the also existing factor for ADC
                }
                for (int iFloat = 0; iFloat < batchSize; iFloat++)
                {
                    iPos = iBatch * batchSize * 2 * 4 + iFloat * 2 + headerSize + batchSize * 2;
                    osciData.adc1Rolling[osciPosition + iFloat] = BitConverter.ToInt16(rawData, iPos)  / ((1 << 16) / (4.096f * 5));
                    //Console.WriteLine(osciData.dac0Rolling[osciPosition + iFloat]);
                }
                for (int iFloat = 0; iFloat < batchSize; iFloat++)
                {
                    iPos = iBatch * batchSize * 2 * 4 + iFloat * 2 + headerSize + 2 * batchSize * 2;
                    osciData.dac0Rolling[osciPosition + iFloat] = (BitConverter.ToInt16(rawData, iPos) ^ (unchecked((short)0x8000)))  / ((1 << 16) / (4.096f * 5));
                }
                for (int iFloat = 0; iFloat < batchSize; iFloat++)
                {
                    iPos = iBatch * batchSize * 2 * 4 + iFloat * 2 + headerSize + 3 * batchSize * 2;
                    osciData.dac1Rolling[osciPosition + iFloat] = (BitConverter.ToInt16(rawData, iPos) ^ (unchecked((short)0x8000))) / ((1 << 16) / (4.096f * 5));
                }


                //transfer last batchSize numbers to xy data storage
                for (int iFloat = 0; iFloat < batchSize; iFloat++)
                {
                    int index = mod(osciPosition, osciData.dac0Rolling.Length);
                    osciData.setDatapoint(osciData.dac0Rolling[index], osciData.adc0Rolling[index]);
                }


                osciPosition = (osciPosition + batchSize) % osciData.dac0Rolling.Length;
                //File.WriteAllBytes(h.sequenceNumber + "packet.bytes", rawData);
            }


        }


    }
}
