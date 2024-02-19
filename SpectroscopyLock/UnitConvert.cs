using System;
using System.Collections.Generic;
using System.Linq;

namespace LaserLeash
{
    public static class UnitConvert
    {
        const double DACLSBPerVolt = (1 << 16) / (4.096 * 5);
        public const double SamplePeriod = 10e-9 * 128;

        /// <summary>
        /// Converts voltage to IIR y limit machine units
        /// </summary>
        /// <param name="voltage"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static int YVToMU(double voltage)
        {
            int code = (int)Math.Round(voltage * DACLSBPerVolt, MidpointRounding.AwayFromZero);
            if (Math.Abs(code) > 0x7FFF)
                throw new ArgumentException("Out of range");
            return code;
        }

        /// <summary>
        /// Converts DAC machine units to volt
        /// </summary>
        /// <param name="mu"></param>
        /// <returns></returns>
        public static double DACMUToV(int mu)
        {
            return (mu^unchecked((short)0x8000))/ DACLSBPerVolt;
        }

        /// <summary>
        /// Converts ADC machine units to volt
        /// </summary>
        /// <param name="mu"></param>
        /// <returns></returns>
        public static double ADCMuToV(int mu)
        {
            return   mu / DACLSBPerVolt;
        }

        /// <summary>
        /// Converts an number of samples to the sampling period in seconds
        /// </summary>
        /// <param name="sampleIndex"></param>
        /// <returns></returns>
        public static double SampleToTime(int sampleIndex)
        {
            return sampleIndex * SamplePeriod;
        }

        /// <summary>
        /// Converts a sampling duration in seconds to a number of samples with integer rounding
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static int TimeToSample(double time)
        {
            return (int)(time/SamplePeriod);
        }

        /// <summary>
        /// Converts PID parameters to IIR coefficients
        /// </summary>
        /// <param name="Kp"></param>
        /// <param name="Ki"></param>
        /// <param name="Kd"></param>
        /// <param name="samplePeriod"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static List<double> CalculateIIR(double Kp, double Ki, double Kd, double samplePeriod)
        {
            // code based on https://github.com/quartiq/stabilizer/blob/831a9ba747b51241936b13a2df23753c03cc9dd5/py/stabilizer/iir_coefficients.py

            double[,] kernels = new double[,] { { 1, 0, 0 }, { 1, -1, 0 }, { 1, -2, 1 } };

            double[] gains = new double[] { 0, Ki, Kp, Kd, 0 };
            double[] limits = new double[] { 0, 0, 1, 0, 0 };

            int order = Ki != 0 ? 1 : 0;
            double w = 2 * Math.PI * samplePeriod;

            double[] b = new double[] { 0, 0, 0 };
            double[] a = new double[] { 0, 0, 0 };

            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 3; i++)
                {
                    b[j] += gains[2 - order + i] * Math.Pow(w, order - i) * kernels[i, j];
                }
            }

            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 3; i++)
                {
                    a[j] += limits[2 - order + i] * Math.Pow(w, order - i) * kernels[i, j];
                }
            }
            for (int i = 0; i < 3; i++)
            {
                b[i] = b[i] / a[0];
                a[i] = a[i] / a[0];
            }
            if (a[0] != 1)
                throw new Exception();

            for (int i = 0; i < 3; i++)
            {
                a[i] *= -1;
            }
            List<double> result = b.ToList();
            result.AddRange(a.Skip(1));

            return result;
        }

    }
}
