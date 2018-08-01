//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System;
using System.Management.Automation;
using Microsoft.Web.Administration;
using Web.Management.PHP.Config;

namespace Web.Management.PHP.Powershell
{

    [Cmdlet(VerbsCommon.Get, "PHPConfiguration")]
    [OutputType(typeof(PHPConfigurationItem))]
    public sealed class GetPHPConfigurationCmdlet : BaseCmdlet
    {
        protected override void DoProcessing()
        {
            using (var serverManager = new ServerManager())
            {
                var serverManagerWrapper = new ServerManagerWrapper(serverManager, SiteName, VirtualPath);
                var configHelper = new PHPConfigHelper(serverManagerWrapper);
                var configInfo = configHelper.GetPHPConfigInfo();
                if (configInfo.RegistrationType == PHPRegistrationType.FastCgi)
                {
                    var configurationItem = new PHPConfigurationItem(configInfo);
                    WriteObject(configurationItem);
                }
                else
                {
                    throw new InvalidOperationException(Resources.PHPIsNotRegisteredError);
                }
            }
        }
    }
}
