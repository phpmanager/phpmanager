//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Microsoft.Web.Administration;

namespace Web.Management.PHP.Config
{

    public sealed class ServerManagerWrapper : IConfigurationWrapper
    {
        private ServerManager _serverManager;
        private string _configurationPath;

        public ServerManagerWrapper(ServerManager serverManager, string configurationPath)
        {
            _serverManager = serverManager;
            _configurationPath = configurationPath;
        }

        public string ConfigurationPath
        {
            get
            {
                return _configurationPath;
            }
        }

        public void CommitChanges()
        {
            _serverManager.CommitChanges();
        }

        public Configuration GetAppHostConfiguration()
        {
            return _serverManager.GetApplicationHostConfiguration();
        }

        private Configuration GetConfiguration(string configurationPath)
        {
            Configuration configuration = null;
            if (!String.IsNullOrEmpty(_configurationPath))
            {
                configuration = _serverManager.GetWebConfiguration(_configurationPath);
            }
            else
            {
                configuration = _serverManager.GetApplicationHostConfiguration();
            }

            return configuration;
        }

        public DefaultDocument.DefaultDocumentSection GetDefaultDocumentSection()
        {
            Configuration config = GetConfiguration(_configurationPath);
            return (DefaultDocument.DefaultDocumentSection)config.GetSection("system.webServer/defaultDocument", typeof(DefaultDocument.DefaultDocumentSection));
        }

        public Handlers.HandlersSection GetHandlersSection()
        {
            Configuration config = GetConfiguration(_configurationPath);
            return (Handlers.HandlersSection)config.GetSection("system.webServer/handlers", typeof(Handlers.HandlersSection));
        }

        public bool IsServerConfigurationPath()
        {
            return String.IsNullOrEmpty(_configurationPath);
        }

    }
}
