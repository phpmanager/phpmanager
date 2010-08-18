//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 


using System;
using System.Collections.Generic;
using Microsoft.Web.Management.Server;

namespace Web.Management.PHP
{

    internal sealed class PHPProvider : ModuleProvider
    {

        public override Type ServiceType
        {
            get {
                 return typeof(PHPService);
            }
        }

        public override ModuleDefinition GetModuleDefinition(IManagementContext context)
        {
            return new ModuleDefinition(Name, "Web.Management.PHP.PHPModule, Web.Management.PHP.Client,Version=1.0.0.0,Culture=neutral,PublicKeyToken=8175de49a9aec91d");
        }

        public override bool SupportsScope(ManagementScope scope)
        {
            return true;
        }
    }
}
