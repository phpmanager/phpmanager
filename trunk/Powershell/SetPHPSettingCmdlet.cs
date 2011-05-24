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
        private string _name;
        private string _value;

        [Parameter(Mandatory = true,
                   Position = 0)]
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        [Parameter(Mandatory = true, Position = 1)]
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        private static PHPIniSetting FindSetting(RemoteObjectCollection<PHPIniSetting> settings, string name)
        {
            PHPIniSetting result = null;

            foreach (PHPIniSetting setting in settings)
            {
                if (String.Equals(setting.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    result = setting;
                    break;
                }
            }

            return result;
        }

        protected override void DoProcessing()
        {
            using (ServerManager serverManager = new ServerManager())
            {
                ServerManagerWrapper serverManagerWrapper = new ServerManagerWrapper(serverManager, this.SiteName, this.VirtualPath);
                PHPConfigHelper configHelper = new PHPConfigHelper(serverManagerWrapper);
                PHPIniFile phpIniFile = configHelper.GetPHPIniFile();

                PHPIniSetting setting = FindSetting(phpIniFile.Settings, Name);
                if (setting != null)
                {
                    if (ShouldProcess(Name))
                    {
                        RemoteObjectCollection<PHPIniSetting> settings = new RemoteObjectCollection<PHPIniSetting>();
                        settings.Add(new PHPIniSetting(Name, Value, setting.Section));
                        configHelper.AddOrUpdatePHPIniSettings(settings);
                    }
                }
                else
                {
                    ArgumentException ex = new ArgumentException(String.Format(Resources.SettingDoesNotExistError, Name));
                    ReportNonTerminatingError(ex, "InvalidArgument", ErrorCategory.ObjectNotFound);
                    return;
                }
            }
        }
    }
}