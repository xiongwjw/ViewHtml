using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using GrgCDbLog;

namespace LogProcessorService
{
    // Method Begin and End Flag


    /// <summary>
    /// Logger recorder
    /// </summary>
    public class Logger
    {
        static bool m_bEnableLog = true;
        static bool m_bEnableAllLog = true;
        static Hashtable HashMatchKeyInfo = new Hashtable();

        static LogFile m_logFile;
        /// <summary>
        /// 
        /// </summary>
        static Logger()
        {
            m_logFile = new LogFile();
            m_logFile.bOpen();
        }

        /// <summary>
        /// Record Debug log
        /// </summary>
        /// <param name="log"></param>
        public static void SetClassNameMaskKeyInfo(string p_strClassName,string p_strMaskKeyInfo)
        {
            if (!string.IsNullOrEmpty(p_strMaskKeyInfo) && !string.IsNullOrEmpty(p_strClassName))
            {
                HashMatchKeyInfo[p_strClassName] = p_strMaskKeyInfo.ToLowerInvariant();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_bFlag"></param>
        public static void SetIsAllEnableLog(bool p_bFlag)
        {
            // Whether or not record all the logs
            m_bEnableAllLog = p_bFlag;

            return;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_bFlag"></param>
        public static void SetIsEnableLog(bool p_bFlag)
        {
            // Whether or not mask partial log
            m_bEnableLog = p_bFlag;
        }
        
        /// <summary>
        /// Transaction log
        /// </summary>
        /// <param name="id"></param>
        /// <param name="log"></param>
        public static void WriteDebugLog(string id, string log, int level = 0, string hiberarchy = "AppControl|Application")
        {
//             if (m_bEnableLog || m_bEnableAllLog)
//             {
//                 ILog Logger4DevCtrl = LogManager.GetLogger4DevCtrl(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
//                 Logger4DevCtrl.DebugFormat("<{0}>: {1}", id, log);
//             }
            m_logFile.bWrite((LogLevel)level, hiberarchy, id, log);
        }

        /// <summary>
        /// 交易日志
        /// </summary>
        /// <param name="id"></param>
        /// <param name="log"></param>
        public static void WriteDebugLog(string id, Exception exp)
        {
//             if (m_bEnableLog || m_bEnableAllLog)
//             {
//                 ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
//                 logger.Info(id, exp);
//             }
            m_logFile.bWrite(GrgCDbLog.LogLevel.logFatal, "AppControl|Application", id, exp.Message +"\r\n\r\n\r\n"+ exp.StackTrace);
        }

        /// <summary>
        /// 记录双屏组件日志
        /// </summary>
        /// <param name="id"></param>
        /// <param name="log"></param>
        public static void WriteTwinInfoLog(string id, string log)
        {
//             if (m_bEnableLog || m_bEnableAllLog)
//             {
//                 ILog logger = LogManager.GetLogger("TWIN");
//                 logger.InfoFormat("<{0}>: {1}", id, log);
//             }
            m_logFile.bWrite("AppControl|Application", id, log);
        }

        /// <summary>
        /// Record the double screen component logs
        /// </summary>
        /// <param name="id"></param>
        /// <param name="log"></param>
        public static void WriteTwinInfoLog(string id, Exception exp)
        {
//             if (m_bEnableLog || m_bEnableAllLog)
//             {
//                 ILog logger = LogManager.GetLogger("TWIN");
//                 logger.Info(id, exp);
//             }
            m_logFile.bWrite("AppControl|Application", id, exp.Message + "\r\n\rn\r\n" + exp.StackTrace);
        }

        /// <summary>
        /// 记录扩展跟踪日志
        /// </summary>
        /// <param name="logger">日志名称</param>
        /// <param name="log">日志</param>
        public static void WriteExtendedInfoLog(string LoggerId, string log)
        {
//             if (m_bEnableLog || m_bEnableAllLog)
//             {
//                 ILog logger = LogManager.GetLogger(LoggerId);
//                 logger.Info(log);
//             }

            m_logFile.bWrite("AppControl|Application", LoggerId, log);
        }

        #region DSM logs

        public static bool MatchKeyInfoIsMask(string p_strClassName,string p_strInfo)
        {
            bool l_bMask = false;
            string l_strMask = (string)HashMatchKeyInfo[p_strClassName];
            if (!string.IsNullOrEmpty(l_strMask))
            {
                string[] strSubMask = l_strMask.Split('|');
                string l_strSrcInfo = p_strInfo.ToLowerInvariant();
                for (int i = 0; i < strSubMask.Length; i++)
                {
                    if (l_strSrcInfo.Contains(strSubMask[i]) || "all" == l_strMask)
                    {
                        l_bMask = true;
                    }
                }
            }
            return l_bMask;
        }

        public static void MethodLog(string id, string name, LogMethodTag tag, string format, params object[] param)
        {
            string classname = id;
            int pos = id.LastIndexOf(".");
            if (pos >= 0)
            {
                classname = id.Substring(pos + 1);
            }

            string customerMsg = "string format error!";
            try
            {
                customerMsg = string.Format(format, param);
            }
            catch (Exception)
            {
            }

            string logmsg = string.Format("{0} - (Method) - [{1}] - ({2}) - {3}", classname, name, tag.ToString(), customerMsg);
//             if (m_bEnableAllLog || (m_bEnableLog && !MatchKeyInfoIsMask(classname, logmsg)))
//             {
//                 ILog logger = LogManager.GetLogger("DEBUG");
//                 logger.Info(logmsg);
//             }
            m_logFile.bWrite("AppControl|Application", "MethodLog", logmsg);
        }


        public static void EventRecvLog(string id, string name, LogMethodTag tag, string format, params object[] param)
        {
            string classname = id;
            int pos = id.LastIndexOf(".");
            if (pos >= 0)
            {
                classname = id.Substring(pos + 1);
            }

            string customerMsg = "string format error!";
            try
            {
                customerMsg = string.Format(format, param);
            }
            catch (Exception)
            {
            }

            string logmsg = string.Format("{0} - (EventRecv) - [{1}] - ({2}) - {3}", classname, name, tag.ToString(), customerMsg);
//             if (m_bEnableAllLog || (m_bEnableLog && !MatchKeyInfoIsMask(classname, logmsg)))
//             {
//                 ILog logger = LogManager.GetLogger("DEBUG");
//                 logger.Info(logmsg);
//            }
            m_logFile.bWrite("AppControl|Application", "EventRecvLog", logmsg);
        }

        public static void EventSendLog(string id, string name, LogMethodTag tag, string format, params object[] param)
        {
            string classname = id;
            int pos = id.LastIndexOf(".");
            if (pos >= 0)
            {
                classname = id.Substring(pos + 1);
            }

            string customerMsg = "string format error!";
            try
            {
                customerMsg = string.Format(format, param);
            }
            catch (Exception)
            {
            }

            string logmsg = string.Format("{0} - (EventSend) - [{1}] - ({2}) - {3}", classname, name, tag.ToString(), customerMsg);
//             if (m_bEnableAllLog || (m_bEnableLog && !MatchKeyInfoIsMask(classname, logmsg)))
//             {
//                 ILog logger = LogManager.GetLogger("DEBUG");
//                 logger.Info(logmsg);
//            }
            m_logFile.bWrite("AppControl|Application", "EventSendLog", logmsg);
        }

        public static void PropertyLog(string id, string name, string format, params object[] param)
        {
            string classname = id;
            int pos = id.LastIndexOf(".");
            if (pos >= 0)
            {
                classname = id.Substring(pos + 1);
            }

            string customerMsg = "string format error!";
            try
            {
                customerMsg = string.Format(format, param);
            }
            catch (Exception)
            {
            }

            string logmsg = string.Format("{0} - (Property) - [{1}] - {2}", classname, name, customerMsg);
//             if (m_bEnableAllLog || (m_bEnableLog && !MatchKeyInfoIsMask(classname, logmsg)))
//             {
//                 ILog logger = LogManager.GetLogger("DEBUG");
//                 logger.Info(logmsg);
//            }
            m_logFile.bWrite("AppControl|Application", "PropertyLog", logmsg);
        }
        #endregion DSM Logs
    }

    public class Logger4DevCtrl
    {
        static bool m_bEnableLog = true;
        static bool m_bEnableAllLog = true;
        static Hashtable HashMatchKeyInfo = new Hashtable();

        static LogFile m_logFile;

        static Logger4DevCtrl()
        {
            m_logFile = new LogFile();
            m_logFile.bOpen();
        }

        /// <summary>
        /// 记录Debug日志
        /// </summary>
        /// <param name="log"></param>

        public static void SetClassNameMaskKeyInfo(string p_strClassName, string p_strMaskKeyInfo)
        {
            if (!string.IsNullOrEmpty(p_strMaskKeyInfo) && !string.IsNullOrEmpty(p_strClassName))
            {
                HashMatchKeyInfo[p_strClassName] = p_strMaskKeyInfo.ToLowerInvariant();
            }
        }

        public static void SetIsAllEnableLog(bool p_bFlag)
        {
            //是否记录所有的日志
            m_bEnableAllLog = p_bFlag;
        }

        public static void SetIsEnableLog(bool p_bFlag)
        {
            //是否屏蔽部分日志
            m_bEnableLog = p_bFlag;
        }

        /// <summary>
        /// 交易日志
        /// </summary>
        /// <param name="id"></param>
        /// <param name="log"></param>
        public static void WriteDebugLog(string id, string log, int level = 0, string hiberarchy = "DeviceControl")
        {
            //             if (m_bEnableLog || m_bEnableAllLog)
            //             {
            //                 ILog Logger4DevCtrl = LogManager.GetLogger4DevCtrl(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            //                 Logger4DevCtrl.DebugFormat("<{0}>: {1}", id, log);
            //             }
            m_logFile.bWrite((LogLevel)level, hiberarchy, id, log);
        }

        /// <summary>
        /// 交易日志
        /// </summary>
        /// <param name="id"></param>
        /// <param name="log"></param>
        public static void WriteDebugLog(string id, Exception exp, int level = 0, string hiberarchy = "DeviceControl")
        {
            //             if (m_bEnableLog || m_bEnableAllLog)
            //             {
            //                 ILog Logger4DevCtrl = LogManager.GetLogger4DevCtrl(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            //                 Logger4DevCtrl.Info(id, exp);
            //             }
            m_logFile.bWrite((LogLevel)level, hiberarchy, id, exp.Message + "\r\n\r\n\r\n" + exp.StackTrace);
        }

        /// <summary>
        /// 记录双屏组件日志
        /// </summary>
        /// <param name="id"></param>
        /// <param name="log"></param>
        public static void WriteTwinInfoLog(string id, string log, int level = 0, string hiberarchy = "DeviceControl")
        {
            //             if (m_bEnableLog || m_bEnableAllLog)
            //             {
            //                 ILog Logger4DevCtrl = LogManager.GetLogger4DevCtrl("TWIN");
            //                 Logger4DevCtrl.InfoFormat("<{0}>: {1}", id, log);
            //             }
            m_logFile.bWrite((LogLevel)level, hiberarchy, id, log);
        }

        /// <summary>
        /// 记录双屏组件日志
        /// </summary>
        /// <param name="id"></param>
        /// <param name="log"></param>
        public static void WriteTwinInfoLog(string id, Exception exp, int level = 0, string hiberarchy = "DeviceControl")
        {
            //             if (m_bEnableLog || m_bEnableAllLog)
            //             {
            //                 ILog Logger4DevCtrl = LogManager.GetLogger4DevCtrl("TWIN");
            //                 Logger4DevCtrl.Info(id, exp);
            //             }
            m_logFile.bWrite((LogLevel)level, hiberarchy, id, exp.Message + "\r\n\rn\r\n" + exp.StackTrace);
        }

        /// <summary>
        /// 记录扩展跟踪日志
        /// </summary>
        /// <param name="Logger4DevCtrl">日志名称</param>
        /// <param name="log">日志</param>
        public static void WriteExtendedInfoLog(string Logger4DevCtrlId, string log, int level = 0, string hiberarchy = "DeviceControl")
        {
            //             if (m_bEnableLog || m_bEnableAllLog)
            //             {
            //                 ILog Logger4DevCtrl = LogManager.GetLogger4DevCtrl(Logger4DevCtrlId);
            //                 Logger4DevCtrl.Info(log);
            //             }

            m_logFile.bWrite((LogLevel)level, hiberarchy, Logger4DevCtrlId, log);
        }

        #region DSM logs

        public static bool MatchKeyInfoIsMask(string p_strClassName, string p_strInfo)
        {
            bool l_bMask = false;
            string l_strMask = (string)HashMatchKeyInfo[p_strClassName];
            if (!string.IsNullOrEmpty(l_strMask))
            {
                string[] strSubMask = l_strMask.Split('|');
                string l_strSrcInfo = p_strInfo.ToLowerInvariant();
                for (int i = 0; i < strSubMask.Length; i++)
                {
                    if (l_strSrcInfo.Contains(strSubMask[i]) || "all" == l_strMask)
                    {
                        l_bMask = true;
                    }
                }
            }
            return l_bMask;
        }

        public static void LogDebug(string format, params object[] param)
        {

            string customerMsg = "string format error!";
            try
            {
                customerMsg = string.Format(format, param);
            }
            catch (Exception)
            {
            }

            m_logFile.bWrite("XXX", "Debug", customerMsg);
        }


        public static void LogError(string format, params object[] param)
        {
            string customerMsg = "string format error!";
            try
            {
                customerMsg = string.Format(format, param);
            }
            catch (Exception)
            {
            }

            m_logFile.bWrite("XXX", "Error", customerMsg);
        }


        public static void LogInfo(string format, params object[] param)
        {
            string customerMsg = "string format error!";
            try
            {
                customerMsg = string.Format(format, param);
            }
            catch (Exception)
            {
            }

            m_logFile.bWrite("XXX", "Info", customerMsg);
        }


        public static void LogWarn(string format, params object[] param)
        {
            string customerMsg = "string format error!";
            try
            {
                customerMsg = string.Format(format, param);
            }
            catch (Exception)
            {
            }

            m_logFile.bWrite("XXX", "Warn", customerMsg);
        }


        public static void LogDebugFormat(string format, params object[] param)
        {
            string customerMsg = "string format error!";
            try
            {
                customerMsg = string.Format(format, param);
            }
            catch (Exception)
            {
            }

            m_logFile.bWrite("XXX", "Debug", customerMsg);
        }


        public static void LogInfoFormat(string format, params object[] param)
        {
            string customerMsg = "string format error!";
            try
            {
                customerMsg = string.Format(format, param);
            }
            catch (Exception)
            {
            }

            m_logFile.bWrite("XXX", "Info", customerMsg);
        }


        public static void LogWarnFormat(string format, params object[] param)
        {
            string customerMsg = "string format error!";
            try
            {
                customerMsg = string.Format(format, param);
            }
            catch (Exception)
            {
            }

            m_logFile.bWrite("XXX", "Warn", customerMsg);
        }

        public static void LogErrorFormat(string format, params object[] param)
        {
            string customerMsg = "string format error!";
            try
            {
                customerMsg = string.Format(format, param);
            }
            catch (Exception)
            {
            }

            m_logFile.bWrite("XXX", "Warn", customerMsg);
        }

        public static void MethodLog(string id, string name, LogMethodTag tag, string format, params object[] param)
        {
            string classname = id;
            int pos = id.LastIndexOf(".");
            if (pos >= 0)
            {
                classname = id.Substring(pos + 1);
            }

            string customerMsg = "string format error!";
            try
            {
                customerMsg = string.Format(format, param);
            }
            catch (Exception)
            {
            }

            string logmsg = string.Format("(Method) - [{0}] - ({1}) - {2}", name, tag.ToString(), customerMsg);
            //             if (m_bEnableAllLog || (m_bEnableLog && !MatchKeyInfoIsMask(classname, logmsg)))
            //             {
            //                 ILog Logger4DevCtrl = LogManager.GetLogger4DevCtrl("DEBUG");
            //                 Logger4DevCtrl.Info(logmsg);
            //             }
            m_logFile.bWrite("DeviceControl", classname, logmsg);
        }


        public static void EventRecvLog(string id, string name, LogMethodTag tag, string format, params object[] param)
        {
            string classname = id;
            int pos = id.LastIndexOf(".");
            if (pos >= 0)
            {
                classname = id.Substring(pos + 1);
            }

            string customerMsg = "string format error!";
            try
            {
                customerMsg = string.Format(format, param);
            }
            catch (Exception)
            {
            }

            string logmsg = string.Format("(EventRecv) - [{0}] - ({1}) - {2}", name, tag.ToString(), customerMsg);
            //             if (m_bEnableAllLog || (m_bEnableLog && !MatchKeyInfoIsMask(classname, logmsg)))
            //             {
            //                 ILog Logger4DevCtrl = LogManager.GetLogger4DevCtrl("DEBUG");
            //                 Logger4DevCtrl.Info(logmsg);
            //            }
            m_logFile.bWrite("DeviceControl", classname, logmsg);
        }

        public static void EventSendLog(string id, string name, LogMethodTag tag, string format, params object[] param)
        {
            string classname = id;
            int pos = id.LastIndexOf(".");
            if (pos >= 0)
            {
                classname = id.Substring(pos + 1);
            }

            string customerMsg = "string format error!";
            try
            {
                customerMsg = string.Format(format, param);
            }
            catch (Exception)
            {
            }

            string logmsg = string.Format("(EventSend) - [{0}] - ({1}) - {2}", name, tag.ToString(), customerMsg);
            //             if (m_bEnableAllLog || (m_bEnableLog && !MatchKeyInfoIsMask(classname, logmsg)))
            //             {
            //                 ILog Logger4DevCtrl = LogManager.GetLogger4DevCtrl("DEBUG");
            //                 Logger4DevCtrl.Info(logmsg);
            //            }
            m_logFile.bWrite("DeviceControl", classname, logmsg);
        }

        public static void PropertyLog(string id, string name, string format, params object[] param)
        {
            string classname = id;
            int pos = id.LastIndexOf(".");
            if (pos >= 0)
            {
                classname = id.Substring(pos + 1);
            }

            string customerMsg = "string format error!";
            try
            {
                customerMsg = string.Format(format, param);
            }
            catch (Exception)
            {
            }

            string logmsg = string.Format("(Property) - [{0}] - {1}", name, customerMsg);
            //             if (m_bEnableAllLog || (m_bEnableLog && !MatchKeyInfoIsMask(classname, logmsg)))
            //             {
            //                 ILog Logger4DevCtrl = LogManager.GetLogger4DevCtrl("DEBUG");
            //                 Logger4DevCtrl.Info(logmsg);
            //            }
            m_logFile.bWrite("DeviceControl", classname, logmsg);
        }

        #endregion DSM Logs
    }

    public struct CAutoLogger : IDisposable
    {
        string m_strHiberarchy;
        string m_strModule;
        string m_strLog;

        public CAutoLogger(string p_strHib, string p_strModule, string p_string)
        {
            m_strHiberarchy = p_strHib;
            m_strModule = p_strModule;
            m_strLog = p_string;
            Logger.WriteDebugLog(m_strModule, "Enter " + m_strLog, 0, m_strHiberarchy);
        }

        public CAutoLogger(string p_strModule, string p_string)
        {
            m_strHiberarchy = "AppControl|Application";
            m_strModule = p_strModule;
            m_strLog = p_string;
            Logger.WriteDebugLog(m_strModule, "Enter " + m_strLog, 0, m_strHiberarchy);
        }

        public void Dispose()
        {
            Logger.WriteDebugLog(m_strModule, "Leave " + m_strLog, 0, m_strHiberarchy);
        }

        //~CAutoLogger()
        //{
        //}
    }
}