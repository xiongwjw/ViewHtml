using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIServiceInWPF.trigger;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Markup;
using UIServiceProtocol;
using System.Windows.Media;
using System.Windows.Media.Animation;
using UIServiceInWPF.GrgStoryboardNS;
using UIServiceInWPF.Resource;
using System.Windows.Controls.Primitives;
using ResourceManagerProtocol;

namespace UIServiceInWPF.screen
{
    public abstract class Screen
    {
#region constructor
        public Screen()
        {
            
        }
#endregion

#region method
        public void AddTrigger( UITriggerBase objTrigger)
        {
            Debug.Assert(null != objTrigger);
            TriggerCollection.Add(objTrigger);
        }

        public void RemoveTrigger( UITriggerBase objTrigger)
        {
            Debug.Assert( null != objTrigger );
            TriggerCollection.Remove(objTrigger);
            objTrigger.Exit();
            objTrigger = null;
        }

        public virtual bool TriggerDo( string strCondition,
                                       string argScrName )
        {
            Debug.Assert(!string.IsNullOrEmpty(strCondition));

            bool bubbleEvent = true;
            if ( null != m_activeChildScreen )
            {
                m_activeChildScreen.TriggerDo(strCondition, argScrName);
            }
            else
            {
                if (null != m_listTrigger)
                {
                    foreach (var objTrigger in m_listTrigger)
                    {
                        if ( objTrigger.CanApply(argScrName) &&
                             objTrigger.CanTrigger(strCondition))
                        {
                            objTrigger.DoAction();
                            if ( !objTrigger.BubbleEvent )
                            {
                                bubbleEvent = false;
                            }
                        }
                    }
                }
            }

            return bubbleEvent;
        }

        public virtual bool SetPropertyValue( string strElement,
                                              string strProperty,
                                              object objValue )
        {
            return false;
        }

        public virtual  bool GetPropertyValue( string strElement,
                                               string strProperty,
                                               out object objValue )
        {
            objValue = null;

            return false;
        }

        public virtual object ExecuteCommand( UICommandType type,
                                              string name,
                                              params object[] args )
        {
            return null;
        }

        public virtual void EnumElements( EnumElementHandler Handler,
                                          ElementType Type,
                                          EnumFlag Flag,
                                          string NameOfElement,
                                          object Param )
        {
        }

        public virtual bool ShowScreen()
        {
            return false;
        }

        public virtual void Present()
        {
            if ( null != m_listTrigger )
            {
                foreach (var item in m_listTrigger)
                {
                    item.Prepare();
                }
            }
        }

        public virtual void Redraw()
        {

        }

        public virtual void SetDatacontext(object obj)
        {

        }

        public virtual void NotifyScreenOk( System.Windows.Forms.HtmlDocument argDoc )
        {
            Debug.Assert(null != argDoc);

            if ( null != m_listStoryboards )
            {
                foreach ( var sb in m_listStoryboards )
                {
                    sb.NotifyScreenOk( argDoc );
                }
            }
        }

        public virtual void Reset()
        {
            if ( null != m_listStoryboards )
            {
                foreach ( var item in m_listStoryboards )
                {
                    item.Stop();
                }
            }

            if ( null != m_listTrigger )
            {
                foreach ( var item in m_listTrigger )
                {
                    item.Terminate();
                }
            }
        }

        public virtual void clear()
        {
            if (null != m_listTrigger)
            {
                foreach (var item in m_listTrigger)
                {
                    item.Exit();
                }
                m_listTrigger.Clear();
                m_listTrigger = null;
            }

            if ( null != m_listStoryboards )
            {
                foreach ( var item in m_listStoryboards )
                {
                    item.Close();
                }
                m_listStoryboards.Clear();
                m_listStoryboards = null;
            }

            if ( null != m_listTemplate )
            {
                foreach ( var item in m_listTemplate )
                {
                    item.Close();
                }
                m_listTemplate.Clear();
                m_listTemplate = null;
            }

            if ( null != m_listNeedClearElements )
            {
                m_listNeedClearElements.Clear();
                m_listNeedClearElements = null;
            }
        }

