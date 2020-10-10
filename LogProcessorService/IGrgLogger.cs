using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogProcessorService
{
    [Flags]
    public enum emLogLevel
    {
        Debug = 0,
        Info,
        Warn,
        Error,
        Fatal
    }

    [Flags]
    public enum LogMethodTag
    {
        Begin,
        End
    }

    public interface IGrgLogger
    {
#region method
        void Log(string message, emLogLevel level);

        /// <summary>
        /// 写日志 (按刘暖琪要求新加的)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="level"></param>
        /// <param name="bDate"></param>
        void Log(string message, emLogLevel level, bool bDate = true);

        void Log(string message, emLogLevel level, Exception exp);

        /// <summary>
        /// 写日志 (按刘暖琪要求新加的)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="level"></param>
        /// <param name="exp"></param>
        /// <param name="bDate"></param>
        void Log(string message, emLogLevel level, Exception exp, bool bDate = true);

        void Log(string message, string key, emLogLevel level);

        /// <summary>
        /// 写日志 (按刘暖琪要求新加的)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="key"></param>
        /// <param name="level"></param>
        /// <param name="bDate"></param>
        void Log(string message, string key, emLogLevel level, bool bDate = true);

        void Log(string message, string key, emLogLevel level, Exception exp);

        /// <summary>
        /// 写日志 (按刘暖琪要求新加的)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="key"></param>
        /// <param name="level"></param>
        /// <param name="exp"></param>
        /// <param name="bDate"></param>
        void Log(string message, string key, emLogLevel level, Exception exp, bool bDate = true);

        void LogHex(byte[] arrBuffer, int argSize);

        void LogHexFormat(string argFormat, byte[] arrBuffer, int argSize);

        void LogFormat(string format, emLogLevel level, params object[] args);

        void LogFormat(string format, string key, emLogLevel level, params object[] args);
#endregion

    }
}
