//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using Web.Management.PHP.Config;

namespace Web.Management.PHP.Powershell
{

    public enum PHPExtensionStatus { Any, Enabled, Disabled };

    public sealed class PHPExtensionItem
    {
        private readonly PHPIniExtension _extension;
        
        public PHPExtensionItem(PHPIniExtension extension)
        {
            _extension = extension;
        }

        public string Name
        {
            get
            {
                return _extension.Name;
            }
        }

        public PHPExtensionStatus Status
        {
            get
            {
                return _extension.Enabled ? PHPExtensionStatus.Enabled : PHPExtensionStatus.Disabled;
            }
        }
    }
}
