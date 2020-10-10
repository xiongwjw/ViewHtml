using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Diagnostics;
using UIServiceInWPF;
using UIServiceProtocol;
using ResourceManagerProtocol;
using ResourceManager;
using LogProcessorService;
using eCATActivityTest;
using LogProcessorService;

namespace ViewHtml
{
    public partial class FormViewHtml : Form
    {
        private AutoResetEvent autoEvent = null;
        public FormViewHtml(string folderPath ="")
        {
            InitializeComponent();
            wb.ScriptErrorsSuppressed = true;
            wb.PreviewKeyDown += new PreviewKeyDownEventHandler(wb_PreviewKeyDown);
            wb.DocumentCompleted+=new WebBrowserDocumentCompletedEventHandler(wb_DocumentCompleted);
            autoEvent = new AutoResetEvent(true);
            autoEvent.Reset();
            ThreadPool.QueueUserWorkItem(ThreadMethod);
            if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
            {
                this.folderPath = folderPath;
                InitList();
            }
        }
        private void wb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //SetWebBrowserZoom();
        }

        private void ThreadMethod(object state)
        {
            while (true)
            {
                autoEvent.WaitOne();
                Thread.Sleep(300);
                isFirstTime = true;
                autoEvent.Reset();
            }
        }
        private bool isFullScreen = false;
        private bool isFirstTime = true;
        private void ShowSerach()
        {
            if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
            {
                if (searchForm.ShowDialog() == DialogResult.OK)
                {
                    tvList.SelectedNode = searchForm.SelectNode;
                }
            }
          
        }
        private void ShowSetting()
        {
            FormSetResolution fr = new FormSetResolution();
            if (fr.ShowDialog() == DialogResult.OK)
            {
                this.WindowState = FormWindowState.Normal;
                splitContainerDistance = this.splitContainer1.SplitterDistance;
                this.Width = this.splitContainer1.SplitterDistance + fr.Width;
                this.Height = this.topPannel.Height+fr.Height;
                this.splitContainer1.SplitterDistance = splitContainerDistance;
            }
        }
        private string currentFile = string.Empty;
        private void wb_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {

            if (e.Modifiers == Keys.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.Q:
                        ShowSerach();
                        break;
                    case Keys.S:
                        ShowSetting();
                        break;
                    case Keys.E:
                        OpenSource(currentFile);
                        break;
                    default:
                        break;
                }
            }
            else if (e.KeyCode == Keys.F11 && isFirstTime)
            {
                isFirstTime = !isFirstTime;
                if (isFullScreen)
                {
                    isFullScreen = !isFullScreen;
                    SetNomallScreen();
                }
                else
                {
                    isFullScreen = !isFullScreen;
                    SetFullScreen();
                }
                autoEvent.Set();
            }
            else if (e.KeyCode == Keys.Left && isFirstTime)
            {
                isFirstTime = !isFirstTime;
                NavigateNext();
                autoEvent.Set();
            }
            else if (e.KeyCode == Keys.Right && isFirstTime)
            {
                isFirstTime = !isFirstTime;
                NavigatePre();
                autoEvent.Set();
            }
            
           
        }
       

     
        private void NavigateNext()
        {
            if(tvList.SelectedNode!=null)
                tvList.SelectedNode = tvList.SelectedNode.NextNode;
        }
        private void NavigatePre()
        {
            if (tvList.SelectedNode != null)
                tvList.SelectedNode = tvList.SelectedNode.PrevNode;
        }

        //private void SetWebBrowserZoom()
        //{
        //    var HTMLDocument = wb.Document;
        //    if (HTMLDocument != null)
        //    {
        //        float zoom = (float)zoomRate / 10;
        //        string strZoom = "ZOOM:" + zoom.ToString("#0.0");
        //        // HTMLDocument.Body.Style = "ZOOM:0.5"; // 缩小尺寸(50%)
        //        HTMLDocument.Body.Style = strZoom;
        //    }
        //}

        private string folderPath = string.Empty;
        private void btnOpenPath_Click(object sender, EventArgs e)
        {
            OpenFileDialog od = new OpenFileDialog();
            if (od.ShowDialog() == DialogResult.OK)
            {
                FileInfo fi = new FileInfo(od.FileName);
                txtPath.Text = fi.Directory.FullName;
                folderPath = txtPath.Text;
                fileList.Clear();
                InitList();
            }
        }

        private void InitList()
        {
            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show("directory not exist, please input another path");
                return;
            } 
            tvList.Nodes.Clear();
            DirectoryInfo di = new DirectoryInfo(folderPath);
            LoopToAddItem(null, di);
            if (searchForm != null)
                searchForm.Close();
            searchForm = new FormSearch(fileList);
        }
        private void LoopToAddItem(TreeNode pNode, DirectoryInfo di)
        {
            foreach (DirectoryInfo childDi in di.GetDirectories())
            {
                TreeNode node = AddDiretory(pNode, childDi);
                LoopToAddItem(node,childDi);
            }
            
            foreach (FileInfo fi in di.GetFiles())
            {
                if(fi.Extension.Equals(".html",StringComparison.OrdinalIgnoreCase))
                      AddFile(pNode, fi);
            }
        }
        private TreeNode AddDiretory(TreeNode pNode,DirectoryInfo di)
        {
            TreeNode node = new TreeNode()
            {
                Name = di.Name,
                Text = di.Name,
                Tag = di.FullName
            };
            if (pNode == null)
                tvList.Nodes.Add(node);
            else
                pNode.Nodes.Add(node);
            return node;
        }

    
        private List<SearchInfo> fileList = new List<SearchInfo>();
        private FormSearch searchForm = null;
        private void AddFile(TreeNode pNode,FileInfo fi)
        {
            TreeNode node = new TreeNode()
            {
                Name = fi.Name,
                Text = fi.Name,
                Tag = fi.FullName
            };
            fileList.Add(new SearchInfo(fi.Name, fi.FullName, node));
            if(pNode==null)
                tvList.Nodes.Add(node);
            else
                pNode.Nodes.Add(node);
        }

        private void tvList_MouseClick(object sender, MouseEventArgs e)
        {
            TreeView tv = sender as TreeView;
            if (e.Button == MouseButtons.Right)
            {
                TreeNode selectNode = tv.GetNodeAt(e.Location);
                if (selectNode != null)
                {
                    tv.SelectedNode = selectNode;
                }
            }
        }
        private int navigateIndex = 0;
        private TreeNode currentNode = null;
        private void tvList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (tvList.SelectedNode != null)
            {
                if (tvList.SelectedNode.Tag == null)
                    return;
                string path = tvList.SelectedNode.Tag.ToString();
                if(File.Exists(path))
                    wb.Navigate(path);
                txtPath.Text = path;
                currentFile = path;
                navigateIndex = tvList.SelectedNode.Index;
                if (currentNode != null)
                    currentNode.BackColor = Color.White;
                currentNode = e.Node;
                currentNode.BackColor = Color.Green;
                
               // tvList.SelectedNode.BackColor = Color.Red;
            }
        }

        private void FormViewHtml_Resize(object sender, EventArgs e)
        {
            btnPrintTest.Location = new Point(this.Width - 20 - btnPrintTest.Width, btnPrintTest.Top);
            btnStarteCat.Location= new Point(btnPrintTest.Location.X - 20 - btnStarteCat.Width, btnStarteCat.Top);
            btnHelp.Location = new Point(btnStarteCat.Location.X - 20 - btnHelp.Width, btnHelp.Top);
            btnOpenPath.Location = new Point(btnHelp.Location.X - 20 - btnOpenPath.Width, btnOpenPath.Top);
            txtPath.Width = btnOpenPath.Location.X - txtPath.Location.X - 20;

        }

        int splitContainerDistance = 0;
        private void SetFullScreen()
        {
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            splitContainerDistance = this.splitContainer1.SplitterDistance;
            this.splitContainer1.SplitterDistance = 0;
            
       
        }
        private void SetNomallScreen()
        {
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.splitContainer1.SplitterDistance = splitContainerDistance;
        }
   

        private void FormViewHtml_FormClosing(object sender, FormClosingEventArgs e)
        {
            wb.Dispose();
            if(uiService!=null)
                 uiService.Close();
            if (resource != null)
                resource.Close();

        }

        private void OpenSource(string fileName)
        {
            if(editorFile!=fileName)
            {
                if (File.Exists(fileName))
                {
                    new FormEditor(fileName).Show();
                    editorFile = fileName;
                }
            }

        }

        private string editorFile = string.Empty;

        private void tvList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TreeNode node = tvList.SelectedNode;
            if (node != null)
            {
                if(isStartEcat)
                {
                    if(File.Exists(node.Tag.ToString()))
                        uiService.ShowDirectScreen(node.Tag.ToString(), true);
                }
                else
                {
                    string fileName = node.Tag.ToString();
                    OpenSource(fileName);
                }

            }
        }
        IUIService2 uiService = null;
        ActivityFactory activityFactory = new ActivityFactory();
        private void FormViewHtml_Load(object sender, EventArgs e)
        {

           // Test();
        }

        public void Test()
        {
            Log.ElectricJournal.LogDebug("woding");
        }

        private void InitActityList()
        {
            activityFactory.InitFactory();

            foreach(string activityName in activityFactory.GetActivityList())
            {
                lbActivity.Items.Add(activityName);
            }

        }


        private bool ObjImp_UIEvent(IUIService argiService, UIEventArg argArg)
        {
            //  Log.Action.LogInfoFormat("UI event: the key {0}({1}) pressed", argUIEvent.ElementName, argUIEvent.Key);
            string key = argArg.Key == null ? string.Empty : argArg.Key.ToString();
            string message = $"ui event: element name: {argArg.ElementName}, key: {key}";
            UpdateLog(message);
            if (currentActivity != null)
                currentActivity.OnUIEvtHandle(argiService, argArg);
            return true;
        }

        private void txtPath_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

        private void txtPath_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                folderPath = txtPath.Text.Trim();
                InitList();
            }
        }

        private void tvList_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            
            //e.Node.BackColor = Color.Red;
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("【ctrl+q】Quick search");
            sb.AppendLine("【ctrl+s】Change resolution");
            sb.AppendLine("【ctrl+e】Open source file");
            MessageBox.Show(sb.ToString());
        }

        private eCATContext context = new eCATContext();
        private IBusinessActivity currentActivity = null;
        private IResourceManager resource = null;

        private void lbActivity_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            IBusinessActivity activity = null;
            activityFactory.CreateActivity(lbActivity.Text, out activity);
            if(activity!=null && File.Exists(currentFile))
            {
                try
                {
                    currentActivity = activity;
                    activity.PreRun(context);
                    activity.Run(context);
                    uiService.ShowDirectScreen(currentFile, activity, true);
                }
                catch (System.Exception ex)
                {
                    UpdateLog(ex.Message);
                }

            }

        }

        private void btnStarteCat_Click(object sender, EventArgs e)
        {
            if(!isStartEcat)
                LoadUIService();
        }

        private void LoadUIService()
        {
            uiService = new UIServiceImp();
            resource = ResourceManagerImp.Create();
            string resourceConfigFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config\\ResourceManagerConfig.xml");
            bool result = resource.Open(resourceConfigFile);
            if(!result)
            {
                MessageBox.Show("open resouce manager failed");
                if(resource!=null)
                    resource.Close();
                return;
            }

            string uiConfigFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config\\UIService.xml");

            result = uiService.Open(uiConfigFile, IntPtr.Zero, resource);
            if (!result)
            {
                MessageBox.Show("open uiService failed");
                if (uiService != null)
                    uiService.Close();
                return;
            }

            uiService.UIEvent += ObjImp_UIEvent;
            uiService.ValueChange += UiService_ValueChange;
            
            uiService.Visible = true;

            InitActityList();
            InitLanguageList();
            splitMain.SplitterDistance = splitMain.Width - 300;
            splitCenter.SplitterDistance = splitCenter.Height - 200;
            editorLog.CurrentLanguage = wjw.editor.Editor.Language.Log;
            splitMain.Panel2Collapsed = false;
            splitCenter.Panel2Collapsed = false;
            btnPrintTest.Visible = true;
            isStartEcat = true;

        }

        private void UpdateLog(string message)
        {
            if(editorLog.InvokeRequired)
            {
                this.Invoke(new Action<string>(WriteUILog), message);
            }
            else
            {
                WriteUILog(message);
            }
        }

        private void WriteUILog(string message)
        {
            editorLog.AppendText(message);
        }

        private void UiService_ValueChange(string id, string property, object value)
        {
            UpdateLog($"id:{id},property:{property} change ,value:{value.ToString()}");
        }

        private bool isStartEcat = false;

        private void InitLanguageList()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resource\Text");
            foreach (var di in new DirectoryInfo(path).GetDirectories())
            {
                cbLanguage.Items.Add(di.Name);
            }
            if (cbLanguage.Items.Count > 0)
                cbLanguage.SelectedIndex = 0;
        }

        private void cbLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (resource != null)
                resource.CurrentUILanguage = cbLanguage.Text;
        }

        #region print form test
        private Thread m_webThread = null;
        private FormPrintTest m_webBrowser = null;
        private object m_synLocker = new object();
        private AutoResetEvent m_docCompleteEvt = new AutoResetEvent(false);
        private AutoResetEvent m_openEvt = null;
        private void Browser_DocumentCompleted(object sender,
                                       WebBrowserDocumentCompletedEventArgs e)
        {
            Log.BusinessService.LogDebugFormat("A4 document[{0}] navigating is completed", e.Url.ToString());
            m_docCompleteEvt.Set();
        }
        private void OnWebThread()
        {
            try
            {
                m_webBrowser = new FormPrintTest();
                m_webBrowser.Show();
                m_webBrowser.Browser.DocumentCompleted += Browser_DocumentCompleted;
                m_webBrowser.FormClosing += M_webBrowser_FormClosing;

                Application.ThreadException += (argSender, argParam) =>
                {
                    Log.BusinessService.LogError("The unhandle exception of A4 print service", argParam.Exception);
                };

                m_openEvt.Set();

                Application.Run();
            }
            catch (ThreadAbortException exp)
            {
                Thread.ResetAbort();
                Log.BusinessService.LogError("The thread to start A4 printer service was terminated", exp);
            }
            catch (System.Exception ex)
            {
                Log.BusinessService.LogError("Failed to start A4 printer service", ex);
            }
        }

        private void M_webBrowser_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseFormPreview();
        }

        private bool StartFormThread()
        {
            m_openEvt = new AutoResetEvent(false);
            m_webThread = new Thread(new ThreadStart(OnWebThread));
            m_webThread.IsBackground = true;
            m_webThread.Name = "A4PrinterService";
            m_webThread.SetApartmentState(ApartmentState.STA);
            m_webThread.Start();

            if (!m_openEvt.WaitOne(30000))
            {
                Log.BusinessService.LogError("Waiting for A4 printer service is timeout");
                return false;
            }
            return true;
        }
        private void btnPrintTest_Click(object sender, EventArgs e)
        {
            try
            {
                if (tvList.SelectedNode == null || tvList.SelectedNode.Tag == null)
                {
                    MessageBox.Show("no select file");
                    return;
                }

                string path = tvList.SelectedNode.Tag.ToString();

                if(!File.Exists(path))
                {
                    MessageBox.Show("file not found");
                    return;
                }
                Dictionary<string, string> dict = new Dictionary<string, string>();
                eCATActivityTest.GenerateDitCarForm.GenerateData(context, ref dict);

                if (m_webBrowser == null)
                    if (!StartFormThread())
                    {
                        MessageBox.Show("start preview form thread failed");
                        return;
                    }

                if (!OpenFormFile(path, dict))
                {
                    MessageBox.Show("open file failed");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CloseFormPreview()
        {
            if (null != m_webBrowser)
            {
                m_webBrowser.Browser.DocumentCompleted -= Browser_DocumentCompleted;
            }

            //if (null != m_docCompleteEvt)
            //{
            //    m_docCompleteEvt.Dispose();
            //    m_docCompleteEvt = null;
            //}

            //m_webBrowser.Exit();
            //m_webBrowser.Close();
            //m_webBrowser = null;

            //if(m_webThread!=null && m_webThread.IsAlive)
            //{
            //    if (m_webThread.Join(30000))
            //    {
            //        m_webThread.Abort();
            //        m_webThread.Join(10000);
            //    }
            //    m_webThread = null;
            //}
            
            m_webBrowser = null;

        }

        public bool OpenFormFile(string argPath, Dictionary<string, string> argDicValue)
        {
            Debug.Assert(!string.IsNullOrEmpty(argPath));
            Log.BusinessService.LogDebugFormat("Prepare for printing a document [{0}]", argPath);
            if (string.IsNullOrEmpty(argPath))
            {
                Log.BusinessService.LogErrorFormat("The url[{0}] is null or empty", argPath);
                return false;
            }

            if (!File.Exists(argPath))
            {
                Log.BusinessService.LogErrorFormat("The path[{0}]  file is not exists", argPath);
                return false;
            }
         
            lock (m_synLocker)
            {
                try
                {
                    m_docCompleteEvt.Reset();
                    if (!m_webBrowser.Navigate(argPath))
                    {
                        Log.BusinessService.LogError("Failed to navigate the A4 document");
                        return false;
                    }
                    if (!m_docCompleteEvt.WaitOne(20000))
                    {
                        return false;
                    }
                    m_docCompleteEvt.Reset();

                    if (!m_webBrowser.SetHtmlDataThenPrint(argDicValue))
                    {
                        Log.BusinessService.LogError("Failed to set A4 data document");
                        return false;
                    }
                    //if (!m_webBrowser.CreateTempPath(tempPath))
                    //{
                    //    Log.BusinessService.LogError("Failed to print A4 data document");
                    //    return false;
                    //}

                }
                catch (System.Exception ex)
                {
                    Log.BusinessService.LogError("Failed to print A4 document", ex);
                    return false;
                }
            }
            Log.BusinessService.LogDebug("SaveDocument data success");
            return true;
        }
        #endregion

    }

    public class SearchInfo
    {
        public SearchInfo(string filename, string filePath, TreeNode node)
        {
            this.FileName = filename; this.FilePath = filePath; this.Node = node;
        }
        public string FileName;
        public string FilePath;
        public TreeNode Node;
    }

}
