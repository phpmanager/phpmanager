//------------------------------------------------------------------------------
// <copyright file="PHPProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Web.Management.Server;

namespace Web.Management.PHP
{

    internal class PHPProvider : ModuleProvider
    {

        public override Type ServiceType
        {
            get {
                 return typeof(PHPService);
            }
        }

        public override ModuleDefinition GetModuleDefinition(IManagementContext context)
        {
            return new ModuleDefinition(Name, "Web.Management.PHP.PHPModule, Web.Management.PHP.Client,Version=1.0.0.0,Culture=neutral,PublicKeyToken=0135f7f354f8c066");
        }

        public override bool SupportsScope(ManagementScope scope)
        {
            return true;
        }
    }
}
