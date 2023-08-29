using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Net.Configuration;
using ChartTest2.Properties;

namespace ChartTest2
{
    public partial class SpectrscopyControlForm : Form
    {
        Series seriesXY;
        Series seriesOutput;
        Series seriesDemod;
        private delegate void SafeCallDelegate();
        private delegate void SafeCallDelegateStr(string s);
        VerticalLineAnnotation LockLineAnnotation;
        MQTTPublisher mqtt;
        bool lockMode = false;
        OsciDisplay osciDisplay;
        Memory memory;

        double previousAmplitude, previousOffset;


        public static SpectrscopyControlForm form;

        public bool stopped { get; private set; }

        public SpectrscopyControlForm(Memory memory, OsciDisplay osciDisplay, MQTTPublisher mqtt)
        {
            stopped = false;
            InitializeComponent();
            InitGraph();
            this.mqtt = mqtt;
            this.osciDisplay = osciDisplay;
            this.memory = memory;

            InitSettings();

            SpectrscopyControlForm.form = this;
        }


        public void InitGraph()
        {
            // create a series for each line
            seriesXY = new Series("Channel 0");
            seriesXY.ChartType = SeriesChartType.Line;
            seriesOutput = new Series("Output (left)");
            seriesOutput.ChartType = SeriesChartType.Line;
            seriesDemod = new Series("Demodulated (right)");
            seriesDemod.ChartType = SeriesChartType.Line;


            // add each series to the chart
            chartXY.Series.Clear();
            chartXY.Series.Add(seriesXY);
            chartTimeseries.Series.Clear();
            chartTimeseries.Series.Add(seriesOutput);
            chartTimeseries.Series.Add(seriesDemod);

            chartXY.Series[0].YAxisType = AxisType.Primary;
            chartTimeseries.Series[0].YAxisType = AxisType.Primary;
            chartTimeseries.Series[1].YAxisType = AxisType.Secondary;

            // additional styling
            chartXY.ResetAutoValues();
            chartTimeseries.ResetAutoValues();
            chartXY.Titles.Clear();
            chartTimeseries.Titles.Clear();
            chartXY.ChartAreas[0].AxisX.Title = "Voltage output";
            chartXY.ChartAreas[0].AxisY.Title = "Demodulation Voltage";
            chartTimeseries.ChartAreas[0].AxisX.Title = "Time (s)";
            chartTimeseries.ChartAreas[0].AxisY2.Title = "Demodulation Voltage";
            chartTimeseries.ChartAreas[0].AxisY.Title = "Voltage output";
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
            SetNumericTooltip(SamplerateText);
            SetNumericTooltip(StreamTargetPortInput);
            SetNumericTooltip(MemorySizeText, $"{UnitConvert.TimeToSample(1)} samples per second\nClears memory when changed");
            SetNumericTooltip(samplesOnDisplayText);
            SetNumericTooltip(AveragesText);
            SetNumericTooltip(XYSmoothing, "Implemented as a low pass");
            SetNumericTooltip(FGFrequencyText);
            SetNumericTooltip(FGAmplitudeText);
        }

        private void SetNumericTooltip(NumericUpDown input, string additionalInfo = "")
        {
            additionalInfo = additionalInfo != "" ? "\n" + additionalInfo : "";
            toolTip1.SetToolTip(input, $"Step: {input.Increment} Min: {input.Minimum} Max: {input.Maximum}" + additionalInfo);
        }


        public void OnNewDataXY()
        {
            if (chartXY.InvokeRequired)
            {
                try
                {
                    chartXY.Invoke(new SafeCallDelegate(OnNewDataXY), new object[] { });
                } catch (System.ComponentModel.InvalidAsynchronousStateException)
                {
                    Console.WriteLine("Idk how to fix this...");
                }
                return;
            }
            else
            {
                if (!lockMode && !stopped)
                {
                    osciDisplay.GetTimeSeries();
                    (double[] xData, double[] yData) = osciDisplay.GetXYNoUpdate();
                    if (xData.Length > 0)
                        seriesXY.Points.DataBindXY(xData, yData); //TODO fix nullreferenceexception
                    chartXY.Update();
                }
            }
        }

