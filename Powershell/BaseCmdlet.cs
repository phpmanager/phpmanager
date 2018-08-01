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
using System.Security.Principal;

namespace Web.Management.PHP.Powershell
{

    public abstract class BaseCmdlet : PSCmdlet
    {
        [Parameter(ValueFromPipeline = false)]
        public string SiteName { get; set; }

        [Parameter(ValueFromPipeline = false)]
        public string VirtualPath { get; set; }

        protected abstract void DoProcessing();

        protected void EnsureAdminUser()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            var sidAdmin = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
            if (!principal.IsInRole(sidAdmin))
            {
                var exception = new UnauthorizedAccessException(Resources.UserIsNotAdminError);
                ReportTerminatingError(exception, "UnathorizedAccess", ErrorCategory.PermissionDenied);
            }
        }

        protected static WildcardPattern PrepareWildcardPattern(string pattern)
        {
            const WildcardOptions options = WildcardOptions.IgnoreCase | WildcardOptions.Compiled;
            WildcardPattern wildcard;

            if (!String.IsNullOrEmpty(pattern))
            {
                wildcard = new WildcardPattern(pattern, options);
            }
            else
            {
                wildcard = new WildcardPattern("*", options);
            }

            return wildcard;
        }

        protected override void ProcessRecord()
        {
            EnsureAdminUser();

            try
            {
                DoProcessing();
            }
            catch (FileNotFoundException ex)
            {
                ReportTerminatingError(ex, "FileNotFound", ErrorCategory.ObjectNotFound);
            }
            catch (InvalidOperationException ex)
            {
                ReportTerminatingError(ex, "InvalidOperation", ErrorCategory.InvalidOperation);
            }
        }

        protected void ReportNonTerminatingError(Exception exception, string errorId, ErrorCategory errorCategory)
        {
            var errorRecord = new ErrorRecord(exception, errorId, errorCategory, null);
            WriteError(errorRecord);
        }

        protected void ReportTerminatingError(Exception exception, string errorId, ErrorCategory errorCategory)
        {
            var errorRecord = new ErrorRecord(exception, errorId, errorCategory, null);
            ThrowTerminatingError(errorRecord);
        }
    }
}
