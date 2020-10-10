using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIServiceInWPF.screen;
using UIServiceInWPF.HtmlScreenElement;
using System.Diagnostics;
using LogProcessorService;
using System.Xml;
using UIServiceInWPF.GrgStoryboardNS.Timeline;
using mshtml;
using System.Threading;

namespace UIServiceInWPF.GrgStoryboardNS
{
    public enum playState
    {
        Play,
        Stop,
        Pause
    };

    class TimelineComparer : IComparer<TimelineBase>
    {
        public int Compare(TimelineBase arg1,
                            TimelineBase arg2)
        {
            if (arg1.Begin == arg2.Begin)
            {
                return 0;
            }
            else if (arg1.Begin < arg2.Begin)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }
    }

    public class GrgStoryboard
    {
        delegate TimelineBase TimelineCreateHandle(GrgStoryboard argStoryboard,
                                                   XmlNode argNode);
        #region constructor
        public GrgStoryboard(Screen argOwnerScreen)
        {
            Debug.Assert(null != argOwnerScreen);
            m_ownerScreen = argOwnerScreen;

        }

        static GrgStoryboard()
        {
            s_dicCreateHandle = new Dictionary<string, TimelineCreateHandle>();

            s_dicCreateHandle.Add(UIServiceCfgDefines.s_mediaTimelineNode, CreateMediaTimeline);
            s_dicCreateHandle.Add(UIServiceCfgDefines.s_slideshowTimelineNode, CreateSlideShowTimeline);
            s_dicCreateHandle.Add(UIServiceCfgDefines.s_slideshowTimelineFrameNode, CreateSlideShowTimelineFrame);
        }
        #endregion

        #region method
        public virtual bool Open(XmlNode argNode)
        {
            Debug.Assert(null != argNode);
            Log.UIService.LogDebug("Prepare for opening a storyboard");

            if (!argNode.HasChildNodes)
            {
                Log.UIService.LogDebug("A storyboard has at least a Timeline node");
                return false;
            }

            if (m_isOpen)
            {
                Log.UIService.LogDebug("The storyboard has been opened");
                return true;
            }

            try
            {
                //load name attribute
                XmlAttribute nameAttri = argNode.Attributes[UIServiceCfgDefines.s_nameAttri];
                if (null == nameAttri ||
                     string.IsNullOrEmpty(nameAttri.Value))
                {
                    throw new Exception("Name must exist!");
                }
                else
                {
                    m_name = nameAttri.Value;
                }

                //load target attribute
                XmlAttribute targetAttri = argNode.Attributes[UIServiceCfgDefines.s_targetAttri];
                if (null != targetAttri &&
                     !string.IsNullOrEmpty(targetAttri.Value))
                {
                    m_targetName = targetAttri.Value;
                }

                //load repeat attribute
                XmlAttribute repeatAttri = argNode.Attributes[UIServiceCfgDefines.s_reperatBehaveAttri];
                if (null != repeatAttri &&
                     !string.IsNullOrEmpty(repeatAttri.Value))
                {
                    m_repeatCount = ConvertReperatValue(repeatAttri.Value);
                }

                //load needAdjustTime attribute
                XmlAttribute needAdjustTimeAttri = argNode.Attributes[UIServiceCfgDefines.s_needAdjustTimeAttri];
                if (null != needAdjustTimeAttri &&
                     !string.IsNullOrEmpty(needAdjustTimeAttri.Value))
                {
                    int temp = 0;
                    if (int.TryParse(needAdjustTimeAttri.Value, out temp) &&
                         temp >= 0)
                    {
                        m_needAdjustTime = temp == 0 ? false : true;
                    }
                }

                var updateXml = argNode.Attributes[UIServiceCfgDefines.s_updateXml];
                if (updateXml!= null && !string.IsNullOrWhiteSpace(updateXml.Value) && updateXml.Value != "0")
                {
                    m_updateXml = true;
                }

                //scan all Timeline nodes
                XmlNodeList listNodes = argNode.SelectNodes("*");
                TimelineBase timeline = null;
                bool needAdjustTime = false;
                foreach (XmlNode node in listNodes)
                {
                    if (node.NodeType != XmlNodeType.Element)
                    {
                        continue;
                    }

                    if (s_dicCreateHandle.ContainsKey(node.Name))
                    {
                        timeline = s_dicCreateHandle[node.Name].Invoke(this, node);
                        if (timeline is SlideshowTimeline)
                        {
                            m_ownerScreen.ResouceManager.languageChanged += new ResourceManagerProtocol.LanguageChangedHandler(timeline.ResouceManager_languageChanged);
                        }
                        if (null != timeline &&
                             timeline.IsValid)
                        {
                            timeline.TimelineEnd += TimelineEnd;
                            timeline.TimelineStart += TimelineStart;
                            timeline.CanStartPlay = timeline.CanStart;
                            m_listTimelines.Add(timeline);
                        }
                        else
                        {
                            needAdjustTime = true;
                        }
                    }
                }

                if (m_listTimelines.Count == 0)
                {
                    throw new Exception("A storyboard must have a Timeline");
                }
                else
                {
                    m_listTimelines.Sort(new TimelineComparer());
                }

                if (m_needAdjustTime &&
                    needAdjustTime)
                {
                    AdjustTimeOfTimelines();
                }
            }
            catch (System.Exception ex)
            {
                Log.UIService.LogError("Failed to open a storyboard", ex);
                return false;
            }

            m_state = playState.Stop;
            m_isOpen = true;

            return true;
        }