        public void OnNewDataTimeSeries()
        {
            if (chartTimeseries.InvokeRequired)
            {
                try
                {
                    chartTimeseries.Invoke(new SafeCallDelegate(OnNewDataTimeSeries), new object[] { });
                } catch (System.ComponentModel.InvalidAsynchronousStateException)
                {
                    Console.WriteLine("Idk how to fix this...");
                }
            return;
            }
            else if(!stopped)
            {
                double[] dataDac, dataAdc;
                (dataAdc, dataDac) = osciDisplay.GetTimeSeries();

                seriesOutput.Points.DataBindXY(osciDisplay.timeData, dataDac);
                seriesDemod.Points.DataBindXY(osciDisplay.timeData, dataAdc);

                chartTimeseries.Update();
            }
        }

        public void OnNewData()
        {
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

                setScanRange(Math.Max(posXStart, 0), Math.Min(posXFinish, 10));
            }
        }

        private void LockButton_Click(object sender, EventArgs e)
        {
            //keep previous range
            double min, max;
            min = osciDisplay.GetDACMinNoUpdate();
            max = osciDisplay.GetDACMaxNoUpdate();
            chartTimeseries.ChartAreas[0].AxisY.Maximum = max + 0.3 * (max - min);
            chartTimeseries.ChartAreas[0].AxisY.Minimum = min - 0.3 * (max - min);
            min = osciDisplay.GetADCMinNoUpdate();
            max = osciDisplay.GetADCMaxNoUpdate();
            chartTimeseries.ChartAreas[0].AxisY2.Maximum = max + 0.3 * (max - min);
            chartTimeseries.ChartAreas[0].AxisY2.Minimum = min - 0.3 * (max - min);

            lockMode = true;

            mqtt.sendScanAmplitude(0);
            Thread.Sleep(100); // is there a better way? we can only check if the value has changed at the broker? but we are interested in when it changed on the stabilizer
            mqtt.sendScanOffset(LockLineAnnotation.X, Decimal.ToDouble(YminText.Value), Decimal.ToDouble(YmaxText.Value));
            Thread.Sleep(100);

            List<double> result = CalculateIIR(Decimal.ToDouble(KpText.Value), Decimal.ToDouble(KiText.Value), Decimal.ToDouble(KdText.Value), Decimal.ToDouble(SamplerateText.Value));

            Task.Run(() =>
            {
                Thread.Sleep(100);
                mqtt.sendPID(0, result.ToBracketString(), Decimal.ToDouble(YminText.Value), Decimal.ToDouble(YmaxText.Value));
            });
        }

