/********************************************************************
	FileName:   UIServiceImp
    purpose:	implement the IUIService interface.

	author:		huang wei
	created:	2012/10/23

    revised history:
	2012/10/23  

================================================================
    Copyright (C) 2012, Grgbanking CO,. Ltd. All rights reserved.
================================================================
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Automation;
using UIServiceProtocol;
using Attribute4ECAT;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Windows.Threading;
using LogProcessorService;
using UIServiceInWPF.screen;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using ResourceManagerProtocol;
using System.Xml;
using UIServiceInWPF.Factory;
using System.Windows.Navigation;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using UIServiceInWPF.trigger;
using UIServiceInWPF.Resource;
using System.Runtime.InteropServices;

namespace UIServiceInWPF
{
    [GrgComponent("{FC3B3234-410E-4FBA-A927-AE28A25C6975}",
                   Name = "UIServiceInWPF",
                   Description = "A UI Service in wpf",
                   Catalog = "UIService",
                   Author = "huang wei",
                   Company = "GRGBanking co,. Ltd")]
    public class UIServiceImp : IUIService2
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool BRePaint);

        #region constructor
        public UIServiceImp()
        {
        }
        #endregion

        #region create
        [GrgCreateFunction("Create a UI service in WPF")]
        public static IUIService2 Create()
        {
            UIServiceImp objImp = new UIServiceImp();
            return objImp as IUIService2;
        }
        #endregion

        #region method of the IUIService interface
        /// <summary>
        /// Open the UI service
        /// </summary>
        /// <param name="strcfg">config file's path for the UI service</param>
        /// <param name="hwndParent">A parent window hosted the UI service.</param>
        bool IUIService.Open(string strcfg,
                              IntPtr hwndParent,
                              IResourceManager iargResourceManager)
        {
            Debug.Assert(!string.IsNullOrEmpty(strcfg));
            LogProcessorService.Log.UIService.LogInfoFormat("Prepare for opening the ui service in wpf with config file [{0}]", strcfg);
            //Check if the UI service has opened.
            if (m_IsOpened)
            {
                LogProcessorService.Log.UIService.LogInfo("The UI service in WPF has been opened");
                return true;
            }

            try
            {
                // m_strCfg = strcfg;
                //Load config file.
                if (!m_objCfg.Load(strcfg))
                {
                    LogProcessorService.Log.UIService.LogError("Failed to load configuration of UIService In WPF");
                    return false;
                }
                m_objCfg.ResouceManager = iargResourceManager;

                m_hWndParent = hwndParent;
                //Create a thread to handle UI service commands.
                LogProcessorService.Log.UIService.LogDebug("create a thread to create the main windown of WPF");
                m_objThread = new Thread(new ThreadStart(OnThreadFun));
                m_objThread.Name = "UIThread";
                m_objThread.SetApartmentState(ApartmentState.STA);
                m_objThread.Start();
                //Wait for the main window of WPF is created.
                LogProcessorService.Log.UIService.LogDebug("Wait for the main window of WPF is created");
                m_objThreadEvt.WaitOne();

                //if (!string.IsNullOrEmpty(m_objCfg.InitPage))
                //{
                //    ((IUIService)this).ShowScreen(m_objCfg.InitPage, false);
                //}
            }
            catch (System.Exception ex)
            {
                LogProcessorService.Log.UIService.LogFatal("Failed to open the UI service in WPF", ex);

                return false;
            }

            LogProcessorService.Log.UIService.LogDebug("Success to open the UI service in WPF");

            return m_IsOpened;
        }
        /// <summary>
        /// Close the UI service.
        /// </summary>
        void IUIService.Close()
        {
            if (!m_IsOpened)
            {
                return;
            }

            m_IsOpened = false;
            m_objApp.Close();
            //Wait for closing the UI thread.
            if (!m_objThread.Join(3000))
            {
                //Waiting is timeout, then abort it.
                m_objThread.Abort();
                m_objThread.Join(3000);
            }
            m_objApp = null;
        }
        public string GetCurrentScreenPropertyValue(string ID, string propertyName)
        {
            return m_objApp.GetCurrentScreenPropertyValue(ID, propertyName);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strScrName"></param>
        /// <param name="bAsyn"></param>
        /// <returns></returns>
        bool IUIService.ShowScreen(string strScrName,
                                    bool bAsyn)
        {
            Debug.Assert(m_IsOpened && null != m_objApp);
            if (!m_IsOpened ||
                null == m_objApp)
            {
                return false;
            }

            return (m_objApp.ShowScreen(strScrName, bAsyn));
        }

        bool IUIService.ShowScreen(string argScrName,
                                    object argDataContext,
                                    bool argIsAsyn)
        {
            Debug.Assert(m_IsOpened && null != m_objApp);
            if (!m_IsOpened ||
                 null == m_objApp)
            {
                return false;
            }

            return m_objApp.ShowScreen(argScrName, argDataContext, argIsAsyn);
        }

        bool IUIService.ShowScreenScript(Stream ScreenScript,
                                          bool bAsyn)
        {
            Debug.Assert(m_IsOpened && null != m_objApp);
            if (!m_IsOpened ||
                 null == m_objApp)
            {
                return false;
            }

            return m_objApp.ShowScreenScript(ScreenScript, bAsyn);
        }

        bool IUIService.ShowScreenScript(Stream ScreenScript,
                                          object argDataContext,
                                          bool bAsyn)
        {
            Debug.Assert(m_IsOpened && null != m_objApp);
            if (!m_IsOpened ||
                 null == m_objApp)
            {
                return false;
            }

            return false;
        }

        bool IUIService2.ShowDirectScreen(string argScrFile,
                                           bool argIsAsyn)
        {
            Debug.Assert(m_IsOpened && null != m_objApp);
            if (!m_IsOpened ||
                 null == m_objApp)
            {
                return false;
            }

            return m_objApp.ShowDirectScreen(argScrFile, argIsAsyn);
        }

        bool IUIService2.ShowDirectScreen(string argScrFile,
                                           object argDataContext,
                                           bool argIsAsyn)
        {
            Debug.Assert(m_IsOpened && null != m_objApp);
            if (!m_IsOpened ||
                 null == m_objApp)
            {
                return false;
            }

            return m_objApp.ShowDirectScreen(argScrFile, argDataContext, argIsAsyn);
        }

        PopupDlgResult IUIService2.ShowDirectPopup(string argScrFile,
                                                    bool argShow,
                                                    bool argIsDialog)
        {
            Debug.Assert(m_IsOpened && null != m_objApp);
            if (!m_IsOpened ||
                 null == m_objApp)
            {
                return PopupDlgResult.Fail;
            }

            return m_objApp.ShowDirectPopup(argScrFile, null, argShow, argIsDialog);
        }

        PopupDlgResult IUIService2.ShowDirectPopup(string argScrFile,
                                                    object argDataContext,
                                                    bool argShow,
                                                    bool argIsDialog)
        {
            Debug.Assert(m_IsOpened && null != m_objApp);
            if (!m_IsOpened ||
                 null == m_objApp)
            {
                return PopupDlgResult.Fail;
            }

            return m_objApp.ShowDirectPopup(argScrFile, argDataContext, argShow, argIsDialog);
        }

        object IUIService2.ExecuteFunction(ExecuteFunctionHandler argHandler,
                                            object argParam)
        {
            Debug.Assert(null != argHandler);
            if (null == argHandler ||
                 !m_IsOpened ||
                 null == m_objApp)
            {
                return null;
            }

            return m_objApp.ExecuteFunction(argHandler, argParam);
        }

        PopupDlgResult IUIService.ShowPopup(string NameOfPopup,
                                            bool argShow,
                                            bool IsDialog)
        {
            Debug.Assert(m_IsOpened && null != m_objApp);
            if (!m_IsOpened ||
                 null == m_objApp)
            {
                return PopupDlgResult.None;
            }

            return m_objApp.ShowPopup(NameOfPopup, argShow, IsDialog);
        }

        void IUIService.HidePopup()
        {
            Debug.Assert(m_IsOpened && null != m_objApp);
            m_objApp.HidePopup();
        }

        PopupDlgResult IUIService.ShowPopupScript(Stream ScreenScript,
                                                  bool argShow,
                                         bool IsDialog)
        {
            Debug.Assert(m_IsOpened && null != m_objApp);
            if (!m_IsOpened ||
                null == m_objApp)
            {
                return PopupDlgResult.None;
            }

            return m_objApp.ShowPopupScript(ScreenScript, argShow, IsDialog);
        }
        PopupDlgResult IUIService.ShowPopup(string argScrName, int timeout, bool argShow = true, bool argIsDialog = true)
        {
            Debug.Assert(m_IsOpened && null != m_objApp);
            if (!m_IsOpened ||
                 null == m_objApp)
            {
                return PopupDlgResult.None;
            }

            return m_objApp.ShowPopup(argScrName, timeout, argShow, argIsDialog);
        }
        PopupDlgResult IUIService.ShowPopup(string NameOfPopup,
                                             object argDataContext,
                                            bool argShow,
                                            bool IsDialog)
        {
            Debug.Assert(m_IsOpened && null != m_objApp);
            if (!m_IsOpened ||
                 null == m_objApp)
            {
                return PopupDlgResult.None;
            }

            return m_objApp.ShowPopup(NameOfPopup, argDataContext, argShow, IsDialog);
        }

        PopupDlgResult IUIService.ShowPopupScript(Stream ScreenScript,
                                                  object argDataContext,
                                                  bool argShow,
                                                  bool IsDialog)
        {
            Debug.Assert(m_IsOpened && null != m_objApp);
            if (!m_IsOpened ||
                 null == m_objApp)
            {
                return PopupDlgResult.None;
            }

            return m_objApp.ShowPopupScript(ScreenScript, argDataContext, argShow, IsDialog);
        }

        void IUIService.TriggerEvent(string strEvtName,
                                      bool bAsyn)
        {
            Debug.Assert(m_IsOpened && null != m_objApp);
            if (!m_IsOpened ||
                 null == m_objApp)
            {
                return;
            }

            m_objApp.TriggerEvent(strEvtName, bAsyn);
        }

        bool IUIService.SetPropertyValueOfElement(string strScrName,
                                                   string strElement,
                                                   string strProperty,
                                                   object objValue)
        {
            Debug.Assert(m_IsOpened && null != m_objApp);
            if (!m_IsOpened ||
                 null == m_objApp)
            {
                return false;
            }

            return m_objApp.SetPropertyValueOfElement(strScrName, strElement, strProperty, objValue);
        }

        bool IUIService.GetPropertyValueOfElement(string strScrName,
                                                   string strElement,
                                                   string strProperty,
                                                   out object objValue)
        {
            Debug.Assert(m_IsOpened && null != m_objApp);
            objValue = null;

            if (!m_IsOpened ||
                 null == m_objApp)
            {
                return false;
            }

            objValue = m_objApp.GetPropertyValueOfElement(strScrName, strElement, strProperty);
            return objValue == null ? false : true;
        }

        bool IUIService.Reload(string strCfg)
        {
            Debug.Assert(m_IsOpened && null != m_objApp);
            if (!m_IsOpened ||
                 null == m_objApp)
            {
                return false;
            }

            return m_objApp.Reload(strCfg);
        }

        void IUIService.RedrawUI()
        {
            Debug.Assert(m_IsOpened && null != m_objApp);
            if (!m_IsOpened ||
                 null == m_objApp)
            {
                return;
            }
            m_objApp.RedrawUI();
        }



        void IUIService.EnumScreens(EnumScreenNameHandler objHandler,
                                     object objParam)
        {
            Debug.Assert(null != m_objCfg);
            if (!m_IsOpened ||
                 null == m_objApp)
            {
                return;
            }
            m_objCfg.EnumScreens(objHandler, objParam);
        }

        void IUIService.EnumElements(EnumElementHandler Handler,
                                      ElementType Type,
                                      EnumFlag Flag,
                                      string NameOfElement,
                                      object Param)
        {
            if (!m_IsOpened ||
                null == m_objApp)
            {
                return;
            }

            m_objApp.EnumElements(Handler, Type, Flag, NameOfElement, Param);
        }

        object IUIService.ExecuteCommand(UICommandType type,
                                        string name,
                                        params object[] args)
        {
            if (!m_IsOpened ||
                 null == m_objApp)
            {
                return null;
            }

            return m_objApp.ExecuteCommand(type, name, args);

        }
        #endregion

        #region event of the IUIService interface

        public event Action<string, string,object> ValueChange;

        public void OnValueChange(string id,string property, object value)
        {
            if (ValueChange != null)
            {
                ValueChange.Invoke(id, property, value);
            }
        }

        public event UIEventHandler UIEvent;

        //Handle Raised UI events.
        public bool OnUIEventRaised(UIEventArg objArg,
                                    string argMask = null )
        {
            if (UIEvent != null)
            {
                if ( string.IsNullOrEmpty(argMask) )
                {
                    Log.UIService.LogDebugFormat("OnUIEventRaised: EventName={0}, Key={1},Element={2}", objArg.EventName, objArg.Key,objArg.ElementName);
                }
                else
                {
                    Log.UIService.LogDebugFormat("OnUIEventRaised: EventName={0}, Key={1},Element={2}", objArg.EventName, argMask,objArg.ElementName);
                }
                
                objArg.ScreenName = null != m_objApp.ActiveScreen ? m_objApp.ActiveScreen.Name : null;
                return UIEvent.Invoke(this as IUIService, objArg);
            }
            else
            {
                Log.UIService.LogError("UIEvent is null.");
            }

            return true;
        }

        //public void OnTouchDown(object argSender,
        //                          RoutedEventArgs arg)
        //{
        //    arg.Handled = true;
        //    if (arg.OriginalSource == null &&
        //         arg.Source == null)
        //    {
        //        Log.UIService.LogError("TouchDown. But original source and source is null");
        //        return;
        //    }

        //    FrameworkElement element = (FrameworkElement)arg.Source;

        //    if (null == element)
        //    {
        //        Log.UIService.LogError("TouchDown. but source and original source isn't frameworkelement");
        //        return;
        //    }

        //    if (!string.IsNullOrEmpty(element.Name))
        //    {
        //        Log.UIService.LogDebugFormat("TouchDown. Element[{0}] Tag[{1}]",
        //                                    element.Name,
        //                                    element.Tag);
        //        OnUIEventRaised(new UIEventArg()
        //        {
        //            EventName = UIEventNames.s_ClickEvent,
        //            Source = this as IUIService,
        //            ScreenName = m_objApp.ActiveScreen.Name,
        //            ElementName = element.Name,
        //            Key = element.Tag
        //        });
        //    }
        //}

        //Handle click event of a button.
        public void OnButtonClick(object objSender,
                                    RoutedEventArgs objArg)
        {
            objArg.Handled = true;
            if ( objArg.OriginalSource == null &&
                 objArg.Source == null )
            {
                Log.UIService.LogError("Button Clicked. But original source and source is null");
                return;
            }

            FrameworkElement element = null;
            if ( objArg.OriginalSource != null &&
                 objArg.OriginalSource is FrameworkElement )
            {
                element = (FrameworkElement)objArg.OriginalSource;
            }
            else if ( objArg.Source != null &&
                      objArg.Source is FrameworkElement )
            {
                element = (FrameworkElement)objArg.Source;
            }

            if ( null == element )
            {
                Log.UIService.LogError("Button clicked. but source and original source isn't frameworkelement");
                return;
            }

            Log.UIService.LogDebugFormat("Button clicked. Element[{0}] Tag[{1}]",
                                            element.Name,
                                            element.Tag);
            OnUIEventRaised(new UIEventArg()
            {
                EventName = UIEventNames.s_ClickEvent,
                Source = this as IUIService,
                ScreenName = m_objApp.ActiveScreen.Name,
                ElementName = element.Name,
                Key = element.Tag
            });
        }

        public void OnChecked(object argSender,
                                RoutedEventArgs argEvtArg)
        {
            argEvtArg.Handled = true;
            OnUIEventRaised(new UIEventArg()
                {
                    EventName = UIEventNames.s_checkedEvent,
                    Source = this as IUIService,
                    ScreenName = m_objApp.ActiveScreen.Name,
                    ElementName = ((FrameworkElement)argEvtArg.OriginalSource
                    ).Name,
                    Key = ((FrameworkElement)argEvtArg.OriginalSource).Tag
                });
        }

        public void OnUnchecked(object argSender,
                                  RoutedEventArgs argEvtArg)
        {
            argEvtArg.Handled = true;
            OnUIEventRaised(new UIEventArg()
                {
                    EventName = UIEventNames.s_uncheckedEvent,
                    Source = this as IUIService,
                    ScreenName = m_objApp.ActiveScreen.Name,
                    ElementName = ((FrameworkElement)argEvtArg.OriginalSource
                    ).Name,
                    Key = ((FrameworkElement)argEvtArg.OriginalSource).Tag
                });
        }

        //add by cming
        public void OnDrop(object objSender,
                                    RoutedEventArgs objArg)
        {
            //objArg.Handled = true;
            OnUIEventRaised(new UIEventArg()
            {
                EventName = UIEventNames.s_DropEvent,
                Source = this as IUIService,
                ScreenName = m_objApp.ActiveScreen.Name,
                ElementName = ((FrameworkElement)objArg.OriginalSource
                ).Name,
                Key = ((FrameworkElement)objArg.OriginalSource).Tag,
                Param = objArg
            });
        }

        private void OnMouseDown(object argSender,
                                  MouseButtonEventArgs arg)
        {
            arg.Handled = true;
            if (arg.OriginalSource == null &&
                 arg.Source == null)
            {
                Log.UIService.LogError("MouseDown. But original source and source is null");
                return;
            }

            FrameworkElement element = null;
            if (arg.OriginalSource != null &&
                 arg.OriginalSource is FrameworkElement)
            {
                element = (FrameworkElement)arg.OriginalSource;
            }
            else if (arg.Source != null &&
                      arg.Source is FrameworkElement)
            {
                element = (FrameworkElement)arg.Source;
            }

            if (null == element)
            {
                Log.UIService.LogError("MouseDown. but source and original source isn't frameworkelement");
                return;
            }

            Log.UIService.LogDebugFormat("MouseDown. Element[{0}]",
                                            element.Name,
                                            element.Tag);
            OnUIEventRaised(new UIEventArg()
            {
                EventName = UIEventNames.s_MouseDown,
                Source = this as IUIService,
                ScreenName = m_objApp.ActiveScreen.Name,
                ElementName = element.Name,
                Key = element.Tag
            });
        }

        private void OnMouseUp(object argSender,
                                MouseButtonEventArgs arg)
        {
            Debug.Assert(null != m_objApp);
            m_objApp.OnMouseUp(argSender, arg);
        }
        /// <summary>
        /// SelectionChanged event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //封装在DraggableListView控件中
            //if (e.OriginalSource is ListBox)
            //{
            //    ListBox listBox = e.OriginalSource as ListBox;
            //    listBox.ScrollIntoView(listBox.SelectedItem);
            //}
            e.Handled = true;
            OnUIEventRaised(new UIEventArg()
            {
                EventName = UIEventNames.s_ListBoxSelectionChanged,
                Source = this as IUIService,
                ScreenName = m_objApp.ActiveScreen.Name,
                ElementName = ((FrameworkElement)e.OriginalSource
                ).Name,
                Key = ((FrameworkElement)e.OriginalSource).Tag
            });
        }

        #endregion

        #region method
        void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            if (!FailureFromFocusedChild(e.Exception))
            {
                // if it's not a this error , then raise and restart the vtm
                Log.UIService.LogFatal("There is a unhanded tread error in UIServiceInWPF", e.Exception.Message);

                // don't raise , I think no such case
                //OnUIEventRaised(new UIEventArg()
                //{
                //    EventName = UIEventNames.s_UIFatalError
                //});
            }
            else
                Log.UIService.LogInfo("Thread exception:" + e.Exception.Message);

        }


        private bool FailureFromFocusedChild(Exception e)
        {
            bool result = false;
            string stackTrace = e.StackTrace;

            result = (e is System.ArgumentException) && (e.Source == "System.Windows.Forms")
                    && (stackTrace.IndexOf("System.Windows.Forms.Integration.WindowsFormsHost.RestoreFocusedChild") >= 0);


            return result;
        }
        /// <summary>
        /// The UI thread routine.
        /// </summary>
        private void OnThreadFun()
        {
            LogProcessorService.Log.UIService.LogInfo("Prepare for running a thread to start the UI service in WPF");

            int nRet = 0;
            try
            {
                //UIServiceWpfApp objApp = new UIServiceWpfApp();
                //create a WPF application.
                m_objApp = new UIServiceWpfApp()
                {
                    CfgOfUIService = m_objCfg,
                    MainThreadID = Thread.CurrentThread.ManagedThreadId,
                    Parent = this,
                    ResourceManager = m_objCfg.ResouceManager
                };

                System.Windows.Forms.Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);

                //handle startup events
                m_objApp.Startup += (argSender, arg) =>
                    {
                        //show init screen in Startup event
                        m_IsOpened = true;
                        m_objThreadEvt.Set();
                    };

                m_objApp.DispatcherUnhandledException += (argSender, arg) =>
                    {
                        //handle unhanded exception.
                        arg.Handled = true;
                        Log.UIService.LogFatal("There is a unhanded exception in UIServiceInWPF", arg.Exception);

                        if (arg.Exception is System.Runtime.InteropServices.COMException)
                        {
                            OnUIEventRaised(new UIEventArg()
                                {
                                    EventName = UIEventNames.s_UIFatalError
                                });
                        }
                    };

                m_objApp.SessionEnding += (argSender, arg) =>
                    {
                        Log.UIService.LogInfo("The UI Service received a session ending message");
                        //if ( arg.ReasonSessionEnding == ReasonSessionEnding.Logoff )
                        //{
                        //    Log.UIService.LogInfo("The user logoffs, then cancel it");
                        //    arg.Cancel = true;
                        //}
                    };

                m_objApp.Exit += (argSender, arg) =>
                    {
                        Log.UIService.LogInfo("The UI Service has been exit");
                    };
                //prepare for loading the main window.
             //   Window objWnd = null; // don't load wpf 
                LogProcessorService.Log.UIService.LogInfo("Load the configuration file of the UI service in WPF");
                //               using (FileStream fs = new FileStream(m_objCfg.UrlOfMotherboard, FileMode.Open, FileAccess.Read))
                //Stream content = null;
                //if (m_objCfg.IsLocal)
                //{
                //    content = new FileStream(m_objCfg.UrlOfMotherboard, FileMode.Open, FileAccess.Read);
                //}
                //else
                //{

                //}

                HtmlRender.SingleInstance.Open(m_objApp);
                WindowConfig wndCfg = m_objCfg.WndConfig;
                //if ( !wndCfg.ShowCursor )
                //{
                //    objWnd.Cursor = Cursors.None;
                //}
                //HtmlRender.SingleInstance.ShowCursor = wndCfg.ShowCursor;
                Interop.NativeWndApi.ShowCursor(wndCfg.ShowCursor ? 1 : 0);
                //  objWnd.Topmost = wndCfg.Topmost;
                HtmlRender.SingleInstance.Topmost = wndCfg.Topmost;

                // add by wjw
                HtmlRender.SingleInstance.Left = wndCfg.Left;
                HtmlRender.SingleInstance.Top = wndCfg.Top;
                HtmlRender.SingleInstance.Width = wndCfg.Width;
                HtmlRender.SingleInstance.Height = wndCfg.Height;

                //using (Stream fs = content)
                //{
                //    LogProcessorService.Log.UIService.LogInfo("Load and parse configuration of the UI service");
                //    objWnd = (Window)XamlReader.Load(fs);
                //    Debug.Assert(null != objWnd);
                //    //objWnd.Visibility = Visibility.Hidden;
                //    HtmlRender.SingleInstance.Open(m_objApp);
                //    WindowConfig wndCfg = m_objCfg.WndConfig;
                //    //if ( !wndCfg.ShowCursor )
                //    //{
                //    //    objWnd.Cursor = Cursors.None;
                //    //}
                //    //HtmlRender.SingleInstance.ShowCursor = wndCfg.ShowCursor;
                //    Interop.NativeWndApi.ShowCursor(wndCfg.ShowCursor ? 1 : 0);
                //  //  objWnd.Topmost = wndCfg.Topmost;
                //    HtmlRender.SingleInstance.Topmost = wndCfg.Topmost;

                //    // add by wjw
                //    HtmlRender.SingleInstance.Left = wndCfg.Left;
                //    HtmlRender.SingleInstance.Top = wndCfg.Top;
                //    HtmlRender.SingleInstance.Width = wndCfg.Width;
                //    HtmlRender.SingleInstance.Height = wndCfg.Height;

                //    //XDCCanvasForm.Instance.TopMost = wndCfg.Topmost;
                //    if (wndCfg.FullScreen)
                //    {
                //        objWnd.Left = 0;
                //        objWnd.Top = 0;
                //        objWnd.Height = SystemParameters.PrimaryScreenHeight;
                //        objWnd.Width = SystemParameters.PrimaryScreenWidth;

                //        HtmlRender.SingleInstance.Left = 0;
                //        HtmlRender.SingleInstance.Top = 0;
                //        HtmlRender.SingleInstance.Height = (int)SystemParameters.PrimaryScreenHeight;
                //        HtmlRender.SingleInstance.Width = (int)SystemParameters.PrimaryScreenWidth;

                //      //  XDCCanvasForm.Instance.Left = 0;
                //      //  XDCCanvasForm.Instance.Top = 0;
                //      //  XDCCanvasForm.Instance.Size = new System.Drawing.Size((int)SystemParameters.PrimaryScreenHeight,
                //                                                                //(int)SystemParameters.PrimaryScreenWidth);
                //    }
                //    else
                //    {
                //        objWnd.Left = wndCfg.Left;
                //        objWnd.Top = wndCfg.Top;
                //        objWnd.Width = wndCfg.Width;
                //        objWnd.Height = wndCfg.Height;

                //        HtmlRender.SingleInstance.Left = wndCfg.Left;
                //        HtmlRender.SingleInstance.Top = wndCfg.Top;
                //        HtmlRender.SingleInstance.Width = wndCfg.Width;
                //        HtmlRender.SingleInstance.Height = wndCfg.Height;

                //     //   XDCCanvasForm.Instance.Left = wndCfg.Left;
                //       // XDCCanvasForm.Instance.Top = wndCfg.Top;
                //       // XDCCanvasForm.Instance.Size = new System.Drawing.Size(wndCfg.Width,
                //                                                               //wndCfg.Height);
                //    }
                //    //m_objApp.ShowScreen(m_objCfg.InitPage, false);
                //}

                //open html render class
                m_objApp.CfgOfUIService = m_objCfg;
              //  SetupEventHandles(objWnd);
               // XDCCanvasForm.Instance.ParentApp = m_objApp;
                //Set height and width of the main window.
                m_objApp.MainWindowHeight = wndCfg.Height;
                m_objApp.MainWindowWidth = wndCfg.Width;
                //LogProcessorService.Log.UIService.LogInfo("Notify the another thread the main window has been loaded");
                //m_IsOpened = true;
                //m_objThreadEvt.Set();
                //run
               // objWnd.Closing += new System.ComponentModel.CancelEventHandler(objWnd_Closing);
                //m_objApp.MainWindow = objWnd;
                LogProcessorService.Log.UIService.LogInfo("run the application of the UI service in WPF");
                //nRet = m_objApp.Run(objWnd);
                m_objApp.InitializeGlobalSettings();
                //objWnd.Loaded += (argSender, e) =>
                //{
                //    HwndSource source = PresentationSource.FromVisual(objWnd) as HwndSource;
                //    if (null != source)
                //    {
                //        source.AddHook((IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) =>
                //        {
                //            handled = false;

                //            if (msg == 0x007E)//当显示器的分辨率改变后发送给所有窗口的消息
                //            {
                //                handled = true;

                //                Log.UIService.LogDebug("A WM_DISPLAYChange message has received");
                //                int resolution = lParam.ToInt32();
                //                int h = resolution & 0x0000FFFF;
                //                int v = ((int)(resolution & 0xFFFF0000)) >> 16;
                //                Log.UIService.LogDebugFormat("TotalWidth[{0}] TotalHeight[{1}] MainWidth[{2}] Height[{3}]",
                //                                             SystemParameters.VirtualScreenWidth,
                //                                             SystemParameters.VirtualScreenHeight,
                //                                             h, v);
                //                Log.UIService.LogDebugFormat("Current window before: left[{0}] top[{1}] width[{2}] height[{3}]",
                //                                             objWnd.Left,
                //                                             objWnd.Top,
                //                                             objWnd.Width,
                //                                             objWnd.Height);
                //                WindowConfig wndCfg = m_objCfg.WndConfig;
                //                objWnd.Left = wndCfg.Left;
                //                objWnd.Top = wndCfg.Top;
                //                objWnd.Width = wndCfg.Width;
                //                objWnd.Height = wndCfg.Height;

                //                Log.UIService.LogDebugFormat("Current window after: left[{0}] top[{1}] width[{2}] height[{3}]",
                //                objWnd.Left,
                //                objWnd.Top,
                //                objWnd.Width,
                //                objWnd.Height);

                //                Thread.Sleep(500);

                //                Log.UIService.LogDebugFormat("Current window before: left[{0}] top[{1}] width[{2}] height[{3}]",
                //         objWnd.Left,
                //         objWnd.Top,
                //         objWnd.Width,
                //         objWnd.Height);
                //                wndCfg = m_objCfg.WndConfig;
                //                objWnd.Left = wndCfg.Left;
                //                objWnd.Top = wndCfg.Top;
                //                objWnd.Width = wndCfg.Width;
                //                objWnd.Height = wndCfg.Height;

                //                Log.UIService.LogDebugFormat("Current window after: left[{0}] top[{1}] width[{2}] height[{3}]",
                //                objWnd.Left,
                //                objWnd.Top,
                //                objWnd.Width,
                //                objWnd.Height);

                //                int result = MoveWindow(m_objApp.HandleOfMainWindow, (int)objWnd.Left, (int)objWnd.Top, (int)objWnd.Width, (int)objWnd.Height, true);
                //                //int result = SetWindowPos(m_objApp.HandleOfMainWindow, new IntPtr(-2), (int)objWnd.Left, (int)objWnd.Top, (int)objWnd.Width, (int)objWnd.Height, 1);
                //                if (result != 1)
                //                {
                //                    int err = Marshal.GetLastWin32Error();
                //                    throw new System.ComponentModel.Win32Exception(err);
                //                }
                //            }

                //            return hwnd;
                //        });
                //    }
                //};
                nRet = m_objApp.Run();
            }
            catch (System.Exception ex)
            {
                LogProcessorService.Log.UIService.LogError("Failed to run the thread", ex);

                m_IsOpened = false;
                m_objThreadEvt.Set();

                return;
            }

            LogProcessorService.Log.UIService.LogInfoFormat("The result of the application running is {0}", nRet);
        }

        void objWnd_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }

        //Setup event handles 
        private void SetupEventHandles(Window Mainwnd)
        {
            Debug.Assert(null != Mainwnd);
            //Add button clicked.
            Mainwnd.AddHandler(Button.ClickEvent, new RoutedEventHandler(OnButtonClick));
            //Add check event
            Mainwnd.AddHandler(ToggleButton.CheckedEvent, new RoutedEventHandler(OnChecked));
            //Add uncheck event
            Mainwnd.AddHandler(ToggleButton.UncheckedEvent, new RoutedEventHandler(OnUnchecked));

            Mainwnd.PreviewKeyDown += new KeyEventHandler(Mainwnd_PreviewKeyDown);

            Mainwnd.PreviewKeyUp += new KeyEventHandler(Mainwnd_PreviewKeyUp);
            //Mouse down event handle
            Mainwnd.AddHandler(UIElement.MouseDownEvent, new MouseButtonEventHandler(OnMouseDown));
            ////Mouse up event handle
            //Mainwnd.AddHandler(UIElement.MouseUpEvent, new MouseButtonEventHandler(OnMouseUp));
            //ListBox's selection changed event handler
            Mainwnd.AddHandler(ListBox.SelectionChangedEvent, new SelectionChangedEventHandler(OnSelectionChanged), false);
            ////add by cming
            //Mainwnd.AddHandler(UIElement.DropEvent, new RoutedEventHandler(OnDrop));

            //Mainwnd.AddHandler(Button.PreviewMouseLeftButtonDownEvent, new RoutedEventHandler(OnPreviewMouseLeftButtonDown));

            //Mainwnd.AddHandler(Button.PreviewMouseLeftButtonUpEvent, new RoutedEventHandler(OnPreviewMouseLeftButtonUp));

            //Mainwnd.AddHandler(Button.MouseLeftButtonDownEvent, new RoutedEventHandler(OnMouseLeftButtonDown));

            //Mainwnd.AddHandler(Button.MouseLeftButtonUpEvent, new RoutedEventHandler(OnMouseLeftButtonUp));
            
            //Mainwnd.AddHandler(UIElement.TouchDownEvent, new RoutedEventHandler(OnTouchDown));
         
        }
       
        void Mainwnd_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if ( e.SystemKey == Key.LeftAlt ||
                 e.SystemKey == Key.RightAlt || e.Key == Key.LWin || e.Key == Key.RWin)
            {
                m_altHold = false;
                e.Handled = true;
            }
        
        }

        void Mainwnd_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ( e.SystemKey == Key.LeftAlt ||
                 e.SystemKey == Key.RightAlt)
            {
                m_altHold = true;
            }
            else if (e.SystemKey == Key.F4  &&
                      m_altHold )
            {
                e.Handled = true;
            }
            else if (e.SystemKey == Key.Space && m_altHold)
            {
                e.Handled = true;
            }
            else if (e.Key == Key.LWin || e.Key == Key.RWin)
            {
                e.Handled = true;
                
            }
        }

        private void OnPreviewMouseLeftButtonDown(object objSender,
                                    RoutedEventArgs objArg)
        { 
            Log.UIService.LogDebugFormat("Preview mouse Left button down. Element[{0}] Tag[{1}]",
                                        ((FrameworkElement)objArg.OriginalSource).Name,
                                        ((FrameworkElement)objArg.OriginalSource).Tag);
            
        }

        private void OnMouseLeftButtonDown(object objSender,
                            RoutedEventArgs objArg)
        {
            
            Log.UIService.LogDebugFormat("Mouse Left button down. Element[{0}] Tag[{1}]",
                                        ((FrameworkElement)objArg.OriginalSource).Name,
                                        ((FrameworkElement)objArg.OriginalSource).Tag);
        }

        private void OnMouseLeftButtonUp(object objSender,
                            RoutedEventArgs objArg)
        {
           
            Log.UIService.LogDebugFormat("Mouse Left button up. Element[{0}] Tag[{1}]",
                                        ((FrameworkElement)objArg.OriginalSource).Name,
                                        ((FrameworkElement)objArg.OriginalSource).Tag);
        }

        private void OnPreviewMouseLeftButtonUp(object objSender,
                            RoutedEventArgs objArg)
        {
           
            Log.UIService.LogDebugFormat("Preview mouse Left button up. Element[{0}] Tag[{1}]",
                                        ((FrameworkElement)objArg.OriginalSource).Name,
                                        ((FrameworkElement)objArg.OriginalSource).Tag);
        }

        #endregion

        #region property of the IUIService interface
        int IUIService.LastErrorCode
        {
            get
            {
                return m_nLastErrCode;
            }
        }

        string IUIService.Name
        {
            get
            {
                return m_strName;
            }
        }

        bool IUIService.Opened
        {
            get
            {
                return m_IsOpened;
            }
        }

        int IUIService.Width
        {
            get
            {
                return (int)m_objApp.MainWindowWidth;
            }
        }

        int IUIService.Height
        {
            get
            {
                return (int)m_objApp.MainWindowHeight;
            }
        }

        string IUIService.CurrentScreenName
        {
            get
            {
                return m_objApp.CurrentScreenName;
            }
        }

        bool IUIService.Visible
        {
            get
            {
                if (!m_IsOpened ||
                     null == m_objApp)
                {
                    return false;
                }

                return m_objApp.MainWindow.Visibility == Visibility.Visible ? true : false;
            }
            set
            {
                if (!m_IsOpened ||
                     null == m_objApp)
                {
                    return;
                }

                m_objApp.SetVisible(value);
            }
        }

        //string IUIService.CurrentLanguage
        //{
        //    get
        //    {
        //        return m_objApp.CurrentLanguage;
        //    }
        //    set
        //    {
        //        m_objApp.CurrentLanguage = value;
        //    }
        //}

        IResourceManager IUIService.ResoureManager
        {
            get
            {
                Debug.Assert(null != m_objApp);
                return m_objApp.ResourceManager;
            }
            set
            {
                Debug.Assert(null != m_objApp);
                m_objApp.ResourceManager = value;
            }
        }

        IntPtr IUIService.HandleOfMainWindow
        {
            get
            {
                if (null == m_objApp)
                {
                    return IntPtr.Zero;
                }

                return m_objApp.HandleOfMainWindow;
            }
        }
        #endregion

        #region field
        private string m_strName = "UIService in WPF";

        private UIServiceCfg m_objCfg = new UIServiceCfg();

        private int m_nLastErrCode = 0;

        private bool m_IsOpened = false;

        private IntPtr m_hWndParent = IntPtr.Zero;

        private UIServiceWpfApp m_objApp = null;

        private Thread m_objThread = null;

        private AutoResetEvent m_objThreadEvt = new AutoResetEvent(false);

        private bool m_altHold = false;
        #endregion
    }

    public delegate void CloseHandler();

    public delegate void RedrawUIHandler();

    public delegate object GetPropertyValueHandler(string strScreen,
                                                  string strElement,
                                                  string strProperty);

    public delegate object ExecuteCommandHandler(UICommandType type,
                                                  string name,
                                                  params object[] args);

    public class UIServiceWpfApp : Application
    {
        #region constructor
        public UIServiceWpfApp()
        {
            //create handlers
            m_ShowScreenHandler = new Func<string, bool>(OnShowScreen);
            m_ShowScreen2Handler = new Func<string, object, bool>(OnShowScreen);
            m_ShowPopupHandler = new Func<string, object, bool, bool, PopupDlgResult>(OnShowPopup);
            m_HidePopupHandler = new Action(OnHidePopup);
            m_ShowScreenScriptHandler = new Func<Stream, bool>(OnShowScreenScript);
            m_ShowPopupScriptHandler = new Func<Stream, object, bool, bool, PopupDlgResult>(OnShowPopupScript);
            m_showDirectScreenHandler = new Func<string, bool>(OnShowDirectScreen);
            m_showDirectScreen2Handler = new Func<string, object, bool>(OnShowDirectScreen);
            m_showDirectPopupHandler = new Func<string, object, bool, bool, PopupDlgResult>(OnShowDirectPopup);
            m_CloseHandler = new CloseHandler(OnClose);
            m_RedrawUIHandler = new RedrawUIHandler(OnRedrawUI);
            m_SetValueHandler = new Func<string, string, string, object, bool>(OnSetPropertyValueOfElement);
            m_GetValueHandler = new GetPropertyValueHandler(OnGetPropertyValueOfElement);
            m_TriggerHandler = new Action<string>(OnTriggerEvent);
            m_enumElementHandler = new Action<EnumElementHandler, ElementType, EnumFlag, string, object>(OnEnumElements);
            m_executeCommandHandler = new ExecuteCommandHandler(OnExcuteCommand);
            m_reloadHandler = new Func<string, bool>(OnReload);
            m_visibleHandler = new Action<bool>(OnSetVisible);
            // m_changeLanguageHandler = new Action<string>(OnChangeLanguage);
        }
        #endregion

        #region method
        public void SetVisible(bool argVisible)
        {
            Dispatcher.Invoke(m_visibleHandler, argVisible);
        }

        public string GetCurrentScreenPropertyValue(string ID, string propertyName)
        {
            return HtmlRender.SingleInstance.GetCurrentScreenPropertyValue(ID, propertyName);
        }
        public void OnSetVisible(bool argVisible)
        {
            if (null == m_objActScreen)
            {
                return;
            }

            if (argVisible)
            {
                if (m_objActScreen is htmlScreen)
                {
                    if (!HtmlRender.SingleInstance.IsVisible)
                    {
                        HtmlRender.SingleInstance.ShowScreen();
                    }
                }
                //else if (m_objActScreen is FormScreen)
                //{
                //    if (!XDCCanvasForm.Instance.Visible)
                //    {
                //        XDCCanvasForm.Instance.Show();
                //    }
                //}
                else
                {
                    if (MainWindow.Visibility != Visibility.Visible)
                    {
                        MainWindow.Visibility = Visibility.Visible;
                        MainWindow.Activate();
                    }
                }
            }
            else
            {
                if (m_objActScreen is htmlScreen)
                {
                    if (HtmlRender.SingleInstance.IsVisible)
                    {
                        HtmlRender.SingleInstance.HideScreen(false);
                    }
                }
                //else if (m_objActScreen is FormScreen)
                //{
                //    if (XDCCanvasForm.Instance.Visible)
                //    {
                //        XDCCanvasForm.Instance.Hide();
                //    }
                //}
                else
                {
                    if (MainWindow.Visibility == Visibility.Visible)
                    {
                        MainWindow.Visibility = Visibility.Hidden;
                    }
                }
            }
        }
        /// <summary>
        /// Show a screen
        /// </summary>
        /// <param name="strScrName">name of a screen</param>
        /// <param name="bAsyn">in asynchronously</param>
        /// <returns>If success, it returns true. otherwise, it returns false.</returns>
        public bool ShowScreen(string strScrName,
                                bool bAsyn = false)
        {
            Log.UIService.LogDebugFormat("Prepare for show screen[{0}] by asyn[{1}]", strScrName, bAsyn);
            if (MainThreadID == Thread.CurrentThread.ManagedThreadId)
            {
                Log.UIService.LogDebug("The call thread is the UI thread, then must asynchronous invoke it");
                bAsyn = true;
            }

            if (bAsyn)
            {
                //Log.UIService.LogDebug("Show screen in asynchronous");
                Dispatcher.BeginInvoke(m_ShowScreenHandler, DispatcherPriority.Normal, strScrName);
            }
            else
            {
                //Log.UIService.LogDebug("Prepare for get a lock to show screen in synchronous");
                lock (m_syncShowScreen)
                {
                    // Log.UIService.LogDebug("Success to get lock");
                    m_ShowScreenSyn.Reset();

                    //Log.UIService.LogDebug("Prepare for show screen in synchronous");
                    Dispatcher.Invoke(m_ShowScreenHandler, DispatcherPriority.Normal, strScrName);
                    //Log.UIService.LogDebug("Wait for to show screen is completed");
                    if (!m_ShowScreenSyn.WaitOne(30000))
                    {
                        Log.UIService.LogWarn("Waiting to show screen is timeout");
                        return false;
                    }
                    m_ShowScreenSyn.Reset();

                    Log.UIService.LogDebug("Success to show screen");
                }
            }

            return true;
        }

        public bool ShowScreen(string argScrName,
                                object argDataContext,
                                bool argAsyn = false)
        {
            Log.UIService.LogDebugFormat("Prepare for show screen[{0}] by asyn[{1}]", argScrName, argAsyn);
            if (MainThreadID == Thread.CurrentThread.ManagedThreadId)
            {
                Log.UIService.LogDebug("The call thread is the UI thread, then must asynchronous invoke it");
                argAsyn = true;
            }

            if (argAsyn)
            {
                //Log.UIService.LogDebug("Show screen in asynchronous");
                Dispatcher.BeginInvoke(m_ShowScreen2Handler, DispatcherPriority.Normal, argScrName, argDataContext);
            }
            else
            {
                //Log.UIService.LogDebug("Prepare for get a lock to show screen in synchronous");
                lock (m_syncShowScreen)
                {
                    // Log.UIService.LogDebug("Success to get lock");
                    m_ShowScreenSyn.Reset();

                    //Log.UIService.LogDebug("Prepare for show screen in synchronous");
                    Dispatcher.Invoke(m_ShowScreen2Handler, DispatcherPriority.Normal, argScrName, argDataContext);
                    //Log.UIService.LogDebug("Wait for to show screen is completed");
                    if (!m_ShowScreenSyn.WaitOne(30000))
                    {
                        Log.UIService.LogWarn("Waiting to show screen is timeout");
                        return false;
                    }
                    m_ShowScreenSyn.Reset();

                    Log.UIService.LogDebug("Success to show screen");
                }
            }

            return true;
        }

        public bool ShowScreenScript(Stream ScreenScript,
                                      bool bAsyn = false)
        {
            if (bAsyn)
            {
                Dispatcher.BeginInvoke(m_ShowScreenScriptHandler, DispatcherPriority.Normal, ScreenScript);
            }
            else
            {
                return (bool)Dispatcher.Invoke(m_ShowScreenScriptHandler, DispatcherPriority.Send, ScreenScript);
            }

            return true;
        }

        public bool ShowScreenScript(Stream argScrScript,
                                      object argDataContext,
                                      bool argAsyn = false)
        {
            return false;
        }

        public PopupDlgResult ShowPopup(string NameOfPopup,
                               bool bShow,
                               bool bIsDialog)
        {
            if (MainThreadID == Thread.CurrentThread.ManagedThreadId)
            {
                if (Monitor.TryEnter(m_syncShowPopup))
                {
                    try
                    {
                        return (PopupDlgResult)Dispatcher.Invoke(m_ShowPopupHandler,
                                                                 DispatcherPriority.Normal,
                                                                 NameOfPopup,
                                                                 null,
                                                                 bShow,
                                                                 bIsDialog);
                    }
                    finally
                    {
                        Monitor.Exit(m_syncShowPopup);
                    }
                }
                else
                {
                    return PopupDlgResult.None;
                }
            }
            else
            {
                lock (m_syncShowPopup)
                {
                    return (PopupDlgResult)Dispatcher.Invoke(m_ShowPopupHandler,
                                                             DispatcherPriority.Normal,
                                                             NameOfPopup,
                                                             null,
                                                             bShow,
                                                             bIsDialog);
                }
            }

        }

        public PopupDlgResult ShowPopup(string argName,
                                        int argTimeout,
                                        bool argShow,
                                        bool argIsDialog)
        {
            Log.UIService.LogDebug("Call ShowPopup(" + argName + ")");
            if (MainThreadID == Thread.CurrentThread.ManagedThreadId)
            {
                if (Monitor.TryEnter(m_syncShowPopup))
                {
                    try
                    {
                        return (PopupDlgResult)Dispatcher.Invoke(m_ShowPopupHandler,
                                                                DispatcherPriority.Normal,
                                                                argName,
                                                                argTimeout,
                                                                argShow,
                                                                argIsDialog);
                    }
                    finally
                    {
                        Monitor.Exit(m_syncShowPopup);
                    }
                }
                else
                {
                    return PopupDlgResult.None;
                }
            }
            else
            {
                lock (m_syncShowPopup)
                {
                    return (PopupDlgResult)Dispatcher.Invoke(m_ShowPopupHandler,
                                                            DispatcherPriority.Normal,
                                                            argName,
                                                            argTimeout,
                                                            argShow,
                                                            argIsDialog);
                }
            }
        }

        public PopupDlgResult ShowPopup(string argName,
                                         object argDataContext,
                                         bool argShow,
                                         bool argIsDialog)
        {
            Log.UIService.LogDebug("Call ShowPopup(" + argName + ")");
            if (MainThreadID == Thread.CurrentThread.ManagedThreadId)
            {
                if (Monitor.TryEnter(m_syncShowPopup))
                {
                    try
                    {
                        return (PopupDlgResult)Dispatcher.Invoke(m_ShowPopupHandler,
                                                                DispatcherPriority.Normal,
                                                                argName,
                                                                argDataContext,
                                                                argShow,
                                                                argIsDialog);
                    }
                    finally
                    {
                        Monitor.Exit(m_syncShowPopup);
                    }
                }
                else
                {
                    return PopupDlgResult.None;
                }
            }
            else
            {
                lock (m_syncShowPopup)
                {
                    return (PopupDlgResult)Dispatcher.Invoke(m_ShowPopupHandler,
                                                            DispatcherPriority.Normal,
                                                            argName,
                                                            argDataContext,
                                                            argShow,
                                                            argIsDialog);
                }
            }
        }

        public void HidePopup()
        {
            Log.UIService.LogDebug("Call HidePopup()");
            if (MainThreadID == Thread.CurrentThread.ManagedThreadId)
            {
                if (Monitor.TryEnter(m_syncHidePopup))
                {
                    try
                    {
                        Dispatcher.Invoke(m_HidePopupHandler, DispatcherPriority.Normal);
                    }
                    finally
                    {
                        Monitor.Exit(m_syncHidePopup);
                    }
                }
            }
            else
            {
                lock (m_syncHidePopup)
                {
                    Dispatcher.Invoke(m_HidePopupHandler, DispatcherPriority.Normal);
                }
            }

        }

        public PopupDlgResult ShowPopupScript(Stream ScreenScript,
                                     bool bShow,
                                     bool bIsDialog)
        {
            if (MainThreadID == Thread.CurrentThread.ManagedThreadId)
            {
                if (Monitor.TryEnter(m_syncShowPopup))
                {
                    try
                    {
                        return (PopupDlgResult)Dispatcher.Invoke(m_ShowPopupScriptHandler,
                                                                 DispatcherPriority.Normal,
                                                                 ScreenScript,
                                                                 null,
                                                                 bShow,
                                                                 bIsDialog);
                    }
                    finally
                    {
                        Monitor.Exit(m_syncShowPopup);
                    }
                }
                else
                {
                    return PopupDlgResult.None;
                }
            }
            else
            {
                lock (m_syncShowPopup)
                {
                    return (PopupDlgResult)Dispatcher.Invoke(m_ShowPopupScriptHandler,
                                                             DispatcherPriority.Normal,
                                                             ScreenScript,
                                                             null,
                                                             bShow,
                                                             bIsDialog);
                }
            }
        }

        public PopupDlgResult ShowPopupScript(Stream argScrScript,
                                               object argDataContext,
                                               bool argShow,
                                               bool argIsDialg)
        {
            if (MainThreadID == Thread.CurrentThread.ManagedThreadId)
            {
                if (Monitor.TryEnter(m_syncShowPopup))
                {
                    try
                    {
                        return (PopupDlgResult)Dispatcher.Invoke(m_ShowPopupScriptHandler,
                                                     DispatcherPriority.Normal,
                                                     argScrScript,
                                                     argDataContext,
                                                     argShow,
                                                     argIsDialg);
                    }
                    finally
                    {
                        Monitor.Exit(m_syncShowPopup);
                    }
                }
                else
                {
                    return PopupDlgResult.None;
                }
            }
            else
            {
                lock (m_syncShowPopup)
                {
                    return (PopupDlgResult)Dispatcher.Invoke(m_ShowPopupScriptHandler,
                                                 DispatcherPriority.Normal,
                                                 argScrScript,
                                                 argDataContext,
                                                 argShow,
                                                 argIsDialg);
                }
            }
        }

        public void TriggerEvent(string strEvtname,
                                  bool bAsyn = false)
        {
            if (bAsyn)
            {
                Dispatcher.BeginInvoke(m_TriggerHandler, DispatcherPriority.Normal, strEvtname);
            }
            else
            {
                Dispatcher.Invoke(m_TriggerHandler, DispatcherPriority.Normal, strEvtname);
            }
        }

        public void Close()
        {
            Dispatcher.Invoke(m_CloseHandler);
        }

        public void RedrawUI()
        {
            Dispatcher.BeginInvoke(m_RedrawUIHandler);
        }

        public void EnumElements(EnumElementHandler Handler,
                                  ElementType Type,
                                  EnumFlag Flag,
                                  string NameOfElement,
                                  object Param)
        {
            Dispatcher.Invoke(m_enumElementHandler,
                               DispatcherPriority.Normal,
                               Handler,
                               Type,
                               Flag,
                               NameOfElement,
                               Param);
        }

        public bool SetPropertyValueOfElement(string strScreen,
                                               string strElement,
                                               string strProperty,
                                               object objValue)
        {
            return (bool)Dispatcher.Invoke(m_SetValueHandler,
                                            DispatcherPriority.Normal,
                                            strScreen, strElement, strProperty, objValue);
        }

        public object GetPropertyValueOfElement(string strScreen,
                                               string strElement,
                                               string strProperty)
        {
            //return OnGetPropertyValueOfElement(strScreen, strElement, strProperty, out objValue);
            //objValue = null;

            return Dispatcher.Invoke(m_GetValueHandler,
                                            DispatcherPriority.Normal,
                                            strScreen,
                                            strElement,
                                            strProperty);
        }

        public object ExecuteCommand(UICommandType type,
                                      string name,
                                      params object[] args)
        {
            if (type == UICommandType.Custom &&
                 string.Equals(name, UIServiceCommands.s_buttonClick, StringComparison.OrdinalIgnoreCase))
            {
                Dispatcher.Invoke(m_executeCommandHandler,
                                   DispatcherPriority.Normal,
                                   type,
                                   UIServiceCommands.s_buttonDown,
                                   args);
                Thread.Sleep(100);
                Dispatcher.Invoke(m_executeCommandHandler,
                   DispatcherPriority.Normal,
                   type,
                   UIServiceCommands.s_buttonUp,
                   args);
                return null;
            }
            else
            {
                return Dispatcher.Invoke(m_executeCommandHandler,
                                          DispatcherPriority.Normal,
                                          type,
                                          name,
                                          args);
            }
        }

        public bool Reload(string argCfg)
        {
            return (bool)Dispatcher.Invoke(m_reloadHandler, DispatcherPriority.Normal, argCfg);
        }

        public bool ShowDirectScreen(string argScrFile,
                                      bool argIsAsyn)
        {
            Log.UIService.LogDebugFormat("Prepare for show screen[{0}] by asyn[{1}]", argScrFile, argIsAsyn);
            if (MainThreadID == Thread.CurrentThread.ManagedThreadId)
            {
                Log.UIService.LogDebug("The call thread is the UI thread, then must asynchronous invoke it");
                argIsAsyn = true;
            }

            if (argIsAsyn)
            {
                Dispatcher.BeginInvoke(m_showDirectScreenHandler, DispatcherPriority.Normal, argScrFile);
            }
            else
            {
                //Log.UIService.LogDebug("Prepare for get a lock to show screen in synchronous");
                lock (m_syncShowScreen)
                {
                    // Log.UIService.LogDebug("Success to get lock");
                    m_ShowScreenSyn.Reset();

                    //Log.UIService.LogDebug("Prepare for show screen in synchronous");
                    Dispatcher.Invoke(m_showDirectScreenHandler, DispatcherPriority.Normal, argScrFile);
                    //Log.UIService.LogDebug("Wait for to show screen is completed");
                    if (!m_ShowScreenSyn.WaitOne(30000))
                    {
                        Log.UIService.LogWarn("Waiting to show screen is timeout");
                        return false;
                    }
                    m_ShowScreenSyn.Reset();

                    Log.UIService.LogDebug("Success to show screen");
                }
            }

            return true;
        }

        public bool ShowDirectScreen(string argScrFile,
                                      object argDataContext,
                                      bool argIsAsyn)
        {
            Log.UIService.LogDebugFormat("Prepare for show screen[{0}] by asyn[{1}]", argScrFile, argIsAsyn);
            if (MainThreadID == Thread.CurrentThread.ManagedThreadId)
            {
                Log.UIService.LogDebug("The call thread is the UI thread, then must asynchronous invoke it");
                argIsAsyn = true;
            }

            if (argIsAsyn)
            {
                Dispatcher.BeginInvoke(m_showDirectScreen2Handler, DispatcherPriority.Normal, argScrFile, argDataContext);
            }
            else
            {
                //Log.UIService.LogDebug("Prepare for get a lock to show screen in synchronous");
                lock (m_syncShowScreen)
                {
                    // Log.UIService.LogDebug("Success to get lock");
                    m_ShowScreenSyn.Reset();

                    //Log.UIService.LogDebug("Prepare for show screen in synchronous");
                    Dispatcher.Invoke(m_showDirectScreen2Handler, DispatcherPriority.Normal, argScrFile, argDataContext);
                    //Log.UIService.LogDebug("Wait for to show screen is completed");
                    if (!m_ShowScreenSyn.WaitOne(30000))
                    {
                        Log.UIService.LogWarn("Waiting to show screen is timeout");
                        return false;
                    }
                    m_ShowScreenSyn.Reset();

                    Log.UIService.LogDebug("Success to show screen");
                }
            }

            return true;
        }

        public object ExecuteFunction(ExecuteFunctionHandler argHandler,
                                       object argParam)
        {
            return Dispatcher.Invoke(argHandler,
                                      DispatcherPriority.Normal, argParam);
        }

        public PopupDlgResult ShowDirectPopup(string argScrFile,
                                               object argDataContext,
                                               bool argIsShow,
                                               bool argIsDialog)
        {
            return (PopupDlgResult)Dispatcher.Invoke(m_showDirectPopupHandler,
                                      DispatcherPriority.Normal,
                                      argScrFile,
                                      argDataContext,
                                      argIsShow,
                                      argIsDialog);
        }

        private object OnExecuteFunction(ExecuteFunctionHandler argHandler,
                                          object argParam)
        {
            Debug.Assert(null != argHandler);

            return argHandler.Invoke(argParam);
        }

        public void OnNavigateCompleted()
        {
            //
            //Log.UIService.LogDebugFormat("Notify UI Event[{0}]", UIEventNames.s_PreShowEvent);
            NotifyUIEvent(UIEventNames.s_PreShowEvent);
            try
            {
                if (null != m_curDataContext)
                {
                    m_objActScreen.SetPropertyValue(string.Empty, UIPropertyKey.s_DataContextKey, m_curDataContext);
                }
            }
            catch (System.Exception ex)
            {
                Log.UIService.LogWarn("Failed to Set data context", ex);
            }

            if (MainWindow.Visibility == Visibility.Visible)
            {
                MainWindow.Visibility = Visibility.Hidden;
            }
            //if (XDCCanvasForm.Instance.Visible)
            //{
            //    XDCCanvasForm.Instance.Visible = false;
            //}

            Debug.Assert(null != m_objActScreen);
            //Log.UIService.LogDebug("Prepare for invoking Present method");
            m_objActScreen.Present();
            //Log.UIService.LogDebugFormat("Notify UI Event[{0}]", UIEventNames.s_ShowEvent);
            NotifyUIEvent(UIEventNames.s_ShowEvent);
            Log.UIService.LogDebug("Notify showing screen was completed");
            //OnShowScreenCompleted(m_objActScreen.Name);
            OnShowScreenCompleted(null);
            // Log.UIService.LogDebug("Success to notify screen show completed");
        }

        public void OnButtonClickInHtml(string elementID,
                                         object tag,
                                        string argMask = null )
        {
            Debug.Assert(null != m_parentUIServiceImp);

            m_parentUIServiceImp.OnUIEventRaised(new UIEventArg()
                {
                    EventName = UIEventNames.s_ClickEvent,
                    Source = m_parentUIServiceImp as IUIService,
                    ScreenName = m_objActScreen != null ? m_objActScreen.Name : null,
                    ElementName = elementID,
                    Key = tag
                }, argMask);
        }

        public void OnValueChange(string id,string property,object value)
        {
            m_parentUIServiceImp.OnValueChange(id, property, value);
        }

        public void OnFocusChangedInHtml(string argID,
                                          string argKey)
        {
            Debug.Assert(null != m_parentUIServiceImp);

            m_parentUIServiceImp.OnUIEventRaised(new UIEventArg()
                {
                    EventName = UIEventNames.s_focusChanged,
                    Source = m_parentUIServiceImp as IUIService,
                    ScreenName = m_objActScreen != null ? m_objActScreen.Name : null,
                    ElementName = argID,
                    Key = argKey
                });
        }

        #endregion

        #region override method
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            base.OnSessionEnding(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
        #endregion

        #region event method
        private bool OnShowScreen(string argName,
                                   object argDataContext)
        {
            m_curDataContext = argDataContext;
            return OnShowScreen(argName);
        }

        private bool OnShowScreen(string strName)
        {
            return true;
            Debug.Assert(!string.IsNullOrEmpty(strName) && null != MainWindow);

            if (null == m_reclaimTimer)
            {
                m_reclaimTimer = new DispatcherTimer();
                m_reclaimTimer.Interval = new System.TimeSpan(6, 0, 0);
                m_reclaimTimer.Tick += ReclaimRCCollection;
                m_reclaimTimer.Start();
            }
            if (IntPtr.Zero == m_hMainWindowHandler)
            {
                WindowInteropHelper interop = new WindowInteropHelper(MainWindow);
                m_hMainWindowHandler = interop.Handle;
                interop = null;
            }

            LogProcessorService.Log.UIService.LogDebugFormat("Prepare for showing a new screen with name {0}", strName);

            if (null != m_objActScreen &&
                strName.Equals(m_objActScreen.Name, StringComparison.OrdinalIgnoreCase))
            {
                LogProcessorService.Log.UIService.LogDebugFormat("The screen[{0}] has been showed", strName);
                try
                {
                    if (null != m_curDataContext)
                    {
                        m_objActScreen.SetPropertyValue(string.Empty, UIPropertyKey.s_DataContextKey, m_curDataContext);
                    }
                }
                catch (System.Exception ex)
                {
                    Log.UIService.LogWarn("Failed to Set data context", ex);
                }

                NotifyUIEvent(UIEventNames.s_PreShowEvent);
                NotifyUIEvent(UIEventNames.s_ShowEvent);
                m_ShowScreenSyn.Set();
                return true;
            }

            try
            {
                //find a screen
                Screen objSreen = null;
                if (m_dicScreens.ContainsKey(strName))
                {
                    //LogProcessorService.Log.UIService.LogDebugFormat("The screen with name {0} has been loaded", strName);
                    objSreen = m_dicScreens[strName];
                }
                else
                {
                    //load a screen and add it into the cache of screens.
                    // LogProcessorService.Log.UIService.LogDebug("Prepare for loading the new screen");
                    Debug.Assert(null != CfgOfUIService);
                    objSreen = CfgOfUIService.LoadScreen(strName, this);
                    if (null == objSreen)
                    {
                        m_ShowScreenSyn.Set();
                        return false;
                    }
                    //objSreen.App = this;

                    // Debug.Assert(null != objSreen);
                    try
                    {
                        m_dicScreens.Add(strName, objSreen);
                    }
                    catch (ArgumentException objExp)
                    {
                        LogProcessorService.Log.UIService.LogWarn("Failed to add the new screen", objExp);
                        LogProcessorService.Log.UIService.LogWarnFormat("The screen with name {0} is exists, so new screen will replace the existed one", strName);
                        m_dicScreens[strName] = objSreen;
                    }
                }
                Debug.Assert(null != objSreen);

                //swap the active screen
                if (objSreen is htmlScreen)
                {
                    try
                    {
                        if (null != m_curDataContext)
                        {
                            //only set data connect ,not binding again
                            objSreen.SetDatacontext(m_curDataContext);
                            //objSreen.SetPropertyValue(string.Empty, UIPropertyKey.s_DataContextKey, m_curDataContext);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Log.UIService.LogWarn("Failed to Set data context", ex);
                    }
                    objSreen.ShowScreen();
                    //objSreen.Present();
                    //if (null != m_objActScreen)
                    //{
                    //    objFrame.Content = objSreen.Content;
                    //}     
                }
                else
                {
                    Screen oldScreen = null;
                    return SwitchXamlScreen(objSreen, strName, out oldScreen);
                }
                //if (null != m_objActScreen)
                //{
                //    Debug.Assert(null != m_objActScreen.Content);
                //    m_objActScreen.Content.Visibility = Visibility.Collapsed;
                //}

                //Page objPage = objSreen.Content;
                //if (null != objPage)
                //{
                //    m_objActScreen = objSreen;
                //    objFrame.Content = objPage;
                //    objPage.Visibility = Visibility.Visible;
                //}
                //else
                //{
                //    return false;
                //}
            }
            catch (System.Exception ex)
            {
                LogProcessorService.Log.UIService.LogError("Failed to show a screen", ex);
                return true;
            }


            // LogProcessorService.Log.UIService.LogDebug("Success to show a new screen");

            return true;
        }

        private bool SwitchXamlScreen(Screen argScreen,
                                       string argName,
                                       out Screen argOldScreen)
        {
            Debug.Assert(null != argScreen);
            argOldScreen = null;
            //LogProcessorService.Log.UIService.LogDebug("Prepare for finding the site of the screen");
            //string strSite = null;
            //find the site of the screen
            if (string.IsNullOrEmpty(argScreen.Site) ||
                 null == m_objActScreen ||
                 m_objActScreen is htmlScreen)
            {
                //Log.UIService.LogDebugFormat("Show screen in site[{0}]", CfgOfUIService.SiteOfScreens);
                //strSite = CfgOfUIService.SiteOfScreens;
                DependencyObject temp = LogicalTreeHelper.FindLogicalNode(MainWindow, CfgOfUIService.SiteOfScreens);
                if (null == temp ||
                     !(temp is Panel))
                {
                    //Log.UIService.LogErrorFormat("Site[{0}] must be exists", CfgOfUIService.SiteOfScreens);
                    //if (!string.IsNullOrEmpty(argName))
                    //{
                    //    m_dicScreens.Remove(argName);
                    //}
                    OnShowScreenCompleted(argName);
                    return false;
                }

                argOldScreen = m_objActScreen;
                argScreen.ShowScreen();
                m_objActScreen = argScreen;
                if (null != m_curDataContext)
                {
                    m_objActScreen.SetPropertyValue(string.Empty, UIPropertyKey.s_DataContextKey, m_curDataContext);
                }
                NotifyUIEvent(UIEventNames.s_PreShowEvent);
                if (HtmlRender.SingleInstance.IsVisible)
                {
                    HtmlRender.SingleInstance.HideScreen();
                }
                //if (XDCCanvasForm.Instance.Visible)
                //{
                //    XDCCanvasForm.Instance.Visible = false;
                //}
                if (MainWindow.Visibility != Visibility.Visible)
                {
                    MainWindow.Visibility = Visibility.Visible;
                }
                // Log.BusinessService.LogDebug("show a screen in a panel");
                Panel site = (Panel)temp;
                if (argScreen.Content != null)
                {
                    //avoid repeat add logical child.
                    var parent = LogicalTreeHelper.GetParent(argScreen.Content) as Panel;
                    if (parent != null )
                        parent.Children.Remove(argScreen.Content);
                }

                if (site.Children.Count == 0)
                {
                    site.Children.Add(argScreen.Content);
                }
                else
                {
                    site.Children.Clear();
                    site.Children.Add(argScreen.Content);
                    //if (site.Children[0] != argScreen.Content)
                    //{
                    //    site.Children.Clear();
                    //    site.Children.Add(argScreen.Content);
                    //}
                }
                if (site.Width > 0)
                {
                    argScreen.Width = (int)site.Width;
                }
                if (site.Height > 0)
                {
                    argScreen.Height = (int)site.Height;
                }

                if (null != argOldScreen &&
                     argOldScreen != m_objActScreen )
                {
                    argOldScreen.Reset();
                    argOldScreen = null;
                    //if (argOldScreen is XamlScreen)
                    //{
                    //    if (((XamlScreen)argOldScreen).AutoDestory)
                    //    {
                    //        argOldScreen.clear();
                    //    }
                    //}
                }

                argScreen.Present();
            }
            else
            {
                //strSite = argScreen.Site;
                //Log.UIService.LogDebugFormat("Show screen in site[{0}]", argScreen.Site);
                DependencyObject temp = LogicalTreeHelper.FindLogicalNode(m_objActScreen.Content, argScreen.Site);
                if (null == temp)
                {
                    Log.UIService.LogErrorFormat("Site[{0}] must be exists", argScreen.Site);
                    //if (!string.IsNullOrEmpty(argName))
                    //{
                    //    m_dicScreens.Remove(argName);
                    //}
                    OnShowScreenCompleted(argName);
                    return false;
                }

                #region clear parent
                if (argScreen.Content != null)
                {
                    var parent = VisualTreeHelper.GetParent(argScreen.Content);
                    if (parent != null)
                    {
                        if (parent is Panel)
                        {
                            (parent as Panel).Children.Remove(argScreen.Content);
                        }
                        else if (parent is Border)
                        {
                            (parent as Border).Child = null;
                        }
                        else if (parent is ContentControl)
                        {
                            (parent as ContentControl).Content = null;
                        }
                        else
                        {
                            Log.UIService.LogDebugFormat("unexpected type: {0}" + parent.ToString());
                        }
                    }
                }
                #endregion


                if (temp is IStepPanel)
                {
                    if (null != m_curDataContext)
                    {
                        argScreen.SetPropertyValue(string.Empty, UIPropertyKey.s_DataContextKey, m_curDataContext);
                    }
                    //Log.BusinessService.LogDebugFormat("Enter: show a screen in the step panel. subsit[{0}]", argScreen.SubSite);
                    Screen oldScr = null;
                    if ( null != m_objActScreen )
                    {
                        oldScr = m_objActScreen.ActiveChild;
                    }

                    IStepPanel iStep = (IStepPanel)temp;
                    iStep.ShowPage(argScreen.Content, argScreen.SubSite);
                    if ( oldScr != argScreen &&
                         null != oldScr )
                    {
                        oldScr.Reset();
                        oldScr = null;
                    }
                    // Log.BusinessService.LogDebug("Leave: show a screen in the step panel");
                }
                else if (temp is Panel)
                {
                    // Log.BusinessService.LogDebug("show a screen in a panel");
                    Screen oldScr = null;
                    if (null != m_objActScreen)
                    {
                        oldScr = m_objActScreen.ActiveChild;
                    }
                    Panel site = (Panel)temp;
                    if (site.Children.Count == 0)
                    {
                        site.Children.Add(argScreen.Content);
                    }
                    else
                    {
                        //if (site.Children[0] != argScreen.Content)
                        //{
                        //    site.Children.Clear();
                        //    site.Children.Add(argScreen.Content);
                        //}
                        site.Children.Clear();
                        site.Children.Add(argScreen.Content);
                    }

                    if (null != m_curDataContext)
                    {
                        argScreen.SetPropertyValue(string.Empty, UIPropertyKey.s_DataContextKey, m_curDataContext);
                    }

                    if ( oldScr != null &&
                         oldScr != argScreen )
                    {
                        oldScr.Reset();
                        oldScr = null;
                    }
                }
                else
                {
                    Log.UIService.LogErrorFormat("Site[{0}] must be exists", argScreen.Site);
                    //if (!string.IsNullOrEmpty(argName))
                    //{
                    //    m_dicScreens.Remove(argName);
                    //}
                    OnShowScreenCompleted(argName);
                    return false;
                }
                m_objActScreen.AddAndActiveScreen(argScreen);
                argScreen.Present();
                NotifyUIEvent(UIEventNames.s_PreShowEvent);
                //if (HtmlRender.SingleInstance.IsVisible)
                //{
                //    HtmlRender.SingleInstance.HideScreen();
                //}
                //if (MainWindow.Visibility != Visibility.Visible)
                //{
                //    MainWindow.Visibility = Visibility.Visible;
                //}
            }

            NotifyUIEvent(UIEventNames.s_ShowEvent);

            OnShowScreenCompleted(argName);

            return true;
        }

        private void OnTriggerEvent(string strEvtName)
        {
            //Debug.Assert(null != m_objActScreen);
            if (string.IsNullOrEmpty(strEvtName))
            {
                return;
            }

            string evtName = strEvtName.Trim();
            bool bubbleEvent = true;
            if ( null != m_objActScreen )
            {
                bubbleEvent = m_objActScreen.TriggerDo(evtName, m_objActScreen.Name);
            }
            
            if ( bubbleEvent &&
                 null != m_globalScreen )
            {
                m_globalScreen.TriggerDo(evtName, null != m_objActScreen ? m_objActScreen.Name : string.Empty);
            }
        }

        public void NotifyPropertyChanged(string argElementName,
                                              string argProperty,
                                              string argValue)
        {
            //Debug.Assert(null != m_objActScreen);
            bool bubbleEvent = true;
            if (null != m_objActScreen)
            {
                bubbleEvent = m_objActScreen.OnPropertyChanged(argElementName, argProperty, argValue, m_objActScreen.Name);
            }

            if ( bubbleEvent &&
                 null != m_globalScreen )
            {
                m_globalScreen.OnPropertyChanged(argElementName, argProperty, argValue, null != m_objActScreen ? m_objActScreen.Name : string.Empty);
            }
        }

        private bool OnSetPropertyValueOfElement(string strScreen,
                                                 string strElement,
                                                 string strProperty,
                                                 object objValue)
        {
            Debug.Assert(!string.IsNullOrEmpty(strProperty));
            if (string.IsNullOrEmpty(strProperty))
            {
                return false;
            }

            if (string.Equals(strElement, UIPropertyKey.s_motherBoardElementName, StringComparison.Ordinal))
            {
                return Screen.SetPropertyValue(MainWindow, strProperty, objValue);
            }

            Screen objScreen = FindScreen(strScreen);
            if (null == objScreen)
            {
                return false;
            }

            bool bRet = objScreen.SetPropertyValue(strElement, strProperty, objValue);
            if (!bRet)
            {
                FrameworkElement objEle = Screen.FindElement(MainWindow, strElement);
                if (null == objEle)
                {
                    //Frame pageFrame = (Frame)LogicalTreeHelper.FindLogicalNode(MainWindow, CfgOfUIService.SiteOfScreens);
                    //DispatcherObject disp = LogicalTreeHelper.FindLogicalNode(MainWindow, CfgOfUIService.SiteOfScreens);
                    //if (null != disp)
                    //{
                    //    if ( disp is Frame )
                    //    {
                    //        Frame pageFrame = (Frame)disp;
                    //        if (pageFrame.Content != null &&
                    //             pageFrame.Content is FrameworkElement)
                    //        {
                    //            objEle = (FrameworkElement)Screen.FindElement((FrameworkElement)pageFrame.Content, strElement);
                    //            bRet = Screen.SetPropertyValue(objEle, strProperty, objValue);
                    //        }
                    //        else
                    //        {
                    //            return false;
                    //        }
                    //    }
                    //    else if ( disp is Panel )
                    //    {
                    //        Panel panel = (Panel)disp;
                    //        if ( panel.Children.Count > 0 &&
                    //             panel.Children[0] is FrameworkElement )
                    //        {
                    //            objEle 
                    //        }
                    //    }
                    //    else
                    //    {
                    //        return false;
                    //    }
                    //}
                    //else
                    //{
                    //    return false;
                    //}
                    return false;
                }

                bRet = Screen.SetPropertyValue(objEle, strProperty, objValue);
            }

            return bRet;
        }

        public object OnGetPropertyValueOfElement(string strScreen,
                                                 string strElement,
                                                 string strProperty)
        {
            Debug.Assert(!string.IsNullOrEmpty(strElement) && !string.IsNullOrEmpty(strProperty));
            if (string.IsNullOrEmpty(strElement) ||
                 string.IsNullOrEmpty(strProperty))
            {
                return null;
            }

            object PropValue = null;
            if (string.Equals(strElement, UIPropertyKey.s_motherBoardElementName, StringComparison.Ordinal))
            {
                Screen.GetPropertyValue(MainWindow, strProperty, out PropValue);
                return PropValue;
            }

            Screen objScreen = FindScreen(strScreen);
            if (null == objScreen)
            {
                return null;
            }

            bool bRet = objScreen.GetPropertyValue(strElement, strProperty, out PropValue);
            if (!bRet)
            {
                FrameworkElement objEle = Screen.FindElement(MainWindow, strElement);
                if (null == objEle)
                {
                    return null;
                }

                Screen.GetPropertyValue(objEle, strProperty, out PropValue);
            }

            return PropValue;
        }

        private void OnClose()
        {
            OnClose(true);
        }

        private void OnClose(bool argShutdown)
        {
            Log.UIService.LogDebug("Prepare for close UI service");

            foreach (var screen in m_dicScreens)
            {
                screen.Value.clear();
            }
            m_dicScreens.Clear();
            m_objActScreen = null;

            CfgOfUIService.Close();

            //lock (m_hotAreaLocker)
            //{
            //    HotAreas.Clear();
            //}

            if (null != m_reclaimTimer)
            {
                m_reclaimTimer.Stop();
                m_reclaimTimer.Tick -= ReclaimRCCollection;
                m_reclaimTimer = null;
            }

          //  FormElementFactory.Instance.Close();

            if ( null != m_globalScreen )
            {
                m_globalScreen.clear();
                m_globalScreen = null;
            }

            if (argShutdown)
            {
                //仅当真正退出时才清空热区信息
                //刷新事件触发时不需要清空热区，否则热区设置丢失无法使用触摸功能
                lock (m_hotAreaLocker)
                {
                    HotAreas.Clear();
                }

                //XDCCanvasForm.Instance.Close();
                //XDCCanvasForm.Instance.Dispose();

                //Close html render.
                HtmlRender.SingleInstance.Close();


                SoundPlayer.Instance.Dispose();

                Shutdown(0);
            }
        }

        private void OnRedrawUI()
        {
            //Debug.Assert(null != MainWindow);
            //MainWindow.InvalidateVisual();
            Debug.Assert(null != m_objActScreen);
            if (null == m_objActScreen)
            {
                return;
            }

            m_objActScreen.Redraw();
        }

        private bool OnShowScreenScript(Stream ScreenScript)
        {
            //Debug.Assert(null != ScreenScript && null != MainWindow);
            //if (null == ScreenScript ||
            //     null == MainWindow)
            //{
            //    return false;
            //}

            //if (null == m_reclaimTimer)
            //{
            //    m_reclaimTimer = new DispatcherTimer();
            //    m_reclaimTimer.Interval = new System.TimeSpan(6, 0, 0);
            //    m_reclaimTimer.Tick += ReclaimRCCollection;
            //    m_reclaimTimer.Start();
            //}

            //try
            //{
            //    ScreenScript.Position = 0;

            //    m_scriptDoc.Load(ScreenScript);

            //    //if ( null == m_showScriptPage1 ||
            //    //     null == m_showScriptPage2 ||
            //    //     null == m_scriptScreen1 ||
            //    //     null == m_scriptScreen2 )
            //    //{
            //    //    m_showScriptPage1 = new Page();
            //    //    m_showScriptPage1.Content = new XDCCanvas();
            //    //    m_showScriptPage2 = new Page();
            //    //    m_showScriptPage2.Content = new XDCCanvas();
            //    //    m_scriptScreen1 = new XamlScreen();
            //    //    m_scriptScreen2 = new XamlScreen();
            //    //}

            //    //Page scriptPage = NextScriptPage;
            //    //XamlScreen scriptScreen = NextScriptScreen;
            //    //XDCCanvas scriptCanvas = (XDCCanvas)scriptPage.Content;
            //    //scriptCanvas.Clear();
            //    //m_xdcCanvas.Reset();
            //    //parse
            //    //FormScreen scr = new FormScreen();
            //    //scr.Parse(m_scriptDoc);

            //    m_scriptDoc.RemoveAll();

            //    if (MainWindow.Visibility == Visibility.Visible)
            //    {
            //        MainWindow.Visibility = Visibility.Hidden;
            //    }
            //    if (HtmlRender.SingleInstance.IsVisible)
            //    {
            //        HtmlRender.SingleInstance.HideScreen(true);
            //    }
            //    //if (!XDCCanvasForm.Instance.Visible)
            //    //{
            //    //    XDCCanvasForm.Instance.Visible = true;
            //    //}

            //    Screen oldScr = m_objActScreen;
            //    m_objActScreen = scr;
            //    HtmlRender.SingleInstance.LastScrIsXDCForm = true;
            //    scr.Show();
            //    oldScr = null;

            //    //    //XamlScreen screen = new XamlScreen(scriptPage)
            //    //    //{
            //    //    //    AutoDestory = true
            //    //    //};
            //    //    scriptScreen.Content = scriptPage;
            //    //    Screen oldScreen = null;
            //    //    if (!SwitchXamlScreen(scriptScreen, null, out oldScreen))
            //    //    {
            //    //        throw new Exception("switch screen error");
            //    //    }
            //    //    //if ( null != oldScreen )
            //    //    //{
            //    //    //    oldScreen = null;
            //    //    //}
            //}
            //catch (System.Exception ex)
            //{
            //    Log.UIService.LogError("Failed to show screen script", ex);
            //    return false;
            //}

            //Log.UIService.LogDebug("Show screen script");

            /* Comment it until I found out a way to release resource.
            try
            {
                using (MemoryStream script = new MemoryStream(((MemoryStream)ScreenScript).GetBuffer(), 0, (int)ScreenScript.Length))
                {
                    Page page = (Page)XamlReader.Load(script);
                    if (null != page)
                    {
                        XamlScreen screen = new XamlScreen(page)
                        {
                            AutoDestory = true
                        };
                        Screen oldScreen = null;
                        if (!SwitchXamlScreen(screen, null, out oldScreen))
                        {
                            throw new Exception("switch screen error");
                        }
                        if (null != oldScreen)
                        {
                            oldScreen = null;
                        }
                    }
                    page = null;
                }
            }
            catch (System.Exception ex)
            {
                Log.UIService.LogError("Failed to show screen script", ex);
                return false;
            }*/

            return true;
        }

        private void ReclaimRCCollection(object argSender,
                                         EventArgs argEvt)
        {
            GC.Collect();
        }

        private PopupDlgResult OnShowPopup(string NameOfPopup,
                                           object argDataContext,
                                           bool argShow,
                                           bool IsDialog)
        {
            Log.UIService.LogDebugFormat("Prepare for showing a popup[{0}]", NameOfPopup);
            Debug.Assert(null != MainWindow);
            if (null == MainWindow ||
                 string.IsNullOrEmpty(NameOfPopup))
            {
                if (!IsDialog &&
                     !argShow)
                {
                    foreach (var item in m_dicPopupWindow)
                    {
                        item.Value.DataContext = null;
                        item.Value.Close();
                    }
                    m_dicPopupWindow.Clear();
                    return PopupDlgResult.Ok;
                }

                Log.UIService.LogError("The MainWindow or nameOfPopup is empty");
                return PopupDlgResult.None;
            }

            PopupDlgResult result = PopupDlgResult.None;
            string uriOfScreen = CfgOfUIService.FindPopupWindowUri(NameOfPopup);
            if (string.IsNullOrEmpty(uriOfScreen))
            {
                Log.UIService.LogError("The NameOFPopup isn't exists");
                return PopupDlgResult.Fail;
            }
            bool isHtml = CfgOfUIService.IsHtml(uriOfScreen);
            bool isXaml = CfgOfUIService.IsXaml(uriOfScreen);
            if (isXaml)
            {
                result = OnShowPopupWPF(NameOfPopup, argDataContext, argShow, IsDialog);
            }
            else if (isHtml)
            {
                bool bResult = ShowPopupHTML(uriOfScreen, argDataContext);
                if (!bResult)
                {
                    Log.UIService.LogError("ShowPopupHTML is fail");
                    result = PopupDlgResult.Fail;
                }
            }

            return result;
        }

        /// <summary>
        /// 关闭弹出窗口
        /// </summary>
        private void OnHidePopup()
        {
            object result = ExecuteCommand(UICommandType.Script, "close_shade");
            Log.UIService.LogDebug(string.Format("ExecuteCommand close_shade return value is {0}", result));
        }

        private bool ShowPopupHTML(string url, object timeout)
        {
            Log.UIService.LogDebug(string.Format("ExecuteCommand shade args url:{0},timeout:{1}", url, timeout));
            object result = ExecuteCommand(UICommandType.Script, "shade", url, timeout);
            Log.UIService.LogDebug(string.Format("ExecuteCommand shade return value is {0}", result));
            if (result == null)
            {
                return false;
            }
            try
            {
                return (bool)result;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void OnPopupEventHtml(string argKey)
        {
            Debug.Assert(null != m_parentUIServiceImp);
            m_parentUIServiceImp.OnUIEventRaised(new UIEventArg()
            {
                EventName = UIEventNames.s_popupEvent,
                Source = m_parentUIServiceImp as IUIService,
                ScreenName = string.Empty,
                ElementName = string.Empty,
                Key = argKey,
                IsImpersonated = true
            });
        }

        private PopupDlgResult OnShowPopupWPF(string NameOfPopup,
                                           object argDataContext,
                                           bool argShow,
                                           bool IsDialog)
        {
            Log.UIService.LogDebugFormat("Prepare for showing a popup[{0}]", NameOfPopup);

            Debug.Assert(null != MainWindow);
            if (null == MainWindow ||
                 string.IsNullOrEmpty(NameOfPopup))
            {
                if (!IsDialog &&
                     !argShow)
                {
                    foreach (var item in m_dicPopupWindow)
                    {
                        item.Value.DataContext = null;
                        item.Value.Close();
                    }
                    m_dicPopupWindow.Clear();
                    return PopupDlgResult.Ok;
                }

                Log.UIService.LogError("The MainWindow or nameOfPopup is empty");
                return PopupDlgResult.None;
            }

            PopupDlgResult result = PopupDlgResult.None;
            string uriOfScreen = CfgOfUIService.FindPopupWindowUri(NameOfPopup);
            if (string.IsNullOrEmpty(uriOfScreen))
            {
                Log.UIService.LogError("The NameOFPopup isn't exists");
                return PopupDlgResult.Fail;
            }

            if (!IsDialog &&
                 m_dicPopupWindow.ContainsKey(uriOfScreen))
            {
                Log.UIService.LogDebug("Find a popup window");
                Window wndPop = m_dicPopupWindow[uriOfScreen];
                if (argShow)
                {
                    wndPop.DataContext = argDataContext;
                    if (IsDialog)
                    {
                        //RoutedEventHandler handler = new RoutedEventHandler((argSender, argEvts) =>
                        //{
                        //    argEvts.Handled = true;
                        //    wndPop.Tag = ((FrameworkElement)argEvts.OriginalSource).Tag;
                        //    wndPop.DialogResult = true;
                        //});
                        //wndPop.AddHandler(Button.ClickEvent, handler);
                        //bool? wndResult = wndPop.ShowDialog();
                        //if (!wndResult.HasValue ||
                        //    !wndResult.Value)
                        //{
                        //    result = PopupDlgResult.Cancel;
                        //}
                        //else
                        //{
                        //    if (null != wndPop.Tag)
                        //    {
                        //        string tagValue = (string)wndPop.Tag;
                        //        result = CheckPopupDlgResult(tagValue);
                        //    }
                        //    else
                        //    {
                        //        result = PopupDlgResult.Cancel;
                        //    }
                        //}

                        //wndPop.RemoveHandler(Button.ClickEvent, handler);
                        //wndPop.DataContext = null;
                        //handler = null;
                    }
                    else
                    {
                        if (wndPop.Visibility != Visibility.Visible)
                        {
                            //wndPop.Visibility = Visibility.Visible;
                            wndPop.Show();
                        }
                    }
                }
                else
                {
                    if (!IsDialog)
                    {
                        //if (wndPop.Visibility == Visibility.Visible)
                        //{
                        //    //wndPop.Visibility = Visibility.Hidden;
                        //    wndPop.Hide();
                        //}

                        if (null != wndPop.DataContext)
                        {
                            wndPop.DataContext = null;
                        }

                        wndPop.RemoveHandler(Button.ClickEvent, new RoutedEventHandler(OnButtonClickedInPopup));
                        wndPop.RemoveHandler(CountdownControl.CountdownCompletedEvent, new RoutedEventHandler(OnTimeoutInPopup));
                        //wndPop.RemoveHandler(Button.TouchDownEvent, new RoutedEventHandler(OnButtonTouchDownedInPopup));
                        wndPop.Close();
                        wndPop = null;
                        m_dicPopupWindow.Remove(uriOfScreen);
                    }
                    //wndPop.Close();
                    //wndPop = null;
                    //m_dicPopupWindow.Remove(NameOfPopup);
                }
            }
            else
            {
                if (!argShow)
                {
                    return PopupDlgResult.None;
                }

                //string uriOfScreen = CfgOfUIService.FindScreenUri(NameOfPopup);
                if (CfgOfUIService.IsLocal)
                {
                    if (!File.Exists(uriOfScreen))
                    {
                        Log.UIService.LogErrorFormat("The uri[{0}] of a popup isn't exists", uriOfScreen);
                        return PopupDlgResult.Fail;
                    }

                    Log.UIService.LogDebug("Prepare for loading content of the popup window");
                    Window wndPop = null;
                    RoutedEventHandler handler = null;
                    RoutedEventHandler TimeoutHandler = null;
                    //RoutedEventHandler touchDownHandler = null;
                    try
                    {
                        using (FileStream fs = new FileStream(uriOfScreen, FileMode.Open, FileAccess.Read))
                        {
                            wndPop = (Window)XamlReader.Load(fs);
                        }
                        wndPop.Owner = MainWindow;
                        if (null != argDataContext)
                        {
                            wndPop.DataContext = argDataContext;
                        }

                        List<WPFBindingExpress> bindings = null;
                        //XamlScreen.ExtractAllBindings(wndPop, ref bindings);
                        if (null != bindings)
                        {
                            foreach (var item in bindings)
                            {
                                item.DataContent = argDataContext;
                            }
                        }

                        if (IsDialog)
                        {
                            Log.UIService.LogDebug("Prepare for show a dialog");
                            //m_dialog = wndPop;

                            handler = new RoutedEventHandler((argSender, argEvts) =>
                                {
                                    argEvts.Handled = true;
                                    wndPop.Tag = ((FrameworkElement)argEvts.OriginalSource).Tag;
                                    Log.UIService.LogDebugFormat("A button[{0}] in the dialog was clicked", wndPop.Tag);
                                    if (null != wndPop.Tag &&
                                         wndPop.Tag is string &&
                                         UIPropertyKey.s_confirmTag.Equals((string)wndPop.Tag, StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (null != bindings)
                                        {
                                            foreach (var item in bindings)
                                            {
                                                item.UpdateData(true);
                                            }
                                        }
                                    }
                                    wndPop.DialogResult = true;
                                });
                            TimeoutHandler = new RoutedEventHandler((argSender, argEvts) =>
                            {
                                argEvts.Handled = true;
                                wndPop.Tag = "OnTimeout";
                                Log.UIService.LogDebugFormat("The dialog is timeout");
                                wndPop.DialogResult = true;
                            });
                            //touchDownHandler = new RoutedEventHandler((argSender, argEvts) =>
                            //{
                            //    argEvts.Handled = true;
                            //    wndPop.Tag = ((FrameworkElement)argEvts.Source).Tag;
                            //    Log.UIService.LogDebugFormat("A button[{0}] in the dialog was touchDown", wndPop.Tag);
                            //    if (null != wndPop.Tag &&
                            //         wndPop.Tag is string &&
                            //         UIPropertyKey.s_confirmTag.Equals((string)wndPop.Tag, StringComparison.OrdinalIgnoreCase))
                            //    {
                            //        if (null != bindings)
                            //        {
                            //            foreach (var item in bindings)
                            //            {
                            //                item.UpdateData(true);
                            //            }
                            //        }
                            //    }
                            //    wndPop.DialogResult = true;
                            //});
                            wndPop.AddHandler(Button.ClickEvent, handler);
                            wndPop.AddHandler(CountdownControl.CountdownCompletedEvent, TimeoutHandler);
                            //wndPop.AddHandler(Button.TouchDownEvent, touchDownHandler);

                            bool? wndResult = wndPop.ShowDialog();
                            wndPop.RemoveHandler(CountdownControl.CountdownCompletedEvent, TimeoutHandler);
                            wndPop.RemoveHandler(Button.ClickEvent, handler);
                            //wndPop.RemoveHandler(Button.TouchDownEvent, touchDownHandler);
                            handler = null;
                            TimeoutHandler = null;

                            if (!wndResult.HasValue ||
                                 !wndResult.Value)
                            {
                                Log.UIService.LogError("The result of showing a dialog is illegal");
                                result = PopupDlgResult.None;
                            }
                            else
                            {
                                if (null != wndPop.Tag)
                                {
                                    Log.UIService.LogDebugFormat("The result of showing a dialog is [{0}]", wndPop.Tag);
                                    string tagValue = (string)wndPop.Tag;
                                    result = CheckPopupDlgResult(tagValue);
                                }
                                else
                                {
                                    Log.UIService.LogWarn("The result of showing a dialog is null");
                                    result = PopupDlgResult.None;
                                }
                                //m_dialog = null;
                            }
                            wndPop.DataContext = null;
                            handler = null;
                            TimeoutHandler = null;
                            wndPop = null;
                            //m_dicPopupWindow.Add(uriOfScreen, wndPop);
                        }
                        else
                        {
                            Log.UIService.LogDebug("Prepare for showing a modaless dialog");
                            wndPop.AddHandler(Button.ClickEvent, new RoutedEventHandler(OnButtonClickedInPopup));
                            wndPop.AddHandler(CountdownControl.CountdownCompletedEvent, new RoutedEventHandler(OnTimeoutInPopup));
                            //wndPop.AddHandler(Button.TouchDownEvent, new RoutedEventHandler(OnButtonTouchDownedInPopup));
                            wndPop.Show();
                            m_dicPopupWindow.Add(uriOfScreen, wndPop);
                        }

                        if (null != bindings)
                        {
                            bindings.Clear();
                            bindings = null;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Log.UIService.LogError("Failed to show popup", ex);
                        if (null != wndPop)
                        {
                            wndPop.RemoveHandler(CountdownControl.CountdownCompletedEvent, TimeoutHandler);
                            wndPop.RemoveHandler(Button.ClickEvent, handler);
                            //wndPop.RemoveHandler(Button.TouchDownEvent,touchDownHandler);
                            wndPop.Close();
                            wndPop = null;
                        }
                        return PopupDlgResult.None;
                    }
                }
            }

            return result;
        }

        private void OnButtonClickedInPopup(object argSender,
                                             RoutedEventArgs argEvt)
        {
            argEvt.Handled = true;

            Debug.Assert(null != m_parentUIServiceImp);
            m_parentUIServiceImp.OnUIEventRaised(new UIEventArg()
                {
                    EventName = UIEventNames.s_ClickEvent,
                    Source = this as IUIService,
                    ScreenName = string.Empty,
                    ElementName = ((FrameworkElement)argEvt.OriginalSource
                    ).Name,
                    Key = ((FrameworkElement)argEvt.OriginalSource).Tag
                });
        }

        //private void OnButtonTouchDownedInPopup(object argSender,
        //                                     RoutedEventArgs argEvt)
        //{
        //    argEvt.Handled = true;

        //    Debug.Assert(null != m_parentUIServiceImp);
        //    m_parentUIServiceImp.OnUIEventRaised(new UIEventArg()
        //    {
        //        EventName = UIEventNames.s_ClickEvent,
        //        Source = this as IUIService,
        //        ScreenName = string.Empty,
        //        ElementName = ((FrameworkElement)argEvt.Source
        //        ).Name,
        //        Key = ((FrameworkElement)argEvt.Source).Tag
        //    });
        //}

        private void OnTimeoutInPopup(object argSender,
                                       RoutedEventArgs argEvt)
        {
            argEvt.Handled = true;
            Debug.Assert(null != m_parentUIServiceImp);
            m_parentUIServiceImp.OnUIEventRaised(new UIEventArg()
            {
                EventName = UIEventNames.s_timeoutEvent,
                Source = this as IUIService,
                ScreenName = string.Empty,
                ElementName = string.Empty,
                Key = null
            });
        }

        private PopupDlgResult CheckPopupDlgResult(string argResult)
        {
            PopupDlgResult result = PopupDlgResult.None;
            if (string.IsNullOrEmpty(argResult))
            {
                result = PopupDlgResult.Fail;
            }
            else
            {
                if (UIPropertyKey.s_confirmTag.Equals(argResult, StringComparison.OrdinalIgnoreCase))
                {
                    result = PopupDlgResult.Ok;
                }
                else if (UIPropertyKey.s_cancelTag.Equals(argResult, StringComparison.OrdinalIgnoreCase))
                {
                    result = PopupDlgResult.Cancel;
                }
                else if (string.Equals(argResult, "OnTimeout", StringComparison.OrdinalIgnoreCase))
                {
                    result = PopupDlgResult.Timeout;
                }
                else if (UIPropertyKey.s_ignoreTag.Equals(argResult, StringComparison.OrdinalIgnoreCase))
                {
                    result = PopupDlgResult.Ignore;
                }
                else if (UIPropertyKey.s_retryTag.Equals(argResult, StringComparison.OrdinalIgnoreCase))
                {
                    result = PopupDlgResult.Retry;
                }
                else
                {
                    result = PopupDlgResult.None;
                }
            }

            return result;
        }

        private PopupDlgResult OnShowPopupScript(Stream PopupScript,
                                                 object argDataContext,
                                        bool argShow,
                                        bool IsDialog)
        {
            Debug.Assert(null != MainWindow && null != PopupScript);
            if (null == PopupScript ||
                 null == MainWindow)
            {
                return PopupDlgResult.None;
            }

            return PopupDlgResult.None;
        }

        private void OnEnumElements(EnumElementHandler Handler,
                                     ElementType Type,
                                     EnumFlag Flag,
                                     string NameOfElement,
                                     object Param)
        {
            if (null == m_objActScreen)
            {
                return;
            }

            m_objActScreen.EnumElements(Handler, Type, Flag, NameOfElement, Param);
        }

        private bool OnShowDirectScreen(string argScrFile)
        {
            Debug.Assert(!string.IsNullOrEmpty(argScrFile));
            Log.UIService.LogDebugFormat("Prepare for show the file[{0}]", argScrFile);

            bool isHtml = CfgOfUIService.IsHtml(argScrFile);
            bool isXaml = CfgOfUIService.IsXaml(argScrFile);
            if (!isHtml &&
                 !isXaml)
            {
                Log.UIService.LogError("The file isn't html or xaml");
                return false;
            }

            Screen scr = null;
            if (isHtml)
            {
                if (null != m_directHtmlScreen)
                {
                    m_directHtmlScreen.clear();
                }
                else
                {
                    m_directHtmlScreen = new htmlScreen();
                    m_directHtmlScreen.HandleHotArea = false;// true;
                    m_directHtmlScreen.App = this;
                }


                scr = m_directHtmlScreen;
                scr.FullPath = argScrFile;

                scr.ShowScreen();
                scr.Present();

                if (null != m_objActScreen &&
                     m_directHtmlScreen != m_objActScreen)
                {
                    m_objActScreen.Reset();
                    m_objActScreen = scr;
                }
            }
            else if (isXaml)
            {
                //if (null != m_directXamlScreen)
                //{
                //    m_directXamlScreen.clear();
                //}
                //else
                //{
                //    m_directXamlScreen = new XamlScreen();
                //    m_directXamlScreen.App = this;
                //}
                //scr = m_directXamlScreen;
                //scr.FullPath = argScrFile;

                //Screen oldScreen = null;
                //return SwitchXamlScreen(scr, null, out oldScreen);
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool OnShowDirectScreen(string argScrFile,
                                         object argDataContext)
        {
            m_curDataContext = argDataContext;
            return OnShowDirectScreen(argScrFile);
        }

        private PopupDlgResult OnShowDirectPopup(string argScrFile,
                                                  object argDataContext,
                                                  bool argShow,
                                                  bool argIsDialog)
        {
            Debug.Assert(!string.IsNullOrEmpty(argScrFile));
            Log.UIService.LogDebugFormat("Prepare for show the file[{0}] as a popup", argScrFile);

            bool isHtml = CfgOfUIService.IsHtml(argScrFile);
            bool isXaml = CfgOfUIService.IsXaml(argScrFile);
            if (!isHtml &&
                 !isXaml)
            {
                Log.UIService.LogError("The file isn't html or xaml");
                return PopupDlgResult.Fail;
            }



            return PopupDlgResult.None;
        }

        private bool OnReload(string argReload)
        {
            Debug.Assert(null != CfgOfUIService);
            try
            {
                string currentName = null;
                if (null != m_objActScreen)
                {
                    currentName = m_objActScreen.Name;
                }

                string cfgFile = null;
                if (string.IsNullOrEmpty(argReload))
                {
                    cfgFile = CfgOfUIService.CfgFilePath;
                    Log.UIService.LogDebugFormat("Reload the current configuration[{0}]", cfgFile);
                }
                else
                {
                    cfgFile = argReload;
                    Log.UIService.LogDebugFormat("Reload the current configuartion[{0}]", cfgFile);
                }
                Log.UIService.LogDebug("Close all loaded resources");
                OnClose(false);

                //
                if (!CfgOfUIService.Load(cfgFile))
                {
                    throw new Exception("Failed to load the configuration of a UI service");
                }

                WindowConfig wndCfg = CfgOfUIService.WndConfig;
                //if (!wndCfg.ShowCursor)
                //{
                //    MainWindow.Cursor = Cursors.None;
                //}
                Interop.NativeWndApi.ShowCursor(wndCfg.ShowCursor ? 1 : 0);
                HtmlRender.SingleInstance.Topmost = wndCfg.Topmost;
                MainWindow.Topmost = wndCfg.Topmost;
                if (wndCfg.FullScreen)
                {
                    MainWindow.Left = 0;
                    MainWindow.Top = 0;
                    MainWindow.Height = SystemParameters.PrimaryScreenHeight;
                    MainWindow.Width = SystemParameters.PrimaryScreenWidth;

                    HtmlRender.SingleInstance.Left = 0;
                    HtmlRender.SingleInstance.Top = 0;
                    HtmlRender.SingleInstance.Height = (int)SystemParameters.PrimaryScreenHeight;
                    HtmlRender.SingleInstance.Width = (int)SystemParameters.PrimaryScreenWidth;

                    //XDCCanvasForm.Instance.Left = 0;
                    //XDCCanvasForm.Instance.Top = 0;
                    //XDCCanvasForm.Instance.Size = new System.Drawing.Size((int)SystemParameters.PrimaryScreenHeight,
                    //                                                        (int)SystemParameters.PrimaryScreenWidth);
                }
                else
                {
                    MainWindow.Left = wndCfg.Left;
                    MainWindow.Top = wndCfg.Top;
                    MainWindow.Width = wndCfg.Width;
                    MainWindow.Height = wndCfg.Height;

                    HtmlRender.SingleInstance.Left = wndCfg.Left;
                    HtmlRender.SingleInstance.Top = wndCfg.Top;
                    HtmlRender.SingleInstance.Width = wndCfg.Width;
                    HtmlRender.SingleInstance.Height = wndCfg.Height;

                    //XDCCanvasForm.Instance.Left = wndCfg.Left;
                    //XDCCanvasForm.Instance.Top = wndCfg.Top;
                    //XDCCanvasForm.Instance.Size = new System.Drawing.Size(wndCfg.Width,
                    //                                                       wndCfg.Height);
                }
                MainWindowWidth = MainWindow.Width;
                MainWindowHeight = MainWindowHeight;

                InitializeGlobalSettings();

                if (!string.IsNullOrEmpty(currentName))
                {
                    OnShowScreen(currentName, m_curDataContext);
                }
            }
            catch (System.Exception ex)
            {
                Log.UIService.LogError("Failed to reload UI", ex);
                return false;
            }

            return true;
        }

        public void NotifyHtmlIsOk(Screen argScreen)
        {
            Debug.Assert(null != argScreen);

            if (null != m_objActScreen)
            {
                m_objActScreen.Reset();
            }
            m_objActScreen = argScreen;
        }

        //private void OnChangeLanguage( string argLanguage )
        //{
        //    m_curLanguage = argLanguage;
        //    if (null != m_objActScreen)
        //    {
        //        m_objActScreen.ChangeLanguage(m_curLanguage);
        //    }
        //}

        private object OnExcuteCommand(UICommandType type,
                                        string name,
                                        params object[] args)
        {
            if (UICommandType.Custom == type)
            {
                switch (name)
                {
                    case UIServiceCommands.s_addHotArea:
                        {
                            Log.UIService.LogDebug("Prepare for executing AddHotArea command");
                            if (args.Length == 0 ||
                                 null == args[0])
                            {
                                Log.UIService.LogWarn("The parameters of addHotArea is illegal");
                            }

                            if (args[0] is HotAreaParamArray)
                            {
                                return HandleAddHotArea((HotAreaParamArray)args[0]);
                            }
                            else
                            {
                                return false;
                            }
                        }

                    case UIServiceCommands.s_removeHotArea:
                        {
                            Log.UIService.LogDebug("Prepare for executing removeHotArea command");
                            if (args.Length == 0 ||
                                 null == args[0])
                            {
                                Log.UIService.LogWarn("The parameters of removeHotArea is illegal");
                            }

                            if (args[0] is HotAreaParamArray)
                            {
                                return HandleRemoveHotArea((HotAreaParamArray)args[0]);
                            }
                            else
                            {
                                return false;
                            }
                        }

                    case UIServiceCommands.s_clearHotArea:
                        {
                            Log.UIService.LogDebug("Prepare for executing to clear HotArea command");
                            return HandleClearHotArea();
                        }

                    case UIServiceCommands.s_clearDataBind:
                        {
                            m_curDataContext = null;
                        }
                        break;

                    case UIServiceCommands.s_reloadAllStoryboards:
                        {
                            foreach (var screen in m_dicScreens)
                            {
                                screen.Value.clear();
                            }
                            m_dicScreens.Clear();
                            m_objActScreen = null;

                            string argFilePath = CfgOfUIService.CfgFilePath;
                            CfgOfUIService.Close();
                            if (!CfgOfUIService.Load(argFilePath))
                            {
                                Log.UIService.LogDebug("Failed to reload config of UIService");
                                return false;
                            }

                            return true;
                        }
                }
            }
            else
            {

            }

            if (null == m_objActScreen)
            {
                return true;
            }

            return m_objActScreen.ExecuteCommand(type, name, args);
        }

        public void OnShowScreenCompleted(string name)
        {
            Debug.Assert(null != m_ShowScreenSyn);
            m_ShowScreenSyn.Set();
        }

        private bool HandleAddHotArea(HotAreaParamArray argarrHotAreas)
        {
            Debug.Assert(null != argarrHotAreas);
            if (null == argarrHotAreas)
            {
                return false;
            }

            lock (m_hotAreaLocker)
            {
                if (argarrHotAreas.HotAreas.Length == 0)
                {
                    return false;
                }

                bool exist = false;
                int index = 0;
                foreach (var hotArea in argarrHotAreas.HotAreas)
                {
                    index = 0;
                    exist = HotAreas.Exists((item) =>
                        {
                            if (item.IsSameLocation(hotArea))
                            {
                                return true;
                            }
                            ++index;
                            return false;
                        });
                    if (exist)
                    {
                        HotAreas.RemoveAt(index);
                    }

                    HotAreas.Add(hotArea);
                }
            }

            return true;
        }

        private bool HandleRemoveHotArea(HotAreaParamArray argarrHotAreas)
        {
            Debug.Assert(null != argarrHotAreas);
            if (null == argarrHotAreas)
            {
                return false;
            }

            lock (m_hotAreaLocker)
            {
                if (argarrHotAreas.HotAreas.Length == 0)
                {
                    return false;
                }

                bool exist = false;
                int index = 0;
                foreach (var hotArea in argarrHotAreas.HotAreas)
                {
                    index = 0;
                    exist = HotAreas.Exists((item) =>
                        {
                            if (item.IsSameLocation(hotArea))
                            {
                                return true;
                            }
                            ++index;
                            return false;
                        });
                    if (exist)
                    {
                        HotAreas.RemoveAt(index);
                    }
                }
            }
            return true;
        }

        private bool HandleClearHotArea()
        {
            lock (m_hotAreaLocker)
            {
                HotAreas.Clear();
            }

            return true;
        }

        private Screen FindScreen(string strScreen)
        {
            Screen objScreen = null;
            if (null == strScreen)
            {
                objScreen = m_objActScreen;
            }
            else
            {
                if (m_dicScreens.ContainsKey(strScreen))
                {
                    objScreen = m_dicScreens[strScreen];
                }
                else
                {
                    Debug.Assert(null != CfgOfUIService);
                    objScreen = CfgOfUIService.LoadScreen(strScreen);
                }
            }

            return objScreen;
        }

        private void NotifyUIEvent(string UIEvent)
        {
            Debug.Assert(!string.IsNullOrEmpty(UIEvent) && null != m_parentUIServiceImp);

            bool result = m_parentUIServiceImp.OnUIEventRaised(new UIEventArg()
                {
                    EventName = UIEvent,
                    Source = m_parentUIServiceImp as IUIService
                });

            if (result)
            {
                OnTriggerEvent(UIEvent);
            }
        }

        private void OnStopInElement(FrameworkElement argElement)
        {
            Debug.Assert(null != argElement);

            var elementlist = LogicalTreeHelper.GetChildren(argElement);
            foreach (var element in elementlist)
            {
                if (element is MediaElement)
                {
                    ((MediaElement)element).Close();
                }
            }
        }

        public void OnMouseDown(object argSender,
                                 MouseButtonEventArgs arg)
        {
            //arg.Handled = true;
        }

        public void OnMouseUp(object argSender,
                               MouseButtonEventArgs arg)
        {
            //arg.Handled = true;

            //Point pt = arg.GetPosition(MainWindow);
            //lock (m_hotAreaLocker)
            //{
            //    foreach (var hotArea in HotAreas)
            //    {
            //        if (hotArea.HitTest((int)pt.X, (int)pt.Y))
            //        {
            //            HandleHotArea(hotArea);
            //            return;
            //        }
            //    }
            //}
        }

        public void HandleHotArea(HotAreaParam argArea)
        {
            Debug.Assert(null != argArea);

            switch (argArea.Type)
            {
                case ImpersonatedType.Button:
                    {
                        Parent.OnUIEventRaised(new UIEventArg()
                            {
                                EventName = UIEventNames.s_ClickEvent,
                                Key = argArea.Key,
                                Source = Parent as IUIService,
                                ScreenName = ActiveScreen == null ? "" :  ActiveScreen.Name,
                                ElementName = argArea.Name,
                                IsImpersonated = true
                            });
                    }
                    break;

                default:
                    {
                        Log.UIService.LogErrorFormat("Unsupported impersonated type [{0}]", argArea.Type);
                        Debug.Assert(false);
                    }
                    break;
            }
        }

        private void OnLanguageChanged(string argOld,
                                        string argNew,
                                        bool argIsUI)
        {
            if (argIsUI)
            {
                //   RedrawUI();
            }

            foreach (var item in m_dicScreens)
            {
                item.Value.ChangeLanguage(argNew);
            }

            Application.Current.Properties["CurrentLanguage"] = CurrentLanguage;
            try
            {
                Application.Current.Resources.MergedDictionaries.Clear();
                if (File.Exists(string.Concat(sourcePath, CurrentLanguage, @"\TextResource.xaml")))
                {
                    Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(string.Concat(sourcePath, CurrentLanguage, @"\TextResource.xaml"), UriKind.RelativeOrAbsolute) });
                }
                if (File.Exists(string.Concat(sourcePath, CurrentLanguage, @"\Style.xaml")))
                {
                    Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(string.Concat(sourcePath, CurrentLanguage, @"\Style.xaml"), UriKind.RelativeOrAbsolute) });
                }
                if (File.Exists(string.Concat(sourcePath, CurrentLanguage, @"\ImageResource.xaml")))
                {
                    Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(string.Concat(sourcePath, CurrentLanguage, @"\ImageResource.xaml"), UriKind.RelativeOrAbsolute) });
                }
            }
            catch (Exception ex)
            {

                Log.UIService.LogError("load wpf resource error", ex);
            }
        }

        public void InitializeGlobalSettings()
        {
            m_globalScreen = new htmlScreen();

            CfgOfUIService.LoadGlobalDataTemplates(m_globalScreen);
            CfgOfUIService.LoadGlobalTriggers(m_globalScreen);
        }

        #endregion

        #region property
        public UIServiceCfg CfgOfUIService
        {
            get;
            set;
        }

        public Screen ActiveScreen
        {
            get
            {
                return m_objActScreen;
            }
        }

        public UIServiceImp Parent
        {
            get
            {
                return m_parentUIServiceImp;
            }
            set
            {
                m_parentUIServiceImp = value;
            }
        }

        public int MainThreadID
        {
            get
            {
                return m_mainThreadID;
            }
            set
            {
                m_mainThreadID = value;
            }
        }

        public double MainWindowWidth
        {
            get
            {
                return m_wndWidth;
            }
            set
            {
                m_wndWidth = value;
            }
        }

        public double MainWindowHeight
        {
            get
            {
                return m_wndHeight;
            }
            set
            {
                m_wndHeight = value;
            }
        }

        public List<HotAreaParam> HotAreas
        {
            get
            {
                if (null == m_listHotAreas)
                {
                    m_listHotAreas = new List<HotAreaParam>();
                }

                return m_listHotAreas;
            }
        }

        public string CurrentLanguage
        {
            get
            {
                //              return m_curLanguage;
                Debug.Assert(null != m_iResourceManager);
                return m_iResourceManager.CurrentUILanguage;
            }
            //set
            //{
            //    if ( string.IsNullOrEmpty ( value ))
            //    {
            //        return;
            //    }

            //    if (null != m_curLanguage &&
            //        m_curLanguage.Equals(value, StringComparison.OrdinalIgnoreCase))
            //    {
            //        return;
            //    }

            //    Dispatcher.Invoke(m_changeLanguageHandler, DispatcherPriority.Normal, value);
            //}
        }

        public string CurrentScreenName
        {
            get
            {
                if (null != m_objActScreen)
                {
                    return m_objActScreen.Name;
                }

                return null;
            }
        }

        //private Page NextScriptPage
        //{
        //    get
        //    {
        //        if ( null == m_nextPage )
        //        {
        //            m_nextPage = m_showScriptPage1;
        //        }
        //        else
        //        {
        //            if ( m_nextPage == m_showScriptPage1 )
        //            {
        //                m_nextPage = m_showScriptPage2;
        //            }
        //            else
        //            {
        //                m_nextPage = m_showScriptPage1;
        //            }
        //        }

        //        return m_nextPage;
        //    }
        //}

        //private XamlScreen NextScriptScreen
        //{
        //    get
        //    {
        //        if ( null == m_nexScreen )
        //        {
        //            m_nexScreen = m_scriptScreen1;
        //        }
        //        else
        //        {
        //            if ( m_nexScreen == m_scriptScreen1 )
        //            {
        //                m_nexScreen = m_scriptScreen2;
        //            }
        //            else
        //            {
        //                m_nexScreen = m_scriptScreen1;
        //            }
        //        }

        //        return m_nexScreen;
        //    }
        //}
        public IntPtr HandleOfMainWindow
        {
            get
            {
                if (null != m_objActScreen)
                {
                    if (m_objActScreen is htmlScreen)
                    {
                        return HtmlRender.SingleInstance.Browser.Handle;
                    }
                    //else if (m_objActScreen is FormScreen)
                    //{
                    //    return XDCCanvasForm.Instance.Handle;
                    //}
                    else
                    {
                        return m_hMainWindowHandler;
                    }
                }
                else
                {
                    return m_hMainWindowHandler;
                }
            }
        }

        public IResourceManager ResourceManager
        {
            get
            {
                return m_iResourceManager;
            }
            set
            {
                Debug.Assert(null != value);
                if (null != m_iResourceManager)
                {
                    m_iResourceManager.languageChanged -= OnLanguageChanged;
                }

                if (null != CfgOfUIService)
                {
                    CfgOfUIService.ResouceManager = value;
                }

                m_iResourceManager = value;
                m_iResourceManager.languageChanged += OnLanguageChanged;
            }
        }

        public htmlScreen GlobalHtmlScreen
        {
            get
            {
                return m_globalScreen;
            }
        }

        #endregion

        #region field

        private UIServiceImp m_parentUIServiceImp = null;

        private int m_mainThreadID = 0;

        private Dictionary<string, Screen> m_dicScreens = new Dictionary<string, Screen>();

        private Screen m_objActScreen = null;

        private IntPtr m_hMainWindowHandler = IntPtr.Zero;

        private Func<string, bool> m_ShowScreenHandler = null;

        private Func<string, object, bool> m_ShowScreen2Handler = null;

        private Func<string, bool> m_showDirectScreenHandler = null;

        private Func<string, object, bool> m_showDirectScreen2Handler = null;

        private Func<string, object, bool, bool, PopupDlgResult> m_ShowPopupHandler = null;
        private Action m_HidePopupHandler = null;           // 关闭窗口处理

        private Func<string, object, bool, bool, PopupDlgResult> m_showDirectPopupHandler = null;

        //       private Action<string> m_changeLanguageHandler = null;

        private Func<Stream, bool> m_ShowScreenScriptHandler = null;

        private Func<Stream, object, bool, bool, PopupDlgResult> m_ShowPopupScriptHandler = null;

        private Func<string, bool> m_reloadHandler = null;

        private Action<bool> m_visibleHandler = null;

        private Action<EnumElementHandler, ElementType, EnumFlag, string, object> m_enumElementHandler = null;

        //private Func<ExecuteFunctionHandler, object, object> m_executeFunctionHandler = null;

        private Action<String> m_TriggerHandler = null;

        private CloseHandler m_CloseHandler = null;

        private RedrawUIHandler m_RedrawUIHandler = null;

        private ExecuteCommandHandler m_executeCommandHandler = null;

        private Func<string, string, string, object, bool> m_SetValueHandler = null;

        private GetPropertyValueHandler m_GetValueHandler = null;

        private ManualResetEvent m_ShowScreenSyn = new ManualResetEvent(false);

        private object m_hotAreaLocker = new object();

        private List<HotAreaParam> m_listHotAreas = null;

        private htmlScreen m_directHtmlScreen = null;


        private double m_wndWidth = 0.0;

        private double m_wndHeight = 0.0;

        public static float s_cellWidth = 0;

        public static float s_cellHeight = 0;

        private XmlDocument m_scriptDoc = new XmlDocument();

        private object m_syncShowScreen = new object();

        private object m_syncShowPopup = new object();
        private object m_syncHidePopup = new object();

        private static string sourcePath = AppDomain.CurrentDomain.BaseDirectory + @"Resource\Resource\";

        private Dictionary<string, Window> m_dicPopupWindow = new Dictionary<string, Window>();

        private IResourceManager m_iResourceManager = null;

        private DispatcherTimer m_reclaimTimer = null;

        private object m_curDataContext = null;

        private htmlScreen m_globalScreen = null;

        #endregion
    }
}
