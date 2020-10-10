/********************************************************************
	FileName:   UIEvtArg
    purpose:	

	author:		huang wei
	created:	2012/10/23

    revised history:
	2012/10/23  

================================================================
    Copyright (C) 2012, Grgbanking CO,. Ltd. All rights reserved.
================================================================
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UIServiceProtocol
{
    public class UIEventArg : EventArgs
    {
#region constructor
        public UIEventArg()
        {
            IsImpersonated = false;
        }
#endregion

#region property
        /// <summary>
        /// Source of a event of a UI service. 
        /// </summary>
        public IUIService Source
        {
            get;
            set;
        }

        /// <summary>
        /// Name of a screen 
        /// </summary>
        public string ScreenName
        {
            get
            {
                return m_scrName;
            }
            set
            {
                m_scrName = value;
            }
        }

        /// <summary>
        /// Name of element
        /// </summary>
        public string ElementName
        {
            get
            {
                return m_elementName;
            }
            set
            {
                m_elementName = value;
            }
        }

        /// <summary>
        /// Name of event
        /// </summary>
        public string EventName
        {
            get
            {
                return m_eventName;
            }
            set
            {
                m_eventName = value;
            }
        }

        /// <summary>
        /// Key value of a element
        /// </summary>
        public object Key
        {
            get
            {
                return m_key;
            }
            set
            {
                m_key = value;
            }
        }

        public object Param
        {
            get
            {
                return m_param;
            }
            set
            {
                m_param = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsImpersonated
        {
            get;
            set;
        }

        public short PriorityLevel
        {
            get
            {
                return m_priLevel;
            }
        }
#endregion

#region field
        protected short m_priLevel = 0;

        protected string m_scrName = string.Empty;

        protected string m_elementName = string.Empty;

        protected string m_eventName = string.Empty;

        protected object m_key = null;

        protected object m_param = null;
#endregion
    }
}
