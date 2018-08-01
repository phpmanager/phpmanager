//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System;

namespace Web.Management.PHP.Settings
{

    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Event|AttributeTargets.Class)]
    internal sealed class SettingDisplayNameAttribute : Attribute {

        private bool _replaced;
        private readonly string _configPropertyName;
        private string _friendlyName;

        internal SettingDisplayNameAttribute(string friendlyName) {
            _friendlyName = friendlyName;
        }

        internal SettingDisplayNameAttribute(string friendlyName, string configPropertyName) {
            _friendlyName = friendlyName;
            _configPropertyName = configPropertyName;
        }

        public string ConfigPropertyName {
            get {
                return _configPropertyName;
            }
        }

        public string FriendlyName {
            get {
                if (!_replaced) {
                    _replaced = true;
                    _friendlyName = Resources.ResourceManager.GetString(_friendlyName);
                }

                return _friendlyName;
            }
        }
    }
}
