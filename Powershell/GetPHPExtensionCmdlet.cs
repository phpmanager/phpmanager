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

    [Cmdlet(VerbsCommon.Get, "PHPExtension")]
    [OutputType(typeof(PHPExtensionItem))]
    public sealed class GetPHPExtensionCmdlet : BaseCmdlet
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0)]
        public string Name { get; set; }

        [Parameter(ValueFromPipeline = false, Position = 1)]
        public PHPExtensionStatus Status { get; set; }

        protected override void DoProcessing()
        {
            using (var serverManager = new ServerManager())
            {
                var serverManagerWrapper = new ServerManagerWrapper(serverManager, SiteName, VirtualPath);
                var configHelper = new PHPConfigHelper(serverManagerWrapper);
                var phpIniFile = configHelper.GetPHPIniFile();

                WildcardPattern wildcard = PrepareWildcardPattern(Name);

                foreach (var extension in phpIniFile.Extensions)
                {
                    if (!wildcard.IsMatch(extension.Name))
                    {
                        continue;
                    }
                    if (Status == PHPExtensionStatus.Disabled && extension.Enabled)
                    {
                        continue;
                    }
                    if (Status == PHPExtensionStatus.Enabled && !extension.Enabled)
                    {
                        continue;
                    }

                    var extensionItem = new PHPExtensionItem(extension);

                    WriteObject(extensionItem);
                }
            }
        }
    }
}
