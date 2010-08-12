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
using System.Collections.Generic;
using Microsoft.Web.Management.Client;
using Microsoft.Web.Management.Server;
using Web.Management.PHP.Config;

namespace Web.Management.PHP
{

    internal sealed class PHPModuleProxy : ModuleServiceProxy
    {

        internal void AddOrUpdateSettings(RemoteObjectCollection<PHPIniSetting> settings)
        {
            Invoke("AddOrUpdateSettings", settings.GetData());
        }

        internal string CreatePHPInfo()
        {
            return (string)Invoke("CreatePHPInfo");
        }

        internal ArrayList GetAllPHPVersions()
        {
            return (ArrayList)Invoke("GetAllPHPVersions");
        }

        internal PHPConfigInfo GetPHPConfigInfo()
        {
            object o = Invoke("GetPHPConfigInfo");
            PHPConfigInfo result = new PHPConfigInfo();
            result.SetData(o);
            return result;
        }

        internal string GetPHPIniPhysicalPath()
        {
            return (string)Invoke("GetPHPIniPhysicalPath");
        }

        internal object GetPHPIniSettings()
        {
            return Invoke("GetPHPIniSettings");
        }

        internal ArrayList GetSiteBindings()
        {
            return (ArrayList)Invoke("GetSiteBindings");
        }

        internal ArrayList GetSites()
        {
            return (ArrayList)Invoke("GetSites");
        }

        internal void RegisterPHPWithIIS(string path)
        {
            Invoke("RegisterPHPWithIIS", path);
        }

        internal void RemovePHPInfo(string filePath)
        {
            Invoke("RemovePHPInfo", filePath);
        }

        internal void RemoveSetting(PHPIniSetting setting)
        {
            Invoke("RemovePHPIniSetting", setting.GetData());
        }

        internal void SelectPHPVersion(string name)
        {
            Invoke("SelectPHPVersion", name);
        }

        internal void UpdatePHPExtensions(RemoteObjectCollection<PHPIniExtension> extensions)
        {
            Invoke("UpdatePHPExtensions", extensions.GetData());
        }
    }
}