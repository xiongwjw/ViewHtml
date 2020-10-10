/********************************************************************
	FileName:   Page4Html.xaml
    purpose:	

	author:		huang wei
	created:	2012/11/22

    revised history:
	2012/11/22  

================================================================
    Copyright (C) 2012, Grgbanking CO,. Ltd. All rights reserved.
================================================================
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Threading;
using UIServiceProtocol;
using UIServiceInWPF.HtmlScreenElement;
using System.Windows.Forms.Integration;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using LogProcessorService;
using ResourceManagerProtocol;
using System.IO;
using UIServiceInWPF.Interop;
using UIServiceInWPF.Resource;
using UIServiceInWPF.screen;

namespace UIServiceInWPF
{
    /// <summary>
    /// Interaction logic for Page4Html.xaml
    /// </summary>
    public partial class Page4Html : Page
    {
        public Page4Html( UIServiceWpfApp app )
        {
            Debug.Assert(null != app);
            m_application = app;

           // m_clickHandler = (System.Windows.Forms.HtmlElementEventHandler)Delegate.CreateDelegate(typeof(System.Windows.Forms.HtmlElementEventHandler), this, "OnClicked");
            m_mouseDownHandler = (System.Windows.Forms.HtmlElementEventHandler)Delegate.CreateDelegate(typeof(System.Windows.Forms.HtmlElementEventHandler), this, "OnLeftButtonDown");
            m_contextMenuHandler = (System.Windows.Forms.HtmlElementEventHandler)Delegate.CreateDelegate(typeof(System.Windows.Forms.HtmlElementEventHandler), this, "OnContextMenuShow");
            m_mouseUpHandler = (System.Windows.Forms.HtmlElementEventHandler)Delegate.CreateDelegate(typeof(System.Windows.Forms.HtmlElementEventHandler), this, "OnLeftButtonUp");

            InitializeComponent();

            //setup handlers for creating screen element
            m_dicScreenElementCreator.Add(s_buttonValue, CreateButtonElement);
            m_dicScreenElementCreator.Add(s_countDownValue, CreateCountDownElement);
            m_dicScreenElementCreator.Add(s_textValue, CreateInputElement);
            m_dicScreenElementCreator.Add(s_passwordValue, CreateInputElement);
            m_dicScreenElementCreator.Add(s_textblockValue, CreateInputElement);
            m_dicScreenElementCreator.Add(s_selectValue, CreateSelectElement);
            m_dicScreenElementCreator.Add(s_listValue, CreateListElement);

            m_frontWebBrowser = htmlrender1;
//            m_backWebBrowser = htmlrender2;
            webbrowser1.Visibility = Visibility.Visible;
     //       webbrowser2.Visibility = Visibility.Hidden;
     //       m_curWebBrowser = m_frontWebBrowser;

            //setup event handlers
            m_frontWebBrowser.DocumentCompleted += OnDocumentCompleted;
            m_frontWebBrowser.Navigating += OnNavigating;
            m_frontWebBrowser.Navigated += OnNavigated;

            m_dataPropChangedHander = new Action<string>(OnPropertyChanged);

            //m_backWebBrowser.DocumentCompleted += OnDocumentCompleted;
            //m_backWebBrowser.Navigating += OnNavigating;
            //m_backWebBrowser.Navigated += OnNavigated;
        }

        public bool SetPropertyValueOfElement( string elementName,
                                               string property,
                                               object value )
        {
            Debug.Assert(!string.IsNullOrEmpty(property));

            if ( null == m_curHtmlDocument )
            {
                return false;
            }

            if ( string.IsNullOrEmpty( elementName ) )
            {
                switch ( property )
                {
                    case UIPropertyKey.s_DataContextKey:
                        {
                            if ( null != m_dataContext &&
                                 m_dataContext is INotifyPropertyChanged )
                            {
                                INotifyPropertyChanged iNotify = m_dataContext as INotifyPropertyChanged;
                                Debug.Assert(null != iNotify);
                                iNotify.PropertyChanged -= OnDataPropertyChanged;
                            }

                            m_dataContext = value;
                            if ( null != m_dataContext &&
                                 m_dataContext is INotifyPropertyChanged)
                            {
                                INotifyPropertyChanged iNotify = m_dataContext as INotifyPropertyChanged;
                                Debug.Assert(null != iNotify);
                                iNotify.PropertyChanged += OnDataPropertyChanged;
                            }

                            //lock ( m_synElementLock ) 
                            {
                                foreach (var screenElement in m_dicScreenElements)
                                {
                                    screenElement.Value.SetBindingTarget(m_dataContext);
                                }
                            }
                        }
                        break;

                    default: 
                        {
                            return false;
                        }
                }
            }
            else if ( elementName.Equals( SpecialElement.s_Focus, StringComparison.OrdinalIgnoreCase ) )
            {
                if ( null != m_focusElement )
                {
                    m_focusElement.SetPropertyValue(property, value);
                }
            }
            else
            {
                elementName = elementName.ToLowerInvariant();
                if ( m_dicScreenElements.ContainsKey( elementName ) )
                {
                    return m_dicScreenElements[elementName].SetPropertyValue( property, value );
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public bool GetPropertyValueOfElement( string elementName,
                                               string property,
                                               out object value )
        {
            value = null;

            Debug.Assert(!string.IsNullOrEmpty(property));

            if (null == m_curHtmlDocument )
            {
                return false;
            }

            if (string.IsNullOrEmpty(elementName))
            {

            }
            else
            {
                elementName = elementName.ToLowerInvariant();
                if (m_dicScreenElements.ContainsKey(elementName))
                {
                    return m_dicScreenElements[elementName].GetPropertyValue(property, out value);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public object ExecuteScriptCommand( string name,
                                            params object[] args )
        {
            Debug.Assert(!string.IsNullOrEmpty(name));
            if ( null == m_curHtmlDocument )
            {
                return null;
            }

            return m_curHtmlDocument.InvokeScript(name, args);
        }

        public object ExecuteCustomCommand( string argName,
                                            params object[] args )
        {
            Debug.Assert( !string.IsNullOrEmpty(argName) );
            Log.UIService.LogDebugFormat("execute command[{0}] in html screen", argName);
            switch ( argName )
            {
                case UIServiceCommands.s_clearDataBind:
                    {
                        Log.UIService.LogDebug("Remove property changed event");
                        RemovePropertyChangedEvent();

                        ShowScreenKeyboard(false);
                    }
                    break;

                case UIServiceCommands.s_updateData:
                    {
                        Log.UIService.LogDebug("Prepare for update data");
                        
                        foreach ( var item in m_dicScreenElements )
                        {
                            if ( (item.Value is HtmlScreenInputElement) ||
                                 (item.Value is HtmlScreenSelectElement) )
                            {
                                item.Value.UpdateBindingData(true);
                            }
                        }
                    }
                    break;

                case UIServiceCommands.s_buttonDown:
                    {
                        if ( null == args[0] ||
                             !(args[0] is string) )
                        {
                            return null;
                        }

                        Log.UIService.LogDebugFormat("The button [{0}] is down", args[0]);
                        string name = (string)args[0];
                        HtmlScreenElementBase element = null;
                        if ( m_dicScreenElements.TryGetValue( name, out element ) )
                        {
                            element.OnLeftMouseDown(null, null);
                        }
                        //foreach (var item in m_dicScreenElements)
                        //{
                        //    if ( (item.Value is HtmlScreenButtonElement) &&
                        //         (name.Equals( item.Value.HostedElement.Id, StringComparison.OrdinalIgnoreCase )) )
                        //    {
                        //        item.Value.OnLeftMouseDown( null, null );
                        //        break;
                        //    }
                        //}
                    }
                    break;

                case UIServiceCommands.s_buttonUp:
                    {
                        if (null == args[0])
                        {
                            return null;
                        }

                        Log.UIService.LogDebugFormat("The button [{0}] is up", args[0]);
                        string name = (string)args[0];
                        HtmlScreenElementBase element = null;
                        if (m_dicScreenElements.TryGetValue(name, out element))
                        {
                            element.OnLeftMouseUp(null, null);
                        }
                        //foreach (var item in m_dicScreenElements)
                        //{
                        //    if ((item.Value is HtmlScreenButtonElement) &&
                        //         (name.Equals(item.Value.HostedElement.Id, StringComparison.OrdinalIgnoreCase)))
                        //    {
                        //        item.Value.OnLeftMouseUp(null, null);
                        //        break;
                        //    }
                        //}
                    }
                    break;

                default:
                    {
                        Log.UIService.LogDebug("The unsupported custom command");
                        Debug.Assert(false);
                    }
                    break;
            }
            return null;
        }

        public void EnumElement( EnumElementHandler Handler,
                                 ElementType Type,
                                 EnumFlag Flag,
                                 string NameOfElement,
                                 object Param )
        {
            Debug.Assert(null != Handler);

            //name
            Regex reg = null;
            if (!string.IsNullOrEmpty(NameOfElement))
            {
                reg = new Regex(NameOfElement, RegexOptions.IgnoreCase);
            }

            bool continueEnum = true;

            foreach ( var ele in m_dicScreenElements )
            {
                if ( (ElementType.Button == Type) &&
                      !(ele.Value is HtmlScreenButtonElement) )
                {
                    continue;
                }

                if ( (ElementType.Input == Type) &&
                     !(ele.Value is HtmlScreenInputElement) )
                {
                    continue;
                }

                if ( EnumFlag.OnlyEnable == Flag &&
                     !(ele.Value.IsEnable) )
                {
                    continue;
                }

                if ( EnumFlag.OnlyVisible == Flag &&
                     !(ele.Value.IsVisible) )
                {
                    continue;
                }

                if ( EnumFlag.VisibleAndEnable == Flag &&
                     !(ele.Value.IsVisible && ele.Value.IsEnable) )
                {
                    continue;
                }

                if ( EnumFlag.VisibleOrEnable == Flag &&
                     (!ele.Value.IsEnable && !ele.Value.IsVisible) )
                {
                    continue;
                }

                if ( null != reg )
                {
                    Match m = reg.Match(ele.Value.HostedElement.Id);
                    if ( !m.Success )
                    {
                        continue;
                    }
                }

                continueEnum = Handler.Invoke(ele.Value.HostedElement.Id, ele.Value.Key, Param);
                if ( !continueEnum )
                {
                    break;
                }
            }
        }

        public bool Navigate( string UriOfHtml )
        {
            Debug.Assert(!string.IsNullOrEmpty(UriOfHtml));

            try
            {
                LogProcessorService.Log.UIService.LogDebugFormat("Prepare for navigate to [{0}]", UriOfHtml);
                RemovePropertyChangedEvent();
                ClearScreenElements();
                m_listBindingExpresses.Clear();
                //if (m_frontWebBrowser.IsBusy)
                //{
                //    m_frontWebBrowser.Stop();
                //}
                //m_frontWebBrowser.Stop();
              //  RemovePropertyChangedEvent();
                //if (m_curWebBrowser.IsBusy)
                //{
                //    m_curWebBrowser.Stop();
                //}

                m_curUrl = UriOfHtml;
                
                m_frontWebBrowser.Navigate(UriOfHtml);
                //m_backWebBrowser.Navigate(UriOfHtml);
                //m_curWebBrowser = m_backWebBrowser;
            }
            catch (System.Exception ex)
            {
                LogProcessorService.Log.UIService.LogError("Failed to navigate to the page", ex);
                return false;            	
            }

            LogProcessorService.Log.UIService.LogDebug("Success to navigate to the page");

            return true;
        }

        public void HideScreen()
        {
           // Log.UIService.LogDebug("Enter HideScreen method");
            Debug.Assert(null != m_frontWebBrowser);
            //m_frontWebBrowser.Stop();
            
            RemovePropertyChangedEvent();

            ClearScreenElements();

          //  Log.UIService.LogDebug("Leave HideScreen method");
        }

        public void OnButtonClicked( string argID,
                                     string argTag )
        {
            Debug.Assert(null != m_application);
            m_application.OnButtonClickInHtml(argID, argTag);
        }

        public bool ShowScreenKeyboard( bool argShow )
        {
            try
            {
                if ( argShow && 
                     null != s_screenKeyboard &&
                     s_screenKeyboard.WaitForExit(0) )
                {
                    s_screenKeyboard.Close();
                    s_screenKeyboard.Dispose();
                    s_screenKeyboard = null;
                }

                if ( argShow && null == s_screenKeyboard )
                {
                    try
                    {
                        if (File.Exists(Environment.SystemDirectory + "\\osk.exe"))
                        {
                            s_screenKeyboard = new Process();
                            s_screenKeyboard.StartInfo.UseShellExecute = true;
                            s_screenKeyboard.StartInfo.FileName = "osk.exe";
                            s_screenKeyboard.StartInfo.Arguments = "";
                            s_screenKeyboard.Start();
                            
                            //s_screenKeyboard.WaitForInputIdle();
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        if ( null != s_screenKeyboard )
                        {
                            s_screenKeyboard.Kill();
                            s_screenKeyboard.WaitForExit();
                            s_screenKeyboard.Close();
                            s_screenKeyboard.Dispose();
                            s_screenKeyboard = null;
                        }

                        return false;
                    }
                }
                //if ( argShow )
                //{
                //    if (!NativeWndApi.IsWindowVisible(s_screenKeyboard.MainWindowHandle))
                //    {
                //        NativeWndApi.ShowWindow(s_screenKeyboard.MainWindowHandle, NativeWndApi.SW_SHOWNA);
                //    }
                //}
                //else
                if ( !argShow && null != s_screenKeyboard )
                {
                    //if ( NativeWndApi.IsWindowVisible( s_screenKeyboard.MainWindowHandle ))
                    {
                        s_screenKeyboard.Kill();
                        s_screenKeyboard.WaitForExit();
                        s_screenKeyboard.Close();
                        s_screenKeyboard.Dispose();
                        s_screenKeyboard = null;
                    }
               }
            }
            catch (System.Exception ex)
            {
                Log.UIService.LogWarn("Failed to show screen keyboard", ex);
                s_screenKeyboard.Close();
                s_screenKeyboard.Dispose();
                s_screenKeyboard = null;
                return false;
            }

            return true;
        }

        public bool ShowHandleInput( bool argShow )
        {
            try
            {
                if ( argShow && null == s_handInput )
                {
                    string handleInputPath = Environment.GetEnvironmentVariable("HANDLEINPUT_PATH", EnvironmentVariableTarget.Machine);
                    if ( !string.IsNullOrEmpty(handleInputPath) )
                    {
                        handleInputPath += @"\HandInput.exe";
                        if ( File.Exists( handleInputPath ) )
                        {
                            s_handInput = new Process();
                            s_handInput.StartInfo.UseShellExecute = true;
                            s_handInput.StartInfo.FileName = handleInputPath;
                            s_handInput.StartInfo.Arguments = "";
                            s_handInput.Start();
                            s_handInput.WaitForInputIdle();
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                if ( !argShow && null != s_handInput )
                {
                    s_handInput.Kill();
                    s_handInput.WaitForExit();
                    s_handInput.Close();
                    s_handInput.Dispose();
                    s_handInput = null;
                }
            }
            catch (System.Exception ex)
            {
                return false;	
            }

            return true;
        }

        private void RemovePropertyChangedEvent()
        {
          //  Log.UIService.LogDebug("Enter RemovePropertyChangedEvent");
            if (null != m_dataContext &&
                m_dataContext is INotifyPropertyChanged)
            {
                INotifyPropertyChanged iNotify = (INotifyPropertyChanged)m_dataContext;
                iNotify.PropertyChanged -= OnDataPropertyChanged;
                m_dataContext = null;
            }
           // Log.UIService.LogDebug("Leave RemovePropertyChangedEvent");
        }

        public void Present()
        {
            //System.Windows.Forms.WebBrowser m_temp = m_frontWebBrowser;
            //m_frontWebBrowser = m_curWebBrowser;
            //m_backWebBrowser = m_temp;

            //WindowsFormsHost fontHost = null;
            //if ( m_frontWebBrowser == htmlrender1 )
            //{
            //    fontHost = webbrowser1;
            //}
            //else
            //{
            //    fontHost = webbrowser2;
            //}
            //if ( fontHost.Visibility != Visibility.Visible )
            //{
            //    fontHost.Visibility = Visibility.Visible;
            //}

            //WindowsFormsHost backHost = null;
            //if ( m_backWebBrowser == htmlrender1 )
            //{
            //    backHost = webbrowser1;
            //}
            //else
            //{
            //    backHost = webbrowser2;
            //}
            //if ( backHost.Visibility == Visibility.Visible )
            //{
            //    backHost.Visibility = Visibility.Hidden;
            //}

            ////stop
            //m_backWebBrowser.Stop();
        }

        private void OnLeftButtonDown( object sender,
                                       System.Windows.Forms.HtmlElementEventArgs arg )
        {
            System.Windows.Forms.HtmlDocument doc = (System.Windows.Forms.HtmlDocument)sender;
            System.Windows.Forms.HtmlElement element = doc.GetElementFromPoint(arg.MousePosition);

            HtmlScreenElementBase newFcsElement = FindFocusElement(element);
            if ( null != newFcsElement )
            {
                if ( m_focusElement != newFcsElement )
                {
                    m_focusElement = newFcsElement;
                  //  ShowHandleInput(true);
                }
                m_focusElement.SetFocus();
            }
            else
            {
                ShowScreenKeyboard(false);
                m_focusElement = null;
               // ShowHandleInput(false);
            }

            HtmlScreenElementBase button = FindButtonInHtml(element);
            if ( null == button )
            {
                System.Windows.Forms.HtmlElement parentElement = element.Parent;
                if ( null == parentElement )
                {
                    return;
                }

                button = FindButtonInHtml(parentElement);
                if ( null == button )
                {
                    return;
                }
            }

            button.OnLeftMouseDown(sender, arg);
            arg.ReturnValue = true;          
        }

        private void OnLeftButtonUp( object sender,
                                     System.Windows.Forms.HtmlElementEventArgs arg )
        {
            System.Windows.Forms.HtmlDocument doc = (System.Windows.Forms.HtmlDocument)sender;
            System.Windows.Forms.HtmlElement element = doc.GetElementFromPoint(arg.MousePosition);

            HtmlScreenElementBase button = FindButtonInHtml(element);
            if (null == button)
            {
                System.Windows.Forms.HtmlElement parentElement = element.Parent;
                if (null == parentElement)
                {
                    return;
                }

                button = FindButtonInHtml(parentElement);
                if (null == button)
                {
                    return;
                }
            }

            button.OnLeftMouseUp(sender, arg);
            arg.ReturnValue = true;
        }

        private void OnClicked( object sender,
                                System.Windows.Forms.HtmlElementEventArgs arg )
        {
            //
            //System.Windows.Forms.HtmlDocument doc = (System.Windows.Forms.HtmlDocument)sender;
            //System.Windows.Forms.HtmlElement element = doc.GetElementFromPoint(arg.MousePosition);
            //System.Windows.Forms.HtmlElement buttonElement = m_listButtons.Find( btn => btn == element );
            //if ( null == buttonElement )
            //{
            //    System.Windows.Forms.HtmlElement parentElement = element.Parent;
            //    if ( null == parentElement )
            //    {
            //        return;
            //    }

            //    buttonElement = m_listButtons.Find( btn => btn == parentElement );
            //    if ( null == buttonElement )
            //    {
            //        return;
            //    }
            //}

            //string tagValue = buttonElement.GetAttribute(s_tagAttri);
            //string tagID = buttonElement.Id;

            //m_app.OnButtonClickInHtml(tagID, tagValue);

            //arg.ReturnValue = true;
        }

        private void OnContextMenuShow( object sender,
                                        System.Windows.Forms.HtmlElementEventArgs arg )
        {
            arg.ReturnValue = true;
        }

        private void OnDocumentCompleted( object sender,
                                          System.Windows.Forms.WebBrowserDocumentCompletedEventArgs arg )
        {
            Log.UIService.LogDebugFormat("The url[{0}] has navigate completed", arg.Url.ToString());
            //if ( 0 != string.Compare( arg.Url.ToString(), m_curUrl, true ) )
            //if ( !arg.Url.ToString().Equals( m_curUrl, StringComparison.OrdinalIgnoreCase ) )
            //{
            //    return;
            //}
            RemoveEventHandlesOfHtmlDocument(m_curHtmlDocument);
            m_curHtmlDocument = null;

            m_curHtmlDocument = ((System.Windows.Forms.WebBrowser)sender).Document;
            Debug.Assert(null != m_curHtmlDocument);
            ChangeCssResource();
            ExtractAllScreenElements(m_curHtmlDocument);
//            m_curHtmlDocument.InvokeScript(s_ChangeLangScript, new object[] { m_application.CurrentLanguage });
            AddEventHandlesOfHtmlDocument(m_curHtmlDocument);

            if (null != OwnerScreen)
            {
                OwnerScreen.NotifyScreenOk(m_curHtmlDocument);
            }
           // FindAllButtonsInHtml(m_curHtmlDocument);
           // Log.UIService.LogDebug("Notify UIServiceApp : navigate completd");
            m_application.OnNavigateCompleted();
        }

        private void ChangeResourceInHtml()
        {
            
        }

        private void OnNavigating( object sender,
                                   System.Windows.Forms.WebBrowserNavigatingEventArgs arg )
        {
          //  Log.UIService.LogDebugFormat("On navigating Targetframe[{0}] : Url[{1}]", arg.TargetFrameName, arg.Url);
        }

        private void OnNavigated( object sender,
                                  System.Windows.Forms.WebBrowserNavigatedEventArgs arg )
        {
          //  Log.UIService.LogDebugFormat("On navigated Url[{0}]", arg.Url);
        }

        private void RemoveEventHandlesOfHtmlDocument( System.Windows.Forms.HtmlDocument doc )
        {
            if ( null == doc )
            {
                return;
            }
          //  doc.Click -= m_clickHandler;
            doc.MouseDown -= m_mouseDownHandler;
            doc.ContextMenuShowing -= m_contextMenuHandler;
            doc.MouseUp -= m_mouseUpHandler;
        }

        public void AddElementBinding(ElementBindingExpress argExpress)
        {
            Debug.Assert(null != argExpress);
            m_listBindingExpresses.Add(argExpress);
        }

        private void AddEventHandlesOfHtmlDocument( System.Windows.Forms.HtmlDocument doc )
        {
            if ( null == doc )
            {
                return;
            }

            Debug.Assert(null != doc);

           // doc.Click += m_clickHandler;
            doc.MouseDown += m_mouseDownHandler;
            doc.ContextMenuShowing += m_contextMenuHandler;
            doc.MouseUp += m_mouseUpHandler;
        }

        //private void FindAllButtonsInHtml( System.Windows.Forms.HtmlDocument doc )
        //{
        //    Debug.Assert(null != doc);

        //    System.Windows.Forms.HtmlElement body = doc.Body;
        //    if ( null == body )
        //    {
        //        return;
        //    }

        //    m_listButtons.Clear();

        //    System.Windows.Forms.HtmlElementCollection elementCollection = body.All;
        //    Debug.Assert(null != elementCollection);
        //    string valueOfAttri = null;
        //    foreach ( System.Windows.Forms.HtmlElement element in elementCollection )
        //    {
        //        valueOfAttri = element.GetAttribute(s_typeAttri);
        //        if ( string.IsNullOrEmpty (valueOfAttri) )
        //        {
        //            continue;
        //        }

        //        if ( 0 == string.Compare( valueOfAttri, s_buttonValue, true ) )
        //        {
        //            m_listButtons.Add(element);
        //        }
        //    }
        //}

        private void ExtractAllScreenElements( System.Windows.Forms.HtmlDocument argDoc )
        {
            Debug.Assert(null != argDoc);

            System.Windows.Forms.HtmlElement body = argDoc.Body;
            if ( null == body )
            {
                return;
            }

            m_listBindingExpresses.Clear();
            ClearScreenElements();

            System.Windows.Forms.HtmlElementCollection elementCollection = body.All;
            Debug.Assert(null != elementCollection);
            HtmlScreenElementBase screenElement = null;
            string idsValue = null;
            string valueOfAttri = null;
            IResourceService iCurrentUIResource = null;
            if ( null != m_application.ResourceManager )
            {
                iCurrentUIResource = m_application.ResourceManager.CurrentUIResource;
            }
            string text = null;
            string replaceAttri = null;
            int replaceValue = 0;
            string imagePath = null;
            string srcPath = null;
            foreach (System.Windows.Forms.HtmlElement element in elementCollection)
            {
                //replace ids
                idsValue = element.GetAttribute(s_UITextKey);
                if ( null != iCurrentUIResource &&
                    !string.IsNullOrEmpty(idsValue) )
                {
                    if ( iCurrentUIResource.LoadString( idsValue, TextCategory.s_UI, out text ) )
                    {
                        element.InnerHtml = ConvertText(text);                
                    }
                }
                //replace image
                if ( null != iCurrentUIResource &&
                     s_imgName.Equals( element.TagName, StringComparison.OrdinalIgnoreCase ) )
                {
                    replaceAttri = element.GetAttribute(s_replaceAttri);
                    srcPath = element.GetAttribute("src");
                    if ( !string.IsNullOrEmpty(replaceAttri) &&
                         !string.IsNullOrEmpty(srcPath) &&
                         int.TryParse(replaceAttri, out replaceValue) &&
                         replaceValue == 1 )
                    {
                        if ( iCurrentUIResource.QueryImagePath( srcPath, out imagePath ) )
                        {
                            element.SetAttribute("src", imagePath);
                        }
                    }
                }

                if ( string.IsNullOrEmpty(element.Id) )
                {
                    continue;
                }

                valueOfAttri = element.GetAttribute(s_typeAttri);
                if (string.IsNullOrEmpty(valueOfAttri))
                {
                    continue;
                }

                valueOfAttri = valueOfAttri.ToLowerInvariant();
                if ( m_dicScreenElementCreator.ContainsKey( valueOfAttri ) )
                {
                    try
                    {
                        screenElement = m_dicScreenElementCreator[valueOfAttri].Invoke(element);
                        if ( null != screenElement )
                        {
                            m_dicScreenElements.Add(element.Id.ToLowerInvariant(), screenElement);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        LogProcessorService.Log.UIService.LogWarn(string.Format("Failed to create a screen element[{0}]", element.Id), ex);	
                    }
                }
            }
        }

        private HtmlScreenElementBase CreateButtonElement( System.Windows.Forms.HtmlElement argElement )
        {
            Debug.Assert(null != argElement);

            HtmlScreenElementBase screenButton = new HtmlScreenButtonElement(argElement, this);
            screenButton.ParseBindingExpress();
            screenButton.ParseResource();

            return screenButton;
        }

        public HtmlScreenElementBase FindElement(string argName)
        {
            //Debug.Assert(!string.IsNullOrEmpty(argName));
            if (string.IsNullOrEmpty(argName))
            {
                return null;
            }

            HtmlScreenElementBase element = null;
            if (m_dicScreenElements.TryGetValue(argName, out element))
            {
                return element;
            }

            return null;
        }

        public ResourceItem FindResourceInHtml(string argKey)
        {
            Debug.Assert(!string.IsNullOrEmpty(argKey));
            if (null == OwnerScreen)
            {
                return null;
            }

            foreach (var item in OwnerScreen.Templates)
            {
                if (item.Name.Equals(argKey, StringComparison.Ordinal))
                {
                    return item;
                }
            }

            return null;
        }

        private HtmlScreenElementBase CreateCountDownElement( System.Windows.Forms.HtmlElement argElement )
        {
            Debug.Assert(null != argElement );

            HtmlScreenElementBase screenCountDown = new HtmlScreenCountdownElement(argElement, this);
            screenCountDown.ParseBindingExpress();

            return screenCountDown;
        }

        private HtmlScreenElementBase CreateSelectElement( System.Windows.Forms.HtmlElement argElement )
        {
            Debug.Assert(null != argElement);

            HtmlScreenElementBase selectElement = new HtmlScreenSelectElement( argElement, this );
            selectElement.ParseBindingExpress();

            return selectElement;
        }

        private HtmlScreenElementBase CreateListElement(System.Windows.Forms.HtmlElement argElement)
        {
            Debug.Assert(null != argElement);

            if (!string.Equals(argElement.TagName, "table", StringComparison.OrdinalIgnoreCase))
            {
                Log.UIService.LogWarn("The element of list type must be <table> in html");
                return null;
            }

            HtmlScreenElementBase screenListElement = new HtmlScreenListElement(argElement, this);
            if (!screenListElement.Open())
            {
                Log.UIService.LogError("Failed to open list element");
                return null;
            }
            screenListElement.ParseBindingExpress();

            return screenListElement;
        }

        private HtmlScreenElementBase CreateInputElement( System.Windows.Forms.HtmlElement argElement )
        {
            Debug.Assert(null != argElement);

            string format = argElement.GetAttribute(s_formatAttri);

            HtmlScreenInputElement screenInput = new HtmlScreenInputElement(argElement, this);
            if ( !string.IsNullOrEmpty(format) )
            {
                screenInput.FormatPattern = format;
            }
            screenInput.ParseBindingExpress();

            //show virtual keyboard
            string showVKeyboard = argElement.GetAttribute( UIPropertyKey.s_showVirtualKeyboard );
            if ( !string.IsNullOrEmpty( showVKeyboard ) )
            {
                int value = 0;
                if ( int.TryParse( showVKeyboard, out value ) )
                {
                    screenInput.CanShowVirtualKeyboard = value == 0 ? false : true;
                }
                else
                {
                    screenInput.CanShowVirtualKeyboard = false;
                }
            }

            return screenInput;
        }

        private HtmlScreenElementBase FindButtonInHtml(System.Windows.Forms.HtmlElement argElement)
        {
            Debug.Assert(null != argElement);

            if (string.IsNullOrEmpty(argElement.Id))
            {
                return null;
            }

            string id = argElement.Id.ToLowerInvariant();
            if (!m_dicScreenElements.ContainsKey(id))
            {
                return null;
            }

            return m_dicScreenElements[id];
        }

        private HtmlScreenElementBase FindFocusElement( System.Windows.Forms.HtmlElement argElement )
        {
            Debug.Assert(null != argElement);
            if ( string.IsNullOrEmpty(argElement.Id) )
            {
                return null;
            }

            string id = argElement.Id.ToLowerInvariant();
            HtmlScreenElementBase element = null;
            if ( m_dicScreenElements.TryGetValue( id, out element ) &&
                 element.CanFocus )
            {
                return element;
            }

            return null;
        }

        private void OnDataPropertyChanged( object sender, 
                                            PropertyChangedEventArgs arg )
        {
            if ( string.IsNullOrEmpty(arg.PropertyName) )
            {
                return;
            }

            this.Dispatcher.BeginInvoke(m_dataPropChangedHander,
                                         DispatcherPriority.Input,
                                         arg.PropertyName);
   //         lock ( m_synElementLock )
            //{
            //    foreach (var element in m_dicScreenElements)
            //    {
            //        element.Value.OnPropertyChanged(arg.PropertyName);
            //    }
            //}
        }

        public static string ConvertText( string argInput )
        {
            //Debug.Assert(!string.IsNullOrEmpty (argInput));
            if ( string.IsNullOrEmpty(argInput) )
            {
                return argInput;
            }

            if (-1 != argInput.IndexOf(s_placeholderBr, 0))
            {
                argInput = argInput.Replace("|", "<BR>");
            }
            if (-1 != argInput.IndexOf(s_placeholderSharp, 0))
            {
                argInput = argInput.Replace("#", "&nbsp;");
            }
            if (-1 != argInput.IndexOf(s_placeholderAt, 0))
            {
                argInput = argInput.Replace(s_placeholderAt, '&');
            }

            return argInput;
        }

        private void OnPropertyChanged( string argName )
        {
            foreach (var element in m_dicScreenElements)
            {
                element.Value.OnPropertyChanged(argName);
            }
        }

        public void NotifyPropertyChanged( string argElement,
                                           string argProperty,
                                           string argValue )
        {
            Debug.Assert(null != m_application);
            foreach (var item in m_listBindingExpresses)
            {
                if (item.CanTrigger(argElement,
                                      argProperty))
                {
                    item.SetBindValue(argProperty, argValue);
                }
            }

            m_application.NotifyPropertyChanged(argElement, argProperty, argValue);

            m_application.NotifyPropertyChanged(argElement, argProperty, argValue);
        }

        private void ClearScreenElements()
        {
           // Log.UIService.LogDebug("Enter ClearScreenElements");
           // lock ( m_synElementLock )
            {
                foreach (var ele in m_dicScreenElements)
                {
                    ele.Value.Dispose();
                }
                m_dicScreenElements.Clear();
            }
         //   Log.UIService.LogDebug("Leave ClearScreenElements");
        }

        //public string CurrentLanguage
        //{
        //    get
        //    {
        //        return m_curLanguage;
        //    }
        //    set
        //    {
        //        m_curLanguage = value;
        //        if (null != m_curHtmlDocument)
        //        {
        //            m_curHtmlDocument.InvokeScript(s_ChangeLangScript, new object[] { m_curLanguage });
        //        }
        //    }
        //}

        public IResourceManager ResourceManager
        {
            get
            {
                return m_application.ResourceManager;
            }
        }

        public Screen OwnerScreen
        {
            get;
            set;
        }

        private void ChangeCssResource() 
        {
            if ( null == m_application.ResourceManager )
            {
                return;
            }

            Debug.Assert(null != m_curHtmlDocument);
            var elementlist = m_curHtmlDocument.GetElementsByTagName(s_linkName);
            string replaceAttri = null;
            string hrefAttri = null;
            int replace = 0;
            string language = null;

            foreach ( System.Windows.Forms.HtmlElement item in elementlist )
            {
                replaceAttri = item.GetAttribute(s_replaceAttri);
                hrefAttri = item.GetAttribute("href");
                if ( !string.IsNullOrEmpty(replaceAttri) &&
                     !string.IsNullOrEmpty(hrefAttri) &&
                     int.TryParse(replaceAttri, out replace) &&
                     replace == 1 )
                {
                    language = System.IO.Path.GetFileNameWithoutExtension(hrefAttri);
                    if (!m_application.ResourceManager.CurrentUIResource.Language.Equals(language, StringComparison.OrdinalIgnoreCase))
                    {
                        string directPath = System.IO.Path.GetDirectoryName(hrefAttri);
                        string newPath = System.IO.Path.Combine(directPath, m_application.ResourceManager.CurrentUIResource.Language + System.IO.Path.GetExtension(hrefAttri));
                        item.SetAttribute("href", newPath.Replace(@"\", "/"));
                        directPath = null;
                        newPath = null;
                    }
                }
            }
        }

        private System.Windows.Forms.WebBrowser m_frontWebBrowser = null;

        //private System.Windows.Forms.WebBrowser m_backWebBrowser = null;

        //private System.Windows.Forms.WebBrowser m_curWebBrowser = null;

        private System.Windows.Forms.HtmlDocument m_curHtmlDocument = null;

      //  private System.Windows.Forms.HtmlElementEventHandler m_clickHandler = null;
        private System.Windows.Forms.HtmlElementEventHandler m_mouseDownHandler = null;

        private System.Windows.Forms.HtmlElementEventHandler m_contextMenuHandler = null;

        private System.Windows.Forms.HtmlElementEventHandler m_mouseUpHandler = null;

        //private List<System.Windows.Forms.HtmlElement> m_listButtons = new List<System.Windows.Forms.HtmlElement>();

        private Dictionary<string, HtmlScreenElementBase> m_dicScreenElements = new Dictionary<string, HtmlScreenElementBase>();

        private Dictionary<string, Func<System.Windows.Forms.HtmlElement, HtmlScreenElementBase>> m_dicScreenElementCreator = new Dictionary<string, Func<System.Windows.Forms.HtmlElement, HtmlScreenElementBase>>();

        private List<ElementBindingExpress> m_listBindingExpresses = new List<ElementBindingExpress>();

        private Action<string> m_dataPropChangedHander = null;

        private UIServiceWpfApp m_application = null;

        private object m_dataContext = null;

        private HtmlScreenElementBase m_focusElement = null;

 //       private string m_curLanguage = "CN";
//        private object m_synElementLock = new object();
        private Process s_screenKeyboard = null;

        private Process s_handInput = null;

        private string m_curUrl = null;

        public const string s_UITextKey = "ids";

        public const string s_imgName = "img";

        public const string s_linkName = "link";

        public const string s_replaceAttri = "replace";

        public const char s_placeholderBr = '|';

        public const char s_placeholderSharp = '#';

        public const char s_placeholderAt = '@';

        public const string s_typeAttri = "type";

        public const string s_formatAttri = "format";

        private const string s_buttonValue = "button";

        private const string s_countDownValue = "countdown";

        private const string s_passwordValue = "password";

        private const string s_textblockValue = "textblock";

        private const string s_listValue = "list";

        private const string s_selectValue = "select";

        private const string s_textValue = "text";

        public const string s_tagAttri = "tag";

        public const string s_hideStyle = "display: none";

        protected const string s_ChangeLangScript = "ChangePageProperty";

        public const string s_resourceNode = "Resource";

        public const string s_dataTemplateNode = "DataTemplate";
    }
}
