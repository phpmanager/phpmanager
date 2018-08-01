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

    [Cmdlet(VerbsCommon.Set, "PHPSetting",
        SupportsShouldProcess = true,
        ConfirmImpact = ConfirmImpact.Medium)]
    public sealed class SetPHPSettingCmdlet : BaseCmdlet
    {
        [Parameter(Mandatory = true,
            Position = 0)]
        public string Name { get; set; }

        [Parameter(Mandatory = true, Position = 1)]
        public string Value { get; set; }

        protected override void DoProcessing()
        {
            using (var serverManager = new ServerManager())
            {
                var serverManagerWrapper = new ServerManagerWrapper(serverManager, SiteName, VirtualPath);
                var configHelper = new PHPConfigHelper(serverManagerWrapper);
                var phpIniFile = configHelper.GetPHPIniFile();

                var setting = Helper.FindSetting(phpIniFile.Settings, Name);
                if (setting != null)
                {
                    if (ShouldProcess(Name))
                    {
                        var settings = new RemoteObjectCollection<PHPIniSetting>
                            {
                                new PHPIniSetting(Name, Value, setting.Section)
                            };
                        configHelper.AddOrUpdatePHPIniSettings(settings);
                    }
                }
                else
                {
                    var ex = new ArgumentException(String.Format(Resources.SettingDoesNotExistError, Name));
                    ReportNonTerminatingError(ex, "InvalidArgument", ErrorCategory.ObjectNotFound);
                }
            }
        }
    }
}