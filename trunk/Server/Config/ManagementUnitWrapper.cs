using System;
using Microsoft.Web.Management.Server;
using Microsoft.Web.Administration;

namespace Web.Management.PHP.Config
{
    internal sealed class ManagementUnitWrapper: ConfigurationWrapper
    {
        private ManagementUnit _managementUnit;

        public ManagementUnitWrapper(ManagementUnit managementUnit)
        {
            _managementUnit = managementUnit;
        }

        public override Handlers.HandlersSection GetHandlersSection()
        {
            ManagementConfiguration config = _managementUnit.Configuration;
            return (Handlers.HandlersSection)config.GetSection("system.webServer/handlers", typeof(Handlers.HandlersSection));
        }

        public override DefaultDocument.DefaultDocumentSection GetDefaultDocumentSection()
        {
            ManagementConfiguration config = _managementUnit.Configuration;
            return (DefaultDocument.DefaultDocumentSection)config.GetSection("system.webServer/defaultDocument", typeof(DefaultDocument.DefaultDocumentSection));
        }

        public override bool IsServerConfigurationPath()
        {
            return (_managementUnit.ConfigurationPath.PathType == ConfigurationPathType.Server);
        }

        public override Configuration GetAppHostConfiguration()
        {
            return _managementUnit.ServerManager.GetApplicationHostConfiguration();
        }

        public override void CommitChanges()
        {
            _managementUnit.Update();
        }
    }
}
