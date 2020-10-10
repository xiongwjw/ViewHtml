/********************************************************************
	FileName:   ImmApi
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

namespace UIServiceInWPF.Interop
{
    public class ImmApi
    {
        public const int IME_CMODE_SOFTKBD = 0x80;

        [DllImport("imm32.dll")]
        public static extern IntPtr ImmGetContext(IntPtr hwnd);

        [DllImport("imm32.dll")]
        public static extern int ImmGetConversionStatus( IntPtr himc,
                                                          ref IntPtr lpdw1,
                                                          ref IntPtr lpdw2 );

        [DllImport("imm32.dll")]
        public static extern int ImmSetConversionStatus( IntPtr himc,
                                                         IntPtr lpdw1,
                                                         IntPtr lpdw2 );

        [DllImport("imm32.dll")]
        public static extern int ImmReleaseContext( IntPtr hwnd,
                                                    IntPtr himc );

        [DllImport("imm32.dll")]
        public static extern int ImmSetOpenStatus( IntPtr himc,
                                                   int argOpen );

        [DllImport("imm32.dll")]
        public static extern int ImmGetOpenStatus( IntPtr himc );
    }
}
