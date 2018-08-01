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

    public sealed class HandlersSection : ConfigurationSection
    {
        private HandlersCollection _handlers;

        public HandlersCollection Handlers
        {
            get
            {
                if (_handlers == null)
                {
                    _handlers = (HandlersCollection)GetCollection(typeof(HandlersCollection));
                }
                return _handlers;
            }
        }
    }
}