        public virtual void ChangeLanguage( string argLanguage )
        {
            Debug.Assert(!string.IsNullOrEmpty(argLanguage));
            string dir = string.Format(@"{0}{1}/", ConfigPath, argLanguage);
            Uri uri = new Uri(dir);
            if (uri.IsFile)
            {
                string localPath = uri.LocalPath;
                if (Directory.Exists(localPath))
                {
                    m_path = dir;
                }
                else
                {
                    m_path = ConfigPath;
                }
            }
            else
            {
                m_path = ConfigPath;
            }
        }

        public GrgStoryboard FindStoryboard( string argName )
        {
            if ( null == m_listStoryboards )
            {
                return null;
            }

            foreach ( var storyboard in m_listStoryboards )
            {
                if ( storyboard.Name.Equals( argName, StringComparison.OrdinalIgnoreCase ) )
                {
                    return storyboard;
                }
            }

            return null;
        }

        public bool OnPropertyChanged( string argName,
                                          string argProperty,
                                          string argValue,
                                       string argScrName )
        {
            if ( null == m_listTrigger )
            {
                return true;
            }

            bool bubbleEvent = true;
            foreach ( var item in m_listTrigger )
            {
                if ( item is PropertyTrigger &&
                     item.CanApply(argScrName) &&
                     item.CanTrigger( argName, argProperty, argValue ) )
                {
                    item.DoAction();
                    if ( !item.BubbleEvent )
                    {
                        bubbleEvent = false;
                    }
                }
            }

            return bubbleEvent;
        }

