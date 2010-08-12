//------------------------------------------------------------------------------
// <copyright file="StderrMode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Web.Management.PHP.FastCgi
{

    internal enum StderrMode
    {
        ReturnStdErrIn500 = 0,
        ReturnGeneric500 = 1,
        IgnoreAndReturn200 = 2,
        TerminateProcess = 3
    }
}
