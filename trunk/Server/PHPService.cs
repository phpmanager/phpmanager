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
using Web.Management.PHP.Handlers;

namespace Web.Management.PHP
{

    internal sealed class PHPService : ModuleService
    {

#if DEBUG
        private const bool Passthrough = false;
#else
        private const bool Passthrough = true;
#endif

        [ModuleServiceMethod(PassThrough = Passthrough)]
        public string AddExtension(string extensionPath)
        {
            EnsureServerConnection();
            string result = null;

            try
            {
                PHPConfigHelper configHelper = new PHPConfigHelper(ManagementUnit);
                result = configHelper.AddExtension(extensionPath);
            }
            catch (FileNotFoundException)
            {
                RaiseException("ErrorPHPIniNotFound");
            }
            catch (InvalidOperationException)
            {
                RaiseException("ErrorExtensionFileAlreadyExists");
            }
            catch (Exception)
            {
                RaiseException("ErrorCannotAddExtension");
            }

            return result;
        }

        [ModuleServiceMethod(PassThrough = Passthrough)]
        public void AddOrUpdateSettings(object settingsData)
        {
            EnsureServerConnection();

            RemoteObjectCollection<PHPIniSetting> settings = new RemoteObjectCollection<PHPIniSetting>();
            ((IRemoteObject)settings).SetData(settingsData);

            try
            {
                PHPConfigHelper configHelper = new PHPConfigHelper(ManagementUnit);
                configHelper.AddOrUpdatePHPIniSettings(settings);
            }
            catch (FileNotFoundException)
            {
                RaiseException("ErrorPHPIniNotFound");
            }
        }

        [ModuleServiceMethod(PassThrough = Passthrough)]
        public void ApplyRecommendedSettings(ArrayList configIssueIndexes)
        {
            EnsureServerConnection();

            try
            {
                PHPConfigHelper configHelper = new PHPConfigHelper(ManagementUnit);
                configHelper.ApplyRecommendedSettings(configIssueIndexes);
            }
            catch (FileNotFoundException)
            {
                RaiseException("ErrorPHPIniNotFound");
            }
        }

        [ModuleServiceMethod(PassThrough = Passthrough)]
        public bool CheckForLocalPHPHandler(string siteName, string configurationPath)
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

            Configuration config = ManagementUnit.ReadOnlyServerManager.GetWebConfiguration(siteName, configurationPath);
            HandlersSection handlersSection = (HandlersSection)config.GetSection("system.webServer/handlers", typeof(HandlersSection));
            HandlersCollection handlersCollection = handlersSection.Handlers;
            foreach (HandlerElement handlerElement in handlersCollection)
            {
                if (handlerElement.IsLocallyStored)
                {
                    return true;
                }
            }

            return false;
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

            string navigatorPath = siteName;
            if (ManagementUnit.ConfigurationPath.PathType != ConfigurationPathType.Server)
            {
                navigatorPath = ManagementUnit.ConfigurationPath.GetEffectiveConfigurationPath(ManagementUnit.Scope);
            }

            ManagementContentNavigator navigator = ManagementContentNavigator.Create(ManagementUnit);
            if (!navigator.MoveToPath(navigatorPath))
            {
                throw new InvalidOperationException();
            }

            string randomString = Path.GetRandomFileName();
            string fileName = String.Format("{0}.php", Path.GetFileNameWithoutExtension(randomString));
            string filePath = Path.Combine(Environment.ExpandEnvironmentVariables(navigator.PhysicalPath), fileName);

            try
            {
                File.WriteAllText(filePath, @"<?php phpinfo(); ?>");
            }
            catch (UnauthorizedAccessException)
            {
                RaiseException("ErrorCannotCreatePHPInfo");
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
            ArrayList versions = null;

            try
            {
                PHPConfigHelper configHelper = new PHPConfigHelper(ManagementUnit);
                versions = configHelper.GetAllPHPVersions();
            }
            catch (FileNotFoundException)
            {
                RaiseException("ErrorPHPIniNotFound");
            }

            return versions;
        }

