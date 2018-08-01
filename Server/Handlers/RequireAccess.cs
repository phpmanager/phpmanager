//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

namespace Web.Management.PHP.Handlers
{

    public enum RequireAccess
    {
        None = 0,
        Read = 1,
        Write = 2,
        Script = 3,
        Execute = 4
    }
}
