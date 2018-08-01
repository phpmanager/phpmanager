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

namespace Web.Management.PHP.Powershell
{

    [Cmdlet(VerbsCommon.Get, "PHPSetting")]
    [OutputType(typeof(PHPSettingItem))]
    public sealed class GetPHPSettingCmdlet : BaseCmdlet
    {
        [Parameter(Position = 0)]
        public string Name { get; set; }

        [Parameter]
        public string Section { get; set; }

        protected override void DoProcessing()
        {
            using (var serverManager = new ServerManager())
            {
                var serverManagerWrapper = new ServerManagerWrapper(serverManager, SiteName, VirtualPath);
                var configHelper = new PHPConfigHelper(serverManagerWrapper);
                var phpIniFile = configHelper.GetPHPIniFile();

                var nameWildcard = PrepareWildcardPattern(Name);
                var sectionWildcard = PrepareWildcardPattern(Section);

                foreach (var setting in phpIniFile.Settings)
                {
                    if (!nameWildcard.IsMatch(setting.Name))
                    {
                        continue;
                    }
                    if (!sectionWildcard.IsMatch(setting.Section))
                    {
                        continue;
                    }

                    var settingItem = new PHPSettingItem(setting);
                    WriteObject(settingItem);
                }
            }
        }
    }
}
