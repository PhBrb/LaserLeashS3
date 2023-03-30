using System;
using System.Collections.Generic;
using System.Drawing;
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
        public Queue<double> adcQueue = new Queue<double>();
        public Queue<double> dacQueue = new Queue<double>();

        public SortedDictionary<double, double[]> xyData = new SortedDictionary<double, double[]>();
        public double resolution = 0.001;

        private int avgSize = 50;
        public int AvgSize { get { return avgSize; } set
            {
                lock (xyData)
                {
                    avgSize = value;
                    xyData.Clear();
                }
            } }


        public OsciData(int size)
        {
            if(size < 1)
            {
                throw new ArgumentException("too few datapoints");
            }

            adc0Rolling = new double[size];
            adc1Rolling = new double[size];
            dac0Rolling = new double[size];
            dac1Rolling = new double[size];
        }

        public void resetXY()
        {
            xyData = new SortedDictionary<double, double[]>();
        }

        public void setDatapoint(double x, double y)
        {
            lock (xyData)
            {

                double xRounded = ((int)(x / resolution)) * resolution;
                double[] ys = new double[avgSize];
                if(!xyData.TryGetValue(xRounded, out ys)){
                    ys = new double[avgSize];
                }
                for(int i = 0; i < avgSize - 1; i++)
                {
                    ys[i] = ys[i + 1];
                }
                ys[avgSize-1] = y;
                xyData[xRounded] = ys;
            }
        }
    }
}
