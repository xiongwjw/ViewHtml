/********************************************************************
	FileName:   SlideshowTimelineFrame
    purpose:	

	author:		huang wei
	created:	2013/03/19

    revised history:
	2013/03/19   

================================================================
    Copyright (C) 2013, Grgbanking CO,. Ltd. All rights reserved.
================================================================
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using LogProcessorService;
using System.Xml;
using System.IO;
using mshtml;
using UIServiceProtocol;

namespace UIServiceInWPF.GrgStoryboardNS.Timeline
{
    class FrameContent
    {
        private int m_frameTime;

        private string m_uri;

        private DateTime? m_startTime;

        private DateTime? m_endTime;

        public FrameContent(string argUri,
                             int argFrameTime)
        {
            m_frameTime = argFrameTime;
            m_uri = argUri;
        }

        public bool Init(string argStartText,
                          string argEndText)
        {
            bool hasStart = string.IsNullOrEmpty(argStartText);
            bool hasEnd = string.IsNullOrEmpty(argEndText);
            if (hasStart &&
                 hasEnd)
            {
                return true;
            }

            DateTime now = DateTime.Now;
            if (!hasStart &&
                 !hasEnd)
            {
                m_startTime = DateTime.Parse(argStartText);
                m_endTime = DateTime.Parse(argEndText);
                int result = m_startTime.Value.CompareTo(m_endTime.Value);
                if (result >= 0)
                {
                    return false;
                }

                if (now >= m_endTime.Value)
                {
                    return false;
                }
            }
            else
            {
                if (!hasStart)
                {
                    m_startTime = DateTime.Parse(argStartText);
                }
                if (!hasEnd)
                {
                    m_endTime = DateTime.Parse(argEndText);
                    if (now >= m_endTime.Value)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool IsValid(ref DateTime argNow)
        {
            if (!m_startTime.HasValue &&
                 !m_endTime.HasValue)
            {
                return true;
            }

            if (m_endTime.HasValue)
            {
                if (argNow >= m_endTime.Value)
                {
                    return false;
                }
            }

            return true;
        }

        public bool CanStart(ref DateTime argNow)
        {
            if (!m_startTime.HasValue)
            {
                return true;
            }

            if (argNow >= m_startTime.Value && argNow <= m_endTime.Value)
            {
                return true;
            }

            return false;
        }

        public XmlNode ToXmlNode(XmlDocument argDoc)
        {
            XmlNode frameNode = argDoc.CreateElement(UIServiceCfgDefines.s_frameNode);
            XmlAttribute attri = argDoc.CreateAttribute(UIServiceCfgDefines.s_keytimeAttri);
            attri.Value = m_frameTime.ToString();
            frameNode.Attributes.Append(attri);
            attri = argDoc.CreateAttribute(UIServiceCfgDefines.s_UriAttri);
            attri.Value = CfgUri;
            frameNode.Attributes.Append(attri);
            attri = argDoc.CreateAttribute(UIServiceCfgDefines.s_durationAttri);
            attri.Value = Duration.ToString();
            frameNode.Attributes.Append(attri);
            if (m_startTime.HasValue)
            {
                attri = argDoc.CreateAttribute(UIServiceCfgDefines.s_startTimeAttri);
                attri.Value = m_startTime.Value.ToString();
                frameNode.Attributes.Append(attri);
            }
            if (m_endTime.HasValue)
            {
                attri = argDoc.CreateAttribute(UIServiceCfgDefines.s_endTimeAttri);
                attri.Value = m_endTime.Value.ToString();
                frameNode.Attributes.Append(attri);
            }

            return frameNode;
        }

        public int FrameTime
        {
            get
            {
                return m_frameTime;
            }
            set
            {
                m_frameTime = value;
            }
        }

        public string Uri
        {
            get
            {
                return m_uri;
            }
        }

        public string CfgUri
        {
            get;
            set;
        }

        public bool CanStartPlay
        {
            get;
            set;
        }

        public int Duration
        {
            get;
            set;
        }

        public bool NeedLoadFromDatacache
        {
            get;
            set;
        }
    }

    class FramtContentCompare : IComparer<FrameContent>
    {
        public int Compare(FrameContent argFrame1,
                            FrameContent argFrame2)
        {
            if (argFrame1.FrameTime == argFrame2.FrameTime)
            {
                return 0;
            }
            else if (argFrame1.FrameTime > argFrame2.FrameTime)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
    }

    class SlideshowTimelineFrame : TimelineBase
    {
        #region constructor
        public SlideshowTimelineFrame(GrgStoryboard argBoard) : base(argBoard)
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
                //将可以播放的图片放入到临时的列表中 add by xjyong
                DateTime now = DateTime.Now;
                var listFrames = new List<FrameContent>();
                foreach (var item in m_listFrames)
                {
                    if (item.CanStart(ref now))
                    {
                        listFrames.Add(item);
                    }
                }
                //从新计算总的播放时间
                if (listFrames[listFrames.Count - 1].Duration > 0)
                {
                    m_duration = listFrames[listFrames.Count - 1].FrameTime + listFrames[listFrames.Count - 1].Duration;
                }
                else
                {
                    m_duration = listFrames[listFrames.Count - 1].FrameTime + SlideshowTimeline.s_defaultSlideInterval;
                }

                if (m_curFrameIndex == listFrames.Count &&
                    m_curPlayTime >= m_duration)
                {
                    if ((--m_curReperat) > 0)
                    {
                        m_curFrameIndex = -1;
                        m_curPlayTime = 0;
                        return false;
                    }
                    else
                    {
                        Stop();
                        return false;
                    }
                }
                else if (m_curFrameIndex < listFrames.Count)
                {
                    int end = Math.Max(0, m_curFrameIndex);
                    int index = 0;
                    bool finded = false;
                    for (index = listFrames.Count - 1; index >= end; --index)
                    {
                        //if (!listFrames[index].CanStartPlay)
                        //{
                        //    continue;
                        //}

                        if (listFrames[index].FrameTime <= m_curPlayTime)
                        {
                            finded = true;
                            break;
                        }
                    }

                    if (finded)
                    {
                        if (m_curFrameIndex != index)
                        {
                            m_curFrameIndex = index;
                            // m_iImgElement.src = m_listFrames[m_curFrameIndex].Uri;
                            if (listFrames[m_curFrameIndex].NeedLoadFromDatacache)
                            {
                                object value = HtmlRender.SingleInstance.GetBindedData(listFrames[m_curFrameIndex].Uri);
                                if (null != value)
                                {
                                    m_targetElement.SetAttribute("src", value.ToString());
                                }
                            }
                            else
                            {
                                m_targetElement.SetAttribute("src", listFrames[m_curFrameIndex].Uri);
                            }

                            if (m_notifyChange)
                            {
                                HtmlRender.SingleInstance.NotifyPropertyChanged(m_targetName, UIPropertyKey.s_ContentKey, m_listFrameFileNames[m_curFrameIndex]);
                            }
                        }
                        else
                        {
                            if (m_curFrameIndex == (listFrames.Count - 1))
                            {
                                m_curFrameIndex = listFrames.Count;
                            }
                        }
                    }
                }

                m_curPlayTime += GrgStoryboardManager.s_defaultInterval;
                listFrames = null;
            }
            catch (System.Exception ex)
            {
                Trace.Write(ex.Message);
                //Log.UIService.LogError("failed to set image source", ex);
            }

            return true;
        }

        public override void ReAdjustTime(ref int argBeginTime)
        {
            m_beginCfgTime = argBeginTime;
            int frameTime = 0;
            foreach (var item in m_listFrames)
            {
                if (item.CanStartPlay)
                {
                    item.FrameTime = frameTime;
                    frameTime += item.Duration;
                }
            }
            m_duration = frameTime;
            argBeginTime += m_duration;

            m_needAdjustTime = false;
            m_curFrameIndex = -1;
            m_curPlayTime = 0;
        }

        public override XmlNode ToXmlNode(XmlDocument argDoc)
        {
            Debug.Assert(null != argDoc);

            XmlNode slideShowNode = argDoc.CreateElement(UIServiceCfgDefines.s_slideshowTimelineFrameNode);
            XmlAttribute attri = argDoc.CreateAttribute(UIServiceCfgDefines.s_beginAttri);
            attri.Value = m_beginCfgTime.ToString();
            slideShowNode.Attributes.Append(attri);
            attri = argDoc.CreateAttribute(UIServiceCfgDefines.s_targetAttri);
            attri.Value = m_targetName;
            slideShowNode.Attributes.Append(attri);
            attri = argDoc.CreateAttribute(UIServiceCfgDefines.s_baseUriAttri);
            attri.Value = m_configBaseUri;
            slideShowNode.Attributes.Append(attri);
            attri = argDoc.CreateAttribute(UIServiceCfgDefines.s_durationAttri);
            attri.Value = m_duration.ToString();
            slideShowNode.Attributes.Append(attri);
            if (m_startTime.HasValue)
            {
                attri = argDoc.CreateAttribute(UIServiceCfgDefines.s_startTimeAttri);
                attri.Value = m_startTime.Value.ToString();
                slideShowNode.Attributes.Append(attri);
            }
            if (m_endTime.HasValue)
            {
                attri = argDoc.CreateAttribute(UIServiceCfgDefines.s_endTimeAttri);
                attri.Value = m_endTime.Value.ToString();
                slideShowNode.Attributes.Append(attri);
            }
            XmlNode frameNode = null;
            foreach (var item in m_listFrames)
            {
                frameNode = item.ToXmlNode(argDoc);
                if (null != frameNode)
                {
                    slideShowNode.AppendChild(frameNode);
                }
            }

            return slideShowNode;
        }

        public override bool Open(XmlNode argNode)
        {
            Debug.Assert(null != argNode);
            Log.UIService.LogDebug("Prepare for opening a slideshowTimelineFrame");

            XmlNodeList listFrames = argNode.SelectNodes(UIServiceCfgDefines.s_frameNode);
            if (null == listFrames)
            {
                Log.UIService.LogWarn("The SlideShowTimelineFrame object doesn't contain any Frame node");
                return false;
            }

            bool result = base.Open(argNode);
            if (false == result)
            {
                Log.UIService.LogDebug("Failed to open the base object ");
                return false;
            }

            m_baseUri = AppDomain.CurrentDomain.BaseDirectory + m_baseUri;
            if (string.IsNullOrEmpty(m_baseUri) ||
                !Directory.Exists(m_baseUri))
            {
                Log.UIService.LogWarn("The baseUri attribute isn't exists");
                return false;
            }
            if (m_baseUri[m_baseUri.Length - 1] != '\\')
            {
                m_baseUri += @"\";
            }

            XmlAttribute keytimeAttri = null;
            XmlAttribute uriAttri = null;
            XmlAttribute startAttri = null;
            XmlAttribute endAttri = null;
            XmlAttribute durationAttri = null;
            int keytime = 0;
            string uri = null;
            FrameContent frameContent = null;
            DateTime now = DateTime.Now;
            bool needLoadFromDatacache = false;
            foreach (XmlNode frameNode in listFrames)
            {
                keytimeAttri = frameNode.Attributes[UIServiceCfgDefines.s_keytimeAttri];
                uriAttri = frameNode.Attributes[UIServiceCfgDefines.s_UriAttri];
                startAttri = frameNode.Attributes[UIServiceCfgDefines.s_startTimeAttri];
                endAttri = frameNode.Attributes[UIServiceCfgDefines.s_endTimeAttri];
                durationAttri = frameNode.Attributes[UIServiceCfgDefines.s_durationAttri];
                if (null == keytimeAttri ||
                     string.IsNullOrEmpty(keytimeAttri.Value) ||
                     null == uriAttri ||
                     string.IsNullOrEmpty(uriAttri.Value))
                {
                    Log.UIService.LogWarn("The keytime attribute or the uri attribute must be exists");
                    continue;
                }
                //check keytime
                if (!int.TryParse(keytimeAttri.Value, out keytime))
                {
                    Log.UIService.LogWarn("The Frame node's key time is invalid");
                    continue;
                }
                if (keytime < 0)
                {
                    Log.UIService.LogWarn("The Frame node's key time is invalid");
                    continue;
                }
                if (IsDuplicateKeyTime(keytime))
                {
                    Log.UIService.LogWarn("The Frame node's key time is duplicate");
                    continue;
                }
                if (m_duration > 0 &&
                     m_duration < keytime)
                {
                    Log.UIService.LogWarn("The Frame node's key time is greater than the duration");
                    continue;
                }
                //check uri
                needLoadFromDatacache = false;
                //if ( uriAttri.Value.StartsWith("(") &&
                //     uriAttri.Value.EndsWith(")"))
                //{
                //    uri = uriAttri.Value.Trim('(',')');

                //}
                if (uriAttri.Value.StartsWith("{") &&
                          uriAttri.Value.EndsWith("}"))
                {
                    needLoadFromDatacache = true;
                    uri = uriAttri.Value.Trim('{', '}');
                }
                else
                {
                    uri = m_baseUri + uriAttri.Value;
                    if (!File.Exists(uri))
                    {
                        Log.UIService.LogWarn("The Frame node's uri isn't exists");
                        continue;
                    }
                }

                frameContent = new FrameContent(uri,
                                                  keytime);
                if (!frameContent.Init(null == startAttri ? null : startAttri.Value,
                                        null == endAttri ? null : endAttri.Value))
                {
                    frameContent = null;
                    continue;
                }
                frameContent.NeedLoadFromDatacache = needLoadFromDatacache;

                if (null != durationAttri &&
                     !string.IsNullOrEmpty(durationAttri.Value))
                {
                    int temp = 0;
                    if (int.TryParse(durationAttri.Value, out temp) &&
                         temp > 0)
                    {
                        frameContent.Duration = temp;
                    }
                }
                frameContent.CanStartPlay = frameContent.CanStart(ref now);
                frameContent.CfgUri = uriAttri.Value;

                m_listFrames.Add(frameContent);
            }

            if (m_listFrames.Count == 0)
            {
                Log.UIService.LogError("The SlideShowTimelineFrame object hasn't any valid frame");
                return false;
            }

            m_listFrames.Sort(new FramtContentCompare());

            if (m_duration == 0)
            {
                m_duration = m_listFrames[m_listFrames.Count - 1].FrameTime + SlideshowTimeline.s_defaultSlideInterval;
            }

            if (listFrames.Count != m_listFrames.Count)
            {
                m_needAdjustTime = true;
            }

            if (m_notifyChange)
            {
                m_listFrameFileNames = new List<string>();
                foreach (var item in m_listFrames)
                {
                    if (item.NeedLoadFromDatacache)
                    {
                        m_listFrameFileNames.Add(item.Uri);
                    }
                    else
                    {
                        m_listFrameFileNames.Add(Path.GetFileName(item.Uri));
                    }
                }
            }

            return true;
        }

        public override void Close()
        {
            base.Close();
            m_listFrames.Clear();
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

            m_curFrameIndex = -1;
            m_curPlayTime = 0;

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
            }
            catch (System.Exception ex)
            {
                Log.UIService.LogError("Failed to stop a slideshowTimelineFrame", ex);
            }
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
        }

        public override void NotifyScreenOk(System.Windows.Forms.HtmlDocument argDoc)
        {
            //            m_iImgElement = null;
            m_targetElement = null;

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
                Log.UIService.LogWarn("The slideshowTimelineFrame's target element must be a image element", ex);
                m_targetElement = null;
            }
        }

        private bool IsDuplicateKeyTime(int argKeytime)
        {
            bool ret = false;
            foreach (var content in m_listFrames)
            {
                if (content.FrameTime == argKeytime)
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }

        protected override bool CheckIsValid()
        {
            if (!base.CheckIsValid())
            {
                m_needAdjustTime = true;
                return false;
            }

            DateTime now = DateTime.Now;
            int total = m_listFrames.Count;
            for (int i = 0; i < total;)
            {
                if (!m_listFrames[i].IsValid(ref now))
                {
                    m_listFrames.RemoveAt(i);
                    --total;
                    m_needAdjustTime = true;
                }
                else
                {
                    ++i;
                }
            }

            return m_listFrames.Count == 0 ? false : true;
        }

        public override void Cancel()
        {
            base.Cancel();

            m_targetElement = null;
        }
        #endregion

        #region property
        public bool NeedAdjustTime
        {
            get
            {
                return m_needAdjustTime;
            }
        }

        public override bool CanStart
        {
            get
            {
                bool canStart = false;
                bool itemCanStart = false;
                DateTime now = DateTime.Now;
                foreach (var item in m_listFrames)
                {
                    itemCanStart = item.CanStart(ref now);
                    if (itemCanStart != item.CanStartPlay)
                    {
                        m_needAdjustTime = true;
                    }
                    item.CanStartPlay = itemCanStart;
                    if (itemCanStart)
                    {
                        canStart = true;
                    }
                }

                return canStart;
            }
        }
        #endregion

        #region field
        private List<FrameContent> m_listFrames = new List<FrameContent>();

        private List<string> m_listFrameFileNames = null;

        //      protected IHTMLImgElement m_iImgElement = null;
        private bool m_needAdjustTime = false;

        private int m_curFrameIndex = -1;

        private int m_curPlayTime = 0;
        #endregion
    }
}
