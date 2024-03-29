﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Net.Configuration;
using LaserLeash.Properties;
using System.IO;
using System.Globalization;

namespace LaserLeash
{
    public partial class SpectroscopyControlForm : Form
    {
        Series seriesXY;
        Series seriesZeroXY;
        Series seriesOutput;
        Series seriesDemod;
        Series seriesZeroDemod;
        private delegate void SafeCallDelegate();
        private delegate void SafeCallDelegateStr(string s);
        VerticalLineAnnotation LockLineAnnotation;
        VerticalLineAnnotation xyLineAnnotaion;
        MQTTPublisher mqtt;
        bool lockMode = false;
        Oscilloscope osciDisplay;
        Memory memory;

        double previousAmplitude, previousOffset;


        public static SpectroscopyControlForm form;

        public static bool stopped { get; private set; }
        public bool receiveEvents { get; private set; }

        public SpectroscopyControlForm(Memory memory, Oscilloscope osciDisplay, MQTTPublisher mqtt)
        {
            stopped = false;
            InitializeComponent();
            InitGraph();
            this.mqtt = mqtt;
            this.osciDisplay = osciDisplay;
            this.memory = memory;

            InitSettings();

            SpectroscopyControlForm.form = this;
        }

        public void InitGraph()
        {
            // create a series for each line
            seriesXY = new Series("Channel 0");
            seriesXY.ChartType = SeriesChartType.Line;
            seriesZeroXY = new Series("Zero Line");
            seriesZeroXY.IsVisibleInLegend = false;
            seriesZeroXY.ChartType = SeriesChartType.Line;
            seriesZeroXY.Color = Color.Gray;
            seriesZeroXY.BorderWidth = 2;
            seriesZeroXY.BorderDashStyle = ChartDashStyle.Dash;
            seriesOutput = new Series("Output (left)");
            seriesOutput.ChartType = SeriesChartType.Line;
            seriesDemod = new Series("Demodulated (right)");
            seriesDemod.ChartType = SeriesChartType.Line;
            seriesZeroDemod = new Series("Zero Line");
            seriesZeroDemod.IsVisibleInLegend = false;
            seriesZeroDemod.ChartType = SeriesChartType.Line;
            seriesZeroDemod.Color = Color.Gray;
            seriesZeroDemod.BorderWidth = 2;
            seriesZeroDemod.BorderDashStyle = ChartDashStyle.Dash;


            // add each series to the chart
            chartXY.Series.Clear();
            chartXY.Series.Add(seriesXY);
            chartXY.Series.Add(seriesZeroXY);
            chartTimeseries.Series.Clear();
            chartTimeseries.Series.Add(seriesOutput);
            chartTimeseries.Series.Add(seriesDemod);
            chartTimeseries.Series.Add(seriesZeroDemod);

            chartXY.Series[0].YAxisType = AxisType.Primary;
            chartTimeseries.Series[0].YAxisType = AxisType.Primary;
            chartTimeseries.Series[1].YAxisType = AxisType.Secondary;
            chartTimeseries.Series[2].YAxisType = AxisType.Secondary;

            // additional styling
            chartXY.ResetAutoValues();
            chartTimeseries.ResetAutoValues();
            chartXY.Titles.Clear();
            chartTimeseries.Titles.Clear();
            chartXY.ChartAreas[0].AxisX.Title = "Output Voltage";
            chartXY.ChartAreas[0].AxisY.Title = "Demodulated Voltage";
            chartTimeseries.ChartAreas[0].AxisX.Title = "Time (s)";
            chartTimeseries.ChartAreas[0].AxisY2.Title = "Demodulated Voltage";
            chartTimeseries.ChartAreas[0].AxisY.Title = "Output Voltage";
            chartXY.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
            chartXY.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
            chartTimeseries.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
            chartTimeseries.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
            chartXY.ChartAreas[0].AxisX.LabelStyle.Format = "0.000";
            chartXY.ChartAreas[0].AxisY.LabelStyle.Format = "0.0000";
            chartTimeseries.ChartAreas[0].AxisY.LabelStyle.Format = "0.0000";
            chartTimeseries.ChartAreas[0].AxisY2.LabelStyle.Format = "0.0000";
            chartTimeseries.ChartAreas[0].AxisY.IsStartedFromZero = false;

            chartTimeseries.ChartAreas[0].AxisY2.LineColor = Color.Transparent;
            chartTimeseries.ChartAreas[0].AxisY2.MajorGrid.Enabled = false;
            chartTimeseries.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;

            // zooming https://stackoverflow.com/questions/13584061/how-to-enable-zooming-in-microsoft-chart-control-by-using-mouse-wheel
            var CA = chartXY.ChartAreas[0];

            // vertical lockpoint line https://stackoverflow.com/questions/25801257/c-sharp-line-chart-how-to-create-vertical-line
            LockLineAnnotation = new VerticalLineAnnotation();
            LockLineAnnotation.AxisX = CA.AxisX;
            LockLineAnnotation.AllowMoving = true;
            LockLineAnnotation.IsInfinitive = true;
            LockLineAnnotation.ClipToChartArea = CA.Name;
            LockLineAnnotation.Name = "Lock Point";
            LockLineAnnotation.LineColor = Color.Red;
            LockLineAnnotation.LineWidth = 1;
            LockLineAnnotation.X = 1;
            chartXY.Annotations.Add(LockLineAnnotation);
            //vertical output voltage line
            xyLineAnnotaion = new VerticalLineAnnotation();
            xyLineAnnotaion.AxisX = CA.AxisX;
            xyLineAnnotaion.AllowMoving = true;
            xyLineAnnotaion.IsInfinitive = true;
            xyLineAnnotaion.ClipToChartArea = CA.Name;
            xyLineAnnotaion.Name = "Output";
            xyLineAnnotaion.LineColor = Color.Gray;
            xyLineAnnotaion.LineWidth = 1;
            xyLineAnnotaion.X = 2;
            chartXY.Annotations.Add(xyLineAnnotaion);

            SetNumericTooltip(modAmpText);
            SetNumericTooltip(modAttText);
            SetNumericTooltip(modFreqText);
            SetNumericTooltip(modPhaseText);
            SetNumericTooltip(demodAmpText);
            SetNumericTooltip(demodAttText);
            SetNumericTooltip(demodFreqText);
            SetNumericTooltip(KpText);
            SetNumericTooltip(KiText);
            SetNumericTooltip(KdText);
            SetNumericTooltip(YminText);
            SetNumericTooltip(YmaxText);
            SetNumericTooltip(StreamTargetPortInput);
            SetNumericTooltip(MemorySizeText, $"{UnitConvert.TimeToSample(1)} samples per second\nClears memory when changed");
            SetNumericTooltip(samplesOnDisplayText);
            SetNumericTooltip(AveragesText);
            SetNumericTooltip(FGFrequencyText);
            SetNumericTooltip(FGAmplitudeText);
            SetNumericTooltip(ChannelInput);

            toolTip1.SetToolTip(InitButton, "Sets Stream Target, Amplitude, Attenuation, Frequency, Phase, Scan Amplitude, Scan Frequency, Scan Offset, Scan Symmetry and goes into Unlock mode");
            toolTip1.SetToolTip(SaveMemoryButton, "Saves the raw data of the current viewing range to a file.");
        }

