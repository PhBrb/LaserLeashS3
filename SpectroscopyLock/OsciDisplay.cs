using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChartTest2
{
    internal class OsciDisplay
    {
        Memory memory;
        bool useAllData = true;
        int oldestSampleToDisplay = 40000;

        int pointsOnDisplay = 400;
        double[] adcData = new double[400];
        double[] dacData = new double[400];
        double[] adcDataSorted = new double[400];
        double[] dacDataSorted = new double[400];

        public OsciDisplay(Memory memory)
        {
            this.memory = memory;
        }

        public (double[], double[]) GetTimeSeries()
        {
            int iSampleDistance = oldestSampleToDisplay / pointsOnDisplay;
            int iAvg = useAllData ? oldestSampleToDisplay / pointsOnDisplay : 1;
            for (int i = 0; i < pointsOnDisplay; i++)
            {
                adcData[pointsOnDisplay - i - 1] = memory.GetADCSum(-i * iSampleDistance - iAvg, iAvg) / iAvg; // get sample corresponding to point i, shift it to make sure there is enough data to average over and save newest data i=0 into last adcData index
            }
            for (int i = 0; i < pointsOnDisplay; i++)
            {
                dacData[pointsOnDisplay - i - 1] = memory.GetDACSum(-i * iSampleDistance - iAvg, iAvg) / iAvg; // get sample corresponding to point i, shift it to make sure there is enough data to average over and save newest data i=0 into last adcData index
            }

            return (adcData, dacData);
        }


        public (double[], double[]) GetXY()
        {
            Array.Copy(adcData, adcDataSorted, pointsOnDisplay);
            Array.Copy(dacData, dacDataSorted, pointsOnDisplay);

            Array.Sort(dacDataSorted, adcDataSorted);
            return (adcDataSorted, dacDataSorted);
        }
    }
}
