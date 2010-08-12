//------------------------------------------------------------------------------
// <copyright file="RequireAccess.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Web.Management.PHP.Handlers
{

    internal enum RequireAccess
    {
        None = 0,
        Read = 1,
        Write = 2,
        Script = 3,
        Execute = 4
    }
}
