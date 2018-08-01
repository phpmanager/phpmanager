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
    public enum HandlerRegistrationType { Local, Inherited };

    public sealed class PHPConfigurationItem
    {
        private readonly PHPConfigInfo _configInfo;

        public PHPConfigurationItem(PHPConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        public string HandlerName
        {
            get
            {
                return _configInfo.HandlerName;
            }
        }

        public string Version
        {
            get
            {
                return _configInfo.Version;
            }
        }

        public string ScriptProcessor
        {
            get
            {
                return _configInfo.Executable;
            }
        }

        public HandlerRegistrationType HandlerType
        {
            get
            {
                return _configInfo.HandlerIsLocal ? HandlerRegistrationType.Local : HandlerRegistrationType.Inherited;
            }
        }

        public string ErrorLog
        {
            get
            {
                return _configInfo.ErrorLog;
            }
        }

        public string PHPIniFilePath
        {
            get
            {
                return _configInfo.PHPIniFilePath;
            }
        }

        public int InstalledExtensionsCount
        {
            get
            {
                return _configInfo.InstalledExtCount;
            }
        }

        public int EnabledExtensionsCount
        {
            get
            {
                return _configInfo.EnabledExtCount;
            }
        }
    }
}
