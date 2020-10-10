/********************************************************************
	FileName:   ResourceManagerImp
    purpose:	

	author:		huang wei
	created:	2013/01/15

    revised history:
	2013/01/15  

================================================================
    Copyright (C) 2013, Grgbanking CO,. Ltd. All rights reserved.
================================================================
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResourceManagerProtocol;
using Attribute4ECAT;
using System.Xml;
using System.IO;
using System.Diagnostics;
using LogProcessorService;

namespace ResourceManager
{
    [GrgComponent("{AFE5005B-3C92-4954-97F0-17B4C62906FD}",
                   Name="ResourceManager",
                   Catalog="GeneralService",
                   Author="huang wei" )]
    public class ResourceManagerImp : IResourceManager
    {
#region constructor
        private ResourceManagerImp()
        {

        }

        static ResourceManagerImp()
        {
            s_manager = new ResourceManagerImp();
        }
#endregion

#region Creating function
        [GrgCreateFunction("Create")]
        public static IResourceManager Create()
        {
            return s_manager as IResourceManager;
        }
#endregion

#region methods of the IResourceManager interface
        bool IResourceManager.Open(string argConfig)
        {
            Debug.Assert( !string.IsNullOrEmpty(argConfig) );
            if ( string.IsNullOrEmpty(argConfig) )
            {
                throw new ArgumentNullException("argConfig");
            }
            Log.ResourceManager.LogDebug("Prepare for opening the resource manager");
            if ( m_isOpen )
            {
                Log.ResourceManager.LogInfo("The resource manager has been open");
                return true;
            }

            try
            {
                m_config = new XmlDocument();
                m_config.Load(argConfig);
                m_configFilePath = argConfig;

                //load host attribute
                m_host = s_localhost;
                XmlAttribute hostAttri = m_config.DocumentElement.Attributes[s_hostAttri];
                if ( null != hostAttri &&
                     !string.IsNullOrEmpty(hostAttri.Value) )
                {
                    m_host = hostAttri.Value;
                }
                if ( m_host.Equals( s_localhost, StringComparison.OrdinalIgnoreCase ) )
                {
                    m_isRemote = false;
                }
                else
                {
                    m_isRemote = true;
                }

                //load base attribute
                string basePath = null;
                XmlAttribute basePathAttri = m_config.DocumentElement.Attributes[s_basePathAttri];
                if ( null != basePathAttri &&
                     !string.IsNullOrEmpty(basePathAttri.Value) )
                {
                    basePath = basePathAttri.Value;
                }
                if ( m_isRemote )
                {
                    m_basePath = m_host + basePath;
                }
                else
                {
                    m_basePath = AppDomain.CurrentDomain.BaseDirectory + basePath;
                }

                //load language attribute
                XmlAttribute langAttri = m_config.DocumentElement.Attributes[s_languageAttri];
                if ( null != langAttri &&
                     !string.IsNullOrEmpty(langAttri.Value) )
                {
                    string[] arrLanguages = langAttri.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    m_listLanguages = arrLanguages.ToList();
                }
                else
                {
                    m_listLanguages = new List<string>();
                    m_listLanguages.Add("cn");
                }
            }
            catch (System.Exception ex)
            {
                Log.ResourceManager.LogError("Failed to open the resource manager", ex);
                return false;
            }
            m_isOpen = true;

            return true;
        }

        void IResourceManager.Close()
        {
            if ( !m_isOpen )
            {
                return;
            }
            m_config.RemoveAll();
            m_config = null;
            m_listLanguages.Clear();
            foreach ( var item in m_dicResourceService )
            {
                item.Value.Close();
            }
            m_dicResourceService.Clear();
        }

        bool IResourceManager.QueryResource( string argLanguage,
                                             out IResourceService argResource )
        {
            argResource = null;
            if ( string.IsNullOrEmpty( argLanguage ) )
            {
                throw new ArgumentNullException( "argLanguage" );
            }

            string temp = argLanguage.ToLowerInvariant();
            ResourceServiceImp resourceImp = null;
            if ( m_dicResourceService.TryGetValue( temp, out resourceImp ) )
            {
                argResource = resourceImp as IResourceService;
            }
            else
            {
                if (m_listLanguages.Exists((item) => item.Equals(temp, StringComparison.OrdinalIgnoreCase) ))
                {
                    ResourceServiceImp imp = new ResourceServiceImp( temp );
                    if ( imp.Open( m_config.DocumentElement,
                                   m_basePath,
                                   m_isRemote ) )
                    {
                        m_dicResourceService.Add(temp, imp);
                        argResource = imp as IResourceService;
                    }
                    else
                    {
                        imp = null;
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     
        //void IResourceManager.EnumResource( EnumResourceHandler argHandler,
        //                                    object argParam)
        //{

        //}

        bool IResourceManager.Refresh()
        {
            IResourceManager iManager = this as IResourceManager;
            iManager.Close();
            return iManager.Open( m_configFilePath );
        }
#endregion

#region property
        string IResourceManager.CurrentSystemLanguage
        {
            get
            {
                return m_currentSystemLanguage;
            }
            set
            {
                if ( string.IsNullOrEmpty(value) )
                {
                    throw new ArgumentNullException("The language cannot empty");
                }
                if ( !string.IsNullOrEmpty(m_currentSystemLanguage) &&
                     m_currentSystemLanguage.Equals(value, StringComparison.OrdinalIgnoreCase) )
                {
                    return;
                }

                IResourceManager iManager = this as IResourceManager;
                IResourceService iService = null;
                if ( iManager.QueryResource( value, out iService ) )
                {
                    OnLanguageChanged(m_currentSystemLanguage, value, false);
                    m_currentSystemLanguage = value;
                    m_iCurrentSystemResourceService = iService;
                }
                else
                {
                    throw new Exception("The language isn't exist");
                }
            }
        }

        string IResourceManager.CurrentUILanguage
        {
            get
            {
                return m_currentUILanguage;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("The language cannot empty");
                }
                if ( !string.IsNullOrEmpty(m_currentUILanguage) &&
                     m_currentUILanguage.Equals(value, StringComparison.OrdinalIgnoreCase) )
                {
                    return;
                }

                IResourceManager iManager = this as IResourceManager;
                IResourceService iService = null;
                if (iManager.QueryResource(value, out iService))
                {
                    m_currentUILanguage = value;
                    m_iCurrentUIResourceService = iService;
                    OnLanguageChanged( m_currentUILanguage, value, true);
                }
                else
                {
                    throw new Exception("The language isn't exist");
                }
            }
        }

        string IResourceManager.CurrentJPTRLanguage
        {
            get
            {
                return m_currentJPTRLanguage;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("The language cannot empty");
                }
                if (!string.IsNullOrEmpty(m_currentJPTRLanguage) &&
                     m_currentJPTRLanguage.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                IResourceManager iManager = this as IResourceManager;
                IResourceService iService = null;
                if (iManager.QueryResource(value, out iService))
                {
                    m_currentJPTRLanguage = value;
                    m_iCurrentJPTRResourceService = iService;
                    OnLanguageChanged(m_currentJPTRLanguage, value, false);
                }
                else
                {
                    throw new Exception("The language isn't exist");
                }
            }
        }

        string IResourceManager.CurrentRPTRLanguage
        {
            get
            {
                return m_currentRPTRLanguage;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("The language cannot empty");
                }
                if (!string.IsNullOrEmpty(m_currentRPTRLanguage) &&
                     m_currentRPTRLanguage.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                IResourceManager iManager = this as IResourceManager;
                IResourceService iService = null;
                if (iManager.QueryResource(value, out iService))
                {
                    m_currentRPTRLanguage = value;
                    m_iCurrentRPTRResourceService = iService;
                    OnLanguageChanged(m_currentRPTRLanguage, value, false);

                }
                else
                {
                    throw new Exception("The language isn't exist");
                }
            }
        }

        string IResourceManager.CurrentMaintenanceLanguage
        {
            get
            {
                return m_currentMaintenanceLanguage;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("The language cannot empty");
                }
                if (!string.IsNullOrEmpty(m_currentMaintenanceLanguage) &&
                     m_currentMaintenanceLanguage.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                IResourceManager iManager = this as IResourceManager;
                IResourceService iService = null;
                if (iManager.QueryResource(value, out iService))
                {
                    m_currentMaintenanceLanguage = value;
                    m_iCurrentMaintenanceService = iService;
                    OnLanguageChanged(m_currentMaintenanceLanguage, value, false);

                }
                else
                {
                    throw new Exception("The language isn't exist");
                }
            }
        }

        IResourceService IResourceManager.CurrentUIResource
        {
            get
            {
                return m_iCurrentUIResourceService;
            }
        }

        IResourceService IResourceManager.CurrentSystemResource
        {
            get
            {
                return m_iCurrentSystemResourceService;
            }
        }

        IResourceService IResourceManager.CurrentJPTRResource
        {
            get
            {
                return m_iCurrentJPTRResourceService;
            }
        }

        IResourceService IResourceManager.CurrentMaintenaceResource
        {
            get
            {
                return m_iCurrentMaintenanceService;
            }
        }

        IResourceService IResourceManager.CurrentRPTRResource
        {
            get
            {
                return m_iCurrentRPTRResourceService;
            }
        }
#endregion

#region event
        public event LanguageChangedHandler languageChanged;

        protected void OnLanguageChanged( string argOld,
                                          string argNew,
                                          bool argIsUI )
        {
            if ( null != languageChanged )
            {
                languageChanged.Invoke( argOld, 
                                        argNew, 
                                        argIsUI );
            }
        }
#endregion

#region field
        private static ResourceManagerImp s_manager = null;

        private bool m_isOpen = false;

        private XmlDocument m_config = null;

        private string m_configFilePath = null;

        private string m_basePath = null;

        private List<string> m_listLanguages = null;

        private Dictionary<string, ResourceServiceImp> m_dicResourceService = new Dictionary<string, ResourceServiceImp>();

        private string m_host = null;

        private string m_currentSystemLanguage = null;

        private string m_currentUILanguage = null;

        private string m_currentJPTRLanguage = null;

        private string m_currentRPTRLanguage = null;

        private string m_currentMaintenanceLanguage = null;

        private IResourceService m_iCurrentUIResourceService = null;

        private IResourceService m_iCurrentSystemResourceService = null;

        private IResourceService m_iCurrentJPTRResourceService = null;

        private IResourceService m_iCurrentRPTRResourceService = null;

        private IResourceService m_iCurrentMaintenanceService = null;

        private bool m_isRemote = false;

        public const string s_basePathAttri = "basePath";

        public const string s_languageAttri = "language";

        public const string s_hostAttri = "host";

        public const string s_localhost = "localhost";
#endregion
    }
}
