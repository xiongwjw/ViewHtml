/********************************************************************
	FileName:   BindingExpress
    purpose:	

	author:		huang wei
	created:	2012/11/09

    revised history:
	2012/11/09  

================================================================
    Copyright (C) 2012, Grgbanking CO,. Ltd. All rights reserved.
================================================================
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIServiceInWPF.HtmlScreenElement;
using Attribute4ECAT;
using System.Diagnostics;
using System.ComponentModel;
using System.Reflection;
using LogProcessorService;
using System.Windows;
using UIServiceInWPF.screen;

namespace UIServiceInWPF
{
    public enum BindingMode : byte
    {
        OneWay = 1,
        TwoWay = 2,
        SoureTarget = 3
    }

    public class BindedTarget
    {
#region constructor
        public BindedTarget( PropertyInfo argInfo,
                             GrgBindTargetAttribute argAttri )
        {
            //Debug.Assert( null != argInfo );
            m_propInfo = argInfo;
            m_type = argAttri.Type;
            m_accessRight = argAttri.Access;
        }

        public BindedTarget( MethodInfo argGetMethodInfo,
                             MethodInfo argSetMethodInfo,
                             string argDataKey )
        {
            Debug.Assert(null != argGetMethodInfo && null != argSetMethodInfo && !string.IsNullOrEmpty(argDataKey));

            m_setMethodInfo = argSetMethodInfo;
            m_getMethodInfo = argGetMethodInfo;

            m_arrGetParam = new string[] { argDataKey };
            m_arrSetParam = new object[2]
            {
                argDataKey,
                null
            };
        }
#endregion

#region method
        public void Close()
        {
            m_target = null;
            m_propInfo = null;
            m_getMethodInfo = null;
            m_setMethodInfo = null;
            m_arrGetParam = null;
            m_arrSetParam = null;
        }
#endregion

#region property
        //public TargetType Type
        //{
        //    set
        //    {
        //        m_type = value;
        //    }
        //}

        public object Target
        {
            set 
            {
                Debug.Assert(null != value);
                m_target = value;
            }
            get
            {
                return m_target;
            }
        }

        public AccessRight Access
        {
            set
            {
                m_accessRight = value;
            }
            get
            {
                return m_accessRight;
            }
        }

        public object TargetValue
        {
            get
            {
                //if ( null == m_target ||
                //     AccessRight.OnlyWrite == m_accessRight )
                //{
                //    return null;
                //}
                if ( null == m_target )
                {
                    return null;
                }

                if ( null != m_propInfo )
                {
                   return m_propInfo.GetValue(m_target, null);
                }
                else if ( null != m_getMethodInfo )
                {
                    return m_getMethodInfo.Invoke(m_target, m_arrGetParam);
                }

                return null;
            }
            set
            {
                //if ( null == m_target ||
                //     AccessRight.OnlyRead == m_accessRight )
                //{
                //    return;
                //}
                if ( null == m_target)
                {
                    return;
                }

                if ( null != m_propInfo )
                {
                    if ( Type.GetTypeCode( m_propInfo.PropertyType ) != Type.GetTypeCode(value.GetType()) )
                    {

                    }
                    else
                    {
                        m_propInfo.SetValue(m_target, value, null);
                    }
                    
                }
                else if ( null != m_setMethodInfo )
                {
                    m_arrSetParam[1] = value;
                    m_setMethodInfo.Invoke(m_target, m_arrSetParam);
                }
            }
        }


#endregion

#region field
        private TargetType m_type = TargetType.Int;

        private AccessRight m_accessRight = AccessRight.ReadAndWrite;

        private object m_target = null;

        private PropertyInfo m_propInfo = null;

        private MethodInfo m_getMethodInfo = null;

        private MethodInfo m_setMethodInfo = null;

        private string[] m_arrGetParam = null;

        private object[] m_arrSetParam = null;
#endregion
    }

    public class BindingExpress
    {
#region constructor
        public BindingExpress( string argBindedProperty,
                               string argProperty,
                               HtmlScreenElementBase argElement )
        {
            Debug.Assert(!string.IsNullOrEmpty(argProperty) && !string.IsNullOrEmpty(argBindedProperty) && null != argElement);

            m_bindedProperty = argBindedProperty;
            m_property = argProperty;
            m_element = argElement;
        }
#endregion

#region property
        //public BindedTarget Target
        //{
        //    set
        //    {
        //        m_target = value;
        //    }
        //}
        public object DataContent
        {
            set
            {
                if ( string.IsNullOrEmpty(m_bindedProperty) )
                {
                    return;
                }
                if ( value == null )
                {
                    if ( null != m_target )
                    {
                        m_target.Close();
                        m_target = null;
                    }
                    m_element.SetPropertyValue(m_property, null);

                    return;
                }

                //if ( null != m_target &&
                //     m_target.Target == value )
                //{
                //    return;
                //}

                PropertyInfo Prop = value.GetType().GetProperty(m_bindedProperty, BindingFlags.Public | BindingFlags.Instance);
                if (null != Prop)
                {
                    GrgBindTargetAttribute[] attri = (GrgBindTargetAttribute[])Prop.GetCustomAttributes(typeof(GrgBindTargetAttribute), true);
                    if ( attri.Length == 0 )
                    {
                        return;
                    }

                    m_target = new BindedTarget(Prop, attri[0])
                    {
                        Target = value
                    };
                    attri = null;

                    if ( Mode == BindingMode.SoureTarget )
                    {
                        UpdateData(true);
                    }
                    else
                    {
                        UpdateData(false);
                    } 
                }
                else
                {
                    MethodInfo getMethodInfo = value.GetType().GetMethod(s_getMethodName, BindingFlags.Instance | BindingFlags.Public);
                    MethodInfo setMethodInfo = value.GetType().GetMethod(s_setMethodName, BindingFlags.Instance | BindingFlags.Public);
                    if ( null == getMethodInfo ||   
                         null == setMethodInfo )
                    {
                        Log.UIService.LogError("Failed to look up binded method");
                        return;
                    }

                    m_target = new BindedTarget(getMethodInfo, setMethodInfo, m_bindedProperty)
                    {
                        Target = value
                    };

                    if (Mode == BindingMode.SoureTarget)
                    {
                        UpdateData(true);
                    }
                    else
                    {
                        UpdateData(false);
                    }
                }
            }
        }

        public bool IsValid
        {
            get
            {
                return null != m_target ? true : false;
            }
        }

        public string BindedProperty
        {
            get
            {
                return m_bindedProperty;
            }
        }

        public BindingMode Mode
        {
            get;
            set;
        }

        public string PropertyOfElement
        {
            get
            {
                return m_property;
            }
        }

        public BindedTarget Target
        {
            get
            {
                return m_target;
            }
        }
#endregion

#region method
        public void UpdateData( bool argSave )
        {
            try
            {
                if (argSave)
                {
                    if (null == m_target)
                    {
                        return;
                    }
                    //if (m_target.Access != AccessRight.OnlyRead)
                    if ( Mode != BindingMode.OneWay )
                    {
                        object value = null;
                        bool result = m_element.GetPropertyValue(m_property, out value);
                        if (!result)
                        {
                            LogProcessorService.Log.UIService.LogWarnFormat("Failed to get value of property[{0}] of the element[{1}]", m_property, m_element.HostedElement.Id);
                        }
                        else
                        {
                            m_target.TargetValue = value;
                            m_element.OnValueChange(m_property, value);
                        }
                    }
                }
                else
                {
                   // if (AccessRight.OnlyWrite != m_target.Access)
                    if ( Mode != BindingMode.SoureTarget )
                    {
                        bool result = m_element.SetPropertyValue(m_property, m_target.TargetValue);
                        if (!result)
                        {
                            LogProcessorService.Log.UIService.LogWarnFormat("Failed to set value of property[{0}] of the element[{1}]", m_property, m_element.HostedElement.Id);
                        }
                        else
                            m_element.OnValueChange(m_property, m_target.TargetValue);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Trace.Write(ex.Message);
                //LogProcessorService.Log.UIService.LogWarn("Failed to update data", ex);	
            }
        }

        public void Close()
        {
            if ( null != m_target )
            {
                m_target.Close();
            }

            m_element = null;
        }
#endregion

#region field
        protected BindedTarget m_target = null;

        protected string m_property = null;

        protected string m_bindedProperty = null;

        protected HtmlScreenElementBase m_element = null; 

        public const string s_getMethodName = "GetBindData";

        public const string s_setMethodName = "SetBindData";
#endregion
    }

    public class WPFBindingExpress
    {
        public WPFBindingExpress( string argBindedProperty,
                                  string argProperty,
                                  FrameworkElement argElement )
        {
            Debug.Assert(null != argElement);
            m_bindedProperty = argBindedProperty;
            m_property = argProperty;
            m_element = argElement;
        }

#region property
        public bool IsValid
        {
            get
            {
                return null != m_target ? true : false;
            }
        }

        public string BindedProperty
        {
            get
            {
                return m_bindedProperty;
            }
        }

        public BindingMode Mode
        {
            get;
            set;
        }

        public string PropertyOfElement
        {
            get
            {
                return m_property;
            }
        }

        public BindedTarget Target
        {
            get
            {
                return m_target;
            }
        }

        public object DataContent
        {
            set
            {
                if (string.IsNullOrEmpty(m_bindedProperty))
                {
                    return;
                }
                if (value == null)
                {
                    if (null != m_target)
                    {
                        m_target.Close();
                        m_target = null;
                    }
                    Screen.SetPropertyValue(m_element, m_property, null);
                    return;
                }

                MethodInfo getMethodInfo = value.GetType().GetMethod(BindingExpress.s_getMethodName, BindingFlags.Instance | BindingFlags.Public);
                MethodInfo setMethodInfo = value.GetType().GetMethod(BindingExpress.s_setMethodName, BindingFlags.Instance | BindingFlags.Public);
                if ( null == getMethodInfo ||
                     null == setMethodInfo)
                {
                    //Log.UIService.LogWarn("Failed to look up binded method");
                    return;
                }
                m_target = new BindedTarget(getMethodInfo, setMethodInfo, m_bindedProperty)
                {
                    Target = value
                };

                if (Mode == BindingMode.SoureTarget)
                {
                    UpdateData(true);
                }
                else
                {
                    UpdateData(false);
                }
            }
        }
#endregion

#region method
        public void UpdateData( bool argSave )
        {
            try
            {
                if (argSave)
                {
                    if (null == m_target)
                    {
                        return;
                    }
                    //if (m_target.Access != AccessRight.OnlyRead)
                    if (Mode != BindingMode.OneWay)
                    {
                        object value = null;
                        bool result = Screen.GetPropertyValue(m_element, m_property, out value);
                        if (!result)
                        {
                           // LogProcessorService.Log.UIService.LogWarnFormat("Failed to get value of property[{0}] of the element[{1}]", m_property, m_element.Name);
                        }
                        else
                        {
                            m_target.TargetValue = value;
                        }
                    }
                }
                else
                {
                    // if (AccessRight.OnlyWrite != m_target.Access)
                    if (Mode != BindingMode.SoureTarget)
                    {
                        bool result = Screen.SetPropertyValue(m_element, m_property, m_target.TargetValue);
                        if (!result)
                        {
                            //LogProcessorService.Log.UIService.LogWarnFormat("Failed to set value of property[{0}] of the element[{1}]", m_property, m_element.Name);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Trace.Write(ex.Message);
               // LogProcessorService.Log.UIService.LogWarn("Failed to update data", ex);
            }
        }

        public void Close()
        {
            if (null != m_target)
            {
                m_target.Close();
            }

            m_element = null;
        }
#endregion

#region field
        protected BindedTarget m_target = null;

        protected string m_property = null;

        protected string m_bindedProperty = null;

        protected FrameworkElement m_element = null;
#endregion
    }

    public class ElementBindingExpress
    {
#region constructor
        public ElementBindingExpress( string argBindElementName,
                                      string argBindProperty,
                                      string argProperty,
                                      HtmlScreenElementBase argElement )
        {
            Debug.Assert(!string.IsNullOrEmpty(argBindElementName) && 
                         !string.IsNullOrEmpty(argBindProperty) && 
                         !string.IsNullOrEmpty(argProperty) &&
                         null != argElement );

            m_bindElementName = argBindElementName;
            m_bindProperty = argBindProperty;
            m_property = argProperty;
            m_element = argElement;
        }
#endregion

#region method
        public void SetBindValue( string argBindProperty,
                                  string argValue )
        {
            Debug.Assert(!string.IsNullOrEmpty(argBindProperty) &&
                          string.Equals(argBindProperty, m_bindProperty, StringComparison.OrdinalIgnoreCase) &&
                          null != m_element );

            m_element.SetPropertyValue(m_property, argValue);
        }

        public bool CanTrigger( string argBindElementName,
                                string argBindProperty )
        {
            if ( string.Equals(argBindElementName, m_bindElementName, StringComparison.Ordinal) &&
                 string.Equals(argBindProperty, m_bindProperty, StringComparison.Ordinal) )
            {
                return true;
            }

            return false;
        }

        public void Close()
        {
            m_element = null;
        }
#endregion

#region property
        public string BindedProperty
        {
            get
            {
                return m_bindProperty;
            }
        }
#endregion

#region field
        protected string m_bindElementName = null;

        protected string m_bindProperty = null;

        protected string m_property = null;

        protected HtmlScreenElementBase m_element = null;
#endregion
    }
}
