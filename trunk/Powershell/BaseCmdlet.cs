//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System;
using System.Management.Automation;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace Web.Management.PHP.Powershell
{

    public class BaseCmdlet : PSCmdlet
    {
        private string _configurationPath;

        [Parameter(ValueFromPipeline = false)]
        public string ConfigurationPath
        {
            set
            {
                _configurationPath = value;
            }
            get
            {
                return _configurationPath;
            }
        }

        protected void EnsureAdminUser()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            SecurityIdentifier sidAdmin = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
            if (!principal.IsInRole(sidAdmin))
            {
                UnauthorizedAccessException exception = new UnauthorizedAccessException(Resources.ErrorUserIsNotAdmin);
                ReportTerminatingError(exception, "PermissionDenied", ErrorCategory.PermissionDenied);
            }
        }

        protected static bool MatchWildcards(string pattern, string text)  
        {
            pattern = String.Format("^{0}$", Regex.Escape(pattern).Replace("\\*", ".*"));
            return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase);
        }

        protected void ReportNonTerminatingError(Exception exception, string errorId, ErrorCategory errorCategory)
        {
            ErrorRecord errorRecord = new ErrorRecord(exception, errorId, errorCategory, null);
            WriteError(errorRecord);
        }

        protected void ReportTerminatingError(Exception exception, string errorId, ErrorCategory errorCategory)
        {
            ErrorRecord errorRecord = new ErrorRecord(exception, errorId, errorCategory, null);
            ThrowTerminatingError(errorRecord);
        }

    }
}
