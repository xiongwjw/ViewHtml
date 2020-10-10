using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace HelperService
{
    public static class SpecialHandle
    {
        public static string SendDataTransType
        {
            get;
            set;
        }

        public static byte[] MaskSendDataContent
        {
            get;
            set;
        }

        public static List<byte> ReceiveDataHead
        {
            get;
            set;
        }

        public static int ReceiveDataLength
        {
            get;
            set;
        }

        public static bool Offline
        {
            get;
            set;
        }

        public static bool OpenAllLog
        {
            get;
            set;
        }

        public static string Track1
        {
            get;
            set;
        }

        public static string Track2
        {
            get;
            set;
        }

        public static string Track3
        {
            get;
            set;
        }

        // 重启机器
        public static void Reboot()
        {
            OsOperationHelper.Reboot();
        }
    }
}