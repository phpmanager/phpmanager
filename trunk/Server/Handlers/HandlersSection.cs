//------------------------------------------------------------------------------
// <copyright file="HandlersSection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.Web.Administration;

namespace Web.Management.PHP.Handlers
{

    internal class HandlersSection : ConfigurationSection
    {
        private HandlersCollection _handlers;

        public HandlersCollection Handlers
        {
            get
            {
                if (this._handlers == null)
                {
                    this._handlers = (HandlersCollection)base.GetCollection(typeof(HandlersCollection));
                }
                return this._handlers;
            }
        }
    }
}
