using System;
using System.Windows.Forms;

namespace LaserLeash
{
    public class Memory
    {
        private CyclicArray adc;
        private CyclicArray dac;

        public int consecutiveTimeJumpsBack;
        
        public bool freeze = false;
        public object locker = new object();

        public Memory(int size)
        {
            if (size < 1)
            {
                throw new ArgumentException("too few datapoints");
            }

            adc = new CyclicArray(UnitConvert.TimeToSample(Properties.Settings.Default.MemorySize));
            dac = new CyclicArray(UnitConvert.TimeToSample(Properties.Settings.Default.MemorySize));
        }

        public void setSize(int size)
        {
            lock (locker)
            {
                adc = new CyclicArray(size);
                dac = new CyclicArray(size);
            }
        }

        public void clear()
        {
            lock (locker)
            {
                adc = new CyclicArray(adc.getSize());
                dac = new CyclicArray(dac.getSize());
            }
        }

        public void ADCEnqueueAt(double value, uint sequence, int batch, int sample)
        {
            if (freeze)
                return;
            adc.writeAt(value, sequence, batch, sample);
        }

        public void DACEnqueueAt(double value, uint sequence, int batch, int sample)
        {
            if (freeze)
                return;
            dac.writeAt(value, sequence, batch, sample);
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

        public double[] getDACArray(int start, int stop)
        {
            return dac.getArray(-start, -stop);
        }

        public double[] getADCArray(int start, int stop)
        {
            return adc.getArray(-start, -stop);
        }
    }
}
