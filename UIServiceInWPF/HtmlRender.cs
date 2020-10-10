/********************************************************************
	FileName:   HtmlRender
    purpose:	

	author:		huang wei
	created:	2012/10/31

    revised history:
	2012/10/31  

================================================================
    Copyright (C) 2012, Grgbanking CO,. Ltd. All rights reserved.
================================================================
********************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Resources;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UIServiceInWPF.HtmlScreenElement;
using System.ComponentModel;
using LogProcessorService;
using ResourceManagerProtocol;
using UIServiceProtocol;
using System.Text.RegularExpressions;
using UIServiceInWPF.screen;
using UIServiceInWPF.Resource;
using System.Runtime.InteropServices;
using System.Reflection;
using UIServiceInWPF.Interop;

namespace UIServiceInWPF
{
    public class HtmlRender
    {
        #region constructor
        private HtmlRender()
        {
        }

        static HtmlRender()
        {
            s_Render = new HtmlRender();
        }
        #endregion

        #region propety
        public static HtmlRender SingleInstance
        {
            get
            {
                return s_Render;
            }
        }

        //public Page HtmlRenderPage
        //{
        //    get
        //    {
        //        return m_htmlFrontPage;
        //    }
        //}

        public string NameOfHtml
        {
            get;
            set;
        }


        #endregion

        #region method
        public void OnButtonClicked(string argID,
                             object argTag,
                             string argMask)
        {
            Debug.Assert(null != m_application);
            HtmlElement htmlButton = m_webBrowserImp.Document.GetElementById(argID);
            if (htmlButton != null && !htmlButton.Enabled)
            {
                return;
            }
            else
            m_application.OnButtonClickInHtml(argID, argTag, argMask);
        }

        public void OnValueChange(string id, string property, object value)
        {
            m_application.OnValueChange(id, property, value);
        }

        public void OnFocusChanged(string argID,
                                    string argKey)
        {
            Debug.Assert(null != m_application);
            m_application.OnFocusChangedInHtml(argID, argKey);
        }

        public object GetBindedData(string argKey)
        {
            Debug.Assert(!string.IsNullOrEmpty(argKey));
            try
            {
            if (null == m_dataContext ||
                 string.IsNullOrEmpty(argKey))
            {
                return null;
            }

            PropertyInfo Prop = m_dataContext.GetType().GetProperty(argKey, BindingFlags.Public | BindingFlags.Instance);
            if (null != Prop)
            {
                return Prop.GetValue(m_dataContext, null);
            }
            else
            {
                MethodInfo getMethodInfo = m_dataContext.GetType().GetMethod(BindingExpress.s_getMethodName, BindingFlags.Instance | BindingFlags.Public);
                if (null != getMethodInfo)
                {
                    return getMethodInfo.Invoke(m_dataContext, new string[] { argKey });
                }
            }
            }
            catch (Exception ex)
            {
                Log.UIService.LogError("GetBindedData " + argKey + " catch:" + ex);
            }

            return null;
        }

        public bool SetBindedData(string argKey,
                                   object argValue)
        {
            Debug.Assert(!string.IsNullOrEmpty(argKey));
            if (null == m_dataContext ||
                 string.IsNullOrEmpty(argKey))
            {
                return false;
            }

            PropertyInfo Prop = m_dataContext.GetType().GetProperty(argKey, BindingFlags.Public | BindingFlags.Instance);
            if (null != Prop)
            {
                Prop.SetValue(m_dataContext, argValue, null);
                return true;
            }
            else
            {
                MethodInfo getMethodInfo = m_dataContext.GetType().GetMethod(BindingExpress.s_setMethodName, BindingFlags.Instance | BindingFlags.Public);
                if (null != getMethodInfo)
                {
                    return (bool)getMethodInfo.Invoke(m_dataContext, new object[] { argKey, argValue });
                }
            }

            return false;
        }

        public string GetResourceValue(string argIdsKey)
        {
            if (string.IsNullOrWhiteSpace(argIdsKey))
            {
                return string.Empty;
            }
            return m_application.ResourceManager.CurrentUIResource.LoadString(argIdsKey);
        }

        public void OnPopupEventRaised(string argKey)
        {
            m_application.OnPopupEventHtml(argKey);
        }

        public bool Open(UIServiceWpfApp app)
        {
            Debug.Assert(null != app);
            m_application = app;

            m_webBrowserImp = new MultiWebbrowserImp();
            m_webBrowserImp.NavigateComleted += new NavigateCompletedHandle(m_webBrowserImp_NavigateComleted);
            m_document_Click = (System.Windows.Forms.HtmlElementEventHandler)Delegate.CreateDelegate(typeof(System.Windows.Forms.HtmlElementEventHandler), this, "Document_Click");

            m_mouseDownHandler = (System.Windows.Forms.HtmlElementEventHandler)Delegate.CreateDelegate(typeof(System.Windows.Forms.HtmlElementEventHandler), this, "OnLeftButtonDown");
            m_contextMenuHandler = (System.Windows.Forms.HtmlElementEventHandler)Delegate.CreateDelegate(typeof(System.Windows.Forms.HtmlElementEventHandler), this, "OnContextMenuShow");
            m_mouseUpHandler = (System.Windows.Forms.HtmlElementEventHandler)Delegate.CreateDelegate(typeof(System.Windows.Forms.HtmlElementEventHandler), this, "OnLeftButtonUp");

            m_dicScreenElementCreator.Add(UIServiceCfgDefines.s_buttonValue, CreateButtonElement);
            m_dicScreenElementCreator.Add(UIServiceCfgDefines.s_checkbox, CreateButtonElement);
            m_dicScreenElementCreator.Add(UIServiceCfgDefines.s_radio, CreateButtonElement);
            m_dicScreenElementCreator.Add(UIServiceCfgDefines.s_submitButton, CreateButtonElement);
            m_dicScreenElementCreator.Add(UIServiceCfgDefines.s_resetButton, CreateButtonElement);
            m_dicScreenElementCreator.Add(UIServiceCfgDefines.s_countDownValue, CreateCountDownElement);
            m_dicScreenElementCreator.Add(UIServiceCfgDefines.s_textValue, CreateInputElement);
            m_dicScreenElementCreator.Add(UIServiceCfgDefines.s_passwordValue, CreateInputElement);
            m_dicScreenElementCreator.Add(UIServiceCfgDefines.s_textblockValue, CreateInputElement);
            m_dicScreenElementCreator.Add(UIServiceCfgDefines.s_selectValue, CreateSelectElement);
            m_dicScreenElementCreator.Add(UIServiceCfgDefines.s_listValue, CreateListElement);
            m_dicScreenElementCreator.Add(UIServiceCfgDefines.s_mediaValue, CreateMediaElement);
            m_dicScreenElementCreator.Add(UIServiceCfgDefines.s_imgValue, CreateImageElement);

            m_dataPropChangedHander = new Action<string>(OnPropertyChanged);
            //           m_htmlFrontPage = new Page4Html( app );
            //            m_htmlBackPage = new Page4Html( app );
            //            m_curHtmlPage = m_htmlFrontPage;
            //m_htmlPage = new Page4Html( app );

            return true;
        }

        public string GetCurrentScreenPropertyValue(string ID, string propertyName)
        {
            HtmlElement e = null;
            if (m_webBrowserImp.Document != null)
            {
                e = m_webBrowserImp.Document.GetElementById(ID);
            }

            if (e != null)
                return e.GetAttribute(propertyName);
            else
                return null;
        }
        void m_webBrowserImp_NavigateComleted(object argSender, string argUrl)
        {
            Log.UIService.LogDebugFormat("The url[{0}] has navigate completed", argUrl);
            //if ( 0 != string.Compare( arg.Url.ToString(), m_curUrl, true ) )
            //if ( !arg.Url.ToString().Equals( m_curUrl, StringComparison.OrdinalIgnoreCase ) )
            //{
            //    return;
            //}
            m_application.NotifyHtmlIsOk(OwnerScreen);

            RemoveEventHandlesOfHtmlDocument(m_curHtmlDocument);
            m_curHtmlDocument = null;

            m_curHtmlDocument = m_webBrowserImp.Document;
            Debug.Assert(null != m_curHtmlDocument);
            ChangeCssResource();
            ExtractAllScreenElements(m_curHtmlDocument);
            //            m_curHtmlDocument.InvokeScript(s_ChangeLangScript, new object[] { m_application.CurrentLanguage });
            AddEventHandlesOfHtmlDocument(m_curHtmlDocument);
            if (null != OwnerScreen)
            {
                OwnerScreen.NotifyScreenOk(m_curHtmlDocument);
            }
            // FindAllButtonsInHtml(m_curHtmlDocument);
            Log.UIService.LogDebug("Notify UIServiceApp : navigate completed");
            m_application.OnNavigateCompleted();
        }

        public void Close()
        {
            m_webBrowserImp.NavigateComleted -= m_webBrowserImp_NavigateComleted;
            RemoveEventHandlesOfHtmlDocument(m_curHtmlDocument);
            ClearScreenElements();
            m_listBindingExpresses.Clear();

            m_webBrowserImp.Close();
            m_webBrowserImp.Dispose();
            m_webBrowserImp = null;
            m_curHtmlDocument = null;
        }

        //add for special handling for some button create at runtime
        private void Document_Click(object sender, HtmlElementEventArgs e)
        {
            HtmlElement htmlButton = m_webBrowserImp.Document.GetElementById("atmcinterop");
            if (htmlButton != null)
            {
                string strHandle = htmlButton.GetAttribute("handle");
                if (string.Equals(strHandle, "true", StringComparison.OrdinalIgnoreCase))
                {
                    string tag = htmlButton.GetAttribute("tag");
                    htmlButton.SetAttribute("handle", "false");
                    OnButtonClicked(htmlButton.Id, tag, null);
                }
            }
        }
        private string lastTempHtmlPath = string.Empty;
        private string tempFileName = "temp.html";
        private void DelTempFile()
        {
            if (!string.IsNullOrEmpty(lastTempHtmlPath)
               && System.IO.File.Exists(lastTempHtmlPath))
                File.Delete(lastTempHtmlPath);
        }
        private string CopyToNewPath(string path)
        {
            string newFilePath = Path.Combine(new FileInfo(path).Directory.FullName, tempFileName);
            System.IO.File.Copy(path, newFilePath, true);
            lastTempHtmlPath = newFilePath;
            return newFilePath;
        }
        private bool PreReplace(string newFilePath)
        {
            StreamReader sr = new StreamReader(newFilePath, Encoding.UTF8);
            try
            {
                String line;
                StringBuilder sb = new StringBuilder();
                Regex reg = new Regex("{Replace.*?}", RegexOptions.IgnoreCase);
                Match match = null;
                string subText = string.Empty;
                string bindingText = string.Empty;
                int bindex = 0;
                string bufferName = string.Empty;
                object bufferValueObj = null;
                
                while ((line = sr.ReadLine()) != null)
                {
                    match = reg.Match(line);
                    subText = line;
                    while (match.Success && !string.IsNullOrEmpty(match.Groups[0].Value))
                    {
                        bindingText = match.Groups[0].Value;
                        bufferName = bindingText.Substring(bindingText.LastIndexOf(' ') + 1, bindingText.Length - bindingText.LastIndexOf(' ') - 2).Trim();
                        Log.UIService.LogDebug("binding name is:"+bufferName);
                        bufferValueObj = GetBindedData(bufferName);
                        
                        if (bufferValueObj != null)
                        {
                            string bindingValue = bufferValueObj.ToString().Replace("\"", "&quot;");
                            line = line.Replace(bindingText, bindingValue);
                            Log.UIService.LogDebug("binding data is:" + bindingValue);
                        }
                        bindex = subText.IndexOf(bindingText) + bindingText.Length + 1;
                        subText = subText.Substring(bindex, subText.Length - bindex);
                        bufferValueObj = null;
                        match = reg.Match(subText);
                    }
                    sb.AppendLine(line);
                }
                sr.Close();
                sr.Dispose();
                WriteFile(newFilePath, sb.ToString());
                return true;

            }
            catch (System.Exception ex)
            {
                Log.UIService.LogError("pre replace value err:" + ex.Message);
                return false;
            }
            finally
            {
                sr.Close();
                sr.Dispose();
            }

        }
        private void WriteFile(string path, string content)
        {
            FileStream fs = new FileStream(path, FileMode.Truncate);
            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
            sw.Write(content.ToString());
            sw.Flush();
            sw.Close();
            fs.Close();
            fs.Dispose();
            sw.Dispose();
        }
        public string PreProcessHtml(string path)
        {
            try
            {
                path = path.Replace(@"file:///", string.Empty);
                if (!CheckIfNeedPreProcess(path))
                    return string.Empty;

                DelTempFile();

                string newFilePath = CopyToNewPath(path);

                if (PreReplace(newFilePath))
                    return newFilePath;
                else
                    return string.Empty;
            }
            catch (System.Exception ex)
            {
                Log.UIService.LogError("pre process replace value err:" + ex.Message);
                return string.Empty;
            }
        }

        private string ReadFile(string path)
        {
            StreamReader sr = new StreamReader(path, Encoding.UTF8);
            string content = sr.ReadToEnd();
            sr.Close();
            sr.Dispose();
            return content;
        }

        private bool CheckIfNeedPreProcess(string path)
        {
            Regex reg = new Regex("{Replace.*?}", RegexOptions.IgnoreCase);
            if (reg.Match(ReadFile(path)).Success)
                return true;
            else
                return false;
        }
        public bool Render(string filePath)
        {
            Debug.Assert(!string.IsNullOrEmpty(filePath));
            string newScreenPath = PreProcessHtml(filePath);
            if (!string.IsNullOrEmpty(newScreenPath))
                filePath = newScreenPath;

            bool result = m_webBrowserImp.Navigate(filePath);
            if (!result)
            {
                return result;
            }

            if (LastScrIsXDCForm)
            {
                Log.UIService.LogDebug("last scr is xdc form ");
                LastScrIsXDCForm = false;
               // XDCCanvasForm.Instance.Show();
            }
            else
            {
                if (!m_webBrowserImp.Visible)
                {
                    m_webBrowserImp.Show();
                }
            }
            //           m_htmlFrontPage.Navigate(filePath);
            //if ( m_htmlFrontPage == m_curHtmlPage )
            //{
            //    m_htmlBackPage.Navigate(filePath);
            //    m_curHtmlPage = m_htmlBackPage;
            //}
            //else
            //{
            //    m_htmlFrontPage.Navigate(filePath);
            //    m_curHtmlPage = m_htmlFrontPage;
            //}

            return true;
        }

        public void AddElementBinding(ElementBindingExpress argExpress)
        {
            Debug.Assert(null != argExpress);
            m_listBindingExpresses.Add(argExpress);
        }

        public void HideScreen(bool argStop = true)
        {
            Debug.Assert(null != m_webBrowserImp);
            if (argStop)
            {
                m_webBrowserImp.HideScreen();
            }
            else
            {
                m_webBrowserImp.Hide();
            }
        }

        public void Present()
        {
            //m_webBrowserImp.Present();

            if (!m_webBrowserImp.Visible)
            {
                m_webBrowserImp.Show();
            }
			
			m_webBrowserImp.Present();

            FirePresentJsEvent();

            // Debug.Assert(null != m_curHtmlPage);

            //if ( m_curHtmlPage == m_htmlFrontPage )
            //{
            //    m_htmlBackPage.HideScreen();
            //}
            //else
            //{
            //    m_htmlFrontPage.HideScreen();
            //}
            //          m_curHtmlPage.Present();
        }

        private void FirePresentJsEvent()
        {
            try
            {
                const string scriptElementId = "ecat_auto_gen_js";
                var autoGenerateJs = m_webBrowserImp.Document.GetElementById(scriptElementId);
                if (autoGenerateJs == null)
                {
                    var scriptElement = m_webBrowserImp.Document.CreateElement("script");
                    if (scriptElement != null)
                    {
                        scriptElement.Id = scriptElementId;
                        scriptElement.InnerHtml = @"
                        function firePresentEvent(){
	                        var myEvent = document.createEvent('HTMLEvents');
                            myEvent.initEvent('present', true, false);
                            if (window.dispatchEvent)
                            {
                                window.dispatchEvent(myEvent);
                            }
                            else
                            {
                                window.fireEvent(myEvent);
                            }
                        }";
                        if (m_webBrowserImp.Document.Body != null)
                        {
                            m_webBrowserImp.Document.Body.AppendChild(scriptElement);
                        }
                    }
                }

                autoGenerateJs = m_webBrowserImp.Document.GetElementById(scriptElementId);
                if (autoGenerateJs != null)
                {
                    m_webBrowserImp.Document.InvokeScript("firePresentEvent");
                }
            }
            catch (Exception ex)
            {
                Log.UIService.LogError(ex.ToString());
            }
        }

        public bool ExtractAllElements(HtmlElement argParent,
                                        Dictionary<string, HtmlScreenElementBase> argElements)
        {
            Debug.Assert(null != argParent && null != argElements);
            try
            {
                HtmlElementCollection elementCollection = argParent.All;
                //Debug.Assert(null != elementCollection);
                if (null == elementCollection ||
                     elementCollection.Count == 0)
                {
                    return true;
                }

                HtmlScreenElementBase screenElement = null;
                string idsValue = null;
                string valueOfAttri = null;
                IResourceService iCurrentUIResource = null;
                if (null != m_application.ResourceManager)
                {
                    iCurrentUIResource = m_application.ResourceManager.CurrentUIResource;
                }
                string text = null;
                string replaceAttri = null;
                int replaceValue = 0;
                string imagePath = null;
                string srcPath = null;
                foreach (System.Windows.Forms.HtmlElement element in elementCollection)
                {
                    //replace ids
                    idsValue = element.GetAttribute(s_UITextKey);
                    if (null != iCurrentUIResource &&
                        !string.IsNullOrEmpty(idsValue))
                    {
                        //if (iCurrentUIResource.LoadString(idsValue, TextCategory.s_UI, out text))
                        iCurrentUIResource.LoadString(idsValue, TextCategory.s_UI, out text);
                        {
                            element.InnerHtml = ConvertText(text);
                        }
                    }
                    //replace image
                    if (null != iCurrentUIResource &&
                         s_imgName.Equals(element.TagName, StringComparison.OrdinalIgnoreCase))
                    {
                        replaceAttri = element.GetAttribute(s_replaceAttri);
                        srcPath = element.GetAttribute("srcKey");
                        if (!string.IsNullOrEmpty(replaceAttri) &&
                             !string.IsNullOrEmpty(srcPath) &&
                             int.TryParse(replaceAttri, out replaceValue) &&
                             replaceValue == 1)
                        {
                            if (iCurrentUIResource.QueryImagePath(srcPath, out imagePath))
                            {
                                element.SetAttribute("src", imagePath);
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(srcPath) &&
                                 srcPath.StartsWith("(", StringComparison.Ordinal) &&
                                 srcPath.EndsWith(")", StringComparison.Ordinal))
                            {
                                string src = srcPath.Trim('(', ')');
                                if (!string.IsNullOrWhiteSpace(src))
                                {
                                    if (iCurrentUIResource.QueryImagePath(src, out imagePath))
                                    {
                                        element.SetAttribute("src", imagePath);
                                    }
                                }
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(element.Id))
                    {
                        continue;
                    }

                    valueOfAttri = element.GetAttribute(s_typeAttri);
                    if (string.IsNullOrEmpty(valueOfAttri))
                    {
                        continue;
                    }

                    valueOfAttri = valueOfAttri.ToLowerInvariant();
                    if (m_dicScreenElementCreator.ContainsKey(valueOfAttri))
                    {
                        try
                        {
                            screenElement = m_dicScreenElementCreator[valueOfAttri].Invoke(element);
                            if (null != screenElement)
                            {
                                argElements.Add(element.Id, screenElement);
                            }
                        }
                        catch (System.Exception ex)
                        {
                            LogProcessorService.Log.UIService.LogWarn(string.Format("Failed to create a screen element[{0}]", element.Id), ex);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.UIService.LogError("Failed to extract all elements", ex);
                return false;
            }

            return true;
        }

        public bool SetPropertyValueOfElement(string strElement,
                                               string strProperty,
                                               object objValue)
        {
            Debug.Assert(!string.IsNullOrEmpty(strProperty));

            if (null == m_curHtmlDocument)
            {
                return false;
            }

            if (string.IsNullOrEmpty(strElement))
            {
                switch (strProperty)
                {
                    case UIPropertyKey.s_DataContextKey:
                        {
                            if (null != m_dataContext &&
                                 m_dataContext is INotifyPropertyChanged)
                            {
                                INotifyPropertyChanged iNotify = m_dataContext as INotifyPropertyChanged;
                                Debug.Assert(null != iNotify);
                                iNotify.PropertyChanged -= OnDataPropertyChanged;
                            }

                            m_dataContext = objValue;
                            if (null != m_dataContext &&
                                 m_dataContext is INotifyPropertyChanged)
                            {
                                INotifyPropertyChanged iNotify = m_dataContext as INotifyPropertyChanged;
                                Debug.Assert(null != iNotify);
                                iNotify.PropertyChanged += OnDataPropertyChanged;
                            }

                            //lock ( m_synElementLock ) 
                            {
                                foreach (var screenElement in m_dicScreenElements)
                                {
                                    screenElement.Value.SetBindingTarget(m_dataContext);
                                }
                            }
                        }
                        break;

                    default:
                        {
                            return false;
                        }
                }
            }
            else if (strElement.Equals(SpecialElement.s_Focus, StringComparison.OrdinalIgnoreCase))
            {
                if (null != m_focusElement)
                {
                    m_focusElement.SetPropertyValue(strProperty, objValue);
                }
            }
            else
            {
                if (m_dicScreenElements.ContainsKey(strElement))
                {
                    return m_dicScreenElements[strElement].SetPropertyValue(strProperty, objValue);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public bool GetPropertyValueOfElement(string strElement,
                                              string strProperty,
                                              out object objValue)
        {
            objValue = null;

            Debug.Assert(!string.IsNullOrEmpty(strProperty));

            if (null == m_curHtmlDocument)
            {
                return false;
            }

            if (string.IsNullOrEmpty(strElement))
            {

            }
            else
            {
                if (m_dicScreenElements.ContainsKey(strElement))
                {
                    return m_dicScreenElements[strElement].GetPropertyValue(strProperty, out objValue);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public void EnumElements(UIServiceProtocol.EnumElementHandler Handler,
                                  UIServiceProtocol.ElementType Type,
                                  UIServiceProtocol.EnumFlag Flag,
                                  string NameOfElement,
                                  object Param)
        {
            Debug.Assert(null != Handler);

            //name
            Regex reg = null;
            if (!string.IsNullOrEmpty(NameOfElement))
            {
                reg = new Regex(NameOfElement, RegexOptions.IgnoreCase);
            }

            bool continueEnum = true;

            foreach (var ele in m_dicScreenElements)
            {
                if ((ElementType.Button == Type) &&
                      !(ele.Value is HtmlScreenButtonElement))
                {
                    continue;
                }

                if ((ElementType.Input == Type) &&
                     !(ele.Value is HtmlScreenInputElement))
                {
                    continue;
                }

                if (EnumFlag.OnlyEnable == Flag &&
                     !(ele.Value.IsEnable))
                {
                    continue;
                }

                if (EnumFlag.OnlyVisible == Flag &&
                     !(ele.Value.IsVisible))
                {
                    continue;
                }

                if (EnumFlag.VisibleAndEnable == Flag &&
                     !(ele.Value.IsVisible && ele.Value.IsEnable))
                {
                    continue;
                }

                if (EnumFlag.VisibleOrEnable == Flag &&
                     (!ele.Value.IsEnable && !ele.Value.IsVisible))
                {
                    continue;
                }

                if (null != reg)
                {
                    Match m = reg.Match(ele.Value.HostedElement.Id);
                    if (!m.Success)
                    {
                        continue;
                    }
                }

                continueEnum = Handler.Invoke(ele.Value.HostedElement.Id, ele.Value.Key, Param);
                if (!continueEnum)
                {
                    break;
                }
            }
        }

        public object ExecuteScriptCommand(string name,
                                            params object[] args)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));
            if (null == m_curHtmlDocument)
            {
                return null;
            }

            return m_curHtmlDocument.InvokeScript(name, args);
        }

        //public bool ShowScreenKeyboard(bool argShow)
        //{
        //    try
        //    {
        //        if (argShow &&
        //             null != s_screenKeyboard &&
        //             s_screenKeyboard.WaitForExit(0))
        //        {
        //            s_screenKeyboard.Close();
        //            s_screenKeyboard.Dispose();
        //            s_screenKeyboard = null;
        //        }

        //        if (argShow && null == s_screenKeyboard)
        //        {
        //            try
        //            {
        //                if (File.Exists(Environment.SystemDirectory + "\\osk.exe"))
        //                {
        //                    s_screenKeyboard = new Process();
        //                    s_screenKeyboard.StartInfo.UseShellExecute = true;
        //                    s_screenKeyboard.StartInfo.FileName = "osk.exe";
        //                    s_screenKeyboard.StartInfo.Arguments = "";
        //                    s_screenKeyboard.Start();

        //                    //s_screenKeyboard.WaitForInputIdle();
        //                }
        //                else
        //                {
        //                    return false;
        //                }
        //            }
        //            catch (System.Exception ex)
        //            {
        //                if (null != s_screenKeyboard)
        //                {
        //                    s_screenKeyboard.Kill();
        //                    s_screenKeyboard.WaitForExit();
        //                    s_screenKeyboard.Close();
        //                    s_screenKeyboard.Dispose();
        //                    s_screenKeyboard = null;
        //                }

        //                return false;
        //            }
        //        }
        //        //if ( argShow )
        //        //{
        //        //    if (!NativeWndApi.IsWindowVisible(s_screenKeyboard.MainWindowHandle))
        //        //    {
        //        //        NativeWndApi.ShowWindow(s_screenKeyboard.MainWindowHandle, NativeWndApi.SW_SHOWNA);
        //        //    }
        //        //}
        //        //else
        //        if (!argShow && null != s_screenKeyboard)
        //        {
        //            //if ( NativeWndApi.IsWindowVisible( s_screenKeyboard.MainWindowHandle ))
        //            {
        //                s_screenKeyboard.Kill();
        //                s_screenKeyboard.WaitForExit();
        //                s_screenKeyboard.Close();
        //                s_screenKeyboard.Dispose();
        //                s_screenKeyboard = null;
        //            }
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        Log.UIService.LogWarn("Failed to show screen keyboard", ex);
        //        s_screenKeyboard.Close();
        //        s_screenKeyboard.Dispose();
        //        s_screenKeyboard = null;
        //        return false;
        //    }

        //    return true;
        //}

        public object ExecuteCustomCommand(string argName,
                                            params object[] args)
        {
            Debug.Assert(!string.IsNullOrEmpty(argName));
            //Log.UIService.LogDebugFormat("execute command[{0}] in html screen", argName);
            switch (argName)
            {
                case UIServiceCommands.s_clearDataBind:
                    {
                        //Log.UIService.LogDebug("Remove property changed event");
                        RemovePropertyChangedEvent();

                        //ShowScreenKeyboard(false);
                    }
                    break;

                case UIServiceCommands.s_updateData:
                    {
                        //Log.UIService.LogDebug("Prepare for update data");

                        foreach (var item in m_dicScreenElements)
                        {
                            if ((item.Value is HtmlScreenInputElement) ||
                                 (item.Value is HtmlScreenSelectElement) ||
                                 (item.Value is HtmlScreenListElement) ||
                                 (item.Value is HtmlScreenButtonElement))
                            {
                                item.Value.UpdateBindingData(true);
                            }
                        }
                    }
                    break;

                case UIServiceCommands.s_buttonDown:
                    {
                        if (null == args[0] ||
                             !(args[0] is string))
                        {
                            return null;
                        }

                        //Log.UIService.LogDebugFormat("The button [{0}] is down", args[0]);
                        string name = (string)args[0];
                        HtmlScreenElementBase element = null;
                        if (m_dicScreenElements.TryGetValue(name, out element))
                        {
                            element.OnLeftMouseDown(null, null);
                        }
                        //foreach (var item in m_dicScreenElements)
                        //{
                        //    if ( (item.Value is HtmlScreenButtonElement) &&
                        //         (name.Equals( item.Value.HostedElement.Id, StringComparison.OrdinalIgnoreCase )) )
                        //    {
                        //        item.Value.OnLeftMouseDown( null, null );
                        //        break;
                        //    }
                        //}
                    }
                    break;

                case UIServiceCommands.s_buttonUp:
                    {
                        if (null == args[0] ||
                            !(args[0] is string))
                        {
                            return null;
                        }

                        // Log.UIService.LogDebugFormat("The button [{0}] is up", args[0]);
                        string name = (string)args[0];
                        HtmlScreenElementBase element = null;
                        if (m_dicScreenElements.TryGetValue(name, out element))
                        {
                            element.OnLeftMouseUp(null, null, false);
                        }
                        //foreach (var item in m_dicScreenElements)
                        //{
                        //    if ((item.Value is HtmlScreenButtonElement) &&
                        //         (name.Equals(item.Value.HostedElement.Id, StringComparison.OrdinalIgnoreCase)))
                        //    {
                        //        item.Value.OnLeftMouseUp(null, null);
                        //        break;
                        //    }
                        //}
                    }
                    break;

                //case UIServiceCommands.s_buttonClick:
                //    {
                //        if (null == args[0] ||
                //            !( args[0] is string ) )
                //        {
                //            return null;
                //        }

                //        string name = (string)args[0];
                //        HtmlScreenElementBase element = null;
                //        if (m_dicScreenElements.TryGetValue(name.ToLowerInvariant(), out element))
                //        {
                //            //element.OnLeftMouseUp(null, null);
                //            element.OnLeftMouseDown(null,null);
                //            //Thread.Sleep(1000);
                //            //element.OnLeftMouseUp(null, null, false);
                //        }
                //    }
                //    break;

                default:
                    {
                        Log.UIService.LogDebug("The unsupported custom command");
                        Debug.Assert(false);
                    }
                    break;
            }
            return null;
        }

        public void ChangeLanguage(string argLanguage)
        {
            //m_htmlFrontPage.CurrentLanguage = argLanguage;
        }

        public ResourceItem FindResourceInHtml(string argKey)
        {
            Debug.Assert(!string.IsNullOrEmpty(argKey));
            if (null == OwnerScreen ||
                 OwnerScreen.Templates.Count == 0)
            {
                Debug.Assert(null != m_application.GlobalHtmlScreen);
                if (m_application.GlobalHtmlScreen.Templates.Count > 0)
                {
                    foreach (var item in m_application.GlobalHtmlScreen.Templates)
                    {
                        if (item.Name.Equals(argKey, StringComparison.Ordinal))
                        {
                            return item;
                        }
                    }
                }
            }
            else
            {
                foreach (var item in OwnerScreen.Templates)
                {
                    if (item.Name.Equals(argKey, StringComparison.Ordinal))
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        public void SetDataContext(object obj)
        {
             m_dataContext = obj;
        }
        private void RemovePropertyChangedEvent()
        {
            //  Log.UIService.LogDebug("Enter RemovePropertyChangedEvent");
            if (null != m_dataContext &&
                m_dataContext is INotifyPropertyChanged)
            {
                INotifyPropertyChanged iNotify = (INotifyPropertyChanged)m_dataContext;
                iNotify.PropertyChanged -= OnDataPropertyChanged;
                m_dataContext = null;
            }
            // Log.UIService.LogDebug("Leave RemovePropertyChangedEvent");
        }

        private void OnLeftButtonDown(object sender,
                                System.Windows.Forms.HtmlElementEventArgs arg)
        {
            System.Windows.Forms.HtmlDocument doc = (System.Windows.Forms.HtmlDocument)sender;
            System.Windows.Forms.HtmlElement element = doc.GetElementFromPoint(arg.ClientMousePosition);
            if (null == element)
            {
                if (null != m_focusElement)
                {
                    m_focusElement.killFocus();
                    m_focusElement = null;
                }
                arg.ReturnValue = true;
                return;
            }

            HtmlScreenElementBase newFcsElement = FindFocusElement(element);
            if (null != newFcsElement)
            {
                if (m_focusElement != newFcsElement)
                {
                    if (null != m_focusElement)
                    {
                        m_focusElement.killFocus();
                    }
                    m_focusElement = newFcsElement;
                    m_focusElement.SetFocus();
                }
            }
            else
            {
                if (null != m_focusElement)
                {
                    m_focusElement.killFocus();
                    m_focusElement = null;
                }
            }

            HtmlScreenElementBase button = FindButtonInHtml(element);
            if (null == button)
            {
                //System.Windows.Forms.HtmlElement parentElement = element.Parent;
                //button = FindButtonInHtml(parentElement);
                // b226+spb78版本，只判断了 Parent 是否 Button 类型，实际开发过程中，HTML 会嵌套多层级，
                // 所以需要判断多级父节点是否 Button 类型    lywang  20160711
                HtmlElement parentElement = element.Parent;
                while (parentElement != null)
                {
                button = FindButtonInHtml(parentElement);
                    // 如果找到了父级按钮，则跳出循环
                    if (button != null)
                    {
                        break;
                    }
                    else
                    {
                        parentElement = parentElement.Parent;
                    }
                }
                if (null == button)
                {
                    return;
                }
            }

            button.OnLeftMouseDown(sender, arg);
            m_downButtonElement = button;
            arg.ReturnValue = true;

            element.DragEnd += element_DragEnd;
            try
            {
                object objID = null;
                button.GetPropertyValue("id", out objID);
                if (objID != null)
                {
                    m_application.OnButtonClickInHtml(button.HostedElement.Id, button.Key, null);
                }
                //m_downButtonElement.OnLeftMouseUp(sender, arg);
            }
            catch (System.Exception ex)
            {

            }
        }

        void element_DragEnd(object sender, HtmlElementEventArgs e)
        {
            OnLeftButtonUp(sender, e);
        }

        private void OnLeftButtonUp(object sender,
                                     System.Windows.Forms.HtmlElementEventArgs arg)
        {
            try
            {

            if (null != m_downButtonElement)
            {
                m_downButtonElement.HostedElement.DragEnd -= element_DragEnd;
            }

            HtmlElement element = null;
            if (sender is HtmlElement)
            {
                element = (HtmlElement)sender;
                if (!element.Enabled)
                {
                    return;
                }
            }
            else
            {
                if (HtmlRender.SingleInstance.OwnerScreen.HandleLButtonUp(arg.MousePosition))
                {
                    return;
                }

                System.Windows.Forms.HtmlDocument doc = (System.Windows.Forms.HtmlDocument)sender;
                element = doc.GetElementFromPoint(arg.ClientMousePosition);
                if (null == element)
                {
                    m_downButtonElement.OnLeftMouseUp(sender, arg);
                    arg.ReturnValue = true;
                    m_downButtonElement = null;
                    return;
                }
            }

            if (null == m_downButtonElement)
            {
                arg.ReturnValue = true;
                return;
            }

            HtmlScreenElementBase button = FindButtonInHtml(element);
            if (null == button)
            {
                    //System.Windows.Forms.HtmlElement parentElement = element.Parent;
                    //button = FindButtonInHtml(parentElement);
                    // b226+spb78版本，只判断了 Parent 是否 Button 类型，实际开发过程中，HTML 会嵌套多层级，
                    // 所以需要判断多级父节点是否 Button 类型    lywang  20160711
                    HtmlElement parentElement = element.Parent;
                    while (parentElement != null)
                    {
                button = FindButtonInHtml(parentElement);
                        // 如果找到了父级按钮，则跳出循环
                        if (button != null)
                        {
                            break;
                        }
                        else
                        {
                            parentElement = parentElement.Parent;
                        }
                    }
                if (null == button)
                {
                    m_downButtonElement.OnLeftMouseUp(sender, arg);
                    arg.ReturnValue = true;
                    m_downButtonElement = null;
                    return;
                }
            }

            if (button != m_downButtonElement)
            {
                m_downButtonElement.OnLeftMouseUp(sender, arg);
            }
            else
            {
                button.OnLeftMouseUp(sender, arg);
            }
            m_downButtonElement = null;
            arg.ReturnValue = true;

            //Message msg = new Message();
            //int result = 0;
            //while (true)
            //{
            //    result = NativeWndApi.PeekMessage(ref msg,
            //                                 IntPtr.Zero,
            //                                 NativeWndApi.WM_LBUTTONDOWN,
            //                                 NativeWndApi.WM_LBUTTONUP,
            //                                 NativeWndApi.PM_REMOVE);
            //    if ( 0 == result )
            //    {
            //        break;
            //    }
            //}
        }
            catch (Exception ex)
            {
                Log.UIService.LogWarn("html render onLeftButtonUp error", ex);
            }

        }

        private void RemoveEventHandlesOfHtmlDocument(System.Windows.Forms.HtmlDocument doc)
        {
            if (null == doc)
            {
                return;
            }
            //  doc.Click -= m_clickHandler;
            doc.Click -= m_document_Click;
            doc.MouseDown -= m_mouseDownHandler;
            doc.ContextMenuShowing -= m_contextMenuHandler;
            doc.MouseUp -= m_mouseUpHandler;
        }

        private void AddEventHandlesOfHtmlDocument(System.Windows.Forms.HtmlDocument doc)
        {
            if (null == doc)
            {
                return;
            }

            Debug.Assert(null != doc);

            // doc.Click += m_clickHandler;
            doc.Click += m_document_Click;
            doc.MouseDown += m_mouseDownHandler;
            doc.ContextMenuShowing += m_contextMenuHandler;
            doc.MouseUp += m_mouseUpHandler;
        }

        private void ExtractAllScreenElements(System.Windows.Forms.HtmlDocument argDoc)
        {
            Debug.Assert(null != argDoc);

            System.Windows.Forms.HtmlElement body = argDoc.Body;
            if (null == body)
            {
                return;
            }

            m_listBindingExpresses.Clear();
            ClearScreenElements();

            ExtractAllElements(body, m_dicScreenElements);
            //System.Windows.Forms.HtmlElementCollection elementCollection = body.All;
            //Debug.Assert(null != elementCollection);
            //HtmlScreenElementBase screenElement = null;
            //string idsValue = null;
            //string valueOfAttri = null;
            //IResourceService iCurrentUIResource = null;
            //if (null != m_application.ResourceManager)
            //{
            //    iCurrentUIResource = m_application.ResourceManager.CurrentUIResource;
            //}
            //string text = null;
            //string replaceAttri = null;
            //int replaceValue = 0;
            //string imagePath = null;
            //string srcPath = null;
            //foreach (System.Windows.Forms.HtmlElement element in elementCollection)
            //{
            //    //replace ids
            //    idsValue = element.GetAttribute(s_UITextKey);
            //    if (null != iCurrentUIResource &&
            //        !string.IsNullOrEmpty(idsValue))
            //    {
            //        //if (iCurrentUIResource.LoadString(idsValue, TextCategory.s_UI, out text))
            //        iCurrentUIResource.LoadString(idsValue, TextCategory.s_UI, out text);
            //        {
            //            element.InnerHtml = ConvertText(text);
            //        }
            //    }
            //    //replace image
            //    if (null != iCurrentUIResource &&
            //         s_imgName.Equals(element.TagName, StringComparison.OrdinalIgnoreCase))
            //    {
            //        replaceAttri = element.GetAttribute(s_replaceAttri);
            //        srcPath = element.GetAttribute("srcKey");
            //        if (!string.IsNullOrEmpty(replaceAttri) &&
            //             !string.IsNullOrEmpty(srcPath) &&
            //             int.TryParse(replaceAttri, out replaceValue) &&
            //             replaceValue == 1)
            //        {
            //            if (iCurrentUIResource.QueryImagePath(srcPath, out imagePath))
            //            {
            //                element.SetAttribute("src", imagePath);
            //            }
            //        }
            //    }

            //    if (string.IsNullOrEmpty(element.Id))
            //    {
            //        continue;
            //    }

            //    valueOfAttri = element.GetAttribute(s_typeAttri);
            //    if ( string.IsNullOrEmpty(valueOfAttri) )
            //    {
            //        continue;
            //    }

            //    valueOfAttri = valueOfAttri.ToLowerInvariant();
            //    if (m_dicScreenElementCreator.ContainsKey(valueOfAttri))
            //    {
            //        try
            //        {
            //            screenElement = m_dicScreenElementCreator[valueOfAttri].Invoke(element);
            //            if (null != screenElement)
            //            {
            //                m_dicScreenElements.Add(element.Id, screenElement);
            //            }
            //        }
            //        catch (System.Exception ex)
            //        {
            //            LogProcessorService.Log.UIService.LogWarn(string.Format("Failed to create a screen element[{0}]", element.Id), ex);
            //        }
            //    }
            //}
        }

        private HtmlScreenElementBase CreateButtonElement(System.Windows.Forms.HtmlElement argElement)
        {
            Debug.Assert(null != argElement);

            HtmlScreenElementBase screenButton = new HtmlScreenButtonElement(argElement, this);
            screenButton.Open();
            screenButton.ParseBindingExpress();
            screenButton.ParseResource();

            return screenButton;
        }

        private HtmlScreenElementBase CreateMediaElement(System.Windows.Forms.HtmlElement argElement)
        {
            HtmlScreenMediaElement mediaElement = new HtmlScreenMediaElement(argElement, this);
            if (!mediaElement.Open())
            {
                Log.UIService.LogError("Failed to create a media element");
                return null;
            }
            mediaElement.ParseBindingExpress();

            return mediaElement;
        }

        private HtmlScreenElementBase CreateImageElement(System.Windows.Forms.HtmlElement argElement)
        {
            HtmlScreenImageElement imgElement = new HtmlScreenImageElement(argElement, this);
            imgElement.Open();
            imgElement.ParseBindingExpress();

            return imgElement;
        }

        private HtmlScreenElementBase CreateListElement(System.Windows.Forms.HtmlElement argElement)
        {
            Debug.Assert(null != argElement);

            if (!string.Equals(argElement.TagName, "table", StringComparison.OrdinalIgnoreCase))
            {
                Log.UIService.LogWarn("The element of list type must be <table> in html");
                return null;
            }

            HtmlScreenElementBase screenListElement = new HtmlScreenListElement(argElement, this);
            if (!screenListElement.Open())
            {
                Log.UIService.LogError("Failed to open list element");
                return null;
            }
            screenListElement.ParseBindingExpress();

            return screenListElement;
        }

        private HtmlScreenElementBase CreateCountDownElement(System.Windows.Forms.HtmlElement argElement)
        {
            Debug.Assert(null != argElement);

            HtmlScreenElementBase screenCountDown = new HtmlScreenCountdownElement(argElement, this);
            screenCountDown.Open();
            screenCountDown.ParseBindingExpress();

            return screenCountDown;
        }

        private HtmlScreenElementBase CreateSelectElement(System.Windows.Forms.HtmlElement argElement)
        {
            Debug.Assert(null != argElement);

            HtmlScreenElementBase selectElement = new HtmlScreenSelectElement(argElement, this);
            selectElement.Open();
            selectElement.ParseBindingExpress();

            return selectElement;
        }

        private HtmlScreenElementBase CreateInputElement(System.Windows.Forms.HtmlElement argElement)
        {
            Debug.Assert(null != argElement);

            string format = argElement.GetAttribute(s_formatAttri);

            HtmlScreenInputElement screenInput = new HtmlScreenInputElement(argElement, this);
            screenInput.Open();
            if (!string.IsNullOrEmpty(format))
            {
                screenInput.FormatPattern = format;
            }
            screenInput.ParseBindingExpress();

            //show virtual keyboard
            string showVKeyboard = argElement.GetAttribute(UIPropertyKey.s_showVirtualKeyboard);
            if (!string.IsNullOrEmpty(showVKeyboard))
            {
                int value = 0;
                if (int.TryParse(showVKeyboard, out value))
                {
                    screenInput.CanShowVirtualKeyboard = value == 0 ? false : true;
                }
                else
                {
                    screenInput.CanShowVirtualKeyboard = false;
                }
            }

            return screenInput;
        }

        private HtmlScreenElementBase FindButtonInHtml(System.Windows.Forms.HtmlElement argElement)
        {
            if (null == argElement)
            {
                return null;
            }

            string id = argElement.Id;
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (m_dicScreenElements.ContainsKey(id))
            {
                HtmlScreenElementBase element = m_dicScreenElements[argElement.Id];
                return element is HtmlScreenButtonElement ? element : null;
            }
            else
            {
                HtmlScreenElementBase element = null;
                foreach (var item in m_dicScreenElements)
                {
                    if (item.Value is HtmlScreenListElement)
                    {
                        element = item.Value.FindHtmlScreenElement(argElement);
                        if (null != element)
                        {
                            return (element is HtmlScreenButtonElement ? element : null);
                        }
                    }
                }
                // 如果找不到按钮，则查找下是否通过 JS 在页面加载完成后添加的按钮
                if (element == null)
                {
                    element = FindButtonInHtmlCreatedByJS(argElement);
                    if (element != null)
                    {
                        return (element is HtmlScreenButtonElement ? element : null);
                    }
                }

                return null;
            }
            //if (string.IsNullOrEmpty(id) ||
            //     !m_dicScreenElements.ContainsKey(id) )
            //{
            //    return null;
            //}
        }

        /// <summary>
        /// 查找通过 javascript，在 html 页面加载完成后产生的 Button
        /// </summary>
        /// <param name="argElement"></param>
        /// <returns></returns>
        private HtmlScreenElementBase FindButtonInHtmlCreatedByJS(System.Windows.Forms.HtmlElement argElement)
        {
            // 如果 Id 为空，则返回 null
            if (string.IsNullOrEmpty(argElement.Id))
            {
                return null;
            }

            var valueOfAttri = argElement.GetAttribute(s_typeAttri);
            if (string.IsNullOrEmpty(valueOfAttri))
            {
                return null;
            }
            HtmlScreenElementBase screenElement = null;
            //
            valueOfAttri = valueOfAttri.ToLowerInvariant();
            // 只有按钮才做处理
            if (m_dicScreenElementCreator.ContainsKey(valueOfAttri) && valueOfAttri == "button")
            {
                try
                {
                    screenElement = m_dicScreenElementCreator[valueOfAttri].Invoke(argElement);
                    if (null != screenElement && screenElement is HtmlScreenButtonElement)
                    {
                        m_dicScreenElements.Add(argElement.Id, screenElement);
                    }
                }
                catch (System.Exception ex)
                {
                    LogProcessorService.Log.UIService.LogWarn(string.Format("Failed to create a screen element[{0}]", argElement.Id), ex);
                }
            }
            return screenElement;
        }

        public HtmlScreenElementBase FindElement(string argName)
        {
            //Debug.Assert(!string.IsNullOrEmpty(argName));
            if (string.IsNullOrEmpty(argName))
            {
                return null;
            }

            HtmlScreenElementBase element = null;
            if (m_dicScreenElements.TryGetValue(argName, out element))
            {
                return element;
            }

            return null;
        }

        private HtmlScreenElementBase FindFocusElement(System.Windows.Forms.HtmlElement argElement)
        {
            Debug.Assert(null != argElement);
            if (string.IsNullOrEmpty(argElement.Id))
            {
                return null;
            }

            HtmlScreenElementBase element = null;
            if (m_dicScreenElements.TryGetValue(argElement.Id, out element) &&
                 element.CanFocus)
            {
                return element;
            }

            return null;
        }

        private void OnDataPropertyChanged(object sender,
                                      PropertyChangedEventArgs arg)
        {
            if (string.IsNullOrEmpty(arg.PropertyName))
            {
                return;
            }

            m_webBrowserImp.BeginInvoke(m_dataPropChangedHander, arg.PropertyName);
            //this.Dispatcher.BeginInvoke(m_dataPropChangedHander,
            //                             DispatcherPriority.Input,
            //                             arg.PropertyName);
            //         lock ( m_synElementLock )
            //{
            //    foreach (var element in m_dicScreenElements)
            //    {
            //        element.Value.OnPropertyChanged(arg.PropertyName);
            //    }
            //}
        }

        public static string ConvertText(string argInput)
        {
            //Debug.Assert(!string.IsNullOrEmpty (argInput));
            if (string.IsNullOrEmpty(argInput))
            {
                return argInput;
            }
            //对核心规定的三个特殊符号做转义规则，两个代表显示一个
            var doubleLine = (char)0xFF;
            var noDisplayChar = (char)0xFE;
            argInput = argInput.Replace("\\\\", doubleLine.ToString());

            argInput = argInput.Replace("\\|", noDisplayChar.ToString());
            if (-1 != argInput.IndexOf("|", 0))
            {
                argInput = argInput.Replace("|", "<BR>");
            }
            argInput = argInput.Replace(noDisplayChar, '|');

            argInput = argInput.Replace("\\#", noDisplayChar.ToString());
            if (-1 != argInput.IndexOf("#", 0))
            {
                argInput = argInput.Replace("#", "&nbsp;");
            }
            argInput = argInput.Replace(noDisplayChar, '#');

            argInput = argInput.Replace("\\@", noDisplayChar.ToString());
            if (-1 != argInput.IndexOf("@", 0))
            {
                argInput = argInput.Replace('@', '&');
            }
            argInput = argInput.Replace(noDisplayChar, '@');

            argInput = argInput.Replace(doubleLine, '\\');
            return argInput;
        }

        /// <summary>
        /// 排除替換@符號，郵箱地址需要輸入@
        /// </summary>
        /// <param name="argInput"></param>
        /// <returns></returns>
        public static string ConvertTextEx(string argInput)
        {
            //Debug.Assert(!string.IsNullOrEmpty (argInput));
            if (string.IsNullOrEmpty(argInput))
            {
                return argInput;
            }
            //对核心规定的三个特殊符号做转义规则，两个代表显示一个
            var doubleLine = (char)0xFF;
            var noDisplayChar = (char)0xFE;
            argInput = argInput.Replace("\\\\", doubleLine.ToString());

            argInput = argInput.Replace("\\|", noDisplayChar.ToString());
            if (-1 != argInput.IndexOf("|", 0))
            {
                argInput = argInput.Replace("|", "<BR>");
            }
            argInput = argInput.Replace(noDisplayChar, '|');

            argInput = argInput.Replace("\\#", noDisplayChar.ToString());
            if (-1 != argInput.IndexOf("#", 0))
            {
                argInput = argInput.Replace("#", "&nbsp;");
            }
            argInput = argInput.Replace(noDisplayChar, '#');

            //argInput = argInput.Replace("\\@", noDisplayChar.ToString());
            //if (-1 != argInput.IndexOf("@", 0))
            //{
            //    argInput = argInput.Replace('@', '&');
            //}
            //argInput = argInput.Replace(noDisplayChar, '@');

            argInput = argInput.Replace(doubleLine, '\\');
            return argInput;
        }

        private void OnPropertyChanged(string argName)
        {
            foreach (var element in m_dicScreenElements)
            {
                element.Value.OnPropertyChanged(argName);
            }
        }

        public void NotifyPropertyChanged(string argElement,
                                           string argProperty,
                                           string argValue)
        {
            Debug.Assert(null != m_application);
            foreach (var item in m_listBindingExpresses)
            {
                if (item.CanTrigger(argElement,
                                      argProperty))
                {
                    item.SetBindValue(argProperty, argValue);
                }
            }

            m_application.NotifyPropertyChanged(argElement, argProperty, argValue);
        }

        public void ClearScreenElements()
        {
            // Log.UIService.LogDebug("Enter ClearScreenElements");
            // lock ( m_synElementLock )
            {
                foreach (var ele in m_dicScreenElements)
                {
                    ele.Value.Dispose();
                }
                m_dicScreenElements.Clear();
            }
            m_focusElement = null;
            m_downButtonElement = null;
            //   Log.UIService.LogDebug("Leave ClearScreenElements");
        }

        public IResourceManager ResourceManager
        {
            get
            {
                return m_application.ResourceManager;
            }
        }

        public htmlScreen OwnerScreen
        {
            get;
            set;
        }

        private void ChangeCssResource()
        {
            if (null == m_application.ResourceManager)
            {
                return;
            }

            Debug.Assert(null != m_curHtmlDocument);
            var elementlist = m_curHtmlDocument.GetElementsByTagName(s_linkName);
            string replaceAttri = null;
            string hrefAttri = null;
            int replace = 0;
            string language = null;

            foreach (System.Windows.Forms.HtmlElement item in elementlist)
            {
                replaceAttri = item.GetAttribute(s_replaceAttri);
                hrefAttri = item.GetAttribute("href");
                if (!string.IsNullOrEmpty(replaceAttri) &&
                     !string.IsNullOrEmpty(hrefAttri) &&
                     int.TryParse(replaceAttri, out replace) &&
                     replace == 1)
                {
                    language = System.IO.Path.GetFileNameWithoutExtension(hrefAttri);
                    if (!m_application.ResourceManager.CurrentUIResource.Language.Equals(language, StringComparison.OrdinalIgnoreCase))
                    {
                        string directPath = System.IO.Path.GetDirectoryName(hrefAttri);
                        string newPath = System.IO.Path.Combine(directPath, m_application.ResourceManager.CurrentUIResource.Language + System.IO.Path.GetExtension(hrefAttri));
                        item.SetAttribute("href", newPath.Replace(@"\", "/"));
                        directPath = null;
                        newPath = null;
                    }
                }
            }
        }

        private void OnContextMenuShow(object sender,
                                System.Windows.Forms.HtmlElementEventArgs arg)
        {
            arg.ReturnValue = true;
        }

        public void ShowScreen()
        {
            if (!m_webBrowserImp.Visible)
            {
                m_webBrowserImp.Show();
                m_webBrowserImp.Activate();
            }

        }

        public HtmlScreenElementBase FindHtmlScreenElementByElement(System.Windows.Forms.HtmlElement argElement)
        {
            Debug.Assert(null != argElement);

            foreach (var item in m_dicScreenElements)
            {
                if (item.Value.HostedElement == argElement)
                {
                    return item.Value;
                }
            }

            return null;
        }
        #endregion

        #region property
        public bool LastScrIsXDCForm = false;
        public bool IsVisible
        {
            get
            {
                Debug.Assert(null != m_webBrowserImp);
                return m_webBrowserImp.Visible;
            }
        }

        public int Left
        {
            get
            {
                return m_webBrowserImp.Left;
            }
            set
            {
                m_webBrowserImp.Left = value;
            }
        }

        public int Top
        {
            get
            {
                return m_webBrowserImp.Top;
            }
            set
            {
                m_webBrowserImp.Top = value;
            }
        }

        public int Width
        {
            get
            {
                return m_webBrowserImp.Width;
            }
            set
            {
                m_webBrowserImp.Width = value;
            }
        }

        public int Height
        {
            get
            {
                return m_webBrowserImp.Height;
            }
            set
            {
                m_webBrowserImp.Height = value;
            }
        }

        public bool Topmost
        {
            get
            {
                return m_webBrowserImp.TopMost;
            }
            set
            {
                m_webBrowserImp.TopMost = value;
            }
        }

        public bool ShowCursor
        {
            get
            {
                return m_webBrowserImp.Cursor == Cursors.No ? false : true;
            }
            set
            {
                if (value)
                {
                    m_webBrowserImp.Cursor = Cursors.Arrow;
                }
                else
                {
                    m_webBrowserImp.Cursor = Cursors.No;
                }
            }
        }

        public MultiWebbrowserImp Browser
        {
            get
            {
                return m_webBrowserImp;
            }
        }
        #endregion

        #region field
        //     private Page4Html m_curHtmlPage = null;

        //        private Page4Html m_htmlFrontPage = null;

        private MultiWebbrowserImp m_webBrowserImp = null;

        private System.Windows.Forms.HtmlDocument m_curHtmlDocument = null;
        private System.Windows.Forms.HtmlElementEventHandler m_document_Click = null;

        private System.Windows.Forms.HtmlElementEventHandler m_mouseDownHandler = null;

        private System.Windows.Forms.HtmlElementEventHandler m_contextMenuHandler = null;

        private System.Windows.Forms.HtmlElementEventHandler m_mouseUpHandler = null;

        private Dictionary<string, HtmlScreenElementBase> m_dicScreenElements = new Dictionary<string, HtmlScreenElementBase>();

        private Dictionary<string, Func<System.Windows.Forms.HtmlElement, HtmlScreenElementBase>> m_dicScreenElementCreator = new Dictionary<string, Func<System.Windows.Forms.HtmlElement, HtmlScreenElementBase>>();

        private List<ElementBindingExpress> m_listBindingExpresses = new List<ElementBindingExpress>();

        private Action<string> m_dataPropChangedHander = null;

        private UIServiceWpfApp m_application = null;

        private object m_dataContext = null;

        private HtmlScreenElementBase m_focusElement = null;

        private HtmlScreenElementBase m_downButtonElement = null;

        private static HtmlRender s_Render = null;

        //        private Process s_screenKeyboard = null;

        public const string s_UITextKey = "ids";

        public const string s_imgName = "img";

        public const string s_linkName = "link";

        public const string s_replaceAttri = "replace";

        public const char s_placeholderBr = '|';

        public const char s_placeholderSharp = '#';

        public const char s_placeholderAt = '@';

        public const string s_typeAttri = "type";

        public const string s_formatAttri = "format";

        public const string s_tagAttri = "tag";

        public const string s_hideStyle = "display: none";

        public const string s_resourceNode = "Resource";

        public const string s_dataTemplateNode = "DataTemplate";
        #endregion
    }
}
