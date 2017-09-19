namespace ACT.MPTimer
{
    using System.ComponentModel;
    using System.ComponentModel.Composition;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Media;

    using ACT.MPTimer.Properties;
    using ACT.MPTimer.Utility;

    [Export(typeof(MPTimerWindowViewModel))]
    public class MPTimerWindowViewModel : INotifyPropertyChanged
    {
        private SolidColorBrush fontFill;
        private SolidColorBrush fontStroke;
        private bool inCombat;
        private double opacity;
        private SolidColorBrush progressBarBackground;
        private SolidColorBrush progressBarBackgroundDefault;
        private SolidColorBrush progressBarBackgroundShift;
        private SolidColorBrush progressBarForeground;
        private SolidColorBrush progressBarForegroundDefault;
        private SolidColorBrush progressBarForegroundShift;
        private double progressBarForegroundWidth;
        //private double oldprogressBarForegroundWidth;
       // private int zInd;
        private SolidColorBrush progressBarStroke;
        private SolidColorBrush progressBarStrokeDefault;
        private SolidColorBrush progressBarStrokeShift;
        private double timeToRecovery = default(double);
        private double maxMP;
        private string timeToRecoveryText;
        private bool visible;
        private bool rev;

        public MPTimerWindowViewModel()
        {
            this.ReloadSettings();

#if DEBUG
            this.visible = true;
            this.inCombat = true;
#endif
        }

        public FontFamily FontFamily
        {
            get { return Settings.Default.Font.ToFontFamilyWPF(); }
        }

        public SolidColorBrush FontFill
        {
            get { return this.fontFill; }
        }

        public double FontSize
        {
            get { return Settings.Default.Font.ToFontSizeWPF(); }
        }

        public SolidColorBrush FontStroke
        {
            get { return this.fontStroke; }
        }

        public double FontStrokeThickness
        {
            get { return 0.5d * this.FontSize / 13.0d; }
        }

        public FontStyle FontStyle
        {
            get { return Settings.Default.Font.ToFontStyleWPF(); }
        }

        public FontWeight FontWeight
        {
            get { return Settings.Default.Font.ToFontWeightWPF(); }
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

        public double Left
        {
            get { return (double)Settings.Default.OverlayLeft; }
            set
            {
                Settings.Default.OverlayLeft = (int)value;
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

        public SolidColorBrush ProgressBarBackground
        {
            get { return this.progressBarBackground; }
        }

        public SolidColorBrush ProgressBarForeground
        {
            get { return this.progressBarForeground; }
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
        /*public double oldProgressBarForegroundWidth
        {
            get { return this.oldprogressBarForegroundWidth; }
            set
            {
                if (this.oldprogressBarForegroundWidth != value)
                {
                    this.oldprogressBarForegroundWidth = value;
                    this.RaisePropertyChanged();
                }
            }
        }*/
        public double ProgressBarHeight
        {
            get { return Settings.Default.ProgressBarSize.Height; }
        }

        public SolidColorBrush ProgressBarStroke
        {
            get { return this.progressBarStroke; }
        }

        public double ProgressBarWidth
        {
            get { return Settings.Default.ProgressBarSize.Width; }
        }
        public double MaxMP
        {
            get
            {
                return this.MaxMP;
            }
            set
            {   if (this.maxMP != value)
                {
                    this.maxMP = value;
                }
            }
        }
        /*public void updateBar()
        {
            if (this.rev)
            {
                this.progressBarForegroundWidth = this.oldProgressBarForegroundWidth;
                this.rev = false;
            }
            else if(this.oldprogressBarForegroundWidth <= this.progressBarForegroundWidth )
            {
                this.oldprogressBarForegroundWidth = this.progressBarForegroundWidth;
            }
        }*/
        public double TimeToRecovery
        {
            get { return this.timeToRecovery; }
            set
            {
                if (this.timeToRecovery != value)
                {
                    this.timeToRecovery = value;

                    // プログレスバーの幅を計算する
                    var rateOfRecovery =
                        (this.timeToRecovery) /
                        (this.maxMP);
                    /*if (this.ProgressBarForegroundWidth > (double)Settings.Default.ProgressBarSize.Width * rateOfRecovery) //Losing MP
                    {
                        this.rev = true;
                        this.oldProgressBarForegroundWidth = (double)Settings.Default.ProgressBarSize.Width * rateOfRecovery; 
                    }
                    else {*/
                        //this.oldProgressBarForegroundWidth = this.ProgressBarForegroundWidth;
                        this.ProgressBarForegroundWidth =
                            (double)Settings.Default.ProgressBarSize.Width * rateOfRecovery;
                    //}
                    

                    // 残り秒数の表示を編集する
                    this.TimeToRecoveryText =
                        (this.timeToRecovery).ToString() + " MP";

                    // 残り秒数でプログレスバーの色を変更する
                    if (Settings.Default.ProgressBarShiftTime > 0.0d)
                    {
                        var fore = default(SolidColorBrush);
                        var back = default(SolidColorBrush);
                        var stroke = default(SolidColorBrush);

                        if (this.timeToRecovery <= (Settings.Default.ProgressBarShiftTime * 1000d))
                        {
                            fore = this.progressBarForegroundShift;
                            back = this.progressBarBackgroundShift;
                            stroke = this.progressBarStrokeShift;
                        }
                        else
                        {
                            fore = this.progressBarForegroundDefault;
                            back = this.progressBarBackgroundDefault;
                            stroke = this.progressBarStrokeDefault;
                        }

                        if (this.progressBarForeground != fore ||
                            this.progressBarBackground != back ||
                            this.progressBarStroke != stroke)
                        {
                            this.progressBarForeground = fore;
                            this.progressBarBackground = back;
                            this.progressBarStroke = stroke;

                            this.RaisePropertyChanged("ProgressBarForeground");
                            this.RaisePropertyChanged("ProgressBarBackground");
                            this.RaisePropertyChanged("ProgressBarStroke");
                        }
                    }
                }
            }
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

        public double Top
        {
            get { return (double)Settings.Default.OverlayTop; }
            set
            {
                Settings.Default.OverlayTop = (int)value;
                this.RaisePropertyChanged();
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

        public void ReloadSettings()
        {
            this.opacity = (100.0d - Settings.Default.OverlayOpacity) / 100.0d;

            this.progressBarForegroundDefault = new SolidColorBrush(Settings.Default.ProgressBarColor.ToWPF());
            this.progressBarBackgroundDefault = new SolidColorBrush(this.progressBarForegroundDefault.Color.ChangeBrightness(0.4d));
            this.progressBarStrokeDefault = new SolidColorBrush(Settings.Default.ProgressBarOutlineColor.ToWPF());
            this.progressBarForegroundShift = new SolidColorBrush(Settings.Default.ProgressBarShiftColor.ToWPF());
            this.progressBarBackgroundShift = new SolidColorBrush(this.progressBarForegroundShift.Color.ChangeBrightness(0.4d));
            this.progressBarStrokeShift = new SolidColorBrush(Settings.Default.ProgressBarOutlineShiftColor.ToWPF());

            this.fontFill = new SolidColorBrush(Settings.Default.FontColor.ToWPF());
            this.fontStroke = new SolidColorBrush(Settings.Default.FontOutlineColor.ToWPF());

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

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }

        #endregion Implementation of INotifyPropertyChanged
    }
}
