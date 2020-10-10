/********************************************************************
	FileName:   ResourceDataTemplate
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
//using mshtml;
using UIServiceInWPF.HtmlScreenElement;
using System.Windows.Forms;
using UIServiceProtocol;

namespace UIServiceInWPF.Resource
{
    public class DataTemplateTDItem
    {
#region constructor
        public DataTemplateTDItem()
        {

        }
#endregion

#region property
        public string ClassName
        {
            get;
            set;
        }

        public string StyleValue
        {
            get;
            set;
        }

        public string InnerHtml
        {
            get;
            set;
        }
#endregion
    }

    public class ResourceDataTemplate : ResourceItem
    {
#region constructor
        public ResourceDataTemplate(string argName)
            : base(argName)
        {

        }
#endregion

#region method
        //public override bool Open(System.Windows.Forms.HtmlElement argElement)
        //{
        //    bool result = base.Open(argElement);
        //    if ( !result )
        //    {
        //        return result;
        //    }

        //    //load all <td> element
        //    System.Windows.Forms.HtmlElementCollection elementCol = argElement.GetElementsByTagName(s_tdTag);
        //    string cssValue = null;
        //    string styleValue = null;
        //    string bindValue = null;
        //    foreach ( HtmlElement item in elementCol )
        //    {
        //        //load bind value
        //        bindValue = HtmlScreenElementBase.ParseBindedProperty(UIPropertyKey.s_ContentKey, item);
        //        if ( string.IsNullOrEmpty(bindValue) )
        //        {
        //            continue;
        //        }
        //        //load css attribute
        //        cssValue = item.GetAttribute(s_cssAttri);
        //        //load style attribute
        //        styleValue = item.GetAttribute(s_styleAttri);
        //        //add a td item.
        //        m_listTdItems.Add(new DataTemplateTDItem()
        //            {
        //                BindKey = bindValue,
        //                ClassName = cssValue,
        //                StyleValue = styleValue
        //            });
        //    }

        //    return result;
        //}

        public override void Close()
        {
            base.Close();

            m_listTdItems.Clear();
            m_listTdItems = null;
        }
#endregion

#region property
        public List<DataTemplateTDItem> TDItems
        {
            get
            {
                return m_listTdItems;
            }
        }
#endregion

#region field
        protected List<DataTemplateTDItem> m_listTdItems = new List<DataTemplateTDItem>();

        //public const string s_tdTag = "td";

        //public const string s_cssAttri = "class";

        //public const string s_styleAttri = "style";
#endregion
    }
}
