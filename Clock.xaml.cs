using System;
using System.Windows;
using System.Windows.Threading;

namespace desktopmascot
{
    /// <summary>
    /// Clock.xaml の相互作用ロジック
    /// </summary>
    public partial class Clock : Window
    {
        public Clock()
        {
            InitializeComponent();

            var desktop = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
            this.Top = 150;
            this.Left = desktop.Width - 150;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Update();
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Normal, this.Dispatcher);
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        void Update()
        {
            DateTime dt = DateTime.Now;
            this.AngleSecond.Angle = dt.Second * 360.0 / 60.0;
            this.AngleMinute.Angle = (dt.Minute + dt.Second / 60.0) * 360.0 / 60.0;
            this.AngleHour.Angle = (dt.Hour + dt.Minute / 60.0) * 360.0 / 12;
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            Update();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
