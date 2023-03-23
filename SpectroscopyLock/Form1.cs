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

namespace ChartTest2
{
    public partial class Form1 : Form
    {
        Deserializer osciWriter;
        Series series1;
        Series series2;
        UDPReceiver udpReceiver;
        private delegate void SafeCallDelegate();

        public Form1(Deserializer osciWriter, UDPReceiver udpReceiver)
        {
            InitializeComponent();
            this.osciWriter = osciWriter;
            this.udpReceiver = udpReceiver;
            InitGraph();
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
                (double[] xData, double[] yData) = getXYData(osciWriter.osciData.xyData);
                series1.Points.DataBindXY(xData, yData);
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


            // add each series to the chart
            chart1.Series.Clear();
            chart1.Series.Add(series1);

            // additional styling
            chart1.ResetAutoValues();
            chart1.Titles.Clear();
            //chart1.Titles.Add($"Fast Line Plot ({pointCount:N0} points per series)");
            chart1.ChartAreas[0].AxisX.Title = "Voltage output";
            chart1.ChartAreas[0].AxisY.Title = "Demodulation Voltage";
            chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
            chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;

            chart1.MouseWheel += chart1_MouseWheel;
            chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;

        }

        private void chart1_MouseWheel(object sender, MouseEventArgs e)
        {
            var chart = (Chart)sender;
            var xAxis = chart.ChartAreas[0].AxisX;

            try
            {
                if (e.Delta < 0) // Scrolled down.
                {
                    xAxis.ScaleView.ZoomReset();
                }
                else if (e.Delta > 0) // Scrolled up.
                {
                    var xMin = xAxis.ScaleView.ViewMinimum;
                    var xMax = xAxis.ScaleView.ViewMaximum;

                    var posXStart = xAxis.PixelPositionToValue(e.Location.X) - (xMax - xMin) / 4;
                    var posXFinish = xAxis.PixelPositionToValue(e.Location.X) + (xMax - xMin) / 4;

                    xAxis.ScaleView.Zoom(posXStart, posXFinish);
                }
            }
            catch { }
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

    }
}
