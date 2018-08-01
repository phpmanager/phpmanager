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
    [Cmdlet(VerbsCommon.Set, "PHPVersion",
        SupportsShouldProcess = true,
        ConfirmImpact = ConfirmImpact.Medium)]
    public sealed class SetPHPVersionCmdlet : BaseCmdlet
    {
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 0)]
        public string HandlerName { get; set; }

        protected override void DoProcessing()
        {
            using (var serverManager = new ServerManager())
            {
                var serverManagerWrapper = new ServerManagerWrapper(serverManager, SiteName, VirtualPath);
                var configHelper = new PHPConfigHelper(serverManagerWrapper);
                if (configHelper.GetPHPHandlerByName(HandlerName) != null)
                {
                    if (ShouldProcess(HandlerName))
                    {
                        configHelper.SelectPHPHandler(HandlerName);
                    }
                }
                else
                {
                    var ex = new ArgumentException(String.Format(Resources.HandlerDoesNotExistError, HandlerName));
                    ReportNonTerminatingError(ex, "InvalidArgument", ErrorCategory.ObjectNotFound);
                }
            }
        }
    }
}
