namespace ACT.MPTimer
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Composition;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Media;

    using ACT.MPTimer.Properties;
    using ACT.MPTimer.Utility;

    [Export(typeof(EnochianTimerWindowViewModel))]
    public class EnochianTimerWindowViewModel : INotifyPropertyChanged
    {
        private double opacity;
        private SolidColorBrush progressBarForeground;
        private SolidColorBrush progressBarBackground;
        private SolidColorBrush progressBarStroke;
        private SolidColorBrush fontFill;
        private SolidColorBrush fontStroke;
        private double progressBarForegroundWidth;
        private string timeToRecoveryText;
        private bool inCombat;
        private bool visible;
        private DateTime endScheduledDateTime;
        private DateTime startDateTime;

        private SolidColorBrush progressBarForegroundDefault;
        private SolidColorBrush progressBarBackgroundDefault;
        private SolidColorBrush progressBarStrokeDefault;
        private SolidColorBrush progressBarForegroundShift;
        private SolidColorBrush progressBarBackgroundShift;
        private SolidColorBrush progressBarStrokeShift;

        public EnochianTimerWindowViewModel()
        {
            this.ReloadSettings();
            this.InCombat = true;
            this.Visible = true;
        }

        public string Label
        {
            get { return Settings.Default.EnochianLabel; }
            set
            {
                Settings.Default.EnochianLabel = value;
                this.RaisePropertyChanged();
            }
        }

        public double Left
        {
            get { return (double)Settings.Default.EnochianOverlayLeft; }
            set
            {
                Settings.Default.EnochianOverlayLeft = (int)value;
                this.RaisePropertyChanged();
            }
        }

        public double Top
        {
            get { return (double)Settings.Default.EnochianOverlayTop; }
            set
            {
                Settings.Default.EnochianOverlayTop = (int)value;
                this.RaisePropertyChanged();
            }
        }

        public double Opacity
        {
            get
            {
                if (!this.visible)
                {
                    return 0.0d;
                }

                if (Settings.Default.CountInCombat &&
                    !this.inCombat)
                {
                    return 0.0d;
                }

                return this.opacity;
            }
        }

        public bool InCombat
        {
            get { return this.inCombat; }
            set
            {
                if (this.inCombat != value)
                {
                    this.inCombat = value;
                    this.RaisePropertyChanged("Opacity");
                }
            }
        }

        public bool Visible
        {
            get { return this.visible; }
            set
            {
                if (this.visible != value)
                {
                    this.visible = value;
                    this.RaisePropertyChanged("Opacity");
                }
            }
        }

        public DateTime StartDateTime
        {
            get { return this.startDateTime; }
            set
            {
                this.startDateTime = value;
            }
        }

        public DateTime EndScheduledDateTime
        {
            get { return this.endScheduledDateTime; }
            set
            {
                this.endScheduledDateTime = value;
                this.UpdateProgress();
            }
        }

        public SolidColorBrush ProgressBarForeground
        {
            get { return this.progressBarForeground; }
        }


        public SolidColorBrush ProgressBarBackground
        {
            get { return this.progressBarBackground; }
        }

        public SolidColorBrush ProgressBarStroke
        {
            get { return this.progressBarStroke; }
        }

        public double ProgressBarWidth
        {
            get { return Settings.Default.EnochianProgressBarSize.Width; }
        }

        public double ProgressBarHeight
        {
            get { return Settings.Default.EnochianProgressBarSize.Height; }
        }

        public double ProgressBarForegroundWidth
        {
            get { return this.progressBarForegroundWidth; }
            set
            {
                if (this.progressBarForegroundWidth != value)
                {
                    this.progressBarForegroundWidth = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public FontFamily FontFamily
        {
            get { return Settings.Default.EnochianFont.ToFontFamilyWPF(); }
        }

        public double FontSize
        {
            get { return Settings.Default.EnochianFont.ToFontSizeWPF(); }
        }

        public FontStyle FontStyle
        {
            get { return Settings.Default.EnochianFont.ToFontStyleWPF(); }
        }

        public FontWeight FontWeight
        {
            get { return Settings.Default.EnochianFont.ToFontWeightWPF(); }
        }

        public SolidColorBrush FontFill
        {
            get { return this.fontFill; }
        }

        public SolidColorBrush FontStroke
        {
            get { return this.fontStroke; }
        }

        public double FontStrokeThickness
        {
            get { return 0.5d * this.FontSize / 13.0d; }
        }

        public string TimeToRecoveryText
        {
            get { return this.timeToRecoveryText; }
            set
            {
                if (this.timeToRecoveryText != value)
                {
                    this.timeToRecoveryText = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public void ReloadSettings()
        {
            this.opacity = (100.0d - Settings.Default.OverlayOpacity) / 100.0d;

            this.progressBarForegroundDefault = new SolidColorBrush(Settings.Default.EnochianProgressBarColor.ToWPF());
            this.progressBarBackgroundDefault = new SolidColorBrush(this.progressBarForegroundDefault.Color.ChangeBrightness(0.4d));
            this.progressBarStrokeDefault = new SolidColorBrush(Settings.Default.EnochianProgressBarOutlineColor.ToWPF());
            this.progressBarForegroundShift = new SolidColorBrush(Settings.Default.EnochianProgressBarShiftColor.ToWPF());
            this.progressBarBackgroundShift = new SolidColorBrush(this.progressBarForegroundShift.Color.ChangeBrightness(0.4d));
            this.progressBarStrokeShift = new SolidColorBrush(Settings.Default.EnochianProgressBarOutlineShiftColor.ToWPF());

            this.fontFill = new SolidColorBrush(Settings.Default.EnochianFontColor.ToWPF());
            this.fontStroke = new SolidColorBrush(Settings.Default.EnochianFontOutlineColor.ToWPF());

            this.progressBarForegroundDefault.Freeze();
            this.progressBarBackgroundDefault.Freeze();
            this.progressBarStrokeDefault.Freeze();
            this.progressBarForegroundShift.Freeze();
            this.progressBarBackgroundShift.Freeze();
            this.progressBarStrokeShift.Freeze();
            this.fontFill.Freeze();
            this.fontStroke.Freeze();

            this.progressBarForeground = this.progressBarForegroundDefault;
            this.progressBarBackground = this.progressBarBackgroundDefault;
            this.progressBarStroke = this.progressBarStrokeDefault;

            this.RaisePropertyChanged("Opacity");
            this.RaisePropertyChanged("ProgressBarForegroundWidth");
            this.RaisePropertyChanged("ProgressBarForeground");
            this.RaisePropertyChanged("ProgressBarBackground");
            this.RaisePropertyChanged("ProgressBarStroke");
            this.RaisePropertyChanged("ProgressBarWidth");
            this.RaisePropertyChanged("ProgressBarHeight");
            this.RaisePropertyChanged("ProgressBarForeWidth");
            this.RaisePropertyChanged("FontFamily");
            this.RaisePropertyChanged("FontSize");
            this.RaisePropertyChanged("FontStyle");
            this.RaisePropertyChanged("FontWeight");
            this.RaisePropertyChanged("FontFill");
            this.RaisePropertyChanged("FontStroke");
            this.RaisePropertyChanged("FontStrokeThickness");
        }

        public void UpdateProgress()
        {
            var now = DateTime.Now;

            var duration = (this.EndScheduledDateTime - this.StartDateTime).TotalSeconds;
            var durationRate = duration / FF14Watcher.EnochianDuration;
            var durationRemain = (this.EndScheduledDateTime - now).TotalSeconds;

            if (durationRemain < 0)
            {
                durationRemain = 0;
            }

            var durationRemainRate = durationRemain / duration;

            if (this.StartDateTime <= DateTime.MinValue ||
                this.EndScheduledDateTime <= DateTime.MinValue)
            {
                if (Settings.Default.CountInCombat)
                {
                    this.Visible = false;
                    return;
                }
            }

            this.ProgressBarForegroundWidth = (double)Settings.Default.EnochianProgressBarSize.Width * durationRate * durationRemainRate;

            if (this.EndScheduledDateTime < now)
            {
                this.TimeToRecoveryText = "Over";

                if (Settings.Default.CountInCombat)
                {
                    if ((now - this.EndScheduledDateTime).TotalSeconds > 1.0d)
                    {
                        this.Visible = false;
                        return;
                    }
                }
            }
            else
            {
                this.TimeToRecoveryText = durationRemain.ToString("N1");
            }

            // 残り秒数でプログレスバーの色を変更する
            if (Settings.Default.EnochianProgressBarShiftTime > 0.0d)
            {
                if (durationRemain <= Settings.Default.EnochianProgressBarShiftTime)
                {
                    this.progressBarForeground = this.progressBarForegroundShift;
                    this.progressBarBackground = this.progressBarBackgroundShift;
                    this.progressBarStroke = this.progressBarStrokeShift;
                }
                else
                {
                    this.progressBarForeground = this.progressBarForegroundDefault;
                    this.progressBarBackground = this.progressBarBackgroundDefault;
                    this.progressBarStroke = this.progressBarStrokeDefault;
                }

                this.RaisePropertyChanged("ProgressBarForeground");
                this.RaisePropertyChanged("ProgressBarBackground");
                this.RaisePropertyChanged("ProgressBarStroke");
            }

            if (Settings.Default.EnabledEnochianTimer &&
                FF14Watcher.Default.EnabledByJobFilter)
            {
                this.Visible = true;
            }
        }

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }

        #endregion
    }
}
