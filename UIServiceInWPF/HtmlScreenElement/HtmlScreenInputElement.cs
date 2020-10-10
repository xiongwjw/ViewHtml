using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using UIServiceInWPF;
using System.Windows.Forms;
using UIServiceProtocol;
using StringFormatter;

namespace UIServiceInWPF.HtmlScreenElement
{
    class HtmlScreenInputElement : HtmlScreenElementBase
    {
        [Flags]
        public enum FormatAlign
        {
            Unknown = 0,
            Far,
            Near
        }

#region constructor
        public HtmlScreenInputElement( HtmlElement argElement,
                                       HtmlRender argParent)
            : base(argElement, argParent)
        {

        }
#endregion

#region property
        public string FormatPattern
        {
            get
            {
                return m_formatPattern;
            }
            set
            {
                m_formatPattern = value;
                if ( !string.IsNullOrEmpty(m_formatPattern) )
                {
                    if ( m_formatPattern[0] != s_plackHolderChar )
                    {
                        m_fmtAlign = FormatAlign.Near;
                    }
                    else
                    {
                        m_fmtAlign = FormatAlign.Far;
                    }
                }
            }
        }

        public override bool CanFocus
        {
            get
            {
                if ( IsEnable )
                {
                    if ( string.Equals( m_ihostedElement.TagName, "input", StringComparison.OrdinalIgnoreCase ) )
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public bool CanShowVirtualKeyboard
        {
            get
            {
                return m_canShowVirtualKeyboard;
            }
            set
            {
                m_canShowVirtualKeyboard = value;
            }
        }
#endregion

#region method
        public override void InnerDispose()
        {
            m_formatContent = null;
            m_formatPattern = null;

            base.InnerDispose();
        }

        public override void SetFocus()
        {
            BindingExpress exp = FindBindingExpress(UIPropertyKey.s_ContentKey);
            if (null != exp)
            {
                exp.UpdateData(true);
                m_parent.OnFocusChanged(m_ihostedElement.Id, exp.BindedProperty);
            }

            if ( m_canShowVirtualKeyboard )
            {
  //              m_parent.ShowScreenKeyboard(true);
            }
            else
            {
//                m_parent.ShowScreenKeyboard(false);
            }
        }

        public override void killFocus()
        {
            BindingExpress exp = FindBindingExpress(UIPropertyKey.s_ContentKey);
            if ( null != exp )
            {
                exp.UpdateData(true);
                m_parent.OnFocusChanged(m_ihostedElement.Id, null);
            }
        }

        private BindingExpress FindBindingExpress( string argPropName )
        {
            BindingExpress exp = null;
            foreach ( var item in m_dicBindingExpress )
            {
                exp = item.Value;
                if ( exp.IsValid &&
                     exp.PropertyOfElement.Equals( argPropName, StringComparison.Ordinal ) )
                {
                    return exp;
                }
            }

            return null;
        }

        protected bool FormatText( string argContent,
                                   out string argResult )
        {
            argResult = null;

            if ( string.IsNullOrEmpty(argContent) ||
                 string.IsNullOrEmpty(m_formatPattern) )
            {
                return false;
            }

            StringFormatterImp.Create().Format(argContent, m_formatPattern, out argResult);

            //m_formatContent.Clear();
            //int contentLength = argContent.Length;
            //int PatternLength = m_formatPattern.Length;
            //int contentIndex = 0;
            //int patternIndex = 0;
            //if ( FormatAlign.Far == m_fmtAlign )
            //{
            //    while (true)
            //    {
            //        if (m_formatPattern[patternIndex] != s_plackHolderChar)
            //        {
            //            m_formatContent.Append(m_formatPattern[patternIndex]);
            //        }
            //        else
            //        {
            //            m_formatContent.Append(argContent[contentIndex]);
            //            ++contentIndex;
            //            if (contentIndex >= contentLength)
            //            {
            //                break;
            //            }
            //        }

            //        ++patternIndex;
            //        if (patternIndex >= PatternLength)
            //        {
            //            patternIndex = 0;
            //        }
            //    }

            //    argResult = m_formatContent.ToString();
            //}
            //else
            //{
            //    contentIndex = contentLength - 1;
            //    patternIndex = PatternLength - 1;
            //    while ( true )
            //    {
            //        if (m_formatPattern[patternIndex] != s_plackHolderChar)
            //        {
            //            m_formatContent.Append(m_formatPattern[patternIndex]);
            //        }
            //        else
            //        {
            //            m_formatContent.Append(argContent[contentIndex]);
            //            --contentIndex;
            //            if (contentIndex < 0)
            //            {
            //                break;
            //            }
            //        }

            //        --patternIndex;
            //        if (patternIndex < 0)
            //        {
            //            patternIndex = PatternLength - 1;
            //        }
            //    }

            //    Char[] arrContent = m_formatContent.ToString().ToCharArray();
            //    Array.Reverse(arrContent);
            //    argResult = new string(arrContent);
            //}

            return true;
        }

        public override bool SetPropertyValue(string argProperty, object argValue)
        {
            if ( /*0 == string.Compare( argProperty, UIPropertyKey.s_ContentKey, true )*/
                 argProperty.Equals(UIPropertyKey.s_ContentKey, StringComparison.OrdinalIgnoreCase) &&
                 (argValue is string || argValue is int || argValue is double || argValue is float || argValue is long || argValue is short))
            {
                string formatValue = null;
                m_realContent.Clear();
                m_realContent.Append(argValue);
                if ( FormatText( argValue.ToString(), out formatValue ) )
                {
                    return base.SetPropertyValue(argProperty, HtmlRender.ConvertText(formatValue));
                }
                else
                {
                    return base.SetPropertyValue(argProperty, HtmlRender.ConvertText(argValue.ToString()));
                }
            }
            //else if ( 0 == string.Compare( argProperty, UIPropertyKey.s_ClearContent, true ) )
            else if ( argProperty.Equals(UIPropertyKey.s_ClearContent, StringComparison.OrdinalIgnoreCase) )
            {
                m_realContent.Clear();
            }
            else if ( argProperty.Equals( UIPropertyKey.s_addValueKey, StringComparison.OrdinalIgnoreCase ) )
            {
                if ( null != argValue )
                {
                    m_realContent.Append(argValue);
                    return SetFormatOutput(UIPropertyKey.s_ContentKey, m_realContent.ToString());
                }

                return true;
            }
            else if ( argProperty.Equals( UIPropertyKey.s_removeValueKey, StringComparison.OrdinalIgnoreCase ) )
            {
                if ( m_realContent.Length > 0 )
                {
                    m_realContent.Remove(m_realContent.Length - 1, 1);
                    if ( m_realContent.Length > 0 )
                    {
                        return SetFormatOutput( UIPropertyKey.s_ContentKey, m_realContent.ToString() );
                    }
                    else
                    {
                        return base.SetPropertyValue(UIPropertyKey.s_ContentKey, null);
                    }
                }

                return true;
            }

            return base.SetPropertyValue(argProperty, argValue);
        }

        public override bool GetPropertyValue(string argProperty, out object argValue)
        {
            //if ( 0 == string.Compare( argProperty, UIPropertyKey.s_ContentKey, true ) )
            if ( argProperty.Equals(UIPropertyKey.s_ContentKey, StringComparison.OrdinalIgnoreCase) )
            {
                if ( !string.IsNullOrEmpty(m_formatPattern) )
                {
                    argValue = m_realContent.ToString();
                    return true;
                }
            }

            return base.GetPropertyValue(argProperty, out argValue);
        }

        private bool SetFormatOutput( string argProperty,
                                      string argContent )
        {
            string formatValue = null;
            if (FormatText(argContent, out formatValue))
            {
                return base.SetPropertyValue(argProperty, HtmlRender.ConvertText(formatValue));
            }
            else
            {
                return base.SetPropertyValue(argProperty, HtmlRender.ConvertText(argContent));
            }
        }
#endregion

#region field
        protected string m_formatPattern = null;

        protected StringBuilder m_realContent = new StringBuilder();

        protected StringBuilder m_formatContent = new StringBuilder();

        protected FormatAlign m_fmtAlign = FormatAlign.Unknown;

        public const char s_plackHolderChar = '#';

        protected bool m_canShowVirtualKeyboard = true;
#endregion
    }
}
