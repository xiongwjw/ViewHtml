/********************************************************************
	FileName:   SetterAction
    purpose:	

	author:		huang wei
	created:	2013/01/12

    revised history:
	2013/01/12  

================================================================
    Copyright (C) 2013, Grgbanking CO,. Ltd. All rights reserved.
================================================================
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using UIServiceInWPF.trigger;
using ResourceManagerProtocol;
using UIServiceInWPF;

namespace UIServiceInWPF.action
{
    class SetterAction : ActionBase
    {
#region constructor
        public SetterAction( UITriggerBase argTrigger,
                             string argTargetElement,
                             string argBindProperty,
                             string argValue) : base(argTrigger)
        {
            Debug.Assert(!string.IsNullOrEmpty(argTargetElement) && !string.IsNullOrEmpty(argBindProperty));
            m_targetElement = argTargetElement;
            m_bindProperty = argBindProperty;
            m_newValue = ((string)argValue).Trim();
            if ( m_newValue.StartsWith(s_varLeftSymbol) &&
                 m_newValue.EndsWith(s_varRightSymbol))
            {
                m_newValue = m_newValue.Trim(s_LeftFlag, s_RightFlag);
                m_needLoadResource = true;
            }
            else if ( m_newValue.StartsWith( s_dataLeftSymbol ) &&
                      m_newValue.EndsWith( s_dataRightSymbol ) )
            {
                m_newValue = m_newValue.Trim( s_dataLeftFlag, s_dataRightFlag );
                m_needLoadDataCache = true;
            }
            else
            {
                m_needLoadResource = false;
                m_needLoadDataCache = false;
            }

            NeedUndo = false;
        }
#endregion

#region property
        public string TargetName
        {
            get
            {
                return m_targetElement;
            }
        }

        public string BindProperty
        {
            get
            {
                return m_bindProperty;
            }
        }

        public bool NeedUndo
        {
            get;
            set;
        }
#endregion

#region method
        public override void Do()
        {
            Debug.Assert(null != m_trigger);
            if ( NeedUndo && 
                 null == m_oldValue )
            {
                m_trigger.Owner.GetPropertyValue(m_targetElement, m_bindProperty, out m_oldValue);
            }

            if ( m_needLoadResource &&
                 null != ResourceManagerInterface )
            {
                string result = null;
                if ( ResourceManagerInterface.CurrentUIResource.LoadString( m_newValue, out result ) )
                {
                    m_trigger.Owner.SetPropertyValue(m_targetElement, m_bindProperty, result);
                }
            }
            else if ( m_needLoadDataCache )
            {
                object value = HtmlRender.SingleInstance.GetBindedData(m_newValue);
                if ( null != value )
                {
                    m_trigger.Owner.SetPropertyValue(m_targetElement, m_bindProperty, value);
                }
            }
            else
            {
                m_trigger.Owner.SetPropertyValue(m_targetElement, m_bindProperty, m_newValue);
            }          
        }

        public override void Terminate()
        {
            UnDo();
            m_oldValue = null;
        }

        public override void UnDo()
        {
            if ( !NeedUndo ||
                 null == m_oldValue )
            {
                return;
            }

            Debug.Assert(null != m_trigger);
            m_trigger.Owner.SetPropertyValue(m_targetElement, m_bindProperty, m_oldValue);
        }
#endregion

#region field
        protected string m_targetElement = null;

        protected string m_bindProperty = null;

        protected object m_oldValue = null;

        protected string m_newValue = null;
#endregion
    }
}
