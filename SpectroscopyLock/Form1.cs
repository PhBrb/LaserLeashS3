using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Threading;
using System.Reflection;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Net.Mime.MediaTypeNames;
using MQTTnet.Client;
using MQTTnet.Samples.Client;
using System.Xml.Linq;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using System.Runtime.InteropServices.ComTypes;

namespace ChartTest2
{
    public partial class Form1 : Form
    {
        Series seriesXY;
        Series seriesOutput;
        Series seriesDemod;
        private delegate void SafeCallDelegate();
        VerticalLineAnnotation LockLineAnnotation;
        MQTTPublisher mqtt;
        bool lockMode = false;
        private double scanFreq = 1;
        OsciDisplay osciDisplay;
        Memory memory;
        private double dacMax;
        private double dacMin;
        private double adcMax;
        private double adcMin;
        private double xMax;
        private double xMin;

        public Form1(Memory memory, OsciDisplay osciDisplay, MQTTPublisher mqtt)
        {
            InitializeComponent();
            InitGraph();
            this.mqtt = mqtt;
            this.osciDisplay = osciDisplay;
            this.memory = memory;
        }

        public void OnNewDataXY()
        {
            if (chartXY.InvokeRequired)
            {
                var d = new SafeCallDelegate(OnNewDataXY);
                chartXY.Invoke(d, new object[] { });
            }
            else
            {
                if (!lockMode)
                {
                    osciDisplay.GetTimeSeries();
                    (double[] xData, double[] yData) = osciDisplay.GetXYNoUpdate();
                    if (xData.Length > 0)
                        seriesXY.Points.DataBindXY(xData, yData);

                    //keep x scale at maximum range
                    if (xData.Max() > xMax)
                    {
                        xMax = xData.Max();
                        chartXY.ChartAreas[0].AxisX.Maximum = xMax;
                    }
                    if (xData.Min() < xMin)
                    {
                        xMin = xData.Min();
                        chartXY.ChartAreas[0].AxisX.Minimum = xMin;
                    }
                }
                chartXY.Update();
            }
        }
        public void OnNewDataTimeSeries()
        {
            if (chartTimeseries.InvokeRequired)
            {
                var d = new SafeCallDelegate(OnNewDataTimeSeries);
                chartTimeseries.Invoke(d, new object[] { });
            }
            else
            {
                double[] dataDac, dataAdc;
                (dataAdc, dataDac) = osciDisplay.GetTimeSeries();

                seriesOutput.Points.DataBindY(dataDac);
                seriesDemod.Points.DataBindY(dataAdc);
                chartTimeseries.Update();

                //keep DAC&ADC y scale at its maximum range
                if (dataDac.Max() > dacMax)
                {
                    dacMax = dataDac.Max();
                    chartTimeseries.ChartAreas[0].AxisY.Maximum = dacMax;
                }
                if (dataDac.Min() < dacMin)
                {
                    dacMin = dataDac.Min();
                    chartTimeseries.ChartAreas[0].AxisY.Minimum = dacMin;
                }
                if (dataAdc.Max() > adcMax)
                {
                    adcMax = dataAdc.Max();
                    chartTimeseries.ChartAreas[0].AxisY.Maximum = adcMax;
                }
                if (dataAdc.Min() < adcMin)
                {
                    adcMin = dataAdc.Min();
                    chartTimeseries.ChartAreas[0].AxisY.Minimum = adcMin;
                }
            }
        }

