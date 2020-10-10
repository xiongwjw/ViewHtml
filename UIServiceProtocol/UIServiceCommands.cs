/********************************************************************
	FileName:   UIServiceCommands
    purpose:	

	author:		huang wei
	created:	2012/12/24

    revised history:
	2012/12/24  

================================================================
    Copyright (C) 2012, Grgbanking CO,. Ltd. All rights reserved.
================================================================
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UIServiceProtocol
{
    public class UIServiceCommands
    {
        /// <summary>
        /// clear data bind
        /// </summary>
        public const string s_clearDataBind = "cleardatabind";
        /// <summary>
        /// Add hot areas of a UI service
        /// </summary>
        public const string s_addHotArea = "addhotarea";
        /// <summary>
        /// Remove hot areas of a UI service
        /// </summary>
        public const string s_removeHotArea = "removehotarea";
        /// <summary>
        /// Clear hot areas of a UI service
        /// </summary>
        public const string s_clearHotArea = "clearhotarea";
        /// <summary>
        /// 
        /// </summary>
        public const string s_addElementScript = "addelementscript";

        /// <summary>
        /// 
        /// </summary>
        public const string s_updateData = "updateData";

        /// <summary>
        /// 
        /// </summary>
        public const string s_buttonDown = "buttondown";

        /// <summary>
        /// 
        /// </summary>
        public const string s_buttonUp = "buttonUp";

        /// <summary>
        /// 
        /// </summary>
        public const string s_buttonClick = "buttonClick";

        /// <summary>
        /// 
        /// </summary>
        public const string s_reloadAllStoryboards = "reloadAllStoryboards";

        public const string s_saveSign = "saveSign";
    }
}
