using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using UIServiceInWPF;
using UIServiceProtocol;
using mshtml;
using System.Runtime.InteropServices;
using LogProcessorService;

namespace UIServiceInWPF.HtmlScreenElement
{
    public class HtmlScreenElementBase : IDisposable
    {
        protected delegate void UpdateDataHandler(BindingExpress argExpress,
                                                  bool argUpdate);
#region constructor
        public HtmlScreenElementBase( HtmlElement argElement,
                                      HtmlRender argParent)
        {
            Debug.Assert(null != argElement && null != argParent);

            m_ihostedElement = argElement;
            m_parent = argParent;

            
        }
#endregion

#region methods of the IDisposable interface
        public void Dispose()
        {
            InnerDispose();
        }
#endregion

#region method
        public virtual bool IsHostedElement( HtmlElement argElement )
        {
            return m_ihostedElement == argElement ? true : false;
        }

        public void OnValueChange(string property,object value)
        {
            m_parent.OnValueChange(m_ihostedElement.Id, property, value);
        }

        public virtual bool Open()
        {
            return true;
        }

        public virtual void InnerDispose()
        {
            if ( null != m_dicBindingExpress )
            {
                foreach ( var item in m_dicBindingExpress )
                {
                    item.Value.Close();
                }
                m_dicBindingExpress.Clear();
                m_dicBindingExpress = null;
            }

            m_ihostedElement = null;
            m_updatDataHandler = null;
            m_parent = null;
        }

        public virtual void SetFocus()
        {

        }

        public virtual HtmlScreenElementBase FindHtmlScreenElement( HtmlElement argElement )
        {
            Debug.Assert(null != argElement);
            if ( IsHostedElement( argElement ) )
            {
                return this;
            }

            return null;
        }

        public virtual void killFocus()
        {

        }

        public virtual void UpdateBindingData( bool argSave )
        {
            foreach (var item in m_dicBindingExpress)
            {
                item.Value.UpdateData(argSave);
            }
        }

        public virtual bool SetPropertyValue( string argProperty,
                                              object argValue )
        {
            //Debug.Assert(null != m_ihostedElement);
            if ( null == m_ihostedElement )
            {
                return false;
            }

