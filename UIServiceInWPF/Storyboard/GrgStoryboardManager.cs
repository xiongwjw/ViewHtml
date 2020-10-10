/********************************************************************
	FileName:   GrgStoryboardManager
    purpose:	

	author:		huang wei
	created:	2013/03/19

    revised history:
	2013/03/19  

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
using UIServiceInWPF.GrgStoryboardNS;
using System.Windows.Threading;

namespace UIServiceInWPF.GrgStoryboardNS
{
    class GrgStoryboardManager
    {
#region constructor
        protected GrgStoryboardManager()
        {
            m_storyboardTimer.Interval = new TimeSpan( 0, 0, 0, 0, s_defaultInterval );
            m_storyboardTimer.Tick += Timer_Tick;
        }

        static GrgStoryboardManager()
        {
            s_manager = new GrgStoryboardManager();
        }
#endregion

#region method
        private void Timer_Tick(object sender, EventArgs e)
        {
            if ( !m_storyboardTimer.IsEnabled ) 
            {
                return;
            }

            lock ( m_storyboardLock )
            {
                foreach ( var board in m_listStoryboard )
                {
                    board.TimeTick();
                }
            }
        }

        public void ScheduleStoryboard( GrgStoryboard argStoryboard )
        {
            Debug.Assert(null != argStoryboard);
            if ( null == argStoryboard )
            {
                throw new ArgumentNullException("argStoryboard");
            }

            lock ( m_storyboardLock )
            {
                GrgStoryboard existBoard = FindStoryBoard(argStoryboard);
                if ( null != existBoard )
                {
                    return;
                }

                m_listStoryboard.Add(argStoryboard);
                if ( !m_storyboardTimer.IsEnabled )
                {
                    m_storyboardTimer.Start();
                }
            }
        }

        public void CancelSchedule( GrgStoryboard argStoryboard )
        {
            Debug.Assert(null != argStoryboard);
            if (null == argStoryboard)
            {
                throw new ArgumentNullException("argStoryboard");
            }

            lock ( m_storyboardLock )
            {
                int total = m_listStoryboard.Count;
                for (int i = 0; i < total; ++i)
                {
                    if ( m_listStoryboard[i].Name.Equals(argStoryboard.Name, StringComparison.Ordinal) )
                    {
                        m_listStoryboard.RemoveAt(i);
                        break;
                    }
                }

                if ( m_listStoryboard.Count == 0 &&
                     m_storyboardTimer.IsEnabled )
                {
                    m_storyboardTimer.Stop();
                }
            }
        }

        private GrgStoryboard FindStoryBoard( GrgStoryboard argStoryboard )
        {
            Debug.Assert( null != argStoryboard );
            foreach ( var board in m_listStoryboard )
            {
                if ( board.Name.Equals( argStoryboard.Name, StringComparison.Ordinal ) )
                {
                    return board;
                }
            }

            return null;
        }
#endregion

#region properpty
        public static GrgStoryboardManager Instance
        {
            get
            {
                return s_manager;
            }
        }
#endregion

#region field
        private static GrgStoryboardManager s_manager = null;

        public const int s_defaultInterval = 50;

        private object m_storyboardLock = new object();

        protected DispatcherTimer m_storyboardTimer = new DispatcherTimer();

        protected List<GrgStoryboard> m_listStoryboard = new List<GrgStoryboard>();
#endregion
    }
}