        private List<double> CalculateIIR(double Kp, double Ki, double Kd, double samplePeriod) {
            // original python code
            //
            // kernels = [
            //    [1, 0, 0],
            //    [1, -1, 0],
            //    [1, -2, 1]
            // ]
            // gains = [args.Kii, args.Ki, args.Kp, args.Kd, args.Kdd]
            // limits = [args.Kii / args.Kii_limit, args.Ki / args.Ki_limit,
            //          1, args.Kd / args.Kd_limit, args.Kdd / args.Kdd_limit]
            // w = 2 * pi * args.sample_period
            // b = [ sum(gains[2 - order + i] * w * *(order - i) * kernels[i][j] for i in range(3))
            //      for j in range(3)]

            // a = [sum(limits[2 - order + i] * w * *(order - i) * kernels[i][j]
            //         for i in range(3)) for j in range(3)]
            // b = [i / a[0] for i in b]
            // a = [i / a[0] for i in a]
            // assert a[0] == 1
            // return b + [-ai for ai in a[1:]]

            double[,] kernels = new double[,] { { 1, 0, 0 }, { 1, -1, 0 }, { 1, -2, 1 } };

            double[] gains = new double[] { 0, Ki, Kp, Kd, 0 };
            double[] limits = new double[] { 0, 0, 1, 0, 0 };

            int order = Ki != 0 ? 1 : 0;
            double w = 2 * Math.PI * samplePeriod;

            double[] b = new double[] { 0, 0, 0 };
            double[] a = new double[] { 0, 0, 0 };

            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 3; i++)
                {
                    b[j] += gains[2 - order + i] * Math.Pow(w, order - i) * kernels[i, j];
                }
            }

            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 3; i++)
                {
                    a[j] += limits[2 - order + i] * Math.Pow(w, order - i) * kernels[i, j];
                }
            }
            for (int i = 0; i < 3; i++)
            {
                b[i] = b[i] / a[0];
                a[i] = a[i] / a[0];
            }
            if (a[0] != 1)
                throw new Exception();

            for (int i = 0; i < 3; i++)
            {
                a[i] *= -1;
            }
            List<double> result = b.ToList();
            result.AddRange(a.Skip(1));

            return result;
        }

        private void UnlockButton_Click(object sender, EventArgs e)
        {
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
                //Console.WriteLine($"click pos {clickPos}");
                double zoomCenterRelativeNewestToOld = 1 - (clickPos - xMin) / (xMax - xMin);
                //Console.WriteLine($"zoom center {zoomCenterRelativeNewestToOld}");

                osciDisplay.ZoomIn(zoomCenterRelativeNewestToOld);
            } else if(me.Button == MouseButtons.Right)
            {
                osciDisplay.ZoomOut();
            }
        }

        private void InitButton_Click(object sender, EventArgs e)
        {
            mqtt.sendStreamTarget(StreamTargetIPInput.Text, StreamTargetPortInput.Text);
            mqtt.sendModulationAmplitude(Decimal.ToDouble(modAmpText.Value));
            mqtt.sendModulationAttenuation(Decimal.ToDouble(modAttText.Value));
            mqtt.sendModulationFrequencyMHz  (Decimal.ToDouble(modFreqText.Value));
            mqtt.sendDemodulationFrequencyMHz(Decimal.ToDouble(demodFreqText.Value));
            mqtt.sendDemodulationAttenuation(Decimal.ToDouble(demodAttText.Value));
            mqtt.sendDemodulationAmplitude(Decimal.ToDouble(demodAmpText.Value));
            mqtt.sendPhase(Decimal.ToDouble(modPhaseText.Value));
            double min = Decimal.ToDouble(YminText.Value), max = Decimal.ToDouble(YmaxText.Value);
            mqtt.sendScanAmplitude((max-min)/2);
            mqtt.sendScanOffset((max+min)/2, min, max);
            mqtt.sendScanFrequency(Decimal.ToDouble(FGFrequencyText.Value));
            mqtt.sendScanSymmetry(0);
            lockMode = false;
        }

        public static void WriteLine(string message)
        {
            if (form.logText.InvokeRequired)
            {
                form.logText.Invoke(new SafeCallDelegateStr(SpectrscopyControlForm.WriteLine), new object[] {message});
                return;
            }
            else
            {
                if(!form.stopped)
                    form.logText.AppendText("\r\n" + message);
            }
        }

        private void SpectrscopyControlForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            stopped = true;
        }

        private void YminText_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (YminText.Value >= YmaxText.Value)
            {
                e.Cancel = true;
                WriteLine("Reduce minimum");
            }
        }

        private void YmaxText_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (YminText.Value >= YmaxText.Value)
            {
                e.Cancel = true;
                WriteLine("Increase maximum");
            }
        }

        private void Memory_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //TODO this does not take newestSampleToDisplay into account
            //TODO the validity check should be in the memory/display class
            if (decimal.ToDouble(samplesOnDisplayText.Value) * (1 + decimal.ToDouble(AveragesText.Value)) > UnitConvert.TimeToSample(decimal.ToDouble(MemorySizeText.Value))) //if there are less samples in the memory than are needed for the display
            {
                e.Cancel = true;
                string msg = "Memory size has to be <= display resolution * (averages +1)";
                if (sender == MemorySizeText)
                {
                    msg = "Can't reduce memory size. Too high display resolution or too much averaging." + msg;
                }
                else
                if (sender == AveragesText)
                {
                    msg = "Can't increase averaging. Too high display resolution or memory too small." + msg;
                }
                else
                if (sender == samplesOnDisplayText)
                {
                    msg = "Can't increase display resolution. Too much averaging or memory too small." + msg;
                }
                WriteLine(msg);
            }
        }

        private void freezeMemoryCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            memory.freeze = ((CheckBox)sender).Checked;
        }
    }
}
