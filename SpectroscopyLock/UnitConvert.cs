using System;

namespace ChartTest2
{
    public static class UnitConvert
    {
        const double DACLSBPerVolt = (1 << 16) / (4.096 * 5);
        const double SamplePeriod = 10e-9 * 128;
        public static int YVToMU(double voltage)
        {
            int code = (int)Math.Round(voltage * DACLSBPerVolt, MidpointRounding.AwayFromZero);
            if (Math.Abs(code) > 0x7FFF)
                throw new ArgumentException("Out of range");
            return code;
        }

        public static double DACMUToV(int mu)
        {
            return (mu^unchecked((short)0x8000))/ DACLSBPerVolt;
        }

        public static double ADCMuToV(int mu)
        {
            return   mu / DACLSBPerVolt; //TODO is this correct? in stream.py the same value is used for both DAC and ADC, ADCVoltsPerLSB is defined but not used
        }

        public static double SampleToTime(int sampleIndex)
        {
            return sampleIndex * SamplePeriod;
        }
    }
}
