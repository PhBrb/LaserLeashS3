using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace ChartTest2
{
    /// <summary>
    /// Contains logic for loading, saving and forwarding changed values. Generic actions are collected in a map, custom logic is handled individually.
    /// </summary>
    partial class SpectrscopyControlForm
    {
        Dictionary<NumericUpDown, Action<double>> OnValueDoubleChangeMap;
        Dictionary<NumericUpDown, Action<double>> OnValueDoubleSaveMap;
        Dictionary<NumericUpDown, Action<int>> OnValueIntChangeMap;
        Dictionary<NumericUpDown, Action<int>> OnValueIntSaveMap;

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
                {MemorySizeText, (duration) =>  {
                                                    int oldestSample = UnitConvert.TimeToSample(duration);
                                                    osciDisplay.oldestSampleToDisplay = Math.Min(oldestSample, osciDisplay.oldestSampleToDisplay);
                                                    memory.setSize(oldestSample);
                                                    osciDisplay.ZoomOut();
                                                }},
                {XYSmoothing, osciDisplay.setXYSmoothing }
            };
            //Actions to perform when int inputs are changed
            OnValueIntChangeMap = new Dictionary<NumericUpDown, Action<int>>(){
                {AveragesText, osciDisplay.setAverages},
                {samplesOnDisplayText, osciDisplay.setSize },
                {ChannelInput,  (value) => { } },
                {StreamTargetPortInput, (value) => {} }
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
                {XYSmoothing, (value) => Properties.Settings.Default.XYSmoothing = value }
            };
            //Where to store changed int values
            OnValueIntSaveMap = new Dictionary<NumericUpDown, Action<int>>(){
                {AveragesText, (value) => Properties.Settings.Default.Averages = value},
                {samplesOnDisplayText, (value) => Properties.Settings.Default.DisplayResolution = value},
                {ChannelInput, (value) => Properties.Settings.Default.Channel = value },
                {StreamTargetPortInput, (value) => Properties.Settings.Default.Port = value }
            };
        }

        private void NumberFieldDouble_ValueChanged(object sender, EventArgs e)
        {
            decimal dec = ((NumericUpDown)sender).Value;
            OnValueDoubleSaveMap[(NumericUpDown)sender].Invoke(Decimal.ToDouble(dec));
            Properties.Settings.Default.Save();
            OnValueDoubleChangeMap[(NumericUpDown)sender].Invoke(Decimal.ToDouble(dec));
        }

        private void NumberFieldInt_ValueChanged(object sender, EventArgs e)
        {
            decimal dec = ((NumericUpDown)sender).Value;
            OnValueIntSaveMap[(NumericUpDown)sender].Invoke(Decimal.ToInt32(dec));
            Properties.Settings.Default.Save();
            OnValueIntChangeMap[(NumericUpDown)sender].Invoke(Decimal.ToInt32(dec));
        }


        private void IPTarget_TextChanged(object sender, EventArgs e)
        {
            var regex = "^((25[0-5]|(2[0-4]|1\\d|[1-9]|)\\d)\\.?\\b){4}$";//https://stackoverflow.com/questions/5284147/validating-ipv4-addresses-with-regexp
            var match = Regex.Match(StreamTargetIPInput.Text, regex);
            if (match.Success)
            {
                Properties.Settings.Default.IP = StreamTargetIPInput.Text;
                Properties.Settings.Default.Save();
            } else
            {
                WriteLine("Bad Target IP format");
            }
        }

        private void StabilizerIDInput_KeyDown(object sender, KeyEventArgs e)
        {
            MaskedTextBox tbSource = (MaskedTextBox)sender;
            if (tbSource.MaskCompleted)
            {
                Properties.Settings.Default.ID = StabilizerIDInput.Text;
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
            //FGAmplitudeText.Value = ;//TODO properly implement function generator
            //FGFrequencyText.Value = ;
            MemorySizeText.Value = (decimal)Properties.Settings.Default.MemorySize;
            XYSmoothing.Value = (decimal)Properties.Settings.Default.XYSmoothing;
            AveragesText.Value = Properties.Settings.Default.Averages;
            samplesOnDisplayText.Value = Properties.Settings.Default.DisplayResolution;
            StreamTargetIPInput.Text = Properties.Settings.Default.IP;
            StreamTargetPortInput.Value = Properties.Settings.Default.Port;
            ChannelInput.Value = Properties.Settings.Default.Channel;
        }

        private void PID_ValueChanged(object sender, EventArgs e)
        {
            double p = Decimal.ToDouble(KpText.Value),
                i = Decimal.ToDouble(KiText.Value),
                d = Decimal.ToDouble(KdText.Value),
                sampleRate = Decimal.ToDouble(SamplerateText.Value),
                yMin = Decimal.ToDouble(YminText.Value),
                yMax = Decimal.ToDouble(YmaxText.Value);

            Properties.Settings.Default.P = p;
            Properties.Settings.Default.I = i;
            Properties.Settings.Default.D = d;
            Properties.Settings.Default.SampleRate = sampleRate;
            Properties.Settings.Default.yMin = yMin;
            Properties.Settings.Default.yMax = yMax;
            Properties.Settings.Default.Save();

            if (!lockMode)
                return;

            List<double> iirParameters = UnitConvert.CalculateIIR(p, i, d, sampleRate);
            mqtt.sendPID(0, iirParameters.ToBracketString(), yMin, yMax);
        }
    }
}
