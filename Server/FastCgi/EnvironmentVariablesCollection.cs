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

    public sealed class EnvironmentVariablesCollection : ConfigurationElementCollectionBase<EnvironmentVariableElement>
    {

        public new EnvironmentVariableElement this[string name]
        {
            get
            {
                for (var i = 0; (i < Count); i = (i + 1))
                {
                    var element = base[i];
                    if (string.Equals(element.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        return element;
                    }
                }
                return null;
            }
        }

        public EnvironmentVariableElement Add(string name, string value)
        {
            var element = CreateElement();
            element.Name = name;
            element.Value = value;

            return Add(element);
        }

    }
}
