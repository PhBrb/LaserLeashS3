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
        Deserializer osciWriter;
        Series series1;
        Series series2;
        Series series3;
        UDPReceiver udpReceiver;
        private delegate void SafeCallDelegate();
        VerticalLineAnnotation VA;
        MQTTPublisher mqtt;
        bool lockMode = false;
        private double scanFreq = 1;
        private int samples = 400;
        private int averages = 400;
        bool update = true;

        public Form1(Deserializer osciWriter, UDPReceiver udpReceiver, MQTTPublisher mqtt)
        {
            InitializeComponent();
            this.osciWriter = osciWriter;
            this.udpReceiver = udpReceiver;
            InitGraph();
            this.mqtt = mqtt;
        }

        public void OnNewData()
        {
            if(!update) 
                return;


            if (chart1.InvokeRequired)
            {
                var d = new SafeCallDelegate(OnNewData);
                chart1.Invoke(d, new object[] {});
            }
            else
            {

                if (lockMode)
                {
                    double[] dataDac, dataAdc;
                    lock(osciWriter.osciData.dacQueue) lock(osciWriter.osciData.adcQueue)
                    {
                        dataDac = osciWriter.osciData.dacQueue.ToArray();
                        dataAdc = osciWriter.osciData.adcQueue.ToArray();
                        }


                    series2.Points.DataBindY(dataDac);
                    series3.Points.DataBindY(dataAdc);
                }
                {
                    (double[] xData, double[] yData) = getXYData(osciWriter.osciData.xyData, osciWriter.osciData.AvgSize);
                    if(xData.Length > 0)
                        series1.Points.DataBindXY(xData, yData);
                }


                chart1.Update();
            }
        }

        static (double[], double[]) getXYData(SortedDictionary<double, double[]> dict, int avgSize)
        {


            lock (dict)
            {
                double[] xData = new double[dict.Count];
                double[] yData = new double[dict.Count];
                int i = 0;
                foreach (var item in dict)
                {
                    xData[i] = item.Key;
                    yData[i] = item.Value.Sum()/avgSize;
                    i++;
                }
                return (xData, yData);
            }
        }

        public void InitGraph()
        {
            // create a series for each line
            series1 = new Series("Channel0");
            //(double[] xData, double[] yData) = getXYData(osciWriter.osciData.xyData);
            //series1.Points.DataBindXY(xData, yData);


            series1.ChartType = SeriesChartType.FastLine;
                series1.ChartType = SeriesChartType.FastLine;

            series2 = new Series("Output (right)");
            series2.ChartType = SeriesChartType.FastLine;
            series2.Enabled= false;
            series3 = new Series("Demodulated (left)");
            series3.ChartType = SeriesChartType.FastLine;
            series3.Enabled= false;


            // add each series to the chart
            chart1.Series.Clear();
            chart1.Series.Add(series1);
            chart1.Series.Add(series2);
            chart1.Series.Add(series3);

            chart1.Series[0].YAxisType = AxisType.Primary;

            chart1.Series[1].YAxisType = AxisType.Secondary;
            chart1.Series[2].YAxisType = AxisType.Primary;

            // additional styling
            chart1.ResetAutoValues();
            chart1.Titles.Clear();
            //chart1.Titles.Add($"Fast Line Plot ({pointCount:N0} points per series)");
            chart1.ChartAreas[0].AxisX.Title = "Voltage output";
            chart1.ChartAreas[0].AxisY.Title = "Demodulation Voltage";
            chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
            chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "0.000";

            chart1.ChartAreas[0].AxisY2.LineColor = Color.Transparent;
            chart1.ChartAreas[0].AxisY2.MajorGrid.Enabled = false;
            chart1.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
            chart1.ChartAreas[0].AxisY2.IsStartedFromZero= false;

            //zooming https://stackoverflow.com/questions/13584061/how-to-enable-zooming-in-microsoft-chart-control-by-using-mouse-wheel
            chart1.MouseWheel += chart1_MouseWheel;
            var CA = chart1.ChartAreas[0];
            //CA.AxisX.ScaleView.Zoomable = true;


            // the vertical line https://stackoverflow.com/questions/25801257/c-sharp-line-chart-how-to-create-vertical-line
            VA = new VerticalLineAnnotation();
            VA.AxisX = CA.AxisX;
            VA.AllowMoving = true;
            VA.IsInfinitive = true;
            VA.ClipToChartArea = CA.Name;
            VA.Name = "Lock Point";
            VA.LineColor = Color.Red;
            VA.LineWidth = 1;
            VA.X = 1;
            chart1.Annotations.Add(VA);
        }

        private void chart1_MouseWheel(object sender, MouseEventArgs e)
        {

            if (lockMode)
                return;

            var chart = (Chart)sender;
            var xAxis = chart.ChartAreas[0].AxisX;

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

                    var posXStart = xAxis.PixelPositionToValue(e.Location.X) - (xMax - xMin) / 3;
                    var posXFinish = xAxis.PixelPositionToValue(e.Location.X) + (xMax - xMin) / 3;

                    setRange(Math.Max(posXStart, 0), Math.Min(posXFinish, 10));
                }
            }
            catch { }
        }

        private void chart1_Click(object sender, EventArgs e)
        {
            var me = e as MouseEventArgs;
            VA.X = chart1.ChartAreas[0].AxisX.PixelPositionToValue(me.X);
        }

        private void LockButton_Click(object sender, EventArgs e)
        {
            mqtt.sendScanAmplitude(0);
            Thread.Sleep(200);
            mqtt.sendScanOffset(VA.X);
            Thread.Sleep(200);

            lockMode = true;
            series1.Enabled = false;
            series2.Enabled = true;
            series3.Enabled = true;

            double offset = 0;
            if (OffsetCompensationCheckbox.Checked)
            {

                double keyOfClosest = 9999999999999;
                lock (osciWriter.osciData.xyData)
                {
                    foreach (var entry in osciWriter.osciData.xyData)
                    {
                        if (Math.Abs(entry.Key - VA.X) < Math.Abs(keyOfClosest - VA.X))
                        {
                            offset = entry.Value.Sum()/entry.Value.Length;
                            keyOfClosest = entry.Key;
                        }
                    }
                }
            }

            Thread.Sleep(200);
            mqtt.sendPID(-offset);
        }

        private void UnlockButton_Click(object sender, EventArgs e)
        {
            mqtt.sendScanAmplitude(5);
            mqtt.sendScanOffset(5);
            lockMode = false;
            series1.Enabled = true;
            series2.Enabled = false;
            series3.Enabled = false;
            Task.Run(() =>
            {
                Thread.Sleep(100);
                mqtt.sendPIDOff();
            });
        }

        private void setRange(double min, double max)
        {
            mqtt.sendScanOffset((min + max) / 2);
            mqtt.sendScanAmplitude((max - min) / 2);

            Task.Run(() =>
            {
                Thread.Sleep(100);
                lock (osciWriter.osciData.xyData)
                {
                    osciWriter.osciData.resolution = (max - min) / samples;
                    osciWriter.osciData.AvgSize = averages;
                    osciWriter.osciData.resetXY();
                }

            });
        }

        private void chart1_DoubleClick(object sender, EventArgs e)
        {
            var me = e as MouseEventArgs;
            var chart = (Chart)sender;
            var xAxis = chart.ChartAreas[0].AxisX;
            var xMin = xAxis.ScaleView.ViewMinimum;
            var xMax = xAxis.ScaleView.ViewMaximum;

            var posXStart = xAxis.PixelPositionToValue(me.Location.X) - (xMax - xMin) / 3;
            var posXFinish = xAxis.PixelPositionToValue(me.Location.X) + (xMax - xMin) / 3;

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
                averages = int.Parse(txt);
                osciWriter.osciData.AvgSize = averages;
            }
        }

        private void SamplesInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string txt = ((System.Windows.Forms.TextBox)sender).Text;
                samples = int.Parse(txt);
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
            mqtt.sendPIDOff();
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
            update = !update;
        }
    }
}
