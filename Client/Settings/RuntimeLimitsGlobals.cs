//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System;

namespace Web.Management.PHP.Settings
{

    internal static class RuntimeLimitsGlobals
    {
        public const int MaxExecutionTime = 0;
        public const int MaxInputTime = 1;
        public const int MemoryLimit = 2;
        public const int PostMaxSize = 3;
        public const int UploadMaxFilesize = 4;
        public const int MaxFileUploads = 5;
    }
}
