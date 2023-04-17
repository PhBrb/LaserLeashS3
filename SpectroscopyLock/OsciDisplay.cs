using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChartTest2
{

    /// <summary>
    /// Prepares date for displaying
    /// </summary>
    public class OsciDisplay
    {
        Memory memory;
        bool useAllData = false;
        int oldestSampleToDisplay = 4500000;

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


        /// <summary>
        /// Transfers the data from TimeSeries storage in (sorted) XY format. <see cref="GetTimeSeries"/> has to be called first to update the internal data storage.
        /// </summary>
        /// <returns></returns>
        public (double[], double[]) GetXYNoUpdate()
        {
            Array.Copy(adcData, adcDataSorted, pointsOnDisplay);
            Array.Copy(dacData, dacDataSorted, pointsOnDisplay);

            Array.Sort(dacDataSorted, adcDataSorted);
            return (dacDataSorted, adcDataSorted);
        }

        public double GetADCMinNoUpdate()
        {
            return adcDataSorted.Min();
        }
        public double GetADCMaxNoUpdate()
        {
            return adcDataSorted.Max();
        }
        public double GetDACMinNoUpdate()
        {
            return dacDataSorted[0];
        }
        public double GetDACMaxNoUpdate()
        {
            return dacDataSorted[adcDataSorted.Length - 1];
        }
    }
}
