//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

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
