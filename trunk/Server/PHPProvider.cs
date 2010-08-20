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
using System.Reflection;

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
            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = assembly.GetName();
            string assemblyFullName = assemblyName.FullName;
            string clientAssemblyFullName = assemblyFullName.Replace(assemblyName.Name, "Web.Management.PHP.Client");

            return new ModuleDefinition(Name, "Web.Management.PHP.PHPModule, " + clientAssemblyFullName);
        }

        public override bool SupportsScope(ManagementScope scope)
        {
            return true;
        }
    }
}
