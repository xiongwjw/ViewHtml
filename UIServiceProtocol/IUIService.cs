/********************************************************************
	FileName:   IUIService
    purpose:	The IUIService protocol defines a UIService module need to implement methods,events and properties

	author:		huang wei
	created:	2012/12/19

    revised history:
	2012/12/19  

================================================================
    Copyright (C) 2012, Grgbanking CO,. Ltd. All rights reserved.
================================================================
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ResourceManagerProtocol;

namespace UIServiceProtocol
{
    /// <summary>
    /// The callback routine used to enumerate names of loaded screens by the UI service
    /// </summary>
    /// <param name="strName">The name of a screen</param>
    /// <param name="objParam">The user define object</param>
    /// <returns>If you want to continue to enumerate, the function returns true.Otherwise,the function returns false</returns>
    public delegate bool EnumScreenNameHandler( string argName,
                                                object argParam );

    /// <summary>
    /// The callback routine used to enumerate elements in a screen.
    /// </summary>
    /// <param name="NameOfElement">name of a element</param>
    /// <param name="Key">key of a element</param>
    /// <param name="objParam">The user define object</param>
    /// <returns>If you want to continue to enumerate, the function returns true.Otherwise,the function returns false</returns>
    public delegate bool EnumElementHandler( string argNameOfElement,
                                             object argKey,
                                             object argParam );

    /// <summary>
    /// The callback routine used to notify upper application a event of UI service has raised.
    /// </summary>
    /// <param name="piService">The IUIService interface</param>
    /// <param name="objArg">The argument of a event</param>
    public delegate bool UIEventHandler( IUIService argiService,
                                         UIEventArg argArg );

    public delegate object ExecuteFunctionHandler( object argParam );

    /// <summary>
    /// type of a UI command.
    /// Script : Javascript code hosted in a html.
    /// Custom : customize commands.
    /// </summary>
    [Flags]
    public enum UICommandType : byte
    {
        Script = 0, 
        Custom
    }
    /// <summary>
    /// type of a element.
    /// All : all types.
    /// Button : a button element.
    /// Input : a Input element.
    /// </summary>
    [Flags]
    public enum ElementType : byte
    {
        All = 0,
        Button,
        Input
    }
    /// <summary>
    /// flag for enumeration flag.
    /// VisibleAndEnable :
    /// OnlyVisible :
    /// VisibleOrEnable :
    /// </summary>
    [Flags]
    public enum EnumFlag : byte
    {
        VisibleAndEnable = 0,
        OnlyVisible,
        OnlyEnable,
        VisibleOrEnable
    }

    [Flags]
    public enum PopupDlgResult : byte
    {
        None = 0,
        Ok,
        Cancel,
        Abort,
        Retry,
        Ignore,
        Yes,
        No,
        Fail,
        Timeout
    }

    /// <summary>
    /// Protocol of the IUIService interface.
    /// </summary>
    public interface IUIService
    {
#region method
        /// <summary>
        /// Open a UI service
        /// </summary>
        /// <param name="strCfg">The configuration of a UI service</param>
        /// <param name="hwndParent">The parent window to host the UIService</param>
        /// <returns>If it is successful, it returns true.Otherwise, it returns false</returns>
        bool Open( string argCfg,
                   IntPtr argHwndParent,
                   IResourceManager iargResourceManager = null );

        /// <summary>
        /// Close a UI service
        /// </summary>
        void Close();

        /// <summary>
        /// Show the screen on the UI Service
        /// </summary>
        /// <param name="strScrName">The name of a screen</param>
        /// <param name="bAsyn">Indicate whether showing screen is asynchronous.
        /// If it is true, showing screen is asynchronous. Otherwise, that is synchronous</param>
        /// <returns>If it is successful, it returns true.Otherwise, it returns false</returns>
        bool ShowScreen(string argScrName,
                         bool argAsyn);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="argScrName"></param>
        /// <param name="argDataContext"></param>
        /// <param name="argAsyn"></param>
        /// <returns></returns>
        bool ShowScreen(string argScrName,
                        object argDataContext,
                        bool argAsyn);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ScreenScriptStream"></param>
        /// <param name="Asyn"></param>
        /// <returns></returns>
        bool ShowScreenScript( Stream argScreenScript,
                               bool argAsyn );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="argScreenScript"></param>
        /// <param name="argDataContext"></param>
        /// <param name="argAsyn"></param>
        /// <returns></returns>
        bool ShowScreenScript(Stream argScreenScript,
                               object argDataContext,
                               bool argAsyn);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strScrName"></param>
        /// <param name="IsDialog"></param>
        /// <returns></returns>
        PopupDlgResult ShowPopup( string argScrName,
                        bool argShow = true,
                        bool argIsDialog = true);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="argScrName"></param>
        /// <param name="timeout"></param>
        /// <param name="argShow"></param>
        /// <param name="argIsDialog"></param>
        /// <returns></returns>
        PopupDlgResult ShowPopup(string argScrName,
                        int timeout,
                        bool argShow = true,
                        bool argIsDialog = true);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="argScrName"></param>
        /// <param name="argDataContext"></param>
        /// <param name="argShow"></param>
        /// <param name="argIsDialog"></param>
        /// <returns></returns>
        PopupDlgResult ShowPopup( string argScrName,
                                  object argDataContext,
                                  bool argShow = true,
                                  bool argIsDialog = true );
        /// <summary>
        /// 关闭弹出窗口
        /// </summary>
        void HidePopup();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="PopupScriptStream"></param>
        /// <param name="IsDialog"></param>
        /// <returns></returns>
        PopupDlgResult ShowPopupScript(Stream argPopupScriptStream,
                              bool argShow = true,
                              bool argIsDialog = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="argPopupScriptStream"></param>
        /// <param name="argDataContext"></param>
        /// <param name="argShow"></param>
        /// <param name="argIsDialog"></param>
        /// <returns></returns>
        PopupDlgResult ShowPopupScript(Stream argPopupScriptStream,
                                        object argDataContext,
                                        bool argShow = true,
                                        bool argIsDialog = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objHandler"></param>
        /// <param name="objParam"></param>
        void EnumScreens( EnumScreenNameHandler argHandler,
                          object argParam );


        /// <summary>
        /// 
        /// </summary>
        /// <param name="Handler"></param>
        /// <param name="Type"></param>
        /// <param name="Flag"></param>
        /// <param name="NameOfElement"></param>
        /// <param name="Param"></param>
        void EnumElements( EnumElementHandler argHandler,
                           ElementType argType,
                           EnumFlag argFlag,
                           string argNameOfElement,
                           object argParam );
        /// <summary>
        /// Trigger a configuration event
        /// </summary>
        /// <param name="strEvtName">The name of a event</param>
        /// <param name="bAsyn">Indicate whether triggering is asynchronous.
        /// If it is true, triggering is asynchronous. Otherwise, that is synchronous</param>
        void TriggerEvent(string argEvtName,
                           bool argAsyn);

        /// <summary>
        /// Set value of a property of a element 
        /// </summary>
        /// <param name="strScrName">The name of a screen</param>
        /// <param name="strElement">The name of a element</param>
        /// <param name="strProperty">The name of a property</param>
        /// <param name="objValue">The value of a property</param>
        /// <returns>If it is successful, it returns true.Otherwise, it returns false</returns>
        bool SetPropertyValueOfElement(string argScrName,
                                        string argElement,
                                        string argProperty,
                                        object argValue);

        /// <summary>
        /// Get value of a property of a element
        /// </summary>
        /// <param name="strScrName">The name of a screen</param>
        /// <param name="strElement">The name of a element</param>
        /// <param name="strProperty">The name of a property</param>
        /// <param name="objValue">The value of a property</param>
        /// <returns>If it is successful, it returns true.Otherwise, it returns false</returns>
        bool GetPropertyValueOfElement(string argScrName,
                                        string argElement,
                                        string argProperty,
                                        out object argValue);

        /// <summary>
        /// Reload the configuration of a UI service
        /// </summary>
        /// <param name="strCfg">The configuration file</param>
        /// <returns>If it is successful, it returns true.Otherwise, it returns false</returns>
        bool Reload(string argCfg = null);

        ///// <summary>
        ///// Create IUIElementBind interface of a screen
        ///// </summary>
        ///// <param name="strScrName">The name of the screen</param>
        ///// <param name="piBind">The created IUIElementBind interface</param>
        ///// <returns>If it is successful, it returns true.Otherwise, it returns false</returns>
        //bool CreateUIBind(string strScrName,
        //                   out IUIElementBind piBind);

        /// <summary>
        /// Force to the UI Service redraws current screen.
        /// </summary>
        void RedrawUI();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="command"></param>
        /// <param name="result"></param>
        /// <param name="args"></param>
        object ExecuteCommand( UICommandType argType,
                             string argCommand,
                             params object[] argArgs );
#endregion

#region event
        /// <summary>
        /// event of a UI service.
        /// </summary>
        event UIEventHandler UIEvent;


        event Action<string, string, object> ValueChange;
#endregion

#region property
        /// <summary>
        /// Last error code of handing a UI service command.
        /// </summary>
        int LastErrorCode
        {
            get;
        }

        /// <summary>
        /// Name of the UI service.
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// Check if the UI service was opened.
        /// </summary>
        bool Opened
        {
            get;
        }
        /// <summary>
        /// Width of the main window
        /// </summary>
        int Width
        {
            get;
        }
        /// <summary>
        /// Height of the main window
        /// </summary>
        int Height
        {
            get;
        }
        /// <summary>
        /// 
        /// </summary>
        string CurrentScreenName
        {
            get;
        }
        ///// <summary>
        ///// 
        ///// </summary>
        //string CurrentLanguage
        //{
        //    get;
        //    set;
        //}

        IResourceManager ResoureManager
        {
            get;
            set;
        }

        bool Visible
        {
            get;
            set;
        }

        IntPtr HandleOfMainWindow
        {
            get;
        }
#endregion
    }

    public interface IUIService2 : IUIService
    {
        string GetCurrentScreenPropertyValue(string ID, string propertyName);
        bool ShowDirectScreen( string argScrFile,
                               bool argIsAsyn );

        bool ShowDirectScreen(string argScrFile,
                               object argContext,
                               bool argIsAsyn);

        PopupDlgResult ShowDirectPopup( string argScrFile,
                                        bool argShow = true,
                                        bool argIsDialog = true );

        PopupDlgResult ShowDirectPopup( string argScrFile,
                                        object argContext,
                                        bool argShow = true,
                                        bool argIsDialog = true );

        object ExecuteFunction( ExecuteFunctionHandler argHandler,
                                object argParam );
  
    }
}
