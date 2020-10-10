/********************************************************************
	FileName:   EventTrigger
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
using UIServiceInWPF.screen;
using System.Diagnostics;
using LogProcessorService;

namespace UIServiceInWPF.trigger
{
    class EventTrigger : UITriggerBase
    {
#region constructor
        public EventTrigger( Screen argOwner,
                             string argCondition ) : base( argOwner )
        {
            Debug.Assert(!string.IsNullOrEmpty(argCondition));
            m_condition = argCondition.Trim();
            //
            string[] arrConditions = m_condition.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
            if ( arrConditions.Length == 0 )
            {
                m_condition = null;
                return;
            }
            m_listConditions = arrConditions.ToList();
        }
#endregion

#region method
        public override bool CanTrigger(string strEvent)
        {
            if ( null == m_listConditions )
            {
                return false;
            }

            foreach ( var item in m_listConditions )
            {
                if ( item.Equals( strEvent, StringComparison.OrdinalIgnoreCase ) )
                {
                    return true;
                }
            }

            return false;
        }

        public override void Exit()
        {
            base.Exit();
            m_condition = null;
            m_listConditions.Clear();
        }
#endregion

#region property
        public string ConditionConfiguration
        {
            get
            {
                return m_condition;
            }
        }
#endregion

#region field
        protected string m_condition = null;

        protected List<string> m_listConditions = null;
#endregion
    }
}
