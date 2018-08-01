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
        [Parameter(Mandatory = true, Position = 0)]
        public string ScriptProcessor { get; set; }

        protected override void DoProcessing()
        {
            try
            {
                using (var serverManager = new ServerManager())
                {
                    var serverManagerWrapper = new ServerManagerWrapper(serverManager, SiteName, VirtualPath);
                    var configHelper = new PHPConfigHelper(serverManagerWrapper);
                    var phpCgiExePath = PrepareFullScriptProcessorPath(ScriptProcessor);
                    configHelper.RegisterPHPWithIIS(phpCgiExePath);
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                ReportTerminatingError(ex, "DirectoryNotFound", ErrorCategory.ObjectNotFound);
            }
        }

        private static string PrepareFullScriptProcessorPath(string scriptProcessor)
        {
            var fullPath = Path.GetFullPath(scriptProcessor);
            if (!fullPath.EndsWith("php-cgi.exe", StringComparison.OrdinalIgnoreCase))
            {
                fullPath = Path.Combine(fullPath, "php-cgi.exe");
            }
            return fullPath;
        }
    }
}
