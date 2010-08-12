//------------------------------------------------------------------------------
// <copyright file="FastCgiSection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

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
