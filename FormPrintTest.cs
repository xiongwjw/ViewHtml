using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogProcessorService;

namespace ViewHtml
{
    public delegate bool NavigateUrlHandler(string argUrl);

    public delegate void ExitHandler();

    public delegate void ClearHtmlHandler();

    public delegate bool SetHtmlDataThenPrintHandler(Dictionary<string, string> argDicData);

    public delegate bool CreateHtmlTempHandler(string tempPath);

    public partial class FormPrintTest : Form
    {
        private string htmlPath = string.Empty; //html路径
        public FormPrintTest()
        {
            InitializeComponent();
            m_navHandler = (NavigateUrlHandler)Delegate.CreateDelegate(typeof(NavigateUrlHandler), this, "OnNavigateUrl");
            m_exitHandler = (ExitHandler)Delegate.CreateDelegate(typeof(ExitHandler), this, "OnExit");
            m_clearHtmlHandler = (ClearHtmlHandler)Delegate.CreateDelegate(typeof(ClearHtmlHandler), this, "OnClearHtml");
            m_setDataThenPrintHandler = (SetHtmlDataThenPrintHandler)Delegate.CreateDelegate(typeof(SetHtmlDataThenPrintHandler), this, "OnSetHtmlDataThenPrint");
            m_CreateHtmlTempHandler = (CreateHtmlTempHandler)Delegate.CreateDelegate(typeof(CreateHtmlTempHandler), this, "OnCreateHtmlTemp");

            this.FormClosed += WebBrowserHost_FormClosed;
            this.FormClosing += WebBrowserHost_FormClosing;
        }


        private void WebBrowserHost_FormClosing(object sender, FormClosingEventArgs e)
        {
            Log.BusinessService.LogInfoFormat("The form of A4 print service is closing. reason[{0}]", e.CloseReason);
        }

        private void WebBrowserHost_FormClosed(object sender, FormClosedEventArgs e)
        {
            OnExit();
            Log.BusinessService.LogInfoFormat("The form of A4 print service was closed, resason[{0}]", e.CloseReason);
        }

        public bool Navigate(string argUrl)
        {
            htmlPath = argUrl;
            Debug.Assert(!string.IsNullOrEmpty(argUrl));
            if (string.IsNullOrEmpty(argUrl) ||
                 !File.Exists(argUrl))
            {
                Log.BusinessService.LogErrorFormat("The url[{0}] is empty or isn't exist", argUrl);
                return false;
            }

            return (bool)this.Invoke(m_navHandler,
                              argUrl);

        }

        public void Exit()
        {
            this.Invoke(m_exitHandler);
        }

        public void ClearHtmlData()
        {
            this.Invoke(m_clearHtmlHandler);
        }

        public bool SetHtmlDataThenPrint(Dictionary<string, string> argData)
        {
            return (bool)this.Invoke(m_setDataThenPrintHandler,
                                argData);
        }

        public bool CreateTempPath(string tempPath)
        {
            return (bool)this.Invoke(m_CreateHtmlTempHandler,
                                tempPath);
        }
        private bool OnNavigateUrl(string argUrl)
        {
            Debug.Assert(!string.IsNullOrEmpty(argUrl));

            try
            {
                Log.BusinessService.LogDebugFormat("The A4 html's url is [{0}]", argUrl);
                m_webBrowser1.Navigate(argUrl);
            }
            catch (System.Exception ex)
            {
                Log.BusinessService.LogError("Failed to navigate url", ex);
                return false;
            }

            return true;
        }

        private void OnExit()
        {
            try
            {
                Log.BusinessService.LogDebug("Prepare for exit A4 printer service");
                Application.ExitThread();
            }
            catch (System.Exception ex)
            {
                Log.BusinessService.LogError("Failed to exit A4 printer service", ex);
            }
        }

