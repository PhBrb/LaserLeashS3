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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.LockButton = new System.Windows.Forms.Button();
            this.UnlockButton = new System.Windows.Forms.Button();
            this.ModulationPowerInput = new System.Windows.Forms.TextBox();
            this.ModulationAttenuationInput = new System.Windows.Forms.TextBox();
            this.DemodulationPowerInput = new System.Windows.Forms.TextBox();
            this.DemodulationAttenuationInput = new System.Windows.Forms.TextBox();
            this.FrequencyInput = new System.Windows.Forms.TextBox();
            this.PhaseInput = new System.Windows.Forms.TextBox();
            this.StreamTargetInput = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.SuspendLayout();
            // 
            // chart1
            // 
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            this.chart1.Dock = System.Windows.Forms.DockStyle.Bottom;
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(0, 147);
            this.chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chart1.Series.Add(series1);
            this.chart1.Size = new System.Drawing.Size(800, 303);
            this.chart1.TabIndex = 0;
            this.chart1.Text = "chart1";
            this.chart1.Click += new System.EventHandler(this.chart1_Click);
            // 
            // LockButton
            // 
            this.LockButton.Location = new System.Drawing.Point(13, 13);
            this.LockButton.Name = "LockButton";
            this.LockButton.Size = new System.Drawing.Size(121, 23);
            this.LockButton.TabIndex = 1;
            this.LockButton.Text = "Lock";
            this.LockButton.UseVisualStyleBackColor = true;
            this.LockButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // UnlockButton
            // 
            this.UnlockButton.Location = new System.Drawing.Point(140, 12);
            this.UnlockButton.Name = "UnlockButton";
            this.UnlockButton.Size = new System.Drawing.Size(121, 23);
            this.UnlockButton.TabIndex = 2;
            this.UnlockButton.Text = "Unlock";
            this.UnlockButton.UseVisualStyleBackColor = true;
            // 
            // ModulationPowerInput
            // 
            this.ModulationPowerInput.Location = new System.Drawing.Point(13, 43);
            this.ModulationPowerInput.Name = "ModulationPowerInput";
            this.ModulationPowerInput.Size = new System.Drawing.Size(121, 20);
            this.ModulationPowerInput.TabIndex = 3;
            this.ModulationPowerInput.Text = "ModulationPower";
            // 
            // ModulationAttenuationInput
            // 
            this.ModulationAttenuationInput.Location = new System.Drawing.Point(140, 42);
            this.ModulationAttenuationInput.Name = "ModulationAttenuationInput";
            this.ModulationAttenuationInput.Size = new System.Drawing.Size(121, 20);
            this.ModulationAttenuationInput.TabIndex = 4;
            this.ModulationAttenuationInput.Text = "ModulationAttenuation";
            // 
            // DemodulationPowerInput
            // 
            this.DemodulationPowerInput.Location = new System.Drawing.Point(13, 70);
            this.DemodulationPowerInput.Name = "DemodulationPowerInput";
            this.DemodulationPowerInput.Size = new System.Drawing.Size(121, 20);
            this.DemodulationPowerInput.TabIndex = 5;
            this.DemodulationPowerInput.Text = "DemodulationPower";
            // 
            // DemodulationAttenuationInput
            // 
            this.DemodulationAttenuationInput.Location = new System.Drawing.Point(140, 68);
            this.DemodulationAttenuationInput.Name = "DemodulationAttenuationInput";
            this.DemodulationAttenuationInput.Size = new System.Drawing.Size(121, 20);
            this.DemodulationAttenuationInput.TabIndex = 6;
            this.DemodulationAttenuationInput.Text = "DemodulationAttenuation";
            // 
            // FrequencyInput
            // 
            this.FrequencyInput.Location = new System.Drawing.Point(13, 97);
            this.FrequencyInput.Name = "FrequencyInput";
            this.FrequencyInput.Size = new System.Drawing.Size(121, 20);
            this.FrequencyInput.TabIndex = 7;
            this.FrequencyInput.Text = "Frequency";
            // 
            // PhaseInput
            // 
            this.PhaseInput.Location = new System.Drawing.Point(140, 96);
            this.PhaseInput.Name = "PhaseInput";
            this.PhaseInput.Size = new System.Drawing.Size(121, 20);
            this.PhaseInput.TabIndex = 8;
            this.PhaseInput.Text = "Phase";
            // 
            // StreamTargetInput
            // 
            this.StreamTargetInput.Location = new System.Drawing.Point(491, 13);
            this.StreamTargetInput.Name = "StreamTargetInput";
            this.StreamTargetInput.Size = new System.Drawing.Size(297, 20);
            this.StreamTargetInput.TabIndex = 9;
            this.StreamTargetInput.Text = "stream_target={\"\"\"ip\"\"\":[192,168,1,229],\"\"\"port\"\"\":1883}";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.StreamTargetInput);
            this.Controls.Add(this.PhaseInput);
            this.Controls.Add(this.FrequencyInput);
            this.Controls.Add(this.DemodulationAttenuationInput);
            this.Controls.Add(this.DemodulationPowerInput);
            this.Controls.Add(this.ModulationAttenuationInput);
            this.Controls.Add(this.ModulationPowerInput);
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
        private System.Windows.Forms.TextBox ModulationPowerInput;
        private System.Windows.Forms.TextBox ModulationAttenuationInput;
        private System.Windows.Forms.TextBox DemodulationPowerInput;
        private System.Windows.Forms.TextBox DemodulationAttenuationInput;
        private System.Windows.Forms.TextBox FrequencyInput;
        private System.Windows.Forms.TextBox PhaseInput;
        private System.Windows.Forms.TextBox StreamTargetInput;
    }
}

