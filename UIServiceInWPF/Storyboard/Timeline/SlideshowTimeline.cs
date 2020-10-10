using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml;
using LogProcessorService;
using System.IO;
using mshtml;
using UIServiceProtocol;

namespace UIServiceInWPF.GrgStoryboardNS.Timeline
{

    class SlideshowTimeline : TimelineBase
    {
        private string imageSources = AppDomain.CurrentDomain.BaseDirectory + @"Resource\Common\Image\";
        private string strLange = string.Empty;
        private string adsxmlFile = string.Empty;
        private string dir = string.Empty;
        private string newimageSources = string.Empty;
        private string src = string.Empty;
        private string description = string.Empty;
        protected enum showOrder : byte
        {
            Sequence = 1,
            Random
        };

        #region constructor
        public SlideshowTimeline(GrgStoryboard argBoard)
            : base(argBoard)
        {

        }
        #endregion

        #region method
        public override bool TimeTick()
        {
            if (!base.TimeTick())
            {
                return false;
            }

            if (null == m_targetElement)
            {
                return false;
            }
            //if ( null == m_iImgElement )
            //{
            //    return false;
            //}
            try
            {
                m_curTimeInterval -= GrgStoryboardManager.s_defaultInterval;
                if (m_curTimeInterval > 0)
                {
                    return false;
                }

                m_curTimeInterval = m_calcTimeInterval;
                if (m_emOrder == showOrder.Sequence)
                {
                    m_curIndex++;
                    if (m_curIndex >= m_listImages.Count)
                    {
                        if ((--m_curReperat) > 0)
                        {
                            m_curIndex = 0;
                        }
                        else
                        {
                            Stop();
                            return false;
                        }
                    }
                    
                    m_targetElement.SetAttribute("src", m_listImages[m_curIndex]);
                    if ( m_notifyChange )
                    {
                        Debug.Assert(null != m_listImageFileNames);
                        HtmlRender.SingleInstance.NotifyPropertyChanged(m_targetName, UIPropertyKey.s_ContentKey, m_listImageFileNames[m_curIndex]);
                    }
                    if (null != m_targetTextElement)
                    {
                        if (m_dic_ads.Keys.Contains(m_listImages[m_curIndex]))
                        {
                            m_targetTextElement.SetAttribute("innerText", m_dic_ads[m_listImages[m_curIndex]]);
                        }
                    }
                }
                else
                {
                    Random random = new Random();
                    int index = random.Next(0, m_listImages.Count - 1);
                    m_targetElement.SetAttribute("src", m_listImages[index]);
                    if (m_notifyChange)
                    {
                        Debug.Assert(null != m_listImageFileNames);
                        HtmlRender.SingleInstance.NotifyPropertyChanged(m_targetName, UIPropertyKey.s_ContentKey, m_listImageFileNames[index]);
                    }
                    if (null != m_targetTextElement)
                    {
                        if (m_dic_ads.Keys.Contains(m_listImages[index]))
                        {
                            m_targetTextElement.SetAttribute("innerText", m_dic_ads[m_listImages[index]]);
                        }
                    }
                    random = null;
                }
            }
            catch (System.Exception ex)
            {
                Trace.Write(ex.Message);
                //Log.UIService.LogError("Failed to set image source", ex);
            }

            //           m_iImgElement.src = m_listImages[m_curIndex];

            return true;
        }
        private bool LoadAdResource()
        { 
             strLange = m_storyBoard.OwnerScreen.ResouceManager.CurrentUILanguage;
         
             adsxmlFile = AppDomain.CurrentDomain.BaseDirectory + @"Resource\Common\Image\ads" + "_" + strLange + ".xml";
             dir = string.Empty;
              newimageSources = string.Concat(imageSources, strLange, @"\");
            if (!File.Exists(adsxmlFile))
            {
                Log.UIService.LogWarn("The ads.xml  isn't exists");
                return false;
            }
            else
            {
                m_dic_ads.Clear();
                m_listImages.Clear();
                //读取配置文件 
                XmlDocument doc = new XmlDocument();
                doc.Load(adsxmlFile);
                if (doc != null)
                {
                    XmlNode logonNode = doc.SelectSingleNode("/ads/ad[@key='" + Key + "']");
                    if (logonNode.Attributes["dir"] != null)
                    {
                        dir = logonNode.Attributes["dir"].Value;
                    }
                    if (!string.IsNullOrEmpty(dir))
                    {
                        newimageSources = string.Concat(newimageSources, dir, @"\");
                    }
                    if (null != logonNode)
                    {
                        XmlNodeList lodlis = logonNode.SelectNodes("content");
                        if (lodlis.Count > 0)
                        {
                             src = string.Empty;
                             description = string.Empty;
                            foreach (XmlNode node in lodlis)
                            {
                                src = string.Empty;
                                description = string.Empty;
                                if (null != node.Attributes["src"])
                                {
                                    src = string.Concat(newimageSources, node.Attributes["src"].Value.ToString());
                                    if (!m_dic_ads.Keys.Contains(src))
                                    {
                                        if (null != node.Attributes["description"])
                                        {
                                            description = node.Attributes["description"].Value.ToString();
                                        }
                                        m_dic_ads.Add(src, description);
                                        if (File.Exists(src))
                                        {
                                            m_listImages.Add(src);
                                        }

                                        }
                                    }
                                }
                                m_listImages.Sort();
                            }

                        }
                    }
                }

            return true;

        }
        public override bool Open(XmlNode argNode)
        {
            Debug.Assert(null != argNode);
            Log.UIService.LogDebug("Prepare for opening a SlideShowTimeline");
            bool result = base.Open(argNode);
            if (!result)
            {
                Log.UIService.LogError("Failed to invoke base's implement");
                return result;
            }
           
            m_baseUri = AppDomain.CurrentDomain.BaseDirectory + m_baseUri;
            if (m_baseUri[m_baseUri.Length - 1] != '\\')
            {
                m_baseUri += @"\";
            }

            if (string.IsNullOrEmpty(m_baseUri) ||
                 !Directory.Exists(m_baseUri))
            {
                Log.UIService.LogWarn("The baseUri attribute isn't exists");
                return false;
            }

            if (IsConfig)
            {
                bool isSuccess = LoadAdResource();
                if (!isSuccess)
                {
                    return false;
                }
            }
            else
            {
                //load ignore attribute
                //处理香港中银首页动画切换时，忽略掉某些图片
                XmlAttribute ignoreAttri = argNode.Attributes[UIServiceCfgDefines.s_ignoreAttri];
                if (null != ignoreAttri &&
                     !string.IsNullOrEmpty(ignoreAttri.Value))
                {
                    string[] arrTemps = ignoreAttri.Value.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (null != arrTemps &&
                         arrTemps.Length > 0)
                    {
                        m_listIgnoreFileNames = new List<string>();
                        foreach (var item in arrTemps)
                        {
                            m_listIgnoreFileNames.Add(item);
                        }
                    }
                }

                //load from attribute 
                XmlAttribute fromAttri = argNode.Attributes[UIServiceCfgDefines.s_FromAttri];
                XmlAttribute toAttri = argNode.Attributes[UIServiceCfgDefines.s_ToAttri];
                if (null == fromAttri ||
                     string.IsNullOrEmpty(fromAttri.Value) ||
                     null == toAttri ||
                     string.IsNullOrEmpty(toAttri.Value))
                {
                    string[] imgFiles = Directory.GetFiles(m_baseUri, "*", SearchOption.TopDirectoryOnly);
                    Array.Sort(imgFiles);
                    if ( null == m_listIgnoreFileNames )
                    {
                        foreach (var file in imgFiles)
                        {
                            if (IsImageFile(Path.GetExtension(file)))
                            {
                                m_listImages.Add(file);
                            }
                        }
                    }
                    else
                    {
                        foreach ( var file in imgFiles )
                        {
                            if ( IsImageFile(Path.GetExtension(file)) &&
                                 !m_listIgnoreFileNames.Exists( item => item.Equals(Path.GetFileName(file), StringComparison.OrdinalIgnoreCase) ) )
                            {
                                m_listImages.Add(file);
                            }
                        }

                        m_listIgnoreFileNames.Clear();
                        m_listIgnoreFileNames = null;
                    }
                }
                else
                {
                    string fromFilePath = m_baseUri + fromAttri.Value;
                    string toFilePath = m_baseUri + toAttri.Value;
                    if (!File.Exists(fromFilePath) &&
                         !File.Exists(toFilePath))
                    {
                        Log.UIService.LogWarn("Image files must exists");
                        return false;
                    }
                    string fromFileName = Path.GetFileNameWithoutExtension(fromFilePath);
                    string toFileName = Path.GetFileNameWithoutExtension(toFilePath);
                    string extendName = Path.GetExtension(fromFilePath);
                    int fromIndex = 0;
                    int toIndex = 0;
                    if (!int.TryParse(fromFileName, out fromIndex) ||
                        !int.TryParse(toFileName, out toIndex))
                    {
                        Log.UIService.LogWarn("Image file name is invalid");
                        return false;
                    }
                    if (toIndex < fromIndex)
                    {
                        int temp = fromIndex;
                        fromIndex = toIndex;
                        toIndex = fromIndex;
                    }
                    string imagePath = null;
                    if ( null == m_listIgnoreFileNames )
                    {
                        for (int i = fromIndex; i <= toIndex; ++i)
                        {
                            imagePath = string.Format("{0}{1}{2}", m_baseUri, i, extendName);
                            if (File.Exists(imagePath))
                            {
                                m_listImages.Add(imagePath);
                            }
                        }
                    }
                    else
                    {
                        for (int i = fromIndex; i <= toIndex; ++i)
                        {
                            imagePath = string.Format("{0}{1}{2}", m_baseUri, i, extendName);
                            if (File.Exists(imagePath) &&
                                !m_listIgnoreFileNames.Exists(item => item.Equals(Path.GetFileName(imagePath), StringComparison.OrdinalIgnoreCase)) )
                            {
                                m_listImages.Add(imagePath);
                            }
                        }
                        m_listIgnoreFileNames.Clear();
                        m_listIgnoreFileNames = null;
                    }
                }
            }
            if (m_listImages.Count == 0)
            {
                Log.UIService.LogError("All images aren't exist");
                return false;
            }

            if (0 == m_duration)
            {
                m_calcTimeInterval = s_defaultSlideInterval;
            }
            else
            {
                m_calcTimeInterval = m_duration / m_listImages.Count;
                if (m_calcTimeInterval < GrgStoryboardManager.s_defaultInterval)
                {
                    m_calcTimeInterval = GrgStoryboardManager.s_defaultInterval;
                }
            }

            //load interval attribute
            XmlAttribute intervalAttri = argNode.Attributes[UIServiceCfgDefines.s_intervalAttri];
            if (null != intervalAttri &&
                 !string.IsNullOrEmpty(intervalAttri.Value))
            {
                int temp = 0;
                if (int.TryParse(intervalAttri.Value, out temp))
                {
                    if (temp > GrgStoryboardManager.s_defaultInterval)
                    {
                        m_calcTimeInterval = temp;
                    }
                }
            }

            //load order attribute
            XmlAttribute orderAttri = argNode.Attributes[UIServiceCfgDefines.s_orderAttri];
            if (null != orderAttri &&
                 !string.IsNullOrEmpty(orderAttri.Value))
            {
                int temp = 0;
                if (int.TryParse(orderAttri.Value, out temp))
                {
                    if (temp == 2)
                    {
                        m_emOrder = showOrder.Random;
                    }
                    else
                    {
                        m_emOrder = showOrder.Sequence;
                    }
                }
            }

            if ( m_notifyChange )
            {
                m_listImageFileNames = new List<string>();
                foreach ( var item in m_listImages )
                {
                    m_listImageFileNames.Add(Path.GetFileName(item));
                }
            }

            return result;
        }

        public override void Close()
        { 
            base.Close();
        }

        public override bool Start()
        {
            if (null == m_targetElement)
            {
                m_state = playState.Stop;
                return false;
            }

            if (!base.Start())
            {
                return false;
            }

            if (m_emOrder == showOrder.Random)
            {
                Random rand = new Random();
                m_curIndex = rand.Next(0, m_listImages.Count - 1);
                rand = null;
            }
            else
            {
                m_curIndex = 0;
            }

            m_curTimeInterval = 0;

            return true;
        }

        public override void Stop()
        {
            base.Stop();

            try
            {
                if (null != m_targetElement)
                {
                    IHTMLElement element = (IHTMLElement)m_targetElement.DomElement;
                    element.style.display = "none";
                    element = null;
                }

                if (null != m_targetTextElement)
                {

                    IHTMLElement element = (IHTMLElement)m_targetTextElement.DomElement;
                    element.style.display = "none";
                    element = null;
                }

            }
            catch (System.Exception ex)
            {
                Log.UIService.LogError("Failed to stop a slideshowtimeline", ex);
            }
        }

        public override void Cancel()
        {
            base.Cancel();

            m_targetElement = null;
        }

        protected override void OnTimelineStart()
        {
            base.OnTimelineStart();

            if (null != m_targetElement)
            {
                IHTMLElement element = (IHTMLElement)m_targetElement.DomElement;
                element.style.display = string.Empty;
                element = null;
            }
            if (null != m_targetTextElement)
            {
                IHTMLElement element = (IHTMLElement)m_targetTextElement.DomElement;
                element.style.display = string.Empty;
                element = null;
            }
        }

        public override void NotifyScreenOk(System.Windows.Forms.HtmlDocument argDoc)
        {
            //          m_iImgElement = null;
            m_targetElement = null;
            m_targetTextElement = null;
            base.NotifyScreenOk(argDoc);
            if (null == m_targetElement)
            {
                return;
            }

            try
            {
                //m_iImgElement = (IHTMLImgElement)m_targetElement.DomElement;
                //if (null == m_iImgElement)
                //{
                //    m_targetElement = null;
                //}
            }
            catch (System.Exception ex)
            {
                Log.UIService.LogWarn("The slideshowTimeline's target element must be a image element", ex);
                m_targetElement = null;
            }
        }

        public override void ResouceManager_languageChanged(string argOld, string argNew, bool argIsUI)
        {
            if (IsConfig)
            {
                Log.UIService.LogDebug("The slideshowTimeline load again by language");
                LoadAdResource();
            }
        }

        #endregion

        #region field
        public const int s_defaultSlideInterval = 5000;

        protected showOrder m_emOrder = showOrder.Sequence;

        //        protected IHTMLImgElement m_iImgElement = null;

        protected List<string> m_listImages = new List<string>();

        protected List<string> m_listImageFileNames = null;

        protected List<string> m_listIgnoreFileNames = null;

        protected int m_curIndex = 0;

        protected Dictionary<string, string> m_dic_ads = new Dictionary<string, string>();

        protected int m_curTimeInterval = 0;

        protected int m_calcTimeInterval = 0;
        #endregion
    }
}