        private void SetNumericTooltip(NumericUpDown input, string additionalInfo = "")
        {
            additionalInfo = additionalInfo != "" ? "\n" + additionalInfo : "";
            toolTip1.SetToolTip(input, $"Min: {input.Minimum} Max: {input.Maximum} Step: {input.Increment} " + additionalInfo);
        }

        public void OnNewDataXY()
        {
            if (stopped)
            {
                return;
            }
            if (chartXY.InvokeRequired)
            {
                try
                {
                    if (!(chartXY.Disposing | chartXY.IsDisposed))
                        chartXY.Invoke(new SafeCallDelegate(OnNewDataXY), new object[] { });
                } catch (System.ComponentModel.InvalidAsynchronousStateException)
                {
                    Console.WriteLine("Idk how to fix this...");
                }
                return;
            }
            else
            {
                if (!lockMode)
                {
                    osciDisplay.GetTimeSeries();
                    (double[] xData, double[] yData) = osciDisplay.GetXYNoUpdate();
                    if (xData.Length > 1)
                    {
                        seriesXY.Points.DataBindXY(xData, yData);
                        seriesZeroXY.Points.DataBindXY(new double[] { xData[0], xData[xData.Length -1] }, new double[] { 0, 0 });
                    }
                    chartXY.Update();
                }
            }
        }

