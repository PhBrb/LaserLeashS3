using System;

namespace LaserLeash
{
    public class CyclicArray
    {
        private readonly double[] array;

        public long newestDataPosition = 0;
        private readonly int size;
        private int batchSize = 8;

        private bool fresh = true;

        public CyclicArray(int size)
        {
            array = new double[size];
            for (int i = 0; i < size; i++)
            {
                array[i] = double.NaN;
            }
            this.size = size;
        }

        /// <summary>
        /// Makes modulo of negative numbers return positive numbers, matching python style array indexing (array[mod(-2,size)] -> second last element)
        /// Exanple: mod(-1,3) = 2
        /// </summary>
        private static int mod(int x, int m)
        {
            return (x % m + m) % m;
        }
        private static int mod(long x, int m)
        {
            return ((int)(x % m) + m) % m;
        }

        public void setPosition(long pos)
        {
            newestDataPosition = pos;
        }

        public void writeAt(double value, uint sequence, int batch, int sample)
        {
            long positionToWriteTo = (sequence + batch) * batchSize + sample;

            if (fresh) //make using a new array a bit faster
                newestDataPosition = positionToWriteTo - 1;

            //if positions were skipped
            long skip = Math.Min(positionToWriteTo - newestDataPosition - 1, size);

            for (int i = 0; i < skip; i++)
            {
                array[(newestDataPosition + i) % size] = double.NaN;
            }

            newestDataPosition = Math.Max(positionToWriteTo, newestDataPosition);

            array[positionToWriteTo%size] = value;

            fresh = false;
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
            int start = mod(newestDataPosition + offset, size);
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
        public double get(long offset)
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
                cutArray[i] = get(newestDataPosition + startOffset - i);
            }
            return cutArray;
        }
    }
}
