﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Threading;
using MQTTnet.Samples.Client;

namespace ChartTest2
{
    public partial class SpectrscopyControlForm : Form
    {
        Series seriesXY;
        Series seriesOutput;
        Series seriesDemod;
        private delegate void SafeCallDelegate();
        VerticalLineAnnotation LockLineAnnotation;
        MQTTPublisher mqtt;
        bool lockMode = false;
        OsciDisplay osciDisplay;
        Memory memory;

        Dictionary<NumericUpDown, Action<double>> OnValueDoubleMap;
        Dictionary<NumericUpDown, Action<int>> OnValueIntMap;

        public static SpectrscopyControlForm form;

        public SpectrscopyControlForm(Memory memory, OsciDisplay osciDisplay, MQTTPublisher mqtt)
        {
            InitializeComponent();
            InitGraph();
            this.mqtt = mqtt;
            this.osciDisplay = osciDisplay;
            this.memory = memory;
            OnValueDoubleMap = new Dictionary<NumericUpDown, Action<double>>(){
                {modFreqText, mqtt.sendModulationFrequency},
                {demodFreqText, mqtt.sendDemodulationFrequency},
                {demodAmpText, mqtt.sendDemodulationAmplitude},
                {modAmpText, mqtt.sendModulationAmplitude},
                {modAttText, mqtt.sendModulationAttenuation},
                {demodAttText, mqtt.sendDemodulationAttenuation},
                {modPhaseText, mqtt.sendPhase},
                {FGAmplitudeText, mqtt.sendScanAmplitude},
                {FGFrequencyText, mqtt.sendScanFrequency},
            };
            OnValueIntMap = new Dictionary<NumericUpDown, Action<int>>(){
                {AveragesText, osciDisplay.setAverages},
                {MemorySizeText, (size) => {osciDisplay.oldestSampleToDisplay = Math.Min(size, osciDisplay.oldestSampleToDisplay); memory.setSize(size); } },
            };
            SpectrscopyControlForm.form = this;
        }

        public void OnNewDataXY()
        {
            if (chartXY.InvokeRequired)
            {
                chartXY.Invoke(new SafeCallDelegate(OnNewDataXY), new object[] { });
                return;
            }
            else
            {
                if (!lockMode)
                {
                    osciDisplay.GetTimeSeries();
                    (double[] xData, double[] yData) = osciDisplay.GetXYNoUpdate();
                    if (xData.Length > 0)
                        seriesXY.Points.DataBindXY(xData, yData);
                    chartXY.Update();
                }
            }
        }
        public void OnNewDataTimeSeries()
        {
            if (chartTimeseries.InvokeRequired)
            {
                chartTimeseries.Invoke(new SafeCallDelegate(OnNewDataTimeSeries), new object[] { });
                return;
            }
            else
            {
                double[] dataDac, dataAdc;
                (dataAdc, dataDac) = osciDisplay.GetTimeSeries();

                seriesOutput.Points.DataBindY(dataDac);
                seriesDemod.Points.DataBindY(dataAdc);

                chartTimeseries.Update();
            }
        }


        public void OnNewData()
        {
            OnNewDataTimeSeries();
            OnNewDataXY();
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
            chartTimeseries.ChartAreas[0].AxisX.Title = "Time (Samples)";
            chartTimeseries.ChartAreas[0].AxisY2.Title = "Demodulation Voltage";
            chartTimeseries.ChartAreas[0].AxisY.Title = "Voltage output";
            chartXY.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
            chartXY.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
            chartTimeseries.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
            chartTimeseries.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
            chartXY.ChartAreas[0].AxisX.LabelStyle.Format = "0.000";
            chartXY.ChartAreas[0].AxisY.LabelStyle.Format = "0.0000";
            chartTimeseries.ChartAreas[0].AxisX.LabelStyle.Format = "{0:#}";
            chartTimeseries.ChartAreas[0].AxisY.LabelStyle.Format = "0.0000";
            chartTimeseries.ChartAreas[0].AxisY2.LabelStyle.Format = "0.0000";
            chartTimeseries.ChartAreas[0].AxisY.IsStartedFromZero = false;

            chartTimeseries.ChartAreas[0].AxisY2.LineColor = Color.Transparent;
            chartTimeseries.ChartAreas[0].AxisY2.MajorGrid.Enabled = false;
            chartTimeseries.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;

            //zooming https://stackoverflow.com/questions/13584061/how-to-enable-zooming-in-microsoft-chart-control-by-using-mouse-wheel
            var CA = chartXY.ChartAreas[0];


            // the vertical line https://stackoverflow.com/questions/25801257/c-sharp-line-chart-how-to-create-vertical-line
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
        }

