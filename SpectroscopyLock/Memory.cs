﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
                adc.Enqueue(adc.GetLast());
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
                dac.Enqueue(dac.GetLast());
            };
        }

        public void DACEnqueue(double value)
        {
            if (freeze)
                return;

            dac.Enqueue(value);
        }

        public double GetADCSum(int previous, int size)
        {
            return adc.GetADCSum(previous, size);
        }

        public double GetDACSum(int previous, int size)
        {
            return dac.GetADCSum(previous, size);
        }
    }
}
