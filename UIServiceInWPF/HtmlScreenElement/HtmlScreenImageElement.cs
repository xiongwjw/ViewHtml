using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using UIServiceProtocol;
using LogProcessorService;

namespace UIServiceInWPF.HtmlScreenElement
{
    class HtmlScreenImageElement : HtmlScreenElementBase
    {
        public HtmlScreenImageElement( HtmlElement argElement,
                                       HtmlRender argParent) : base(argElement, argParent)
        {

        }

        public override bool GetPropertyValue(string argProperty, out object argValue)
        {
            if ( string.Equals( argProperty, UIPropertyKey.s_ContentKey, StringComparison.Ordinal ) )
            {
                Debug.Assert(null != m_ihostedElement);
                string src = m_ihostedElement.GetAttribute("src");
                argValue = src;
                return true;
            }

            return base.GetPropertyValue(argProperty, out argValue);
        }

        public override bool SetPropertyValue(string argProperty, object argValue)
        {
            if ( string.Equals( argProperty, UIPropertyKey.s_ContentKey, StringComparison.Ordinal ) )
            {
                Debug.Assert(null != m_ihostedElement);
                if ( null == argValue )
                {
                    m_ihostedElement.SetAttribute("src", string.Empty);
                }
                else
                {
                    m_ihostedElement.SetAttribute("src", argValue.ToString());
                }
                return true;
            }
            return base.SetPropertyValue(argProperty, argValue);
        }
    }
}