        private void chart1_Click(object sender, EventArgs e)
        {
            var me = e as MouseEventArgs;
            LockLineAnnotation.X = chartXY.ChartAreas[0].AxisX.PixelPositionToValue(me.X);
        }

        private void LockButton_Click(object sender, EventArgs e)
        {
            mqtt.sendScanAmplitude(0);
            Thread.Sleep(100);
            mqtt.sendScanOffset(LockLineAnnotation.X);
            Thread.Sleep(100);

            lockMode = true;

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

            int order = 1;
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

            List<double> result = b.ToList();
            result.AddRange(a.Skip(1));

            return result;
        }

        private void UnlockButton_Click(object sender, EventArgs e)
        {
            mqtt.sendPIDOff();
            setRange(0, 10);

            chartTimeseries.ChartAreas[0].AxisY.Maximum = double.NaN;
            chartTimeseries.ChartAreas[0].AxisY.Minimum = double.NaN;
            chartTimeseries.ChartAreas[0].AxisY2.Maximum = double.NaN;
            chartTimeseries.ChartAreas[0].AxisY2.Minimum = double.NaN;

            lockMode = false;
        }

        private void setRange(double min, double max)
        {
            mqtt.sendScanAmplitude((max - min) / 2);
            mqtt.sendScanOffset((min + max) / 2);
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

                setRange(Math.Max(posXStart, 0), Math.Min(posXFinish, 10));
            }
            else if(me.Button == MouseButtons.Right)
            {
                var xAxis = chartXY.ChartAreas[0].AxisX;
                var xMin = xAxis.ScaleView.ViewMinimum;
                var xMax = xAxis.ScaleView.ViewMaximum;

                var posXStart = (xMax + xMin)/2 - (xMax - xMin) / 1.3;
                var posXFinish = (xMax + xMin) / 2 + (xMax - xMin) / 1.3;

                setRange(Math.Max(posXStart, 0), Math.Min(posXFinish, 10));
            }
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

        private void StreamTargetPortInput_TextChanged(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string txt = ((System.Windows.Forms.TextBox)sender).Text;
                mqtt.sendStreamTarget(StreamTargetIPInput.Text, StreamTargetPortInput.Text);
            }
        }

        private void StabilizerIDInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string txt = ((System.Windows.Forms.TextBox)sender).Text;
                mqtt.setStabilizerID(txt);
            }
        }

        private void InitButton_Click(object sender, EventArgs e)
        {
            mqtt.sendStreamTarget(StreamTargetIPInput.Text.Replace('.', ','), StreamTargetPortInput.Text);
            mqtt.sendModulationAmplitude(1);
            mqtt.sendModulationAttenuation(0);
            mqtt.sendModulationFrequency  (3000000);
            mqtt.sendDemodulationFrequency(3000000);
            mqtt.sendDemodulationAttenuation(0);
            mqtt.sendDemodulationAmplitude(1);
            mqtt.sendPhase(0);
            mqtt.sendScanAmplitude(5);
            mqtt.sendScanOffset(5);
            mqtt.sendScanFrequency(1);
        }

        private void HoldButton_Click(object sender, EventArgs e)
        {
            memory.freeze = !memory.freeze;
        }

        private void NumberFieldDouble_ValueChanged(object sender, EventArgs e)
        {
            decimal dec = ((NumericUpDown)sender).Value;
            OnValueDoubleMap[(NumericUpDown)sender].Invoke(Decimal.ToDouble(dec));
        }

        private void NumberFieldInt_ValueChanged(object sender, EventArgs e)
        {
            decimal dec = ((NumericUpDown)sender).Value;
            OnValueIntMap[(NumericUpDown)sender].Invoke(Decimal.ToInt32(dec));
        }

        private void PID_ValueChanged(object sender, EventArgs e)
        {
            List<double> result = CalculateIIR(Decimal.ToDouble(KpText.Value), Decimal.ToDouble(KiText.Value), Decimal.ToDouble(KdText.Value), Decimal.ToDouble(SamplerateText.Value));
            mqtt.sendPID(0, result.ToBracketString(), Decimal.ToDouble(YminText.Value), Decimal.ToDouble(YmaxText.Value));
        }

        public static void WriteLine(string message)
        {
            form.logText.AppendText("\r\n" + message);
        }
    }
}
