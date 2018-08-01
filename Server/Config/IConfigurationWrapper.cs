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

    public interface IConfigurationWrapper
    {

        void CommitChanges();

        Configuration GetAppHostConfiguration();

        DefaultDocument.DefaultDocumentSection GetDefaultDocumentSection();

        Handlers.HandlersSection GetHandlersSection();

        bool IsServerConfigurationPath();

    }
}
