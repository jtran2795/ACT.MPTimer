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
    public partial class MPTimerWindow : Window
    {
        private static object lockObject = new object();

        private static MPTimerWindow instance;

        public static MPTimerWindow Default
        {
            get { lock (lockObject) { return instance ?? (instance = new MPTimerWindow()); } }
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

                instance = new MPTimerWindow();
            }
        }

        public MPTimerWindow()
        {
            this.InitializeComponent();

            this.ViewModel = this.DataContext as MPTimerWindowViewModel;

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
            };

            Trace.WriteLine("New MPTimerOverlay.");
        }

        public MPTimerWindowViewModel ViewModel
        {
            get;
            private set;
        }
    }
}
