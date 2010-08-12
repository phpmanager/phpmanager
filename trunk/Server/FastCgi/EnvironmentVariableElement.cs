//------------------------------------------------------------------------------
// <copyright file="EnvironmentVariableElement.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.Web.Administration;

namespace Web.Management.PHP.FastCgi
{

    internal class EnvironmentVariableElement : ConfigurationElement
    {

        public string Name
        {
            get
            {
                return (string)base["name"];
            }
            set
            {
                base["name"] = value;
            }
        }

        public string Value
        {
            get
            {
                return (string)base["value"];
            }
            set
            {
                base["value"] = value;
            }
        }

    }
}
