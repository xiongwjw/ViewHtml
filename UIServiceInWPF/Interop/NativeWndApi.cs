/********************************************************************
	FileName:   NativeWndApi
    purpose:	

	author:		huang wei
	created:	2013/02/18

    revised history:
	2013/02/18  

================================================================
    Copyright (C) 2013, Grgbanking CO,. Ltd. All rights reserved.
================================================================
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace UIServiceInWPF.Interop
{
    public class NativeWndApi
    {
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_NOREDRAW = 0x0008;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_FRAMECHANGED = 0x0020;
        public const uint SWP_SHOWWINDOW = 0x0040;
        public const uint SWP_HIDEWINDOW = 0x0080;
        public const uint SWP_NOCOPYBITS = 0x0100;
        public const uint SWP_NOOWNERZORDER = 0x0200;
        public const uint SWP_NOSENDCHANGING = 0x0400;   
        [DllImport("user32.dll")]    
        public static extern bool SetWindowPos( IntPtr hWnd, 
                                                IntPtr hWndInsertAfter, 
                                                int X,
                                                int Y, 
                                                int cx, 
                                                int cy, 
                                                uint uFlags);
    
        [DllImport("user32.dll")]    
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern bool IsWindowVisible( IntPtr hWnd );


        public const int SW_FORCEMINIMIZE = 11;
        public const int SW_HIDE = 0;
        public const int SW_MAXIMIZE = 3;
        public const int SW_MINIMIZE = 6;
        public const int SW_RESTORE = 9;
        public const int SW_SHOW = 5;
        public const int SW_SHOWDEFAULT = 10;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMINNOACTIVE = 7;
        public const int SW_SHOWNA = 8;
        public const int SW_SHOWNOACTIVATE = 4;
        public const int SW_SHOWNORMAL = 1;
        [DllImport("User32.dll")]
        public static extern bool ShowWindow( IntPtr hWnd,
                                              int argCmd );

        public const int WM_SHOWWINDOW = 0x0018;
        [DllImport("User32.dll")]
        public static extern long SendMessage( IntPtr hWnd,
                                               uint msg,
                                               int wParam,
                                               long lParam );

        public const int GWL_STYLE = -16;

        public const int GWL_EXSTYLE = -20;

        public const int WS_BORDER = 0x00800000;

        public const int WS_EX_CLIENTEDGE = 0x00000200;
             
        [DllImport("User32.dll")]
        public static extern int GetWindowLong( IntPtr hwnd,
                                                int nIndex );

        [DllImport("User32.dll")]
        public static extern int SetWindowLong( IntPtr hwnd,
                                                int nIndex,
                                                int nValue );

        [DllImport("User32.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        public static extern uint MsgWaitForMultipleObjects(uint Count,
                                                     ref IntPtr Handler,
                                                     int WaitAll,
                                                     uint Timeout,
                                                     uint Mask);

        /////////////////////////////////////////////////////////////////////////
        //const value for waiting functions.
        public const int WaitObject0 = 0;

        public const uint WaitFailed = 0xFFFFFFFF;

        public const int WaitTimeout = 258;

        public const int WM_LBUTTONDOWN = 0x0201;

        public const int WM_LBUTTONUP = 0x0202;

        public const int PM_REMOVE = 1;

        //////////////////////////////////////////////////////////////////////////
        //const value for MsgWaitForMultipleObjects
        public const int ALLININPUT = 0x04FF;

        [DllImport("User32.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern int TranslateMessage(ref Message Msg);

        [DllImport("User32.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern int DispatchMessage(ref Message Msg);

        [DllImport("User32.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern int PeekMessage(ref Message Msg,
                                       IntPtr hwnd,
                                       uint MsgFilterMin,
                                       uint MsgFilterMax,
                                       uint RemoveMsg);

        [DllImport("User32.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern int ShowCursor( int argShow );

        [DllImport("Gdi32.dll")]
        public extern static IntPtr CreateCompatibleBitmap(IntPtr argHDC, int argWidth, int argHeight);

        [DllImport("Gdi32.dll")]
        public extern static bool DeleteObject(IntPtr argHDC);
    }
}
