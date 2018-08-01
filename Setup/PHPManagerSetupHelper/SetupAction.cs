//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System.ComponentModel;
using System.Configuration.Install;
using System.Reflection;

namespace Web.Management.PHP.Setup
{

    [RunInstaller(true)]
    public class SetupAction : Installer
    {

        public override void Install(System.Collections.IDictionary stateSaver)
        {
            base.Install(stateSaver);

            var assembly = Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName();
            var assemblyFullName = assemblyName.FullName;
            var clientAssemblyFullName = assemblyFullName.Replace(assemblyName.Name, "Web.Management.PHP");

            InstallUtil.RemoveUIModuleProvider("PHP"); // This is necessary for the upgrade scenario
            InstallUtil.AddUIModuleProvider("PHP", "Web.Management.PHP.PHPProvider, " + clientAssemblyFullName);
        }

        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            base.Uninstall(savedState);

            InstallUtil.RemoveUIModuleProvider("PHP");
        }
    }
}
