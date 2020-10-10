/********************************************************************
	FileName:   PlaySoundAction
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
using LogProcessorService;
using UIServiceInWPF.trigger;
using ResourceManagerProtocol;

namespace UIServiceInWPF.action
{
    class PlaySoundAction : ActionBase
    {
#region constructor
        public PlaySoundAction( UITriggerBase argTrigger,
                                string argSource ) : base(argTrigger)
        {
            Debug.Assert(!string.IsNullOrEmpty(argSource));
            m_source = argSource.Trim();
            if (m_source.StartsWith(s_varLeftSymbol) &&
                 m_source.EndsWith(s_varRightSymbol))
            {
                m_source = m_source.Trim(s_LeftFlag, s_RightFlag);
                m_needLoadResource = true;
            }
            else
            {
                m_needLoadResource = false;
                m_source = AppDomain.CurrentDomain.BaseDirectory + m_source;
            }
        }
#endregion

#region method
        public override void Do()
        {
            if ( m_needLoadResource &&
                 null != ResourceManagerInterface )
            {
                string soundFile = null;
                if ( !ResourceManagerInterface.CurrentUIResource.QueryVoicePath( m_source, out soundFile ) )
                {
                    soundFile = m_source;
                }

                SoundPlayer.Instance.Play(soundFile, Loop, SkipEnable);
            }
            else
            {
                SoundPlayer.Instance.Play(AppDomain.CurrentDomain.BaseDirectory + m_source, Loop, SkipEnable);
            }          
        }

        public override void Terminate()
        {
            
            SoundPlayer.Instance.Stop();
        }
#endregion

#region property
        public string Source
        {
            get
            {
                return m_source;
            }
        }

        public bool Loop
        {
            get;
            set;
        }

        public bool SkipEnable
        {
            get;
            set;
        }
#endregion

#region field
        protected string m_source = null;
#endregion
    }
}
