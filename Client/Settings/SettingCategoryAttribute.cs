//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System;
using System.ComponentModel;
using System.Globalization;

namespace Web.Management.PHP.Settings
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Event)]
    internal sealed class SettingCategoryAttribute : CategoryAttribute {

        internal SettingCategoryAttribute(string categoryValue)
            : base(categoryValue) {
        }

        protected override string GetLocalizedString(string value) {
            return Resources.ResourceManager.GetString(value, CultureInfo.CurrentUICulture);
        }
    }
}
