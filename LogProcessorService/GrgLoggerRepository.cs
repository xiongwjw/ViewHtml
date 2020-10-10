using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace LogProcessorService
{
    public static class Log
    {
#region property
        public static GrgLoggerWrapper Root
        {
            get
            {
                if ( null == s_rootLogger )
                {
                    s_rootLogger = GrgLoggerWrapper.Create("Root");
                }

                return s_rootLogger;
            }
        }

        public static GrgLoggerWrapper Kernel
        {
            get
            {
                if ( null == s_kernelLogger )
                {
                    s_kernelLogger = GrgLoggerWrapper.Create("Kernel");
                }

                return s_kernelLogger;
            }
        }

        public static GrgLoggerWrapper Action
        {
            get
            {
                if ( null == s_actionLogger )
                {
                    s_actionLogger = GrgLoggerWrapper.Create("Action");
                }

                return s_actionLogger;
            }
        }

        public static GrgLoggerWrapper MaintenanceAction
        {
            get
            {
                if (null == s_maintenanceActionLogger)
                {
                    s_maintenanceActionLogger = GrgLoggerWrapper.Create("MaintenanceAction");
                }

                return s_maintenanceActionLogger;
            }
        }

        public static GrgLoggerWrapper Workflow
        {
            get
            {
                if ( null == s_workflowLogger )
                {
                    s_workflowLogger = GrgLoggerWrapper.Create("Workflow");
                }

                return s_workflowLogger;
            }
        }

        public static GrgLoggerWrapper BusinessService
        {
            get
            {
                if ( null == s_businessServiceLogger )
                {
                    s_businessServiceLogger = GrgLoggerWrapper.Create("BusinessService");
                }

                return s_businessServiceLogger;
            }
        }

        public static GrgLoggerWrapper UIService
        {
            get
            {
                if (null == s_UILogger)
                {
                    s_UILogger = GrgLoggerWrapper.Create("UIService");
                }

                return s_UILogger;
            }
        }

        public static GrgLoggerWrapper Engine
        {
            get
            {
                if ( null == s_engineLogger )
                {
                    s_engineLogger = GrgLoggerWrapper.Create("Engine");
                }

                return s_engineLogger;
            }
        }

        public static GrgLoggerWrapper Device
        {
            get
            {
                if ( null == s_deviceLogger )
                {
                    s_deviceLogger = GrgLoggerWrapper.Create("Device");
                }

                return s_deviceLogger;
            }
        }

        public static GrgLoggerWrapper NetworkLogger
        {
            get
            {
                if ( null == s_networkLogger )
                {
                    s_networkLogger = GrgLoggerWrapper.Create("Network");
                }

                return s_networkLogger;
            }
        }

        public static GrgLoggerWrapper ISO8583
        {
            get
            {
                if ( null == s_iso8583Logger )
                {
                    s_iso8583Logger = GrgLoggerWrapper.Create("ISO8583");
                }

                return s_iso8583Logger;
            }
        }

        public static GrgLoggerWrapper HouseKeeper
        {
            get
            {
                if ( null == s_houseKeeperLogger )
                {
                    s_houseKeeperLogger = GrgLoggerWrapper.Create("HouseKeeper");
                }

                return s_houseKeeperLogger;
            }
        }

        public static GrgLoggerWrapper ElectricJournal
        {
            get
            {
                if (null == s_electricJournalLogger)
                {
                    s_electricJournalLogger = GrgLoggerWrapper.Create("ElectricJournal");
                }

                return s_electricJournalLogger;
            }
        }

        public static GrgLoggerWrapper XDCScreenParser
        {
            get
            {
                if ( null == s_xdcScreenParserLog )
                {
                    s_xdcScreenParserLog = GrgLoggerWrapper.Create("XDCScreenParser");
                }

                return s_xdcScreenParserLog;
            }
        }

        /// <summary>
        /// The definition of the XDC Electronic Log object
        /// </summary>
        public static GrgLoggerWrapper XdcEJ
        {
            get
            {
                if (null == s_xdcEJLogger)
                {
                    s_xdcEJLogger = GrgLoggerWrapper.Create("XdcTrace.XdcEJ");
                }

                return s_xdcEJLogger;
            }
        }

        /// <summary>
        /// The definition of the XDC Trace Log object
        /// </summary>
        public static GrgLoggerWrapper XdcTrace
        {
            get
            {
                if (null == s_xdcTraceLogger)
                {
                    s_xdcTraceLogger = GrgLoggerWrapper.Create("XdcTrace");
                }

                return s_xdcTraceLogger;
            }
        }
        /// <summary>
        /// The definition of the XDC Device Status log object
        /// </summary>
        public static GrgLoggerWrapper XdcDeviceStatus
        {
            get
            {
                if (null == s_xdcDeviceStatusLogger)
                {
                    s_xdcDeviceStatusLogger = GrgLoggerWrapper.Create("XdcDeviceStatus");
                }

                return s_xdcDeviceStatusLogger;
            }
        }
        public static GrgLoggerWrapper DataGateway
        {
            get
            {
                if (null == s_dataGatewayLogger)
                {
                    s_dataGatewayLogger = GrgLoggerWrapper.Create("BusinessService.DataGateway");
                }

                return s_dataGatewayLogger;
            }
        }

        public static GrgLoggerWrapper ResourceManager
        {
            get
            {
                if ( null == s_resourceManager )
                {
                    s_resourceManager = GrgLoggerWrapper.Create("ResourceManager");
                }
                return s_resourceManager;
            }
        }

        public static GrgLoggerWrapper COMLog
        {
            get
            {
                if (null == s_comLog)
                {
                    s_comLog = GrgLoggerWrapper.Create("COMLog");
                }

                return s_comLog;
            }
        }

        public static GrgLoggerWrapper FileTransfer
        {
            get
            {
                if ( null == s_fileTransfer )
                {
                    s_fileTransfer = GrgLoggerWrapper.Create("FileTransfer");
                }

                return s_fileTransfer;
            }
        }

        public static GrgLoggerWrapper WCFServiceHost
        {
            get
            {
                if ( null == s_wcfServiceHost )
                {
                    s_wcfServiceHost = GrgLoggerWrapper.Create("wcfServiceHost");
                }

                return s_wcfServiceHost;
            }
        }

        public static GrgLoggerWrapper Project
        {
            get
            {
                if (null == s_projectLogger)
                {
                    s_projectLogger = GrgLoggerWrapper.Create("Project");
                }

                return s_projectLogger;
            }
        }

        public static GrgLoggerWrapper DataCache
        {
            get
            {
                if ( null == s_dataCacheLogger )
                {
                    s_dataCacheLogger = GrgLoggerWrapper.Create("DataCache");
                }

                return s_dataCacheLogger;
            }
        }

        public static GrgLoggerWrapper Validation
        {
            get
            {
                if ( null == s_validationLogger )
                {
                    s_validationLogger = GrgLoggerWrapper.Create("Validation");
                }

                return s_validationLogger;
            }
        }

        public static GrgLoggerWrapper Keeper
        {
            get
            {
                if (null == s_keeperLogger)
                {
                    s_keeperLogger = GrgLoggerWrapper.Create("Keeper");
                }

                return s_keeperLogger;
            }
        }
 #endregion

#region field
        private static GrgLoggerWrapper s_rootLogger = null;

        private static GrgLoggerWrapper s_kernelLogger = null;

        private static GrgLoggerWrapper s_actionLogger = null;

        private static GrgLoggerWrapper s_maintenanceActionLogger = null;

        private static GrgLoggerWrapper s_businessServiceLogger = null;

        private static GrgLoggerWrapper s_engineLogger = null;

        private static GrgLoggerWrapper s_workflowLogger = null;

        private static GrgLoggerWrapper s_deviceLogger = null;

        private static GrgLoggerWrapper s_networkLogger = null;

        private static GrgLoggerWrapper s_UILogger = null;

        private static GrgLoggerWrapper s_iso8583Logger = null;

        private static GrgLoggerWrapper s_houseKeeperLogger = null;

        private static GrgLoggerWrapper s_electricJournalLogger = null;

        private static GrgLoggerWrapper s_xdcScreenParserLog = null;

        private static GrgLoggerWrapper s_xdcEJLogger = null;

        private static GrgLoggerWrapper s_xdcTraceLogger = null;

        private static GrgLoggerWrapper s_xdcDeviceStatusLogger = null;

        private static GrgLoggerWrapper s_dataGatewayLogger = null;

        private static GrgLoggerWrapper s_resourceManager = null;

        private static GrgLoggerWrapper s_comLog = null;

        private static GrgLoggerWrapper s_fileTransfer = null;

        private static GrgLoggerWrapper s_wcfServiceHost = null;

        private static GrgLoggerWrapper s_projectLogger = null;

        private static GrgLoggerWrapper s_dataCacheLogger = null;

        private static GrgLoggerWrapper s_validationLogger = null;

        private static GrgLoggerWrapper s_keeperLogger = null;
#endregion
    }

    public struct AutoLoggerGuard : IDisposable
    {
#region constructor
        public AutoLoggerGuard( GrgLoggerWrapper argLogger,
                                string argScope)
        {
            Debug.Assert( null != argLogger );
 
            m_logger = argLogger;
            m_scope = argScope;

            m_logger.LogDebugFormat("Enter {0}", m_scope);
        }
#endregion

#region method
        void IDisposable.Dispose()
        {
            Debug.Assert(null != m_logger);

            m_logger.LogDebugFormat("Leave {0}", m_scope);
        }
#endregion

#region field
        private string m_scope;

        private GrgLoggerWrapper m_logger; 
#endregion
    }
}
