namespace ACT.MPTimer
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Threading;

    using ACT.MPTimer.Properties;

    /// <summary>
    /// MPTimer Window
    /// </summary>
    public partial class EnochianTimerWindow : Window
    {
        private static object lockObject = new object();

        private static EnochianTimerWindow instance;

        public static EnochianTimerWindow Default
        {
            get { lock (lockObject) { return instance ?? (instance = new EnochianTimerWindow()); } }
        }

        public static void Reload()
        {
            lock (lockObject)
            {
                if (instance != null)
                {
                    instance.Close();
                    instance = null;
                }

                instance = new EnochianTimerWindow();
            }
        }

        public EnochianTimerWindow()
        {
            this.InitializeComponent();

            this.ViewModel = this.DataContext as EnochianTimerWindowViewModel;

            if (Settings.Default.ClickThrough)
            {
                this.ToTransparentWindow();
            }

            this.MouseLeftButtonDown += (s, e) =>
            {
                this.DragMove();
            };

            this.Loaded += (s, e) =>
            {
                // 裏面防止用タイマを開始する
                var timer = new DispatcherTimer()
                {
                    Interval = new TimeSpan(0, 0, 0, 3, 0),
                };

                timer.Tick += (s1, e1) =>
                {
                    if (this.Opacity > 0.0d)
                    {
                        this.Topmost = false;
                        this.Topmost = true;
                    }
                };

                timer.Start();

                // プログレスバーの更新タイマを開始する
                var updateTimer = new DispatcherTimer()
                {
                    Interval = new TimeSpan(0, 0, 0, 0, 10),
                };

                updateTimer.Tick += (s2, e2) =>
                {
                    if (this.ViewModel != null)
                    {
                        this.ViewModel.UpdateProgress();
                    }
                };

                updateTimer.Start();
            };

            Trace.WriteLine("New EnochianTimerOverlay.");
        }

        public EnochianTimerWindowViewModel ViewModel
        {
            get;
            private set;
        }
    }
}
