﻿/**************************************************************************
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
using StopWatch.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace StopWatch
{
    public partial class MainForm : Form
    {
        #region public methods
        public MainForm()
        {
            settings = Settings.Instance;
            if (!settings.Load())
                MessageBox.Show(string.Format("An error occurred while loading settings for Jira StopWatch. Your configuration file has most likely become corrupted.{0}{0}And older configuration file has been loaded instead, so please verify your settings.", Environment.NewLine), "Jira StopWatch");

            Logger.Instance.LogfilePath = Path.Combine(Application.UserAppDataPath, "jirastopwatch.log");
            Logger.Instance.Enabled = settings.LoggingEnabled;

            restRequestFactory = new RestRequestFactory();
            jiraApiRequestFactory = new JiraApiRequestFactory(restRequestFactory);

            restClientFactory = new RestClientFactory();
            restClientFactory.BaseUrl = this.settings.JiraBaseUrl;

            jiraApiRequester = new JiraApiRequester(restClientFactory, jiraApiRequestFactory);

            jiraClient = new JiraClient(jiraApiRequestFactory, jiraApiRequester);

            InitializeComponent();

            pMain.HorizontalScroll.Maximum = 0;
            pMain.AutoScroll = false;
            pMain.VerticalScroll.Visible = false;
            pMain.AutoScroll = true;

            Text = string.Format("{0} {1}", Application.ProductName, Application.ProductVersion);

            cbFilters.DropDownStyle = ComboBoxStyle.DropDownList;
            cbFilters.DisplayMember = "Name";

            ticker = new Timer();
            // First run should be almost immediately after start
            ticker.Interval = firstDelay;
            ticker.Tick += ticker_Tick;

            idleTicker = new Timer();
            idleTicker.Interval = idleCheckInterval;
            idleTicker.Tick += idle_Tick;

            runningTicker = new Timer();
            runningTicker.Interval = runningCheckInterval;
            runningTicker.Tick += running_Tick;

            backupTicker = new Timer();
            backupTicker.Interval = backupCheckInterval;
            backupTicker.Tick += backup_Tick;
            
            backupTimer = new WatchTimer();

            // load backup timer state
            TimerState backupTimerState = new TimerState
            {
                TotalTime = settings.BackupTimer
            };
            backupTimer.SetState(backupTimerState);

            UpdateBackupTime();
        }


        public void HandleSessionLock()
        {
            if (settings.PauseOnSessionLock == PauseAndResumeSetting.NoPause)
                return;

            foreach (var issue in issueControls)
            {
                if (issue.WatchTimer.Running)
                {
                    lastRunningIssue = issue;
                    issue.InvokeIfRequired(
                        () => issue.Pause()
                    );
                    return;
                }
            }
        }


        public void HandleSessionUnlock()
        {
            if (settings.PauseOnSessionLock != PauseAndResumeSetting.PauseAndResume)
                return;

            if (lastRunningIssue != null)
            {
                lastRunningIssue.InvokeIfRequired(
                    () => lastRunningIssue.Start()
                );
                lastRunningIssue = null;
            }
        }
        #endregion


        #region private eventhandlers
        void issue_TimerStarted(object sender, EventArgs e)
        {
            IssueControl senderCtrl = (IssueControl)sender;

            PauseBackupTimer();

            if (settings.AllowMultipleTimers)
                return;

            foreach (var issue in this.issueControls)
                if (issue != senderCtrl)
                    issue.Pause();
        }

        void issue_TimerPaused(object sender, EventArgs e)
        {
            TryStartingBackupTimer();
        }

        void Issue_TimerReset(object sender, EventArgs e)
        {
            TryStartingBackupTimer();
        }

        void ticker_Tick(object sender, EventArgs e)
        {
            bool firstTick = ticker.Interval == firstDelay;

            ticker.Interval = defaultDelay;

            UpdateJiraRelatedData(firstTick);
            UpdateIssuesOutput(firstTick);

            SaveSettingsAndIssueStates();

						if (firstTick)
            {
                CheckForUpdates();
            }
        }

		internal struct LASTINPUTINFO
		{
			public uint cbSize;
			public uint dwTime;
		}

		/// <summary>
		/// Helps to find the idle time, (in milliseconds) spent since the last user input
		/// </summary>
		public class IdleTimeFinder
		{
			[DllImport("User32.dll")]
			private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

			[DllImport("Kernel32.dll")]
			private static extern uint GetLastError();

			public static uint GetIdleTime()
			{
				LASTINPUTINFO lastInPut = new LASTINPUTINFO();
				lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
				GetLastInputInfo(ref lastInPut);

				return ((uint)Environment.TickCount - lastInPut.dwTime);
			}
			/// <summary>
			/// Get the Last input time in milliseconds
			/// </summary>
			/// <returns></returns>
			public static long GetLastInputTime()
			{
				LASTINPUTINFO lastInPut = new LASTINPUTINFO();
				lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
				if (!GetLastInputInfo(ref lastInPut))
				{
					throw new Exception(GetLastError().ToString());
				}
				return lastInPut.dwTime;
			}
		}

        void running_Tick(object sender, EventArgs e)
        {
            runningTicker.Stop();
            using (RunningDialog dialog = new RunningDialog())
            {
                ShowOnTop();
                dialog.ShowDialog();
                runningTicker.Interval = dialog.GetSnooze();
            }
            runningTicker.Start();
        }

		void idle_Tick(object sender, EventArgs e)
		{
			var idleTime = IdleTimeFinder.GetIdleTime();
			if (idleTime > maxIdleTime)
			{
				ShowOnTop();

				String str = String.Format("You have been idle for more than {0} minutes.", maxIdleTime / 60000);
				var timeOnDialog = DateTime.Now.TimeOfDay;
				idleTicker.Stop();
				MessageBox.Show(str, "You have been idle!", MessageBoxButtons.OK);

				var currTime = DateTime.Now.TimeOfDay;
				var diff = currTime - timeOnDialog;
				var totalIdleTime = idleTime + diff.TotalMilliseconds;

				String remStr = String.Format("Subtract {0} minutes from all running timers?", totalIdleTime / 60000);
				DialogResult timeResult = MessageBox.Show(remStr, "Subtract time?", MessageBoxButtons.YesNo);

				if(timeResult == DialogResult.Yes)
				{
					// subtract from all running timers
					foreach (IssueControl issue in this.issueControls)
					{
						issue.WatchTimer.SubtractWhenRunning(totalIdleTime);
						issue.UpdateOutput();
					}

          backupTimer.SubtractWhenRunning(totalIdleTime);
          UpdateBackupTime();
        }

				idleTicker.Start();
			}
		}

        void backup_Tick(object sender, EventArgs e)
        {
            UpdateBackupTime();
        }

        void TryStartingBackupTimer()
        {
            if(issueControls.All(issue => !issue.WatchTimer.Running))
            {
                backupTicker.Start();
                backupTimer.Start();
                runningTicker.Start();
                tbBackupTime.BackColor = Color.PaleGreen;
            }
        }

        void PauseBackupTimer()
        {
            backupTicker.Stop();
            backupTimer.Pause();
            runningTicker.Stop();
            tbBackupTime.BackColor = SystemColors.Control;
        }

        void ResetBackupTimer()
        {
            backupTicker.Stop();
            backupTimer.Reset();
            runningTicker.Stop();
            tbBackupTime.BackColor = SystemColors.Control;

            UpdateBackupTime();
            TryStartingBackupTimer();
        }

        void UpdateBackupTime()
        {
            tbBackupTime.Text = JiraTimeHelpers.TimeSpanToJiraTime(backupTimer.TimeElapsedNearestMinute);
        }

        public void SaveBackupTimer()
        {
            settings.BackupTimer = backupTimer.GetState().TotalTime;
            settings.Save();
        }

        private void pbSettings_Click(object sender, EventArgs e)
        {
            if (settings.AlwaysOnTop)
                this.TopMost = false;
            EditSettings();
            if (settings.AlwaysOnTop)
                this.TopMost = settings.AlwaysOnTop;
        }


        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveSettingsAndIssueStates();
            SaveBackupTimer();
        }


        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (this.settings.FirstRun)
            {
                this.settings.FirstRun = false;
                EditSettings();
            }
            else
            {
                if (IsJiraEnabled)
                    AuthenticateJira(this.settings.Username, this.settings.ApiToken);
            }

            InitializeIssueControls();

            // Add issuekeys from settings to issueControl controls
            int i = 0;
            foreach (var issueControl in this.issueControls)
            {
                if (i < settings.PersistedIssues.Count)
                {
                    var persistedIssue = settings.PersistedIssues[i];
                    issueControl.IssueKey = persistedIssue.Key;

                    if (this.settings.SaveTimerState != SaveTimerSetting.NoSave)
                    {
                        TimerState timerState = new TimerState
                        {
                            Running = this.settings.SaveTimerState == SaveTimerSetting.SavePause ? false : persistedIssue.TimerRunning,
                            SessionStartTime = persistedIssue.SessionStartTime,
                            InitialStartTime = persistedIssue.InitialStartTime,
                            TotalTime = persistedIssue.TotalTime
                        };
                        issueControl.WatchTimer.SetState(timerState);
                        issueControl.Comment = persistedIssue.Comment;
                        issueControl.EstimateUpdateMethod = persistedIssue.EstimateUpdateMethod;
                        issueControl.EstimateUpdateValue = persistedIssue.EstimateUpdateValue;
                    }
                }
                i++;
            }

            Startup.Set(settings.RunOnStartup);

            ticker.Start();
            idleTicker.Start();

            TryStartingBackupTimer();
        }


        private void cbFilters_DropDown(object sender, EventArgs e)
        {
            LoadFilters();
        }


        private void cbFilters_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = (CBFilterItem)cbFilters.SelectedItem;
            this.settings.CurrentFilter = item.Id;
        }


        private void MainForm_Resize(object sender, EventArgs e)
        {
            // Mono for MacOSX and Linux do not implement the notifyIcon
            // so ignore this feature if we are not running on Windows
            if (!CrossPlatformHelpers.IsWindowsEnvironment())
                return;

            if (!this.settings.MinimizeToTray)
                return;

            if (WindowState == FormWindowState.Minimized)
            {
                this.notifyIcon.Visible = true;
                this.Hide();
						}
            else if (WindowState == FormWindowState.Normal)
            {
                this.notifyIcon.Visible = false;
            }
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void lblConnectionStatus_Click(object sender, EventArgs e)
        {
            if (jiraClient.SessionValid)
                return;

            string msg = string.Format("Jira StopWatch could not connect to your Jira server. Error returned:{0}{0}{1}", Environment.NewLine, jiraClient.ErrorMessage);
            MessageBox.Show(msg, "Connection error");
        }
        #endregion


        #region private methods
        private void AuthenticateJira(string username, string apiToken)
        {
            Task.Factory.StartNew(
                () =>
                {
                    this.InvokeIfRequired(
                        () =>
                        {
                            lblConnectionStatus.Text = "Connecting...";
                            lblConnectionStatus.ForeColor = SystemColors.ControlText;
                        }
                    );

                    if (jiraClient.Authenticate(username, apiToken))
                        this.InvokeIfRequired(
                            () => UpdateIssuesOutput(true)
                        );

                    this.InvokeIfRequired(
                        () => UpdateJiraRelatedData(true)
                    );
                }
            );
        }

        private void issue_RemoveMeTriggered(object sender, EventArgs e)
        {
            if (this.settings.IssueCount > 1)
            {
                this.settings.IssueCount--;
            }
            this.InitializeIssueControls();
            TryStartingBackupTimer();
        }

        private void pbAddIssue_Clicked(object sender, EventArgs e)
        {
            IssueAdd();
        }

        private void IssueAdd()
        {
            if (this.settings.IssueCount < maxIssues || this.issueControls.Count() < maxIssues)
            {
                this.settings.IssueCount++;
                this.InitializeIssueControls();
                IssueControl AddedIssue = this.issueControls.Last();
                IssueSetCurrentByControl(AddedIssue);
                this.pMain.ScrollControlIntoView(AddedIssue);
            }
        }

        private void InitializeIssueControls()
        {
            this.SuspendLayout();

            if (this.settings.IssueCount >= maxIssues)
            {
                // Max reached.  Reset number in case it is larger 
                this.settings.IssueCount = maxIssues;

                // Update tooltip to reflect the fact that you can't add anymore
                // We don't disable the button since then the tooltip doesn't show but
                // the click won't do anything if we have too many issues
                this.ttMain.SetToolTip(this.pbAddIssue, string.Format("You have reached the max limit of {0} issues and cannot add another", maxIssues.ToString()));
                this.pbAddIssue.Cursor = System.Windows.Forms.Cursors.No;
            }
            else
            {
                if (this.settings.IssueCount < 1)
                    this.settings.IssueCount = 1;

                // Reset status 
                this.ttMain.SetToolTip(this.pbAddIssue, "Add another issue row (CTRL-N)");
                this.pbAddIssue.Cursor = System.Windows.Forms.Cursors.Hand;
            }
            
            // Remove IssueControl where user has clicked the remove button
            foreach (IssueControl issue in this.issueControls)
            {
                if (issue.MarkedForRemoval)
                    this.pMain.Controls.Remove(issue);
            }


            // If we have too many issueControl controls, compared to this.IssueCount
            // remove the ones not needed
            while (this.issueControls.Count() > this.settings.IssueCount)
            {
                var issue = this.issueControls.Last();
                this.pMain.Controls.Remove(issue);
            }

            // Create issueControl controls needed
            while (this.issueControls.Count() < this.settings.IssueCount)
            {
                var issue = new IssueControl(this.jiraClient, this.settings);
                issue.RemoveMeTriggered += new EventHandler(this.issue_RemoveMeTriggered);
                issue.TimerStarted += issue_TimerStarted;
                issue.TimerPaused += issue_TimerPaused;
                issue.TimerReset += Issue_TimerReset;
                issue.Selected += Issue_Selected;
                issue.TimeEdited += Issue_TimeEdited;
                this.pMain.Controls.Add(issue);
            }

            // To make sure that pMain's scrollbar doesn't screw up, all IssueControls need to have
            // their position reset, before positioning them again
            foreach (IssueControl issue in this.issueControls)
            {
                issue.Left = 0;
                issue.Top = 0;
            }

            // Now position all issueControl controls
            int i = 0;
            bool EnableRemoveIssue = this.issueControls.Count() > 1;
            foreach (IssueControl issue in this.issueControls)
            {
                issue.ToggleRemoveIssueButton(EnableRemoveIssue);
                issue.Top = i * issue.Height;
                i++;
            }

            this.ClientSize = new Size(pBottom.Width, this.settings.IssueCount * issueControls.Last().Height + pMain.Top + pBottom.Height);

            var workingArea = Screen.FromControl(this).WorkingArea;
            if (this.Height > workingArea.Height)
                this.Height = workingArea.Height;

            if (this.Bottom > workingArea.Bottom)
                this.Top = workingArea.Bottom - this.Height;
            
            pMain.Height = ClientSize.Height - pTop.Height - pBottom.Height;
            pBottom.Top = ClientSize.Height - pBottom.Height;

            this.TopMost = this.settings.AlwaysOnTop;

            if (currentIssueIndex >= issueControls.Count())
                IssueSetCurrent(issueControls.Count() - 1);
            else
                IssueSetCurrent(currentIssueIndex);

            this.ResumeLayout(false);
            this.PerformLayout();
            UpdateIssuesOutput(true);
        }

        private void Issue_TimeEdited(object sender, EventArgs e)
        {

        }

        private void Issue_Selected(object sender, EventArgs e)
        {
            IssueSetCurrentByControl((IssueControl)sender);
        }

        private void IssueSetCurrentByControl(IssueControl control)
        {
            int i = 0;
            foreach (var issue in issueControls)
            {
                if (issue == control)
                {
                    IssueSetCurrent(i);
                    return;
                }
                i++;
            }
        }

        private void UpdateIssuesOutput(bool updateSummary = false)
        {
            foreach (var issue in this.issueControls)
                issue.UpdateOutput(updateSummary);
        }

        private void UpdateJiraRelatedData(bool firstTick)
        {
            Task.Factory.StartNew(
                () =>
                {
                    if (!IsJiraEnabled)
                    {
                        SetConnectionStatus(false);
                        return;
                    }

                    if (jiraClient.SessionValid || jiraClient.ValidateSession())
                    {
                        SetConnectionStatus(true);

                        this.InvokeIfRequired(
                            () =>
                            {
                                if (firstTick)
                                    LoadFilters();

                                UpdateIssuesOutput(firstTick);
                            }
                        );
                        return;
                    }

                    SetConnectionStatus(false);
                }
            );
        }


        private void SetConnectionStatus(bool connected)
        {
            this.InvokeIfRequired(
                () =>
                {
                    if (connected)
                    {
                        lblConnectionStatus.Text = "Connected";
                        lblConnectionStatus.ForeColor = Color.DarkGreen;
                        lblConnectionStatus.Font = new Font(lblConnectionStatus.Font, FontStyle.Regular);
                        lblConnectionStatus.Cursor = Cursors.Default;
                    }
                    else
                    {
                        lblConnectionStatus.Text = "Not connected";
                        lblConnectionStatus.ForeColor = Color.Tomato;
                        lblConnectionStatus.Font = new Font(lblConnectionStatus.Font, FontStyle.Regular | FontStyle.Underline);
                        lblConnectionStatus.Cursor = Cursors.Hand;
                    }
                }
            );
        }

        private void EditSettings()
        {
            using (var form = new SettingsForm(this.settings))
            {
                if (form.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    restClientFactory.BaseUrl = this.settings.JiraBaseUrl;
                    Logging.Logger.Instance.Enabled = settings.LoggingEnabled;
                    if (IsJiraEnabled)
                        AuthenticateJira(this.settings.Username, this.settings.ApiToken);
                    InitializeIssueControls();
                }
            }
        }


        private void SaveSettingsAndIssueStates()
        {
            settings.PersistedIssues.Clear();

            foreach (var issueControl in this.issueControls)
            {
                TimerState timerState = issueControl.WatchTimer.GetState();

                var persistedIssue = new PersistedIssue
                {
                    Key = issueControl.IssueKey,
                    TimerRunning = timerState.Running,
                    SessionStartTime = timerState.SessionStartTime,
                    InitialStartTime = timerState.InitialStartTime,
                    TotalTime = timerState.TotalTime,
                    Comment = issueControl.Comment,
                    EstimateUpdateMethod = issueControl.EstimateUpdateMethod,
                    EstimateUpdateValue = issueControl.EstimateUpdateValue
                };

                settings.PersistedIssues.Add(persistedIssue);
            }

            this.settings.Save();
        }


        private void LoadFilters()
        {
            Task.Factory.StartNew(
                () =>
                {
                    List<Filter> filters = jiraClient.GetFavoriteFilters();
                    if (filters == null)
                        return;

                    filters.Insert(0, new Filter
                    {
                        Id = -1,
                        Name = "My open issues",
                        Jql = "assignee = currentUser() AND resolution = Unresolved order by updated DESC"
                    });

                    if (filters.Count() > 1)
                    {
                        filters.Insert(1, new Filter
                        {
                            Id = 0,
                            Name = "--------------",
                            Jql = ""
                        });
                    }

                    this.InvokeIfRequired(
                        () =>
                        {
                            CBFilterItem currentItem = null;

                            cbFilters.Items.Clear();
                            foreach (var filter in filters)
                            {
                                var item = new CBFilterItem(filter.Id, filter.Name, filter.Jql);
                                cbFilters.Items.Add(item);
                                if (item.Id == this.settings.CurrentFilter)
                                   currentItem = item;
                            }

                            if (currentItem != null)
                                cbFilters.SelectedItem = currentItem;
                        }
                    );
                }
            );
        }


        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_SHOWME)
                ShowOnTop();

            base.WndProc(ref m);
        }


        private void ShowOnTop()
        {
            if (WindowState == FormWindowState.Minimized) {
                Show();
                WindowState = FormWindowState.Normal;
                notifyIcon.Visible = false;
            }

            // get our current "TopMost" value (ours will always be false though)
            // make our form jump to the top of everything
            // set it back to whatever it was
            bool top = TopMost;
            TopMost = true;
            TopMost = top;
        }


        private void CheckForUpdates()
        {
            if (!settings.CheckForUpdate)
                return;

            Task.Factory.StartNew(
                () =>
                {
                    GithubRelease latestRelease = ReleaseHelper.GetLatestVersion();
                    if (latestRelease == null)
                        return;

                    string currentVersion = Application.ProductVersion;
                    if (string.Compare(latestRelease.TagName, currentVersion) <= 0)
                        return;

                    this.InvokeIfRequired(
                        () =>
                        {
                            string msg = string.Format("There is a newer version available of Jira StopWatch.{0}{0}Latest release is {1}. You are running version {2}.{0}{0}Do you want to download latest release?",
                                Environment.NewLine,
                                latestRelease.TagName,
                                currentVersion);
                            if (MessageBox.Show(msg, "New version available", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                System.Diagnostics.Process.Start("https://github.com/mikee111/jirastopwatch/releases/latest");
                        }
                    );
                }
            );





        }
        #endregion


        #region private members
        private bool IsJiraEnabled
        {
            get
            {
                return !(
                    string.IsNullOrWhiteSpace(settings.JiraBaseUrl) ||
                    string.IsNullOrWhiteSpace(settings.Username) ||
                    string.IsNullOrWhiteSpace(settings.ApiToken)
                );
            }
        }

        private IEnumerable<IssueControl> issueControls
        {
            get
            {
                return this.pMain.Controls.OfType<IssueControl>();
            }
        }

        private Timer ticker;
		private Timer idleTicker;
        private Timer backupTicker;
        private Timer runningTicker;

        private WatchTimer backupTimer;

        private JiraApiRequestFactory jiraApiRequestFactory;
        private RestRequestFactory restRequestFactory;
        private JiraApiRequester jiraApiRequester;
        private RestClientFactory restClientFactory;
        private JiraClient jiraClient;

        private Settings settings;

        private IssueControl lastRunningIssue = null;
        #endregion


        #region private consts
        private const int firstDelay = 500;
        private const int defaultDelay = 30000;
        private const int maxIssues = 20;
		private const int maxIdleTime = 10*60*1000;
		private const int idleCheckInterval = 30*1000;
        private const int backupCheckInterval = 2*1000;
        private const int runningCheckInterval = 5*60*1000;
        #endregion

        private int currentIssueIndex;

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.Up))
            {
                IssueMoveUp();
                return true;
            }


            if (keyData == (Keys.Control | Keys.Down))
            {
                IssueMoveDown();
                return true;
            }

            if (keyData == (Keys.Control | Keys.P))
            {
                IssueTogglePlay();
                return true;
            }

            if (keyData == (Keys.Control | Keys.L))
            {
                IssuePostWorklog();
                return true;
            }

            if (keyData == (Keys.Control | Keys.E))
            {
                IssueEditTime();
                return true;
            }

            if (keyData == (Keys.Control | Keys.R))
            {
                IssueReset();
                return true;
            }

            if (keyData == (Keys.Control | Keys.Delete))
            {
                IssueDelete();
                return true;
            }

            if (keyData == (Keys.Control | Keys.I))
            {
                IssueFocusKey();
                return true;
            }

            if (keyData == (Keys.Control | Keys.N))
            {
                IssueAdd();
                return true;
            }

            if (keyData == (Keys.Control | Keys.C))
            {
                IssueCopyToClipboard();
                return true;
            }

            if (keyData == (Keys.Control | Keys.V))
            {
                IssuePasteFromClipboard();
                return true;
            }

            if (keyData == (Keys.Control | Keys.O))
            {
                IssueOpenInBrowser();
                return true;
            }

            if (keyData == (Keys.Alt | Keys.Down))
            {
                IssueOpenCombo();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }



        private void IssueOpenInBrowser()
        {
            issueControls.ToList()[currentIssueIndex].OpenJira();
        }

        private void IssueCopyToClipboard()
        {
            issueControls.ToList()[currentIssueIndex].CopyKeyToClipboard();
        }

        private void IssuePasteFromClipboard()
        {
            issueControls.ToList()[currentIssueIndex].PasteKeyFromClipboard();
        }

        private void IssueEditTime()
        {
            issueControls.ToList()[currentIssueIndex].EditTime();
        }

        private void IssueDelete()
        {
            issueControls.ToList()[currentIssueIndex].Remove();
        }

        private void IssueFocusKey()
        {
            issueControls.ToList()[currentIssueIndex].FocusKey();
        }

        private void IssueReset()
        {
            issueControls.ToList()[currentIssueIndex].Reset();
        }

        private void IssuePostWorklog()
        {
            issueControls.ToList()[currentIssueIndex].PostAndReset();
        }

        private void IssueTogglePlay()
        {
            issueControls.ToList()[currentIssueIndex].StartStop();
        }

        private void IssueMoveDown()
        {
            if (currentIssueIndex == issueControls.Count() - 1)
                return;

            IssueSetCurrent(currentIssueIndex + 1);
        }

        private void IssueMoveUp()
        {
            if (currentIssueIndex == 0)
                return;

            IssueSetCurrent(currentIssueIndex - 1);
        }

        private void IssueSetCurrent(int index)
        {
            currentIssueIndex = index;
            int i = 0;
            foreach (var issue in issueControls)
            {
                issue.Current = i == currentIssueIndex;
                if (i == currentIssueIndex)
                {
                    pMain.ScrollControlIntoView(issue);
                    issue.Focus();
                }
                i++;
            }
        }

        private void IssueOpenCombo()
        {
            issueControls.ToList()[currentIssueIndex].OpenCombo();
        }

        private void pbHelp_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://jirastopwatch.com/doc");
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("This will reset the backup timer. Do you want to also add the time to the currently running timer?", "Reset", MessageBoxButtons.YesNoCancel);
            if (dialogResult == DialogResult.Yes)
            {
                foreach (var issue in issueControls)
                {
                    issue.WatchTimer.AddWhenRunning(backupTimer.GetState().TotalTime.TotalMilliseconds);
                    issue.UpdateOutput();
                }

                ResetBackupTimer();
            }
            else if (dialogResult == DialogResult.No)
            {
                ResetBackupTimer();
            }
        }
    }

    // content item for the combo box
    public class CBFilterItem {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Jql { get; set; }

        public CBFilterItem(int id, string name, string jql) {
            Id = id;
            Name = name;
            Jql = jql;
        }
    }
}
