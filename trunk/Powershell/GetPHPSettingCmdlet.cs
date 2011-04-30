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
        private string _settingName;
        private string _sectionName;

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 2)]
        public string SectionName
        {
            get
            {
                return _sectionName;
            }
            set
            {
                _sectionName = value;
            }
        }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1)]
        public string SettingName
        {
            get
            {
                return _settingName;
            }
            set
            {
                _settingName = value;
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

                    bool filterBySettingName = false;
                    bool filterBySectionName = false;
                    if (!String.IsNullOrEmpty(SettingName))
                    {
                        filterBySettingName = true;
                    }
                    if (!String.IsNullOrEmpty(SectionName))
                    {
                        filterBySectionName = true;
                    }

                    foreach (PHPIniSetting setting in phpIniFile.Settings)
                    {
                        if (filterBySettingName)
                        {
                            if (setting.Name.IndexOf(SettingName, StringComparison.OrdinalIgnoreCase) == -1)
                            {
                                continue;
                            }
                        }
                        if (filterBySectionName)
                        {
                            if (setting.Section.IndexOf(SectionName, StringComparison.OrdinalIgnoreCase) == -1)
                            {
                                continue;
                            }
                        }

                        WriteObject(setting);
                    }

                }
            }
            catch (FileNotFoundException ex)
            {
                ReportError(ex, "FileNotFound", ErrorCategory.ObjectNotFound);
            }
        }
    }
}
