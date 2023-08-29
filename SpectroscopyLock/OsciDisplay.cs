using System;
using System.Data;
using System.Linq;

namespace ChartTest2
{

    /// <summary>
    /// Prepares date for displaying
    /// </summary>
    public class OsciDisplay
    {
        Memory memory;
        int averages = Properties.Settings.Default.Averages;
        double XYSmoothing = Properties.Settings.Default.XYSmoothing;

        int pointsOnDisplay = Properties.Settings.Default.DisplayResolution;
        double[] adcData = new double[Properties.Settings.Default.DisplayResolution];
        double[] dacData = new double[Properties.Settings.Default.DisplayResolution];
        double[] adcDataSorted = new double[Properties.Settings.Default.DisplayResolution];
        double[] dacDataSorted = new double[Properties.Settings.Default.DisplayResolution];
        public double[] timeData = new double[Properties.Settings.Default.DisplayResolution];

        int newestSampleToDisplay = 0;
        public int oldestSampleToDisplay = UnitConvert.TimeToSample(Properties.Settings.Default.MemorySize);

        public OsciDisplay(Memory memory)
        {
            this.memory = memory;
            UpdateTimeData();
        }

        private void UpdateTimeData()
        {
            timeData = new double[pointsOnDisplay];
            for (int i = 0; i < pointsOnDisplay; i++)
            {
                ///map a range of 0 - <see cref="pointsOnDisplay"/> to <see cref="newestSampleToDisplay"/> - <see cref="oldestSampleToDisplay"/>
                timeData[i] = -UnitConvert.SampleToTime((pointsOnDisplay - i) * (oldestSampleToDisplay - newestSampleToDisplay) / pointsOnDisplay + newestSampleToDisplay);
            }
        }

        public (double[], double[]) GetTimeSeries()
        {
            int iSampleDistance = (oldestSampleToDisplay - newestSampleToDisplay) / pointsOnDisplay;
            int samplesToUse = averages + 1;
            int iOffset = newestSampleToDisplay;
            for (int i = 0; i < pointsOnDisplay; i++)
            {
                adcData[pointsOnDisplay - i - 1] = memory.GetADCSumFromPast(-i * iSampleDistance - samplesToUse - iOffset, samplesToUse) / samplesToUse; // get sample corresponding to point i, shift it to make sure there is enough data to average over and save newest data i=0 into last adcData index
            }
            for (int i = 0; i < pointsOnDisplay; i++)
            {
                dacData[pointsOnDisplay - i - 1] = memory.GetDACSumFromPast(-i * iSampleDistance - samplesToUse - iOffset, samplesToUse) / samplesToUse; // get sample corresponding to point i, shift it to make sure there is enough data to average over and save newest data i=0 into last adcData index
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
            UpdateTimeData();
        }

        public void ZoomOut() {
            int iCenter = (int)((newestSampleToDisplay + oldestSampleToDisplay) * 0.5);
            int iRange = (int)(1.3 * (oldestSampleToDisplay - newestSampleToDisplay) / 2);

            oldestSampleToDisplay = Math.Min(memory.getSize(),iCenter + iRange);
            newestSampleToDisplay = Math.Max(0, iCenter - iRange);
            UpdateTimeData();
        }

        public void setAverages(int count)
        {
            if (count < 0)
                throw new ArgumentException("must be positive");
            if (count > oldestSampleToDisplay / pointsOnDisplay) //TODO this does not take newestSampleToDisplay into account
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
            
            for(int i = pointsOnDisplay - 1; i >= 0 ; i--)
            {
                if (double.IsNaN(dacDataSorted[i]) || double.IsNaN(adcDataSorted[i]))
                {
                    adcList.RemoveAt(i);
                    dacList.RemoveAt(i);
                }
                else if(i < adcList.Count - 1)
                {
                    adcList[i] = (1-XYSmoothing)*adcList[i] + XYSmoothing* adcList[i + 1];
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

        public int getSize()
        {
            return pointsOnDisplay;
        }

        public void setSize(int size)
        {
            pointsOnDisplay = size;
            adcData = new double[size];
            dacData = new double[size];
            adcDataSorted = new double[size];
            dacDataSorted = new double[size];
            UpdateTimeData();
        }

        public void setXYSmoothing(double value)
        {
            XYSmoothing= value;
        }

        public int getAverages()
        {
            return averages;
        }
    }
}
