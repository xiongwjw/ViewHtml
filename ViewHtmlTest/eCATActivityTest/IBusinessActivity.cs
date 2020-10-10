using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIServiceProtocol;

namespace eCATActivityTest
{
    public interface IBusinessActivity
    {
        #region methods
        bool Init();

        void Exit();

        emBusActivityResult_t PreRun(eCATContext objContext);

        emBusActivityResult_t Run(eCATContext objContext);

        void EndRun(eCATContext argContext);

        bool CanTerminate(ref string strMsg);

        void Terminate(bool argIsUserCancel = false);

        void Close(string argNextCondition = "9999");

        emBusiCallbackResult_t OnUIEvtHandle(IUIService iSender,
                                              UIEventArg objArg);

        #endregion

        #region properties
        string Name
        {
            get;
            set;
        }
        #endregion
    }
}