        public virtual void Close()
        {
            //Log.UIService.LogDebug("Prepare for closing a storyboard");
            if (!m_isOpen)
            {
                //Log.UIService.LogDebug("The storyboard isn't open");
                return;
            }

            m_isOpen = false;
            foreach (var timeline in m_listTimelines)
            {
                timeline.TimelineEnd -= TimelineEnd;
                timeline.TimelineStart -= TimelineStart;
                if (timeline is SlideshowTimeline)
                {
                    m_ownerScreen.ResouceManager.languageChanged -= timeline.ResouceManager_languageChanged;
                }
                timeline.Close();
            }

            m_listTimelines.Clear();
            m_targetElement = null;
            m_targetName = null;
            m_repeatCount = 0;
            m_name = null;
        }

        public void Start()
        {
            CheckValidOfAllTimelines();

            m_state = playState.Play;
            foreach (var timeline in m_listTimelines)
            {
                timeline.Start();
            }
            m_curReperat = m_repeatCount;

            GrgStoryboardManager.Instance.ScheduleStoryboard(this);
            OnStoryboardPlayStart();
        }

        public void Stop(bool argStopTimeline = true)
        {
            if (m_state == playState.Stop)
            {
                return;
            }

            m_state = playState.Stop;
            if (argStopTimeline)
            {
                GrgStoryboardManager.Instance.CancelSchedule(this);
                foreach (var timeline in m_listTimelines)
                {
                    timeline.Stop();
                }
            }

            OnStoryboardPlayEnd();

            CheckValidOfAllTimelines();
        }

        private void CheckValidOfAllTimelines(bool argNeedRemove = true)
        {
            if (!m_needAdjustTime)
            {
                return;
            }

            bool NeedAdjustTime = false;
            int total = m_listTimelines.Count;
            for (int i = 0; i < total;)
            {
                if (m_listTimelines[i].NeedRemove)
                {
                    ++i;
                    continue;
                }

                if (!m_listTimelines[i].IsValid)
                {
                    m_listTimelines[i].Close();
                    if (argNeedRemove)
                    {
                        m_listTimelines.RemoveAt(i);
                        --total;
                    }
                    else
                    {
                        m_listTimelines[i].NeedRemove = true;
                        ++i;
                    }
                    NeedAdjustTime = true;
                }
                else if (m_listTimelines[i] is SlideshowTimelineFrame)
                {
                    if (((SlideshowTimelineFrame)m_listTimelines[i]).NeedAdjustTime)
                    {
                        NeedAdjustTime = true;
                    }
                    ++i;
                }
                else
                {
                    ++i;
                }
            }

            bool canStart = false;
            foreach (var item in m_listTimelines)
            {
                if (item.NeedRemove)
                {
                    continue;
                }

                canStart = item.CanStart;
                if (item.CanStartPlay != canStart)
                {
                    NeedAdjustTime = true;
                }
                item.CanStartPlay = canStart;
            }

            if (NeedAdjustTime &&
                 m_needAdjustTime)
            {
                AdjustTimeOfTimelines();
            }
        }

