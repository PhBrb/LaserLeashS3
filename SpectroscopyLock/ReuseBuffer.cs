using System;

namespace LaserLeash
{
    internal class ReuseBuffer
    {
        public class Frame
        {
            public byte[] data = new byte[1416];
            public uint sequenceNumber = 0;

            public void calcMetadata()
            {
                sequenceNumber = BitConverter.ToUInt32(data, 4);
            }
        }

        Frame[] frames;
        int writingTo = 1;
        int readingFrom = 0;
        int size;

        public ReuseBuffer(int size) 
        {
            frames = new Frame[size];
            for(int i = 0; i < size; i++)
            {
                frames[i] = new Frame();
            }
            this.size = size;
        }

        /// <summary>
        /// Returns the buffer that should be written to next. Blocks until buffer is available.
        /// </summary>
        /// <returns></returns>
        public Frame getNextBuffer()
        {
            int next = (writingTo + 1) % size;
            while (next == readingFrom)
            {//TODO is SpinWait appropriate here?
            }
            writingTo = next;
            return frames[next];
        }

        /// <summary>
        /// Returns the next oldest received data. Blocks until data is available.
        /// </summary>
        /// <returns></returns>
        public Frame getNextFrame()
        {
            int next = (readingFrom + 1) % size;
            while (next == writingTo)
            {//TODO is SpinWait appropriate here?
            }
            readingFrom = next;
            Frame ret = frames[next];
            return ret;
        }
    }
}
