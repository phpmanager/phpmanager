//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System.Management.Automation;
using Microsoft.Web.Administration;
using Web.Management.PHP.Config;

namespace Web.Management.PHP
{

    [Cmdlet(VerbsCommon.Get, "PHPConfiguration")]
    [OutputType(typeof(PHPConfigurationItem))]
    public sealed class GetPHPConfigurationCmdlet : BaseCmdlet
    {

        protected override void ProcessRecord()
        {
            EnsureAdminUser();

            using (ServerManager serverManager = new ServerManager())
            {
                ServerManagerWrapper serverManagerWrapper = new ServerManagerWrapper(serverManager, this.ConfigurationPath);
                PHPConfigHelper configHelper = new PHPConfigHelper(serverManagerWrapper);
                PHPConfigInfo configInfo = configHelper.GetPHPConfigInfo();
                if (configInfo != null)
                {
                    PHPConfigurationItem configurationItem = new PHPConfigurationItem(configInfo);
                    WriteObject(configurationItem);
                }
            }
        }
    }
}
