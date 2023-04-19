using System;
using System.IO;
namespace ChartTest2
{
    public static class Deserializer
    {
        /// <summary>
        /// Data at the beginning of each packet
        /// </summary>
        struct header
        {
            public ushort magic;
            public byte formatId;
            public byte batchSize;
            public uint sequenceNumber;
        }

        static uint firstSequenceNumber = 0;
        static uint skipped = 0;

        public static void Deserialize(byte[] rawData, Memory memory)
        {
            //rawData = File.ReadAllBytes(1027017276 + "packet.bytes"); //read data from a file for debuging purpose

            header h;
            h.magic = BitConverter.ToUInt16(rawData, 0);
            h.formatId = rawData[2];
            h.batchSize = rawData[3];
            h.sequenceNumber = BitConverter.ToUInt32(rawData, 4);

            if (h.sequenceNumber == memory.lastSequenceNumber) //cancel if already processed previously
                return;
            if (firstSequenceNumber == 0)
                firstSequenceNumber = h.sequenceNumber;

            int skip = (int)(h.sequenceNumber - memory.lastSequenceNumber) / 22 - 1; //not perfectly accurate, will be wrong once, when sequenceNumber has an overflow
            if(skip > 0 && skip < 1000) //skip only of no overflow and if not too much to skip (eg at start)
            {
                memory.ADCSkip(skip*22*8);//22 batches per frame, 8 numbers per batch
                memory.DACSkip(skip*22*8);
                skipped += (uint)skip * 22;

                Console.WriteLine((float)skipped/(h.sequenceNumber - firstSequenceNumber));
            }

            memory.lastSequenceNumber = h.sequenceNumber;


            if (h.magic != 0x57B)
                throw new ArgumentOutOfRangeException("wrong magic number");
            if (h.formatId != 1 || h.batchSize != 8)
                throw new ArgumentOutOfRangeException("unsupported formating");

            //I think the structure is like this (with batch size 8)
            //array([[ 0,  1,  2,  3,  4,  5,  6,  7, 32, 33, 34, 35, 36, 37, 38, 39, 64, 65, 66, 67, 68, 69, 70, 71],
            //[ 8,  9, 10, 11, 12, 13, 14, 15, 40, 41, 42, 43, 44, 45, 46, 47, 72, 73, 74, 75, 76, 77, 78, 79],
            //[16, 17, 18, 19, 20, 21, 22, 23, 48, 49, 50, 51, 52, 53, 54, 55, 80, 81, 82, 83, 84, 85, 86, 87],
            //[24, 25, 26, 27, 28, 29, 30, 31, 56, 57, 58, 59, 60, 61, 62, 63, 88, 89, 90, 91, 92, 93, 94, 95]])
            // first dimension element 0 and 1 are adc, 1 and 2 are dac
            int batchSize = 8;
            int headerSize = 8;
            int iPos = 0;
            //TODO: if this gets refactored, use a byterunner instead of calculating which byte to read. avoids bugs when calculating the position.
            for (int iBatch = 0; iBatch < 22; iBatch++)//176 2 byte intergers per frame -> with batch size 8 thats 22 batches per frame per channel
            {
                for (int iFloat = 0; iFloat < batchSize; iFloat++)
                {
                    iPos = iBatch * batchSize * 2 * 4 + iFloat * 2 + headerSize;
                    memory.ADCEnqueue(BitConverter.ToInt16(rawData, iPos)  / ((1 << 16) / (4.096f * 5))); //TODO: is this correct? The python script uses the DAC conversion factor also for the ADC, and ignores the also existing factor for ADC
                }
                for (int iFloat = 0; iFloat < batchSize; iFloat++)
                {
                    iPos = iBatch * batchSize * 2 * 4 + iFloat * 2 + headerSize + 2 * batchSize * 2;
                    memory.DACEnqueue((BitConverter.ToInt16(rawData, iPos) ^ (unchecked((short)0x8000)))  / ((1 << 16) / (4.096f * 5)));
                }

                //File.WriteAllBytes(h.sequenceNumber + "packet.bytes", rawData); //save data to a file for debuging purpose
            }
            if (BitConverter.ToUInt32(rawData, 4) != h.sequenceNumber)
                throw new Exception("converting data wasnt thread safe");
        }
    }
}
