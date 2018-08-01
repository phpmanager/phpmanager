//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using Microsoft.Web.Administration;

namespace Web.Management.PHP.FastCgi
{

    public sealed class FastCgiSection : ConfigurationSection
    {
        private FastCgiApplicationCollection _applications;

        public FastCgiApplicationCollection Applications
        {
            get {
                return _applications ?? (_applications = (FastCgiApplicationCollection) GetCollection(typeof (FastCgiApplicationCollection)));
            }
        }
    }
}
