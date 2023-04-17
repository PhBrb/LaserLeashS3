using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public double GetLast()
        {
            return array[lastWrittenPosition];
        }

        public double GetADCSum(int previous, int sumSize)
        {
            if (previous >= 0)
                throw new ArgumentException("previous must be negative");
            if (previous + sumSize > 0)
                throw new ArgumentException("previous + outArray size must be <= 0"); //TODO this only catches if you request too much new data, it needs also to be caught if you request too old data
            int start = mod(lastWrittenPosition + previous, size);
            double sum = 0;
            for (int i = 0; i < sumSize; i++)
            {
                sum += array[mod(start + i, size)];
            }
            return sum;
        }

        internal int getSize()
        {
            return size;
        }
    }
}
