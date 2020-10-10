/********************************************************************
	FileName:   PropertyTrigger
    purpose:	

	author:		huang wei
	created:	2013/01/13

    revised history:
	2013/01/13  

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
using UIServiceInWPF.action;
using UIServiceInWPF;
using UIServiceInWPF.screen;
using System.Xml;

namespace UIServiceInWPF.trigger
{
    class PropertyTrigger : UITriggerBase
    {
#region constructor
        public PropertyTrigger(Screen argScreen,
                               string argElementName,
                               string argProperty,
                               string argCondition ) : base( argScreen )
        {
            Debug.Assert(!string.IsNullOrEmpty(argElementName) && !string.IsNullOrEmpty(argProperty) && !string.IsNullOrEmpty(argProperty));
            m_elementName = argElementName;
            m_elementProperty = argProperty;
            m_conditionValue = argCondition;
            if ( argCondition.StartsWith("!") )
            {
                m_isEqual = false;
                m_conditionValue = m_conditionValue.TrimStart('!');
            }            
        }
#endregion

#region method
        public override bool Initialize(System.Xml.XmlNode argNode)
        {
            if ( !base.Initialize(argNode) )
            {
                return false;
            }

            XmlAttribute triggerCountAttri = argNode.Attributes[s_TriggerCountAttri];
            if ( null != triggerCountAttri &&
                 !string.IsNullOrEmpty(triggerCountAttri.Value) )
            {
                int temp = 0;
                if ( int.TryParse( triggerCountAttri.Value, out temp ) &&
                     temp > 1 )
                {
                    m_maxTriggerCount = temp;
                }
            }

            return base.Initialize(argNode);
        }

        public override bool CanTrigger(string argElementName, 
                                        string argProperty, 
                                        string argNewValue)
        {
            Debug.Assert(!string.IsNullOrEmpty(argElementName) && !string.IsNullOrEmpty(argProperty));
            if ( m_elementName.Equals(argElementName, StringComparison.OrdinalIgnoreCase) &&
                 m_elementProperty.Equals( argProperty, StringComparison.OrdinalIgnoreCase ) )
            {
                if ( m_conditionValue.Equals(argNewValue, StringComparison.Ordinal ) )
                {
                    if ( m_isEqual )
                    {
                        if ( m_currentTriggerCount < m_maxTriggerCount )
                        {
                            m_currentTriggerCount += 1;
                            return true;
                        }
                    }
                    else
                    {
                        if ( m_currentTriggerCount > 0 )
                        {
                            m_currentTriggerCount -= 1;
                        }
                    }
                }
                else
                {
                    if ( !m_isEqual )
                    {
                        if ( m_currentTriggerCount < m_maxTriggerCount )
                        {
                            m_currentTriggerCount += 1;
                            return true;
                        }
                    }
                    else
                    {
                        if ( m_currentTriggerCount > 0 )
                        {
                            m_currentTriggerCount -= 1;
                        }
                    }
                }
            }

            return false;
        }

        public override void Prepare()
        {
            base.Prepare();

            object value = null;
            if (m_owner.GetPropertyValue(m_elementName, m_elementProperty, out value))
            {
                if (value is string)
                {
                    if (CanTrigger(m_elementName, m_elementProperty, (string)value))
                    {
                        DoAction();
                    }
                }
                else if (value is bool)
                {
                    if (CanTrigger(m_elementName, m_elementProperty, (bool)value ? "1" : "0"))
                    {
                        DoAction();
                    }
                }
                else
                {
                    if (CanTrigger(m_elementName, m_elementProperty, value.ToString()))
                    {
                        DoAction();
                    }
                }
            }
        }

        public override void Terminate()
        {
            base.Terminate();

            m_currentTriggerCount = 0;
        }
#endregion

#region field
        protected string m_elementName = null;

        protected string m_elementProperty = null;

        protected string m_conditionValue = null;

        protected bool m_isEqual = true;

        protected int m_maxTriggerCount = 1;

        protected int m_currentTriggerCount = 0;

        public const string s_TriggerCountAttri = "triggerCount";
#endregion
    }
}
