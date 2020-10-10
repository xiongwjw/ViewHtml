using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using LogProcessorService;
using System.Xml;
//using mshtml;
using System.IO;
using UIServiceInWPF.HtmlScreenElement;
using UIServiceProtocol;

namespace UIServiceInWPF.GrgStoryboardNS.Timeline
{
    class MediaTimeline : TimelineBase
    {
#region constructor
        public MediaTimeline(GrgStoryboard argBoard)
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

            if ( !m_isAudio &&
                 null == m_targetScreenElement )
            {
                return false;
            }

            m_curTimeInterval += GrgStoryboardManager.s_defaultInterval;
            if ( m_curTimeInterval > m_duration )
            {
                Stop();
                return false;
            }

            return true;
        }

        public override bool Open(XmlNode argNode)
        {
            Debug.Assert(null != argNode);

            Log.UIService.LogDebug("Prepare for opening media timeline"); 
            bool result = base.Open(argNode);
            if ( !result )
            {
                Log.UIService.LogError("Failed to open media timeline");
                return result;
            }

            //query source attribute
            XmlAttribute sourceAttri = argNode.Attributes[UIServiceCfgDefines.s_sourceAttri];
            if ( null == sourceAttri ||
                 string.IsNullOrEmpty(sourceAttri.Value) )
            {
                Log.UIService.LogError("The source attribute of media time line must be exist");
                return false;
            }
            //check if the file is exists.
            string sourcFilePath = AppDomain.CurrentDomain.BaseDirectory + sourceAttri.Value;
            if ( !File.Exists( sourcFilePath ) )
            {
                Log.UIService.LogError("The source file isn't exists");
                return false;
            }
            m_source = sourcFilePath;
            m_configSource = sourceAttri.Value;
            m_isAudio = IsAudio(Path.GetExtension(sourcFilePath));
            //check target attribute
            if ( !m_isAudio &&
                 string.IsNullOrEmpty( m_targetName ) )
            {
                Log.UIService.LogWarn("The target to play the video must exist");
                return false;
            }

            return true;
        }

        public override void Close()
        {
            base.Close();
        }

        public override void NotifyScreenOk(System.Windows.Forms.HtmlDocument argDoc)
        {
            base.NotifyScreenOk(argDoc);

            if ( null != m_targetElement )
            {
                m_targetScreenElement = HtmlRender.SingleInstance.FindHtmlScreenElementByElement(m_targetElement);
            }
        }

        public override bool Start()
        {
            if ( null == m_targetScreenElement )
            {
                m_state = playState.Stop;
                return false;
            }

            if (!base.Start())
            {
                return false;
            }

            m_curTimeInterval = 0;
            if ( m_duration == 0 )
            {
                m_duration = int.MaxValue;
            }

            return true;
        }

        public override void Stop()
        {
            base.Stop();

            try
            {
                if (null != m_targetScreenElement)
                {
                    m_targetScreenElement.SetPropertyValue(UIPropertyKey.s_VisibleKey, false);
                    m_targetScreenElement.SetPropertyValue(UIPropertyKey.s_ContentKey, string.Empty);
                }
                else if (m_isAudio)
                {
                    SoundPlayer.Instance.Stop();
                }
            }
            catch (System.Exception ex)
            {
                Trace.Write(ex.Message);
                //Log.UIService.LogError("Failed to stop a media timeline", ex);	
            }
        }

        public override void Cancel()
        {
            base.Cancel();

            if ( null != m_targetElement )
            {
                m_targetElement = null;
            }
            else if ( m_isAudio )
            {
                SoundPlayer.Instance.Stop();
            }
        }

        public override void ReAdjustTime(ref int argBeginTime)
        {
            m_beginCfgTime = argBeginTime;
            argBeginTime += m_duration;
        }

        public override XmlNode ToXmlNode(XmlDocument argDoc)
        {
            Debug.Assert(null != argDoc);

            XmlNode mediaNode = argDoc.CreateElement( UIServiceCfgDefines.s_mediaTimelineNode );
            XmlAttribute attri = argDoc.CreateAttribute(UIServiceCfgDefines.s_beginAttri);
            attri.Value = m_beginCfgTime.ToString();
            mediaNode.Attributes.Append(attri);
            attri = argDoc.CreateAttribute(UIServiceCfgDefines.s_sourceAttri);
            attri.Value = m_configSource;
            mediaNode.Attributes.Append(attri);
            attri = argDoc.CreateAttribute(UIServiceCfgDefines.s_targetAttri);
            attri.Value = m_targetName;
            mediaNode.Attributes.Append(attri);
            attri = argDoc.CreateAttribute(UIServiceCfgDefines.s_durationAttri);
            attri.Value = m_duration.ToString();
            mediaNode.Attributes.Append(attri);
            if ( m_startTime.HasValue )
            {
                attri = argDoc.CreateAttribute(UIServiceCfgDefines.s_startTimeAttri);
                attri.Value = m_startTime.Value.ToString();
                mediaNode.Attributes.Append(attri);
            }
            if ( m_endTime.HasValue )
            {
                attri = argDoc.CreateAttribute(UIServiceCfgDefines.s_endTimeAttri);
                attri.Value = m_endTime.Value.ToString();
                mediaNode.Attributes.Append(attri);
            }

            return mediaNode;
        }

        private bool IsAudio( string argFileExt )
        {
            Debug.Assert(!string.IsNullOrEmpty(argFileExt));

            if ( string.Equals( argFileExt, ".wav", StringComparison.OrdinalIgnoreCase ) ||
                 string.Equals( argFileExt, ".mp3", StringComparison.OrdinalIgnoreCase ) ||
                 string.Equals( argFileExt, ".mid", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals( argFileExt, ".midi", StringComparison.OrdinalIgnoreCase ) ||
                 string.Equals( argFileExt, ".mpga", StringComparison.OrdinalIgnoreCase ) ||
                 string.Equals( argFileExt, ".wma", StringComparison.OrdinalIgnoreCase ) )
            {
                return true;
            }  

            

            return false;
        }

        protected override void OnTimelineStart()
        {
            base.OnTimelineStart();

            try
            {
                if (null != m_targetScreenElement)
                {
                    m_targetScreenElement.SetPropertyValue(UIPropertyKey.s_ContentKey, m_source);
                    m_targetScreenElement.SetPropertyValue(UIPropertyKey.s_VisibleKey, true);
                }
                else if (m_isAudio)
                {
                    SoundPlayer.Instance.Play(m_source);
                }
            }
            catch (System.Exception ex)
            {
                Log.UIService.LogDebug("Failed to start a media timeline", ex);	
            }
        }
#endregion

#region field
        protected string m_source = null;

        protected string m_configSource = null;

        protected bool m_isAudio = false;

        protected int m_curTimeInterval = 0;

        protected HtmlScreenElementBase m_targetScreenElement = null;
#endregion
    }
}
