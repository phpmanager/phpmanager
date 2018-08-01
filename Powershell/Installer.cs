//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//-----------------------------------------------------------------------


using System.ComponentModel;
using System.Management.Automation;

namespace Web.Management.PHP.Powershell
{

    [RunInstaller(true)]
    public class Installer : PSSnapIn
    {

        public override string Description
        {
            get 
            {
                return Resources.SnapInDescription;
            }
        }

        public override string Name
        {
            get 
            {
                return "PHPManagerSnapin";
            }
        }

        public override string Vendor
        {
            get 
            {
                return Resources.SnapInVendor;
            }
        }
    }
}
