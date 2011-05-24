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
        private string _handlerName;

        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 0)]
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

        protected override void DoProcessing()
        {
            using (ServerManager serverManager = new ServerManager())
            {
                ServerManagerWrapper serverManagerWrapper = new ServerManagerWrapper(serverManager, this.SiteName, this.VirtualPath);
                PHPConfigHelper configHelper = new PHPConfigHelper(serverManagerWrapper);
                if (configHelper.GetPHPHandlerByName(HandlerName) != null)
                {
                    if (ShouldProcess(HandlerName))
                    {
                        configHelper.SelectPHPHandler(HandlerName);
                    }
                }
                else
                {
                    ArgumentException ex = new ArgumentException(String.Format(Resources.HandlerDoesNotExistError, HandlerName));
                    ReportNonTerminatingError(ex, "InvalidArgument", ErrorCategory.ObjectNotFound);
                }
            }
        }
    }
}
