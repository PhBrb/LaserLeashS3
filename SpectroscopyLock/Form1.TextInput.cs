using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace ChartTest2
{
    /// <summary>
    /// Contains logic handling user input; loading, saving and forwarding changed values. Generic actions are collected in a map, more complex logic is handled individually.
    /// </summary>
    partial class SpectroscopyControlForm
    {
        Dictionary<NumericUpDown, Action<double>> OnValueDoubleChangeMap;
        Dictionary<NumericUpDown, Action<double>> OnValueDoubleSaveMap;
        Dictionary<NumericUpDown, Action<int>> OnValueIntChangeMap;
        Dictionary<NumericUpDown, Action<int>> OnValueIntSaveMap;

        private int requestedMemorySize;
        Thread memoryResizeThread;
        DateTime resizeAt;

        private void InitSettings()
        {
            //Actions to perform when double inputs are changed
            OnValueDoubleChangeMap = new Dictionary<NumericUpDown, Action<double>>(){
                {modFreqText, mqtt.sendModulationFrequencyMHz},
                {demodFreqText, mqtt.sendDemodulationFrequencyMHz},
                {demodAmpText, mqtt.sendDemodulationAmplitude},
                {modAmpText, mqtt.sendModulationAmplitude},
                {modAttText, mqtt.sendModulationAttenuation},
                {demodAttText, mqtt.sendDemodulationAttenuation},
                {modPhaseText, mqtt.sendPhase},
                {FGAmplitudeText, (amplitude) => {previousAmplitude = amplitude; mqtt.sendScanAmplitude(amplitude); } },
                {FGFrequencyText, mqtt.sendScanFrequency},
                {MemorySizeText, MemoryResize},
                {XYSmoothing, osciDisplay.setXYSmoothing },
                {KpText, PID_ValueChanged},
                {KiText, PID_ValueChanged},
                {KdText, PID_ValueChanged},
                {YminText, PID_ValueChanged},
                {YmaxText, PID_ValueChanged},
                {SamplerateText, PID_ValueChanged}
            };
            //Actions to perform when int inputs are changed
            OnValueIntChangeMap = new Dictionary<NumericUpDown, Action<int>>(){
                {AveragesText, osciDisplay.setAverages},
                {samplesOnDisplayText, osciDisplay.setSize},
                {ChannelInput,  (value) => { } },
                {StreamTargetPortInput, (value) => {
                                                        mqtt.sendStreamTarget(Properties.Settings.Default.StreamIP, value.ToString());
                                                        Program.udpReceiver.port = value;
                                                   }}
            };

            //Where to store changed doulbe values
            OnValueDoubleSaveMap = new Dictionary<NumericUpDown, Action<double>>
            {
                {modFreqText, (value) => Properties.Settings.Default.ModFreq = value},
                {demodFreqText, (value) => Properties.Settings.Default.DemodFreq = value},
                {demodAmpText, (value) => Properties.Settings.Default.DemodAmp = value},
                {modAmpText, (value) => Properties.Settings.Default.ModAmp = value},
                {modAttText, (value) => Properties.Settings.Default.ModAtt = value},
                {demodAttText, (value) => Properties.Settings.Default.DemodAtt = value},
                {modPhaseText, (value) => Properties.Settings.Default.Phase = value},
                {FGAmplitudeText, (value) => { }}, //TODO properly implement function generator
                {FGFrequencyText, (value) => { }},
                {MemorySizeText, (value) => Properties.Settings.Default.MemorySize = value},
                {XYSmoothing, (value) => Properties.Settings.Default.XYSmoothing = value },
                {KpText,  (value) => Properties.Settings.Default.P = value},
                {KiText,  (value) => Properties.Settings.Default.I = value},
                {KdText,  (value) => Properties.Settings.Default.D = value},
                {YminText,  (value) => Properties.Settings.Default.yMin = value},
                {YmaxText,  (value) => Properties.Settings.Default.yMax = value},
                {SamplerateText,  (value) => Properties.Settings.Default.SampleRate = value}
            };
            //Where to store changed int values
            OnValueIntSaveMap = new Dictionary<NumericUpDown, Action<int>>(){
                {AveragesText, (value) => Properties.Settings.Default.Averages = value},
                {samplesOnDisplayText, (value) => Properties.Settings.Default.DisplayResolution = value},
                {ChannelInput, (value) => Properties.Settings.Default.Channel = value },
                {StreamTargetPortInput, (value) => Properties.Settings.Default.StreamPort = value }
            };
        }

        /// <summary>
        /// Schedules a resize of the memory. The resize is delayed to not stress the GC if the user keeps pressing the resize button.
        /// </summary>
        /// <param name="duration"></param>
        private void MemoryResize(double duration)
        {
            requestedMemorySize = UnitConvert.TimeToSample(duration);
            resizeAt = DateTime.Now.AddMilliseconds(200);

            if (memoryResizeThread == null || !memoryResizeThread.IsAlive)
            {
                resizeAt = DateTime.Now.AddMilliseconds(400); //the first time that a NumericUpDown sends an update, the delay between updates is a bit longer 
                memoryResizeThread = new Thread(new ThreadStart(() =>
                {
                    while(resizeAt > DateTime.Now)
                    {
                        Thread.Sleep(300);
                    }
                    if (requestedMemorySize != memory.getSize())
                    {
                        memory.setSize(requestedMemorySize);
                        osciDisplay.ZoomReset();
                    }
                }));
                memoryResizeThread.Start();
            }
        }

        private void NumberFieldDouble_ValueChanged(object sender, EventArgs e)
        {
            if (!receiveEvents)
                return;
            decimal dec = ((NumericUpDown)sender).Value;
            OnValueDoubleSaveMap[(NumericUpDown)sender].Invoke(Decimal.ToDouble(dec));
            Properties.Settings.Default.Save();
            OnValueDoubleChangeMap[(NumericUpDown)sender].Invoke(Decimal.ToDouble(dec));
        }

        private void NumberFieldInt_ValueChanged(object sender, EventArgs e)
        {
            if (!receiveEvents)
                return;
            decimal dec = ((NumericUpDown)sender).Value;
            OnValueIntSaveMap[(NumericUpDown)sender].Invoke(Decimal.ToInt32(dec));
            Properties.Settings.Default.Save();
            OnValueIntChangeMap[(NumericUpDown)sender].Invoke(Decimal.ToInt32(dec));
        }

        private const string IPRegEx = "^((25[0-5]|(2[0-4]|1\\d|[1-9]|)\\d)\\.?\\b){4}$";//https://stackoverflow.com/questions/5284147/validating-ipv4-addresses-with-regexp
        private void StreamTargetIP_TextChanged(object sender, EventArgs e)
        {
            if (!receiveEvents)
                return;
            Match match = Regex.Match(StreamTargetIPInput.Text, IPRegEx);
            if (match.Success)
            {
                Properties.Settings.Default.StreamIP = StreamTargetIPInput.Text;
                Properties.Settings.Default.Save();
                mqtt.sendStreamTarget(StreamTargetIPInput.Text, Properties.Settings.Default.StreamPort.ToString());
                StreamTargetIPInput.ForeColor = System.Drawing.SystemColors.WindowText;
            } else
            {
                WriteLine("Bad Target IP format");
                StreamTargetIPInput.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void MQTTServer_TextChanged(object sender, EventArgs e)
        {
            if (!receiveEvents)
                return;
            var match = Regex.Match(MQTTServer.Text, IPRegEx);
            if (match.Success || MQTTServer.Text == "localhost")
            {
                Properties.Settings.Default.MQTTServer = MQTTServer.Text;
                Properties.Settings.Default.Save();
                mqtt.disconnect();
                mqtt = new MQTTPublisher(MQTTServer.Text, Properties.Settings.Default.MQTTPort);
                MQTTServer.ForeColor = System.Drawing.SystemColors.WindowText;
            }
            else
            {
                WriteLine("Bad Target IP format");
                MQTTServer.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void StabilizerIDInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (!receiveEvents)
                return;
            MaskedTextBox tbSource = (MaskedTextBox)sender;
            if (tbSource.MaskCompleted)
            {
                Properties.Settings.Default.StabilizerID = StabilizerIDInput.Text;
                Properties.Settings.Default.Save();
            } else
            {
                WriteLine("Bad Stabilizer ID format");
            }
        }

        public void LoadSettings()
        {
            modFreqText.Value = (decimal)Properties.Settings.Default.ModFreq;
            demodFreqText.Value = (decimal)Properties.Settings.Default.DemodFreq;
            demodAmpText.Value = (decimal)Properties.Settings.Default.DemodAmp;
            modAmpText.Value = (decimal)Properties.Settings.Default.ModAmp;
            modAttText.Value = (decimal)Properties.Settings.Default.ModAtt;
            demodAttText.Value = (decimal)Properties.Settings.Default.DemodAtt;
            modPhaseText.Value = (decimal)Properties.Settings.Default.Phase;

            KpText.Value = (decimal)Properties.Settings.Default.P;
            KiText.Value = (decimal)Properties.Settings.Default.I;
            KdText.Value = (decimal)Properties.Settings.Default.D;
            YminText.Value = (decimal)Properties.Settings.Default.yMin;
            YmaxText.Value = (decimal)Properties.Settings.Default.yMax;
            SamplerateText.Value = (decimal)Properties.Settings.Default.SampleRate;

            MQTTServer.Text = Properties.Settings.Default.MQTTServer;
            StreamTargetIPInput.Text = Properties.Settings.Default.StreamIP;
            StreamTargetPortInput.Value = Properties.Settings.Default.StreamPort;
            StabilizerIDInput.Text = Properties.Settings.Default.StabilizerID;
            ChannelInput.Value = Properties.Settings.Default.Channel;

            //FGAmplitudeText.Value = ;//TODO properly implement function generator
            //FGFrequencyText.Value = ;
            MemorySizeText.Value = (decimal)Properties.Settings.Default.MemorySize;
            XYSmoothing.Value = (decimal)Properties.Settings.Default.XYSmoothing;
            AveragesText.Value = Properties.Settings.Default.Averages;
            samplesOnDisplayText.Value = Properties.Settings.Default.DisplayResolution;

            radioButton1.Checked = true;

            receiveEvents = true;
        }

        private void PID_ValueChanged(double dummy)
        {
            if (!(lockMode & receiveEvents))
                return;
            double p = Decimal.ToDouble(KpText.Value);
            double i = Decimal.ToDouble(KiText.Value);
            double d = Decimal.ToDouble(KdText.Value);
            double sampleRate = Decimal.ToDouble(SamplerateText.Value);
            double yMin = Decimal.ToDouble(YminText.Value);
            double yMax = Decimal.ToDouble(YmaxText.Value);

            List<double> iirParameters = UnitConvert.CalculateIIR(p, i, d, sampleRate);
            mqtt.sendPID(0, iirParameters.ToBracketString(), yMin, yMax);
        }

        /// <summary>
        /// Workaround to trigger validation of NumericUpDowns
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Unused, needed by ValueChanged event</param>
        private void Refocus(object sender, EventArgs e)
        {
            this.ActiveControl = null;
            ((NumericUpDown)sender).Focus();
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

        /// <summary>
        /// Checks if the display is requiring more datapoints than available
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValidateMemorySize(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int samplesOnDisplay = decimal.ToInt32(samplesOnDisplayText.Value);
            int averages = decimal.ToInt32(AveragesText.Value);
            int samples = sender == MemorySizeText ?//memory size change triggers zooming out
                UnitConvert.TimeToSample(Decimal.ToDouble(MemorySizeText.Value)) :
                osciDisplay.oldestSampleToDisplay - osciDisplay.newestSampleToDisplay + 1;
            if (!OsciDisplay.enoughSamples(samplesOnDisplay, averages, samples))
            {
                e.Cancel = true;
                string msg = "Available memory has to be >= display resolution * (averages +1)";
                if (sender == MemorySizeText)
                {
                    msg = "Can't reduce memory size. Too high display resolution or too much averaging." + msg;
                }
                else
                if (sender == AveragesText)
                {
                    msg = "Can't increase averaging. Too high display resolution, memory too small or zoomed in too far." + msg;
                }
                else
                if (sender == samplesOnDisplayText)
                {
                    msg = "Can't increase display resolution. Too much averaging or memory too small, or zoomed in too far." + msg;
                }
                WriteLine(msg);
            }
        }
    }
}
