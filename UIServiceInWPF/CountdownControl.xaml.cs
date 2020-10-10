using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace UIServiceInWPF
{
    /// <summary>
    /// Interaction logic for CountdownControl.xaml
    /// </summary>
    public partial class CountdownControl : UserControl
    {
        #region     DependencyProperty
        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register("Duration", typeof(double), typeof(CountdownControl), new PropertyMetadata(new PropertyChangedCallback(OnDurationPropertyChanged)));
        /// <summary>
        /// Duration
        /// </summary>
        public double Duration
        {
            get { return (double)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        private static void OnDurationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as CountdownControl).OnDurationChanged(d, e);
        }

        public static readonly DependencyProperty TickProperty = DependencyProperty.Register("Tick", typeof(string), typeof(CountdownControl));
        /// <summary>
        /// Tick
        /// </summary>
        public string Tick
        {
            get { return GetValue(TickProperty) as string; }
            set { SetValue(TickProperty, value); }
        }

        public static readonly DependencyProperty DisplayFormatProperty = DependencyProperty.Register("DisplayFormat", typeof(string), typeof(CountdownControl), new PropertyMetadata("ss"));
        /// <summary>
        /// DisplayFormat
        /// </summary>
        public string DisplayFormat
        {
            get { return GetValue(DisplayFormatProperty) as string; }
            set { SetValue(DisplayFormatProperty, value); }
        }

        #endregion

        #region RoutedEvent

        public static readonly RoutedEvent CountdownCompletedEvent = EventManager.RegisterRoutedEvent("CountdownCompleted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CountdownControl));
        /// <summary>
        ///  CountdownCompleted event
        /// </summary>
        public event RoutedEventHandler CountdownCompleted
        {
            add { AddHandler(CountdownCompletedEvent, value); }
            remove { RemoveHandler(CountdownCompletedEvent, value); }
        }

        #endregion

        #region Variables

        private double _current;
        private DispatcherTimer _timer;

        #endregion

        public CountdownControl()
        {
            InitializeComponent();
            _timer = new DispatcherTimer(DispatcherPriority.DataBind, this.Dispatcher);
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += new EventHandler(Timer_Tick);
        }


        void Timer_Tick(object sender, EventArgs e)
        {
            _current += 1;
            UpdateTick();
            if (_current >= Math.Abs(Duration))
            {
                _timer.Stop();
                if (Math.Abs(Duration) > 0.000001)
                {
                    RaiseEvent(new RoutedEventArgs(CountdownCompletedEvent, this));
                    LogProcessorService.Log.UIService.LogInfo("Raise Event: CountdownCompletedEvent");
                }
            }
        }

        void UpdateTick()
        {
            double value = Duration < 0 ? _current : Math.Abs(Duration) - _current;
            if (Math.Abs(Duration) < 0.000001)
            {
                value = 0.0;
            }
            Tick = TimeSpan.FromSeconds(value).ToString(DisplayFormat);
            //LogProcessorService.Log.UIService.LogInfoFormat("Duration:{0}   Current:{1}    Value:{2}    Tick:{3}", Duration, _current, value, Tick);
        }

        protected void OnDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LogProcessorService.Log.UIService.LogInfoFormat("CountdownControl New Duration:{0}", e.NewValue);
            if (e.NewValue != null)
            {
                _current = 0;
                UpdateTick();
                _timer.Stop();
                if (Math.Abs(Duration) > 0.000001 && _timer != null)
                {
                    _timer.Start();
                }
            }
        }


    }
}
