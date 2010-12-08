//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System;
using System.Collections;
using System.ComponentModel;
using Microsoft.Web.Management.Client;
using Microsoft.Web.Management.Client.Win32;

namespace Web.Management.PHP.Settings
{

    internal class SettingPropertyGridObject : PropertyGridObject
    {

        public SettingPropertyGridObject(ModulePropertiesPage page)
            : this(page, false)
        {
        }

        public SettingPropertyGridObject(ModulePropertiesPage page, bool readOnly)
            : base(page, readOnly)
        {
        }

        protected override System.ComponentModel.PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection props = base.GetProperties(attributes);

            Type thisType = GetType();
            ArrayList properties = new ArrayList();
            foreach (PropertyDescriptor prop in props)
            {
                AttributeCollection collection = prop.Attributes;

                SettingDisplayNameAttribute displayNameAttribute =
                    (SettingDisplayNameAttribute)(collection[typeof(SettingDisplayNameAttribute)]);

                if (displayNameAttribute != null)
                {
                    DisplayNameAttribute newDisplayNameAttribute =
                        GetDisplayNameAttribute(displayNameAttribute.FriendlyName, displayNameAttribute.ConfigPropertyName);

                    properties.Add(TypeDescriptor.CreateProperty(thisType, prop, newDisplayNameAttribute));
                }
                else
                {
                    properties.Add(prop);
                }
            }

            props = new PropertyDescriptorCollection((PropertyDescriptor[])properties.ToArray(typeof(PropertyDescriptor)));
            return props;
        }

    }
}
