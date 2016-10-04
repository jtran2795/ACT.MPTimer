namespace ACT.MPTimer
{
    using System;
    using System.Diagnostics;
    using System.Windows.Forms;
    using System.Windows.Threading;
    using ACT.MPTimer.Properties;
    using ACT.MPTimer.Utility;

    /// <summary>
    /// 設定Panel
    /// </summary>
    public partial class ConfigPanel : UserControl
    {
        private static ConfigPanel instance;

        public static ConfigPanel Default
        {
            get { return instance ?? (instance = new ConfigPanel()); }
        }

        private ColorDialog colorDialog = new ColorDialog()
        {
            AnyColor = true,
            FullOpen = true,
        };

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ConfigPanel()
        {
            this.InitializeComponent();
            this.Dock = DockStyle.Fill;

            this.ProgressBarShiftTimeNumericUpDown.Maximum = (decimal)Settings.Default.MPRecoveryInterval;

            TraceUtility.LogTextBox = this.LogRichTextBox;
        }

        /// <summary>
        /// Load
        /// </summary>
        /// <param name="sender">イベント発生元</param>
        /// <param name="e">イベント引数</param>
        private void ConfigPanel_Load(object sender, EventArgs e)
        {
            this.TargetJobComboBox.DataSource = Job.GetJobList();
            this.TargetJobComboBox.ValueMember = "JobId";
            this.TargetJobComboBox.DisplayMember = "JobName";

            this.LoadSettings();

            this.TekiyoButton.Click += (s1, e1) =>
            {
                try
                {
                    FF14Watcher.Deinitialize();

                    Settings.Default.OverlayTop = (int)MPTimerWindow.Default.Top;
                    Settings.Default.OverlayLeft = (int)MPTimerWindow.Default.Left;
                    Settings.Default.EnochianOverlayTop = (int)EnochianTimerWindow.Default.Top;
                    Settings.Default.EnochianOverlayLeft = (int)EnochianTimerWindow.Default.Left;

                    this.SaveSettings();

                    FF14Watcher.Default.UpdateTargetJobId();

                    MPTimerWindow.Reload();
                    MPTimerWindow.Default.Show();
                    EnochianTimerWindow.Reload();
                    EnochianTimerWindow.Default.Show();

                    Trace.WriteLine("Change settings.");
                }
                finally
                {
                    FF14Watcher.Initialize();
                }
            };

            this.ShokikaButton.Click += (s1, e1) =>
            {
                try
                {
                    FF14Watcher.Deinitialize();

                    Settings.Default.Reset();
                    Settings.Default.Save();

                    MPTimerWindow.Default.Top = Settings.Default.OverlayTop;
                    MPTimerWindow.Default.Left = Settings.Default.OverlayLeft;
                    EnochianTimerWindow.Default.Top = Settings.Default.EnochianOverlayTop;
                    EnochianTimerWindow.Default.Left = Settings.Default.EnochianOverlayLeft;

                    this.LoadSettings();

                    FF14Watcher.Default.UpdateTargetJobId();

                    MPTimerWindow.Reload();
                    MPTimerWindow.Default.Show();
                    EnochianTimerWindow.Reload();
                    EnochianTimerWindow.Default.Show();

                    Trace.WriteLine("Reset settings.");
                }
                finally
                {
                    FF14Watcher.Initialize();
                }
            };

            this.ProgressBarShiftColorButton.Click += (s1, e1) =>
            {
                var button = s1 as Button;
                this.colorDialog.Color = button.BackColor;
                if (this.colorDialog.ShowDialog(this.ParentForm) == DialogResult.OK)
                {
                    button.BackColor = this.colorDialog.Color;
                }
            };

            this.ProgressBarShiftOutlineColorButton.Click += (s1, e1) =>
            {
                var button = s1 as Button;
                this.colorDialog.Color = button.BackColor;
                if (this.colorDialog.ShowDialog(this.ParentForm) == DialogResult.OK)
                {
                    button.BackColor = this.colorDialog.Color;
                }
            };

            this.EnochianBarShiftColorButton.Click += (s1, e1) =>
            {
                var button = s1 as Button;
                this.colorDialog.Color = button.BackColor;
                if (this.colorDialog.ShowDialog(this.ParentForm) == DialogResult.OK)
                {
                    button.BackColor = this.colorDialog.Color;
                }
            };

            this.EnochianBarShiftOutlineColorButton.Click += (s1, e1) =>
            {
                var button = s1 as Button;
                this.colorDialog.Color = button.BackColor;
                if (this.colorDialog.ShowDialog(this.ParentForm) == DialogResult.OK)
                {
                    button.BackColor = this.colorDialog.Color;
                }
            };

            this.OverlayLocationXNumericUpDown.DataBindings.Add(
                new Binding("Value", MPTimerWindow.Default.ViewModel, "Left", false, DataSourceUpdateMode.OnPropertyChanged));
            this.OverlayLocationYNumericUpDown.DataBindings.Add(
                new Binding("Value", MPTimerWindow.Default.ViewModel, "Top", false, DataSourceUpdateMode.OnPropertyChanged));
            this.EnochianXNumericUpDown.DataBindings.Add(
                new Binding("Value", EnochianTimerWindow.Default.ViewModel, "Left", false, DataSourceUpdateMode.OnPropertyChanged));
            this.EnochianYNumericUpDown.DataBindings.Add(
                new Binding("Value", EnochianTimerWindow.Default.ViewModel, "Top", false, DataSourceUpdateMode.OnPropertyChanged));
        }

        /// <summary>
        /// 設定をLoadする
        /// </summary>
        private void LoadSettings()
        {
            this.VisualSetting.BarColor = Settings.Default.ProgressBarColor;
            this.VisualSetting.BarOutlineColor = Settings.Default.ProgressBarOutlineColor;
            this.VisualSetting.TextFont = Settings.Default.Font;
            this.VisualSetting.FontColor = Settings.Default.FontColor;
            this.VisualSetting.FontOutlineColor = Settings.Default.FontOutlineColor;
            this.VisualSetting.BarSize = Settings.Default.ProgressBarSize;
            this.VisualSetting.RefreshSampleImage();

            this.EnochianVisualSetting.BarColor = Settings.Default.EnochianProgressBarColor;
            this.EnochianVisualSetting.BarOutlineColor = Settings.Default.EnochianProgressBarOutlineColor;
            this.EnochianVisualSetting.TextFont = Settings.Default.EnochianFont;
            this.EnochianVisualSetting.FontColor = Settings.Default.EnochianFontColor;
            this.EnochianVisualSetting.FontOutlineColor = Settings.Default.EnochianFontOutlineColor;
            this.EnochianVisualSetting.BarSize = Settings.Default.EnochianProgressBarSize;
            this.EnochianVisualSetting.RefreshSampleImage();

            this.TokaRitsuNumericUpDown.Value = Settings.Default.OverlayOpacity;

            this.CountInCombatCheckBox.Checked = Settings.Default.CountInCombat;
            this.CountInCombatNumericUpDown.Value = Settings.Default.CountInCombatSpan;
            this.TargetJobComboBox.SelectedValue = Settings.Default.TargetJobId;
            this.ClickThroughCheckBox.Checked = Settings.Default.ClickThrough;

            this.MPRefreshRateNumericUpDown.Value = Settings.Default.ParameterRefreshRate;

            this.ProgressBarShiftTimeNumericUpDown.Value = (decimal)Settings.Default.ProgressBarShiftTime;
            this.ProgressBarShiftColorButton.BackColor = Settings.Default.ProgressBarShiftColor;
            this.ProgressBarShiftOutlineColorButton.BackColor = Settings.Default.ProgressBarOutlineShiftColor;

            this.EnochianBarShiftNumericUpDown.Value = (decimal)Settings.Default.EnochianProgressBarShiftTime;
            this.EnochianBarShiftColorButton.BackColor = Settings.Default.EnochianProgressBarShiftColor;
            this.EnochianBarShiftOutlineColorButton.BackColor = Settings.Default.EnochianProgressBarOutlineShiftColor;

            this.EnochianLabelTextBox.Text = Settings.Default.EnochianLabel;

            this.EnabledMPTimerCheckBox.Checked = Settings.Default.EnabledMPTimer;
            this.EnabledEnochianTimerCheckBox.Checked = Settings.Default.EnabledEnochianTimer;
        }

        /// <summary>
        /// 設定をSaveする
        /// </summary>
        private void SaveSettings()
        {
            Settings.Default.ProgressBarColor = this.VisualSetting.BarColor;
            Settings.Default.ProgressBarOutlineColor = this.VisualSetting.BarOutlineColor;
            Settings.Default.Font = this.VisualSetting.TextFont;
            Settings.Default.FontColor = this.VisualSetting.FontColor;
            Settings.Default.FontOutlineColor = this.VisualSetting.FontOutlineColor;
            Settings.Default.ProgressBarSize = this.VisualSetting.BarSize;

            Settings.Default.EnochianProgressBarColor = this.EnochianVisualSetting.BarColor;
            Settings.Default.EnochianProgressBarOutlineColor = this.EnochianVisualSetting.BarOutlineColor;
            Settings.Default.EnochianFont = this.EnochianVisualSetting.TextFont;
            Settings.Default.EnochianFontColor = this.EnochianVisualSetting.FontColor;
            Settings.Default.EnochianFontOutlineColor = this.EnochianVisualSetting.FontOutlineColor;
            Settings.Default.EnochianProgressBarSize = this.EnochianVisualSetting.BarSize;

            Settings.Default.OverlayOpacity = (int)this.TokaRitsuNumericUpDown.Value;

            Settings.Default.CountInCombat = this.CountInCombatCheckBox.Checked;
            Settings.Default.CountInCombatSpan = (int)this.CountInCombatNumericUpDown.Value;
            Settings.Default.TargetJobId = (int)this.TargetJobComboBox.SelectedValue;
            Settings.Default.ClickThrough = this.ClickThroughCheckBox.Checked;

            Settings.Default.ParameterRefreshRate = (int)this.MPRefreshRateNumericUpDown.Value;

            Settings.Default.ProgressBarShiftTime = (double)this.ProgressBarShiftTimeNumericUpDown.Value;
            Settings.Default.ProgressBarShiftColor = this.ProgressBarShiftColorButton.BackColor;
            Settings.Default.ProgressBarOutlineShiftColor = this.ProgressBarShiftOutlineColorButton.BackColor;

            Settings.Default.EnochianProgressBarShiftTime = (double)this.EnochianBarShiftNumericUpDown.Value;
            Settings.Default.EnochianProgressBarShiftColor = this.EnochianBarShiftColorButton.BackColor;
            Settings.Default.EnochianProgressBarOutlineShiftColor = this.EnochianBarShiftOutlineColorButton.BackColor;

            Settings.Default.EnochianLabel = this.EnochianLabelTextBox.Text;

            Settings.Default.EnabledMPTimer = this.EnabledMPTimerCheckBox.Checked;
            Settings.Default.EnabledEnochianTimer = this.EnabledEnochianTimerCheckBox.Checked;

            Settings.Default.Save();
        }
    }
}
