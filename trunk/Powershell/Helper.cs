//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System;
using Web.Management.PHP.Config;

namespace Web.Management.PHP.Powershell
{

    internal static class Helper
    {

        public static PHPIniExtension FindExtension(RemoteObjectCollection<PHPIniExtension> extensions, string name)
        {
            PHPIniExtension result = null;

            foreach (PHPIniExtension extension in extensions)
            {
                if (String.Equals(extension.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    result = extension;
                    break;
                }
            }

            return result;
        }

        public static PHPIniSetting FindSetting(RemoteObjectCollection<PHPIniSetting> settings, string name)
        {
            PHPIniSetting result = null;

            foreach (PHPIniSetting setting in settings)
            {
                if (String.Equals(setting.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    result = setting;
                    break;
                }
            }

            return result;
        }

    }
}
