using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ChartTest2
{
    public class OsciData
    {
        public double[] adc0;
        public double[] adc1;
        public double[] dac0;
        public double[] dac1;

        private double[] xData;
        private double[] yData;
        private double[] range = new double[] { 0, 10 };
        private double resolutionLimit = 0.001;
        private int size;


        public OsciData(int size)
        {
            if(size < 1)
            {
                throw new ArgumentException("too few datapoints");
            }
            this.size = size;

            xData = new double[size];
            yData = new double[size];

            setRange(0, 10);

            adc0 = new double[size];
            adc1 = new double[size];
            dac0 = new double[size];
            dac1 = new double[size];
        }

        public bool setRange(double lower, double upper)
        {
            double rangeLen = range[1] - range[0];
            if(rangeLen/size < resolutionLimit)
            {
                return false;
            }

            range[0] = lower;
            range[1] = upper;
            for (int i = 0; i < size; i++)
            {
                xData[i] = range[0] + (rangeLen * i) / size;
                yData[i] = 0;
            }
            return true;
        }

        public void setDatapoint(double x, double y)
        {
            int index = (int)(size*(x - range[0]) / (range[1] - range[0]));
            yData[index] = y;
        }
    }
}
