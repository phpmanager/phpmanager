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

namespace Web.Management.PHP.Powershell
{

    [Cmdlet(VerbsCommon.Set, "PHPExtension", 
            SupportsShouldProcess = true, 
            ConfirmImpact = ConfirmImpact.Medium)]
    public sealed class SetPHPExtensionCmdlet : BaseCmdlet
    {
        private PHPExtensionStatus _status = PHPExtensionStatus.Any;

        private ServerManager _serverManager;
        private PHPConfigHelper _configHelper;
        private PHPIniFile _phpIniFile;
        private RemoteObjectCollection<PHPIniExtension> _extensions;

        [Parameter( Mandatory = true, 
            ValueFromPipelineByPropertyName = true, 
            Position = 0)]
        public string[] Name { get; set; }

        [Parameter(Mandatory = true, Position = 1)]
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

        protected override void BeginProcessing()
        {
            EnsureAdminUser();

            try
            {
                _serverManager = new ServerManager();
                var serverManagerWrapper = new ServerManagerWrapper(_serverManager, SiteName, VirtualPath);
                _configHelper = new PHPConfigHelper(serverManagerWrapper);
                _phpIniFile = _configHelper.GetPHPIniFile();
                _extensions = new RemoteObjectCollection<PHPIniExtension>();
            }
            catch (FileNotFoundException ex)
            {
                DisposeServerManager();
                ReportTerminatingError(ex, "FileNotFound", ErrorCategory.ObjectNotFound);
            }
            catch (InvalidOperationException ex)
            {
                DisposeServerManager();
                ReportTerminatingError(ex, "InvalidOperation", ErrorCategory.InvalidOperation);
            }
        }

        private void DisposeServerManager()
        {
            if (_serverManager != null)
            {
                _serverManager.Dispose();
                _serverManager = null;
            }
        }

        protected override void DoProcessing()
        {
            Debug.Assert(_phpIniFile != null);
            Debug.Assert(_extensions != null);

            foreach (var extensionName in Name)
            {
                var extension = Helper.FindExtension(_phpIniFile.Extensions, extensionName);
                if (extension != null)
                {
                    if ((extension.Enabled && Status == PHPExtensionStatus.Disabled) ||
                        (!extension.Enabled && Status == PHPExtensionStatus.Enabled))
                    {
                        if (ShouldProcess(extensionName))
                        {
                            extension = new PHPIniExtension(extensionName, (Status == PHPExtensionStatus.Enabled));
                            _extensions.Add(extension);
                        }
                    }
                }
                else
                {
                    var ex = new ArgumentException(String.Format(Resources.ExtensionDoesNotExistError, extensionName));
                    ReportNonTerminatingError(ex, "InvalidArgument", ErrorCategory.ObjectNotFound);
                    return;
                }
            }
        }

        protected override void EndProcessing()
        {
            Debug.Assert(_extensions != null);
            Debug.Assert(_configHelper != null);
            Debug.Assert(_serverManager != null);

            if (_extensions.Count > 0)
            {
                _configHelper.UpdateExtensions(_extensions);
            }

            DisposeServerManager();
        }
    }
}
