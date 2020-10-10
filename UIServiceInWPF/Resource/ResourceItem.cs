/********************************************************************
	FileName:   ResourceItem
    purpose:	

	author:		huang wei
	created:	2013/03/29

    revised history:
	2013/03/29  

================================================================
    Copyright (C) 2013, Grgbanking CO,. Ltd. All rights reserved.
================================================================
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using LogProcessorService;

namespace UIServiceInWPF.Resource
{
    public class ResourceItem
    {
#region constructor
        public ResourceItem( string argName )
        {
            Debug.Assert(!string.IsNullOrEmpty(argName));
            m_name = argName;
        }
#endregion

#region method
        public virtual bool Open( System.Windows.Forms.HtmlElement argElement )
        {
            Debug.Assert(null != argElement);

            m_name = argElement.Id;

            return true;
        }

        public virtual void Close()
        {

        }
#endregion

#region property
        public string Name
        {
            get
            {
                return m_name;
            }
        }
#endregion

#region field
        protected string m_name;
#endregion
    }
}