        public void OnNewDataTimeSeries()
        {
            if (stopped)
            {
                return;
            }
            if (chartTimeseries.InvokeRequired)
            {
                try
                {
                    if (!(chartTimeseries.Disposing | chartTimeseries.IsDisposed))
                        chartTimeseries.Invoke(new SafeCallDelegate(OnNewDataTimeSeries), new object[] { });
                } catch (System.ComponentModel.InvalidAsynchronousStateException)
                {
                    Console.WriteLine("Idk how to fix this...");
                }
                return;
            }
            else 
            {
                double[] dataDac, dataAdc;
                double newestValue;
                bool allNaN;
                (dataAdc, dataDac, newestValue, allNaN) = osciDisplay.GetTimeSeries();

                seriesOutput.Points.DataBindXY(osciDisplay.timeData, dataDac);
                seriesDemod.Points.DataBindXY(osciDisplay.timeData, dataAdc);
                seriesZeroDemod.Points.DataBindXY(new double[] { osciDisplay.timeData[0], osciDisplay.timeData[osciDisplay.timeData.Length - 1] }, new double[] { 0, 0 });

                xyLineAnnotaion.X = newestValue;

                if (allNaN) //prevent exception if data is only NaN
                {
                    chartTimeseries.ChartAreas[0].AxisY.Minimum = 0;
                    chartTimeseries.ChartAreas[0].AxisY.Maximum = 1;
                    chartTimeseries.ChartAreas[0].AxisY2.Minimum = 0;
                    chartTimeseries.ChartAreas[0].AxisY2.Maximum = 1;
                } else
                {
                    //if there is data, then auto scale
                    chartTimeseries.ChartAreas[0].AxisY.Minimum = double.NaN;
                    chartTimeseries.ChartAreas[0].AxisY.Maximum = double.NaN;
                    chartTimeseries.ChartAreas[0].AxisY2.Minimum = double.NaN;
                    chartTimeseries.ChartAreas[0].AxisY2.Maximum = double.NaN;
                }

                chartTimeseries.Update();
            }
        }

        public void RefreshData()
        {
            osciDisplay.UpdateParameters();

            OnNewDataTimeSeries();
            OnNewDataXY();
        }

        private void chartXY_Click(object sender, EventArgs e)
        {
            var me = e as MouseEventArgs;
            LockLineAnnotation.X = chartXY.ChartAreas[0].AxisX.PixelPositionToValue(me.X);
        }

        private void setScanRange(double min, double max)
        {
            previousAmplitude = (max - min) / 2;
            mqtt.sendScanAmplitude(previousAmplitude);
            previousOffset = (min + max) / 2;
            mqtt.sendScanOffset(previousOffset, Decimal.ToDouble(YminText.Value), Decimal.ToDouble(YmaxText.Value));
        }

