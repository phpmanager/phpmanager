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

    // Used internally for the select version combo box
    public sealed class PHPVersion : IRemoteObject
    {
        private object[] _data;

        private const int IndexHandlerName = 0;
        private const int IndexScriptProcessor = 1;
        private const int IndexVersion = 2;

        private const int Size = 3;

        public PHPVersion()
        {
            _data = new object[Size];
            HandlerName = String.Empty;
            ScriptProcessor = String.Empty;
            Version = String.Empty;
        }

        public PHPVersion(string name, string scriptProcessor, string version)
        {
            _data = new object[Size];
            HandlerName = name;
            ScriptProcessor = scriptProcessor;
            Version = version;
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
