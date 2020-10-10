using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIServiceInWPF.action;
using UIServiceInWPF.screen;
using System.Diagnostics;
using System.Windows;
using System.Xml;

namespace UIServiceInWPF.trigger
{
    public class UITriggerBase
    {
#region Constructor
        public UITriggerBase( Screen argOwner )
        {
            Debug.Assert(null != argOwner);
            m_owner = argOwner;
        }
#endregion

#region virtual method
        public virtual bool Initialize( XmlNode argNode )
        {
            Debug.Assert(null != argNode);
            //bubbleEvent
            XmlAttribute bubbleEventAttri = argNode.Attributes[UIServiceCfgDefines.s_bubbleEventAttri];
            if (null != bubbleEventAttri &&
                 !string.IsNullOrEmpty(bubbleEventAttri.Value))
            {
                int temp = 0;
                if (int.TryParse(bubbleEventAttri.Value, out temp))
                {
                    m_bubbleEvent = temp == 0 ? false : true;
                }
            }
            //ApplyTo
            XmlAttribute applyToAttri = argNode.Attributes[UIServiceCfgDefines.s_ApplyToAttri];
            if ( null != applyToAttri &&
                 !string.IsNullOrEmpty(applyToAttri.Value) )
            {
                string[] arrScreens = applyToAttri.Value.Split(new char[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
                if ( null != arrScreens &&
                     arrScreens.Length > 0 )
                {
                    m_listApplyToScreens = new List<string>();
                    foreach (var item in arrScreens)
                    {
                        if ( item.Equals( "ALL", StringComparison.Ordinal ) )
                        {
                            m_listApplyToScreens.Clear();
                            m_listApplyToScreens = null;
                            break;
                        }
                        m_listApplyToScreens.Add(item);
                    }
                }
            }

            return true;
        }

        public virtual void Exit()
        {
            foreach ( var action in m_listActions )
            {
                action.Clear();
            }
            m_listActions.Clear();

            if ( null != m_listApplyToScreens )
            {
                m_listApplyToScreens.Clear();
                m_listApplyToScreens = null;
            }
        }

        public virtual void Prepare()
        {

        }

        public virtual void Terminate()
        {
            foreach ( var action in m_listActions )
            {
                action.Terminate();
            }
        }

        public virtual bool CanTrigger( string strEvent )
        {
            return false;
        }

        public virtual bool CanTrigger( string argElementName,
                                        string argProperty,
                                        string argNewValue )
        {
            return false;
        }

        public virtual bool CanApply( string argScrName )
        {
            if ( string.IsNullOrEmpty(argScrName) )
            {
                return true;
            }

            if ( null != m_listApplyToScreens )
            {
                return m_listApplyToScreens.Exists(item => item.Equals(argScrName, StringComparison.Ordinal));
            }

            return true;
        }

        public virtual void DoAction()
        {
            foreach (var action in m_listActions)
            {
                action.Do();
            }
        }

        public virtual void UndoAction()
        {
            foreach (var action in m_listActions)
            {
                action.UnDo();
            }
        }
#endregion

#region method
        public void AddAction( ActionBase objAct )
        {
            Debug.Assert(null != objAct);

            m_listActions.Add(objAct);
        }
#endregion

#region property
        public Screen Owner
        {
            get
            {
                return m_owner;
            }
        }

        public bool Empty
        {
            get
            {
                return m_listActions.Count == 0 ? true : false;
            }
        }

        public bool BubbleEvent
        {
            get
            {
                return m_bubbleEvent;
            }
        }
#endregion

#region field
        protected List<ActionBase> m_listActions = new List<ActionBase>();

        protected List<string> m_listApplyToScreens = null;

        protected Screen m_owner = null;

        protected bool m_bubbleEvent = true;
#endregion
    }
}
