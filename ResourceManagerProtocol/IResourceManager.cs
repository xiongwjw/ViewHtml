/********************************************************************
	FileName:   IResourceManager
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

namespace ResourceManagerProtocol
{
    //public delegate bool EnumResourceHandler( IResourceService argResource,
    //                                          object argParam );

    public delegate void LanguageChangedHandler( string argOld,
                                                 string argNew,
                                                 bool argIsUI );

    public interface IResourceManager
    {
#region method
        bool Open( string argConfig );

        void Close();

        bool QueryResource( string argLanguage,
                            out IResourceService argResource );

        //void EnumResource( EnumResourceHandler argHandler,
        //                   object argParam );

        bool Refresh();
#endregion

#region event
        event LanguageChangedHandler languageChanged;
#endregion

#region property
        string CurrentUILanguage
        {
            get;
            set;
        }

        string CurrentSystemLanguage
        {
            get;
            set;
        }

        string CurrentJPTRLanguage
        {
            get;
            set;
        }

        string CurrentRPTRLanguage
        {
            get;
            set;
        }

        string CurrentMaintenanceLanguage
        {
            get;
            set;
        }

        IResourceService CurrentUIResource
        {
            get;
        }

        IResourceService CurrentSystemResource
        {
            get;
        }

        IResourceService CurrentJPTRResource
        {
            get;
        }

        IResourceService CurrentRPTRResource
        {
            get;
        }

        IResourceService CurrentMaintenaceResource
        {
            get;
        }
#endregion
    }
}
