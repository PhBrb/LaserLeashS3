namespace ChartTest2
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend3 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea4 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend4 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.chartXY = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.LockButton = new System.Windows.Forms.Button();
            this.UnlockButton = new System.Windows.Forms.Button();
            this.ModulationAmplitudeInput = new System.Windows.Forms.TextBox();
            this.ModulationAttenuationInput = new System.Windows.Forms.TextBox();
            this.DemodulationAmplitudeInput = new System.Windows.Forms.TextBox();
            this.DemodulationAttenuationInput = new System.Windows.Forms.TextBox();
            this.ModulationFrequencyInput = new System.Windows.Forms.TextBox();
            this.PhaseInput = new System.Windows.Forms.TextBox();
            this.StreamTargetPortInput = new System.Windows.Forms.TextBox();
            this.ScanFreqDownButton = new System.Windows.Forms.Button();
            this.ScanFreqUpButton = new System.Windows.Forms.Button();
            this.DemodulationFrequencyInput = new System.Windows.Forms.TextBox();
            this.AveragesInput = new System.Windows.Forms.TextBox();
            this.SamplesMemoryInput = new System.Windows.Forms.TextBox();
            this.InitButton = new System.Windows.Forms.Button();
            this.StreamTargetIPInput = new System.Windows.Forms.TextBox();
            this.StabilizerIDInput = new System.Windows.Forms.TextBox();
            this.HoldButton = new System.Windows.Forms.Button();
            this.chartTimeseries = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.SamplesOnDisplay = new System.Windows.Forms.TextBox();
            this.ITextBox = new System.Windows.Forms.TextBox();
            this.PTextBox = new System.Windows.Forms.TextBox();
            this.DTextBox = new System.Windows.Forms.TextBox();
            this.yminTextBox = new System.Windows.Forms.TextBox();
            this.ymaxTextBox = new System.Windows.Forms.TextBox();
            this.sampleRateTextBox = new System.Windows.Forms.TextBox();
            this.updatePIDButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.chartXY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartTimeseries)).BeginInit();
            this.SuspendLayout();
            // 
            // chartXY
            // 
            chartArea3.Name = "ChartArea1";
            this.chartXY.ChartAreas.Add(chartArea3);
            this.chartXY.Dock = System.Windows.Forms.DockStyle.Bottom;
            legend3.Name = "Legend1";
            this.chartXY.Legends.Add(legend3);
            this.chartXY.Location = new System.Drawing.Point(0, 435);
            this.chartXY.Name = "chartXY";
            series3.ChartArea = "ChartArea1";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series3.Legend = "Legend1";
            series3.Name = "Series1";
            this.chartXY.Series.Add(series3);
            this.chartXY.Size = new System.Drawing.Size(854, 303);
            this.chartXY.TabIndex = 0;
            this.chartXY.Text = "chart1";
            this.chartXY.Click += new System.EventHandler(this.chart1_Click);
            this.chartXY.DoubleClick += new System.EventHandler(this.chartXY_DoubleClick);
            // 
            // LockButton
            // 
            this.LockButton.Location = new System.Drawing.Point(13, 13);
            this.LockButton.Name = "LockButton";
            this.LockButton.Size = new System.Drawing.Size(121, 23);
            this.LockButton.TabIndex = 1;
            this.LockButton.Text = "Lock";
            this.LockButton.UseVisualStyleBackColor = true;
            this.LockButton.Click += new System.EventHandler(this.LockButton_Click);
            // 
            // UnlockButton
            // 
            this.UnlockButton.Location = new System.Drawing.Point(140, 12);
            this.UnlockButton.Name = "UnlockButton";
            this.UnlockButton.Size = new System.Drawing.Size(121, 23);
            this.UnlockButton.TabIndex = 2;
            this.UnlockButton.Text = "Unlock";
            this.UnlockButton.UseVisualStyleBackColor = true;
            this.UnlockButton.Click += new System.EventHandler(this.UnlockButton_Click);
            // 
            // ModulationAmplitudeInput
            // 
            this.ModulationAmplitudeInput.Location = new System.Drawing.Point(13, 43);
            this.ModulationAmplitudeInput.Name = "ModulationAmplitudeInput";
            this.ModulationAmplitudeInput.Size = new System.Drawing.Size(121, 20);
            this.ModulationAmplitudeInput.TabIndex = 3;
            this.ModulationAmplitudeInput.Text = "ModulationAmplitude";
            this.ModulationAmplitudeInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ModulationAmplitudeInput_TextChanged);
            // 
            // ModulationAttenuationInput
            // 
            this.ModulationAttenuationInput.Location = new System.Drawing.Point(13, 71);
            this.ModulationAttenuationInput.Name = "ModulationAttenuationInput";
            this.ModulationAttenuationInput.Size = new System.Drawing.Size(121, 20);
            this.ModulationAttenuationInput.TabIndex = 4;
            this.ModulationAttenuationInput.Text = "ModulationAttenuation";
            this.ModulationAttenuationInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ModulationAttenuationInput_TextChanged);
            // 
            // DemodulationAmplitudeInput
            // 
            this.DemodulationAmplitudeInput.Location = new System.Drawing.Point(140, 43);
            this.DemodulationAmplitudeInput.Name = "DemodulationAmplitudeInput";
            this.DemodulationAmplitudeInput.Size = new System.Drawing.Size(121, 20);
            this.DemodulationAmplitudeInput.TabIndex = 5;
            this.DemodulationAmplitudeInput.Text = "DemodulationAmplitude";
            this.DemodulationAmplitudeInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DemodulationAmplitudeInput_TextChanged);
            // 
            // DemodulationAttenuationInput
            // 
            this.DemodulationAttenuationInput.Location = new System.Drawing.Point(140, 68);
            this.DemodulationAttenuationInput.Name = "DemodulationAttenuationInput";
            this.DemodulationAttenuationInput.Size = new System.Drawing.Size(121, 20);
            this.DemodulationAttenuationInput.TabIndex = 6;
            this.DemodulationAttenuationInput.Text = "DemodulationAttenuation";
            this.DemodulationAttenuationInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DemodulationAttenuationInput_TextChanged);
            // 
            // ModulationFrequencyInput
            // 
            this.ModulationFrequencyInput.Location = new System.Drawing.Point(13, 97);
            this.ModulationFrequencyInput.Name = "ModulationFrequencyInput";
            this.ModulationFrequencyInput.Size = new System.Drawing.Size(121, 20);
            this.ModulationFrequencyInput.TabIndex = 7;
            this.ModulationFrequencyInput.Text = "ModulationFrequency";
            this.ModulationFrequencyInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ModulationFrequencyInput_TextChanged);
            // 
            // PhaseInput
            // 
            this.PhaseInput.Location = new System.Drawing.Point(13, 123);
            this.PhaseInput.Name = "PhaseInput";
            this.PhaseInput.Size = new System.Drawing.Size(121, 20);
            this.PhaseInput.TabIndex = 8;
            this.PhaseInput.Text = "Phase";
            this.PhaseInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PhaseInput_TextChanged);
            // 
            // StreamTargetPortInput
            // 
            this.StreamTargetPortInput.Location = new System.Drawing.Point(687, 13);
            this.StreamTargetPortInput.Name = "StreamTargetPortInput";
            this.StreamTargetPortInput.Size = new System.Drawing.Size(101, 20);
            this.StreamTargetPortInput.TabIndex = 9;
            this.StreamTargetPortInput.Text = "1883";
            this.StreamTargetPortInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.StreamTargetPortInput_TextChanged);
            // 
            // ScanFreqDownButton
            // 
            this.ScanFreqDownButton.Location = new System.Drawing.Point(364, 118);
            this.ScanFreqDownButton.Name = "ScanFreqDownButton";
            this.ScanFreqDownButton.Size = new System.Drawing.Size(75, 23);
            this.ScanFreqDownButton.TabIndex = 11;
            this.ScanFreqDownButton.Text = "ScanFreq/2";
            this.ScanFreqDownButton.UseVisualStyleBackColor = true;
            this.ScanFreqDownButton.Click += new System.EventHandler(this.ScanFreqDownButton_Click);
            // 
            // ScanFreqUpButton
            // 
            this.ScanFreqUpButton.Location = new System.Drawing.Point(446, 118);
            this.ScanFreqUpButton.Name = "ScanFreqUpButton";
            this.ScanFreqUpButton.Size = new System.Drawing.Size(75, 23);
            this.ScanFreqUpButton.TabIndex = 12;
            this.ScanFreqUpButton.Text = "ScanFreq*2";
            this.ScanFreqUpButton.UseVisualStyleBackColor = true;
            this.ScanFreqUpButton.Click += new System.EventHandler(this.ScanFreqUpButton_Click);
            // 
            // DemodulationFrequencyInput
            // 
            this.DemodulationFrequencyInput.Location = new System.Drawing.Point(140, 97);
            this.DemodulationFrequencyInput.Name = "DemodulationFrequencyInput";
            this.DemodulationFrequencyInput.Size = new System.Drawing.Size(121, 20);
            this.DemodulationFrequencyInput.TabIndex = 13;
            this.DemodulationFrequencyInput.Text = "DemodulationFrequency";
            this.DemodulationFrequencyInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DemodulationFrequencyInput_TextChanged);
            // 
            // AveragesInput
            // 
            this.AveragesInput.Location = new System.Drawing.Point(688, 67);
            this.AveragesInput.Name = "AveragesInput";
            this.AveragesInput.Size = new System.Drawing.Size(100, 20);
            this.AveragesInput.TabIndex = 14;
            this.AveragesInput.Text = "Averages";
            this.AveragesInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AveragesInput_KeyDown);
            // 
            // SamplesMemoryInput
            // 
            this.SamplesMemoryInput.Location = new System.Drawing.Point(687, 94);
            this.SamplesMemoryInput.Name = "SamplesMemoryInput";
            this.SamplesMemoryInput.Size = new System.Drawing.Size(100, 20);
            this.SamplesMemoryInput.TabIndex = 15;
            this.SamplesMemoryInput.Text = "SamplesInMemory";
            this.SamplesMemoryInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SamplesMemoryInput_KeyDown);
            // 
            // InitButton
            // 
            this.InitButton.Location = new System.Drawing.Point(267, 12);
            this.InitButton.Name = "InitButton";
            this.InitButton.Size = new System.Drawing.Size(75, 23);
            this.InitButton.TabIndex = 17;
            this.InitButton.Text = "Init";
            this.InitButton.UseVisualStyleBackColor = true;
            this.InitButton.Click += new System.EventHandler(this.InitButton_Click);
            // 
            // StreamTargetIPInput
            // 
            this.StreamTargetIPInput.Location = new System.Drawing.Point(581, 13);
            this.StreamTargetIPInput.Name = "StreamTargetIPInput";
            this.StreamTargetIPInput.Size = new System.Drawing.Size(100, 20);
            this.StreamTargetIPInput.TabIndex = 19;
            this.StreamTargetIPInput.Text = "192.168.1.229";
            this.StreamTargetIPInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.StreamTargetIPInput_KeyDown);
            // 
            // StabilizerIDInput
            // 
            this.StabilizerIDInput.Location = new System.Drawing.Point(581, 40);
            this.StabilizerIDInput.Name = "StabilizerIDInput";
            this.StabilizerIDInput.Size = new System.Drawing.Size(206, 20);
            this.StabilizerIDInput.TabIndex = 20;
            this.StabilizerIDInput.Text = "04-91-62-d2-60-2f";
            this.StabilizerIDInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.StabilizerIDInput_KeyDown);
            // 
            // HoldButton
            // 
            this.HoldButton.Location = new System.Drawing.Point(543, 118);
            this.HoldButton.Name = "HoldButton";
            this.HoldButton.Size = new System.Drawing.Size(93, 23);
            this.HoldButton.TabIndex = 23;
            this.HoldButton.Text = "Freeze Memory";
            this.HoldButton.UseVisualStyleBackColor = true;
            this.HoldButton.Click += new System.EventHandler(this.HoldButton_Click);
            // 
            // chartTimeseries
            // 
            chartArea4.Name = "ChartArea1";
            this.chartTimeseries.ChartAreas.Add(chartArea4);
            this.chartTimeseries.Dock = System.Windows.Forms.DockStyle.Bottom;
            legend4.Name = "Legend1";
            this.chartTimeseries.Legends.Add(legend4);
            this.chartTimeseries.Location = new System.Drawing.Point(0, 155);
            this.chartTimeseries.Name = "chartTimeseries";
            series4.ChartArea = "ChartArea1";
            series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series4.Legend = "Legend1";
            series4.Name = "Series1";
            this.chartTimeseries.Series.Add(series4);
            this.chartTimeseries.Size = new System.Drawing.Size(854, 280);
            this.chartTimeseries.TabIndex = 24;
            this.chartTimeseries.Text = "chart2";
            this.chartTimeseries.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.chartTimeseries_MouseDoubleClick);
            // 
            // SamplesOnDisplay
            // 
            this.SamplesOnDisplay.Location = new System.Drawing.Point(687, 120);
            this.SamplesOnDisplay.Name = "SamplesOnDisplay";
            this.SamplesOnDisplay.Size = new System.Drawing.Size(100, 20);
            this.SamplesOnDisplay.TabIndex = 25;
            this.SamplesOnDisplay.Text = "Samples";
            this.SamplesOnDisplay.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SamplesOnDisplay_KeyDown);
            // 
            // ITextBox
            // 
            this.ITextBox.Location = new System.Drawing.Point(403, 71);
            this.ITextBox.Name = "ITextBox";
            this.ITextBox.Size = new System.Drawing.Size(36, 20);
            this.ITextBox.TabIndex = 27;
            this.ITextBox.Text = "0.1";
            // 
            // PTextBox
            // 
            this.PTextBox.Location = new System.Drawing.Point(361, 71);
            this.PTextBox.Name = "PTextBox";
            this.PTextBox.Size = new System.Drawing.Size(36, 20);
            this.PTextBox.TabIndex = 28;
            this.PTextBox.Text = "1";
            // 
            // DTextBox
            // 
            this.DTextBox.Location = new System.Drawing.Point(445, 71);
            this.DTextBox.Name = "DTextBox";
            this.DTextBox.Size = new System.Drawing.Size(36, 20);
            this.DTextBox.TabIndex = 29;
            this.DTextBox.Text = "0";
            // 
            // yminTextBox
            // 
            this.yminTextBox.Location = new System.Drawing.Point(507, 71);
            this.yminTextBox.Name = "yminTextBox";
            this.yminTextBox.Size = new System.Drawing.Size(36, 20);
            this.yminTextBox.TabIndex = 30;
            this.yminTextBox.Text = "0";
            // 
            // ymaxTextBox
            // 
            this.ymaxTextBox.Location = new System.Drawing.Point(549, 71);
            this.ymaxTextBox.Name = "ymaxTextBox";
            this.ymaxTextBox.Size = new System.Drawing.Size(36, 20);
            this.ymaxTextBox.TabIndex = 31;
            this.ymaxTextBox.Text = "10";
            // 
            // sampleRateTextBox
            // 
            this.sampleRateTextBox.Location = new System.Drawing.Point(605, 71);
            this.sampleRateTextBox.Name = "sampleRateTextBox";
            this.sampleRateTextBox.Size = new System.Drawing.Size(50, 20);
            this.sampleRateTextBox.TabIndex = 32;
            this.sampleRateTextBox.Text = "0.001";
            // 
            // updatePIDButton
            // 
            this.updatePIDButton.Location = new System.Drawing.Point(361, 39);
            this.updatePIDButton.Name = "updatePIDButton";
            this.updatePIDButton.Size = new System.Drawing.Size(75, 23);
            this.updatePIDButton.TabIndex = 33;
            this.updatePIDButton.Text = "Update PID";
            this.updatePIDButton.UseVisualStyleBackColor = true;
            this.updatePIDButton.Click += new System.EventHandler(this.updatePIDButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(854, 738);
            this.Controls.Add(this.updatePIDButton);
            this.Controls.Add(this.sampleRateTextBox);
            this.Controls.Add(this.ymaxTextBox);
            this.Controls.Add(this.yminTextBox);
            this.Controls.Add(this.DTextBox);
            this.Controls.Add(this.PTextBox);
            this.Controls.Add(this.ITextBox);
            this.Controls.Add(this.SamplesOnDisplay);
            this.Controls.Add(this.chartTimeseries);
            this.Controls.Add(this.HoldButton);
            this.Controls.Add(this.StabilizerIDInput);
            this.Controls.Add(this.StreamTargetIPInput);
            this.Controls.Add(this.InitButton);
            this.Controls.Add(this.SamplesMemoryInput);
            this.Controls.Add(this.AveragesInput);
            this.Controls.Add(this.DemodulationFrequencyInput);
            this.Controls.Add(this.ScanFreqUpButton);
            this.Controls.Add(this.ScanFreqDownButton);
            this.Controls.Add(this.StreamTargetPortInput);
            this.Controls.Add(this.PhaseInput);
            this.Controls.Add(this.ModulationFrequencyInput);
            this.Controls.Add(this.DemodulationAttenuationInput);
            this.Controls.Add(this.DemodulationAmplitudeInput);
            this.Controls.Add(this.ModulationAttenuationInput);
            this.Controls.Add(this.ModulationAmplitudeInput);
            this.Controls.Add(this.UnlockButton);
            this.Controls.Add(this.LockButton);
            this.Controls.Add(this.chartXY);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.chartXY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartTimeseries)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart chartXY;
        private System.Windows.Forms.Button LockButton;
        private System.Windows.Forms.Button UnlockButton;
        private System.Windows.Forms.TextBox ModulationAmplitudeInput;
        private System.Windows.Forms.TextBox ModulationAttenuationInput;
        private System.Windows.Forms.TextBox DemodulationAmplitudeInput;
        private System.Windows.Forms.TextBox DemodulationAttenuationInput;
        private System.Windows.Forms.TextBox ModulationFrequencyInput;
        private System.Windows.Forms.TextBox PhaseInput;
        private System.Windows.Forms.TextBox StreamTargetPortInput;
        private System.Windows.Forms.Button ScanFreqDownButton;
        private System.Windows.Forms.Button ScanFreqUpButton;
        private System.Windows.Forms.TextBox DemodulationFrequencyInput;
        private System.Windows.Forms.TextBox AveragesInput;
        private System.Windows.Forms.TextBox SamplesMemoryInput;
        private System.Windows.Forms.Button InitButton;
        private System.Windows.Forms.TextBox StreamTargetIPInput;
        private System.Windows.Forms.TextBox StabilizerIDInput;
        private System.Windows.Forms.Button HoldButton;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartTimeseries;
        private System.Windows.Forms.TextBox SamplesOnDisplay;
        private System.Windows.Forms.TextBox ITextBox;
        private System.Windows.Forms.TextBox PTextBox;
        private System.Windows.Forms.TextBox DTextBox;
        private System.Windows.Forms.TextBox yminTextBox;
        private System.Windows.Forms.TextBox ymaxTextBox;
        private System.Windows.Forms.TextBox sampleRateTextBox;
        private System.Windows.Forms.Button updatePIDButton;
    }
}

