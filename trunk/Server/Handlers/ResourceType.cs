//------------------------------------------------------------------------------
// <copyright file="ResourceType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Web.Management.PHP.Handlers
{

    internal enum ResourceType
    {
        File = 0,
        Directory = 1,
        Either = 2,
        Unspecified = 3
    }
}
