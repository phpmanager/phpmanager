//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using Microsoft.Web.Administration;
using Web.Management.PHP.Config;
using Web.Management.PHP.Handlers;

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

        protected override void ProcessRecord()
        {
            try
            {
                using (ServerManager serverManager = new ServerManager())
                {
                    ServerManagerWrapper serverManagerWrapper = new ServerManagerWrapper(serverManager, this.ConfigurationPath);
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
                        throw new ArgumentException(String.Format(Resources.ErrorHandlerDoesNotExist, HandlerName));
                    }
                }
            }
            catch (FileNotFoundException)
            {
                FileNotFoundException ex = new FileNotFoundException(Resources.ErrorPHPIniNotFound);
                ReportTerminatingError(ex, "FileNotFound", ErrorCategory.ObjectNotFound);
            }
            catch (InvalidOperationException)
            {
                InvalidOperationException ex = new InvalidOperationException(Resources.ErrorPHPIsNotRegistered);
                ReportTerminatingError(ex, "PHPIsNotRegistered", ErrorCategory.InvalidOperation);
            }
            catch (ArgumentException argumentException)
            {
                ReportNonTerminatingError(argumentException, "InvalidHandlerName", ErrorCategory.InvalidArgument);
            }
        }
    }
}
