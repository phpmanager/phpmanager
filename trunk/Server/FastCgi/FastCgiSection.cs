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

namespace Web.Management.PHP.FastCgi
{

    internal class FastCgiSection : ConfigurationSection
    {
        private FastCgiApplicationCollection _applications;

        public FastCgiSection() {
             }

        public FastCgiApplicationCollection Applications
        {
            get
            {
                if (this._applications == null)
                {
                    this._applications = (FastCgiApplicationCollection)base.GetCollection(typeof(FastCgiApplicationCollection));
                }
                return this._applications;
            }
        }
    }
}
