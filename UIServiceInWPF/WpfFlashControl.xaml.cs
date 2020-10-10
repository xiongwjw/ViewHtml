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
using System.Runtime.InteropServices;

namespace UIServiceInWPF
{
    /// <summary>
    /// Interaction logic for FlashControl.xaml
    /// </summary>
    public partial class WpfFlashControl : UserControl
    {
        #region Dependency Property
        /// <summary>
        /// The <see cref="FileName" /> dependency property's name.
        /// </summary>
        public const string FileNamePropertyName = "FileName";

        /// <summary>
        /// Identifies the <see cref="FileName" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty FileNameProperty = DependencyProperty.Register(
            FileNamePropertyName,
            typeof(string),
            typeof(WpfFlashControl),
            new PropertyMetadata(new PropertyChangedCallback(OnFileNamePropertyChanged)));

        /// <summary>
        /// Gets or sets the value of the <see cref="FileName" />
        /// property. This is a dependency property.
        /// </summary>
        public string FileName
        {
            get
            {
                return GetValue(FileNameProperty) as string;
            }
            set
            {
                SetValue(FileNameProperty, value);
            }
        }

        private static void OnFileNamePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as WpfFlashControl).OnFileNameChanged(e);
        }


        #endregion

        private AxShockwaveFlashObjects.AxShockwaveFlash _flash;

        #region Disable Flash's Context Menu
        private const int GWL_WNDPROC = -4;
        public delegate IntPtr MyFlashWndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private IntPtr _oldWndProc = IntPtr.Zero;
        private MyFlashWndProc _wpr = null;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, MyFlashWndProc wndProc);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr CallWindowProc(IntPtr wndProc, IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private IntPtr FlashWndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == 516)
                return (IntPtr)0;
            return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
        }
        #endregion

        public WpfFlashControl()
        {
            InitializeComponent();
            IninitFlashComponent();
            this._wpr = new MyFlashWndProc(this.FlashWndProc);
            this._oldWndProc = SetWindowLong(_flash.Handle, GWL_WNDPROC, _wpr);
        }

        private void IninitFlashComponent()
        {
            try
            {
                _flash = new AxShockwaveFlashObjects.AxShockwaveFlash();
                _flash.BeginInit();
                _flash.EndInit();   
                FormHost.Child = _flash;
                _flash.Visible = false;
                //_flash.WMode = "transparent";
                //_flash.Loop = true;
                _flash.OnReadyStateChange += new AxShockwaveFlashObjects._IShockwaveFlashEvents_OnReadyStateChangeEventHandler(_flash_OnReadyStateChange);
            }
            catch (Exception ex)
            {
                LogProcessorService.Log.UIService.LogError("Ininit Flash Component Failed. ", ex);
            }
        }

        void _flash_OnReadyStateChange(object sender, AxShockwaveFlashObjects._IShockwaveFlashEvents_OnReadyStateChangeEvent e)
        {
            LogProcessorService.Log.UIService.LogInfoFormat("WpfFlashControl - ReadyState: {0}", e.newState);
            if (e.newState == 4)
            {
                _flash.Visible = true;
            }

        }

        private string _currentFile;
        protected void OnFileNameChanged(DependencyPropertyChangedEventArgs e)
        {
            if (_flash == null) return;

            LogProcessorService.Log.UIService.LogInfoFormat("WpfFlashControl - New FileName: {0}", e.NewValue);

            if (e.NewValue != null)
            {
                _flash.Visible = false;
                _currentFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, (e.NewValue as string));
            }
            _flash.Movie = _currentFile;
            _flash.Play();
        }


    }
}
