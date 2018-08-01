//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System;
using System.Reflection;
using Microsoft.Web.Management.Server;

namespace Web.Management.PHP
{

    internal sealed class PHPProvider : ModuleProvider
    {

        public override string FriendlyName
        {
            get
            {
                return Resources.PHPManagerFriendlyName;
            }
        }

        public override Type ServiceType
        {
            get {
                 return typeof(PHPService);
            }
        }

        public override ModuleDefinition GetModuleDefinition(IManagementContext context)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName();
            var assemblyFullName = assemblyName.FullName;
            var clientAssemblyFullName = assemblyFullName.Replace(assemblyName.Name, "Web.Management.PHP.Client");

            return new ModuleDefinition(Name, "Web.Management.PHP.PHPModule, " + clientAssemblyFullName);
        }

        public override bool SupportsScope(ManagementScope scope)
        {
            return (scope == ManagementScope.Site) ||
                   (scope == ManagementScope.Server);
        }
    }
}
