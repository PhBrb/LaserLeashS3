using System;

namespace ChartTest2
{
    public class ArrayQueue
    {
        private double[] array;

        private int lastWrittenPosition = 0;
        private int size = 0;

        public ArrayQueue(int size)
        {
            array = new double[size];
            this.size = size;
        }

        /// <summary>
        /// mod(-4,3) = 2
        /// </summary>
        private static int mod(int x, int m)
        {
            return (x % m + m) % m;
        }

        public void Clear()
        {
            array = new double[size];
        }

        public void Enqueue(double value)
        {
            lastWrittenPosition = (lastWrittenPosition+1) % size;
            array[lastWrittenPosition] = value;
        }

        /// <summary>
        /// Returns the sum of a chunk, which is offset before the newest data 
        /// </summary>
        /// <param name="offset">Offset from newest datapoint, must be negative</param>
        /// <param name="sumSize">Number of samples</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public double GetADCSumFromPast(int offset, int sumSize)
        {
            if (offset >= 0)
                throw new ArgumentException("previous must be negative");
            if (sumSize <= 0)
                throw new ArgumentException("sum must be positive");
            if (offset + sumSize > 0)
                throw new ArgumentException("previous + outArray size must be <= 0");
            if (offset >= size - 1)
                throw new ArgumentException("requested too old data");
            int start = mod(lastWrittenPosition + offset, size);
            double sum = 0;
            for (int i = 0; i < sumSize; i++)
            {
                sum += get(start + i);
            }
            return sum;
        }

        public double get(int offset)
        {
            return array[mod(offset, size)];
        }

        internal int getSize()
        {
            return size;
        }

        public double[] getArray(int start, int stop)
        {
            double[] cutArray = new double[stop - start];
            for(int i = start; i < cutArray.Length; i++) 
            {
                cutArray[i] = get(start + i);//TODO thread safety
            }
            return cutArray;
        }
    }
}
