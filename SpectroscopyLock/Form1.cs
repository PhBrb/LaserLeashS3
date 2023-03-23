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

namespace ChartTest2
{
    public partial class Form1 : Form
    {
        Deserializer osciWriter;
        Series series1;
        Series series2;
        UDPReceiver udpReceiver;
        private delegate void SafeCallDelegate();
        VerticalLineAnnotation VA;
        MQTTPublisher mqtt;
        bool lockMode = false;

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
                    series1.Points.DataBindY(osciWriter.osciData.dac0Rolling);
                    series2.Points.DataBindY(osciWriter.osciData.adc0Rolling);
                }
                {
                    (double[] xData, double[] yData) = getXYData(osciWriter.osciData.xyData);
                    series1.Points.DataBindXY(xData, yData);
                }


                chart1.Update();
            }
        }

        static (double[], double[]) getXYData(Dictionary<double, double> dict)
        {
            double[] xData = new double[dict.Count];
            double[] yData = new double[dict.Count];

            int i = 0;
            foreach(var item in dict)
            {
                xData[i] = item.Key;
                yData[i] = item.Value;
                i++;
            }
            return (xData, yData);
        }

        public void InitGraph()
        {
            // create a series for each line
            series1 = new Series("Channel0");
            (double[] xData, double[] yData) = getXYData(osciWriter.osciData.xyData);
            series1.Points.DataBindXY(xData, yData);
            series1.ChartType = SeriesChartType.FastLine;


            series2 = new Series("asd");
            series2.Points.DataBindY(osciWriter.osciData.adc0Rolling);
            series2.ChartType = SeriesChartType.FastLine;


            // add each series to the chart
            chart1.Series.Clear();
            chart1.Series.Add(series1);
            chart1.Series.Add(series2);

            chart1.Series[0].YAxisType = AxisType.Primary;
            chart1.Series[1].YAxisType = AxisType.Secondary;

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
            VA.LineWidth = 2;         // use your numbers!
            VA.X = 1;
            chart1.Annotations.Add(VA);
        }

        private void chart1_MouseWheel(object sender, MouseEventArgs e)
        {
            var chart = (Chart)sender;
            var xAxis = chart.ChartAreas[0].AxisX;

            try
            {
                if (e.Delta < 0) // Scrolled down.
                {
                    //xAxis.ScaleView.ZoomReset();
                    mqtt.sendOffset(5);
                    mqtt.sendAmplitude(5);
                }
                else if (e.Delta > 0) // Scrolled up.
                {
                    var xMin = xAxis.ScaleView.ViewMinimum;
                    var xMax = xAxis.ScaleView.ViewMaximum;

                    var posXStart = xAxis.PixelPositionToValue(e.Location.X) - (xMax - xMin) / 4;
                    var posXFinish = xAxis.PixelPositionToValue(e.Location.X) + (xMax - xMin) / 4;

                    posXStart = Math.Max(posXStart, 0);
                    posXFinish = Math.Min(posXFinish, 10);

                    mqtt.sendOffset((posXStart + posXFinish) / 2);
                    mqtt.sendAmplitude((posXFinish - posXStart) / 2);


                    //xAxis.ScaleView.Zoom(posXStart, posXFinish);

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
            mqtt.sendOffset(VA.X);
            mqtt.sendAmplitude(0);
            lockMode= true;
            OnNewData();
            chart1.ChartAreas[0].AxisX.ScaleView.ZoomReset();
        }

        private void UnlockButton_Click(object sender, EventArgs e)
        {
            mqtt.sendOffset(5);
            mqtt.sendAmplitude(5);
            lockMode= false;
            OnNewData();
            chart1.ChartAreas[0].AxisX.ScaleView.ZoomReset();
        }
    }
}
