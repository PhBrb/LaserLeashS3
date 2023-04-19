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
        int averages = 50;
        public int oldestSampleToDisplay = 1000000;

        int pointsOnDisplay = 400;
        double[] adcData = new double[400];
        double[] dacData = new double[400];
        double[] adcDataSorted = new double[400];
        double[] dacDataSorted = new double[400];

        int newestSampleToDisplay = 0;

        public OsciDisplay(Memory memory)
        {
            this.memory = memory;
        }

        public (double[], double[]) GetTimeSeries()
        {
            int iSampleDistance = (oldestSampleToDisplay - newestSampleToDisplay) / pointsOnDisplay;
            int samplesToUse = averages + 1;
            int iOffset = newestSampleToDisplay;
            for (int i = 0; i < pointsOnDisplay; i++)
            {
                adcData[pointsOnDisplay - i - 1] = memory.GetADCSum(-i * iSampleDistance - samplesToUse - iOffset, samplesToUse) / samplesToUse; // get sample corresponding to point i, shift it to make sure there is enough data to average over and save newest data i=0 into last adcData index
            }
            for (int i = 0; i < pointsOnDisplay; i++)
            {
                dacData[pointsOnDisplay - i - 1] = memory.GetDACSum(-i * iSampleDistance - samplesToUse - iOffset, samplesToUse) / samplesToUse; // get sample corresponding to point i, shift it to make sure there is enough data to average over and save newest data i=0 into last adcData index
            }

            return (adcData, dacData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position">Position from 0 to 1, relative to current viewing range</param>
        public void ZoomIn(double position)
        {
            int iCenter = (newestSampleToDisplay + oldestSampleToDisplay)/2;
            int iRange = (int)(0.7*(oldestSampleToDisplay - newestSampleToDisplay) / 2);
            int iNewCenter = iCenter - iRange + (int)(2*iRange*position);

            oldestSampleToDisplay = Math.Min(oldestSampleToDisplay, iNewCenter + iRange);
            newestSampleToDisplay = Math.Max(newestSampleToDisplay, iNewCenter - iRange);

            Console.WriteLine($"sampe old {oldestSampleToDisplay}");
            Console.WriteLine($"sample new {newestSampleToDisplay}");
        }

        public void ZoomOut() {
            int iCenter = (int)((newestSampleToDisplay + oldestSampleToDisplay) * 0.5);
            int iRange = (int)(1.3 * (oldestSampleToDisplay - newestSampleToDisplay) / 2);

            oldestSampleToDisplay = Math.Min(memory.getSize(),iCenter + iRange);
            newestSampleToDisplay = Math.Max(0, iCenter - iRange);
        }

        public void setAverages(int count)
        {
            if (count < 0)
                throw new ArgumentException("must be positive");
            if (count > oldestSampleToDisplay / pointsOnDisplay)
                throw new ArgumentException("not enough data for averages");
            averages = count;
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
            var dacList = dacDataSorted.ToList();
            var adcList = adcDataSorted.ToList();
            //TODO optimize memory usage
            for(int i = pointsOnDisplay - 1; i >=0 ; i--)
            {
                if (double.IsNaN(dacDataSorted[i]) || double.IsNaN(adcDataSorted[i]))
                {
                    adcList.RemoveAt(i);
                    dacList.RemoveAt(i);
                }
            }
            return (dacList.ToArray(), adcList.ToArray());
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
