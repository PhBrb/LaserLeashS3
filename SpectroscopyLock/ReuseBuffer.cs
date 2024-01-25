using System;
using System.Threading;

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

        /// <summary>
        /// Data storage
        /// </summary>
        Frame[] frames;

        /// <summary>
        ///  Index pointer to the buffer element that is being written to
        /// </summary>
        int writingTo = 1;

        /// <summary>
        ///  Index pointer to the buffer element that is being read
        /// </summary>
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
            SpinWait spin = new SpinWait();
            int next = (writingTo + 1) % size;
            while (next == readingFrom)
            {
                spin.SpinOnce();
                if (SpectroscopyControlForm.stopped == true)
                    return null;
            }
            writingTo = next;
            return frames[next];
        }

        /// <summary>
        /// Returns the next received data. Blocks until data is available.
        /// </summary>
        /// <returns></returns>
        public Frame getNextFrame()
        {
            SpinWait spin = new SpinWait();
            int next = (readingFrom + 1) % size;
            while (next == writingTo)
            {
                spin.SpinOnce();
                if (SpectroscopyControlForm.stopped == true)
                    return null;
            }
            readingFrom = next;
            Frame ret = frames[next];
            return ret;
        }
    }
}
