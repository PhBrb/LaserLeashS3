using System;

namespace LaserLeash
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
            public const int size = 2 + 1 + 1 + 4;
        }

        public const int batchesPerFrame = 22;
        private static uint lastSequenceNumber;
        private static int consecutiveTimeJumpsBack;

        /// <summary>
        /// Deserializes stream data and writes it to memory
        /// </summary>
        /// <param name="rawData"></param>
        /// <param name="memory"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void Deserialize(byte[] rawData, Memory memory)
        {
            header h;
            h.magic = BitConverter.ToUInt16(rawData, 0);
            h.formatId = rawData[2];
            h.batchSize = rawData[3];
            h.sequenceNumber = BitConverter.ToUInt32(rawData, 4);

            if (h.sequenceNumber == lastSequenceNumber) //cancel if already processed previously
                return;

            lock (memory.locker)
            {
                //handle not monotonically increasing sequenceNumbers
                if (h.sequenceNumber < lastSequenceNumber)
                {
                    consecutiveTimeJumpsBack += 1;
                    SpectroscopyControlForm.WriteLine("Received data out of order");

                    //if consistent time jump backwards happened eg. on sequence number overflow or stabilizer reset
                    if (consecutiveTimeJumpsBack > 7)
                    {
                        SpectroscopyControlForm.WriteLine("Time jump detected, resetting memory.");
                        memory.clear();
                    }
                    else
                    {
                        try
                        {
                            //if time jumped back more than the memory size, ignore data
                            if (checked((lastSequenceNumber - h.sequenceNumber + batchesPerFrame) * 8) > memory.getSize())
                            {
                                SpectroscopyControlForm.WriteLine($"Rejecting data as it is older than the memory size.");
                                return;
                            }
                        }
                        catch (OverflowException)
                        {
                            SpectroscopyControlForm.WriteLine($"Rejecting data as it is older than the memory size.");
                            return;
                        }
                    }
                }
                else
                {
                    consecutiveTimeJumpsBack = 0;
                }

                lastSequenceNumber = h.sequenceNumber;

                if (h.magic != 0x57B)
                    throw new ArgumentOutOfRangeException("wrong magic number");
                if (h.formatId != 1)
                    throw new ArgumentOutOfRangeException("unsupported header formating");
                if (h.batchSize != 8)
                    throw new ArgumentOutOfRangeException("unsupported batch size");
                if (rawData.Length - header.size != 176 * 4 * 2) // 4 arrays, 2 bytes per array element
                    throw new ArgumentOutOfRangeException("unsupported array size");

                //I think the structure is like this (with batch size 8)
                //array([[ 0,  1,  2,  3,  4,  5,  6,  7, 32, 33, 34, 35, 36, 37, 38, 39, 64, 65, 66, 67, 68, 69, 70, 71],
                //[ 8,  9, 10, 11, 12, 13, 14, 15, 40, 41, 42, 43, 44, 45, 46, 47, 72, 73, 74, 75, 76, 77, 78, 79],
                //[16, 17, 18, 19, 20, 21, 22, 23, 48, 49, 50, 51, 52, 53, 54, 55, 80, 81, 82, 83, 84, 85, 86, 87],
                //[24, 25, 26, 27, 28, 29, 30, 31, 56, 57, 58, 59, 60, 61, 62, 63, 88, 89, 90, 91, 92, 93, 94, 95]])
                // first dimension element 0 and 1 are ADC, 1 and 2 are DAC
                int iPos = 0;
                for (int iBatch = 0; iBatch < batchesPerFrame; iBatch++)//176 2 byte intergers per frame per array -> with batch size 8 thats 22 batches per frame per array
                {
                    for (int iFloat = 0; iFloat < h.batchSize; iFloat++)
                    {
                        iPos = iBatch * h.batchSize * 2 * 4 + iFloat * 2 + header.size;
                        memory.ADCEnqueueAt(UnitConvert.ADCMuToV(BitConverter.ToInt16(rawData, iPos)), h.sequenceNumber, iBatch, iFloat);
                    }
                    for (int iFloat = 0; iFloat < h.batchSize; iFloat++)
                    {
                        iPos = iBatch * h.batchSize * 2 * 4 + iFloat * 2 + header.size + 2 * h.batchSize * 2;
                        memory.DACEnqueueAt(UnitConvert.DACMUToV(BitConverter.ToInt16(rawData, iPos)), h.sequenceNumber, iBatch, iFloat);
                    }
                }
            }
        }
    }
}
