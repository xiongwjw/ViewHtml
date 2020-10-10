/********************************************************************
	FileName:   ActionBase
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

namespace UIServiceInWPF.action
{
    public abstract class ActionBase
    {
#region constructor
        public ActionBase( UITriggerBase argTrigger )
        {
            Debug.Assert(null != argTrigger);
            m_trigger = argTrigger;
        }
#endregion

#region method
        public abstract void Do();

        public abstract void Terminate();

        public virtual void UnDo()
        {

        }

        public virtual void Clear()
        {

        }
#endregion

#region property
        public UITriggerBase Trigger
        {
            get
            {
                return m_trigger;
            }
        }

        public IResourceManager ResourceManagerInterface
        {
            get;
            set;
        }
#endregion

#region field
        protected UITriggerBase m_trigger = null;

        protected bool m_needLoadResource = false;

        protected bool m_needLoadDataCache = false;

        public const string s_varLeftSymbol = "(";

        public const string s_varRightSymbol = ")";

        public const char s_LeftFlag = '(';

        public const char s_RightFlag = ')';

        public const string s_dataLeftSymbol = "{";

        public const string s_dataRightSymbol = "}";

        public const char s_dataLeftFlag = '{';

        public const char s_dataRightFlag = '}';
#endregion
    }
}
