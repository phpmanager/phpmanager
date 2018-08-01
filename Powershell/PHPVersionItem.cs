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

    public sealed class PHPVersionItem
    {
        readonly PHPVersion _phpVersion;
        readonly bool _active;

        public PHPVersionItem(PHPVersion phpVersion, bool active)
        {
            _phpVersion = phpVersion;
            _active = active;
        }

        public string HandlerName
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

        public bool Active
        {
            get
            {
                return _active;
            }
        }
    }
}
