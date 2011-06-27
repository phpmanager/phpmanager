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

    [Cmdlet(VerbsCommon.Get, "PHPVersion")]
    public sealed class GetPHPVersionCmdlet : BaseCmdlet
    {
        private string _handlerName;
        private string _version;

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0)]
        public string HandlerName
        {
            get
            {
                return _handlerName;
            }
            set
            {
                _handlerName = value;
            }
        }

        [Parameter(ValueFromPipeline = false, Position = 1)]
        public string Version
        {
            get
            {
                return _version;
            }
            set
            {
                _version = value;
            }
        }

        protected override void DoProcessing()
        {
            using (ServerManager serverManager = new ServerManager())
            {
                ServerManagerWrapper serverManagerWrapper = new ServerManagerWrapper(serverManager, this.SiteName, this.VirtualPath);
                PHPConfigHelper configHelper = new PHPConfigHelper(serverManagerWrapper);
                RemoteObjectCollection<PHPVersion> phpVersions = configHelper.GetAllPHPVersions();

                WildcardPattern nameWildcard = PrepareWildcardPattern(HandlerName);
                WildcardPattern versionWildcard = PrepareWildcardPattern(Version);

                bool isActive = true;
                foreach (PHPVersion phpVersion in phpVersions)
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

                    PHPVersionItem versionItem = new PHPVersionItem(phpVersion, isActive);
                    WriteObject(versionItem);
                    isActive = false;
                }
            }
        }
    }
}
