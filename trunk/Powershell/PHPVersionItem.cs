//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System.Management.Automation;
using Web.Management.PHP.Config;

namespace Web.Management.PHP
{

    public sealed class PHPVersionItem
    {
        PHPVersion _phpVersion;
        bool _isActive;

        public PHPVersionItem(PHPVersion phpVersion, bool isActive)
        {
            _phpVersion = phpVersion;
            _isActive = isActive;
        }

        public string Name
        {
            get
            {
                return _phpVersion.HandlerName;
            }
        }

        public string Version
        {
            get
            {
                return _phpVersion.Version;
            }
        }

        public string ScriptProcessor
        {
            get
            {
                return _phpVersion.ScriptProcessor;
            }
        }

        public bool IsActive
        {
            get
            {
                return _isActive;
            }
        }
    }
}
