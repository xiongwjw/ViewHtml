using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using LogProcessorService;
using System.Xml;
using UIServiceInWPF.HtmlScreenElement;
using System.Windows.Forms;

namespace UIServiceInWPF.GrgStoryboardNS.Timeline
{
    class TimelineBase
    {
#region constructor
        public TimelineBase( GrgStoryboard argStoryboard )
        {
            Debug.Assert(null != argStoryboard);
            m_storyBoard = argStoryboard;
            NeedRemove = false;
        }
#endregion

#region method
        public virtual bool TimeTick()
        {
            if ( m_state != playState.Play ||
                 !m_canStartPlay )
            {
                return false;
            }

            if ( m_curBeginTime != 0 )
            {
                if (m_curBeginTime > 0)
                {
                    m_curBeginTime -= GrgStoryboardManager.s_defaultInterval;
                }
                else if (m_curBeginTime < 0)
                {
                    m_curBeginTime = 0;
                }

                return false;
            }

            if ( !m_bFirstStart )
            {
                m_bFirstStart = true;
                OnTimelineStart();
            }

            return true;
        }

        public virtual bool Open( XmlNode argNode )
        {
            Debug.Assert(null != argNode);

            try
            {
                //load begin attribute
                XmlAttribute beginAttri = argNode.Attributes[UIServiceCfgDefines.s_beginAttri];
                if ( null != beginAttri &&
                     !string.IsNullOrEmpty(beginAttri.Value) )
                {
                    if ( !int.TryParse( beginAttri.Value, out m_beginCfgTime ) )
                    {
                        m_beginCfgTime = 0;
                    }
                }

                XmlAttribute isConfigAttri = argNode.Attributes[UIServiceCfgDefines.s_isConfig];
                if (null != isConfigAttri &&
                     !string.IsNullOrEmpty(isConfigAttri.Value))
                {
                    try
                    {
                        m_isConfig = Convert.ToBoolean(isConfigAttri.Value);
                    }
                    catch (Exception ex) {
                        m_isConfig = false;
                        Log.UIService.LogError("Failed to open a time line", ex);
                    }
                }

                //load duration attribute
                XmlAttribute durationAttri = argNode.Attributes[UIServiceCfgDefines.s_durationAttri];
                if ( null != durationAttri &&
                     !string.IsNullOrEmpty(durationAttri.Value) )
                {
                    if ( !int.TryParse( durationAttri.Value, out m_duration ) )
                    {
                        m_duration = 0;
                    }
                }
                
                //load target attribute
                XmlAttribute targetAttri = argNode.Attributes[UIServiceCfgDefines.s_targetAttri];
                if ( null != targetAttri &&
                     !string.IsNullOrEmpty(targetAttri.Value) )
                {
                    m_targetName = targetAttri.Value;
                }

                //load targetText attribute
                XmlAttribute targetTextAttri = argNode.Attributes[UIServiceCfgDefines.s_targetTextAttri];
                if (null != targetTextAttri &&
                     !string.IsNullOrEmpty(targetTextAttri.Value))
                {
                    m_targetTextName = targetTextAttri.Value;
                }

                XmlAttribute keyAttri = argNode.Attributes[UIServiceCfgDefines.s_key];
                if (null != keyAttri &&
                     !string.IsNullOrEmpty(keyAttri.Value))
                { 
                    m_key = keyAttri.Value;
                }

                //load repeat behavie attribute
                XmlAttribute reperatAttri = argNode.Attributes[UIServiceCfgDefines.s_reperatBehaveAttri];
                if (null != reperatAttri &&
                     !string.IsNullOrEmpty(reperatAttri.Value))
                {
                    m_reperatBehavior = GrgStoryboard.ConvertReperatValue(reperatAttri.Value);
                }

                //load baseuri attribute
                XmlAttribute baseUriAttri = argNode.Attributes[UIServiceCfgDefines.s_baseUriAttri];
                if ( null != baseUriAttri &&
                     !string.IsNullOrEmpty(baseUriAttri.Value) )
                {
                    m_baseUri = baseUriAttri.Value;
                    m_configBaseUri = m_baseUri;
                }

                //load start time attribute
                XmlAttribute startTimeAttri = argNode.Attributes[UIServiceCfgDefines.s_startTimeAttri];
                if ( null != startTimeAttri &&
                     !string.IsNullOrEmpty(startTimeAttri.Value) )
                {
                    DateTime temp;
                    if (DateTime.TryParse(startTimeAttri.Value, out temp))
                    {
                        m_startTime = temp;
                    }
                }

                //load end time attribute
                XmlAttribute endTimeAttri = argNode.Attributes[UIServiceCfgDefines.s_endAttri];
                if ( null != endTimeAttri &&
                     !string.IsNullOrEmpty(endTimeAttri.Value) )
                {
                    DateTime temp;
                    if ( DateTime.TryParse( endTimeAttri.Value, out temp ) )
                    {
                        m_endTime = temp;
                    }
                }

                if ( m_startTime.HasValue &&
                     m_endTime.HasValue )
                {
                    if ( m_endTime <= m_startTime )
                    {
                        throw new Exception("The start time or end time isn't valid of a timeline");
                    }
                }

                //notify change
                XmlAttribute notifyChangeAttri = argNode.Attributes[UIServiceCfgDefines.s_notifyChangeAttri];
                if ( null != notifyChangeAttri &&
                     !string.IsNullOrEmpty(notifyChangeAttri.Value) )
                {
                    int temp = 0;
                    if ( int.TryParse(notifyChangeAttri.Value, out temp) )
                    {
                        m_notifyChange = temp == 0 ? false : true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.UIService.LogError("Failed to open a time line", ex);
                return false;	
            }

            m_state = playState.Stop;

            return true;
        }

        public virtual void Close()
        {

        }

        public virtual bool Start()
        {
            if ( !CanStart )
            {
                m_state = playState.Stop;
                return false;
            }
           
            m_curBeginTime = m_beginCfgTime;
            m_curReperat = m_reperatBehavior;
            m_state = playState.Play;
            m_bFirstStart = false;

            return true;
        }

        public virtual void Stop()
        {
            m_state = playState.Stop;
            OnTimelineEnd();
        }

        public virtual void Cancel()
        {
            m_state = playState.Stop;
            OnTimelineEnd();
        }

        public virtual void ReAdjustTime( ref int argBeginTime )
        {

        }

        public virtual XmlNode ToXmlNode( XmlDocument argDoc )
        {
            return null;
        }

        public virtual void NotifyScreenOk( System.Windows.Forms.HtmlDocument argDoc )
        {
            if ( !string.IsNullOrEmpty(m_targetName) )
            {
                m_targetElement = argDoc.GetElementById(m_targetName);
            }
            if (!string.IsNullOrEmpty(m_targetTextName))
            {
                m_targetTextElement = argDoc.GetElementById(m_targetTextName);
            }
            if ( null == m_targetElement )
            {
                m_targetElement = m_storyBoard.TargetElement;
                if ( null != m_targetElement )
                {
                    m_targetName = m_storyBoard.TargetName;
                }
            }
        }

        protected bool IsImageFile( string argExt )
        {
            Debug.Assert(!string.IsNullOrEmpty(argExt));

            bool ret = false;
            if ( string.Equals( argExt, ".jpg", StringComparison.OrdinalIgnoreCase ) ||
                 string.Equals( argExt, ".jpeg", StringComparison.OrdinalIgnoreCase ) ||
                 string.Equals( argExt, ".png", StringComparison.OrdinalIgnoreCase ) ||
                 string.Equals( argExt, ".bmp", StringComparison.OrdinalIgnoreCase ) ||
                 string.Equals( argExt, ".gif", StringComparison.OrdinalIgnoreCase ) ||
                 string.Equals( argExt, ".ico", StringComparison.OrdinalIgnoreCase ) ||
                 string.Equals( argExt, ".exif", StringComparison.OrdinalIgnoreCase ) ||
                 string.Equals( argExt, ".tiff", StringComparison.OrdinalIgnoreCase ) ||
                 string.Equals( argExt, ".emf", StringComparison.OrdinalIgnoreCase ) ||  
                 string.Equals( argExt, ".wmf", StringComparison.OrdinalIgnoreCase ) )
            {
                ret = true;
            }

            return ret;
        }

        public virtual void ResouceManager_languageChanged(string argOld, string argNew, bool argIsUI) { 
        

        }
        protected virtual bool CheckIsValid()
        {
            if ( !m_startTime.HasValue &&
                 !m_endTime.HasValue )
            {
                return true;
            }

            bool valid = true;
            DateTime now = DateTime.Now;
            //if ( m_startTime.HasValue &&
            //     m_endTime.HasValue )
            //{
            //    //if ( now < m_startTime ||
            //    //     now > m_endTime )
            //    //{
            //    //    valid = false;
            //    //}
            //}
            //else if ( m_startTime.HasValue )
            //{
            //    if (now < m_startTime)
            //    {
            //        valid = false;
            //    }
            //}
            if ( m_endTime.HasValue )
            {
                if ( now >= m_endTime.Value )
                {
                    valid = false;
                }
            }

            return valid;
        }
#endregion

#region property
        public GrgStoryboard OwnerBoard
        {
            get
            {
                return m_storyBoard;
            }
        }

        public string TargetName
        {
            get
            {
                return m_targetName;
            }
        }

        public HtmlElement Target
        {
            get
            {
                return m_targetElement;
            }
        }

        public HtmlElement TargetText
        {
            get
            {
                return m_targetTextElement;
            }
        }

        public virtual bool CanStart
        {
            get
            {
                if ( !m_startTime.HasValue )
                {
                    return true;
                }
                else
                {
                    DateTime now = DateTime.Now;
                    if ( now >= m_startTime.Value )
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        //public HtmlScreenElementBase AnimElement
        //{
        //    get
        //    {
        //        return m_element;
        //    }
        //}

        public bool IsStop
        {
            get
            {
                return m_state == playState.Play ? false : true;
            }
        }

        public string Key
        {
            get
            {
                return  m_key;
            }
        }

        public int Begin
        {
            get
            {
                return m_beginCfgTime;
            }
        }

        public int Duration
        {
            get
            {
                return m_duration;
            }
        }

        public int ReperatCount
        {
            get
            {
                return m_reperatBehavior;
            }
        }

        public bool IsValid
        {
            get
            {
                return CheckIsValid();
            }
        }

        public bool NeedRemove
        {
            get;
            set;
        }

        public bool IsConfig
        {
            get
            {
                return m_isConfig;
            }
            set
            {
                m_isConfig = value;
            }
        }

        public bool CanStartPlay
        {
            get
            {
                return m_canStartPlay;
            }
            set
            {
                m_canStartPlay = value;
            }
        }

        public bool NotifyChange
        {
            get
            {
                return m_notifyChange;
            }
        }
#endregion

#region event
        public event EventHandler TimelineEnd;

        public event EventHandler TimelineStart;

        protected void OnTimelineEnd()
        {
            if ( null != TimelineEnd )
            {
                TimelineEnd.Invoke(this, EventArgs.Empty);
            }
        }

        protected virtual void OnTimelineStart()
        {
            if ( null != TimelineStart )
            {
                TimelineStart.Invoke(this, EventArgs.Empty);
            }
        }
#endregion

#region  field
        protected GrgStoryboard m_storyBoard = null;

        protected int m_beginCfgTime = 0;

        protected int m_curBeginTime = 0;

        protected string m_targetName = null;

        protected string m_key = null;

        protected string m_targetTextName = null;

 //       protected HtmlScreenElementBase m_element = null;
        protected HtmlElement m_targetElement = null;

        protected HtmlElement m_targetTextElement = null;

        protected int m_reperatBehavior = 1;

        protected int m_curReperat = 0;

        protected playState m_state = playState.Stop;

        protected int m_duration = 0;

        protected bool m_bFirstStart = false;

        protected string m_baseUri = null;

        protected string m_configBaseUri = null;

        protected bool m_canStartPlay = true;

        protected bool m_isConfig = false;

        protected bool m_notifyChange = false;

        protected DateTime? m_startTime;

        protected DateTime? m_endTime;
#endregion
    }
}