        private void OnChartReload()
        {
            memory.Clear();
            dacMax = 0;
            dacMin = 0;
            chartTimeseries.ChartAreas[0].AxisY.Maximum = double.NaN;
            chartTimeseries.ChartAreas[0].AxisY.Minimum = double.NaN;
            adcMax = 0;
            adcMin = 0;
            chartTimeseries.ChartAreas[0].AxisY2.Maximum = double.NaN;
            chartTimeseries.ChartAreas[0].AxisY2.Minimum = double.NaN;
            xMax = 0;
            xMin = 0;
            chartXY.ChartAreas[0].AxisX.Maximum = double.NaN;
            chartXY.ChartAreas[0].AxisX.Minimum = double.NaN;
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
            seriesXY.ChartType = SeriesChartType.FastLine;
            seriesOutput = new Series("Output (left)");
            seriesOutput.ChartType = SeriesChartType.FastLine;
            seriesDemod = new Series("Demodulated (right)");
            seriesDemod.ChartType = SeriesChartType.FastLine;


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
            //chart1.Titles.Add($"Fast Line Plot ({pointCount:N0} points per series)");
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

            chartTimeseries.ChartAreas[0].AxisY2.LineColor = Color.Transparent;
            chartTimeseries.ChartAreas[0].AxisY2.MajorGrid.Enabled = false;
            chartTimeseries.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
            chartTimeseries.ChartAreas[0].AxisY2.IsStartedFromZero= false;

            //zooming https://stackoverflow.com/questions/13584061/how-to-enable-zooming-in-microsoft-chart-control-by-using-mouse-wheel
            chartXY.MouseWheel += chartXY_MouseWheel;
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

        private void chartXY_MouseWheel(object sender, MouseEventArgs e)
        {
            if (lockMode)
                return;

            var xAxis = chartXY.ChartAreas[0].AxisX;

            try
            {
                if (e.Delta < 0) // Scrolled down.
                {
                    setRange(0, 10);
                }
                else if (e.Delta > 0) // Scrolled up.
                {
                    var xMin = xAxis.ScaleView.ViewMinimum;
                    var xMax = xAxis.ScaleView.ViewMaximum;

                    var posXStart = xAxis.PixelPositionToValue(e.Location.X) - (xMax - xMin) / 4;
                    var posXFinish = xAxis.PixelPositionToValue(e.Location.X) + (xMax - xMin) / 4;

                    setRange(Math.Max(posXStart, 0), Math.Min(posXFinish, 10));
                }
            }
            catch { }
        }

        private void chart1_Click(object sender, EventArgs e)
        {
            var me = e as MouseEventArgs;
            LockLineAnnotation.X = chartXY.ChartAreas[0].AxisX.PixelPositionToValue(me.X);
        }

        private void LockButton_Click(object sender, EventArgs e)
        {
            mqtt.sendScanAmplitude(0);
            Thread.Sleep(200);
            mqtt.sendScanOffset(LockLineAnnotation.X);
            Thread.Sleep(200);

            lockMode = true;

            //keep previous range
            double min, max;
            min = osciDisplay.GetDACMinNoUpdate();
            max = osciDisplay.GetDACMaxNoUpdate();
            chartTimeseries.ChartAreas[0].AxisY.Maximum = max + 0.5 * (max - min);
            chartTimeseries.ChartAreas[0].AxisY.Minimum = min - 0.5 * (max - min);
            min = osciDisplay.GetADCMinNoUpdate();
            max = osciDisplay.GetADCMaxNoUpdate();
            chartTimeseries.ChartAreas[0].AxisY2.Maximum = max + 0.5 * (max - min);
            chartTimeseries.ChartAreas[0].AxisY2.Minimum = min - 0.5 * (max - min);

            Thread.Sleep(200);
            mqtt.sendPID(0, iirTextBox.Text, 0);
        }

        private void UnlockButton_Click(object sender, EventArgs e)
        {
            mqtt.sendPIDOff();
            setRange(0, 10);
            lockMode = false;
        }

        private void setRange(double min, double max)
        {
            mqtt.sendScanAmplitude((max - min) / 2);
            mqtt.sendScanOffset((min + max) / 2);

            Task.Run(() =>
            {
                Thread.Sleep(100);
                OnChartReload();
            });
        }

        private void chart1_DoubleClick(object sender, EventArgs e)
        {
            if (lockMode)
                return;
            var me = e as MouseEventArgs;
            var xAxis = chartXY.ChartAreas[0].AxisX;
            var xMin = xAxis.ScaleView.ViewMinimum;
            var xMax = xAxis.ScaleView.ViewMaximum;

            var posXStart = xAxis.PixelPositionToValue(me.Location.X) - (xMax - xMin) / 4;
            var posXFinish = xAxis.PixelPositionToValue(me.Location.X) + (xMax - xMin) / 4;

            setRange(Math.Max(posXStart, 0), Math.Min(posXFinish, 10));
        }

        private void ModulationAmplitudeInput_TextChanged(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string txt = ((System.Windows.Forms.TextBox)sender).Text;
                mqtt.sendModulationAmplitude(double.Parse(txt));
            }
        }

        private void ModulationAttenuationInput_TextChanged(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string txt = ((System.Windows.Forms.TextBox)sender).Text;
                mqtt.sendModulationAttenuation(double.Parse(txt));
            }
        }

        private void DemodulationAttenuationInput_TextChanged(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                System.Windows.Forms.TextBox txt = (System.Windows.Forms.TextBox)sender;
                mqtt.sendDemodulationAttenuation(double.Parse(txt.Text));
            }
        }

        private void PhaseInput_TextChanged(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string txt = ((System.Windows.Forms.TextBox)sender).Text;
                mqtt.sendPhase(double.Parse(txt));
            }
        }


        private void StreamTargetIPInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string txt = ((System.Windows.Forms.TextBox)sender).Text;
                mqtt.sendStreamTarget(txt.Replace('.', ','), StreamTargetIPInput.Text);
            }
        }

        private void StreamTargetPortInput_TextChanged(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string txt = ((System.Windows.Forms.TextBox)sender).Text;
                mqtt.sendStreamTarget(StreamTargetIPInput.Text.Replace('.', ','), txt);
            }
        }

        private void DemodulationAmplitudeInput_TextChanged(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string txt = ((System.Windows.Forms.TextBox)sender).Text;
                mqtt.sendDemodulationAmplitude(double.Parse(txt));
            }
        }

        private void DemodulationFrequencyInput_TextChanged(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string txt = ((System.Windows.Forms.TextBox)sender).Text;
                mqtt.sendDemodulationFrequency(double.Parse(txt));
            }
        }

        private void ModulationFrequencyInput_TextChanged(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string txt = ((System.Windows.Forms.TextBox)sender).Text;
                mqtt.sendModulationFrequency(double.Parse(txt));
            }
        }

        private void AveragesInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string txt = ((System.Windows.Forms.TextBox)sender).Text;
                int averages = int.Parse(txt);
                throw new NotImplementedException();
            }
        }

        private void SamplesInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string txt = ((System.Windows.Forms.TextBox)sender).Text;
                int samples = int.Parse(txt);
                throw new NotImplementedException();
            }
        }

        private void ScanFreqDownButton_Click(object sender, EventArgs e)
        {
            scanFreq /= 2;
            mqtt.sendScanFrequency(scanFreq);
        }

        private void ScanFreqUpButton_Click(object sender, EventArgs e)
        {
            scanFreq *= 2;
            mqtt.sendScanFrequency(scanFreq);
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
    }
}
