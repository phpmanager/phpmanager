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

namespace Web.Management.PHP.Setup
{

    public static class InstallUtil
    {

        public static void AddUIModuleProvider(string name, string type)
        {
            using (ServerManager mgr = new ServerManager())
            {

                // First register the Module Provider  
                Configuration adminConfig = mgr.GetAdministrationConfiguration();

                ConfigurationSection moduleProvidersSection = adminConfig.GetSection("moduleProviders");
                ConfigurationElementCollection moduleProviders = moduleProvidersSection.GetCollection();
                if (FindByAttribute(moduleProviders, "name", name) == null)
                {
                    ConfigurationElement moduleProvider = moduleProviders.CreateElement();
                    moduleProvider.SetAttributeValue("name", name);
                    moduleProvider.SetAttributeValue("type", type);
                    moduleProviders.Add(moduleProvider);
                }

                // Now register it so that all Sites have access to this module 
                ConfigurationSection modulesSection = adminConfig.GetSection("modules");
                ConfigurationElementCollection modules = modulesSection.GetCollection();
                if (FindByAttribute(modules, "name", name) == null)
                {
                    ConfigurationElement module = modules.CreateElement();
                    module.SetAttributeValue("name", name);
                    modules.Add(module);
                }

                mgr.CommitChanges();
            }
        }

        /// <summary> 
        /// Helper method to find an element based on an attribute 
        /// </summary> 
        private static ConfigurationElement FindByAttribute(ConfigurationElementCollection collection, string attributeName, string value)
        {
            foreach (ConfigurationElement element in collection)
            {
                if (String.Equals((string)element.GetAttribute(attributeName).Value, value, StringComparison.OrdinalIgnoreCase))
                {
                    return element;
                }
            }

            return null;
        }

        /// <summary> 
        /// Removes the specified UI Module by name 
        /// </summary> 
        public static void RemoveUIModuleProvider(string name)
        {
            using (ServerManager mgr = new ServerManager())
            {
                // First remove it from the sites 
                Configuration adminConfig = mgr.GetAdministrationConfiguration();
                ConfigurationSection modulesSection = adminConfig.GetSection("modules");
                ConfigurationElementCollection modules = modulesSection.GetCollection();
                ConfigurationElement module = FindByAttribute(modules, "name", name);
                if (module != null)
                {
                    modules.Remove(module);
                }

                // now remove the ModuleProvider 
                ConfigurationSection moduleProvidersSection = adminConfig.GetSection("moduleProviders");
                ConfigurationElementCollection moduleProviders = moduleProvidersSection.GetCollection();
                ConfigurationElement moduleProvider = FindByAttribute(moduleProviders, "name", name);
                if (moduleProvider != null)
                {
                    moduleProviders.Remove(moduleProvider);
                }

                mgr.CommitChanges();
            }
        }
    }
}