        private void AdjustTimeOfTimelines()
        {
            try
            {
                XmlDocument doc = null;
                XmlNode storyboardNode = null;
                if (m_updateXml)
                {
                    doc = new XmlDocument();
                    doc.Load(m_ownerScreen.ConfigOfUI);
                    storyboardNode = doc.DocumentElement.SelectSingleNode(string.Format("Screens/Screen[@name='{0}']/Storyboards/Storyboard[@name='{1}']",
                                                                                  m_ownerScreen.Name,
                                                                                  m_name));
                    if (null == storyboardNode)
                    {
                        return;
                    }
                    while (null != storyboardNode.FirstChild)
                    {
                        storyboardNode.RemoveChild(storyboardNode.FirstChild);
                    }
                }

                int beginTime = 0;
                XmlNode timelineNode = null;
                foreach (var item in m_listTimelines)
                {
                    if (!(item is SlideshowTimeline) &&
                         !item.NeedRemove)
                    {
                        item.ReAdjustTime(ref beginTime);
                        if (m_updateXml)
                        {
                            timelineNode = item.ToXmlNode(doc);
                            if (null == timelineNode)
                            {
                                throw new Exception("Failed to generate XmlNode of a timeline");
                            }
                            storyboardNode.AppendChild(timelineNode);
                        }
                    }
                }

                if (m_updateXml)
                {
                    doc.Save(m_ownerScreen.ConfigOfUI);
                    doc.RemoveAll();
                    doc = null;
                }
            }
            catch (System.Exception ex)
            {
                Log.UIService.LogError("Failed to adjust time of timelines", ex);
            }
        }

        public virtual void TimeTick()
        {
            if (Monitor.TryEnter(m_timeTickLock))
            {
                lock (m_timeTickLock)
                {
                    if (m_state != playState.Play)
                    {
                        return;
                    }

                    foreach (var timeline in m_listTimelines)
                    {
                        if (timeline.NeedRemove)
                        {
                            continue;
                        }

                        CheckValidOfAllTimelines();
                        timeline.TimeTick();
                    }
                }
            }
        }

        private static TimelineBase CreateMediaTimeline(GrgStoryboard argBoard,
                                                         XmlNode argNode)
        {
            Debug.Assert(null != argNode && null != argBoard);

            MediaTimeline media = new MediaTimeline(argBoard);
            if (!media.Open(argNode))
            {
                media = null;
                return null;
            }

            return media;
        }

        private static TimelineBase CreateSlideShowTimeline(GrgStoryboard argBoard,
                                                             XmlNode argNode)
        {
            Debug.Assert(null != argNode && null != argBoard);

            SlideshowTimeline slideshow = new SlideshowTimeline(argBoard);
            if (!slideshow.Open(argNode))
            {
                slideshow = null;
                return null;
            }

            return slideshow;
        }

        private static TimelineBase CreateSlideShowTimelineFrame(GrgStoryboard argBoard,
                                                                  XmlNode argNode)
        {
            Debug.Assert(null != argBoard && null != argNode);

            SlideshowTimelineFrame slideshow = new SlideshowTimelineFrame(argBoard);
            if (!slideshow.Open(argNode))
            {
                slideshow = null;
                return null;
            }

            return slideshow;
        }

