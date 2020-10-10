using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eCATActivityTest
{
    [Flags]
    public enum emBusActivityResult_t : byte
    {
        Success,
        Failure,
        Timeout,
        KeyError,
        InvalidCard,
        HardwareError,
        GeneralError,
        InputWithdrawalAmountExceedRetryTimes,
        IDCFromErrorToNormal,
        //rollback之前，idc状态为error
        IDCErrorBeforeRollBack
    }

    [Flags]
    public enum emBusiCallbackResult_t : byte
    {
        Bypass,
        Swallowd,
        Unhandle
    }

}
