using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UIServiceInWPF;
using UIServiceProtocol;
using System.Diagnostics;
using System.Windows.Threading;

namespace UIServiceInWPF.HtmlScreenElement
{
    class HtmlScreenCountdownElement : HtmlScreenElementBase
    {
#region constructor
        public HtmlScreenCountdownElement( HtmlElement argElement,
                                           HtmlRender argParent)
            : base(argElement, argParent)
        {
            string reserved = m_ihostedElement.GetAttribute(UIServiceCfgDefines.s_reservedNumAttri);
            if (!string.IsNullOrEmpty(reserved))
            {
                int temp = 0;
                if (int.TryParse(reserved, out temp))
                {
                    m_reservedNumber = Math.Abs(temp) == 0 ? -1 : Math.Abs(temp);
                    m_reservedFormat = string.Format("D{0}", m_reservedNumber);
                }
            }
        }
#endregion

#region methods of the base
        public override bool SetPropertyValue(string argProperty, object argValue)
        {
            if ( m_useUITimer &&
                 argProperty.Equals(UIPropertyKey.s_ContentKey, StringComparison.Ordinal) )
            {
                return true;
            }

            if ( null != argValue &&
                 argProperty.Equals( UIPropertyKey.s_ContentKey, StringComparison.Ordinal ) &&
                 m_reservedNumber > 0 &&
                 ( argValue is int ||
                   argValue is long ||
                   argValue is short ) )
            {
                return base.SetPropertyValue(argProperty, ((int)argValue).ToString(m_reservedFormat));
            }
            else
            {
                return base.SetPropertyValue(argProperty, argValue);
            }
            
        }

        public override void InnerDispose()
        {
            base.InnerDispose();

            if ( null != m_timer )
            {
                m_timer.Stop();
                m_timer = null;
            }

            m_useUITimer = false;
            m_begin = 0;
            m_end = 0;
        }

        public override bool Open()
        {
            Debug.Assert(null != m_ihostedElement);
            //load useUITimer attribute
            string attriValue = m_ihostedElement.GetAttribute(UIServiceCfgDefines.s_useUITimerAttri);
            if ( !string.IsNullOrEmpty(attriValue) )
            {
                int value = 0;
                if ( int.TryParse( attriValue,out value ) )
                {
                    m_useUITimer = value == 0 ? false : true;
                }
            }
            if ( !m_useUITimer )
            {
                return base.Open();
            }

            //load begin attribute
            attriValue = m_ihostedElement.GetAttribute(UIServiceCfgDefines.s_beginAttri);
            if ( !string.IsNullOrEmpty( attriValue ) )
            {
                if ( !int.TryParse( attriValue, out m_begin) ||
                     m_begin < 0 )
                {
                    m_useUITimer = false;
                    return base.Open();
                }
            }
            //load end attribute
            attriValue = m_ihostedElement.GetAttribute(UIServiceCfgDefines.s_endAttri);
            if ( string.IsNullOrEmpty( attriValue ) )
            {
                if ( m_begin > 0 )
                {
                    //may be countdown
                    m_end = 0;
                }
                else
                {
                    //may be 
                    m_end = int.MaxValue;
                }
            }
            else
            {
                if ( !int.TryParse( attriValue, out m_end ) ||
                     m_end < 0 )
                {
                    m_useUITimer = false;
                    return base.Open();
                }
            }
            if ( m_end == m_begin )
            {
                m_useUITimer = false;
                return base.Open();
            }

            if ( m_begin > m_end )
            {
                m_step = -1;
            }
            else
            {
                m_step = 1;
            }
            m_curCount = m_begin;
            m_ihostedElement.InnerText = m_curCount.ToString();

            m_timer = new DispatcherTimer();
            m_timer.Interval = new TimeSpan(0, 0, 1);
            m_timer.Tick += new EventHandler(m_timer_Tick);
            m_timer.Start();

            return base.Open();
        }

        public void m_timer_Tick(object sender, EventArgs e)
        {
            if ( !m_timer.IsEnabled )
            {
                return;
            }

            m_curCount += m_step;
            if ( m_step > 0 )
            {
                if ( m_curCount >= m_end )
                {
                    m_timer.Stop();
                }
                else
                {
                    if ( m_reservedNumber > 0 )
                    {
                        m_ihostedElement.InnerText = m_curCount.ToString(m_reservedFormat);
                    }
                    else
                    {
                        m_ihostedElement.InnerText = m_curCount.ToString();
                    }                    
                }
            }
            else
            {
                if ( m_curCount < m_end )
                {
                    m_timer.Stop();
                }
                else
                {
                    if ( m_reservedNumber > 0 )
                    {
                        m_ihostedElement.InnerText = m_curCount.ToString(m_reservedFormat);
                    }
                    else
                    {
                        m_ihostedElement.InnerText = m_curCount.ToString();
                    }             
                }
            }
        }
#endregion

#region field
        protected bool m_useUITimer = false;

        protected int m_begin = 0;

        protected int m_end = 0;

        protected int m_curCount = 0;

        protected int m_step = 1;

        protected int m_reservedNumber = -1;

        protected string m_reservedFormat = null;

        protected DispatcherTimer m_timer = null;
#endregion
    }
}
