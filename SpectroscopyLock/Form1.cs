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
                series1.Points.DataBindXY(osciWriter.osciData.dac0, osciWriter.osciData.adc0);
                chart1.Update();
            }
        }


        public void InitGraph()
        {
            // create a series for each line
            series1 = new Series("adc0");
            series1.Points.DataBindXY(osciWriter.osciData.dac0, osciWriter.osciData.adc0);
            series1.ChartType = SeriesChartType.FastLine;

            //series2 = new Series("dac0");
            //series2.Points.DataBindY(osciWriter.osciData.dac0);
            //series2.ChartType = SeriesChartType.FastLine;

            // add each series to the chart
            chart1.Series.Clear();
            chart1.Series.Add(series1);
            //chart1.Series.Add(series2);

            // additional styling
            chart1.ResetAutoValues();
            chart1.Titles.Clear();
            //chart1.Titles.Add($"Fast Line Plot ({pointCount:N0} points per series)");
            chart1.ChartAreas[0].AxisX.Title = "Horizontal Axis Label";
            chart1.ChartAreas[0].AxisY.Title = "Vertical Axis Label";
            chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
            chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

    }
}
