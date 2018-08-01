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
    public sealed class PHPSettingItem
    {
        private readonly PHPIniSetting _setting;

        public PHPSettingItem(PHPIniSetting setting)
        {
            _setting = setting;
        }

        public string Name
        {
            get
            {
                return _setting.Name;
            }
        }

        public string Value
        {
            get
            {
                return String.IsNullOrEmpty(_setting.Value) ? Resources.NotSetValue : _setting.Value;
            }
        }

        public string Section
        {
            get
            {
                return _setting.Section;
            }
        }
    }
}
