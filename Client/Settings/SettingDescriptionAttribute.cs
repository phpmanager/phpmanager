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

namespace Web.Management.PHP.Settings
{
    
    [AttributeUsage(AttributeTargets.Property| AttributeTargets.Event|AttributeTargets.Class)]
    internal sealed class SettingDescriptionAttribute : DescriptionAttribute {

        private bool _replaced;

        internal SettingDescriptionAttribute(string description)
            : base(description) {
        }

        public override string Description {
            get {
                if (!_replaced) {
                    _replaced = true;
                    DescriptionValue = Resources.ResourceManager.GetString(base.Description);
                }

                return base.Description;
            }
        }

    }
}
