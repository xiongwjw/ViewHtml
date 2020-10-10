/********************************************************************
	FileName:   UIPropertyKey
    purpose:	keys of properties of a element

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
    /// keys of propeties of a element
    /// </summary>
    public static class UIPropertyKey
    {
        /// <summary>
        /// The key of the "data context" property
        /// </summary>
        public const string s_DataContextKey = "datacontext";

        /// <summary>
        /// The key of the "Item source" property
        /// </summary>
        public const string s_ItemSourceKey = "itemSource";

        /// <summary>
        /// The key of the "visible" property
        /// </summary>
        public const string s_VisibleKey = "visible";

        /// <summary>
        /// The key of the "enable" property
        /// </summary>
        public const string s_EnableKey = "enable";

        /// <summary>
        /// The key of the "value" property
        /// </summary>
        public const string s_ValueKey = "value";

        /// <summary>
        /// The key of the "clritemsource" property
        /// </summary>
        public const string s_ClearItemSourceKey = "clritemsource";

        /// <summary>
        /// The key of the "clearcontent" property
        /// </summary>
        public const string s_ClearContent = "clearcontent";

        /// <summary>
        /// The key of the "beginstoryboard" property
        /// </summary>
        public const string s_BeginStoryboard = "beginstoryboard";

        /// <summary>
        /// 
        /// </summary>
        public const string s_ContentKey = "content";

        /// <summary>
        /// 
        /// </summary>
        public const string s_NameKey = "name";

        /// <summary>
        /// 
        /// </summary>
        public const string s_tagKey = "tag";

        /// <summary>
        /// 
        /// </summary>
        public const string s_addValueKey = "+value";

        /// <summary>
        /// 
        /// </summary>
        public const string s_removeValueKey = "-value";

        /// <summary>
        /// 
        /// </summary>
        public const string s_formatedContentKey = "formatedContent";

        /// <summary>
        /// 
        /// </summary>
        public const string s_selectIndexKey = "selectIndex";

        /// <summary>
        /// 
        /// </summary>
        public const string s_selectValueKey = "selectValue";

        /// <summary>
        /// 
        /// </summary>
        public const string s_selectTitleKey = "selectTitle";

        /// <summary>
        /// 
        /// </summary>
        public const string s_showVirtualKeyboard = "showVKeyboard";

        /// <summary>
        /// The current page's index of a list control in html
        /// </summary>
        public const string s_currentPageIndexKey = "curPageIndex";

        /// <summary>
        /// The total page count of a list control in html
        /// </summary>
        public const string s_totalPageCountKey = "totalPageCount";

        /// <summary>
        /// Visible item count of a lite control in html
        /// </summary>
        public const string s_visibleItemCountKey = "visibleItemCount";

        public const string s_canFirstPageKey = "canFirstPage";

        public const string s_canLastPageKey = "canLastPage";

        public const string s_canBackwardKey = "canBackward";

        public const string s_canForwardKey = "canForward";

        /// <summary>
        /// 
        /// </summary>
        public const string s_itemTemplateKey = "itemTemplate";

        /// <summary>
        /// 
        /// </summary>
        public const string s_clickKey = "click";

        public const string s_firstPageKey = "first";

        public const string s_lastPageKey = "last";

        public const string s_stateKey = "state";

        public const string s_confirmTag = "OnConfirm";

        public const string s_cancelTag = "OnCancel";

        public const string s_ignoreTag = "OnIgnore";

        public const string s_retryTag = "OnRetry";

        public const string s_motherBoardElementName = "motherBoard";
    }

    public class SpecialElement
    {
        public const string s_Focus = "focusElement";
    }
}
