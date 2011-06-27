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

    [Cmdlet(VerbsCommon.Get, "PHPSetting")]
    [OutputType(typeof(PHPSettingItem))]
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

        protected override void DoProcessing()
        {
            using (ServerManager serverManager = new ServerManager())
            {
                ServerManagerWrapper serverManagerWrapper = new ServerManagerWrapper(serverManager, this.SiteName, this.VirtualPath);
                PHPConfigHelper configHelper = new PHPConfigHelper(serverManagerWrapper);
                PHPIniFile phpIniFile = configHelper.GetPHPIniFile();

                WildcardPattern nameWildcard = PrepareWildcardPattern(Name);
                WildcardPattern sectionWildcard = PrepareWildcardPattern(Section);

                foreach (PHPIniSetting setting in phpIniFile.Settings)
                {
                    if (!nameWildcard.IsMatch(setting.Name))
                    {
                        continue;
                    }
                    if (!sectionWildcard.IsMatch(setting.Section))
                    {
                        continue;
                    }

                    PHPSettingItem settingItem = new PHPSettingItem(setting);
                    WriteObject(settingItem);
                }
            }
        }
    }
}
