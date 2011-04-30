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

namespace Web.Management.PHP
{

    [Cmdlet(VerbsCommon.Get, "PHPVersions")]
    public sealed class GetPHPVersionsCmdlet : BaseCmdlet
    {
        protected override void ProcessRecord()
        {
            EnsureAdminUser();
            
            using (ServerManager serverManager = new ServerManager())
            {
                ServerManagerWrapper serverManagerWrapper = new ServerManagerWrapper(serverManager, this.ConfigurationPath);
                PHPConfigHelper configHelper = new PHPConfigHelper(serverManagerWrapper);
                RemoteObjectCollection<PHPVersion> phpVersions = configHelper.GetAllPHPVersions();
                foreach (PHPVersion phpVersion in phpVersions)
                {
                    WriteObject(phpVersion);
                }
            }
        }
    }
}
