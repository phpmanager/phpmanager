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

    public sealed class FastCgiApplicationCollection : ConfigurationElementCollectionBase<ApplicationElement>
    {

        protected override ApplicationElement CreateNewElement(string elementTagName)
        {
            return new ApplicationElement();
        }

        public ApplicationElement GetApplication(string fullPath, string arguments)
        {
            for (var i = 0; i < Count; i++)
            {
                var element = base[i];
                if (String.Equals(fullPath, element.FullPath, StringComparison.OrdinalIgnoreCase) &&
                    String.Equals(arguments, element.Arguments, StringComparison.OrdinalIgnoreCase))
                {
                    return element;
                }
            }
            return null;
        }

    }
}