            try
            {
                switch (argProperty)
                {
                    case UIPropertyKey.s_ClearContent:
                        {
                            if (m_ihostedElement.TagName.Equals("input", StringComparison.OrdinalIgnoreCase))
                            {
                                m_ihostedElement.SetAttribute("value", "");
                            }
                            else
                            {
                                m_ihostedElement.InnerHtml = "";
                                m_ihostedElement.InnerText = "";
                            }
                        }
                        break;

                    case UIPropertyKey.s_ValueKey:
                    case UIPropertyKey.s_ContentKey:
                        {
                            //if (null == argValue)
                            //{
                            //    return false;
                            //}

                            //if (0 == string.Compare(m_ihostedElement.TagName, "input", true))
                            if ( null != argValue )
                            {
                                string contentValue = null;
                                if (m_ihostedElement.TagName.Equals("input", StringComparison.OrdinalIgnoreCase))
                                {
                                    contentValue = argValue is string ? (string)argValue : argValue.ToString();
                                    m_ihostedElement.SetAttribute("value", contentValue);
                                }
                                else if (argValue is double ||
                                          argValue is Single)
                                {
                                    contentValue = ((double)argValue).ToString("F");
                                    m_ihostedElement.InnerText = contentValue;
                                }
                                else
                                {
                                    contentValue = argValue is string ? (string)argValue : argValue.ToString();
                                    m_ihostedElement.InnerHtml = contentValue;
                                }
                                m_parent.NotifyPropertyChanged(HostedElement.Id, UIPropertyKey.s_ContentKey, contentValue);
                            }
                            else
                            {
                                if (m_ihostedElement.TagName.Equals("input", StringComparison.OrdinalIgnoreCase))
                                {
                                    //如果绑定池为null，则取defaultValue值,如果不为空，显示defaultValue,如果为空，显示"" by lsjie5
                                    var defaultValue = m_ihostedElement.GetAttribute("defaultValue");
                                    if (string.IsNullOrEmpty(defaultValue))
                                    {
                                        m_ihostedElement.SetAttribute("value", "");
                                    }
                                    else
                                    {
                                        m_ihostedElement.SetAttribute("value", defaultValue);
                                    } 
                                }
                                else
                                {
                                    //如果绑定池为null，则取defaultValue值,如果不为空，显示defaultValue,如果为空，显示"" by lsjie5
                                    var defaultValue = m_ihostedElement.GetAttribute("defaultValue");
                                    if (string.IsNullOrEmpty(defaultValue))
                                    {
                                        m_ihostedElement.InnerHtml = "";
                                        m_ihostedElement.InnerText = "";
                                    }
                                    else
                                    {
                                        m_ihostedElement.InnerHtml = defaultValue;
                                    }
                                    
                                }
                            }
                        }
                        break;

                    case UIPropertyKey.s_NameKey:
                        {
                            if (null == argValue ||
                                !(argValue is string))
                            {
                                return false;
                            }

                            m_ihostedElement.Id = (string)argValue;
                            m_parent.NotifyPropertyChanged(HostedElement.Id, UIPropertyKey.s_NameKey, m_ihostedElement.Id);
                        }
                        break;

                    case UIPropertyKey.s_tagKey:
                        {
                            if (null == argValue )
                            {
                                return false;
                            }

                            if ( argValue is string )
                            {
                                m_ihostedElement.SetAttribute(HtmlRender.s_tagAttri, (string)argValue);
                            }
                            else
                            {
                                m_ihostedElement.SetAttribute(HtmlRender.s_tagAttri, argValue.ToString());
                            }

                            m_parent.NotifyPropertyChanged(HostedElement.Id, UIPropertyKey.s_NameKey, (string)argValue);
                        }
                        break;

                    case UIPropertyKey.s_VisibleKey:
                        {
                            if (null == argValue )
                            {
                                return false;
                            }

                            bool visible = true;
                            if ( argValue is string )
                            {
                                int temp = 0;
                                if (!int.TryParse((string)argValue, out temp))
                                {
                                    return false;
                                }
                                visible = temp == 0 ? false : true;
                            }
                            else if ( argValue is bool )
                            {
                                visible = (bool)argValue;
                            }
                            else
                            {
                                return false;
                            }

                            if (false == visible)
                            {
                                IHTMLElement element = (IHTMLElement)m_ihostedElement.DomElement;
                                IHTMLStyle iStyle = element.style;
                                if ( null != iStyle )
                                {
                                    if ( !string.Equals( iStyle.display, "none", StringComparison.OrdinalIgnoreCase ) )
                                    {
                                        m_oldDisplayState = iStyle.display;
                                        iStyle.display = "none";
                                    }

                                    iStyle = null;
                                }
                                else
                                {
                                    m_oldDisplayState = string.Empty;
                                    m_ihostedElement.SetAttribute("style", "display:none");
                                }
                                element = null;
                                //if (!string.IsNullOrEmpty(m_ihostedElement.Style))
                                //{
                                //    int index = m_ihostedElement.Style.ToLowerInvariant().IndexOf(HtmlRender.s_hideStyle);
                                //    if (-1 == index)
                                //    {
                                //        m_ihostedElement.Style += HtmlRender.s_hideStyle;
                                //    }
                                //}
                                //else
                                //{
                                //    m_ihostedElement.Style = HtmlRender.s_hideStyle;
                                //}
                            }
                            else
                            {
                                IHTMLElement element = (IHTMLElement)m_ihostedElement.DomElement;
                                IHTMLStyle iStyle = element.style;
                                if ( null != iStyle )
                                {
                                    iStyle.display = m_oldDisplayState;
                                    m_oldDisplayState = string.Empty;
                                }
                                element = null;
                    //            m_ihostedElement.Style = "";
                                //if (!string.IsNullOrEmpty (m_ihostedElement.Style))
                                //{
                                //    int index = m_ihostedElement.Style.ToLowerInvariant().IndexOf(Page4Html.s_hideStyle);
                                //    if (-1 != index)
                                //    {
                                //        m_ihostedElement.Style = null;
                                //    }
                                //}
                            }

                            m_parent.NotifyPropertyChanged(HostedElement.Id, UIPropertyKey.s_VisibleKey, visible ? "1" : "0");
                        }
                        break;

                    case UIPropertyKey.s_EnableKey:
                        {
                            if (null == argValue )
                            {
                                return false;
                            }

                            bool enable = true;
                            if (argValue is string)
                            {
                                int temp = 0;
                                if (!int.TryParse((string)argValue, out temp))
                                {
                                    return false;
                                }
                                enable = temp == 0 ? false : true;
                            }
                            else if ( argValue is bool )
                            {
                                enable = (bool)argValue;
                            }
                            else
                            {
                                return false;
                            }

                            if ( m_ihostedElement.Enabled != enable )
                            {
                                m_ihostedElement.Enabled = enable;
                                m_parent.NotifyPropertyChanged(HostedElement.Id, UIPropertyKey.s_EnableKey, enable ? "1" : "0");
                            }                          
                        }
                        break;

                    default:
                        {
                            return false;
                        }
                }
            }
            catch (System.Exception ex)
            {
                Trace.Write(ex.Message);
               // LogProcessorService.Log.UIService.LogError(string.Format("Failed to set property[{0}] value of the element[{1}]", argProperty, m_ihostedElement.Id), ex);
                return false;
            }

