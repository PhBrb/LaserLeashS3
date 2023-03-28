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
        public double[] adc0Rolling;
        public double[] adc1Rolling;
        public double[] dac0Rolling;
        public double[] dac1Rolling;

        public Dictionary<double, double> xyData = new Dictionary<double, double>();
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


            adc0Rolling = new double[size];
            adc1Rolling = new double[size];
            dac0Rolling = new double[size];
            dac1Rolling = new double[size];
        }

        public void resetXY()
        {
            xyData = new Dictionary<double, double>();
        }

        public void setDatapoint(double x, double y)
        {
            lock (xyData)
            {
                xyData[x] = y;
            }
        }
    }
}
