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
        private string _siteName;
        private string _virtualPath;

        public ServerManagerWrapper(ServerManager serverManager, string siteName, string virtualPath)
        {
            _serverManager = serverManager;
            _siteName = siteName;
            _virtualPath = virtualPath;
        }

        public string SiteName
        {
            get
            {
                return _siteName;
            }
        }

        public string VirtualPath
        {
            get
            {
                return _virtualPath;
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

        private Configuration GetConfiguration()
        {
            if (String.IsNullOrEmpty(_siteName) && String.IsNullOrEmpty(_virtualPath))
            {
                return _serverManager.GetApplicationHostConfiguration();
            }

            string siteName = String.IsNullOrEmpty(_siteName) ? "Default Web Site" : _siteName;

            if (String.IsNullOrEmpty(_virtualPath))
            {
                return _serverManager.GetWebConfiguration(siteName);
            }
            else
            {
                return _serverManager.GetWebConfiguration(siteName, _virtualPath);
            }
        }

        public DefaultDocument.DefaultDocumentSection GetDefaultDocumentSection()
        {
            Configuration config = GetConfiguration();
            return (DefaultDocument.DefaultDocumentSection)config.GetSection("system.webServer/defaultDocument", typeof(DefaultDocument.DefaultDocumentSection));
        }

        public Handlers.HandlersSection GetHandlersSection()
        {
            Configuration config = GetConfiguration();
            return (Handlers.HandlersSection)config.GetSection("system.webServer/handlers", typeof(Handlers.HandlersSection));
        }

        public bool IsServerConfigurationPath()
        {
            return (String.IsNullOrEmpty(_siteName) && String.IsNullOrEmpty(_virtualPath));
        }

    }
}
