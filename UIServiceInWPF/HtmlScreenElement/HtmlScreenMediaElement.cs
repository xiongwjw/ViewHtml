using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogProcessorService;
using mshtml;
using System.Windows.Forms;
using System.Diagnostics;
using UIServiceProtocol;

namespace UIServiceInWPF.HtmlScreenElement
{
    class HtmlScreenMediaElement : HtmlScreenElementBase
    {
#region constructor
        public HtmlScreenMediaElement( HtmlElement argElement,
                                       HtmlRender argParent)
            : base(argElement, argParent)
        {
            if ( null != argElement.FirstChild &&
                 argElement.FirstChild.TagName.Equals( s_grgMediaType, StringComparison.OrdinalIgnoreCase ) )
            {
                m_grgMediaOriginContent = argElement.FirstChild.OuterHtml.Trim();
                m_grgMediaOriginContent = m_grgMediaOriginContent.Replace(s_grgMediaType, "embed");
            }
        }
#endregion

#region method
        public override void InnerDispose()
        {
            base.InnerDispose();

            m_grgMediaOriginContent = null;
            m_source = null;
        }

        public override bool GetPropertyValue(string argProperty, out object argValue)
        {
            argValue = null;
            if ( null == m_ihostedElement )
            {
                return false;
            }
          
            if ( argProperty.Equals(UIPropertyKey.s_ContentKey, StringComparison.OrdinalIgnoreCase) )
            {
                argValue = m_source;
                return true;
            }
            else
            {
                return base.GetPropertyValue(argProperty, out argValue);
            }         
        }

        public override bool SetPropertyValue(string argProperty, object argValue)
        {
            if (null == m_ihostedElement)
            {
                return false;
            }

            if ( argProperty.Equals( UIPropertyKey.s_ContentKey, StringComparison.OrdinalIgnoreCase ) )
            {
                if ( string.IsNullOrEmpty(m_grgMediaOriginContent) )
                {
                    return false;
                }

                if ( null == argValue ||
                     string.IsNullOrEmpty( (string)argValue ) )
                {
                    m_ihostedElement.InnerHtml = string.Empty;
                }
                else
                {
                    //if ( !string.Equals( m_source, (string)argValue, StringComparison.OrdinalIgnoreCase ) )
                    {
                        m_source = (string)argValue;
                        string embedHtml = m_grgMediaOriginContent.Insert( 6, string.Format( s_sourceFormat, m_source ) );
                        m_ihostedElement.InnerHtml = embedHtml;
                    }
                }

                return true;
            }
            else
            {
                return base.SetPropertyValue(argProperty, argValue);
            }         
        }
#endregion

#region field
        protected string m_source = null;

        protected string m_grgMediaOriginContent = null;

        public const string s_sourceFormat = " src=\"{0}\" ";

        public const string s_flashType = "application/x-shockwave-flash";

        public const string s_grgMediaType = "MEDIAPLACEHOLDER";
#endregion
    }
}
