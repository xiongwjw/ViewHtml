using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net.Appender;

namespace LogProcessorService
{
    /// <summary>
    /// 日志密钥帮助类
    /// </summary>
    public static class LoggerKeyHelper
    {
        private static string _key;

        /// <summary>
        /// 加密LoggerAppender列表
        /// </summary>
        public static IEnumerable<string> EncryptLoggerAppenders { get; set; }

        /// <summary>
        /// 密钥(长度必须8位以上)
        /// </summary>
        public static string Key
        {
            get
            {
                if (string.IsNullOrEmpty(_key))
                {
                    _key = "1a2b3c4d";
                }
                return _key;
            }
            set
            {
                if (string.IsNullOrEmpty(value) || value.Length < 8)
                {
                    throw new Exception("The Key min length is 8.");
                }

                _key = value;
            }
        }
    }

    public class EncryptRollingFileAppender : RollingFileAppender
    {
        /// <summary>
        /// Sets the quiet writer being used.
        /// </summary>
        /// <remarks>
        /// This method can be overridden by sub classes.
        /// </remarks>
        /// <param name="writer">the writer to set</param>
        override protected void SetQWForFiles(TextWriter writer)
        {

            if (LoggerKeyHelper.EncryptLoggerAppenders == null ||
                LoggerKeyHelper.EncryptLoggerAppenders.All(t => !String.Equals(t, base.Name, StringComparison.CurrentCultureIgnoreCase))
            )
            {
                base.SetQWForFiles(writer);
            }
            else
            {
                QuietWriter = new EncryptCountingQuietTextWriter(
                    LoggerKeyHelper.Key,
                    writer,
                    ErrorHandler
                );
            }
        }
    }
}
