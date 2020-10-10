using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using UIServiceProtocol;
using mshtml;
using LogProcessorService;

namespace UIServiceInWPF.HtmlScreenElement
{
    class HtmlScreenButtonElement : HtmlScreenElementBase
    {
#region constructor
        public HtmlScreenButtonElement( HtmlElement argElement,
                                        HtmlRender argParent)
            : base(argElement, argParent)
        {
            string typeAttri = argElement.GetAttribute("type");
            if (UIServiceCfgDefines.s_checkbox.Equals(typeAttri, StringComparison.OrdinalIgnoreCase) ||
                 UIServiceCfgDefines.s_radio.Equals(typeAttri, StringComparison.OrdinalIgnoreCase))
            {
                if (argElement.TagName.Equals("input", StringComparison.OrdinalIgnoreCase))
                {
                    m_iButtonElement = (IHTMLOptionButtonElement)argElement.DomElement;
                }
                m_isStateButton = true;
            }

            if ( typeAttri.Equals( UIServiceCfgDefines.s_checkbox, StringComparison.OrdinalIgnoreCase ) ||  
                 typeAttri.Equals( UIServiceCfgDefines.s_radio, StringComparison.OrdinalIgnoreCase ) ||
                 typeAttri.Equals( UIServiceCfgDefines.s_submitButton, StringComparison.OrdinalIgnoreCase ) ||
                 typeAttri.Equals( UIServiceCfgDefines.s_resetButton, StringComparison.OrdinalIgnoreCase ) )
            {
                if (argElement.TagName.Equals("input", StringComparison.OrdinalIgnoreCase))
                {
                    m_ihostedElement.Click += m_ihostedElement_Click;
                }          
            }

            string maskChar = argElement.GetAttribute("maskChar");
            if (!string.IsNullOrWhiteSpace(maskChar))
            {
                m_maskChar = maskChar;
            }
        }
#endregion

#region override method
        public override void InnerDispose()
        {
            if ( m_ihostedElement.TagName.Equals("input", StringComparison.OrdinalIgnoreCase) )
            {
                m_ihostedElement.Click -= m_ihostedElement_Click;
            }
            
            m_iButtonElement = null;

            base.InnerDispose();
        }

        public override bool GetPropertyValue(string argProperty, out object argValue)
        {
            Debug.Assert(!string.IsNullOrEmpty(argProperty));
            argValue = null;

            try
            {
                if (argProperty.Equals(UIPropertyKey.s_stateKey, StringComparison.OrdinalIgnoreCase) &&
                    m_isStateButton)
                {
                    if (null != m_iButtonElement)
                    {
                        argValue = m_iButtonElement.status;
                    }
                    return true;
                }
                else if (argProperty.Equals(UIPropertyKey.s_ContentKey, StringComparison.OrdinalIgnoreCase))
                {
                    if (m_ihostedElement.TagName.Equals("input", StringComparison.OrdinalIgnoreCase))
                    {
                        argValue = m_ihostedElement.GetAttribute("value");
                    }
                    else
                    {
                        argValue = m_ihostedElement.InnerText;
                    }

                    return true;
                }
                else
                {
                    return base.GetPropertyValue(argProperty, out argValue);
                }
            }
            catch (System.Exception ex)
            {
                Log.UIService.LogError("Failed to get property from a button", ex);
                return false;
            }      
        }

        public override bool SetPropertyValue(string argProperty, object argValue)
        {
            Debug.Assert(!string.IsNullOrEmpty(argProperty));

            try
            {
                if (argProperty.Equals(UIPropertyKey.s_stateKey, StringComparison.OrdinalIgnoreCase) &&
                     m_isStateButton)
                {
                    if (null != m_iButtonElement && null != argValue)
                    {
                        if ((argValue is string) && !string.IsNullOrEmpty((string)argValue))
                        {
                            int temp = int.Parse((string)argValue);
                            m_iButtonElement.status = temp == 0 ? false : true;
                        }
                        else if (argValue is bool)
                        {
                            m_iButtonElement.status = (bool)argValue;
                        }
                    }
                    return true;
                }
                else if (argProperty.Equals(UIPropertyKey.s_ContentKey, StringComparison.OrdinalIgnoreCase))
                {
                    if (m_ihostedElement.TagName.Equals("input", StringComparison.OrdinalIgnoreCase))
                    {
                        if (null != argValue)
                        {
                            if (argValue is string)
                            {
                                m_ihostedElement.SetAttribute("value", (string)argValue);
                            }
                        }
                    }
                    return true;
                }

                return base.SetPropertyValue(argProperty, argValue);
            }
            catch (System.Exception ex)
            {
                Log.UIService.LogError("Failed to set property of a button", ex);
                return false;
            }         
        }

        public override void OnLeftMouseDown(object argSender, HtmlElementEventArgs arg)
        {
            base.OnLeftMouseDown(argSender, arg);

            Debug.Assert(null != m_ihostedElement);

            if ( !string.IsNullOrEmpty(m_downImgPath) )
            {
                m_bgElement.SetAttribute("src", m_downImgPath);
            }        
        }

