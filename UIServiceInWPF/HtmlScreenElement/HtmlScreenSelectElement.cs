/********************************************************************
	FileName:   HtmlScreenSelectElement
    purpose:	

	author:		huang wei
	created:	2013/02/17

    revised history:
	2013/02/17  

================================================================
    Copyright (C) 2013, Grgbanking CO,. Ltd. All rights reserved.
================================================================
********************************************************************/

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
    class HtmlScreenSelectElement : HtmlScreenElementBase
    {
        #region constructor
        public HtmlScreenSelectElement(HtmlElement argElement,
                                        HtmlRender argParent)
            : base(argElement, argParent)
        {

        }
        #endregion

        #region overide method
        public override bool SetPropertyValue(string argProperty, object argValue)
        {
            if (string.Equals(argProperty, UIPropertyKey.s_selectIndexKey, StringComparison.OrdinalIgnoreCase))
            {
                if (null == argValue ||
                     !(argValue is int))
                {
                    return false;
                }

                try
                {
                    IHTMLSelectElement iSelect = QuerySelectInterface();
                    iSelect.selectedIndex = (int)argValue;
                    iSelect = null;
                }
                catch (System.Exception ex)
                {
                    Log.UIService.LogWarn("Failed to set property value of a select element", ex);
                    return false;
                }

                return true;
            }
            else if (string.Equals(argProperty, UIPropertyKey.s_selectTitleKey, StringComparison.OrdinalIgnoreCase))
            {
                if (null == argValue ||
                     !(argValue is string))
                {
                    return false;
                }
                try
                {
                    IHTMLSelectElement iSelect = QuerySelectInterface();
                    string Title = (string)argValue;
                    int index = 0;
                    foreach (IHTMLOptionElement option in iSelect.options)
                    {
                        if (string.Equals(option.text, Title, StringComparison.Ordinal))
                        {
                            iSelect.selectedIndex = index;
                            break;
                        }
                        ++index;
                    }
                    iSelect = null;
                }
                catch (System.Exception ex)
                {
                    Log.UIService.LogWarn("Failed to set property value of a select element", ex);
                    return false;
                }

                return true;
            }
            else if (string.Equals(argProperty, UIPropertyKey.s_selectValueKey, StringComparison.OrdinalIgnoreCase))
            {
                if (null == argValue ||
                     !(argValue is string))
                {
                    return false;
                }
                try
                {
                    IHTMLSelectElement iSelect = QuerySelectInterface();
                    string opValue = (string)argValue;
                    int index = 0;
                    foreach (IHTMLOptionElement option in iSelect.options)
                    {
                        if (string.Equals(option.value, opValue, StringComparison.Ordinal))
                        {
                            iSelect.selectedIndex = index;
                            break;
                        }
                        ++index;
                    }
                    iSelect = null;
                }
                catch (System.Exception ex)
                {
                    Log.UIService.LogWarn("Failed to set property value of a select element", ex);
                    return false;
                }

                return true;
            }
            else if (string.Equals(argProperty, UIPropertyKey.s_ItemSourceKey, StringComparison.OrdinalIgnoreCase))
            {
                if (null == argValue ||
                     !(argValue is UISelectOptionsCollection))
                {
                    return false;
                }

                try
                {
                    UISelectOptionsCollection options = (UISelectOptionsCollection)argValue;
                    HtmlElement element = null;
                    foreach (var item in options.Options)
                    {
                        element = m_ihostedElement.Document.CreateElement("Option");
                        if (null != element)
                        {
                            element.InnerText = item.Title;
                            if (!string.IsNullOrEmpty(item.Value))
                            {
                                element.SetAttribute("value", item.Value);
                            }

                            m_ihostedElement.AppendChild(element);
                        }
                    }
                }
                catch (Exception exp)
                {
                    Trace.Write(exp.Message);
                    return false;
                }

                return true;
            }
            else
            {
                return base.SetPropertyValue(argProperty, argValue);
            }
        }

        public override bool GetPropertyValue(string argProperty, out object argValue)
        {
            argValue = null;
            if (string.Equals(argProperty, UIPropertyKey.s_selectIndexKey, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    IHTMLSelectElement iSelect = QuerySelectInterface();
                    argValue = iSelect.selectedIndex;
                    iSelect = null;
                }
                catch (System.Exception ex)
                {
                    Log.UIService.LogWarn("Failed to set property value of a select element", ex);
                    return false;
                }

                return true;
            }
            else if (string.Equals(argProperty, UIPropertyKey.s_selectTitleKey, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    IHTMLSelectElement iSelect = QuerySelectInterface();
                    IHTMLOptionElement option = iSelect.options[iSelect.selectedIndex];
                    if (null == option)
                    {
                        iSelect = null;
                        return false;
                    }

                    argValue = option.text;
                    option = null;
                    iSelect = null;
                }
                catch (System.Exception ex)
                {
                    Log.UIService.LogWarn("Failed to set property value of a select element", ex);
                    return false;
                }

                return true;
            }
            else if (string.Equals(argProperty, UIPropertyKey.s_selectValueKey, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    IHTMLSelectElement iSelect = QuerySelectInterface();
                    IHTMLOptionElement option = iSelect.options[iSelect.selectedIndex];
                    if (null == option)
                    {
                        iSelect = null;
                        return false;
                    }

                    argValue = option.value;
                    option = null;
                    iSelect = null;
                }
                catch (System.Exception ex)
                {
                    Log.UIService.LogWarn("Failed to set property value of a select element", ex);
                    return false;
                }

                return true;
            }
            else
            {
                return base.GetPropertyValue(argProperty, out argValue);
            }
        }

        public override void ParseBindingExpress()
        {
            var mode = BindingMode.OneWay;
            //check item source property
            var propValue = ParseBindedProperty(UIPropertyKey.s_ItemSourceKey, m_ihostedElement, ref mode);
            if (!string.IsNullOrEmpty(propValue))
            {
                m_dicBindingExpress.Add(propValue, new BindingExpress(propValue, UIPropertyKey.s_ItemSourceKey, this)
                {
                    Mode = mode
                });
            }

            //check selectIndex property
            propValue = ParseBindedProperty(UIPropertyKey.s_selectIndexKey, m_ihostedElement, ref mode);
            if (!string.IsNullOrEmpty(propValue))
            {
                m_dicBindingExpress.Add(propValue, new BindingExpress(propValue, UIPropertyKey.s_selectIndexKey, this)
                {
                    Mode = mode
                });
            }

            //check selectValue property
            propValue = ParseBindedProperty(UIPropertyKey.s_selectValueKey, m_ihostedElement, ref mode);
            if (!string.IsNullOrEmpty(propValue))
            {
                m_dicBindingExpress.Add(propValue, new BindingExpress(propValue, UIPropertyKey.s_selectValueKey, this)
                {
                    Mode = mode
                });
            }

            //check selectTitle property
            propValue = ParseBindedProperty(UIPropertyKey.s_selectTitleKey, m_ihostedElement, ref mode);
            if (!string.IsNullOrEmpty(propValue))
            {
                m_dicBindingExpress.Add(propValue, new BindingExpress(propValue, UIPropertyKey.s_selectTitleKey, this)
                {
                    Mode = mode
                });
            }

            base.ParseBindingExpress();
        }

        private IHTMLSelectElement QuerySelectInterface()
        {
            Debug.Assert(null != m_ihostedElement);
            IHTMLSelectElement iSelect = (IHTMLSelectElement)m_ihostedElement.DomElement;
            if (null == iSelect)
            {
                return null;
            }

            return iSelect;
        }
        #endregion

        #region field

        #endregion
    }
}
