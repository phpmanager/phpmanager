//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System;
using System.IO;
using System.Management.Automation;
using Microsoft.Web.Administration;
using Web.Management.PHP.Config;

namespace Web.Management.PHP
{

    [Cmdlet(VerbsCommon.Get, "PHPExtension")]
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

        protected override void ProcessRecord()
        {
            EnsureAdminUser();

            try
            {
                using (ServerManager serverManager = new ServerManager())
                {
                    ServerManagerWrapper serverManagerWrapper = new ServerManagerWrapper(serverManager, this.ConfigurationPath);
                    PHPConfigHelper configHelper = new PHPConfigHelper(serverManagerWrapper);
                    PHPIniFile phpIniFile = configHelper.GetPHPIniFile();

                    bool filterByName = false;
                    if (!String.IsNullOrEmpty(Name))
                    {
                        filterByName = true;
                    }

                    foreach (PHPIniExtension extension in phpIniFile.Extensions)
                    {
                        if (filterByName)
                        {
                            if (extension.Name.IndexOf(Name, StringComparison.OrdinalIgnoreCase) == -1)
                            {
                                continue;
                            }
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
            catch (FileNotFoundException ex)
            {
                ReportError(ex, "FileNotFound", ErrorCategory.ObjectNotFound);
            }

        }
    }
}
