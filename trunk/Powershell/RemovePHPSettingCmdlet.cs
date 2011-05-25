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

    [Cmdlet(VerbsCommon.Remove, "PHPSetting",
            SupportsShouldProcess = true,
            ConfirmImpact = ConfirmImpact.Medium)]
    public sealed class RemovePHPSettingCmdlet : BaseCmdlet
    {
        private string _name;
        private bool _force;

        [Parameter(Mandatory = false)]
        public SwitchParameter Force
        {
            get {
                 return _force;
            }
            set {
                 _force = value;
            }
        }

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

        protected override void DoProcessing()
        {
            using (ServerManager serverManager = new ServerManager())
            {
                ServerManagerWrapper serverManagerWrapper = new ServerManagerWrapper(serverManager, this.SiteName, this.VirtualPath);
                PHPConfigHelper configHelper = new PHPConfigHelper(serverManagerWrapper);
                PHPIniFile phpIniFile = configHelper.GetPHPIniFile();

                PHPIniSetting setting = Helper.FindSetting(phpIniFile.Settings, Name);
                if (setting != null)
                {
                    if (ShouldProcess(Name))
                    {
                        string warningMessage = String.Format(Resources.DeleteSettingWarningMessage, setting.Name, setting.Value);
                        if (Force || ShouldContinue(warningMessage, Resources.DeleteSettingWarningCaption))
                        {
                            configHelper.RemovePHPIniSetting(setting);
                        }
                    }
                }
                else
                {
                    ArgumentException ex = new ArgumentException(String.Format(Resources.SettingDoesNotExistError, Name));
                    ReportNonTerminatingError(ex, "InvalidArgument", ErrorCategory.ObjectNotFound);
                }
            }
        }
    }
}