            return true;
        }

        public virtual bool GetPropertyValue( string argProperty,
                                              out object argValue )
        {
            argValue = null;
            if (null == m_ihostedElement)
            {
                return false;
            }

            //Debug.Assert(null != m_ihostedElement)
            try
            {
                switch (argProperty)
                {
                    case UIPropertyKey.s_ValueKey:
                    case UIPropertyKey.s_ContentKey:
                    case UIPropertyKey.s_formatedContentKey:
                        {
                            //Log.UIService.LogDebugFormat("Get content of element[{0}]", m_ihostedElement.Id);
                            string content = null;
                            //if (0 == string.Compare(m_ihostedElement.TagName, "input", true))
                            if ( m_ihostedElement.TagName.Equals("input", StringComparison.OrdinalIgnoreCase) )
                            {
                                content = m_ihostedElement.GetAttribute("value");
                                //Log.UIService.LogDebugFormat("Get content[{1}] of element[{0}]", m_ihostedElement.Id, content);
                            }
                            else
                            {
                                content = m_ihostedElement.InnerText;
                            }

                            argValue = content;
                        }
                        break;

                    case UIPropertyKey.s_NameKey:
                        {
                            argValue = m_ihostedElement.Id;
                        }
                        break;

                    case UIPropertyKey.s_tagKey:
                        {
                            if (!IsElementVisible(m_ihostedElement) ||
                                 !m_ihostedElement.Enabled)
                            {
                                return false;
                            }

                            string tag = m_ihostedElement.GetAttribute(HtmlRender.s_tagAttri);
                            argValue = tag;
                        }
                        break;

                    case UIPropertyKey.s_VisibleKey:
                        {
                            argValue = IsElementVisible(m_ihostedElement);
                        }
                        break;

                    case UIPropertyKey.s_EnableKey:
                        {
                            argValue = m_ihostedElement.Enabled;
                        }
                        break;

                    default:
                        {
                            return false;
                        }
                }
            }
            catch (System.Exception ex)
            {
                Trace.Write(ex.Message);
                //LogProcessorService.Log.UIService.LogError(string.Format("Failed to get property[{0}] value of the element[{1}]", argProperty, m_ihostedElement.Id), ex);
                return false;
            }

            return true;
        }

