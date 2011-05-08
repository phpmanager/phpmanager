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

        protected override void ProcessRecord()
        {
            EnsureAdminUser();

            try
            {
                using (ServerManager serverManager = new ServerManager())
                {
                    ServerManagerWrapper serverManagerWrapper = new ServerManagerWrapper(serverManager, this.ConfigurationPath);
                    PHPConfigHelper configHelper = new PHPConfigHelper(serverManagerWrapper);
                    RemoteObjectCollection<PHPVersion> phpVersions = configHelper.GetAllPHPVersions();

                    bool filterByName = false;
                    bool filterByVersion = false;
                    if (!String.IsNullOrEmpty(HandlerName))
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
                            if (!MatchWildcards(HandlerName, phpVersion.HandlerName))
                            {
                                isActive = false;
                                continue;
                            }
                        }
                        if (filterByVersion)
                        {
                            if (!MatchWildcards(Version, phpVersion.Version))
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
        }
    }
}