        /// <summary>
        /// Get property value of a xaml screen
        /// </summary>
        /// <param name="objElement">a framework element</param>
        /// <param name="strProperty">Property name of a element</param>
        /// <param name="objValue">Property value</param>
        /// <returns>If success, it returns true. Otherwise, it returns false</returns>
        public static bool GetPropertyValue(FrameworkElement objElement,
                                      string strProperty,
                                      out object objValue)
        {
            objValue = null;
            //If element is null, it return false.
            if (null == objElement)
            {
                return false;
            }

            bool bRet = false;
            switch (strProperty)
            {
                case UIPropertyKey.s_DataContextKey:
                    {
                        //Get value of DataContext property
                        objValue = objElement.DataContext;
                        bRet = true;
                    }
                    break;

                case UIPropertyKey.s_EnableKey:
                    {
                        //Get value of Enable property
                        objValue = objElement.IsEnabled;
                        bRet = true;
                    }
                    break;

                case UIPropertyKey.s_ItemSourceKey:
                    {
                        //Get value of ItemSource property
                        ItemsControl objItems = (ItemsControl)objElement;
                        objValue = objItems.ItemsSource;
                        bRet = true;
                    }
                    break;

                case UIPropertyKey.s_stateKey:
                    {
                        if ( objElement is ToggleButton )
                        {
                            objValue = ((ToggleButton)objElement).IsChecked;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                case UIPropertyKey.s_ValueKey:
                case UIPropertyKey.s_ContentKey:
                    {
                        //Get value of Value property.
                        if (objElement is TextBlock)
                        {
                            //For TextBlock element
                            objValue = ((TextBlock)objElement).Text;
                        }
                        else if ( objElement is TextBox )
                        {
                            objValue = ((TextBox)objElement).Text;
                        }
                        else if (objElement is PasswordBox)
                        {
                            //For PasswordBox element
                            objValue = ((PasswordBox)objElement).Password;
                        }
                        else if (objElement is ComboBox)
                        {
                            ComboBoxItem item = (ComboBoxItem)((ComboBox)objElement).SelectedItem;
                            if ( null != item )
                            {
                                objValue = item.Content;
                            }
                            else
                            {
                                objValue = null;
                            }
                        }
                        //else if ( objElement is WpfFlashControl )
                        //{
                        //    objValue = ((WpfFlashControl)objElement).FileName;
                        //}
                        else if (objElement is ContentControl)
                        {
                            //For ContentControl element
                            objValue = ((ContentControl)objElement).Content;
                        }
                        bRet = true;
                    }
                    break;

                case UIPropertyKey.s_VisibleKey:
                    {
                        //Get value of Visible property
                        if (objElement.Visibility == Visibility.Visible)
                        {
                            objValue = true;
                        }
                        else
                        {
                            objValue = false;
                        }

                        bRet = true;
                    }
                    break;
                case UIPropertyKey.s_selectIndexKey:
                    if (objElement is ListBox)
                    {
                        objValue = (objElement as ListBox).SelectedIndex;
                        bRet = true;
                    }
                    break;
                    //更新获取tag值
                case UIPropertyKey.s_tagKey:
                    if (null != objElement.Tag)
                    {

                        objValue = objElement.Tag;
                        bRet = true;
                    }
                    break;
                default:
                    {
                        //Unknown property key
                        Debug.Assert(false);
                    }
                    break;
            }

            return bRet;
        }

        /// <summary>
        /// Set property value a element
        /// </summary>
        /// <param name="objElement">a framework element</param>
        /// <param name="strProperty">Property key of a element</param>
        /// <param name="objValue">Property value</param>
        /// <returns>If success, it returns true. Otherwise, it return false</returns>
        public static bool SetPropertyValue(FrameworkElement objElement,
                              string strProperty,
                              object objValue)
        {
            Debug.Assert(null != strProperty);
            //If element is null, it return false.
            if (null == objElement)
            {
                return false;
            }

            bool bRet = false;
            switch (strProperty)
            {
                case UIPropertyKey.s_DataContextKey:
                    {
                        //Set DataContext property of a element
                        objElement.DataContext = objValue;
                        bRet = true;
                    }
                    break;

                case UIPropertyKey.s_EnableKey:
                    {
                        if ( null == objValue )
                        {
                            return true;
                        }
                        //Set Enable property of a element
                        bool value = false;
                        if (objValue is int)
                        {
                            value = (int)objValue == 0 ? false : true;
                        }
                        else if (objValue is bool)
                        {
                            value = (bool)objValue;
                        }
                        else if (objValue is string)
                        {
                            int temp = 0;
                            if (int.TryParse((string)objValue, out temp))
                            {
                                value = temp == 0 ? false : true;
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

                        objElement.IsEnabled = value;
                        bRet = true;
                    }
                    break;

                case UIPropertyKey.s_ClearItemSourceKey:
                    {
                        //Clear ItemSource property of a element
                        ItemsControl objItems = (ItemsControl)objElement;
                        if (null != objItems.ItemsSource)
                        {
                            objItems.ItemsSource = null;
                        }
                    }
                    break;

                case UIPropertyKey.s_ItemSourceKey:
                    {
                        //Set ItemSource property of a element
                        ItemsControl objItems = (ItemsControl)objElement;
                        if (null == objValue)
                        {
                            return false;
                        }

                        objItems.ItemsSource = (IEnumerable)objValue;
                        bRet = true;
                    }
                    break;

                case UIPropertyKey.s_ClearContent:
                    {
                        //Clear a content value of a element
                        if (objElement is InkCanvas)
                        {
                            ((InkCanvas)objElement).Strokes.Clear();
                        }
                        else if ( objElement is TextBlock )
                        {
                            ((TextBlock)objElement).Text = string.Empty;
                        }
                        else if ( objElement is TextBox )
                        {
                            ((TextBox)objElement).Text = string.Empty;
                        }
                        else if ( objElement is ContentControl )
                        {
                            ((ContentControl)objElement).Content = null;
                        }
                        //else if ( objElement is WpfFlashControl )
                        //{
                        //    ((WpfFlashControl)objElement).FileName = string.Empty;
                        //}
                        bRet = true;
                    }
                    break;

                case UIPropertyKey.s_BeginStoryboard:
                    {
                        try
                        {
                            if ( null == objValue ||
                                 !(objValue is string) ||
                                 string.IsNullOrEmpty((string)objValue) )
                            {
                                return false;
                            }
                            //Prepare for start a story board
                            FrameworkElement objEle = (FrameworkElement)objElement;
                            Debug.Assert(null != objEle);
                            string strKey = (string)objValue;
                            Storyboard objStory = (Storyboard)objEle.FindResource(strKey);
                            if ( null != objStory )
                            {
                                objStory.Begin();
                            }                    
                        }
                        catch (System.Exception ex)
                        {
                            LogProcessorService.Log.UIService.LogWarn("Failed to begin a story board", ex);
                        }
                    }
                    break;

                case UIPropertyKey.s_ValueKey:
                case UIPropertyKey.s_ContentKey:
                    {
                        //Set value property of a element.
                        if (objElement is TextBlock)
                        {
                            if ( null == objValue )
                            {
                                ((TextBlock)objElement).Text = string.Empty;
                            }
                            else if ( objValue is string )
                            {
                                ((TextBlock)objElement).Text = (string)objValue;
                            }
                            else
                            {
                                ((TextBlock)objElement).Text = objValue.ToString();
                            }
                        }
                        else if ( objElement is TextBox )
                        {
                            if ( null == objValue )
                            {
                                ((TextBox)objElement).Text = string.Empty;
                            }
                            else if ( objValue is string )
                            {
                                ((TextBox)objElement).Text = (string)objValue;
                            }
                            else
                            {
                                ((TextBox)objElement).Text = objValue.ToString();
                            }
                        }
                        else if (objElement is PasswordBox)
                        {
                            if (null == objValue)
                            {
                                ((PasswordBox)objElement).Password = string.Empty;
                            }
                            else if (objValue is string)
                            {
                                ((PasswordBox)objElement).Password = (string)objValue;
                            }
                            else
                            {
                                ((PasswordBox)objElement).Password = objValue.ToString();                            
                            }
                            
                        }
                        else if ( objElement is ComboBox )
                        {
                            if ( null == objValue )
                            {
                                ((ComboBox)objElement).SelectedItem = null;
                            }
                            else
                            {
                                foreach (ComboBoxItem item in ((ComboBox)objElement).Items)
                                {
                                    if ( item.Content is string &&
                                         objValue is string )
                                    {
                                        if (string.Equals((string)item.Content, (string)objValue, StringComparison.Ordinal))
                                        {
                                            ((ComboBox)objElement).SelectedItem = item;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (item.Content == objValue)
                                        {
                                            ((ComboBox)objElement).SelectedItem = item;
                                            break;
                                        }
                                    }

                                }
                            }
                        }
                        //else if ( objElement is WpfFlashControl )
                        //{
                        //    if ( null == objValue )
                        //    {
                        //        ((WpfFlashControl)objElement).FileName = string.Empty;
                        //    }
                        //    else
                        //    {
                        //        ((WpfFlashControl)objElement).FileName = (string)objValue;
                        //    }
                            
                        //}
                        else if (objElement is ContentControl)
                        {
                            ((ContentControl)objElement).Content = objValue;
                        }
                        
                        bRet = true;
                    }
                    break;

                case UIPropertyKey.s_VisibleKey:
                    {
                        //Set visible property of a element
                        if ( null == objValue )
                        {
                            return true;
                        }

                        bool value = false;
                        if ( objValue is int )
                        {
                            value = (int)objValue == 0 ? false : true;
                        }
                        else if ( objValue is bool )
                        {
                            value = (bool)objValue;
                        }
                        else if ( objValue is string )
                        {
                            int temp = 0;
                            if ( int.TryParse( (string)objValue, out temp ) )
                            {
                                value = temp == 0 ? false : true;
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

                        objElement.Visibility = value ? Visibility.Visible : Visibility.Hidden;
                        bRet = true;
                    }
                    break;

                case UIPropertyKey.s_stateKey:
                    {
                        if ( null == objValue ||
                             !(objElement is ToggleButton) )
                        {
                            return false;
                        }

                        if ( objValue is bool )
                        {
                            ((ToggleButton)objElement).IsChecked = (bool)objValue;
                        }
                        else if (objValue is int)
                        {
                            ((ToggleButton)objElement).IsChecked = (int)objValue == 0 ? false : true;
                        }
                        else if (objValue is string)
                        {
                            int temp = 0;
                            if (int.TryParse((string)objValue, out temp))
                            {
                                ((ToggleButton)objElement).IsChecked = temp == 0 ? false : true;
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
                    break;

                default:
                    {
                        //Unknown property key
                        Debug.Assert(false);
                    }
                    break;
            }

            return bRet;
        }

        public static FrameworkElement FindElement(FrameworkElement argParent,
                                            string argName)
        {
            if (string.IsNullOrEmpty(argName) ||
                 null == argParent)
            {
                return null;
            }

            DependencyObject element = LogicalTreeHelper.FindLogicalNode(argParent, argName);
            return element == null ? null : (FrameworkElement)element;
        }

        protected virtual void SetBindedFrameworkElement( FrameworkElement argElement )
        {
            
        }

        public void AddAndActiveScreen( Screen argChild )
        {
            Debug.Assert(null != argChild);

            if ( null == m_listScreens )
            {
                m_listScreens = new List<Screen>();
            }

            if ( m_listScreens.Count > 0 )
            {
                if ( !m_listScreens.Exists( scr => scr == argChild ) )
                {
                    m_listScreens.Add(argChild);
                }
            }
            else
            {
                m_listScreens.Add(argChild);
            }

            m_activeChildScreen = argChild;
        }

        public void AddNeedClearElement( string argElement )
        {
            Debug.Assert(!string.IsNullOrEmpty(argElement));
            if ( null == m_listNeedClearElements )
            {
                m_listNeedClearElements = new List<string>();
            }

            if ( !m_listNeedClearElements.Contains( argElement ) )
            {
                m_listNeedClearElements.Add(argElement);
            }
        }
#endregion

#region property
        public string Name
        {
            get;
            set;
        }

        public string Path
        {
            get
            {
                return m_path;
            }
            set
            {
                m_path = value;
            }
        }

        public string Site
        {
            get;
            set;
        }

        public string Url
        {
            get;
            set;
        }

        public string SubSite
        {
            get;
            set;
        }

        public virtual int Width
        {
            get
            {
                return 0;
            }
            set
            {
                
            }
        }

        public virtual int Height
        {
            get
            {
                return 0;
            }
            set 
            {

            }
        }

        public virtual Panel Content
        {
            get
            {
                return null;
            }
            set
            {

            }
        }

        public bool NeedCache
        {
            get
            {
                return m_needCache;
            }
            set
            {
                m_needCache = value;
            }
        }

        public string FullPath
        {
            get
            {
                return m_fullPath;
            }
            set
            {
                m_fullPath = value;
            }
        }

        protected List<UITriggerBase> TriggerCollection
        {
            get
            {
                if ( null == m_listTrigger )
                {
                    m_listTrigger = new List<UITriggerBase>();
                }

                return m_listTrigger;
            }
        }

        public List<GrgStoryboard> Storyboards
        {
            get
            {
                if ( null == m_listStoryboards )
                {
                    m_listStoryboards = new List<GrgStoryboard>();
                }

                return m_listStoryboards;
            }
        }

        public List<ResourceDataTemplate> Templates
        {
            get
            {
                if ( null == m_listTemplate )
                {
                    m_listTemplate = new List<ResourceDataTemplate>();
                }

                return m_listTemplate;
            }
        }

        public string ConfigOfUI
        {
            get
            {
                Debug.Assert(null != m_app);
                return m_app.CfgOfUIService.CfgFilePath;
            }
        }

        public UIServiceWpfApp App
        {
            set
            {
                m_app = value;
            } 
        }
        public IResourceManager ResouceManager
        {
            get{
                return m_ResourceManager;
            }
            set {

                m_ResourceManager = value;
            }
        }

        public Screen ActiveChild
        {
            get
            {
                return m_activeChildScreen;
            }
        }

        public virtual Panel InnerContent
        {
            get
            {
                return null;
            }
        }

        public string ConfigPath
        {
            get;
            set;
        }
#endregion

#region field

        protected IResourceManager m_ResourceManager=null;

        protected List<UITriggerBase> m_listTrigger = null;

        protected List<GrgStoryboard> m_listStoryboards = null;

        protected List<ResourceDataTemplate> m_listTemplate = null;

        protected List<string> m_listNeedClearElements = null;

        protected UIServiceWpfApp m_app = null;

        protected bool m_needCache = true;

        protected string m_fullPath = null;

        protected string m_path = null;

        protected List<Screen> m_listScreens = null;

        protected Screen m_activeChildScreen = null;
#endregion
    }
}
