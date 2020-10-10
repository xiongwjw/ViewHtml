using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace UIServiceInWPF
{
    public class ExtendWebbrowser : WebBrowser
    {
#region constructor
        public ExtendWebbrowser()
        {
            this.DocumentCompleted += ExtendWebBrowser_DocumentCompleted;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
           // this.DoubleBuffered = true;
            this.IsWebBrowserContextMenuEnabled = false;
            this.WebBrowserShortcutsEnabled = false;
            this.ScriptErrorsSuppressed = true;
        }
#endregion

        [DllImport("user32.dll")]
        protected static extern int GetWindowLong(int hwindow, int unindex);
        [DllImport("user32.dll")]
        protected static extern int CallWindowProc(int lpPrevWndFunc, int hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        protected static extern int SetWindowLong(int hwindow, int unindex, CallWindowProcDelegate lnewvalue); 
        
        public static int oldWindow = 0; 
        public const int GWL_WNDPROC = -4;
        public const int WM_ERASEBKGND = 0x14;
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_SYSKEYDOWN = 0x0104;
        public const int VK_MENU = 0x12;
        public const int VK_F4 = 0x73;
        public delegate int CallWindowProcDelegate(int Wnd, int Msg, int WParam, int LParam); 
        public CallWindowProcDelegate MyCallWindowProc;
        bool add = false; 
        void ExtendWebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e) 
        { 
            if (add) 
            {
                return; 
            } 
            add = true; 
            FindWindow fw = new FindWindow(this.Handle, "Internet Explorer_Server"); 
            IntPtr hIE = fw.FoundHandle; 
            if (hIE.ToInt32() != 0) 
            { 
                oldWindow = (int)GetWindowLong(hIE.ToInt32(), GWL_WNDPROC); 
                MyCallWindowProc = new CallWindowProcDelegate(WndProc); 
                SetWindowLong(hIE.ToInt32(), GWL_WNDPROC, MyCallWindowProc); 
            } 
        }           
        private int WndProc(int Wnd, int Msg, int WParam, int LParam) 
        {
            if (Msg == WM_ERASEBKGND)
            {
                return 1;
            } 
            if ( Msg == WM_KEYDOWN )
            {
                if ( WParam == VK_F4 ||
                     WParam == VK_MENU )
                {
                    return 0;
                }
            }
            if (Msg == WM_SYSKEYDOWN)
            {
                return 0;
            }

            return CallWindowProc(oldWindow, Wnd, Msg, WParam, LParam); 
        }   
    }

    class FindWindow
    {
        [DllImport("user32")]         
        [return: MarshalAs(UnmanagedType.Bool)]    
        private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr i);         
        //the callback function for the EnumChildWindows         
        private delegate bool EnumWindowProc(IntPtr hWnd, IntPtr parameter);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow); 
        private string m_classname; 
        // class name to look for           
        private IntPtr m_hWnd; 
        // HWND if found
        public IntPtr FoundHandle         
        {
            get { return m_hWnd; } 
        }

        // ctor does the work--just instantiate and go         
        public FindWindow(IntPtr hwndParent, string classname)
        {            
            m_hWnd = IntPtr.Zero;             
            m_classname = classname;
            FindChildClassHwnd(hwndParent, IntPtr.Zero);  
        } 

        private bool FindChildClassHwnd(IntPtr hwndParent, IntPtr lParam)         
        {
            EnumWindowProc childProc = new EnumWindowProc(FindChildClassHwnd);            
            IntPtr hwnd = FindWindowEx(hwndParent, IntPtr.Zero, this.m_classname, string.Empty);           
            if (hwnd != IntPtr.Zero)            
            {                
                this.m_hWnd = hwnd; // found: save it             
                return false; // stop enumerating            
            }             
            EnumChildWindows(hwndParent, childProc, IntPtr.Zero); 
            // recurse  redo FindChildClassHwnd           
            return true;// keep looking       
        } 
    }
}
