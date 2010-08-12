//------------------------------------------------------------------------------
// <copyright file="PHPService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.Web.Administration;
using Microsoft.Web.Management.Server;
using Web.Management.PHP.Config;

namespace Web.Management.PHP
{

    internal class PHPService : ModuleService
    {

        [ModuleServiceMethod(PassThrough = true)]
        public void AddOrUpdateSettings(object settingsData)
        {
            EnsureServerConnection();

            PHPIniFile file = GetPHPIniFile();

            RemoteObjectCollection<PHPIniSetting> settings = new RemoteObjectCollection<PHPIniSetting>();
            ((IRemoteObject)settings).SetData(settingsData);

            file.AddOrUpdateSettings(settings);
            file.Save(file.FileName);
        }

        [ModuleServiceMethod(PassThrough = true)]
        public string CreatePHPInfo()
        {
            string siteName = ManagementUnit.ConfigurationPath.SiteName;
            if (String.IsNullOrEmpty(siteName))
            {
                throw new InvalidOperationException();
            }

            Site site = ManagementUnit.ReadOnlyServerManager.Sites[siteName];
            if (site == null)
            {
                throw new InvalidOperationException();
            }

            Application app = site.Applications["/"];
            if (app == null)
            {
                throw new InvalidOperationException();
            }

            VirtualDirectory vdir = app.VirtualDirectories["/"];
            if (vdir == null)
            {
                throw new InvalidOperationException();
            }

            string randomString = Path.GetRandomFileName();
            string fileName = randomString.Substring(0, randomString.IndexOf('.')) + ".php";
            string filePath = Path.Combine(Environment.ExpandEnvironmentVariables(vdir.PhysicalPath), fileName);

            try
            {
                File.WriteAllText(filePath, @"<?php phpinfo(); ?>");
            }
            catch (Exception)
            {
                RaiseException("CannotCreatePHPInfoFile");
            }

            return filePath;
        }

        private void EnsureServerConnection()
        {
            if (ManagementUnit.Scope != ManagementScope.Server)
            {
                throw new UnauthorizedAccessException();
            }
        }

        [ModuleServiceMethod(PassThrough = true)]
        public ArrayList GetAllPHPVersions()
        {
            PHPConfigHelper phpConfig = new PHPConfigHelper(ManagementUnit);
            ArrayList versions = phpConfig.GetAllPHPVersions();
            return versions;
        }

        [ModuleServiceMethod(PassThrough = true)]
        public object GetPHPConfigInfo()
        {
            PHPConfigHelper phpConfig = new PHPConfigHelper(ManagementUnit);
            PHPConfigInfo result = phpConfig.GetPHPConfigInfo();

            return (result != null) ? result.GetData() : null;
        }

        private PHPIniFile GetPHPIniFile()
        {
            string phpiniPath = GetPHPIniPath();

            if (!File.Exists(phpiniPath))
            {
                RaiseException("PHPIniFileNotFound");
            }

            PHPIniFile file = new PHPIniFile(phpiniPath);
            file.Parse();

            return file;
        }

        private string GetPHPIniPath()
        {
            PHPConfigHelper phpConfig = new PHPConfigHelper(ManagementUnit);
            string path = phpConfig.GetPHPIniDirectory();
            if (!String.IsNullOrEmpty(path))
            {
                path = Path.Combine(path, "php.ini");
            }

            return path;
        }

        [ModuleServiceMethod(PassThrough = true)]
        public object GetPHPIniPhysicalPath()
        {
            if (!ManagementUnit.Context.IsLocalConnection)
            {
                return null;
            }

            string phpIniPath = GetPHPIniPath();
            if (String.IsNullOrEmpty(phpIniPath))
            {
                RaiseException("PHPIniPathNotDefined");
            }
            if (!File.Exists(phpIniPath))
            {
                RaiseException("PHPIniFileDoesNotExist");
            }

            return phpIniPath;
        }

        [ModuleServiceMethod(PassThrough = true)]
        public object GetPHPIniSettings()
        {
            PHPIniFile file = GetPHPIniFile();

            return file.GetData();
        }

        [ModuleServiceMethod(PassThrough = true)]
        public ArrayList GetSiteBindings()
        {
            string siteName = ManagementUnit.ConfigurationPath.SiteName;
            if (String.IsNullOrEmpty(siteName))
            {
                throw new InvalidOperationException();
            }

            Site site = ManagementUnit.ReadOnlyServerManager.Sites[siteName];
            if (site == null)
            {
                throw new InvalidOperationException();
            }

            ArrayList list = new ArrayList();
            foreach (Binding binding in site.Bindings)
            {
                string protocol = binding.Protocol;
                if (String.Equals(protocol, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(protocol, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                {
                    list.Add(new string[] { protocol, binding.BindingInformation });
                }
            }

            return list;
        }

        [ModuleServiceMethod(PassThrough = true)]
        public ArrayList GetSites()
        {

            EnsureServerConnection();

            SiteCollection siteCollection = ManagementUnit.ReadOnlyServerManager.Sites;
            ArrayList sites = new ArrayList(siteCollection.Count);
            foreach (Site site in siteCollection)
            {
                sites.Add(site.Name);
            }

            return sites;
        }

        [ModuleServiceMethod(PassThrough = true)]
        public void RegisterPHPWithIIS(string phpDirectory)
        {
            EnsureServerConnection();

            try
            {
                PHPConfigHelper phpConfig = new PHPConfigHelper(ManagementUnit);
                phpConfig.RegisterPHPWithIIS(phpDirectory);
            }
            catch(ArgumentException)
            {
                RaiseException("NoPHPFilesInDirectory");
            }
        }

        [ModuleServiceMethod(PassThrough = true)]
        public void RemovePHPInfo(string filePath)
        {
            if (!File.Exists(filePath)) {
                return;
            }
            try
            {
                File.Delete(filePath);
            }
            catch (UnauthorizedAccessException)
            {
                RaiseException("Cannot delete a temporary file due to insufficient privileges");
            }
        }

        [ModuleServiceMethod(PassThrough = true)]
        public void RemovePHPIniSetting(object settingData)
        {
            EnsureServerConnection();

            PHPIniFile file = GetPHPIniFile();

            PHPIniSetting setting = new PHPIniSetting();
            setting.SetData(settingData);

            if (file.Remove(setting))
            {
                file.Save(file.FileName);
            }
        }

        [ModuleServiceMethod(PassThrough = true)]
        public void SelectPHPVersion(string name)
        {
            PHPConfigHelper phpConfig = new PHPConfigHelper(ManagementUnit);
            phpConfig.SelectPHPHandler(name);
        }

        [ModuleServiceMethod(PassThrough = true)]
        public void UpdatePHPExtensions(object extensionsData)
        {
            EnsureServerConnection();

            PHPIniFile file = GetPHPIniFile();

            RemoteObjectCollection<PHPIniExtension> extensions = new RemoteObjectCollection<PHPIniExtension>();
            ((IRemoteObject)extensions).SetData(extensionsData);

            file.UpdateExtensions(extensions);
            file.Save(file.FileName);
        }

    }
}
