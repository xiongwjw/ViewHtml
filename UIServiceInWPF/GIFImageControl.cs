using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Diagnostics;

namespace UIServiceInWPF
{
    public class GIFImageControl : System.Windows.Controls.Image
    {
        public delegate void OnFrameChangedDelegate();

        private Bitmap m_Bitmap = null;
        private BitmapSource m_bitmapSource = null;
        private string m_fileName = null;
        private OnFrameChangedDelegate m_frameChangedDelegate = null;

        public GIFImageControl()
        {
            m_frameChangedDelegate = new OnFrameChangedDelegate(OnFrameChangedInMainThread);
            this.Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            this.Unloaded -= OnUnloaded;

            Reset();
        }    

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
            typeof(GIFImageControl),
            new PropertyMetadata(new PropertyChangedCallback(OnFileNamePropertyChanged)));

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
            (d as GIFImageControl).OnFileNameChanged(e);
        }

        protected void OnFileNameChanged(DependencyPropertyChangedEventArgs e)
        {
            string value = (string)e.NewValue;
            if (string.Equals(value, m_fileName, StringComparison.Ordinal))
            {
                return;
            }

            if (!string.IsNullOrEmpty(value))
            {
                if (Path.IsPathRooted(value))
                {
                    if (!File.Exists(value))
                    {
                        return;
                    }
                    m_fileName = value;
                }
                else
                {
                    string temp = AppDomain.CurrentDomain.BaseDirectory + value;
                    if (!File.Exists(temp))
                    {
                        return;
                    }
                    m_fileName = temp;
                }
            }

            try
            {
                Reset();

                m_Bitmap = (Bitmap)Image.FromFile(m_fileName);
                Width = m_Bitmap.Width;
                Height = m_Bitmap.Height;
                if (ImageAnimator.CanAnimate(m_Bitmap))
                {
                    ImageAnimator.Animate(m_Bitmap, OnFrameChanged);
                }
                //bitmapSource = GetBitmapSource();

                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(m_Bitmap.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(m_Bitmap.Width, m_Bitmap.Height));
                Source = bs;
            }
            catch (System.Exception ex)
            {
                Trace.TraceError(ex.Message);
                m_fileName = null;
            }
        }

        private void OnFrameChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                   m_frameChangedDelegate );
        }

        private void OnFrameChangedInMainThread()
        {
            ImageAnimator.UpdateFrames();

            if ( m_bitmapSource != null )
            {
                m_bitmapSource.Freeze();
            }

            m_bitmapSource = GetBitmapSource();
            if ( null == m_bitmapSource )
            {
                return;
            }

            Source = m_bitmapSource;

            InvalidateVisual();
        }
        //private static bool loaded;
        private BitmapSource GetBitmapSource()
        {
            try
            {
                IntPtr inptr = m_Bitmap.GetHbitmap();
                m_bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                    inptr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(inptr);

                return m_bitmapSource;
            }
            catch (System.Exception ex)
            {
                Trace.TraceError(ex.Message);

                return null;
            }
        }

        private void Reset()
        {
            try
            {
                if ( null != m_Bitmap )
                {
                    if ( ImageAnimator.CanAnimate(m_Bitmap) )
                    {
                        ImageAnimator.StopAnimate(m_Bitmap, 
                                                  OnFrameChanged);
                    }

                    m_Bitmap.Dispose();
                    m_Bitmap = null;
                }

                if ( null != m_bitmapSource )
                {
                    m_bitmapSource = null;
                }
            }
            catch (System.Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }

        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);
    }
}
