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

        private string[] _name;
        private PHPExtensionStatus _status = PHPExtensionStatus.Any;

        private ServerManager _serverManager;
        private PHPConfigHelper _configHelper;
        private PHPIniFile _phpIniFile;
        private RemoteObjectCollection<PHPIniExtension> _extensions;

        [Parameter( Mandatory = true, 
                    ValueFromPipelineByPropertyName = true, 
                    Position = 0)]
        public string[] Name
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
                ServerManagerWrapper serverManagerWrapper = new ServerManagerWrapper(_serverManager, this.SiteName, this.VirtualPath);
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

            foreach (string extensionName in Name)
            {
                bool currentlyEnabled = false;
                if (!ExtensionExists(_phpIniFile.Extensions, extensionName, out currentlyEnabled))
                {
                    ArgumentException ex = new ArgumentException(String.Format(Resources.ExtensionDoesNotExistError, extensionName));
                    ReportNonTerminatingError(ex, "InvalidArgument", ErrorCategory.ObjectNotFound);
                    return;
                }

                if ((currentlyEnabled && Status == PHPExtensionStatus.Disabled) ||
                    (!currentlyEnabled && Status == PHPExtensionStatus.Enabled))
                {
                    if (ShouldProcess(extensionName))
                    {
                        PHPIniExtension extension = new PHPIniExtension(extensionName, (Status == PHPExtensionStatus.Enabled) ? true : false);
                        _extensions.Add(extension);
                    }
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

        private static bool ExtensionExists(RemoteObjectCollection<PHPIniExtension> extensions, string name, out bool enabled)
        {
            bool found = false;
            enabled = false;

            foreach (PHPIniExtension extension in extensions)
            {
                if (String.Equals(extension.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                    enabled = extension.Enabled;
                    break;
                }
            }

            return found;
        }

    }
}