        public virtual void OnLeftMouseDown( object argSender,
                                             HtmlElementEventArgs arg )
        {

        }

        public virtual void OnLeftMouseUp( object argSender,
                                           HtmlElementEventArgs arg,
                                           bool argTriggerEvt = true )
        {

        }

        public virtual void ParseResource()
        {

        }

        private bool IsElementVisible(System.Windows.Forms.HtmlElement argElement)
        {
            Debug.Assert(null != argElement);

            bool Result = true;
            try
            {
                IHTMLElement iElement = (IHTMLElement)argElement.DomElement;
                IHTMLStyle iStyle = iElement.style;
                if ( !string.IsNullOrEmpty( iStyle.display ) &&
                     iStyle.display.Equals("none", StringComparison.OrdinalIgnoreCase))
                {
                    Result = false;
                }
                else
                {
                    Result = true;
                }

                iElement = null;
                iStyle = null;
            }
            catch (System.Exception ex)
            {
                Trace.Write(ex.Message);
                return true;
            }
            
            //if (!string.IsNullOrEmpty(argElement.Style))
            //{
            //    string style = argElement.Style;
            //    style = style.ToLowerInvariant();
            //    int index = style.IndexOf(HtmlRender.s_hideStyle);
            //    if (-1 != index)
            //    {
            //        Result = false;
            //    }
            //}

            return Result;
        }

        public void SetBindingTarget( object argTarget )
        {
            foreach ( var bind in m_dicBindingExpress )
            {
                bind.Value.DataContent = argTarget;
            }
        }

        public bool OnPropertyChanged( string argProperty )
        {
            Debug.Assert(!string.IsNullOrEmpty(argProperty));
            if ( null == m_dicBindingExpress )
            {
                return false;
            }

            if ( m_dicBindingExpress.ContainsKey(argProperty) &&
                 m_dicBindingExpress[argProperty].IsValid )
            {
                //m_parent.Dispatcher.Invoke(UpdateData,
                //                            m_dicBindingExpress[argProperty],
                //                            false);
                m_dicBindingExpress[argProperty].UpdateData(false);
            }
            else
            {
                return false;
            }

            return true;
        }

        public virtual void ParseBindingExpress()
        {
            try
            {
                //check content property
                BindingMode mode = BindingMode.OneWay;
                string propValue = ParseBindedProperty(UIPropertyKey.s_ContentKey, m_ihostedElement, ref mode);
                if ( !string.IsNullOrEmpty( propValue ) )
                {
                    m_dicBindingExpress.Add(propValue, new BindingExpress(propValue, UIPropertyKey.s_ContentKey, this)
                        {
                            Mode = mode
                        });
                }

                //check visible property
                propValue = ParseBindedProperty(UIPropertyKey.s_VisibleKey, m_ihostedElement, ref mode);
                if ( !string.IsNullOrEmpty( propValue ) )
                {
                    m_dicBindingExpress.Add(propValue, new BindingExpress(propValue, UIPropertyKey.s_VisibleKey, this)
                        {
                            Mode = mode
                        });
                }

                //check enable property
                propValue = ParseBindedProperty(UIPropertyKey.s_EnableKey, m_ihostedElement, ref mode);
                if ( !string.IsNullOrEmpty( propValue ) )
                {
                    m_dicBindingExpress.Add(propValue, new BindingExpress(propValue, UIPropertyKey.s_EnableKey, this)
                        {
                            Mode = mode
                        });
                }

                //check tag property
                propValue = ParseBindedProperty(UIPropertyKey.s_tagKey, m_ihostedElement, ref mode);
                if ( !string.IsNullOrEmpty( propValue ) )
                {
                    m_dicBindingExpress.Add(propValue, new BindingExpress(propValue, UIPropertyKey.s_tagKey, this)
                        {
                            Mode = mode
                        });
                }
            }
            catch (System.Exception ex)
            {
                LogProcessorService.Log.UIService.LogWarn("Failed to parse binded express", ex);	
            }
        }

