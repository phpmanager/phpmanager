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

namespace Web.Management.PHP
{
    public sealed class PHPSettingItem
    {
        private PHPIniSetting _setting;

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
                if (String.IsNullOrEmpty(_setting.Value))
                {
                    return "<Not set>";
                }
                else
                {
                    return _setting.Value;
                }
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
