/********************************************************************
	FileName:   UIEventNames
    purpose:	Events of the UI service

	author:		huang wei
	created:	2012/10/23

    revised history:
	2012/10/23  

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
    /// <summary>
    /// Event names of the UI service
    /// </summary>
    public static class UIEventNames
    {
        public const string s_MouseDown = "MouseDown";

        /// <summary>
        /// Click event of a button.
        /// </summary>
        public const string s_ClickEvent = "click";

        /// <summary>
        /// PreShow event of a screen. A UI service raises the PreShow event before a screen appears.
        /// </summary>
        public const string s_PreShowEvent = "preshow";

        /// <summary>
        /// Show event of a screen. A UI service raises the Show event after a screen appears.
        /// </summary>
        public const string s_ShowEvent = "show";

        /// <summary>
        /// cdcompleted event of a Countdown element. A Countdown element raises the "cdcompleted" event after count of Countdown element is up to the designed value.
        /// </summary>
        public const string s_CountdownCompleteEvent = "cdcompleted";
        /// <summary>
        /// SelectionChanged event of a listbox
        /// </summary>
        public const string s_ListBoxSelectionChanged = "listboxselectionchanged";

        /// <summary>
        /// drop event of listview
        /// </summary>
        public const string s_DropEvent = "drop";

        /// <summary>
        /// 
        /// </summary>
        public const string s_focusChanged = "fcsChanged";

        public const string s_checkedEvent = "checked";

        public const string s_uncheckedEvent = "unchecked";

        public const string s_UIFatalError = "UIFatalError";

        public const string s_timeoutEvent = "timeout";
        /// <summary>
        /// 弹出界面事件
        /// </summary>
        public const string s_popupEvent = "popup";
    }
}
