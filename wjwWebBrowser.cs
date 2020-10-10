using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ViewHtml
{
    public class wjwWebBrowser :WebBrowser
    {
        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;
        public bool isCtrlKeyPress = false;
        const int MOUSEEVENTF_WHEEL = 0x0800; //{ wheel button rolled } 
        public override bool PreProcessMessage(ref Message msg)
        {
            switch (msg.Msg)
            {
                case WM_KEYDOWN:
                    if ((int)(msg.WParam) == 17)  //control key
                        isCtrlKeyPress = true;
                    break;
                case WM_KEYUP:
                    if ((int)(msg.WParam) == 17)  //control key
                        isCtrlKeyPress = false;
                    break;
                //case MOUSEEVENTF_WHEEL:
                //    break;
            }
            return base.PreProcessMessage(ref msg);
        }
    }
}
