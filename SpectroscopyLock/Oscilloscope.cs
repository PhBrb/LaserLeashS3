using System;
using System.Collections.Generic;
using System.Linq;

namespace LaserLeash
{
    /// <summary>
    /// Holds and prepares date for displaying
    /// </summary>
    public class Oscilloscope
    {
        Memory memory;
        int averages = Properties.Settings.Default.Averages;
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
        /// Pools jobs that change parameters in the class (for thread safety)
        /// </summary>
        private List<Action> editJobs = new List<Action>();

        public Oscilloscope(Memory memory)
        {
            this.memory = memory;

            //default to maximum range
            newestSampleToDisplay = 0;
            oldestSampleToDisplay = memory.getSize();

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

        /// <summary>
        /// Updates the internal time array, necessary after changing <see cref="pointsOnDisplay"/>, <see cref="oldestSampleToDisplay"/> or <see cref="newestSampleToDisplay"/>
        /// </summary>
        public void UpdateTimeData()
        {
            lock (editJobs) 
                editJobs.Add(_UpdateTimeData);
        }
        private void _UpdateTimeData()
        {
            timeData = new double[pointsOnDisplay];
            int samplesPerPoint = (oldestSampleToDisplay - newestSampleToDisplay + 1) / pointsOnDisplay;

            ///map a range of 0 - <see cref="pointsOnDisplay"/> to <see cref="newestSampleToDisplay"/> - <see cref="oldestSampleToDisplay"/>
            for (int i = 0; i < pointsOnDisplay; i++)
            {
                timeData[i] = -UnitConvert.SampleToTime((pointsOnDisplay - i) * samplesPerPoint + newestSampleToDisplay);
            }
        }

        /// <summary>
        /// Returns ADC and DAC data ín the display range, the current DAC output and a flag if all values in the array are NaN
        /// </summary>
        /// <returns>(ADC, DAC, current DAC, NaN)</returns>
        public (double[], double[], double, bool) GetTimeSeries()
        {
            double newestDACValue = 0;
            int iSampleDistance = (oldestSampleToDisplay - newestSampleToDisplay + 1) / pointsOnDisplay;
            int samplesToUse = averages + 1;
            int iOffset = newestSampleToDisplay;
            bool allNaN = true;
            lock (memory)
            {
                for (int i = 0; i < pointsOnDisplay; i++)
                {
                    /// get sample corresponding to point i, shift it by <see cref="samplesToUse"/>  to make sure there is enough data to average over
                    /// and save newest data i=0 into last adcData index
                    adcData[pointsOnDisplay - i - 1] = memory.GetADCAverageFromPast(-i * iSampleDistance - samplesToUse - iOffset, samplesToUse);
                    if(!double.IsNaN(adcData[pointsOnDisplay - i - 1]))
                        allNaN = false;
                }
                for (int i = 0; i < pointsOnDisplay; i++)
                {
                    /// get sample corresponding to point i, shift it by <see cref="samplesToUse"/>  to make sure there is enough data to average over
                    /// and save newest data i=0 into last dacData index
                    dacData[pointsOnDisplay - i - 1] = memory.GetDACAverageFromPast(-i * iSampleDistance - samplesToUse - iOffset, samplesToUse);
                    if (!double.IsNaN(adcData[pointsOnDisplay - i - 1]))
                        allNaN = false;
                }
                newestDACValue = memory.GetDACAverageFromPast(-samplesToUse,samplesToUse);
            }
            return (adcData, dacData, newestDACValue, allNaN);
        }

        /// <summary>
        /// Reduces viewing range of the timeseries and tries to center around <paramref name="position"/>
        /// </summary>
        /// <param name="position">Position from 0 to 1, relative to current viewing range</param>
        public void ZoomIn(double position)
        {
            lock (editJobs)
                editJobs.Add(() => _ZoomIn(position));
        }
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
                SpectroscopyControlForm.WriteLine(msg);

                //zoom in as far as possible
                int samplesNeeded = pointsOnDisplay * (averages + 1);
                //relative to oldest sample for easier calculation
                int iStart = Math.Max(0, iNewCenter - samplesNeeded / 2);
                iStart = Math.Min(memory.getSize() - 1 - samplesNeeded, iStart);

                newestSampleToDisplay = iStart; 
                oldestSampleToDisplay = iStart + samplesNeeded - 1;
            }
            _UpdateTimeData();
        }

