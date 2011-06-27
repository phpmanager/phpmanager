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

        protected override void DoProcessing()
        {
            try
            {
                using (ServerManager serverManager = new ServerManager())
                {
                    ServerManagerWrapper serverManagerWrapper = new ServerManagerWrapper(serverManager, this.SiteName, this.VirtualPath);
                    PHPConfigHelper _configHelper = new PHPConfigHelper(serverManagerWrapper);
                    string phpCgiExePath = PrepareFullScriptProcessorPath(ScriptProcessor);
                    _configHelper.RegisterPHPWithIIS(phpCgiExePath);
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                ReportTerminatingError(ex, "DirectoryNotFound", ErrorCategory.ObjectNotFound);
            }
        }

        private string PrepareFullScriptProcessorPath(string scriptProcessor)
        {
            string fullPath = Path.GetFullPath(scriptProcessor);
            if (!fullPath.EndsWith("php-cgi.exe", StringComparison.OrdinalIgnoreCase))
            {
                fullPath = Path.Combine(fullPath, "php-cgi.exe");
            }
            return fullPath;
        }
    }
}
