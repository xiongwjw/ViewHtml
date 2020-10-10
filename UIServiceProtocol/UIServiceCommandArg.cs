/********************************************************************
	FileName:   UIServiceCommandArg
    purpose:	

	author:		huang wei
	created:	2012/12/29

    revised history:
	2012/12/29  

================================================================
    Copyright (C) 2012, Grgbanking CO,. Ltd. All rights reserved.
================================================================
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace UIServiceProtocol
{
    public enum ImpersonatedType
    {
        Button = 0
    };

    public class HotAreaParam
    {
        /// <summary>
        /// 
        /// </summary>
        public float X
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public float Y
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public float Width
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public float Height
        {
            get;
            set;
        }
        /// <summary>
        /// Impersonated type of a hot area
        /// </summary>
        public ImpersonatedType Type
        {
            get;
            set;
        }
        /// <summary>
        /// Key of a hot area
        /// </summary>
        public object Key
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        public bool IsSameLocation( HotAreaParam argParam )
        {
            Debug.Assert(null != argParam);

            if ( argParam.X == this.X &&
                 argParam.Y == this.Y &&
                 argParam.Width == this.Width &&
                 argParam.Height == this.Height )
            {
                return true;
            }

            return false;
        }

        public bool HitTest( float argX,
                             float argY)
        {
            if ( X <= argX &&
                 Y <= argY &&
                 argX <= (X + Width) &&
                 argY <= (Y + Height) )
            {
                return true;
            }

            return false;
        }

        public bool HitTest(int argX,
                           int argY)
        {
            if (X <= argX &&
                 Y <= argY &&
                 argX <= (X + Width) &&
                 argY <= (Y + Height))
            {
                return true;
            }

            return false;
        }
    }

    public class HotAreaParamArray
    {
        /// <summary>
        /// a array to contain all hot areas.
        /// </summary>
        public HotAreaParam[] HotAreas
        {
            get;
            set;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class AddElementScriptParam
    {
        public string ElementScript
        {
            get;
            set;
        }
    }
}
