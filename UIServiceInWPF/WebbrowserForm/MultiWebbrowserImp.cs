using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LogProcessorService;
using UIServiceInWPF.Interop;
using System.Diagnostics;

namespace UIServiceInWPF
{
    public delegate void NavigateCompletedHandle( object argSender,
                                                  string argUrl );

    public partial class MultiWebbrowserImp : Form
    {
        private int _m_fCount = 0;
        private int _m_bCount = 0;
        public MultiWebbrowserImp()
        {
            InitializeComponent();

            m_javaObject = new JavaScriptObject(HtmlRender.SingleInstance);

            m_frontBrowserHost = new WebBrowserHost();
            m_backBrowserHost = new WebBrowserHost();

            m_frontBrowserHost.JavaScript = m_javaObject;
            m_backBrowserHost.JavaScript = m_javaObject;

            m_frontBrowserHost.MdiParent = this;
            m_backBrowserHost.MdiParent = this;

            m_frontBrowserHost.Dock = DockStyle.Fill;
            m_backBrowserHost.Dock = DockStyle.Fill;

            m_frontBrowserHost.DocumentCompleted += m_BrowserHost_DocumentCompleted;
            m_backBrowserHost.DocumentCompleted += m_BrowserHost_DocumentCompleted;

            m_frontBrowserHost.Show();
            m_backBrowserHost.Show();
            m_curBrowserHost = m_backBrowserHost;

            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);

            MdiClient client = null;
            for (int i = 0; i < this.Controls.Count; ++i )
            {
                client = this.Controls[i] as MdiClient;
                if ( null != client )
                {
                    int style = NativeWndApi.GetWindowLong(client.Handle, NativeWndApi.GWL_STYLE);
                    int exStyle = NativeWndApi.GetWindowLong(client.Handle, NativeWndApi.GWL_EXSTYLE);
                    style &= ~NativeWndApi.WS_BORDER;
                    exStyle &= ~NativeWndApi.WS_EX_CLIENTEDGE;

                    NativeWndApi.SetWindowLong(client.Handle, NativeWndApi.GWL_STYLE, style);
                    NativeWndApi.SetWindowLong(client.Handle, NativeWndApi.GWL_EXSTYLE, exStyle);

                    UpdateStyles();
                }
            }

            this.KeyDown += new KeyEventHandler(MultiWebbrowserImp_KeyDown);
            this.FormClosing += new FormClosingEventHandler(MultiWebbrowserImp_FormClosing);
        }

        #region private method
        private void NewFrontBrowser()
        {
            m_frontBrowserHost = new WebBrowserHost();
            m_frontBrowserHost.JavaScript = m_javaObject;
            m_frontBrowserHost.MdiParent = this;
            m_frontBrowserHost.Dock = DockStyle.Fill;
            m_frontBrowserHost.DocumentCompleted += m_BrowserHost_DocumentCompleted;
            m_frontBrowserHost.Show();
            m_frontBrowserHost.SendToBack();


        }

        private void NewBackBrowser()
        {
            m_backBrowserHost = new WebBrowserHost();
            m_backBrowserHost.JavaScript = m_javaObject;
            m_backBrowserHost.MdiParent = this;
            m_backBrowserHost.Dock = DockStyle.Fill;
            m_backBrowserHost.DocumentCompleted += m_BrowserHost_DocumentCompleted;

            m_backBrowserHost.Show();
            m_backBrowserHost.SendToBack();
        }
        private void CloseFrontBrowser()
        {
            if (m_frontBrowserHost != null)
            {
                m_frontBrowserHost.DocumentCompleted -= m_BrowserHost_DocumentCompleted;
                m_frontBrowserHost.Dispose();

                m_frontBrowserHost = null;
            }
        }
        private void CloseBackBrowser()
        {
            if (m_backBrowserHost != null)
            {
                m_backBrowserHost.DocumentCompleted -= m_BrowserHost_DocumentCompleted;
                m_backBrowserHost.Dispose();

                m_backBrowserHost = null;
            }
        }
        #endregion

        void MultiWebbrowserImp_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ( e.CloseReason == CloseReason.UserClosing )
            {
                e.Cancel = true;
            }
        }

        void MultiWebbrowserImp_KeyDown(object sender, KeyEventArgs e)
        {
            if ( e.KeyCode == Keys.F4 &&
                 e.Modifiers == Keys.Alt )
            {
                e.Handled = true;
            }
        }

        public bool Navigate(string argUrl)
        {
            WebBrowserHost old = m_curBrowserHost;

            try
            {
                lock (m_locker)
                {
                    //WebBrowserHost nextHost = NextWebBrowserHost;
                    //m_curBrowserHost = nextHost;
                    //nextHost.Navigate(argUrl);
                    (m_curBrowserHost = NextWebBrowserHost).Navigate(argUrl);
                }
            }
            catch (System.Exception ex)
            {
                Log.UIService.LogError(string.Format("Failed to navigage new address[{0}]", argUrl), ex);

                //m_curBrowserHost = old;
                return false;
            }

            return true;
        }

        public void Present()
        {
            if (m_curBrowserHost == m_frontBrowserHost)
            {
                _m_fCount++;
                m_backBrowserHost.HideHost();
                m_frontBrowserHost.ShowHost();
                if (_m_fCount > 50)
                {
                    _m_fCount = 0;
                    CloseBackBrowser();
                    NewBackBrowser();
                }
            }
            else
            {
                _m_bCount++;
                m_frontBrowserHost.HideHost();
                m_backBrowserHost.ShowHost();
                if (_m_bCount > 50)
                {
                    _m_bCount = 0;
                    CloseFrontBrowser();
                    NewFrontBrowser();
                }
            }
        }
        public void HideScreen()
        {
            Hide();

            if( null != m_curBrowserHost )  
            {
                m_curBrowserHost.Browser.Stop();
            }
        }

        public HtmlDocument Document
        {
            get
            {
                lock (m_locker)
                {
                    if (null != m_curBrowserHost)
                    {
                        return m_curBrowserHost.Document;
                    }
                }

                return null;
            }
        }

        private WebBrowserHost NextWebBrowserHost
        {
            get
            {
                if (m_curBrowserHost == m_backBrowserHost)
                {
                    return m_frontBrowserHost;
                }
                else
                {
                    return m_backBrowserHost;
                }
            }
        }

        protected void OnNavigateCompleted(string argUrl)
        {
            if (null != NavigateComleted)
            {
                NavigateComleted.Invoke(this, argUrl);
            }
        }

        void m_BrowserHost_DocumentCompleted(object argSender, string argUrl)
        {
            if ( m_curBrowserHost == argSender )
            {
                //判断是否真正Completed，否则不执行 Edited by lnqi 20160115
                if ((argSender as WebBrowserHost) != null &&
                    (argSender as WebBrowserHost).Browser.ReadyState != WebBrowserReadyState.Complete)
                {
                    Log.UIService.LogDebugFormat("m_BrowserHost_DocumentCompleted-->Browser.ReadyState is:{0},return.", (argSender as WebBrowserHost).Browser.ReadyState);
                    return;
                }
                OnNavigateCompleted(argUrl);
            }        
        }

        public event NavigateCompletedHandle NavigateComleted;

        private WebBrowserHost m_frontBrowserHost = null;

        private WebBrowserHost m_backBrowserHost = null;

        private WebBrowserHost m_curBrowserHost = null;

        private object m_locker = new object();

        private JavaScriptObject m_javaObject = null;
    }
}
