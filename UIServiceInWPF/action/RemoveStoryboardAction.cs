/********************************************************************
	FileName:   RemoveStoryboardAction
    purpose:	

	author:		huang wei
	created:	2013/03/20

    revised history:
	2013/03/20  

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
using UIServiceInWPF.trigger;

namespace UIServiceInWPF.action
{
    class RemoveStoryboardAction : ActionBase
    {
#region constructor
        public RemoveStoryboardAction( UITriggerBase argTrigger,
                                       string argName ) : base(argTrigger)
        {
            m_storyboardName = argName;
        }
#endregion

#region method
        public override void Do()
        {
            
        }

        public override void Terminate()
        {
            
        }

        public override void UnDo()
        {
            base.UnDo();
        }

        public override void Clear()
        {
            base.Clear();
        }
#endregion

#region field
        protected string m_storyboardName = null;
#endregion
    }
}
