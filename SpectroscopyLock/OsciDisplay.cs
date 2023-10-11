using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

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

        /// <summary>
        /// Positive number, the higher the number, the older the sample
        /// </summary>
        public int newestSampleToDisplay { get; private set; }
        /// <summary>
        /// Positive number, the higher the number, the older the sample
        /// </summary>
        public int oldestSampleToDisplay { get; private set; }

        double[] adcData = new double[Properties.Settings.Default.DisplayResolution];
        double[] dacData = new double[Properties.Settings.Default.DisplayResolution];
        double[] adcDataSorted = new double[Properties.Settings.Default.DisplayResolution];
        double[] dacDataSorted = new double[Properties.Settings.Default.DisplayResolution];
        public double[] timeData = new double[Properties.Settings.Default.DisplayResolution];


        /// <summary>
        /// Pools jobs that change parameters in the class, to stay thread safe.
        /// </summary>
        private List<Action> editJobs = new List<Action>();

        public OsciDisplay(Memory memory)
        {
            this.memory = memory;

            //default to maximum range
            newestSampleToDisplay = 0;
            oldestSampleToDisplay = UnitConvert.TimeToSample(Properties.Settings.Default.MemorySize);

            _UpdateTimeData();
        }

        public void UpdateParameters()
        {
            lock (editJobs)
            {
                foreach (Action a in editJobs)
                {
                    a();
                }
                editJobs.Clear();
            }
        }

        public void UpdateTimeData()
        {
            lock (editJobs) 
                editJobs.Add(_UpdateTimeData);
        }
        private void _UpdateTimeData()
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
            lock (memory)
            {
                for (int i = 0; i < pointsOnDisplay; i++)
                {
                    adcData[pointsOnDisplay - i - 1] = memory.GetADCSumFromPast(-i * iSampleDistance - samplesToUse - iOffset, samplesToUse) / samplesToUse; // get sample corresponding to point i, shift it to make sure there is enough data to average over and save newest data i=0 into last adcData index
                }
                for (int i = 0; i < pointsOnDisplay; i++)
                {
                    dacData[pointsOnDisplay - i - 1] = memory.GetDACSumFromPast(-i * iSampleDistance - samplesToUse - iOffset, samplesToUse) / samplesToUse; // get sample corresponding to point i, shift it to make sure there is enough data to average over and save newest data i=0 into last adcData index
                }
            }

            return (adcData, dacData);
        }

        public void ZoomIn(double position)
        {
            lock (editJobs)
                editJobs.Add(() => _ZoomIn(position));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position">Position from 0 to 1, relative to current viewing range</param>
        private void _ZoomIn(double position)
        {
            int iCenter = (newestSampleToDisplay + oldestSampleToDisplay)/2;
            int iRange = (int)(0.7*(oldestSampleToDisplay - newestSampleToDisplay) / 2);
            int iNewCenter = iCenter - iRange + (int)(2*iRange*position);

            int tmpOldestSampleToDisplay = Math.Min(oldestSampleToDisplay, iNewCenter + iRange);
            int tmpNewestSampleToDisplay = Math.Max(newestSampleToDisplay, iNewCenter - iRange);
            if(enoughSamples(pointsOnDisplay, averages, tmpOldestSampleToDisplay, tmpNewestSampleToDisplay))
            {
                newestSampleToDisplay = tmpNewestSampleToDisplay;
                oldestSampleToDisplay = tmpOldestSampleToDisplay;
            }
            else
            {
                string msg = "Can't zoom in further. Too high display resolution or too much averaging. Available memory has to be >= display resolution * (averages +1)";
                SpectrscopyControlForm.WriteLine(msg);

                //zoom in as far as possible
                newestSampleToDisplay = Math.Max(0, iNewCenter - (pointsOnDisplay * (averages + 1)) / 2);
                oldestSampleToDisplay = newestSampleToDisplay + pointsOnDisplay * (averages + 1);
            }
            _UpdateTimeData();
        }

        public void ZoomOut()
        {
            lock (editJobs)
                editJobs.Add(_ZoomOut);
        }
        private void _ZoomOut() {
            int iCenter = (int)((newestSampleToDisplay + oldestSampleToDisplay) * 0.5);
            int iRange = (int)(1/0.7 * (oldestSampleToDisplay - newestSampleToDisplay) / 2);

            oldestSampleToDisplay = Math.Min(memory.getSize(),iCenter + iRange);
            newestSampleToDisplay = Math.Max(0, iCenter - iRange);
            _UpdateTimeData();
        }

        public void ZoomReset()
        {
            lock (editJobs)
                editJobs.Add(_ZoomReset);
        }
        private void _ZoomReset()
        {
            oldestSampleToDisplay = memory.getSize();
            newestSampleToDisplay = 0;
            _UpdateTimeData();
        }

        public void setAverages(int count)
        {
            lock (editJobs)
                editJobs.Add(() => _setAverages(count));
        }
        private void _setAverages(int count)
        {
            if (count < 0)
                throw new ArgumentException("must be positive");
            averages = count;
        }

        /// <summary>
        /// Returns stored data in (sorted) XY format. <see cref="GetTimeSeries"/> has to be called first to update the internal data storage.
        /// </summary>
        /// <returns></returns>
        public (double[], double[]) GetXYNoUpdate()
        {
            Array.Copy(adcData, adcDataSorted, pointsOnDisplay);
            Array.Copy(dacData, dacDataSorted, pointsOnDisplay);

            Array.Sort(dacDataSorted, adcDataSorted);
            List<double> dacList = dacDataSorted.ToList();
            List<double> adcList = adcDataSorted.ToList();
            
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

        public void setSize(int size)
        {
            lock (editJobs)
                editJobs.Add(() => _setSize(size));
        }
        private void _setSize(int size)
        {
            pointsOnDisplay = size;
            adcData = new double[size];
            dacData = new double[size];
            adcDataSorted = new double[size];
            dacDataSorted = new double[size];
            _UpdateTimeData();
        }

        public void setXYSmoothing(double value)
        {
            lock (editJobs)
                editJobs.Add(() => _setXYSmoothing(value));
        }
        private void _setXYSmoothing(double value)
        {
            XYSmoothing = value;
        }

        /// <summary>
        /// Checks if there is enough data available to be displayed
        /// </summary>
        /// <param name="displayResolution"></param>
        /// <param name="averages"></param>
        /// <param name="oldestData"></param>
        /// <param name="newestData"></param>
        /// <returns></returns>
        public static bool enoughSamples(int displayResolution, int averages, int oldestData, int newestData)
        {
            return displayResolution * (averages + 1) <= oldestData - newestData + 1;
        }
    }
}
