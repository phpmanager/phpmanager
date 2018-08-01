//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System;

namespace Web.Management.PHP.Config
{

    public sealed class PHPConfigInfo : IRemoteObject
    {
        private object[] _data;

        private const int IndexPhpRegistrationType = 0;
        private const int IndexHandlerName = 1;
        private const int IndexHandlerIsLocal = 2;
        private const int IndexExecutable = 3;
        private const int IndexVersion = 4;
        private const int IndexPhpIniFilePath = 5;
        private const int IndexErrorLog = 6;
        private const int IndexEnabledExtCount = 7;
        private const int IndexInstalledExtCount = 8;
        private const int IndexIsConfigOptimal = 9;

        private const int Size = 10;

        public PHPConfigInfo()
        {
            _data = new object[Size];
            HandlerName = String.Empty;
            HandlerIsLocal = false;
            Executable = String.Empty;
            Version = String.Empty;
            PHPIniFilePath = String.Empty;
            ErrorLog = String.Empty;
            EnabledExtCount = 0;
            InstalledExtCount = 0;
            IsConfigOptimal = false;
        }

        public int EnabledExtCount
        {
            get
            {
                return (int)_data[IndexEnabledExtCount];
            }
            set
            {
                _data[IndexEnabledExtCount] = value;
            }
        }

        public string ErrorLog
        {
            get
            {
                return (string)_data[IndexErrorLog];
            }
            set
            {
                _data[IndexErrorLog] = value;
            }
        }

        public string Executable
        {
            get
            {
                return (string)_data[IndexExecutable];
            }
            set
            {
                _data[IndexExecutable] = value;
            }
        }

        public bool HandlerIsLocal
        {
            get
            {
                return (bool)_data[IndexHandlerIsLocal];
            }
            set
            {
                _data[IndexHandlerIsLocal] = value;
            }
        }

        public string HandlerName
        {
            get
            {
                return (string)_data[IndexHandlerName];
            }
            set
            {
                _data[IndexHandlerName] = value;
            }
        }

        public int InstalledExtCount
        {
            get
            {
                return (int)_data[IndexInstalledExtCount];
            }
            set
            {
                _data[IndexInstalledExtCount] = value;
            }
        }

        public bool IsConfigOptimal
        {
            get
            {
                return (bool)_data[IndexIsConfigOptimal];
            }
            set
            {
                _data[IndexIsConfigOptimal] = value;
            }
        }

        public string PHPIniFilePath
        {
            get
            {
                return (string)_data[IndexPhpIniFilePath];
            }
            set
            {
                _data[IndexPhpIniFilePath] = value;
            }
        }

        public PHPRegistrationType RegistrationType
        {
            get
            {
                return (PHPRegistrationType)_data[IndexPhpRegistrationType];
            }
            set
            {
                _data[IndexPhpRegistrationType] = (int)value;
            }
        }

        public string Version
        {
            get
            {
                return (string)_data[IndexVersion];
            }
            set
            {
                _data[IndexVersion] = value;
            }
        }

        #region IRemoteObject Members

        public object GetData()
        {
            return _data;
        }

        public void SetData(object o)
        {
            _data = (object[])o;
        }

        #endregion
    }
}