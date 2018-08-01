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

    [Cmdlet(VerbsCommon.Get, "PHPVersion")]
    public sealed class GetPHPVersionCmdlet : BaseCmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0)]
        public string HandlerName { get; set; }

        [Parameter(ValueFromPipeline = false, Position = 1)]
        public string Version { get; set; }

        protected override void DoProcessing()
        {
            using (var serverManager = new ServerManager())
            {
                var serverManagerWrapper = new ServerManagerWrapper(serverManager, SiteName, VirtualPath);
                var configHelper = new PHPConfigHelper(serverManagerWrapper);
                var phpVersions = configHelper.GetAllPHPVersions();

                var nameWildcard = PrepareWildcardPattern(HandlerName);
                var versionWildcard = PrepareWildcardPattern(Version);

                var isActive = true;
                foreach (var phpVersion in phpVersions)
                {
                    if (!nameWildcard.IsMatch(phpVersion.HandlerName))
                    {
                        isActive = false;
                        continue;
                    }
                    if (!versionWildcard.IsMatch(phpVersion.Version))
                    {
                        isActive = false;
                        continue;
                    }

                    var versionItem = new PHPVersionItem(phpVersion, isActive);
                    WriteObject(versionItem);
                    isActive = false;
                }
            }
        }
    }
}