        /// <summary>
        /// Increases range of the timeseries
        /// </summary>
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

        /// <summary>
        /// Resets to full viewing range of the timeseries
        /// </summary>
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
        /// <returns>(DAC, ADC)</returns>
        public (double[], double[]) GetXYNoUpdate()
        {
            Array.Copy(adcData, adcDataSorted, pointsOnDisplay);
            Array.Copy(dacData, dacDataSorted, pointsOnDisplay);

            Array.Sort(dacDataSorted, adcDataSorted); //sort by x axis value, since the graph connects data by index

            List<double> dacList = dacDataSorted.ToList(); //convert to list to make editing easier
            List<double> adcList = adcDataSorted.ToList();
            
            for(int i = pointsOnDisplay - 1; i >= 0 ; i--) // remove NaN
            {
                if (double.IsNaN(dacDataSorted[i]) || double.IsNaN(adcDataSorted[i]))
                {
                    adcList.RemoveAt(i);
                    dacList.RemoveAt(i);
                }
            }

            for (int i = adcList.Count - 1; i > 0; i--) // average ADC values if DAC values are the same (multiple datapoints at the same output voltage)
            {
                int sumCount = 1; //one element will be added separately to not delete it in the loop
                double sum = 0;
                while (i > 1 && dacList[i] == dacList[i - 1]) //relying on equal values being grouped by previous sorting
                {
                    sumCount += 1;
                    sum += adcList[i];
                    adcList.RemoveAt(i);
                    dacList.RemoveAt(i);
                    i--;
                }
                adcList[i] = (sum + adcList[i]) / sumCount;
            }
            return (dacList.ToArray(), adcList.ToArray());
        }

        /// <summary>
        /// Returns the minimum of the ADC values, using values that are already in the memory
        /// </summary>
        /// <returns></returns>
        public double GetADCMinNoUpdate()
        {
            return adcDataSorted.Min();
        }

        /// <summary>
        /// Returns the maximum of the ADC values, using values that are already in the memory
        /// </summary>
        /// <returns></returns>
        public double GetADCMaxNoUpdate()
        {
            return adcDataSorted.Max();
        }

        /// <summary>
        /// Returns the minimum of the DAC values, using values that are already in the memory
        /// </summary>
        /// <returns></returns>
        public double GetDACMinNoUpdate()
        {
            return dacDataSorted[0];
        }


        /// <summary>
        /// Returns the maximum of the DAC values, using values that are already in the memory
        /// </summary>
        /// <returns></returns>
        public double GetDACMaxNoUpdate()
        {
            return dacDataSorted[adcDataSorted.Length - 1];
        }

        public void setDisplaySize(int samples)
        {
            lock (editJobs)
                editJobs.Add(() => _setDisplaySize(samples));
        }
        private void _setDisplaySize(int samples)
        {
            pointsOnDisplay = samples;
            adcData = new double[samples];
            dacData = new double[samples];
            adcDataSorted = new double[samples];
            dacDataSorted = new double[samples];
            _UpdateTimeData();
        }

        /// <summary>
        /// Checks if there is enough data available to be displayed
        /// </summary>
        /// <param name="displayResolution"></param>
        /// <param name="averages"></param>
        /// <param name="memorySize"></param>
        /// <returns></returns>
        public static bool enoughSamples(int displayResolution, int averages, int memorySize)
        {
            return displayResolution * (averages + 1) <= memorySize;
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
            return enoughSamples(displayResolution, averages, oldestData - newestData + 1);
        }
    }
}
