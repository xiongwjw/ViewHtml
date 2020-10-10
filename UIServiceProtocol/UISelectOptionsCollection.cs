using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;

namespace UIServiceProtocol
{
    public class UISelectOptionsCollection
    {
        #region
        public UISelectOptionsCollection()
        {
        }
        #endregion

        public void AddOption( string argTitle,
                               string argValue )
        {
            Debug.Assert( !string.IsNullOrEmpty(argTitle) );
            if ( string.IsNullOrEmpty( argTitle ) )
            {
                return;
            }

            m_listOpts.Add(new OptionItem()
                {
                    Title = argTitle,
                    Value = argValue
                });
        }

        public List<OptionItem> Options
        {
            get
            {
                return m_listOpts;
            }
        }

        private List<OptionItem> m_listOpts = new List<OptionItem>();
    }

    public class OptionItem : INotifyPropertyChanged
    {
        public string Title
        {
            get
            {
                return m_title;
            }
            set
            {
                if ( !string.Equals( m_title, value, StringComparison.Ordinal ) )
                {
                    m_title = value;
                    OnPropertyChanged("Title");
                }
            }
        }

        public string Value
        {
            get
            {
                return m_value;
            }
            set
            {
                if ( !string.Equals( m_value, value, StringComparison.Ordinal ) )
                {
                    m_value = value;
                    OnPropertyChanged("Value");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged( string argName )
        {
            if ( null != PropertyChanged )
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(argName));
            }
        }

        protected string m_title = null;

        protected string m_value = null;
    }
}
