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

namespace Web.Management.PHP
{

    [Cmdlet(VerbsCommon.Get, "PHPVersion")]
    public sealed class GetPHPVersionCmdlet : BaseCmdlet
    {
        private string _name;
        private string _version;

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

        protected override void ProcessRecord()
        {
            EnsureAdminUser();
            
            using (ServerManager serverManager = new ServerManager())
            {
                ServerManagerWrapper serverManagerWrapper = new ServerManagerWrapper(serverManager, this.ConfigurationPath);
                PHPConfigHelper configHelper = new PHPConfigHelper(serverManagerWrapper);
                RemoteObjectCollection<PHPVersion> phpVersions = configHelper.GetAllPHPVersions();

                bool filterByName = false;
                bool filterByVersion = false;
                if (!String.IsNullOrEmpty(Name))
                {
                    filterByName = true;
                }
                if (!String.IsNullOrEmpty(Version))
                {
                    filterByVersion = true;
                }
                
                bool isActive = true;
                foreach (PHPVersion phpVersion in phpVersions)
                {
                    if (filterByName)
                    {
                        if (phpVersion.HandlerName.IndexOf(Name, StringComparison.OrdinalIgnoreCase) == -1)
                        {
                            isActive = false;
                            continue;
                        }
                    }
                    if (filterByVersion)
                    {
                        if (phpVersion.Version.IndexOf(Version, StringComparison.OrdinalIgnoreCase) == -1)
                        {
                            isActive = false;
                            continue;
                        }
                    }

                    PHPVersionItem versionItem = new PHPVersionItem(phpVersion, isActive);
                    WriteObject(versionItem);
                    isActive = false;
                }
            }
        }
    }
}
