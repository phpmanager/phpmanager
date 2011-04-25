using System;
using Microsoft.Web.Administration;

namespace Web.Management.PHP.Config
{
    internal abstract class ConfigurationWrapper
    {
        public abstract Handlers.HandlersSection GetHandlersSection();
        public abstract DefaultDocument.DefaultDocumentSection GetDefaultDocumentSection();
        public abstract bool IsServerConfigurationPath();
        public abstract Configuration GetAppHostConfiguration();
        public abstract void CommitChanges();
    }
}
