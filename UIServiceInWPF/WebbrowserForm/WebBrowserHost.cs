using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using UIServiceInWPF.Interop;

namespace UIServiceInWPF
{
    public partial class WebBrowserHost : Form
    {
        public delegate void DocumentCompletedHandle( object argSender, 
                                                      string argUrl );

        public WebBrowserHost()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);

            this.KeyDown += new KeyEventHandler(WebBrowserHost_KeyDown);
            this.FormClosing += new FormClosingEventHandler(WebBrowserHost_FormClosing);
        }

        void WebBrowserHost_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ( e.CloseReason == CloseReason.UserClosing )
            {
                e.Cancel = true;
            }
        }

        void WebBrowserHost_KeyDown(object sender, KeyEventArgs e)
        {
            if ( e.KeyCode == Keys.F4 &&
                 e.Modifiers == Keys.Alt )
            {
                e.Handled = true;
            }
        }

        private void WebBrowserHost_Load(object sender, EventArgs e)
        {
            webBrowserHtml.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowserHtml_DocumentCompleted);
            webBrowserHtml.ObjectForScripting = m_javaObject;
        }

        void webBrowserHtml_DocumentCompleted(object sender,
                                       WebBrowserDocumentCompletedEventArgs e)
        {
            OnDocumentCompleted(e.Url.ToString());
        }

        private void WebBrowserHost_Resize(object sender, EventArgs e)
        {
            webBrowserHtml.Size = new Size(this.Width, this.Height);
        }

        protected void OnDocumentCompleted( string arg )
        {
            if ( null != DocumentCompleted )
            {
                DocumentCompleted.Invoke( this, arg );
            }
        }

        public HtmlDocument Document
        {
            get
            {
                return webBrowserHtml.Document;
            }
        }

        public WebBrowser Browser
        {
            get
            {
                return webBrowserHtml;
            }
        }

        public JavaScriptObject JavaScript
        {
            set
            {
                m_javaObject = value;
            }
        }

        public void Navigate( string argUrl )
        {
            Debug.Assert(!string.IsNullOrEmpty(argUrl));
            m_curUrl = argUrl;
            webBrowserHtml.Navigate(argUrl);
        }

        public void HideHost()
        {
            SendToBack();
            //webBrowserHtml.Stop();
        }

        public void ShowHost()
        {
            BringToFront();
        }

        public event DocumentCompletedHandle DocumentCompleted;

        public string m_curUrl = null;

        private JavaScriptObject m_javaObject = null;
    }
}
