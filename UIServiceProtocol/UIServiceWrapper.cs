/********************************************************************
	FileName:   UIServiceWrapper
    purpose:	

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
using System.Diagnostics;
using System.IO;

namespace UIServiceProtocol
{
    public sealed class UIServiceWrapper
    {
#region constructor
        public UIServiceWrapper( IUIService2 iUI )
        {
            Debug.Assert(null != iUI);
            m_iHostService = iUI;
        }
#endregion

#region property
        public IUIService HostService
        {
            get
            {
                return m_iHostService;
            }
        }
#endregion

#region method
    #region show 
        public bool ShowScreenSyn( string ScreenName,
                                   object argDataContext = null )
        {
            Debug.Assert(null != m_iHostService && !string.IsNullOrEmpty(ScreenName));

            if ( null != argDataContext )
            {
                return m_iHostService.ShowScreen(ScreenName, argDataContext, false);
            }
            else
            {
                return m_iHostService.ShowScreen(ScreenName, false);
            }         
        }

        public bool ShowScreenAsyn( string ScreenName,
                                    object argDataContext = null )
        {
            Debug.Assert(null != m_iHostService && !string.IsNullOrEmpty(ScreenName));

            if ( null != argDataContext )
            {
                return m_iHostService.ShowScreen(ScreenName, argDataContext, true);
            }
            else
            {
                return m_iHostService.ShowScreen(ScreenName, true);
            }
        }

            public string GetCurrentScreenPropertyValue(string ID, string propertyName)
    {
        return m_iHostService.GetCurrentScreenPropertyValue(ID, propertyName);
    }
        public bool ShowScreenScriptSyn(Stream StreamOfScreen,
                                        object argDataContext = null )
        {
            Debug.Assert(null != m_iHostService && null != StreamOfScreen);

            if ( null != argDataContext )
            {
                return m_iHostService.ShowScreenScript(StreamOfScreen, argDataContext, false);
            }
            else
            {
                return m_iHostService.ShowScreenScript(StreamOfScreen, false);
            }
            
        }

        public bool ShowScreenScriptAsyn( Stream StreamOfScreen,
                                          object argDataContext = null )
        {
            Debug.Assert(null != m_iHostService && null != StreamOfScreen);

            if ( null != argDataContext )
            {
                return m_iHostService.ShowScreenScript(StreamOfScreen, argDataContext, true);
            }
            else
            {
                return m_iHostService.ShowScreenScript(StreamOfScreen, true);
            }
            
        }

        public bool ShowDirectScreenSyn( string argScrFile,
                                         object argDataContext = null )
        {
            Debug.Assert(!string.IsNullOrEmpty(argScrFile) && null != m_iHostService);
            if ( null != argDataContext )
            {
                return m_iHostService.ShowDirectScreen(argScrFile, argDataContext, false);
            }
            else
            {
                return m_iHostService.ShowDirectScreen(argScrFile, false);
            }
        }

        public bool ShowDirectScreenAsyn( string argScrFile,
                                          object argDataContext = null )
        {
            Debug.Assert(null != m_iHostService && !string.IsNullOrEmpty(argScrFile));
            if ( null != argDataContext )
            {
                return m_iHostService.ShowDirectScreen(argScrFile, argDataContext, true);
            }
            else
            {
                return m_iHostService.ShowDirectScreen(argScrFile, true);
            }
        }

        public PopupDlgResult ShowPopupSyn( string NameOfPopup,
                                            bool argShow = true,
                                            bool bIsDialog = true )
        {
            Debug.Assert(null != m_iHostService );

            return m_iHostService.ShowPopup(NameOfPopup, argShow, bIsDialog);
        }

        public PopupDlgResult ShowPopupSyn(string NameOfPopup,
                                            object argDataContext,
                                            bool argShow = true,
                                            bool bIsDialog = true)
        {
            Debug.Assert(null != m_iHostService );

            return m_iHostService.ShowPopup(NameOfPopup, argDataContext, argShow, bIsDialog);
        }

        public PopupDlgResult ShowDirectPopupSyn( string argScrFile,
                                                  bool argShow = true,
                                                  bool argIsDialog = true )
        {
            Debug.Assert(null != m_iHostService );

            return m_iHostService.ShowDirectPopup(argScrFile, argShow, argIsDialog);
        }

        public PopupDlgResult ShowDirectPopupSyn( string argScrFile,
                                                  object argDataContext,
                                                  bool argShow = true,
                                                  bool argIsDialog = true )
        {
            Debug.Assert(null != m_iHostService);

            return m_iHostService.ShowDirectPopup(argScrFile, argDataContext, argShow, argIsDialog);
        }

        //public PopupDlgResult ShowPopupAsyn(string NameOfPopup,
        //                                    bool argShow = true,
        //                                    bool IsDialog = true )
        //{
        //    Debug.Assert(null != m_iHostService && !string.IsNullOrEmpty(NameOfPopup));

        //    return m_iHostService.ShowPopup(NameOfPopup, argShow, IsDialog, true);
        //}

        public PopupDlgResult ShowPopupScriptSyn(Stream PopupScriptStream,
                                                 bool argShow = true,
                                                 bool bIsDialog = true)
        {
            Debug.Assert(null != m_iHostService);

            return m_iHostService.ShowPopupScript(PopupScriptStream, argShow, bIsDialog);
        }

        public PopupDlgResult ShowPopupScriptSyn( Stream PopupScriptStream,
                                                  object argDataContext,
                                                 bool argShow = true,
                                                 bool bIsDialog = true)
        {
            Debug.Assert(null != m_iHostService);

            return m_iHostService.ShowPopupScript(PopupScriptStream, argDataContext, argShow, bIsDialog);
        }

        //public PopupDlgResult ShowPopupScriptAsyn(Stream PopupScriptStream,
        //                                          bool argShow = true,
        //                                          bool IsDialog = true )
        //{
        //    Debug.Assert(null != m_iHostService && null != PopupScriptStream);

        //    return m_iHostService.ShowPopupScript(PopupScriptStream, argShow, IsDialog, true);
        //}
    #endregion

    #region trigger
        public void TriggerEventSyn(string NameOfEvent)
        {
            Debug.Assert(null != m_iHostService && !string.IsNullOrEmpty(NameOfEvent));

            m_iHostService.TriggerEvent(NameOfEvent, false);
        }

        public void TriggerEventAsyn( string NameOfEvent )
        {
            Debug.Assert(null != m_iHostService && !string.IsNullOrEmpty(NameOfEvent));

            m_iHostService.TriggerEvent(NameOfEvent, true);
        }
    #endregion

        #region enumeration
        public void EnumScreens(EnumScreenNameHandler Handler,
                                 object Param)
        {
            Debug.Assert(null != m_iHostService);

            m_iHostService.EnumScreens(Handler, Param);
        }

        public void EnumElements(EnumElementHandler Handler,
                                  ElementType Type,
                                  EnumFlag Flag,
                                  string NameOfElement,
                                  object Param)
        {
            Debug.Assert(null != m_iHostService);

            m_iHostService.EnumElements(Handler, Type, Flag, NameOfElement, Param);
        }
        #endregion

        #region set/get value of property
        public bool SetPropertyValueOfElement(string ScreenName,
                                               string ElementName,
                                               string PropertyName,
                                               object Value)
        {
            Debug.Assert(null != m_iHostService);


            return m_iHostService.SetPropertyValueOfElement(ScreenName, ElementName, PropertyName, Value);
        }

        public bool GetPropertyValueOfElement(string ScreenName,
                                               string ElementName,
                                               string PropertyName,
                                               out object Value)
        {
            Debug.Assert(null != m_iHostService);

            return m_iHostService.GetPropertyValueOfElement(ScreenName, ElementName, PropertyName, out Value);
        }
        #endregion

        #region command
        public object ExecuteScriptCommand(string name,
                                          params object[] args)
        {
            Debug.Assert(null != m_iHostService && !string.IsNullOrEmpty(name));

            return m_iHostService.ExecuteCommand(UICommandType.Script, name, args);
        }

        public object ExecuteCustomCommand(string name,
                                          params object[] args)
        {
            Debug.Assert(null != m_iHostService && !string.IsNullOrEmpty(name));

            return m_iHostService.ExecuteCommand(UICommandType.Custom, name, args);
        }
        #endregion
#endregion

        #region field
        private IUIService2 m_iHostService = null;
#endregion
    }
}
