//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

namespace Web.Management.PHP.FastCgi
{

    public enum StderrMode
    {
        ReturnStdErrIn500 = 0,
        ReturnGeneric500 = 1,
        IgnoreAndReturn200 = 2,
        TerminateProcess = 3
    }
}
