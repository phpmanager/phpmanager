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

    internal sealed class PHPConfigInfo : IRemoteObject
    {
        private object[] _data;

        private const int IndexHandlerName = 0;
        private const int IndexScriptProcessor = 1;
        private const int IndexVersion = 2;
        private const int IndexPHPIniFilePath = 3;
        private const int IndexErrorLog = 4;
        private const int IndexEnabledExtCount = 5;
        private const int IndexInstalledExtCount = 6;

        private const int Size = 7;

        public PHPConfigInfo()
        {
            _data = new object[Size];
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

        public string PHPIniFilePath
        {
            get
            {
                return (string)_data[IndexPHPIniFilePath];
            }
            set
            {
                _data[IndexPHPIniFilePath] = value;
            }
        }

        public string ScriptProcessor
        {
            get
            {
                return (string)_data[IndexScriptProcessor];
            }
            set
            {
                _data[IndexScriptProcessor] = value;
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