        public string ParseBindedProperty( string argProperty,
                                           HtmlElement argHostedElement,
                                           ref BindingMode argMode )
        {
            Debug.Assert(!string.IsNullOrEmpty(argProperty));

            string strPropvalue = argHostedElement.GetAttribute(argProperty);
            if ( string.IsNullOrEmpty(strPropvalue) )
            {
                return null;
            }

            string bindElement = null;
            string bindProp = null;
            //BindingMode mode = BindingMode.OneWay;
            string value = ParseBindValue(strPropvalue, ref bindElement, ref bindProp, ref argMode);
            if ( !string.IsNullOrEmpty(bindElement) &&
                 !string.IsNullOrEmpty(bindProp) )
            {
                m_parent.AddElementBinding(new ElementBindingExpress(bindElement,
                                                                       bindProp,
                                                                       argProperty,
                                                                       this));
            }

            return value;
        }

        public static string ParseBindValue( string argBind,
                                             ref string argBindElement,
                                             ref string argBindProp,
                                             ref BindingMode argMode )
        {
            Debug.Assert(!string.IsNullOrEmpty(argBind));
            argBindElement = null;
            argBindProp = null;
            argMode = BindingMode.OneWay;

            argBind = argBind.Trim();
            if (string.IsNullOrEmpty(argBind))
            {
                return null;
            }

            if (!argBind.StartsWith(s_leftDelimit) ||
                 !argBind.EndsWith(s_rightDelimit))
            {
                return null;
            }

            argBind = argBind.Trim('{', '}');
            if (string.IsNullOrEmpty(argBind))
            {
                return null;
            }

            string[] argBindExpress = argBind.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (argBindExpress.Length < 2 ||
                (!s_binding.Equals(argBindExpress[0], StringComparison.OrdinalIgnoreCase) &&
                 !s_elementBinding.Equals(argBindExpress[0],StringComparison.OrdinalIgnoreCase) ))
            {
                return null;
            }

            if (s_binding.Equals(argBindExpress[0], StringComparison.OrdinalIgnoreCase))
            {
                if ( argBindExpress.Length > 2 )
                {
                    string[] arrMode = argBindExpress[2].Split( new char[] {'='}, StringSplitOptions.RemoveEmptyEntries );
                    if ( arrMode.Length != 2 ||
                         !arrMode[0].Equals( UIServiceCfgDefines.s_modeAttri, StringComparison.OrdinalIgnoreCase ) ||
                         string.IsNullOrEmpty(arrMode[1]) )
                    {
                        return argBindExpress[1];
                    }
                    int temp = 0;
                    if ( int.TryParse( arrMode[1], out temp ) )
                    {
                        switch (temp)
                        {
                            case 1:
                                {
                                    argMode = BindingMode.OneWay;
                                }
                                break;

                            case 2:
                                {
                                    argMode = BindingMode.TwoWay;
                                }
                                break;

                            case 3:
                                {
                                    argMode = BindingMode.SoureTarget;
                                }
                                break;
                        }
                    }
                    arrMode = null;
                }
                return argBindExpress[1];
            }
            else if ( s_elementBinding.Equals(argBindExpress[0], StringComparison.OrdinalIgnoreCase) )
            {
                if ( argBindExpress.Length < 3 )
                {
                    return null;
                }

                int total = argBindExpress.Length;
                string[] arrProp = null;
                char[] arrSp = new char[]{'='};
                for (int i = 1; i < total; ++i)
                {
                    arrProp = argBindExpress[i].Split(arrSp, StringSplitOptions.RemoveEmptyEntries);
                    if (arrProp.Length != 2)
                    {
                        continue;
                    }
                    switch ( arrProp[0] )
                    {
                        case UIServiceCfgDefines.s_nameAttri:
                            {
                                argBindElement = arrProp[1];
                            }
                            break;

                        case UIServiceCfgDefines.s_PathAttri:
                            {
                                argBindProp = arrProp[1];
                            }
                            break;

                        case UIServiceCfgDefines.s_modeAttri:
                            {
                                int nTemp = 0;
                                if ( int.TryParse( arrProp[1], out nTemp ) )
                                {
                                    switch ( nTemp )
                                    {
                                        case 1:
                                            {
                                                argMode = BindingMode.OneWay;
                                            }
                                            break;

                                        case 2:
                                            {
                                                argMode = BindingMode.TwoWay;
                                            }
                                            break;

                                        case 3:
                                            {
                                                argMode = BindingMode.SoureTarget;
                                            }
                                            break;
                                    }
                                }
                            }
                            break;
                    }
                }

                if (string.IsNullOrEmpty(argBindElement) ||
                     string.IsNullOrEmpty(argBindProp))
                {
                    return null;
                }
            }

            return null;
            
        }

