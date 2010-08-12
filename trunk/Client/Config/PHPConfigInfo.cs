//------------------------------------------------------------------------------
// <copyright file="PHPConfigInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Web.Management.PHP.Config
{

    internal sealed class PHPConfigInfo : IRemoteObject
    {
        private object[] _data;
        private const int IndexHandlerName = 0;
        private const int IndexScriptProcessor = 1;
        private const int IndexVersion = 2;
        private const int Size = 3;

        public PHPConfigInfo()
        {
            _data = new object[Size];
        }

        public PHPConfigInfo(string name, string scriptProcessor, string version)
            : this()
        {
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