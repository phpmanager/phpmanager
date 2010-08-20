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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Web.Management.Client;
using Microsoft.Web.Management.Server;
using Web.Management.PHP.Extensions;
using Web.Management.PHP.Settings;
using Web.Management.PHP.Setup;

namespace Web.Management.PHP
{

    internal sealed class PHPModule : Module
    {
        private PHPModuleProxy _proxy;

        internal PHPModuleProxy Proxy
        {
            get
            {
                if (_proxy == null)
                {
                    Connection connection = (Connection)GetService(typeof(Connection));
                    _proxy = (PHPModuleProxy)connection.CreateProxy(this, typeof(PHPModuleProxy));
                }

                return _proxy;
            }
        }

        protected override void Initialize(IServiceProvider serviceProvider, ModuleInfo moduleInfo)
        {
            base.Initialize(serviceProvider, moduleInfo);

            IControlPanel controlPanel = (IControlPanel)GetService(typeof(IControlPanel));
            Debug.Assert(controlPanel != null, "Couldn't get IControlPanel");

            //PHPInfo page
            ModulePageInfo modulePageInfo = new ModulePageInfo(this,
                typeof(PHPInfoPage), Resources.PHPInfoPageTitle, Resources.PHPInfoPageDescription,
                Resources.PHPLogo16, Resources.PHPLogo32, Resources.PHPInfoPageLongDescription);

            controlPanel.RegisterPage(modulePageInfo);

            //PHP Settings page
            modulePageInfo = new ModulePageInfo(this,
                typeof(AllSettingsPage), Resources.PHPSettingsPageTitle, Resources.PHPSettingsPageDescription,
                Resources.PHPLogo16, Resources.PHPLogo32, Resources.PHPSettingsPageLongDescription);

            controlPanel.RegisterPage(modulePageInfo);

            modulePageInfo = new ModulePageInfo(this,
                typeof(ErrorReportingPage), Resources.ErrorReportingPageTitle, Resources.ErrorReportingPageDescription,
                Resources.PHPLogo16, Resources.PHPLogo32, Resources.ErrorReportingPageLongDescription);

            controlPanel.RegisterPage(modulePageInfo);

            modulePageInfo = new ModulePageInfo(this,
                typeof(RuntimeLimitsPage), Resources.RuntimeLimitsPageTitle, Resources.RuntimeLimitsPageDescription,
                Resources.PHPLogo16, Resources.PHPLogo32, Resources.RuntimeLimitsPageLongDescription);

            controlPanel.RegisterPage(modulePageInfo);

            //PHP Extensions page
            modulePageInfo = new ModulePageInfo(this,
                typeof(AllExtensionsPage), Resources.PHPExtensionsPageTitle, Resources.PHPExtensionsPageDescription,
                Resources.PHPLogo16, Resources.PHPLogo32, Resources.PHPExtensionsPageLongDescription);

            controlPanel.RegisterPage(modulePageInfo);


            //PHPPage - PHP feature start page
            modulePageInfo = new ModulePageInfo(this, 
                typeof(PHPPage), Resources.PHPPageTitle, Resources.PHPPageDescription,
                Resources.PHPLogo16, Resources.PHPLogo32, Resources.PHPPageLongDescription);

            controlPanel.RegisterPage(ControlPanelCategoryInfo.Iis, modulePageInfo);
            controlPanel.RegisterPage(ControlPanelCategoryInfo.ApplicationDevelopment, modulePageInfo);
        }

        protected override bool IsPageEnabled(ModulePageInfo pageInfo)
        {
            Connection connection = (Connection)GetService(typeof(Connection));

            // We only want the module configuration to be available on site, application of folder levels.
            return (connection.ConfigurationPath.PathType == ConfigurationPathType.Server ||
                    connection.ConfigurationPath.PathType == ConfigurationPathType.Site);
        }

    }
}