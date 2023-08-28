using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartTest2
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

        public Frame getBuffer()
        {
            int next = (writingTo + 1) % size;
            while (next == readingFrom)
            {//TODO this is just burning cpu resources on the check
            }
            writingTo = next;
            return frames[next];
        }

        public Frame getFrame()
        {
            int next = (readingFrom + 1) % size;
            while (next == writingTo)
            {
            }
            readingFrom = next;
            Frame ret = frames[next];
            return ret;
        }
    }
}
