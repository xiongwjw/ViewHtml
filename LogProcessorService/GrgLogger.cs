/********************************************************************
	FileName:   GrgLogger
    purpose:	

	author:		huang wei
	created:	2012/11/06

    revised history:
	2012/11/06  

================================================================
    Copyright (C) 2012, Grgbanking CO,. Ltd. All rights reserved.
================================================================
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using GrgCDbLog;
using System.Xml;
using HelperService;

namespace LogProcessorService
{
    class GrgLogger : IGrgLogger
    {
#region constructor
        private GrgLogger( string name )
        {
            Debug.Assert( !string.IsNullOrEmpty(name) );

            m_loggerName = name;
        }

        static GrgLogger()
        {
            //
            try
            {
                string strPath = AppDomain.CurrentDomain.BaseDirectory + @"Config\LogConfig.xml";
                s_LogFile = new LogFile3();
                bool bRet = s_LogFile.Open(strPath);
                if ( !bRet )
                {
                    throw new Exception("GRGCbLog Error!");
                }

                s_showHex = true;

                XmlDocument doc = new XmlDocument();
                doc.Load(strPath);
                XmlAttribute attri = doc.DocumentElement.Attributes["showHex"];
                if ( null != attri &&
                     !string.IsNullOrEmpty( attri.Value ) )
                {
                    int value = 0;
                    if ( int.TryParse( attri.Value, out value ) )
                    {
                        s_showHex = value == 0 ? false : true;
                    }
                }
                doc.RemoveAll();
                doc = null;
            }
            catch (System.Exception ex)
            {
                Log4netWrapper.Create("root").Log("Failed to open the log file", emLogLevel.Fatal, ex);
                s_LogFile = null;
            }
        }
#endregion

#region functin to create a logger
        public static IGrgLogger Create( string name )
        {
            Debug.Assert(!string.IsNullOrEmpty(name));
            return new GrgLogger(name) as IGrgLogger;
        }
#endregion

#region methods of the IGrgLogger interface
        void IGrgLogger.Log( string message, 
                             emLogLevel level )
        {
            if ( null == s_LogFile )
            {
                return;
            }

            s_LogFile.Write(m_loggerName, Convert(level), null, message);
        }

        void IGrgLogger.LogHex( byte[] argarrBuffer,
                                int argSize)
        {
            if (null == s_LogFile)
            {
                return;
            }

            s_LogFile.Write(m_loggerName, LogLevel.logInfo, null, s_showHex ? ConvertHelper.HexToString(argarrBuffer, argSize) : ASCIIEncoding.ASCII.GetString(argarrBuffer));
        }

        void IGrgLogger.LogHexFormat( string argFormat,
                                      byte[] argarrBuffer,
                                      int argSize)
        {
            if ( null == s_LogFile )
            {
                return;
            }

            string msg = null;
            if ( s_showHex )
            {
                msg = ConvertHelper.HexToString(argarrBuffer, argSize);
            }
            else
            {
                msg = ASCIIEncoding.ASCII.GetString(argarrBuffer);
            }

            s_LogFile.Write( m_loggerName, LogLevel.logInfo, null, string.IsNullOrEmpty(argFormat) ? msg : string.Format( argFormat, msg ) );
        }

        void IGrgLogger.Log( string message, 
                             emLogLevel level,
                             Exception exp)
        {
            if ( null == s_LogFile )
            {
                return;
            }

            s_LogFile.Write(m_loggerName, Convert(level), null, exp.Message + "\r\n\r\n\r\n" + exp.StackTrace);
        }

        void IGrgLogger.Log( string message, 
                             string key, 
                             emLogLevel level )
        {
            if ( null == s_LogFile )
            {
                return;
            }

            s_LogFile.Write( m_loggerName, Convert(level), key, message );
        }

        void IGrgLogger.Log( string message, 
                             string key, 
                             emLogLevel level, 
                             Exception exp )
        {
            if ( null == s_LogFile )
            {
                return;
            }

            s_LogFile.Write(m_loggerName, Convert(level), key, exp.Message + "\r\n\r\n\r\n" + exp.StackTrace);
        }

        void IGrgLogger.LogFormat( string format, 
                                   emLogLevel level, 
                                   params object[] args )
        {
            if ( null == s_LogFile )
            {
                return;
            }

            s_LogFile.Write(m_loggerName, Convert(level), null, string.Format(format, args));
        }

        void IGrgLogger.LogFormat( string format, 
                                   string key, 
                                   emLogLevel level, 
                                   params object[] args )
        {
            if ( null == s_LogFile )
            {
                return;
            }

            s_LogFile.Write( m_loggerName, Convert(level), key, string.Format(format, args) );
        }
#endregion

#region method
        private LogLevel Convert( emLogLevel argLevel )
        {
            LogLevel level = LogLevel.logDebug;
            switch ( argLevel )
            {
                case emLogLevel.Debug:
                    {
                        level = LogLevel.logDebug;
                    }
                    break;

                case emLogLevel.Info:
                    {
                        level = LogLevel.logInfo;
                    }
                    break;

                case emLogLevel.Warn:
                    {
                        level = LogLevel.logWarning;
                    }
                    break;

                case emLogLevel.Error:
                    {
                        level = LogLevel.logError;
                    }
                    break;

                case emLogLevel.Fatal:
                    {
                        level = LogLevel.logFatal;
                    }
                    break;
            }

            return level;
        }
#endregion

#region field
        private string m_loggerName = null;

        private static LogFile3 s_LogFile = null;

        private static bool s_showHex;
#endregion
    }
}
