//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Web.Administration;

namespace Web.Management.PHP.Setup
{

    public static class InstallUtil
    {

        public static void AddUIModuleProvider(string name, string type)
        {
            using (var mgr = new ServerManager())
            {

                // First register the Module Provider  
                var adminConfig = mgr.GetAdministrationConfiguration();

                var moduleProvidersSection = adminConfig.GetSection("moduleProviders");
                var moduleProviders = moduleProvidersSection.GetCollection();
                if (FindByAttribute(moduleProviders, "name", name) == null)
                {
                    var moduleProvider = moduleProviders.CreateElement();
                    moduleProvider.SetAttributeValue("name", name);
                    moduleProvider.SetAttributeValue("type", type);
                    moduleProviders.Add(moduleProvider);
                }

                // Now register it so that all Sites have access to this module 
                var modulesSection = adminConfig.GetSection("modules");
                var modules = modulesSection.GetCollection();
                if (FindByAttribute(modules, "name", name) == null)
                {
                    var module = modules.CreateElement();
                    module.SetAttributeValue("name", name);
                    modules.Add(module);
                }

                mgr.CommitChanges();
            }
        }

        /// <summary> 
        /// Helper method to find an element based on an attribute 
        /// </summary> 
        private static ConfigurationElement FindByAttribute(IEnumerable<ConfigurationElement> collection, string attributeName, string value)
        {
            return collection.FirstOrDefault(element => String.Equals((string) element.GetAttribute(attributeName).Value, value, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary> 
        /// Removes the specified UI Module by name 
        /// </summary> 
        public static void RemoveUIModuleProvider(string name)
        {
            using (var mgr = new ServerManager())
            {
                // First remove it from the sites 
                var adminConfig = mgr.GetAdministrationConfiguration();
                var modulesSection = adminConfig.GetSection("modules");
                var modules = modulesSection.GetCollection();
                var module = FindByAttribute(modules, "name", name);
                if (module != null)
                {
                    modules.Remove(module);
                }

                // now remove the ModuleProvider 
                var moduleProvidersSection = adminConfig.GetSection("moduleProviders");
                var moduleProviders = moduleProvidersSection.GetCollection();
                var moduleProvider = FindByAttribute(moduleProviders, "name", name);
                if (moduleProvider != null)
                {
                    moduleProviders.Remove(moduleProvider);
                }

                mgr.CommitChanges();
            }
        }
    }
}
