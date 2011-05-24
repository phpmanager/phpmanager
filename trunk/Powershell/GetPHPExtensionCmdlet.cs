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

    [Cmdlet(VerbsCommon.Get, "PHPExtension")]
    [OutputType(typeof(PHPExtensionItem))]
    public sealed class GetPHPExtensionCmdlet : BaseCmdlet
    {
        private string _name;
        private PHPExtensionStatus _status;

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0)]
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

        [Parameter(ValueFromPipeline = false, Position = 1)]
        public PHPExtensionStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
            }
        }

        protected override void DoProcessing()
        {
            using (ServerManager serverManager = new ServerManager())
            {
                ServerManagerWrapper serverManagerWrapper = new ServerManagerWrapper(serverManager, this.SiteName, this.VirtualPath);
                PHPConfigHelper configHelper = new PHPConfigHelper(serverManagerWrapper);
                PHPIniFile phpIniFile = configHelper.GetPHPIniFile();

                WildcardPattern wildcard = PrepareWildcardPattern(Name);

                foreach (PHPIniExtension extension in phpIniFile.Extensions)
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

                    PHPExtensionItem extensionItem = new PHPExtensionItem(extension);

                    WriteObject(extensionItem);
                }
            }
        }
    }
}
