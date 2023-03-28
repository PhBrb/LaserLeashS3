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
            if (chart1.InvokeRequired)
            {
                var d = new SafeCallDelegate(OnNewData);
                chart1.Invoke(d, new object[] {});
            }
            else
            {

                if (lockMode)
                {
                    series2.Points.DataBindY(osciWriter.osciData.dac0Rolling);
                    series3.Points.DataBindY(osciWriter.osciData.adc0Rolling);
                }
                {
                    (double[] xData, double[] yData) = getXYData(osciWriter.osciData.xyData, osciWriter.osciData.avgSize);
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

            series2 = new Series("asd");
            series2.ChartType = SeriesChartType.FastLine;
            series2.Enabled= false;
            series3 = new Series("das");
            series3.ChartType = SeriesChartType.FastLine;
            series3.Enabled= false;

            chart1.Legends.Clear();

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
                    mqtt.sendScanOffset(5);
                    mqtt.sendScanAmplitude(5);

                    Task.Run(() =>
                    {
                        Thread.Sleep(100);
                        lock (osciWriter.osciData.xyData)
                        {
                            osciWriter.osciData.resolution = (10.0 - 0) / 400;
                            osciWriter.osciData.avgSize = 50;
                            osciWriter.osciData.resetXY();

                        }
                    });
                }
                else if (e.Delta > 0) // Scrolled up.
                {
                    var xMin = xAxis.ScaleView.ViewMinimum;
                    var xMax = xAxis.ScaleView.ViewMaximum;

                    var posXStart = xAxis.PixelPositionToValue(e.Location.X) - (xMax - xMin) / 3;
                    var posXFinish = xAxis.PixelPositionToValue(e.Location.X) + (xMax - xMin) / 3;

                    posXStart = Math.Max(posXStart, 0);
                    posXFinish = Math.Min(posXFinish, 10);


                    mqtt.sendScanOffset((posXStart + posXFinish) / 2);
                    mqtt.sendScanAmplitude((posXFinish - posXStart) / 2);

                    Task.Run(() =>
                    {
                        Thread.Sleep(100);
                        lock (osciWriter.osciData.xyData)
                        {
                            osciWriter.osciData.resolution = (posXFinish - posXStart) / 400;
                            osciWriter.osciData.avgSize = 50;
                            osciWriter.osciData.resetXY();
                        }

                    });
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
            mqtt.sendScanOffset(VA.X);
            mqtt.sendScanAmplitude(0);
            lockMode= true;
            series1.Enabled = false;
            series2.Enabled = true;
            series3.Enabled = true;
        }

        private void UnlockButton_Click(object sender, EventArgs e)
        {
            mqtt.sendScanOffset(5);
            mqtt.sendScanAmplitude(5);
            lockMode= false;
            series1.Enabled = true;
            series2.Enabled = false;
            series3.Enabled = false;
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

            posXStart = Math.Max(posXStart, 0);
            posXFinish = Math.Min(posXFinish, 10);


            mqtt.sendScanOffset((posXStart + posXFinish) / 2);
            mqtt.sendScanAmplitude((posXFinish - posXStart) / 2);

            Task.Run(() =>
            {
                Thread.Sleep(100);
                lock (osciWriter.osciData.xyData)
                {
                    osciWriter.osciData.resolution = (posXFinish - posXStart) / 400;
                    osciWriter.osciData.avgSize = 50;
                    osciWriter.osciData.resetXY();
                }
                
            });


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

        private void StreamTargetInput_TextChanged(object sender, KeyEventArgs e)
        {

        }

        private void StabilizerIDInput_TextChanged(object sender, KeyEventArgs e)
        {

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

        }

        private void SamplesInput_KeyDown(object sender, KeyEventArgs e)
        {

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
    }
}