        private void chartXY_DoubleClick(object sender, EventArgs e)
        {
            if (lockMode)
                return;

            var me = e as MouseEventArgs;

            if (me.Button == MouseButtons.Left)
            {
                var xAxis = chartXY.ChartAreas[0].AxisX;
                var xMin = xAxis.ScaleView.ViewMinimum;
                var xMax = xAxis.ScaleView.ViewMaximum;

                var posXStart = xAxis.PixelPositionToValue(me.Location.X) - (xMax - xMin) / 4;
                var posXFinish = xAxis.PixelPositionToValue(me.Location.X) + (xMax - xMin) / 4;

                setScanRange(Math.Max(posXStart, 0), Math.Min(posXFinish, 10));
            }
            else if (me.Button == MouseButtons.Right)
            {
                var xAxis = chartXY.ChartAreas[0].AxisX;
                var xMin = xAxis.ScaleView.ViewMinimum;
                var xMax = xAxis.ScaleView.ViewMaximum;

                var posXStart = (xMax + xMin) / 2 - (xMax - xMin) / 1.3;
                var posXFinish = (xMax + xMin) / 2 + (xMax - xMin) / 1.3;

                setScanRange(Math.Max(posXStart, Decimal.ToDouble(YminText.Value)), Math.Min(posXFinish, Decimal.ToDouble(YmaxText.Value)));
            }
        }

        private void LockButton_Click(object sender, EventArgs e)
        {
            LockButton.Checked = true;
            UnlockButton.Checked = false;

            //keep previous range for timeseries
            double min, max;
            min = osciDisplay.GetDACMinNoUpdate();
            max = osciDisplay.GetDACMaxNoUpdate();
            chartTimeseries.ChartAreas[0].AxisY.Maximum = max + 0.3 * (max - min);
            chartTimeseries.ChartAreas[0].AxisY.Minimum = min - 0.3 * (max - min);
            min = osciDisplay.GetADCMinNoUpdate();
            max = osciDisplay.GetADCMaxNoUpdate();
            chartTimeseries.ChartAreas[0].AxisY2.Maximum = max + 0.3 * (max - min);
            chartTimeseries.ChartAreas[0].AxisY2.Minimum = min - 0.3 * (max - min);

            Task.Run(() => // do this in a separate thread to be able to update the graph during locking
            {
                lockMode = true;

                mqtt.sendScanAmplitude(0);
                Thread.Sleep(300); 
                mqtt.sendScanOffset(LockLineAnnotation.X, Decimal.ToDouble(YminText.Value), Decimal.ToDouble(YmaxText.Value));
                Thread.Sleep(300); // is there a better way? we can only check if the value has changed at the broker? but we are interested in when it changed on the stabilizer
                List<double> iirs = UnitConvert.CalculateIIR(Decimal.ToDouble(KpText.Value), Decimal.ToDouble(KiText.Value), Decimal.ToDouble(KdText.Value), UnitConvert.SamplePeriod);
                mqtt.sendPID(0, iirs.ToBracketString(), Decimal.ToDouble(YminText.Value), Decimal.ToDouble(YmaxText.Value));
            });
        }

        private void UnlockButton_Click(object sender, EventArgs e)
        {
            LockButton.Checked = false;
            UnlockButton.Checked = true;

            setScanRange(previousOffset - previousAmplitude, previousOffset + previousAmplitude);

            chartTimeseries.ChartAreas[0].AxisY.Maximum = double.NaN;
            chartTimeseries.ChartAreas[0].AxisY.Minimum = double.NaN;
            chartTimeseries.ChartAreas[0].AxisY2.Maximum = double.NaN;
            chartTimeseries.ChartAreas[0].AxisY2.Minimum = double.NaN;

            lockMode = false;
        }

