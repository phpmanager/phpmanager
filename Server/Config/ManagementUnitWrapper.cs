//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Web.Administration;
using Microsoft.Web.Management.Server;

namespace Web.Management.PHP.Config
{

    internal sealed class ManagementUnitWrapper : IConfigurationWrapper
    {
        private readonly ManagementUnit _managementUnit;

        public ManagementUnitWrapper(ManagementUnit managementUnit)
        {
            _managementUnit = managementUnit;
        }

        #region IConfigurationWrapper Members

        public void CommitChanges()
        {
            _managementUnit.Update();
        }

        public Configuration GetAppHostConfiguration()
        {
            return _managementUnit.ServerManager.GetApplicationHostConfiguration();
        }

        public DefaultDocument.DefaultDocumentSection GetDefaultDocumentSection()
        {
            var config = _managementUnit.Configuration;
            return (DefaultDocument.DefaultDocumentSection)config.GetSection("system.webServer/defaultDocument", typeof(DefaultDocument.DefaultDocumentSection));
        }

        public Handlers.HandlersSection GetHandlersSection()
        {
            var config = _managementUnit.Configuration;
            return (Handlers.HandlersSection)config.GetSection("system.webServer/handlers", typeof(Handlers.HandlersSection));
        }

        public bool IsServerConfigurationPath()
        {
            return (_managementUnit.ConfigurationPath.PathType == ConfigurationPathType.Server);
        }

        #endregion
    }
}
