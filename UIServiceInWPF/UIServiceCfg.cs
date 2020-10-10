/********************************************************************
	FileName:   UIServiceCfg
    purpose:	

	author:		huang wei
	created:	2013/01/08

    revised history:
	2013/01/08  

================================================================
    Copyright (C) 2013, Grgbanking CO,. Ltd. All rights reserved.
================================================================
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;
using UIServiceInWPF.screen;
using System.Reflection;
using UIServiceProtocol;
using System.IO;
using LogProcessorService;
using UIServiceInWPF.trigger;
using UIServiceInWPF.action;
using ResourceManagerProtocol;
using UIServiceInWPF.GrgStoryboardNS;
using UIServiceInWPF.Resource;
using UIServiceInWPF.HtmlScreenElement;

namespace UIServiceInWPF
{
    public class WindowConfig
    {
        public bool FullScreen
        {
            get;
            set;
        }

        public bool ShowCursor
        {
            get;
            set;
        }

        public bool Topmost
        {
            get;
            set;
        }

        public int Left
        {
            get;
            set;
        }

        public int Top
        {
            get;
            set;
        }

        public int Width
        {
            get;
            set;
        }

        public int Height
        {
            get;
            set;
        }

        public void Reset()
        {
            FullScreen = false;
            ShowCursor = false;
            Topmost = false;
            Left = 0;
            Top = 0;
            Width = 0;
            Height = 0;
        }
    }

    public class UIServiceCfg
    {
        #region constructor
        public UIServiceCfg()
        {
            m_wndConfig.ShowCursor = false;
            m_wndConfig.Topmost = false;
            m_wndConfig.FullScreen = false;

            UrlPrefix = s_fileProto;
            m_IsLocalHost = true;
        }
        #endregion
        private static string xmlPath = AppDomain.CurrentDomain.BaseDirectory + @"Resource\UIConfig\";
        private const string searchPattern = "*_UIService.xml";
        private XmlDocument otherDoc;
        private XmlNodeList xmlList;
        private XmlNode xmlNod;
        private string uiName = string.Empty;
        private XmlAttribute xmlAttr;
        #region method
        public bool Load(string strPath)
        {
            //for test
            //return true;
            Debug.Assert(!string.IsNullOrEmpty(strPath));

            LogProcessorService.Log.UIService.LogDebugFormat("Prepare for loading configuration [{0}] of UIService in wpf", strPath);

            try
            {
                m_strCfg = strPath;

                m_objDoc = new XmlDocument();
                m_objDoc.Load(strPath);

                //add Other Screen Config from xml
             //   LoadOtherScreenXml(m_objDoc);

                LogProcessorService.Log.UIService.LogDebug("Load the configuration of a window");
                //load configuration of a window
                LoadWindowConfiguration(m_objDoc);

                LogProcessorService.Log.UIService.LogDebug("Load the <screens> node");
                //load the <screens> node
                LoadScreensNode(m_objDoc);

                LogProcessorService.Log.UIService.LogDebug("Load and bind assemblies");
                //Load and bind assemblies
                LoadAndBindAssemblies(m_objDoc);
            }
            catch (System.Exception ex)
            {
                LogProcessorService.Log.UIService.LogError("Failed to load configuration of UIService in wpf", ex);

                return false;
            }

            LogProcessorService.Log.UIService.LogInfo("Success to load configuration of UIService in WPF");

            return true;
        }

        public virtual void LoadOtherScreenXml(XmlDocument doc)
        {
            if (!Directory.Exists(xmlPath))
            {
                LogProcessorService.Log.UIService.LogDebug("UIConfig does not exist");
                return;
            }

            if (null == otherDoc)
            {
                otherDoc = new XmlDocument();
            }

            string[] fileinfos = Directory.GetFiles(xmlPath, searchPattern);

            LogProcessorService.Log.UIService.LogDebug("load other xml has  " + fileinfos.Count());
            if (fileinfos.Count() > 0)
            {
                foreach (string filePath in fileinfos)
                {
                    otherDoc.Load(filePath);
                    xmlList = otherDoc.SelectNodes("/Screens/Screen");
                    if (null != xmlList && xmlList.Count > 0)
                    {
                        foreach (XmlNode xn in xmlList)
                        {

                            xmlAttr = xn.Attributes["name"];
                            if (null != xmlAttr && !string.IsNullOrEmpty(xmlAttr.Value))
                            {
                                xmlNod = doc.SelectSingleNode("/Window/Screens/Screen[@name='" + xmlAttr.Value + "']");
                                if (null != xmlNod)
                                {
                                    LogProcessorService.Log.UIService.LogError("repeat for  Screen name is " + xmlAttr.Value);
                                }
                                else
                                {
                                    doc.SelectSingleNode("/Window/Screens").AppendChild(doc.ImportNode(xn, true));
                                }
                            }

                        }
                    }
                }
            }

        }

        public void Close()
        {
            m_objDoc.RemoveAll();
            m_objDoc = null;
            m_strCfg = null;
            m_motherboard = null;
            m_IsLocalHost = true;
            m_wndConfig.Reset();
        }

        public void LoadGlobalTriggers( Screen argOwner )
        {
            Debug.Assert(null != m_objDoc && null != argOwner);
            XmlNode triggersNode = m_objDoc.DocumentElement.SelectSingleNode(UIServiceCfgDefines.s_TriggersNode);
            if ( null != triggersNode )
            {
                LoadTriggers(triggersNode, argOwner);
            }
        }

        public void LoadGlobalDataTemplates(Screen argOwner )
        {
            Debug.Assert(null != m_objDoc && null != argOwner);
            XmlNode dataTemplatesNode = m_objDoc.DocumentElement.SelectSingleNode(UIServiceCfgDefines.s_templatesNode);
            if ( null != dataTemplatesNode )
            {
                LoadTemplates(dataTemplatesNode, argOwner);
            }
        }

        public Screen LoadScreen( string strName,
                                  UIServiceWpfApp argApp = null )
        {
            Debug.Assert(!string.IsNullOrEmpty(strName) && null != m_objDoc);

            LogProcessorService.Log.UIService.LogDebugFormat("Prepare for loading the screen with name [{0}]", strName);
            string strScr = string.Format(@"/{0}/{1}/{2}[@name='{3}']", UIServiceCfgDefines.s_WndNode, UIServiceCfgDefines.s_ScreensNode, UIServiceCfgDefines.s_ScreenNode, strName);
            //LogProcessorService.Log.UIService.LogDebugFormat("The xpath value is {0}", strScr);

            XmlNode objNode = m_objDoc.SelectSingleNode(strScr);
            if (null == objNode)
            {
                LogProcessorService.Log.UIService.LogWarnFormat("The screen [{0}] isn't exist", strName);
                return null;
            }

            //
            string strUrl = null;
            string strPath = null;
            string strSite = null;
            string subSite = null;
            string needClearElement = null;
            bool needCache = true;
            //bool validationEnable = false;
            XmlAttribute objAttri = objNode.Attributes[UIServiceCfgDefines.s_UriAttri];
            if (null != objAttri)
            {
                strUrl = objAttri.Value;
                //LogProcessorService.Log.UIService.LogDebugFormat("The url of a page is {0}", strUrl);
            }
            else
            {
                throw new Exception("The url of a page must be exists");
            }

            bool bHtml = IsHtml(strUrl);
            bool bXaml = IsXaml(strUrl);
            if (!bHtml &&
                 !bXaml)
            {
                return null;
            }
            if (bHtml)
            {
                strUrl = strUrl.Replace(@"\", @"/");
            }

            objAttri = objNode.Attributes[UIServiceCfgDefines.s_SiteAttri];
            if (null != objAttri)
            {
                strSite = objAttri.Value;
            }
            objAttri = objNode.Attributes[UIServiceCfgDefines.s_subSiteAttri];
            if (null != objAttri)
            {
                subSite = objAttri.Value;
            }
            objAttri = objNode.Attributes[UIServiceCfgDefines.s_cacheAttri];
            if (null != objAttri &&
                 !string.IsNullOrEmpty(objAttri.Value))
            {
                int temp = 0;
                if (int.TryParse(objAttri.Value, out temp))
                {
                    needCache = temp == 0 ? false : true;
                }
            }

            if (needCache)
            {
                objAttri = objNode.Attributes[UIServiceCfgDefines.s_needClearElementAttri];
                if (null != objAttri &&
                     !string.IsNullOrEmpty(objAttri.Value))
                {
                    needClearElement = objAttri.Value;
                }
            }
            //objAttri = objNode.Attributes[UIServiceCfgDefines.s_validationEnableAttri];
            //if ( null != objAttri &&
            //     !string.IsNullOrEmpty(objAttri.Value) )
            //{
            //    int temp = 0;
            //    if ( int.TryParse( objAttri.Value, out temp ) )
            //    {
            //        validationEnable = temp == 0 ? false : true;
            //    }
            //}
            //else
            //{
            //    strSite = SiteOfScreens;
            //}
            //LogProcessorService.Log.UIService.LogDebugFormat("The site of a page is {0}", strSite);

            //            objAttri = objNode.Attributes[s_PathAttri];
            //if ( null != objAttri )
            //{
            //    strPath = AppDomain.CurrentDomain.BaseDirectory + objAttri.Value;
            //}
            //else
            //{
            if (bHtml)
            {
                strPath = basePath4Html;
            }
            else if (bXaml)
            {
                strPath = basePath4Xaml;
            }
            string configPath = strPath;
            string dir = string.Format(@"{0}{1}/", strPath, this.ResouceManager.CurrentUILanguage);
            Uri uri = new Uri(dir);
            if (uri.IsFile)
            {
                string localPath = uri.LocalPath;
                if (Directory.Exists(localPath))
                {
                    strPath = dir;
                }
            }
            //           }
            //LogProcessorService.Log.UIService.LogDebugFormat("The path of a page is {0}", strPath);

            Screen Scr = null;
            if (bHtml)
            {
                Scr = new htmlScreen()
                {
                    Name = strName,
                    Path = strPath,
                    Url = strUrl,
                    Site = strSite,
                    SubSite = subSite,
                    NeedCache = needCache,
                    ConfigPath = configPath
                };
            }
            else if (bXaml)
            {
                //Scr = new XamlScreen()
                //{
                //    Name = strName,
                //    Path = strPath,
                //    Url = strUrl,
                //    Site = strSite,
                //    SubSite = subSite,
                //    NeedCache = needCache,
                //    ConfigPath = configPath
                //};
            }
            Scr.App = argApp;
            Scr.ResouceManager = this.ResouceManager;
            //Screen objScr = new Screen()
            //{
            //    Name = strName,
            //    Path = strPath,
            //    Url = strUrl,
            //    Site = strSite
            //};
            if (needCache &&
                !string.IsNullOrEmpty(needClearElement))
            {
                string[] arrElements = needClearElement.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in arrElements)
                {
                    Scr.AddNeedClearElement(item);
                }
            }

            //load storyboards of a screen.
            XmlNode storyboards = objNode.SelectSingleNode(UIServiceCfgDefines.s_storyboardsNode);
            if (null != storyboards)
            {
                LoadStoryboards(storyboards, Scr);
            }

            //load triggers of a screen
            XmlNode triggers = objNode.SelectSingleNode(UIServiceCfgDefines.s_TriggersNode);
            if (null != triggers)
            {
                LoadTriggers(triggers, Scr);
            }

            //load data template of a screen
            XmlNode templates = objNode.SelectSingleNode(UIServiceCfgDefines.s_templatesNode);
            if (null != templates)
            {
                LoadTemplates(templates, Scr);
            }

            //LogProcessorService.Log.UIService.LogInfoFormat("Success to load a screen with name [{0}] from a configuration file", strName);

            return Scr;
        }

        public string FindScreenUri(string argName)
        {
            Debug.Assert(!string.IsNullOrEmpty(argName));
            string strScr = string.Format(@"/{0}/{1}/{2}[@name='{3}']", UIServiceCfgDefines.s_WndNode, UIServiceCfgDefines.s_ScreensNode, UIServiceCfgDefines.s_ScreenNode, argName);
            XmlNode objNode = m_objDoc.SelectSingleNode(strScr);
            if (null == objNode)
            {
                LogProcessorService.Log.UIService.LogWarnFormat("The screen [{0}] isn't exist", argName);
                return null;
            }

            XmlAttribute objAttri = objNode.Attributes[UIServiceCfgDefines.s_UriAttri];
            if (null == objAttri ||
                 string.IsNullOrEmpty(objAttri.Value))
            {
                LogProcessorService.Log.UIService.LogWarnFormat("The screen [{0}] isn't exist", argName);
                return null;
            }

            return (basePath4Xaml + objAttri.Value);
        }

        public string FindPopupWindowUri(string argName)
        {
            Debug.Assert(!string.IsNullOrEmpty(argName));
            string xpath = string.Format(@"{0}/{1}[@name='{2}']", UIServiceCfgDefines.s_popupWindowsNode, UIServiceCfgDefines.s_WndNode, argName);
            XmlNode node = m_objDoc.DocumentElement.SelectSingleNode(xpath);
            if (null == node)
            {
                Log.UIService.LogWarnFormat("The popup window [{0}] isn't exist", argName);
                return null;
            }

            XmlAttribute uriAttri = node.Attributes[UIServiceCfgDefines.s_UriAttri];
            if (null == uriAttri ||
                 string.IsNullOrEmpty(uriAttri.Value))
            {
                Log.UIService.LogWarnFormat("The popup window [{0}] isn't exist", argName);
                return null;
            }

            return (basePath4Xaml + uriAttri.Value);
        }

        private void LoadWindowConfiguration(XmlDocument objDoc)
        {
            Debug.Assert(null != objDoc);

            XmlElement objEle = (XmlElement)objDoc.SelectSingleNode(UIServiceCfgDefines.s_WndNode);
            if (null != objEle)
            {
                LogProcessorService.Log.UIService.LogDebug("Load the name of a window");
                //load name
                XmlAttribute objAttri = objEle.Attributes[UIServiceCfgDefines.s_nameAttri];
                if (null != objAttri)
                {
                    NameOfWindow = objAttri.Value;
                }

                //load url of the motherBoard
                objAttri = objEle.Attributes[UIServiceCfgDefines.s_motherBoardAttri];
                if (null != objAttri)
                {
                    m_motherboard = objAttri.Value;
                    LogProcessorService.Log.UIService.LogDebugFormat("Load the url [{0}] of a window", m_motherboard);
                }
                else
                {
                    throw new Exception("The url of a motherBoard must be exist");
                }

                //load initial page
                objAttri = objEle.Attributes[UIServiceCfgDefines.s_InitPageAttri];
                if (null != objAttri)
                {
                    InitPage = objAttri.Value;
                    LogProcessorService.Log.UIService.LogDebugFormat("Load the initial page [{0}] of a window", InitPage);
                }
                int value = 0;
                //load fullscreen
                objAttri = objEle.Attributes[UIServiceCfgDefines.s_fullScreenAttri];
                if (null != objAttri)
                {
                    if (int.TryParse(objAttri.Value, out value))
                    {
                        m_wndConfig.FullScreen = value == 0 ? false : true;
                    }
                }

                //load location
                objAttri = objEle.Attributes[UIServiceCfgDefines.s_locationAttri];
                if (null != objAttri)
                {
                    ParseLocation(objAttri.Value);
                    Log.UIService.LogDebugFormat("WPF window's location : {0} {1} {2} {3}",
                                                 m_wndConfig.Left,
                                                 m_wndConfig.Top,
                                                 m_wndConfig.Width,
                                                 m_wndConfig.Height);
                }
                else if (!m_wndConfig.FullScreen)
                {
                    try
                    {
                        string generalConfigPath = AppDomain.CurrentDomain.BaseDirectory + @"Config\GeneralConfig.xml";
                        if (File.Exists(generalConfigPath))
                        {
                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.Load(generalConfigPath);
                            XmlElement node = xmlDoc.DocumentElement.SelectSingleNode("UI") as XmlElement;
                            if (null != node)
                            {
                                //load location
                                if (node.HasAttribute("location"))
                                {
                                    ParseLocation(node.Attributes["location"].Value);
                                    Log.UIService.LogDebugFormat("WPF window's location : {0} {1} {2} {3}",
                                                                 m_wndConfig.Left,
                                                                 m_wndConfig.Top,
                                                                 m_wndConfig.Width,
                                                                 m_wndConfig.Height);
                                }
                                else if (!m_wndConfig.FullScreen)
                                {
                                    throw new Exception("The location of a WPF window must be exist");
                                }

                                //load topmost
                                if (node.HasAttribute("topmost"))
                                {
                                    if (int.TryParse(node.Attributes["topmost"].Value, out value))
                                    {
                                        m_wndConfig.Topmost = value == 0 ? false : true;
                                        m_wndConfig.ShowCursor = value == 0 ? true : false;
                                    }
                                }
                            }

                            xmlDoc = null;

                            return;
                        }
                        else
                        {
                            throw new Exception("The location of a WPF window must be exist");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Trace.Write(ex.Message);
                        throw new Exception("The location of a WPF window must be exist");
                    }
                }

                //load topmost
                objAttri = objEle.Attributes[UIServiceCfgDefines.s_topmostAttri];
                if (null != objAttri)
                {
                    if (int.TryParse(objAttri.Value, out value))
                    {
                        m_wndConfig.Topmost = value == 0 ? false : true;
                    }
                }

                //load show cursor
                objAttri = objEle.Attributes[UIServiceCfgDefines.s_showCursorAttri];
                if (null != objAttri)
                {
                    if (int.TryParse(objAttri.Value, out value))
                    {
                        m_wndConfig.ShowCursor = value == 0 ? false : true;
                    }
                }
            }
            else
            {
                throw new Exception("The configuration of the window must be exist");
            }
        }

        private void ParseLocation(string argLocation)
        {
            Debug.Assert(!string.IsNullOrEmpty(argLocation));
            if (string.IsNullOrEmpty(argLocation))
            {
                throw new ArgumentNullException("argLocation", "The location of a WPF window must exist");
            }
            Debug.Assert(null != m_wndConfig);

            try
            {
                string[] arrValues = argLocation.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (arrValues.Length != 4)
                {
                    throw new Exception("The location of a WPF window is illegal");
                }
                //left
                int value = 0;
                if (!int.TryParse(arrValues[0], out value))
                {
                    throw new Exception("The left value of the location is illegal");
                }
                m_wndConfig.Left = value;
                //top
                if (!int.TryParse(arrValues[1], out value))
                {
                    throw new Exception("The top value of the location is illegal");
                }
                m_wndConfig.Top = value;
                //width
                if (!int.TryParse(arrValues[2], out value))
                {
                    throw new Exception("The width value of the location is illegal");
                }
                m_wndConfig.Width = value;
                //height
                if (!int.TryParse(arrValues[3], out value))
                {
                    throw new Exception("The height value of the location is illegal");
                }
                m_wndConfig.Height = value;
            }
            catch (System.Exception ex)
            {
                Log.UIService.LogWarn("Failed to parse location of a WPF window", ex);
                if (!m_wndConfig.FullScreen)
                {
                    throw;
                }
            }
        }

        private void LoadScreensNode(XmlDocument objDoc)
        {
            Debug.Assert(null != objDoc);

            LogProcessorService.Log.UIService.LogDebug("Prepare for loading the <screens> node");
            string strPath = string.Format("/{0}/{1}", UIServiceCfgDefines.s_WndNode, UIServiceCfgDefines.s_ScreensNode);
            LogProcessorService.Log.UIService.LogDebugFormat("The xpath is {0}", strPath);

            XmlNode objNode = objDoc.SelectSingleNode(strPath);
            if (null != objNode)
            {
                //load url prefix
                XmlAttribute objAttri = objNode.Attributes[UIServiceCfgDefines.s_urlPrefixAttri];
                if (null != objAttri)
                {
                    UrlPrefix = objAttri.Value;
                }
                //load host
                objAttri = objNode.Attributes[UIServiceCfgDefines.s_hostAttri];
                if (null != objAttri)
                {
                    Host = objAttri.Value;
                    if (s_localHost.Equals(Host, StringComparison.OrdinalIgnoreCase))
                    {
                        m_IsLocalHost = true;
                    }
                    else
                    {
                        m_IsLocalHost = false;
                    }
                }
                //Load site of all screens
                objAttri = objNode.Attributes[UIServiceCfgDefines.s_SiteAttri];
                if (null != objAttri)
                {
                    SiteOfScreens = objAttri.Value;
                    LogProcessorService.Log.UIService.LogDebugFormat("The site of all screens is {0}", SiteOfScreens);
                }

                //Load base path of all screens
                objAttri = objNode.Attributes[UIServiceCfgDefines.s_basePath4XamlAttri];
                if (null != objAttri)
                {
                    if (m_IsLocalHost)
                    {
                        basePath4Xaml = AppDomain.CurrentDomain.BaseDirectory + objAttri.Value;
                    }
                    else
                    {
                        basePath4Xaml = UrlPrefix + Host + objAttri.Value;
                    }

                    LogProcessorService.Log.UIService.LogDebugFormat("The path for xaml of all screens is {0}", basePath4Xaml);
                }

                //load base path (html) 
                objAttri = objNode.Attributes[UIServiceCfgDefines.s_basePath4HtmlAttri];
                if (null != objAttri)
                {
                    if (m_IsLocalHost)
                    {
                        basePath4Html = UrlPrefix + AppDomain.CurrentDomain.BaseDirectory + objAttri.Value;
                    }
                    else
                    {
                        basePath4Html = UrlPrefix + Host + objAttri.Value;
                    }
                    //                   basePath4Html = s_fileProto + AppDomain.CurrentDomain.BaseDirectory + objAttri.Value;
                    basePath4Html = basePath4Html.Replace(@"\", @"/");
                }
            }
            else
            {
                throw new Exception("The <screens> node must be exist");
            }

            LogProcessorService.Log.UIService.LogInfo("Success to load the <screens> node");
        }

        private void LoadAndBindAssemblies(XmlDocument objDoc)
        {
            Debug.Assert(null != objDoc);

            string strAssemblyPath = string.Format("/{0}/{1}/{2}", UIServiceCfgDefines.s_WndNode, UIServiceCfgDefines.s_BindingAssemblyNode, UIServiceCfgDefines.s_AssemblyNode);
            XmlNodeList listNodes = m_objDoc.SelectNodes(strAssemblyPath);
            XmlAttribute objAttri = null;
            foreach (XmlNode objNode in listNodes)
            {
                try
                {
                    objAttri = objNode.Attributes[UIServiceCfgDefines.s_nameAttri];
                    if (null != objAttri)
                    {
                        //Load assemblies
                        LogProcessorService.Log.UIService.LogDebugFormat("Load assembly with name [{0}]", objAttri.Value);
                        Assembly objAssembly = Assembly.Load(objAttri.Value);
                    }
                }
                catch (System.Exception ex)
                {
                    LogProcessorService.Log.UIService.LogWarn("Failed to assembly", ex);
                }
            }

            LogProcessorService.Log.UIService.LogInfo("Success to load and then bind assemblies");
        }

        private void LoadTemplates(XmlNode argNode,
                                    Screen argScr)
        {
            Debug.Assert(null != argNode && null != argScr);
            XmlNodeList templateList = argNode.SelectNodes(UIServiceCfgDefines.s_templateNode);
            ResourceDataTemplate item = null;
            foreach (XmlNode node in templateList)
            {
                item = LoadTemplate(node);
                if (null != item)
                {
                    argScr.Templates.Add(item);
                }
            }
        }

        private ResourceDataTemplate LoadTemplate(XmlNode argNode)
        {
            Debug.Assert(null != argNode);

            XmlAttribute nameAttri = argNode.Attributes[UIServiceCfgDefines.s_nameAttri];
            if (null == nameAttri ||
                 string.IsNullOrEmpty(nameAttri.Value))
            {
                return null;
            }

            XmlAttribute classAttri = null;
            XmlAttribute styleAttri = null;
            //XmlAttribute bindAttri = null;
            XmlNodeList listTd = argNode.SelectNodes("*");
            ResourceDataTemplate template = new ResourceDataTemplate(nameAttri.Value);
            //string bindElement = null;
            //string bindProp = null;
            //BindingMode mode = BindingMode.OneWay;
            foreach (XmlNode node in listTd)
            {
                if (node.NodeType != XmlNodeType.Element)
                {
                    continue;
                }
                if (!string.Equals(node.Name, "td", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (!node.HasChildNodes)
                {
                    continue;
                }

                classAttri = node.Attributes["class"];
                styleAttri = node.Attributes["style"];
                //bindAttri = node.Attributes["content"];
                //if ( null == bindAttri ||
                //     string.IsNullOrEmpty(bindAttri.Value) )
                //{
                //    continue;
                //}

                //bindValue = HtmlScreenElementBase.ParseBindValue(bindAttri.Value, ref bindElement, ref bindProp, ref mode);
                //if (string.IsNullOrEmpty(bindValue))
                //{
                //    continue;
                //}

                template.TDItems.Add(new DataTemplateTDItem()
                    {
                        ClassName = null != classAttri ? classAttri.Value : null,
                        StyleValue = null != styleAttri ? styleAttri.Value : null,
                        InnerHtml = node.InnerXml
                    });
            }

            return template;
        }

        private void LoadTriggers(XmlNode objNode,
                                   Screen objScr)
        {
            Debug.Assert(null != objNode && null != objScr);

            //      LogProcessorService.Log.UIService.LogDebug("Prepare for loading triggers of a screen");
            //            string strPath = string.Format("{0}/*", s_TriggersNode);
            //            XmlNodeList listNodes = objNode.SelectNodes(strPath);
            foreach (XmlNode obj in objNode.ChildNodes)
            {
                if (obj.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                //LoadEventTrigger( obj, objScr );
                switch (obj.Name)
                {
                    case UIServiceCfgDefines.s_EventTriggerNode:
                        {
                            LoadEventTrigger(obj, objScr);
                        }
                        break;

                    case UIServiceCfgDefines.s_PropertyTriggerNode:
                        {
                            LoadPropertyTriggers(obj, objScr);
                        }
                        break;

                    default:
                        {

                        }
                        break;
                }
            }

            //     LogProcessorService.Log.UIService.LogDebug("Success to load triggers of a screen");
        }

        private void LoadStoryboards(XmlNode argNode,
                                      Screen argScreen)
        {
            Debug.Assert(null != argNode && null != argScreen);

            // Log.UIService.LogDebug("Prepare for loading storyboards of a screen");

            GrgStoryboard storyboard = null;
            XmlNodeList listNodes = argNode.SelectNodes(UIServiceCfgDefines.s_storyboardNode);
            foreach (XmlNode node in listNodes)
            {
                storyboard = new GrgStoryboard(argScreen);
                if (storyboard.Open(node))
                {
                    argScreen.Storyboards.Add(storyboard);
                }
                storyboard = null;
            }
        }

        private void LoadEventTrigger(XmlNode objNode,
                                       Screen objScr)
        {
            Debug.Assert(null != objNode && null != objScr);
            if (!objNode.HasChildNodes)
            {
                return;
            }

            XmlAttribute conditionAttri = objNode.Attributes[UIServiceCfgDefines.s_ConditionAttri];
            if (null == conditionAttri ||
                 string.IsNullOrEmpty(conditionAttri.Value))
            {
                return;
            }

            try
            {
                EventTrigger trigger = new EventTrigger(objScr, conditionAttri.Value);
                if ( !trigger.Initialize( objNode ) )
                {
                    trigger = null;
                    return;
                }
                //load all actions
                LoadAllActions(trigger, objNode);

                //XmlAttribute bubbleEventAttri = objNode.Attributes[UIServiceCfgDefines.s_bubbleEventAttri];
                //if (null != bubbleEventAttri &&
                //     !string.IsNullOrEmpty(bubbleEventAttri.Value))
                //{
                //    int temp = 0;
                //    if (int.TryParse(bubbleEventAttri.Value, out temp))
                //    {
                //        trigger.BubbleEvent = temp == 0 ? false : true;
                //    }
                //}

                if (!trigger.Empty)
                {
                    objScr.AddTrigger(trigger);
                }
                else
                {
                    trigger = null;
                }
            }
            catch (System.Exception ex)
            {
                Log.UIService.LogError("Failed to load triggers", ex);
            }
        }

        private void LoadPropertyTriggers(XmlNode objNode,
                                           Screen objScr)
        {
            Debug.Assert(null != objNode && null != objScr);
            if (!objNode.HasChildNodes)
            {
                return;
            }

            XmlAttribute targetAttri = objNode.Attributes[UIServiceCfgDefines.s_TargetAttri];
            XmlAttribute propertyAttri = objNode.Attributes[UIServiceCfgDefines.s_PropertyAttri];
            XmlAttribute valueAttri = objNode.Attributes[UIServiceCfgDefines.s_ValueAttri];
            if (null == targetAttri ||
                 null == propertyAttri ||
                 null == valueAttri ||
                 string.IsNullOrEmpty(targetAttri.Value) ||
                 string.IsNullOrEmpty(propertyAttri.Value) ||
                 string.IsNullOrEmpty(valueAttri.Value))
            {
                return;
            }

            PropertyTrigger propTrigger = new PropertyTrigger(objScr,
                                                               targetAttri.Value,
                                                               propertyAttri.Value,
                                                               valueAttri.Value);
            if ( !propTrigger.Initialize( objNode ) )
            {
                propTrigger = null;
                return;
            }
            LoadAllActions(propTrigger, objNode);

            //XmlAttribute bubbleEventAttri = objNode.Attributes[UIServiceCfgDefines.s_bubbleEventAttri];
            //if (null != bubbleEventAttri &&
            //     !string.IsNullOrEmpty(bubbleEventAttri.Value))
            //{
            //    int temp = 0;
            //    if (int.TryParse(bubbleEventAttri.Value, out temp))
            //    {
            //        propTrigger.BubbleEvent = temp == 0 ? false : true;
            //    }
            //}

            if (!propTrigger.Empty)
            {
                objScr.AddTrigger(propTrigger);
            }
            else
            {
                propTrigger = null;
            }
        }

        private ActionBase LoadSetterAction(UITriggerBase argTrigger,
                                       XmlNode argNode)
        {
            Debug.Assert(null != argTrigger && null != argNode);
            XmlAttribute targetAttri = argNode.Attributes[UIServiceCfgDefines.s_TargetAttri];
            XmlAttribute propertyAttri = argNode.Attributes[UIServiceCfgDefines.s_PropertyAttri];
            XmlAttribute valueAttri = argNode.Attributes[UIServiceCfgDefines.s_ValueAttri];
            if (null == targetAttri ||
                 null == propertyAttri ||
                 null == valueAttri)
            {
                return null;
            }
            if (string.IsNullOrEmpty(targetAttri.Value) ||
                 string.IsNullOrEmpty(propertyAttri.Value) ||
                 string.IsNullOrEmpty(valueAttri.Value))
            {
                return null;
            }

            SetterAction action = new SetterAction(argTrigger, targetAttri.Value, propertyAttri.Value, valueAttri.Value)
            {
                ResourceManagerInterface = this.ResouceManager
            };

            return action;
        }

        protected void LoadAllActions(UITriggerBase argTrigger,
                                       XmlNode argNode)
        {
            Debug.Assert(null != argTrigger && null != argNode);

            ActionBase action = null;
            foreach (XmlNode node in argNode.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                switch (node.Name)
                {
                    case UIServiceCfgDefines.s_SetterActNode:
                        {
                            action = LoadSetterAction(argTrigger, node);
                        }
                        break;

                    case UIServiceCfgDefines.s_BeginStoryboardActNode:
                        {
                            action = LoadBeginStoryboardAction(argTrigger, node);
                        }
                        break;

                    case UIServiceCfgDefines.s_removeStoryboardActNode:
                        {
                            action = LoadRemoveStoryboardAction(argTrigger, node);
                        }
                        break;

                    case UIServiceCfgDefines.s_playSoundNode:
                        {
                            action = LoadPlaySoundAction(argTrigger, node);
                        }
                        break;

                    default:
                        {

                        }
                        break;
                }

                if (null != action)
                {
                    argTrigger.AddAction(action);
                }
            }
        }

        private ActionBase LoadBeginStoryboardAction(UITriggerBase argTrigger,
                                                XmlNode argNode)
        {
            Debug.Assert(null != argTrigger && null != argNode);

            //load name attribute
            XmlAttribute nameAttri = argNode.Attributes[UIServiceCfgDefines.s_nameAttri];
            if (null == nameAttri ||
                 string.IsNullOrEmpty(nameAttri.Value))
            {
                return null;
            }

            BeginStoryboardAction beginAction = new BeginStoryboardAction(argTrigger, nameAttri.Value);

            return beginAction;
        }

        private ActionBase LoadRemoveStoryboardAction(UITriggerBase argTrigger,
                                                 XmlNode argNode)
        {
            Debug.Assert(null != argTrigger && null != argNode);

            //load name attribute
            XmlAttribute nameAttri = argNode.Attributes[UIServiceCfgDefines.s_nameAttri];
            if (null != nameAttri ||
                 string.IsNullOrEmpty(nameAttri.Value))
            {
                return null;
            }

            return null;
        }

        private ActionBase LoadPlaySoundAction(UITriggerBase argTrigger,
                                          XmlNode argNode)
        {
            Debug.Assert(null != argTrigger && null != argNode);
            XmlAttribute sourceAttri = argNode.Attributes[UIServiceCfgDefines.s_sourceAttri];
            if (null == sourceAttri ||
                 string.IsNullOrEmpty(sourceAttri.Value))
            {
                return null;
            }

            //string soundfile = AppDomain.CurrentDomain.BaseDirectory + sourceAttri.Value;
            //if ( !File.Exists(soundfile) )
            //{
            //    return null;
            //}
            XmlAttribute loopAttri = argNode.Attributes[UIServiceCfgDefines.s_loopAttri];
            int loop = 0;
            if (null != loopAttri)
            {
                if (!int.TryParse(loopAttri.Value, out loop))
                {
                    loop = 0;
                }
            }

            XmlAttribute skipEnableAttri = argNode.Attributes[UIServiceCfgDefines.s_skipEnableAttri];
            int skipEnable = 0;
            if (null != skipEnableAttri)
            {
                if (!int.TryParse(skipEnableAttri.Value, out skipEnable))
                {
                    skipEnable = 0;
                }
            }

            return new PlaySoundAction(argTrigger, sourceAttri.Value)
                {
                    Loop = loop == 0 ? false : true,
                    ResourceManagerInterface = this.ResouceManager,
                    SkipEnable = skipEnable == 0 ? false : true
                };
        }

        public void EnumScreens(EnumScreenNameHandler objHandler,
                                 object objParam)
        {
            Debug.Assert(null != objHandler);

            string strPath = string.Format("/{0}/{1}/{2}", UIServiceCfgDefines.s_WndNode, UIServiceCfgDefines.s_ScreensNode, UIServiceCfgDefines.s_ScreenNode);
            XmlNodeList listNode = m_objDoc.SelectNodes(strPath);
            XmlAttribute objAttri = null;
            bool bContinue = false;
            foreach (XmlNode objNode in listNode)
            {
                objAttri = objNode.Attributes[UIServiceCfgDefines.s_nameAttri];
                if (null != objAttri)
                {
                    bContinue = objHandler.Invoke(objAttri.Value, objParam);
                    if (!bContinue)
                    {
                        break;
                    }
                }
            }
        }

        public bool IsHtml(string filePath)
        {
            Debug.Assert(!string.IsNullOrEmpty(filePath));
            string extOfFile = Path.GetExtension(filePath);
            bool Result = false;

            //if ( 0 == string.Compare( extOfFile, s_htmlExtension, true ) ||
            //     0 == string.Compare( extOfFile, s_htmExtension, true ) )
            if (extOfFile.Equals(s_htmExtension, StringComparison.OrdinalIgnoreCase) ||
                 extOfFile.Equals(s_htmlExtension, StringComparison.OrdinalIgnoreCase))
            {
                Result = true;
            }

            return Result;
        }

        public bool IsXaml(string filePath)
        {
            Debug.Assert(!string.IsNullOrEmpty(filePath));
            string extOfFile = Path.GetExtension(filePath);
            bool Result = false;

            //if (0 == string.Compare(extOfFile, s_xamlExtension, true) )
            if (extOfFile.Equals(s_xamlExtension, StringComparison.OrdinalIgnoreCase))
            {
                Result = true;
            }

            return Result;
        }
        #endregion

        #region property
        public string NameOfWindow
        {
            get;
            set;
        }

        public string SiteOfScreens
        {
            get;
            set;
        }

        public string UrlOfMotherboard
        {
            get
            {
                if (m_IsLocalHost)
                {
                    return AppDomain.CurrentDomain.BaseDirectory + m_motherboard;
                }
                else
                {
                    return UrlPrefix + Host + m_motherboard;
                }
            }
        }

        public string InitPage
        {
            get;
            set;
        }

        public string basePath4Xaml
        {
            get;
            set;
        }

        public string basePath4Html
        {
            get;
            set;
        }

        public WindowConfig WndConfig
        {
            get
            {
                return m_wndConfig;
            }
        }

        public string UrlPrefix
        {
            get;
            set;
        }

        public string Host
        {
            get;
            set;
        }

        public bool IsLocal
        {
            get
            {
                return m_IsLocalHost;
            }
        }

        public string CfgFilePath
        {
            get
            {
                return m_strCfg;
            }
        }

        public IResourceManager ResouceManager
        {
            get;
            set;
        }
        #endregion

        #region field
        private string m_strCfg;

        private WindowConfig m_wndConfig = new WindowConfig();

        private XmlDocument m_objDoc = null;

        private bool m_IsLocalHost = true;

        private string m_motherboard = null;

        public const string s_htmlExtension = ".html";

        public const string s_htmExtension = ".htm";

        public const string s_xamlExtension = ".xaml";

        public const string s_localHost = "localhost";

        public const string s_fileProto = @"file:///";
        #endregion
    }
}