        protected string QueryResourcePath( string argRes )
        {
            Debug.Assert(!string.IsNullOrEmpty(argRes));

            if ( argRes.StartsWith("(", StringComparison.OrdinalIgnoreCase) &&
                 argRes.EndsWith( ")", StringComparison.OrdinalIgnoreCase ) )
            {
                string imgRes = null;
                string temp = argRes.Trim(new char[] { '(', ')', ' ' });
                if ( string.IsNullOrEmpty(temp) )
                {
                    return null;
                }

                if (m_parent.ResourceManager.CurrentUIResource.QueryImagePath(temp, out imgRes))
                {
                    return imgRes;
                }

                return argRes;
            }
            else
            {
                return argRes;
            }
        }

        private void UpdateBindingExpress( BindingExpress argExpress,
                                           bool argUpdate )
        {
            Debug.Assert(null != argExpress);

            argExpress.UpdateData(argUpdate);
        }

        protected void UpdateBindingExpress( string argProp,
                                             bool argUpdate )
        {
            Debug.Assert(!string.IsNullOrEmpty(argProp));

            BindingExpress express = null;
            foreach ( var item in m_dicBindingExpress )
            {
                if ( item.Value.PropertyOfElement.Equals( argProp, StringComparison.OrdinalIgnoreCase ) )
                {
                    express = item.Value;
                    break;
                }
            }

            if ( null != express )
            {
                UpdateBindingExpress(express, argUpdate);
            }
        }
#endregion

#region property
        public bool IsVisible
        {
            get
            {
                object visible = false;
                GetPropertyValue(UIPropertyKey.s_VisibleKey, out visible);

                return (bool)visible;
            }
        }

        public virtual bool CanFocus
        {
            get
            {
                return false;
            }
        }

        public bool IsEnable
        {
            get
            {
                object visible = false;
                GetPropertyValue(UIPropertyKey.s_EnableKey, out visible);

                return (bool)visible;
            }
        }

        public object Key
        {
            get
            {
                object key = null;
                GetPropertyValue(UIPropertyKey.s_tagKey, out key);

                return key;
            }
        }

        public HtmlElement HostedElement
        {
            get
            {
                return m_ihostedElement;
            }
        }

        protected UpdateDataHandler UpdateData
        {
            get 
            {
                if ( null == m_updatDataHandler )
                {
                    m_updatDataHandler = (UpdateDataHandler)Delegate.CreateDelegate(typeof(UpdateDataHandler), this, "UpdateBindingExpress");
                }

                return m_updatDataHandler;
            }
        }
#endregion

#region field
        protected HtmlElement m_ihostedElement = null;

        protected HtmlRender m_parent = null;

        protected string m_oldDisplayState = string.Empty;

        protected bool m_isVisible = true;

        protected bool m_isEnable = true;

        protected UpdateDataHandler m_updatDataHandler = null;

        protected Dictionary<string, BindingExpress> m_dicBindingExpress = new Dictionary<string,BindingExpress>();

        public const string s_leftDelimit = "{";

        public const string s_rightDelimit = "}";

        public const string s_binding = "binding";

        public const string s_elementBinding = "elementBinding";
#endregion
    }
}
