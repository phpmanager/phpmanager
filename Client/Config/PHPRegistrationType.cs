//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

namespace Web.Management.PHP.Config
{

    public enum PHPRegistrationType
    {
        FastCgi,
        Cgi,
        Isapi,
        None,
        NoneNoFastCgi
    }
}