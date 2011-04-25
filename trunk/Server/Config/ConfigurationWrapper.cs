//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Web.Administration;

namespace Web.Management.PHP.Config
{

    internal abstract class ConfigurationWrapper
    {

        public abstract void CommitChanges();

        public abstract Configuration GetAppHostConfiguration();

        public abstract DefaultDocument.DefaultDocumentSection GetDefaultDocumentSection();

        public abstract Handlers.HandlersSection GetHandlersSection();

        public abstract bool IsServerConfigurationPath();

    }
}
