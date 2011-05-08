//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System;
using System.IO;
using System.Management.Automation;
using Microsoft.Web.Administration;
using Web.Management.PHP.Config;

namespace Web.Management.PHP.Powershell
{

    [Cmdlet(VerbsCommon.Get, "PHPConfiguration")]
    [OutputType(typeof(PHPConfigurationItem))]
    public sealed class GetPHPConfigurationCmdlet : BaseCmdlet
    {

        protected override void ProcessRecord()
        {
            EnsureAdminUser();

            try
            {
                using (ServerManager serverManager = new ServerManager())
                {
                    ServerManagerWrapper serverManagerWrapper = new ServerManagerWrapper(serverManager, this.ConfigurationPath);
                    PHPConfigHelper configHelper = new PHPConfigHelper(serverManagerWrapper);
                    PHPConfigInfo configInfo = configHelper.GetPHPConfigInfo();
                    if (configInfo.RegistrationType == PHPRegistrationType.FastCgi)
                    {
                        PHPConfigurationItem configurationItem = new PHPConfigurationItem(configInfo);
                        WriteObject(configurationItem);
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }
            }
            catch (FileNotFoundException)
            {
                FileNotFoundException ex = new FileNotFoundException(Resources.ErrorPHPIniNotFound);
                ReportTerminatingError(ex, "FileNotFound", ErrorCategory.ObjectNotFound);
            }
            catch (InvalidOperationException)
            {
                InvalidOperationException ex = new InvalidOperationException(Resources.ErrorPHPIsNotRegistered);
                ReportTerminatingError(ex, "PHPIsNotRegistered", ErrorCategory.InvalidOperation);
            }
        }
    }
}