        private void TimelineStart(object sender, EventArgs e)
        {
            TimelineBase startTimeline = (TimelineBase)sender;
            Debug.Assert(null != startTimeline);
            if (null != startTimeline.Target)
            {
                IHTMLElement iElement = (IHTMLElement)startTimeline.Target.DomElement;
                IHTMLStyle iStyle = iElement.style;
                iStyle.display = string.Empty;

                iStyle = null;
                iElement = null;
            }
            //foreach ( var timeline in m_listTimelines )
            //{
            //    if ( !timeline.IsStop &&
            //         !string.IsNullOrEmpty(timeline.TargetName) &&
            //          timeline.TargetName.Equals( startTimeline.TargetName, StringComparison.OrdinalIgnoreCase ) )
            //    {
            //        timeline.Cancel();
            //        break;
            //    }
            //}
        }

        private void TimelineEnd(object sender, EventArgs e)
        {
            bool allStop = true;
            foreach (var timeline in m_listTimelines)
            {
                if (!timeline.IsStop)
                {
                    allStop = false;
                    break;
                }
            }

            if (allStop)
            {
                if (--m_curReperat < 0)
                {
                    Stop(false);
                }
                else
                {
                    CheckValidOfAllTimelines(false);
                    foreach (var timeline in m_listTimelines)
                    {
                        if (timeline.NeedRemove ||
                            !timeline.IsValid)
                        {
                            continue;
                        }

                        timeline.Start();
                    }
                }
            }
        }

        public static int ConvertReperatValue(string argReperat)
        {
            if (string.IsNullOrEmpty(argReperat))
            {
                return 0;
            }

            int Ret = 0;
            if (string.Equals(argReperat, UIServiceCfgDefines.s_foreverAttri, StringComparison.OrdinalIgnoreCase))
            {
                Ret = int.MaxValue;
            }
            else
            {
                int value = 0;
                if (int.TryParse(argReperat, out value))
                {
                    Ret = Math.Abs(value);
                }
            }

            return Ret;
        }

        public void NotifyScreenOk(System.Windows.Forms.HtmlDocument argDoc)
        {
            Debug.Assert(null != argDoc);

            if (!string.IsNullOrEmpty(m_targetName))
            {
                m_targetElement = argDoc.GetElementById(m_targetName);
            }

            foreach (var timeline in m_listTimelines)
            {
                timeline.NotifyScreenOk(argDoc);
            }
        }
        #endregion

        #region property
        public Screen OwnerScreen
        {
            get
            {
                return m_ownerScreen;
            }
        }

        public string Name
        {
            get
            {
                return m_name;
            }
        }

        public bool IsOpen
        {
            get
            {
                return m_isOpen;
            }
        }

        public playState State
        {
            get
            {
                return m_state;
            }
        }

        public System.Windows.Forms.HtmlElement TargetElement
        {
            get
            {
                return m_targetElement;
            }
        }

        public string TargetName
        {
            get
            {
                return m_targetName;
            }
        }
        #endregion

        #region event
        public event EventHandler StoryboardPlayEnd;

        public event EventHandler StoryboardPlayStart;

        protected void OnStoryboardPlayEnd()
        {
            if (null != StoryboardPlayEnd)
            {
                StoryboardPlayEnd.Invoke(this, EventArgs.Empty);
            }
        }

        protected void OnStoryboardPlayStart()
        {
            if (null != StoryboardPlayStart)
            {
                StoryboardPlayStart.Invoke(this, EventArgs.Empty);
            }
        }
        #endregion

        #region field
        protected Screen m_ownerScreen = null;

        protected string m_name = null;

        protected bool m_isOpen = false;

        protected int m_repeatCount = int.MaxValue;

        protected int m_curReperat = 0;

        protected string m_targetName = null;

        //       protected HtmlScreenElementBase m_targetElement = null;
        protected System.Windows.Forms.HtmlElement m_targetElement = null;

        protected playState m_state = playState.Stop;

        private bool m_needAdjustTime = false;

        private List<TimelineBase> m_listTimelines = new List<TimelineBase>();

        private static Dictionary<string, TimelineCreateHandle> s_dicCreateHandle = null;

        private bool m_updateXml = false;

        private static readonly object m_timeTickLock = new object();
        #endregion
    }
}
