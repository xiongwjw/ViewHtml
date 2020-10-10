using Attribute4ECAT;
using LogProcessorService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIServiceProtocol;

namespace eCATActivityTest
{
    [GrgActivity("{8A840579-19CE-4395-92F0-8690BB152251}",
               Name = "InputInfoBase",
               Description = "InputInfoBaseDes",
               NodeNameOfConfiguration = "InputInfoBase",
               Catalog = "InputOrSelect")]
    public class BusinessActivityInputInfoBase : BusinessActivityeCATBase
    {

        protected string m_CustomerIDNumber = null;
        protected int m_IsCardInsertFlag = 0;


        [GrgBindTarget("CustomerIDNumber", Type = TargetType.String, Access = AccessRight.ReadAndWrite)]
        public string CustomerIDNumber
        {
            get
            {
                return m_CustomerIDNumber;
            }
            set
            {
                m_CustomerIDNumber = value;
                OnPropertyChanged("CustomerIDNumber");
            }
        }

        [GrgBindTarget("isCardInsertFlag", Type = TargetType.Int, Access = AccessRight.ReadAndWrite)]
        public int IsCardInsertFlag
        {
            get
            {
                return m_IsCardInsertFlag;
            }
            set
            {
                m_IsCardInsertFlag = value;
                OnPropertyChanged("IsCardInsertFlag");
            }
        }

        #region method of creating
        [GrgCreateFunction("Create")]
        public  static IBusinessActivity Create()
        {
            return new BusinessActivityInputInfoBase() as IBusinessActivity;
        }
        #endregion

        #region constructor
        public BusinessActivityInputInfoBase()
        {
        }
        #endregion


        protected override emBusActivityResult_t InnerRun(eCATContext argContext)
        {
            Log.Action.LogDebug("Enter action: InputInfoBase");

            base.InnerRun(argContext);

            m_CustomerIDNumber = "jax.wang";
            

            return emBusActivityResult_t.Success;
        }

        protected override emBusiCallbackResult_t InnerOnUIEvtHandle(IUIService iUI, UIEventArg argUIEvent)
        {
            Debug.Assert(null != iUI && null != argUIEvent);

            emBusiCallbackResult_t result = base.InnerOnUIEvtHandle(iUI, argUIEvent);
            if (emBusiCallbackResult_t.Swallowd == result)
            {
                return result;
            }

            try
            {
                if (argUIEvent.EventName == UIEventNames.s_ClickEvent)
                {
                    if (null != argUIEvent.Key)
                    {
                        Log.Action.LogInfoFormat("UI event: the key {0}({1}) pressed", argUIEvent.ElementName, argUIEvent.Key);

                    }
                    return emBusiCallbackResult_t.Swallowd;
                }
            }
            catch (System.Exception ex)
            {
                Log.Action.LogError(ex.Message, ex);

            }

            return emBusiCallbackResult_t.Bypass;
        }


    }
}
