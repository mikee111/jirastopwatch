/**************************************************************************
Copyright 2016 Carsten Gehling

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
**************************************************************************/
namespace StopWatch
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.pbSettings = new System.Windows.Forms.PictureBox();
            this.lblConnectionStatus = new System.Windows.Forms.Label();
            this.cbFilters = new System.Windows.Forms.ComboBox();
            this.lblActiveFilter = new System.Windows.Forms.Label();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.lblBackupTime = new System.Windows.Forms.Label();
            this.tbBackupTime = new System.Windows.Forms.TextBox();
            this.pMain = new System.Windows.Forms.Panel();
            this.pBottom = new System.Windows.Forms.Panel();
            this.btnReset = new System.Windows.Forms.Button();
            this.lblDivider = new System.Windows.Forms.Label();
            this.pbAddIssue = new System.Windows.Forms.PictureBox();
            this.ttMain = new System.Windows.Forms.ToolTip(this.components);
            this.pbHelp = new System.Windows.Forms.PictureBox();
            this.pTop = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.pbSettings)).BeginInit();
            this.pBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbAddIssue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbHelp)).BeginInit();
            this.pTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // pbSettings
            // 
            this.pbSettings.BackgroundImage = global::StopWatch.Properties.Resources.settings22;
            this.pbSettings.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbSettings.Location = new System.Drawing.Point(457, 6);
            this.pbSettings.Margin = new System.Windows.Forms.Padding(2);
            this.pbSettings.Name = "pbSettings";
            this.pbSettings.Size = new System.Drawing.Size(22, 22);
            this.pbSettings.TabIndex = 0;
            this.pbSettings.TabStop = false;
            this.ttMain.SetToolTip(this.pbSettings, "Configure Jira Stopwatch");
            this.pbSettings.Click += new System.EventHandler(this.pbSettings_Click);
            // 
            // lblConnectionStatus
            // 
            this.lblConnectionStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblConnectionStatus.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.lblConnectionStatus.Location = new System.Drawing.Point(12, 8);
            this.lblConnectionStatus.Name = "lblConnectionStatus";
            this.lblConnectionStatus.Size = new System.Drawing.Size(117, 21);
            this.lblConnectionStatus.TabIndex = 3;
            this.lblConnectionStatus.Text = "x";
            this.lblConnectionStatus.Click += new System.EventHandler(this.lblConnectionStatus_Click);
            // 
            // cbFilters
            // 
            this.cbFilters.DropDownWidth = 422;
            this.cbFilters.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.cbFilters.FormattingEnabled = true;
            this.cbFilters.Location = new System.Drawing.Point(56, 5);
            this.cbFilters.Margin = new System.Windows.Forms.Padding(2);
            this.cbFilters.Name = "cbFilters";
            this.cbFilters.Size = new System.Drawing.Size(151, 24);
            this.cbFilters.TabIndex = 4;
            this.cbFilters.DropDown += new System.EventHandler(this.cbFilters_DropDown);
            this.cbFilters.SelectedIndexChanged += new System.EventHandler(this.cbFilters_SelectedIndexChanged);
            // 
            // lblActiveFilter
            // 
            this.lblActiveFilter.AutoSize = true;
            this.lblActiveFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblActiveFilter.ForeColor = System.Drawing.Color.White;
            this.lblActiveFilter.Location = new System.Drawing.Point(12, 8);
            this.lblActiveFilter.Name = "lblActiveFilter";
            this.lblActiveFilter.Size = new System.Drawing.Size(39, 17);
            this.lblActiveFilter.TabIndex = 5;
            this.lblActiveFilter.Text = "Filter";
            // 
            // notifyIcon
            // 
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "JIRA StopWatch";
            this.notifyIcon.Click += new System.EventHandler(this.notifyIcon_Click);
            // 
            // lblBackupTime
            // 
            this.lblBackupTime.AutoSize = true;
            this.lblBackupTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblBackupTime.Location = new System.Drawing.Point(241, 9);
            this.lblBackupTime.Name = "lblBackupTime";
            this.lblBackupTime.Size = new System.Drawing.Size(95, 17);
            this.lblBackupTime.TabIndex = 6;
            this.lblBackupTime.Text = "Backup Timer";
            this.lblBackupTime.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // tbBackupTime
            // 
            this.tbBackupTime.BackColor = System.Drawing.SystemColors.Window;
            this.tbBackupTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbBackupTime.Location = new System.Drawing.Point(342, 5);
            this.tbBackupTime.Name = "tbBackupTime";
            this.tbBackupTime.ReadOnly = true;
            this.tbBackupTime.Size = new System.Drawing.Size(83, 23);
            this.tbBackupTime.TabIndex = 8;
            this.tbBackupTime.Text = "00d 0h 00m";
            this.tbBackupTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.ttMain.SetToolTip(this.tbBackupTime, "fdfdf");
            // 
            // pMain
            // 
            this.pMain.BackColor = System.Drawing.SystemColors.Window;
            this.pMain.Location = new System.Drawing.Point(0, 35);
            this.pMain.Margin = new System.Windows.Forms.Padding(0);
            this.pMain.Name = "pMain";
            this.pMain.Size = new System.Drawing.Size(484, 70);
            this.pMain.TabIndex = 9;
            // 
            // pBottom
            // 
            this.pBottom.Controls.Add(this.btnReset);
            this.pBottom.Controls.Add(this.lblDivider);
            this.pBottom.Controls.Add(this.tbBackupTime);
            this.pBottom.Controls.Add(this.pbSettings);
            this.pBottom.Controls.Add(this.lblBackupTime);
            this.pBottom.Controls.Add(this.lblConnectionStatus);
            this.pBottom.Location = new System.Drawing.Point(0, 140);
            this.pBottom.Name = "pBottom";
            this.pBottom.Size = new System.Drawing.Size(484, 32);
            this.pBottom.TabIndex = 10;
            // 
            // btnReset
            // 
            this.btnReset.BackgroundImage = global::StopWatch.Properties.Resources.reset24;
            this.btnReset.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnReset.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnReset.Location = new System.Drawing.Point(430, 6);
            this.btnReset.Margin = new System.Windows.Forms.Padding(2);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(22, 22);
            this.btnReset.TabIndex = 12;
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // lblDivider
            // 
            this.lblDivider.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblDivider.Location = new System.Drawing.Point(0, 0);
            this.lblDivider.Name = "lblDivider";
            this.lblDivider.Size = new System.Drawing.Size(517, 2);
            this.lblDivider.TabIndex = 9;
            // 
            // pbAddIssue
            // 
            this.pbAddIssue.BackgroundImage = global::StopWatch.Properties.Resources.addissue22;
            this.pbAddIssue.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbAddIssue.Location = new System.Drawing.Point(429, 5);
            this.pbAddIssue.Margin = new System.Windows.Forms.Padding(2);
            this.pbAddIssue.Name = "pbAddIssue";
            this.pbAddIssue.Size = new System.Drawing.Size(24, 24);
            this.pbAddIssue.TabIndex = 11;
            this.pbAddIssue.TabStop = false;
            this.ttMain.SetToolTip(this.pbAddIssue, "Add another issue row (CTRL-N)");
            this.pbAddIssue.Click += new System.EventHandler(this.pbAddIssue_Clicked);
            // 
            // pbHelp
            // 
            this.pbHelp.BackgroundImage = global::StopWatch.Properties.Resources.help22;
            this.pbHelp.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbHelp.Location = new System.Drawing.Point(458, 5);
            this.pbHelp.Name = "pbHelp";
            this.pbHelp.Size = new System.Drawing.Size(22, 22);
            this.pbHelp.TabIndex = 12;
            this.pbHelp.TabStop = false;
            this.ttMain.SetToolTip(this.pbHelp, "Open help page in your browser");
            this.pbHelp.Click += new System.EventHandler(this.pbHelp_Click);
            // 
            // pTop
            // 
            this.pTop.BackColor = System.Drawing.Color.SteelBlue;
            this.pTop.Controls.Add(this.pbHelp);
            this.pTop.Controls.Add(this.lblActiveFilter);
            this.pTop.Controls.Add(this.cbFilters);
            this.pTop.Controls.Add(this.pbAddIssue);
            this.pTop.Location = new System.Drawing.Point(0, 0);
            this.pTop.Name = "pTop";
            this.pTop.Size = new System.Drawing.Size(484, 35);
            this.pTop.TabIndex = 11;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(483, 172);
            this.Controls.Add(this.pTop);
            this.Controls.Add(this.pBottom);
            this.Controls.Add(this.pMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = global::StopWatch.Properties.Resources.stopwatchicon;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "JIRA StopWatch";
            this.TopMost = true;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.pbSettings)).EndInit();
            this.pBottom.ResumeLayout(false);
            this.pBottom.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbAddIssue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbHelp)).EndInit();
            this.pTop.ResumeLayout(false);
            this.pTop.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbSettings;
        private System.Windows.Forms.Label lblConnectionStatus;
        private System.Windows.Forms.ComboBox cbFilters;
        private System.Windows.Forms.Label lblActiveFilter;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.Label lblBackupTime;
        private System.Windows.Forms.TextBox tbBackupTime;
        private System.Windows.Forms.Panel pMain;
        private System.Windows.Forms.Panel pBottom;
        private System.Windows.Forms.ToolTip ttMain;
        private System.Windows.Forms.PictureBox pbAddIssue;
        private System.Windows.Forms.Panel pTop;
        private System.Windows.Forms.Label lblDivider;
        private System.Windows.Forms.PictureBox pbHelp;
        private System.Windows.Forms.Button btnReset;
    }
}

