using System;

namespace LaserLeash
{
    public class Memory
    {
        private CyclicArray adc;
        private CyclicArray dac;

        /// <summary>
        /// If true, no new data will be written to the memory.
        /// </summary>
        public bool freeze = false;

        /// <summary>
        /// Object for multithreading synchronization, lock this if you operate on the memory data
        /// </summary>
        public object locker = new object();

        /// <summary>
        /// Holds datastructures, for streamdata of ADC and DAC channels
        /// </summary>
        /// <param name="size"></param>
        /// <exception cref="ArgumentException"></exception>
        public Memory(int size)
        {
            if (size < 1)
            {
                throw new ArgumentException("too few datapoints");
            }

            adc = new CyclicArray(UnitConvert.TimeToSample(Properties.Settings.Default.MemorySize));
            dac = new CyclicArray(UnitConvert.TimeToSample(Properties.Settings.Default.MemorySize));
        }

        /// <summary>
        /// Changes the size of the memory. Clears memory.
        /// </summary>
        /// <param name="size"></param>
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

        public void ADCEnqueue(double value, uint sequence, int batch, int sample)
        {
            if (freeze)
                return;
            adc.writeAt(value, sequence, batch, sample);
        }

        public void DACEnqueue(double value, uint sequence, int batch, int sample)
        {
            if (freeze)
                return;
            dac.writeAt(value, sequence, batch, sample);
        }

        /// <summary>
        /// Returns the sum of multiple samples, which are offset from the newest data 
        /// </summary>
        /// <param name="offset">Offset from newest datapoint, must be negative</param>
        /// <param name="size">Number of samples</param>
        /// <returns></returns>
        public double GetADCAverageFromPast(int offset, int size)
        {
            return adc.GetAverageFromPast(offset, size);
        }

        /// <summary>
        /// Returns the sum of multiple samples, which are offset from the newest data 
        /// </summary>
        /// <param name="offset">Offset from newest datapoint, must be negative</param>
        /// <param name="size">Number of samples</param>
        /// <returns></returns>
        public double GetDACAverageFromPast(int offset, int size)
        {
            return dac.GetAverageFromPast(offset, size);
        }

        internal int getSize()
        {
            return adc.getSize();
        }

        /// <summary>
        /// Returns an array of samples that are <paramref name="start"/> to <paramref name="stop"/> samples old.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        public double[] getDACArray(int start, int stop)
        {
            return dac.getArray(-start, -stop);
        }

        /// <summary>
        /// Returns an array of samples that are <paramref name="start"/> to <paramref name="stop"/> samples old.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        public double[] getADCArray(int start, int stop)
        {
            return adc.getArray(-start, -stop);
        }
    }
}
