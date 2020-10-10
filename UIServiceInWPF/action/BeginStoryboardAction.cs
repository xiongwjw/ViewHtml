using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using LogProcessorService;
using UIServiceInWPF.trigger;
using UIServiceInWPF.GrgStoryboardNS;

namespace UIServiceInWPF.action
{
    class BeginStoryboardAction : ActionBase
    {
#region constructor
        public BeginStoryboardAction( UITriggerBase argTrigger,
                                      string argStoryboardName ) : base(argTrigger)
        {
            Debug.Assert(!string.IsNullOrEmpty(argStoryboardName));

            m_storyboardName = argStoryboardName;
        }
#endregion

#region method
        public override void Do()
        {
            Debug.Assert(null != m_trigger.Owner);

            GrgStoryboard storyboard = m_trigger.Owner.FindStoryboard(m_storyboardName);
            if ( null != storyboard )
            {
                storyboard.Start();
            }
        }

        public override void Terminate()
        {
            Debug.Assert(null != m_trigger.Owner);

            GrgStoryboard storyboard = m_trigger.Owner.FindStoryboard(m_storyboardName);
            if ( null != storyboard )
            {
                storyboard.Stop();
            }
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