        private void OnClearHtml()
        {
            try
            {
                HtmlDocument doc = m_webBrowser1.Document;
                if (doc == null)
                {
                    return;
                }

                foreach (HtmlElement element in doc.GetElementsByTagName("*"))
                {
                    if (!string.IsNullOrWhiteSpace(element.Id))
                    {
                        if (element.TagName.Equals("IMG", StringComparison.OrdinalIgnoreCase))
                        {
                            element.SetAttribute("src", string.Empty);
                        }
                        else
                        {
                            element.InnerHtml = string.Empty;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.BusinessService.LogError("A4PrinterService.WebBrowserHost  OnClearHtml error", ex);
            }
        }
        private bool OnCreateHtmlTemp(string tempPath)
        {

            return WriteHtmlFromBrowser(tempPath);

        }
        private bool OnSetHtmlDataThenPrint(Dictionary<string, string> argData)
        {
            try
            {
                if (null != argData &&
                    argData.Count > 0)
                {
                    HtmlDocument doc = m_webBrowser1.Document;
                    if (doc == null)
                    {
                        return false;
                    }

                    HtmlElement element;
                    foreach (var item in argData)
                    {
                        element = doc.GetElementById(item.Key);
                        if (element != null)
                        {
                            if (element.TagName.Equals("IMG", StringComparison.OrdinalIgnoreCase))
                            {
                                if (File.Exists(item.Value))
                                {
                                    element.SetAttribute("src", item.Value);
                                }
                                else
                                {
                                    Log.BusinessService.LogWarnFormat("file is not found[{0}]", item.Value);
                                }
                            }
                            else
                            {
                                element.InnerHtml = item.Value;
                            }
                        }
                        else
                        {
                            Log.BusinessService.LogWarnFormat("HtmlElement for {0} is null", item.Key);
                        }
                    }
                }
                Log.BusinessService.LogDebug("HtmlElement set data success");
                //将数据写入到html中
                //bool writeHtml = WriteHtmlFromBrowser(m_webBrowser1, htmlPath);
                //if (!writeHtml)
                //{
                //    Log.BusinessService.LogError("Failed to write html data a");
                //}
                //取消打印
                //m_webBrowser1.Print();
            }
            catch (System.Exception ex)
            {
                Log.BusinessService.LogError("Failed to set html data and print", ex);
                return false;
            }

            return true;
        }

        public WebBrowser Browser
        {
            get
            {
                return m_webBrowser1;
            }
        }

        private NavigateUrlHandler m_navHandler = null;

        private ExitHandler m_exitHandler = null;

        private ClearHtmlHandler m_clearHtmlHandler = null;

        private SetHtmlDataThenPrintHandler m_setDataThenPrintHandler = null;

        private CreateHtmlTempHandler m_CreateHtmlTempHandler = null;

        private byte[] array;
        private FileStream fs = null;
        private StringBuilder sbuild = new StringBuilder();
        /// <summary>
        ///  将Webbrowser中的数据导出
        /// </summary> 
        /// <param name="tempPath">创建的文件</param>
        /// <returns>是否成功</returns>
        private bool WriteHtmlFromBrowser(string tempPath)
        {
            //讲文件写成html
            try
            {
                if (m_webBrowser1.Document == null)
                {
                    return false;
                }
                sbuild.Length = 0;
                sbuild.Append(m_webBrowser1.DocumentText.Substring(0, m_webBrowser1.DocumentText.IndexOf("</head>") + 7));
                sbuild.Append("<body>" + m_webBrowser1.Document.Body.InnerHtml + "</body>");
                sbuild.Append("</html>");
                array = UnicodeEncoding.GetEncoding("utf-8").GetBytes(sbuild.ToString());
                fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write);
                fs.Write(array, 0, array.Length);

            }
            catch (Exception ex)
            {
                Log.BusinessService.LogError("Write Html From Brower Fail" + ex.Message);
                return false;
            }
            finally
            {
                if (fs != null)
                {

                    fs.Close();
                    fs = null;
                }

            }
            return true;
        }

    }
}
