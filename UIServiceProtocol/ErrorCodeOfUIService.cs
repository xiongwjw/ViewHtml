/********************************************************************
	FileName:   ErrorCodeOfUIService
    purpose:	defines of error code of a UI service

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

namespace UIServiceProtocol
{
    /// <summary>
    /// Error codes of a UI service
    /// </summary>
    public static class ErrorCodeOfUIService
    {
        /// <summary>
        /// Unknown error code
        /// </summary>
        public const int s_Unknown = 0;

        /// <summary>
        /// The service didn't open.
        /// </summary>
        public const int s_Unopen = 1;

        /// <summary>
        /// Error of the handling thread of UI service 
        /// </summary>
        public const int s_ThreadError = 2;

        /// <summary>
        /// Unknown name of a screen.
        /// </summary>
        public const int s_UnknownScreen = 3;

        /// <summary>
        /// Unknown property of a element
        /// </summary>
        public const int s_UnknownProperty = 4;

        /// <summary>
        /// Unknown name of a element in a screen
        /// </summary>
        public const int s_UnknownElement = 5;

        /// <summary>
        /// Failed to parse script of a UI service.
        /// </summary>
        public const int s_ParseFailure = 6;
    }
}
