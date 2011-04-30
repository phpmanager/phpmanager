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

namespace Web.Management.PHP
{

    public class BaseCommand : PSCmdlet
    {

        protected static void EnsureAdminUser()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            SecurityIdentifier sidAdmin = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
            if (!principal.IsInRole(sidAdmin))
            {
                throw new UnauthorizedAccessException(Resources.UserIsNotAdminException);
            }
        }
    }
}
