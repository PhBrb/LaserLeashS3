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
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
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
            this.SamplesInput = new System.Windows.Forms.TextBox();
            this.InitButton = new System.Windows.Forms.Button();
            this.StreamTargetIPInput = new System.Windows.Forms.TextBox();
            this.StabilizerIDInput = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.SuspendLayout();
            // 
            // chart1
            // 
            chartArea3.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea3);
            this.chart1.Dock = System.Windows.Forms.DockStyle.Bottom;
            legend3.Name = "Legend1";
            this.chart1.Legends.Add(legend3);
            this.chart1.Location = new System.Drawing.Point(0, 147);
            this.chart1.Name = "chart1";
            series3.ChartArea = "ChartArea1";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series3.Legend = "Legend1";
            series3.Name = "Series1";
            this.chart1.Series.Add(series3);
            this.chart1.Size = new System.Drawing.Size(800, 303);
            this.chart1.TabIndex = 0;
            this.chart1.Text = "chart1";
            this.chart1.Click += new System.EventHandler(this.chart1_Click);
            this.chart1.DoubleClick += new System.EventHandler(this.chart1_DoubleClick);
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
            // SamplesInput
            // 
            this.SamplesInput.Location = new System.Drawing.Point(687, 94);
            this.SamplesInput.Name = "SamplesInput";
            this.SamplesInput.Size = new System.Drawing.Size(100, 20);
            this.SamplesInput.TabIndex = 15;
            this.SamplesInput.Text = "Samples";
            this.SamplesInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SamplesInput_KeyDown);
            // 
            // InitButton
            // 
            this.InitButton.Location = new System.Drawing.Point(301, 13);
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
            this.StreamTargetIPInput.Text = "127.0.0.1";
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
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(338, 70);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 21;
            this.textBox1.Text = "YOffset";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.StabilizerIDInput);
            this.Controls.Add(this.StreamTargetIPInput);
            this.Controls.Add(this.InitButton);
            this.Controls.Add(this.SamplesInput);
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
            this.Controls.Add(this.chart1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
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
        private System.Windows.Forms.TextBox SamplesInput;
        private System.Windows.Forms.Button InitButton;
        private System.Windows.Forms.TextBox StreamTargetIPInput;
        private System.Windows.Forms.TextBox StabilizerIDInput;
        private System.Windows.Forms.TextBox textBox1;
    }
}