        public override void OnLeftMouseUp(object argSender, HtmlElementEventArgs arg,
                                           bool argTriggerEvt = true )
        {
            if (!string.IsNullOrEmpty(m_upImgPath))
            {
                m_bgElement.SetAttribute("src", m_upImgPath);
            }

            base.OnLeftMouseUp(argSender, arg);

            if ( argTriggerEvt )
            {
                if (!string.IsNullOrEmpty(m_bindElementName))
                {
                    if (null != m_bindTargetElement)
                    {
                        m_bindTargetElement.SetPropertyValue(m_bindProp, Key);
                    }
                    else
                    {
                        m_bindTargetElement = m_parent.FindElement(m_ihostedElement.Id);
                        if (null != m_bindTargetElement)
                        {
                            m_bindTargetElement.SetPropertyValue(m_bindProp, Key);
                        }
                        else
                        {
                            //m_parent.OnButtonClicked(m_ihostedElement.Id, m_ihostedElement.GetAttribute(HtmlRender.s_tagAttri));
                            m_parent.OnButtonClicked(m_ihostedElement.Id, GetTagValue(), m_maskChar);
                        }
                    }
                }
                else
                {
                    //m_parent.OnButtonClicked(m_ihostedElement.Id, m_ihostedElement.GetAttribute(HtmlRender.s_tagAttri));
                    m_parent.OnButtonClicked(m_ihostedElement.Id, GetTagValue(), m_maskChar);
                }
            }
        }

        public override void ParseBindingExpress()
        {
            base.ParseBindingExpress();

            BindingMode mode = BindingMode.OneWay;
            string propvalue = ParseBindedProperty( UIPropertyKey.s_stateKey,
                                                    m_ihostedElement,
                                                    ref mode);
            if (!string.IsNullOrEmpty(propvalue))
            {
                m_dicBindingExpress.Add(propvalue, new BindingExpress(propvalue,
                                                                      UIPropertyKey.s_stateKey,
                                                                      this)
                {
                    Mode = mode
                });
            }

            ////load binding click
            //string clickAttri = m_ihostedElement.GetAttribute(UIPropertyKey.s_clickKey);
            //if ( !string.IsNullOrEmpty(clickAttri) )
            //{
            //    BindingMode mode = BindingMode.OneWay;
            //    ParseBindValue(clickAttri, ref m_bindElementName, ref m_bindProp, ref mode);
            //    //try find a element
            //    if ( !string.IsNullOrEmpty(m_bindElementName) &&
            //         !string.IsNullOrEmpty(m_bindProp) )
            //    {
            //        m_bindTargetElement = m_parent.FindElement(m_bindElementName);
            //    }              
            //}
        }

        private void m_ihostedElement_Click(object sender, HtmlElementEventArgs e)
        {
            e.BubbleEvent = false;

            if (!m_isStateButton)
            {
                //m_parent.OnButtonClicked(m_ihostedElement.Id, m_ihostedElement.GetAttribute(HtmlRender.s_tagAttri));
                m_parent.OnButtonClicked(m_ihostedElement.Id, GetTagValue(), m_maskChar);
            }
        }

        public override void ParseResource()
        {
            Debug.Assert(null != m_ihostedElement);

            HtmlElement bgElement = FindBgImgNode();
            if ( null == bgElement )
            {
                return;
            }
            m_bgElement = bgElement;

            string imgUpPath = bgElement.GetAttribute(s_btnUpAttri);
            string imgDwPath = bgElement.GetAttribute(s_btnDwAttri);
            if ( !string.IsNullOrEmpty(imgUpPath) )
            {
                m_upImgPath = QueryResourcePath(imgUpPath);
            }
            if ( !string.IsNullOrEmpty(imgDwPath) )
            {
                m_downImgPath = QueryResourcePath(imgDwPath);
            }
        }

        protected HtmlElement FindBgImgNode()
        {
            HtmlElement imgElement = null;
            Debug.Assert(null != m_ihostedElement);
            foreach ( HtmlElement item in m_ihostedElement.Children )
            {
                if ( null != item.Id && item.Id.Equals( s_bgimgFlag, StringComparison.OrdinalIgnoreCase ) )
                {
                    return item;
                }
            }

            return imgElement;
        }

        private object GetTagValue()
        {
            foreach (var item in m_dicBindingExpress)
            {
                if (item.Value.IsValid &&
                     item.Value.PropertyOfElement.Equals(UIPropertyKey.s_tagKey, StringComparison.Ordinal))
                {
                    return item.Value.Target.TargetValue;
                }
            }

            return m_ihostedElement.GetAttribute(HtmlRender.s_tagAttri);
        }
#endregion

#region field
        protected IHTMLOptionButtonElement m_iButtonElement = null;

        protected string m_maskChar = null;

        protected bool m_isStateButton = false;

        public const string s_btnUpAttri = "up";

        public const string s_btnDwAttri = "down";

        public const string s_bgimgFlag = "bgimg";

        public HtmlScreenElementBase m_bindTargetElement = null;

        public string m_bindElementName = null;

        public string m_bindProp = null;

        protected HtmlElement m_bgElement = null;

        protected string m_upImgPath = null;

        protected string m_downImgPath = null;
#endregion
    }
}
