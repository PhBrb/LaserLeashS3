using System;

namespace LaserLeash
{
    public class CyclicArray
    {
        /// <summary>
        /// Holds the data of the object
        /// </summary>
        private readonly double[] array;

        /// <summary>
        /// Size of the data storage
        /// </summary>
        private readonly int size;

        /// <summary>
        /// Index of the highest used index
        /// </summary>
        public long newestDataPosition = 0;

        /// <summary>
        /// Size of a batch in a stabilizer frame
        /// </summary>
        private int batchSize = 8;

        /// <summary>
        /// Indicates if the object was not used before
        /// </summary>
        private bool fresh = true;

        /// <summary>
        /// An array for chronological data. The index wraps over to the beginning if the maximum size is reached, overwriting old data
        /// </summary>
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
        /// <summary>
        /// Makes modulo of negative numbers return positive numbers, matching python style array indexing (array[mod(-2,size)] -> second last element)
        /// Exanple: mod(-1,3) = 2
        /// </summary>
        private static int mod(long x, int m)
        {
            return ((int)(x % m) + m) % m;
        }

        public void setNewestDataPosition(long pos)
        {
            newestDataPosition = pos;
        }

        public void writeAt(double value, uint sequence, int batch, int sample)
        {
            long positionToWriteTo = (sequence + batch) * batchSize + sample;

            if (fresh) //make using a new array a bit faster, avoiding having to overwrite skipped values
                newestDataPosition = positionToWriteTo - 1;

            //set skipped entries to NaN
            long skip = Math.Min(positionToWriteTo - newestDataPosition - 1, size);
            for (int i = 0; i < skip; i++)
            {
                array[(newestDataPosition + i) % size] = double.NaN;
            }

            //advance newest data index if necessary
            newestDataPosition = Math.Max(positionToWriteTo, newestDataPosition);

            array[positionToWriteTo%size] = value;

            fresh = false;
        }

        /// <summary>
        /// Returns the sum of a chunk, which is offset from the newest data 
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
            for (int i = 0; i < sumSize; i++)//if necessary for perfrmance, this could be replaced by 2*memcopy
            {
                sum += get(start + i); 
            }
            return sum;
        }

        /// <summary>
        /// REturns element at index. Wraps over if index exceeds array size. Uses python indexing for negative numbers.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public double get(int index)
        {
            return array[mod(index, size)];
        }

        /// <summary>
        /// Returns element at index. Wraps over if index exceeds array size. Uses python indexing for negative numbers.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public double get(long index)
        {
            return array[mod(index, size)];
        }

        internal int getSize()
        {
            return size;
        }

        /// <summary>
        /// Returns an array of values with the first value being <see cref="startOffset"/> indexes old.
        /// </summary>
        /// <param name="start">Offset from newest datapoint, must be negative. First value in the returned array.</param>
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
