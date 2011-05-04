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

namespace Web.Management.PHP
{
    [Cmdlet(VerbsCommon.Set, "PHPVersion",
        SupportsShouldProcess = true,
        ConfirmImpact = ConfirmImpact.Medium)]
    public sealed class SetPHPVersionCmdlet : BaseCmdlet
    {
        private string _name;

        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            Position = 0)]
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

        protected override void ProcessRecord()
        {
            try
            {
                using (ServerManager serverManager = new ServerManager())
                {
                    ServerManagerWrapper serverManagerWrapper = new ServerManagerWrapper(serverManager, this.ConfigurationPath);
                    PHPConfigHelper configHelper = new PHPConfigHelper(serverManagerWrapper);
                    if (configHelper.GetPHPHandlerByName(Name) != null)
                    {
                        if (ShouldProcess(Name))
                        {
                            configHelper.SelectPHPHandler(Name);
                        }
                    }
                    else
                    {
                        ArgumentException e = new ArgumentException(String.Format("PHP handler with name \"{0}\" does not exist.", Name));
                        ReportNonTerminatingError(e, "InvalidHandlerName", ErrorCategory.InvalidArgument);
                    }
                }
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                ReportTerminatingError(fileNotFoundException, "FileNotFound", ErrorCategory.ObjectNotFound);
            }
        }
    }
}
