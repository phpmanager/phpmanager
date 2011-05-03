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

namespace Web.Management.PHP
{

    [Cmdlet(VerbsCommon.Get, "PHPSetting")]
    public sealed class GetPHPSettingCmdlet : BaseCmdlet
    {
        private string _name;
        private string _section;

        [Parameter(Position = 0)]
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

        [Parameter]
        public string Section
        {
            get
            {
                return _section;
            }
            set
            {
                _section = value;
            }
        }

        protected override void ProcessRecord()
        {
            EnsureAdminUser();

            try
            {
                using (ServerManager serverManager = new ServerManager())
                {
                    ServerManagerWrapper serverManagerWrapper = new ServerManagerWrapper(serverManager, this.ConfigurationPath);
                    PHPConfigHelper configHelper = new PHPConfigHelper(serverManagerWrapper);
                    PHPIniFile phpIniFile = configHelper.GetPHPIniFile();

                    bool filterByName = false;
                    bool filterBySection = false;
                    if (!String.IsNullOrEmpty(Name))
                    {
                        filterByName = true;
                    }
                    if (!String.IsNullOrEmpty(Section))
                    {
                        filterBySection = true;
                    }

                    foreach (PHPIniSetting setting in phpIniFile.Settings)
                    {
                        if (filterByName)
                        {
                            if (setting.Name.IndexOf(Name, StringComparison.OrdinalIgnoreCase) == -1)
                            {
                                continue;
                            }
                        }
                        if (filterBySection)
                        {
                            if (setting.Section.IndexOf(Section, StringComparison.OrdinalIgnoreCase) == -1)
                            {
                                continue;
                            }
                        }

                        PHPSettingItem settingItem = new PHPSettingItem(setting);
                        WriteObject(settingItem);
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                ReportTerminatingError(ex, "FileNotFound", ErrorCategory.ObjectNotFound);
            }
        }
    }
}
