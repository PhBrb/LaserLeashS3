using System;

namespace ChartTest2
{
    public class ArrayQueue
    {
        private readonly double[] array;

        private int lastWrittenPosition = 0;
        private readonly int size;

        public ArrayQueue(int size)
        {
            array = new double[size];
            this.size = size;
        }

        /// <summary>
        /// Makes % of negative numbers return positive numbers, matching python style array indexing (array[mod(-2,size)] -> second last element)
        /// Exanple: mod(-1,3) = 2
        /// </summary>
        private static int mod(int x, int m)
        {
            return (x % m + m) % m;
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

        /// <summary>
        /// Returns an array with the first value being <see cref="startOffset"/> indexes old.
        /// </summary>
        /// <param name="start">Offset from newest datapoint, must be negative. Fisrt value in the returned array.</param>
        /// <param name="stop">Offset from newest datapoint, must be negative. Last value in the returned array.</param>
        /// <returns></returns>
        public double[] getArray(int startOffset, int stopOffset)
        {
            double[] cutArray = new double[-stopOffset + startOffset];
            for(int i = 0; i < cutArray.Length; i++) 
            {
                cutArray[i] = get(lastWrittenPosition + startOffset - i);
            }
            return cutArray;
        }
    }
}
