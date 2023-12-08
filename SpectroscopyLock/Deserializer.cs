using System;
using System.Reflection;
using System.Windows.Forms;

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


        public static void Deserialize(byte[] rawData, Memory memory)
        {
            header h;
            h.magic = BitConverter.ToUInt16(rawData, 0);
            h.formatId = rawData[2];
            h.batchSize = rawData[3];
            h.sequenceNumber = BitConverter.ToUInt32(rawData, 4);

            if (h.sequenceNumber == memory.lastSequenceNumber) //cancel if already processed previously
                return;

            //handle not monotonically increasing sequenceNumbers
            if (h.sequenceNumber < memory.lastSequenceNumber)
            {
                memory.consecutiveTimeJumpsBack += 1;
                SpectroscopyControlForm.WriteLine("Received data out of order");

                //if consistent time jump backwards happened eg on sequence number overflow or stabilizer reset
                if (memory.consecutiveTimeJumpsBack > 10)
                {
                    SpectroscopyControlForm.WriteLine("Time jump detected, reseting memory.");
                    memory.clear();
                    memory.lastSequenceNumber = 0;
                }
                else
                {
                    try
                    {
                        //if time jumped back more than the memory size
                        if (checked((memory.lastSequenceNumber - h.sequenceNumber + 22) * 8) > memory.getSize())
                        {
                            SpectroscopyControlForm.WriteLine($"Rejecting data as it is older than the memory size.");
                            return;
                        }
                        else //if jump was small enough so that the data can be inserted
                        {
                            uint sampleToJumpTo = (memory.lastSequenceNumber - h.sequenceNumber + 22) * 8;

                            //TODO this requires a bigger rewrite, as jumping back in time was not planned for when the memory data structure was designed
                            //make the memory not a queue, but just put modulus on the sequence number afterwards add the batch+sample number
                            //still keep track of the newest sample that was added, as this is needed for the readout of the memory and overwriting old data


                        }
                    }
                    catch (OverflowException)
                    {
                        SpectroscopyControlForm.WriteLine($"Rejecting data as it is older than the memory size.");
                        return;
                    }
                }
            } else {
                memory.consecutiveTimeJumpsBack = 0;

                //check if sequenceNumber jumped forwards
                int skip = (int)(h.sequenceNumber - memory.lastSequenceNumber) / 22 - 1;
                if (skip > 0)
                {
                    double samplesSkipped = h.sequenceNumber * 8.0 - memory.lastSequenceNumber * 8.0; //use double to avoid overflow
                    if (samplesSkipped > memory.getSize() / 2) //catch huge jumps to avoid lag
                    {
                        SpectroscopyControlForm.WriteLine($"A lot of data was not received. Resetting memory");
                        memory.clear();
                    }
                    else
                    {
                        lock (memory.locker) //prevent channels from getting a timeshift if memory gets resized at the same time
                        {
                            memory.ADCSkip(skip * 22 * 8);//22 batches per frame, 8 numbers per batch
                            memory.DACSkip(skip * 22 * 8);
                        }
                    }
                }
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
            for (int iBatch = 0; iBatch < 22; iBatch++)//176 2 byte intergers per frame -> with batch size 8 thats 22 batches per frame per channel
            {
                lock (memory.locker) //prevent channels from getting a timeshift if memory gets resized at the same time
                {
                    for (int iFloat = 0; iFloat < batchSize; iFloat++)
                    {
                        iPos = iBatch * batchSize * 2 * 4 + iFloat * 2 + headerSize;
                        memory.ADCEnqueue(UnitConvert.ADCMuToV(BitConverter.ToInt16(rawData, iPos)));
                    }
                    for (int iFloat = 0; iFloat < batchSize; iFloat++)
                    {
                        iPos = iBatch * batchSize * 2 * 4 + iFloat * 2 + headerSize + 2 * batchSize * 2;
                        memory.DACEnqueue(UnitConvert.DACMUToV(BitConverter.ToInt16(rawData, iPos)));
                    }
                }
            }
            if (BitConverter.ToUInt32(rawData, 4) != h.sequenceNumber)
                throw new Exception("converting data wasnt thread safe");
        }
    }
}
