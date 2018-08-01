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
using System.Diagnostics;
using System.IO;
using Microsoft.Web.Management.Server;
using Web.Management.PHP.Config;

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
                var mgmtUnitWrapper = new ManagementUnitWrapper(ManagementUnit);
                var configHelper = new PHPConfigHelper(mgmtUnitWrapper);
                result = configHelper.AddExtension(extensionPath);
            }
            catch (FileNotFoundException)
            {
                RaiseException("ErrorPHPIniNotFound");
            }
            catch (IOException)
            {
                RaiseException("ErrorExtensionFileAlreadyExists");
            }
            catch (InvalidOperationException)
            {
                RaiseException("ErrorPHPIsNotRegistered");
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

            var settings = new RemoteObjectCollection<PHPIniSetting>();
            settings.SetData(settingsData);

            try
            {
                var mgmtUnitWrapper = new ManagementUnitWrapper(ManagementUnit);
                var configHelper = new PHPConfigHelper(mgmtUnitWrapper);
                configHelper.AddOrUpdatePHPIniSettings(settings);
            }
            catch (FileNotFoundException)
            {
                RaiseException("ErrorPHPIniNotFound");
            }
            catch (InvalidOperationException)
            {
                RaiseException("ErrorPHPIsNotRegistered");
            }
        }

        [ModuleServiceMethod(PassThrough = Passthrough)]
        public void ApplyRecommendedSettings(ArrayList configIssueIndexes)
        {
            EnsureServerConnection();

            try
            {
                var mgmtUnitWrapper = new ManagementUnitWrapper(ManagementUnit);
                var configHelper = new PHPConfigHelper(mgmtUnitWrapper);
                configHelper.ApplyRecommendedSettings(configIssueIndexes);
            }
            catch (FileNotFoundException)
            {
                RaiseException("ErrorPHPIniNotFound");
            }
            catch (InvalidOperationException)
            {
                RaiseException("ErrorPHPIsNotRegistered");
            }
        }

        [ModuleServiceMethod(PassThrough = Passthrough)]
        public bool CheckForLocalPHPHandler(string siteName, string virtualPath)
        {
            EnsureServerOrSiteConnection();

            if (String.IsNullOrEmpty(siteName))
            {
                throw new InvalidOperationException();
            }

            var site = ManagementUnit.ReadOnlyServerManager.Sites[siteName];
            if (site == null)
            {
                throw new InvalidOperationException();
            }

            var serverManagerWrapper = new ServerManagerWrapper(ManagementUnit.ReadOnlyServerManager, siteName, virtualPath);
            var configHelper = new PHPConfigHelper(serverManagerWrapper);

            var configInfo = configHelper.GetPHPConfigInfo();
            if (configInfo.RegistrationType != PHPRegistrationType.FastCgi)
            {
                throw new InvalidOperationException("PHP is not registered via FastCGI, hence there is no FastCGI handler defined");
            }

            return configInfo.HandlerIsLocal;
        }

        [ModuleServiceMethod(PassThrough = Passthrough)]
        public string CreatePHPInfo(string siteName)
        {
            EnsureServerOrSiteConnection();

            if (String.IsNullOrEmpty(siteName))
            {
                throw new InvalidOperationException();
            }

            var site = ManagementUnit.ReadOnlyServerManager.Sites[siteName];
            if (site == null)
            {
                throw new InvalidOperationException();
            }

            var navigatorPath = siteName;
            if (ManagementUnit.ConfigurationPath.PathType != ConfigurationPathType.Server)
            {
                navigatorPath = ManagementUnit.ConfigurationPath.GetEffectiveConfigurationPath(ManagementUnit.Scope);
            }

            var navigator = ManagementContentNavigator.Create(ManagementUnit);
            if (!navigator.MoveToPath(navigatorPath))
            {
                throw new InvalidOperationException();
            }

            var randomString = Path.GetRandomFileName();
            var fileName = String.Format("{0}.php", Path.GetFileNameWithoutExtension(randomString));
            var filePath = Path.Combine(Environment.ExpandEnvironmentVariables(navigator.PhysicalPath), fileName);

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
        public object GetAllPHPVersions()
        {
            EnsureServerOrSiteConnection();
            RemoteObjectCollection<PHPVersion> versions = null;

            try
            {
                var mgmtUnitWrapper = new ManagementUnitWrapper(ManagementUnit);
                var configHelper = new PHPConfigHelper(mgmtUnitWrapper);
                versions = configHelper.GetAllPHPVersions();
            }
            catch (FileNotFoundException)
            {
                RaiseException("ErrorPHPIniNotFound");
            }
            catch (InvalidOperationException)
            {
                RaiseException("ErrorPHPIsNotRegistered");
            }

            return (versions != null) ? versions.GetData() : null;
        }

        [ModuleServiceMethod(PassThrough = Passthrough)]
        public object GetConfigIssues()
        {
            RemoteObjectCollection<PHPConfigIssue> configIssues = null;
            try
            {
                var mgmtUnitWrapper = new ManagementUnitWrapper(ManagementUnit);
                var configHelper = new PHPConfigHelper(mgmtUnitWrapper);
                configIssues = configHelper.ValidateConfiguration();
            }
            catch (FileNotFoundException)
            {
                RaiseException("ErrorPHPIniNotFound");
            }
            catch (InvalidOperationException)
            {
                RaiseException("ErrorPHPIsNotRegistered");
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
                var mgmtUnitWrapper = new ManagementUnitWrapper(ManagementUnit);
                var configHelper = new PHPConfigHelper(mgmtUnitWrapper);
                result = configHelper.GetPHPConfigInfo();
            }
            catch (FileNotFoundException)
            {
                RaiseException("ErrorPHPIniNotFound");
            }
            catch (InvalidOperationException)
            {
                RaiseException("ErrorPHPIsNotRegistered");
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
                var mgmtUnitWrapper = new ManagementUnitWrapper(ManagementUnit);
                var configHelper = new PHPConfigHelper(mgmtUnitWrapper);
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
                var mgmtUnitWrapper = new ManagementUnitWrapper(ManagementUnit);
                var configHelper = new PHPConfigHelper(mgmtUnitWrapper);
                file = configHelper.GetPHPIniFile();
            }
            catch (FileNotFoundException)
            {
                RaiseException("ErrorPHPIniNotFound");
            }
            catch (InvalidOperationException)
            {
                RaiseException("ErrorPHPIsNotRegistered");
            }

            Debug.Assert(file != null);
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

            var site = ManagementUnit.ReadOnlyServerManager.Sites[siteName];
            if (site == null)
            {
                throw new InvalidOperationException();
            }

            var list = new ArrayList();
            foreach (var binding in site.Bindings)
            {
                var protocol = binding.Protocol;
                if (String.Equals(protocol, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(protocol, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                {
                    list.Add(new[] { protocol, binding.BindingInformation });
                }
            }

            return list;
        }

        [ModuleServiceMethod(PassThrough = Passthrough)]
        public ArrayList GetSites()
        {
            EnsureServerConnection();

            var siteCollection = ManagementUnit.ReadOnlyServerManager.Sites;
            var sites = new ArrayList(siteCollection.Count);
            foreach (var site in siteCollection)
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
                var mgmtUnitWrapper = new ManagementUnitWrapper(ManagementUnit);
                var configHelper = new PHPConfigHelper(mgmtUnitWrapper);
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

            var setting = new PHPIniSetting();
            setting.SetData(settingData);

            try
            {
                var mgmtUnitWrapper = new ManagementUnitWrapper(ManagementUnit);
                var configHelper = new PHPConfigHelper(mgmtUnitWrapper);
                configHelper.RemovePHPIniSetting(setting);
            }
            catch (FileNotFoundException)
            {
                RaiseException("ErrorPHPIniNotFound");
            }
            catch (InvalidOperationException)
            {
                RaiseException("ErrorPHPIsNotRegistered");
            }
        }

        [ModuleServiceMethod(PassThrough = Passthrough)]
        public void SelectPHPVersion(string name)
        {
            EnsureServerOrSiteConnection();

            try
            {
                var mgmtUnitWrapper = new ManagementUnitWrapper(ManagementUnit);
                var configHelper = new PHPConfigHelper(mgmtUnitWrapper);
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
            catch (InvalidOperationException)
            {
                RaiseException("ErrorPHPIsNotRegistered");
            }
        }

        [ModuleServiceMethod(PassThrough = Passthrough)]
        public void UpdateExtensions(object extensionsData)
        {
            EnsureServerConnection();

            var extensions = new RemoteObjectCollection<PHPIniExtension>();
            extensions.SetData(extensionsData);

            try
            {
                var mgmtUnitWrapper = new ManagementUnitWrapper(ManagementUnit);
                var configHelper = new PHPConfigHelper(mgmtUnitWrapper);
                configHelper.UpdateExtensions(extensions);
            }
            catch (FileNotFoundException)
            {
                RaiseException("ErrorPHPIniNotFound");
            }
            catch (InvalidOperationException)
            {
                RaiseException("ErrorPHPIsNotRegistered");
            }
        }

    }
}