        [ModuleServiceMethod(PassThrough = Passthrough)]
        public object GetConfigIssues()
        {
            RemoteObjectCollection<PHPConfigIssue> configIssues = null;
            try
            {
                PHPConfigHelper configHelper = new PHPConfigHelper(ManagementUnit);
                configIssues = configHelper.ValidateConfiguration();
            }
            catch (FileNotFoundException)
            {
                RaiseException("ErrorPHPIniNotFound");
            }

            return (configIssues != null) ? configIssues.GetData() : null;
        }

        [ModuleServiceMethod(PassThrough = Passthrough)]
        public object GetPHPConfigInfo()
        {
            EnsureServerOrSiteConnection();

            PHPConfigInfo result = null;
            try
            {
                PHPConfigHelper configHelper = new PHPConfigHelper(ManagementUnit);
                result = configHelper.GetPHPConfigInfo();
            }
            catch (FileNotFoundException)
            {
                RaiseException("ErrorPHPIniNotFound");
            }

            return (result != null) ? result.GetData() : null;
        }

        [ModuleServiceMethod(PassThrough = Passthrough)]
        public object GetPHPIniPhysicalPath()
        {
            if (!ManagementUnit.Context.IsLocalConnection)
            {
                return null;
            }
            
            string phpiniPath = null;

            try
            {
                PHPConfigHelper configHelper = new PHPConfigHelper(ManagementUnit);
                phpiniPath = configHelper.PHPIniFilePath;
            }
            catch(FileNotFoundException)
            {
                RaiseException("ErrorPHPIniNotFound");
            }

            return phpiniPath;
        }

        [ModuleServiceMethod(PassThrough = Passthrough)]
        public object GetPHPIniSettings()
        {
            EnsureServerOrSiteConnection();

            PHPIniFile file = null;
            try
            {
                PHPConfigHelper configHelper = new PHPConfigHelper(ManagementUnit);
                file = configHelper.GetPHPIniFile();
            }
            catch (FileNotFoundException)
            {
                RaiseException("ErrorPHPIniNotFound");
            }

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
                PHPConfigHelper configHelper = new PHPConfigHelper(ManagementUnit);
                configHelper.RegisterPHPWithIIS(phpExePath);
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

            PHPIniSetting setting = new PHPIniSetting();
            setting.SetData(settingData);

            try
            {
                PHPConfigHelper configHelper = new PHPConfigHelper(ManagementUnit);
                configHelper.RemovePHPIniSetting(setting);
            }
            catch (FileNotFoundException)
            {
                RaiseException("ErrorPHPIniNotFound");
            }
        }

        [ModuleServiceMethod(PassThrough = Passthrough)]
        public void SelectPHPVersion(string name)
        {
            EnsureServerOrSiteConnection();

            try
            {
                PHPConfigHelper configHelper = new PHPConfigHelper(ManagementUnit);
                configHelper.SelectPHPHandler(name);
            }
            catch (FileLoadException)
            {
                RaiseException("ErrorSomeHandlersLocked");
            }
            catch (FileNotFoundException)
            {
                RaiseException("ErrorPHPIniNotFound");
            }
        }

        [ModuleServiceMethod(PassThrough = Passthrough)]
        public void UpdateExtensions(object extensionsData)
        {
            EnsureServerConnection();

            RemoteObjectCollection<PHPIniExtension> extensions = new RemoteObjectCollection<PHPIniExtension>();
            ((IRemoteObject)extensions).SetData(extensionsData);

            try
            {
                PHPConfigHelper configHelper = new PHPConfigHelper(ManagementUnit);
                configHelper.UpdateExtensions(extensions);
            }
            catch (FileNotFoundException)
            {
                RaiseException("ErrorPHPIniNotFound");
            }
        }

    }
}