        private void chartTimeseries_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var me = e as MouseEventArgs;
            if (me.Button == MouseButtons.Left)
            {
                var xAxis = chartTimeseries.ChartAreas[0].AxisX;
                var xMin = xAxis.ScaleView.ViewMinimum;
                var xMax = xAxis.ScaleView.ViewMaximum;
                double clickPos = chartTimeseries.ChartAreas[0].AxisX.PixelPositionToValue(me.X);
                double zoomCenterRelativeNewestToOld = 1 - (clickPos - xMin) / (xMax - xMin);

                osciDisplay.ZoomIn(zoomCenterRelativeNewestToOld);
            } else if(me.Button == MouseButtons.Right)
            {
                osciDisplay.ZoomOut();
            }
        }

        private void InitButton_Click(object sender, EventArgs e)
        {
            LockButton.Checked = false;
            UnlockButton.Checked = false;

            mqtt.sendStreamTarget(StreamTargetIPInput.Text, StreamTargetPortInput.Text);
            mqtt.sendModulationAmplitude(Decimal.ToDouble(modAmpText.Value));
            mqtt.sendModulationAttenuation(Decimal.ToDouble(modAttText.Value));
            mqtt.sendModulationFrequencyMHz  (Decimal.ToDouble(modFreqText.Value));
            mqtt.sendDemodulationFrequencyMHz(Decimal.ToDouble(demodFreqText.Value));
            mqtt.sendDemodulationAttenuation(Decimal.ToDouble(demodAttText.Value));
            mqtt.sendDemodulationAmplitude(Decimal.ToDouble(demodAmpText.Value));
            mqtt.sendPhase(Decimal.ToDouble(modPhaseText.Value));
            double min = Decimal.ToDouble(YminText.Value), max = Decimal.ToDouble(YmaxText.Value);
            setScanRange(min, max);
            mqtt.sendScanFrequency(Decimal.ToDouble(FGFrequencyText.Value));
            mqtt.sendScanSymmetry(1);
            mqtt.sendSignal();
            lockMode = false;

            InitButton.BackColor = System.Drawing.SystemColors.Control; //this is the same color as the other buttons, but looks different...
        }

        public static void WriteLine(string message)
        {
            if (!(form != null && form.logText.InvokeRequired))
                Console.WriteLine(message);
            if (form == null)
                return;
            if (SpectroscopyControlForm.stopped)
                return;
            if (form.logText.InvokeRequired)
            {
                form.logText.BeginInvoke(new SafeCallDelegateStr(SpectroscopyControlForm.WriteLine), new object[] { message }); //asynchronous required to avoid deadlock on memory when called from deserializer with updating the froms data
                return;
            }
            else
            {
                form.logText.AppendText(Environment.NewLine);
                form.logText.AppendText(message);
            }
        }

        protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e)
        {
            stopped = true;
        }

        private void SaveMemoryButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            saveFileDialog1.Title = "Save Memory";
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.Filter = "All files (*.*)|*.*";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter file = new StreamWriter(saveFileDialog1.FileName))
                {
                    double[] dac = memory.getDACArray(osciDisplay.newestSampleToDisplay, osciDisplay.oldestSampleToDisplay);
                    double[] adc = memory.getADCArray(osciDisplay.newestSampleToDisplay, osciDisplay.oldestSampleToDisplay);

                    NumberFormatInfo nfi = new NumberFormatInfo();
                    nfi.NumberDecimalSeparator = ".";

                    file.Write("DAC [V],ADC [V]\n");
                    for (int i = 0; i < dac.Length; i++)
                    {
                        file.Write(dac[i].ToString(nfi) + "," + adc[i].ToString(nfi) + "\n");
                    }
                }
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (!receiveEvents)
                return;
            if (radioButton1.Checked)
                mqtt.sendScanSymmetry(1);
        }
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (!receiveEvents)
                return;
            if (radioButton2.Checked)
                mqtt.sendScanSymmetry(0.5);
        }

        private void reconnectButton_Click(object sender, EventArgs e)
        {
            mqtt.connect(Properties.Settings.Default.MQTTServer, Properties.Settings.Default.MQTTPort);
        }

        private void freezeMemoryCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            memory.freeze = ((CheckBox)sender).Checked;
        }
    }
}
