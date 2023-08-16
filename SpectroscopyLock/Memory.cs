using System;

namespace ChartTest2
{
    public class Memory
    {
        private ArrayQueue adc;
        private ArrayQueue dac;
        public uint lastSequenceNumber;
        public bool freeze = false;

        public Memory(int size)
        {
            if (size < 1)
            {
                throw new ArgumentException("too few datapoints");
            }

            adc = new ArrayQueue(size);
            dac = new ArrayQueue(size);
        }

        public void setSize(int size)
        {
            adc = new ArrayQueue(size);
            dac = new ArrayQueue(size);
        }

        public void Clear()
        {
            adc.Clear();
            dac.Clear();
        }

        public void ADCSkip(int skip)
        {
            if (freeze)
                return;

            for (int i = 0; i < skip; i++)
            {
                adc.Enqueue(double.NaN);
            }
        }

        public void ADCEnqueue(double value)
        {
            if (freeze)
                return;

            adc.Enqueue(value);
        }

        public void DACSkip(int skip)
        {
            if (freeze)
                return;

            for (int i = 0; i < skip; i++)
            {
                dac.Enqueue(double.NaN);
            };
        }

        public void DACEnqueue(double value)
        {
            if (freeze)
                return;

            dac.Enqueue(value);
        }

        public double GetADCSumFromPast(int offset, int size)
        {
            return adc.GetADCSumFromPast(offset, size);
        }

        public double GetDACSumFromPast(int offset, int size)
        {
            return dac.GetADCSumFromPast(offset, size);
        }

        internal int getSize()
        {
            return adc.getSize();
        }
    }
}
