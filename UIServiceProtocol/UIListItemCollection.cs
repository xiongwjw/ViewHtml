/********************************************************************
	FileName:   UIListItemCollection
    purpose:	

	author:		huang wei
	created:	2013/03/30

    revised history:
	2013/03/30  

================================================================
    Copyright (C) 2013, Grgbanking CO,. Ltd. All rights reserved.
================================================================
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;

namespace UIServiceProtocol
{
    public class UIListItem : INotifyPropertyChanged,
                              IDisposable
    {
#region constructor
        public UIListItem()
        {

        }
#endregion

#region method
        public void Dispose()
        {
            m_dicKeyvalues.Clear();
            m_dicKeyvalues = null;
        }

        public object GetBindData( string argKey )
        {
            if ( string.IsNullOrEmpty(argKey) )
            {
                return null;
            }

            object value = null;
            if (m_dicKeyvalues.TryGetValue(argKey, out value))
            {
                return value;
            }

            return null;
        }

        public bool SetBindData(string argKey,
                         object argValue)
        {
            if (string.IsNullOrEmpty(argKey))
            {
                return false;
            }

            m_dicKeyvalues[argKey] = argValue;

            return true;
        }

        public void AddKeyValue( string argKey,
                                 string argValue )
        {
            AddKeyValue(argKey, (object)argValue);
        }

        public void AddKeyValue( string argKey,
                                 object argValue)
        {
            Debug.Assert(!string.IsNullOrEmpty(argKey));

            if (m_dicKeyvalues.ContainsKey(argKey))
            {
                Trace.TraceWarning("There is a duplicate key[{0}]", argKey);
            }
            else
            {
                m_dicKeyvalues.Add(argKey, argValue);
            }
        }

        public void ChangeKeyValue( string argKey,
                                    string argValue )
        {
            Debug.Assert(!string.IsNullOrEmpty(argKey));

            if (m_dicKeyvalues.ContainsKey(argKey) &&
                 !string.Equals((string)m_dicKeyvalues[argKey], argValue, StringComparison.Ordinal))
            {
                m_dicKeyvalues[argKey] = argValue;
                OnPropertyChanged(argKey);
            }
        }

        public void ChangeKeyValue( string argKey,
                                    object argValue)
        {
            Debug.Assert(!string.IsNullOrEmpty(argKey));
            
            if ( m_dicKeyvalues.ContainsKey(argKey) )
            {
                m_dicKeyvalues[argKey] = argValue;
                OnPropertyChanged(argKey);
            }
        }

        public void RemoveKey( string argkey )
        {
            Debug.Assert(!string.IsNullOrEmpty(argkey));
            m_dicKeyvalues.Remove(argkey);
        }

        public void Clear()
        {
            m_dicKeyvalues.Clear();
        }
#endregion

#region event
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged( string argKey )
        {
            Debug.Assert(!string.IsNullOrEmpty(argKey));
            if ( null != PropertyChanged )
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs( argKey ));
            }
        }
#endregion

#region field
        //protected Dictionary<string, string> m_dicKeyvalues = new Dictionary<string, string>();
        protected Dictionary<string, object> m_dicKeyvalues = new Dictionary<string, object>();
#endregion
    }

    public class UIListItemCollection : IDisposable
    {
#region constructor
        public UIListItemCollection( string argName )
        {
            Debug.Assert(!string.IsNullOrEmpty(argName));
            m_name = argName;
        }
#endregion

#region method
        public void Dispose()
        {
            Clear();
            m_listItems = null;
        }

        public void Clear()
        {
            foreach (var item in m_listItems)
            {
                item.Dispose();
            }
            m_listItems.Clear();
        }
#endregion

#region property
        public List<UIListItem> Items
        {
            get
            {
                return m_listItems;
            }
        }

        public string Name
        {
            get
            {
                return m_name;
            }
        }
#endregion

#region field
        protected List<UIListItem> m_listItems = new List<UIListItem>();

        protected string m_name = null;
#endregion
    }
}
