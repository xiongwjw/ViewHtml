using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace LogProcessorService
{
    public class GrgLoggerWrapper
    {
#region constructor
        private GrgLoggerWrapper( IGrgLogger iLogger )
        {
            Debug.Assert( null != iLogger );

            m_iLogger = iLogger;
        }
#endregion
        
#region function for creating
        public static GrgLoggerWrapper Create( string name )
        {
            Debug.Assert(!string.IsNullOrEmpty(name));

            return new GrgLoggerWrapper( Log4netWrapper.Create(name) );
        }
#endregion

#region method
        public void LogDebug( string message )
        {
            m_iLogger.Log(message, emLogLevel.Debug);
        }


        /// <summary>
        /// 写日志 (按刘暖琪要求新加的)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="argDate"></param>
        public void LogDebug(string message, bool argDate = true)
        {
            m_iLogger.Log(message, emLogLevel.Debug, argDate);
        }

        public void LogDebug( string message,
                              string key)
        {
            m_iLogger.Log(message, key, emLogLevel.Debug);
        }

        public void LogDebug( string message,
                              Exception exp)
        {
            m_iLogger.Log(message, emLogLevel.Debug, exp);
        }

        public void LogDebug( string message,
                              string key,
                              Exception exp)
        {
            m_iLogger.Log(message, key, emLogLevel.Debug, exp);
        }

        public void LogDebugFormat( string format,
                                    params object[] args )
        {
            m_iLogger.LogFormat(format, emLogLevel.Debug, args);
        }

        //public void LogDebugFormat( string format,
        //                            string key,
        //                            params object[] args)
        //{
        //    m_iLogger.LogFormat(format, key, emLogLevel.Debug, args);
        //}

        /// <summary>
        /// 写日志 (按刘暖琪要求新加的)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="argDate"></param>
        public void LogInfo(string message, bool argDate = true)
        {

            m_iLogger.Log(message, emLogLevel.Info, argDate);
        }

        public void LogInfo(string message)
        {
            m_iLogger.Log(message, emLogLevel.Info);
        }

        public void LogInfo(string message,
                              string key)
        {
            m_iLogger.Log(message, key, emLogLevel.Info);
        }

        public void LogInfo(string message,
                              Exception exp)
        {
            m_iLogger.Log(message, emLogLevel.Info, exp);
        }

        public void LogInfo(string message,
                              string key,
                              Exception exp)
        {
            m_iLogger.Log(message, key, emLogLevel.Info, exp);
        }

        public void LogInfoFormat(string format,
                                    params object[] args)
        {
            m_iLogger.LogFormat(format, emLogLevel.Info, args);
        }

        //public void LogInfoFormat(string format,
        //                            string key,
        //                            params object[] args)
        //{
        //    m_iLogger.LogFormat(format, key, emLogLevel.Info, args);
        //}

        public void LogWarn(string message)
        {
            m_iLogger.Log(message, emLogLevel.Warn);
        }

        public void LogWarn(string message,
                              string key)
        {
            m_iLogger.Log(message, key, emLogLevel.Warn);
        }

        public void LogWarn(string message,
                              Exception exp)
        {
            m_iLogger.Log(message, emLogLevel.Warn, exp);
        }

        public void LogWarn(string message,
                              string key,
                              Exception exp)
        {
            m_iLogger.Log(message, key, emLogLevel.Warn, exp);
        }

        public void LogWarnFormat( string format,
                                   params object[] args)
        {
            m_iLogger.LogFormat(format, emLogLevel.Warn, args);
        }

        //public void LogWarnFormat( string format,
        //                           string key,
        //                           params object[] args)
        //{
        //    m_iLogger.LogFormat(format, key, emLogLevel.Warn, args);
        //}

        public void LogError(string message)
        {
            m_iLogger.Log(message, emLogLevel.Error);
        }

        public void LogError(string message,
                              string key)
        {
            m_iLogger.Log(message, key, emLogLevel.Error);
        }

        public void LogError(string message,
                              Exception exp)
        {
            m_iLogger.Log(message, emLogLevel.Error, exp);
        }

        public void LogError(string message,
                              string key,
                              Exception exp)
        {
            m_iLogger.Log(message, key, emLogLevel.Error, exp);
        }

        public void LogErrorFormat(string format,
                                   params object[] args)
        {
            m_iLogger.LogFormat(format, emLogLevel.Error, args);
        }

        //public void LogErrorFormat(string format,
        //                           string key,
        //                           params object[] args)
        //{
        //    m_iLogger.LogFormat(format, key, emLogLevel.Error, args);
        //}

        public void LogFatal(string message)
        {
            m_iLogger.Log(message, emLogLevel.Fatal);
        }

        public void LogFatal(string message,
                              string key)
        {
            m_iLogger.Log(message, key, emLogLevel.Fatal);
        }

        public void LogFatal(string message,
                              Exception exp)
        {
            m_iLogger.Log(message, emLogLevel.Fatal, exp);
        }

        public void LogFatal(string message,
                              string key,
                              Exception exp)
        {
            m_iLogger.Log(message, key, emLogLevel.Fatal, exp);
        }

        public void LogFatalFormat(string format,
                                   params object[] args)
        {
            m_iLogger.LogFormat(format, emLogLevel.Fatal, args);
        }

        public void LogHex( byte[] arrBuffer,
                            int argSize )
        {
            m_iLogger.LogHex(arrBuffer, argSize);
        }

        public void LogHexFormat( string argFormat,
                                  byte[] arrBuffer,
                                  int argSize )
        {
            m_iLogger.LogHexFormat(  argFormat, arrBuffer, argSize );
        }
        //public void LogFatalFormat(string format,
        //                           string key,
        //                           params object[] args)
        //{
        //    m_iLogger.LogFormat(format, key, emLogLevel.Fatal, args);
        //}
#endregion

#region field
        private IGrgLogger m_iLogger = null; 
#endregion
    }
}
