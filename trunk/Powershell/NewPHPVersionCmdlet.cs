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
    [Cmdlet(VerbsCommon.New, "PHPVersion")]
    public sealed class NewPHPVersionCmdlet : BaseCmdlet
    {
        private string _scriptProcessor;

        [Parameter(Mandatory = true, Position = 0)]
        public string ScriptProcessor
        {
            get
            {
                return _scriptProcessor;
            }
            set
            {
                _scriptProcessor = value;
            }
        }

        protected override void ProcessRecord()
        {
            try
            {
                using (ServerManager serverManager = new ServerManager())
                {
                    ServerManagerWrapper serverManagerWrapper = new ServerManagerWrapper(serverManager, this.ConfigurationPath);
                    PHPConfigHelper _configHelper = new PHPConfigHelper(serverManagerWrapper);
                    string phpCgiExePath = PrepareFullScriptProcessorPath(ScriptProcessor);
                    _configHelper.RegisterPHPWithIIS(phpCgiExePath);
                }
            }
            catch (ArgumentException)
            {
                ArgumentException ex = new ArgumentException(Resources.ErrorInvalidPHPExecutablePath);
                ReportTerminatingError(ex, "InvalidPHPExecutablePath", ErrorCategory.InvalidArgument);
            }
            catch (FileNotFoundException)
            {
                FileNotFoundException ex = new FileNotFoundException(Resources.ErrorNoPHPFilesInDirectory);
                ReportTerminatingError(ex, "NoPHPFilesInDirectory", ErrorCategory.ObjectNotFound);
            }
            catch (DirectoryNotFoundException)
            {
                DirectoryNotFoundException ex = new DirectoryNotFoundException(Resources.ErrorNoExtDirectory);
                ReportTerminatingError(ex, "NoExtDirectory", ErrorCategory.ObjectNotFound);
            }
            catch (InvalidOperationException)
            {
                InvalidOperationException ex = new InvalidOperationException(Resources.ErrorFastCgiNotEnabled);
                ReportTerminatingError(ex, "NoFastCgi", ErrorCategory.ObjectNotFound);
            }
        }

        private string PrepareFullScriptProcessorPath(string scriptProcessor)
        {
            string fullPath = Path.GetFullPath(scriptProcessor);
            if (!fullPath.EndsWith("php-cgi.exe", StringComparison.OrdinalIgnoreCase))
            {
                fullPath = Path.Combine(fullPath, "php-cgi.exe");
            }
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException();
            }

            return fullPath;
        }
    }
}
