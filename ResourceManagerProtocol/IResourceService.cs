/********************************************************************
	FileName:   IResource
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
    public interface IResourceService
    {
#region method
        bool LoadString( string argKey,
                         string argCategory,
                         out string argResult );

        bool LoadString( string argKey,
                         out string argResult );

        string LoadString(string argkey, 
                          string argCategory = null);

        bool QueryVoicePath( string argKey,
                             out string argResult );

        bool QueryImagePath( string argKey,
                             out string argResult );

        bool QueryVideoPath( string argKey,
                             out string argResult );
#endregion

#region property
        string Language
        {
            get;
        }
#endregion
    }
}
