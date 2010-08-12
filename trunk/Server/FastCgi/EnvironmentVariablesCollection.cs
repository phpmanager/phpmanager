//------------------------------------------------------------------------------
// <copyright file="EnvironmentVariablesCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.Web.Administration;

namespace Web.Management.PHP.FastCgi
{

    internal class EnvironmentVariablesCollection : ConfigurationElementCollectionBase<EnvironmentVariableElement>
    {

        public new EnvironmentVariableElement this[string name]
        {
            get
            {
                for (int i = 0; (i < this.Count); i = (i + 1))
                {
                    EnvironmentVariableElement element = base[i];
                    if ((string.Equals(element.Name, name, StringComparison.OrdinalIgnoreCase) == true))
                    {
                        return element;
                    }
                }
                return null;
            }
        }

        public EnvironmentVariableElement Add(string name, string value)
        {
            EnvironmentVariableElement element = this.CreateElement();
            element.Name = name;
            element.Value = value;

            return base.Add(element);
        }

    }
}
