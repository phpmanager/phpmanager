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
using System.IO;
using Microsoft.Web.Administration;
using Microsoft.Web.Management.Server;
using Web.Management.PHP.Config;

namespace Web.Management.PHP
{

    internal sealed class PHPService : ModuleService
    {

#if DEBUG
        private const bool Passthrough = false;
#else
        private const bool PassThrough = true;
#endif

        [ModuleServiceMethod(PassThrough = Passthrough)]
        public void AddOrUpdateSettings(object settingsData)
        {
            EnsureServerConnection();

            PHPIniFile file = GetPHPIniFile();

            RemoteObjectCollection<PHPIniSetting> settings = new RemoteObjectCollection<PHPIniSetting>();
            ((IRemoteObject)settings).SetData(settingsData);

            file.AddOrUpdateSettings(settings);
            file.Save(file.FileName);
        }

        [ModuleServiceMethod(PassThrough = Passthrough)]
        public string CreatePHPInfo(string siteName)
        {
            EnsureServerOrSiteConnection();

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

            File.WriteAllText(filePath, @"<?php phpinfo(); ?>");

            return filePath;
        }

        private void EnsureServerConnection()
        {
            if (ManagementUnit.Scope != ManagementScope.Server)
            {
                throw new UnauthorizedAccessException();
            }
        }

        private void EnsureServerOrSiteConnection()
        {
            if (ManagementUnit.Scope != ManagementScope.Server && ManagementUnit.Scope != ManagementScope.Site)
            {
                throw new UnauthorizedAccessException();
            }
        }

        [ModuleServiceMethod(PassThrough = Passthrough)]
        public ArrayList GetAllPHPVersions()
        {
            EnsureServerOrSiteConnection();

            PHPConfigHelper phpConfig = new PHPConfigHelper(ManagementUnit);
            ArrayList versions = phpConfig.GetAllPHPVersions();
            return versions;
        }

        [ModuleServiceMethod(PassThrough = Passthrough)]
        public object GetPHPConfigInfo()
        {
            EnsureServerOrSiteConnection();

            PHPConfigHelper phpConfig = new PHPConfigHelper(ManagementUnit);
            PHPConfigInfo result = phpConfig.GetPHPConfigInfo();

            return (result != null) ? result.GetData() : null;
        }

        private PHPIniFile GetPHPIniFile()
        {
            PHPConfigHelper phpConfig = new PHPConfigHelper(ManagementUnit);
            string phpiniPath = phpConfig.GetPHPIniPath();

            if (String.IsNullOrEmpty(phpiniPath))
            {
                RaiseException("ErrorPHPIniNotFound");
            }

            PHPIniFile file = new PHPIniFile(phpiniPath);
            file.Parse();

            return file;
        }

        [ModuleServiceMethod(PassThrough = Passthrough)]
        public object GetPHPIniPhysicalPath()
        {
            if (!ManagementUnit.Context.IsLocalConnection)
            {
                return null;
            }

            PHPConfigHelper phpConfig = new PHPConfigHelper(ManagementUnit);
            string phpiniPath = phpConfig.GetPHPIniPath();

            if (String.IsNullOrEmpty(phpiniPath))
            {
                RaiseException("ErrorPHPIniNotFound");
            }

            return phpiniPath;
        }

        [ModuleServiceMethod(PassThrough = Passthrough)]
        public object GetPHPIniSettings()
        {
            EnsureServerOrSiteConnection();

            PHPIniFile file = GetPHPIniFile();

            return file.GetData();
        }

        [ModuleServiceMethod(PassThrough = Passthrough)]
        public ArrayList GetSiteBindings(string siteName)
        {
            EnsureServerOrSiteConnection();

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

        [ModuleServiceMethod(PassThrough = Passthrough)]
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

        [ModuleServiceMethod(PassThrough = Passthrough)]
        public void RegisterPHPWithIIS(string phpExePath)
        {
            EnsureServerConnection();

            try
            {
                PHPConfigHelper phpConfig = new PHPConfigHelper(ManagementUnit);
                phpConfig.RegisterPHPWithIIS(phpExePath);
            }
            catch (ArgumentException)
            {
                RaiseException("ErrorInvalidPHPExecutablePath");
            }
            catch (FileNotFoundException)
            {
                RaiseException("ErrorNoPHPFilesInDirectory");
            }
            catch (DirectoryNotFoundException)
            {
                RaiseException("ErrorNoExtDirectory");
            }
        }

        [ModuleServiceMethod(PassThrough = Passthrough)]
        public void RemovePHPInfo(string filePath)
        {
            EnsureServerOrSiteConnection();
            
            if (!File.Exists(filePath)) {
                return;
            }

            File.Delete(filePath);
        }

        [ModuleServiceMethod(PassThrough = Passthrough)]
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

        [ModuleServiceMethod(PassThrough = Passthrough)]
        public void SelectPHPVersion(string name)
        {
            EnsureServerOrSiteConnection();
            
            PHPConfigHelper phpConfig = new PHPConfigHelper(ManagementUnit);
            phpConfig.SelectPHPHandler(name);
        }

        [ModuleServiceMethod(PassThrough = Passthrough)]
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
