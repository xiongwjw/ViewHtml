using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;
using UIServiceProtocol;
using Attribute4ECAT;
using System.Timers;
using LogProcessorService;

namespace eCATActivityTest
{
    public class BusinessActivityeCATBase : IBusinessActivity, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int m_curCount = 60;
        private bool m_couterVisible = true;

        [GrgBindTarget("Count", Access = AccessRight.OnlyRead, Type = TargetType.Int)]
        public int Count
        {
            get
            {
                return m_curCount;
            }
            set
            {
                m_curCount = value;
                OnPropertyChanged("Count");
            }
        }

        [GrgBindTarget("CouterVisible", Access = AccessRight.OnlyRead, Type = TargetType.Bool)]
        public bool CouterVisible
        {
            get
            {
                return m_couterVisible;
            }
            set
            {
                m_couterVisible = value;
                OnPropertyChanged("CouterVisible");
            }
        }

        //private Timer m_counterTimer = new Timer();

        //protected bool StartCounter()
        //{
        //    //if (!ResetCounter())
        //    //{
        //    //    Log.BusinessService.LogWarn("Failed to reset counter of timeout and then send timeout signal");
        //    //  //  SignalTimeout();
        //    //    return false;
        //    //}



        //    if (null == m_counterTimer)
        //    {
        //        Log.BusinessService.LogWarn("The counter timer has been disposed");
        //        return false;
        //    }

        //    m_counterTimer.BeginInit();

        //    m_counterTimer.AutoReset = true;
        //    m_counterTimer.Interval = 1000;
        //    m_counterTimer.Elapsed += OnTimerCount;

        //    m_counterTimer.EndInit();


        //    if (m_counterTimer.Enabled)
        //    {
        //        m_counterTimer.Stop();
        //    }
        //    //      m_counterStoped = false;

        //    m_counterTimer.Start();
        //    return true;
        //}

        // ~BusinessActivityeCATBase()
        //{
        //    StopCounter();
        //}

        //protected void StopCounter()
        //{
        //    //m_counterStoped = true;

        //    if (null != m_counterTimer)
        //    {
        //        m_counterTimer.Stop();
        //        m_counterTimer.Elapsed -= OnTimerCount;
        //        m_counterTimer.Dispose();
        //        m_counterTimer = null;
        //    }
        //}


        //private int m_countDeltra = 1;

        //private void OnTimerCount(object argSender,
        //                           ElapsedEventArgs arg)
        //{
        //    if (null == m_counterTimer ||
        //        !m_counterTimer.Enabled)
        //    {
        //        return;
        //    }

        //    m_curCount += m_countDeltra;
        //    Count = m_curCount;

        //}


        protected void OnPropertyChanged(string propName)
        {
            Debug.Assert(!string.IsNullOrEmpty(propName));
            if (null != PropertyChanged)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }

        public  bool Init()
        {
            return true;
        }

        public void Exit()
        { }

        public emBusActivityResult_t PreRun(eCATContext objContext)
        {
            return InnerPreRun(objContext);
        }

        public  emBusActivityResult_t Run(eCATContext objContext)
        {
            return InnerRun(objContext);
        }


        public void EndRun(eCATContext argContext)
        {
            InnerEndRun(argContext);
        }

        public  bool CanTerminate(ref string strMsg)
        {
            return InnerCanTerminate(ref strMsg);
        }

       public void Terminate(bool argIsUserCancel = false)
        {
            InnerTerminate(argIsUserCancel);
        }

        public void Close(string argNextCondition = "9999")
        {
            InnerClose(argNextCondition);
        }

        public  emBusiCallbackResult_t OnUIEvtHandle(IUIService iSender,
                                              UIEventArg objArg)
        {
            return InnerOnUIEvtHandle(iSender, objArg);
        }

        public string Name { get; set; }


        protected virtual emBusActivityResult_t InnerRun(eCATContext objContext)
        {
          //  StartCounter();
            return emBusActivityResult_t.Success;
        }

        protected virtual void InnerEndRun(eCATContext argContext)
        {
            
        }

        protected virtual bool InnerCanTerminate(ref string strMsg)
        {
            return true;
        }

        protected virtual void InnerTerminate(bool argIsUserCancel)
        {
            
        }

        protected virtual void InnerClose(string argNextConditon)
        {

        }

        protected virtual emBusActivityResult_t InnerPreRun(eCATContext objContext)
        {

            return emBusActivityResult_t.Success;
        }

        protected virtual emBusiCallbackResult_t InnerOnUIEvtHandle(IUIService iUI,
                                                             UIEventArg objArg)
        {
            return emBusiCallbackResult_t.Bypass;
        }



    }
}
