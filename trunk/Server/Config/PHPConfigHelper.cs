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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Web.Administration;
using Microsoft.Web.Management.Server;
using Microsoft.Win32;
using Web.Management.PHP.DefaultDocument;
using Web.Management.PHP.FastCgi;
using Web.Management.PHP.Handlers;

namespace Web.Management.PHP.Config
{   

    /// <summary>
    /// Provides functions to register PHP with IIS and to manage IIS and PHP configuration.
    /// </summary>
    internal sealed class PHPConfigHelper
    {

        private ManagementUnit _managementUnit;

        private ApplicationElement _currentFastCgiApplication;
        private HandlerElement _currentPHPHandler;
        private HandlersCollection _handlersCollection;
        private FastCgiApplicationCollection _fastCgiApplicationCollection;
        private FilesCollection _defaultDocumentCollection;
        private string _phpIniFilePath;
        private string _phpDirectory;
        private PHPRegistrationType _registrationType;

        public PHPConfigHelper(ManagementUnit mgmtUnit)
        {
            _managementUnit = mgmtUnit;
            Initialize();
        }

        public string PHPDirectory
        {
            get
            {
                return _phpDirectory;
            }
        }

        public string PHPIniFilePath
        {
            get
            {
                return _phpIniFilePath;
            }
        }

        private void ApplyRecommendedFastCgiSettings(ArrayList configIssueIndexes)
        {
            bool iisChangeHappened = false;

            foreach (int configIssueIndex in configIssueIndexes)
            {
                switch (configIssueIndex)
                {
                    case PHPConfigIssueIndex.DefaultDocument:
                        {
                            iisChangeHappened = ChangeDefaultDocument() || iisChangeHappened;
                            break;
                        }
                    case PHPConfigIssueIndex.ResourceType:
                        {
                            iisChangeHappened = ChangeResourceType() || iisChangeHappened;
                            break;
                        }
                    case PHPConfigIssueIndex.PHPMaxRequests:
                        {
                            iisChangeHappened = ChangePHPMaxRequests() || iisChangeHappened;
                            break;
                        }
                    case PHPConfigIssueIndex.PHPRC:
                        {
                            iisChangeHappened = ChangePHPRC() || iisChangeHappened;
                            break;
                        }
                    case PHPConfigIssueIndex.MonitorChangesTo:
                        {
                            iisChangeHappened = ChangeMonitorChanges() || iisChangeHappened;
                            break;
                        }
                }
            }
            if (iisChangeHappened)
            {
                _managementUnit.Update();
            }
        }

        private void ApplyRecommendedPHPIniSettings(ArrayList configIssueIndexes)
        {
            PHPIniFile file = new PHPIniFile(PHPIniFilePath);
            file.Parse();

            List<PHPIniSetting> settings = new List<PHPIniSetting>();

            foreach (int configIssueIndex in configIssueIndexes)
            {
                switch (configIssueIndex)
                {
                    case PHPConfigIssueIndex.ExtensionDir:
                        {
                            settings.Add(GetToApplyExtensionDir());
                            break;
                        }
                    case PHPConfigIssueIndex.LogErrors:
                        {
                            settings.Add(GetToApplyLogErrors());
                            break;
                        }
                    case PHPConfigIssueIndex.ErrorLog:
                        {
                            settings.Add(GetToApplyErrorLog(file));
                            break;
                        }
                    case PHPConfigIssueIndex.SessionPath:
                        {
                            settings.Add(GetToApplySessionPath(file));
                            break;
                        }
                    case PHPConfigIssueIndex.UploadDir:
                        {
                            settings.Add(GetToApplyUploadTmpDir(file));
                            break;
                        }
                    case PHPConfigIssueIndex.CgiForceRedirect:
                        {
                            settings.Add(GetToApplyCgiForceRedirect());
                            break;
                        }
                    case PHPConfigIssueIndex.CgiPathInfo:
                        {
                            settings.Add(GetToApplyCgiPathInfo());
                            break;
                        }
                    case PHPConfigIssueIndex.FastCgiImpersonation:
                        {
                            settings.Add(GetToApplyFastCgiImpersonate());
                            break;
                        }
                }
            }

            if (settings.Count > 0)
            {
                file.AddOrUpdateSettings(settings);
                file.Save(PHPIniFilePath);
            }
        }

        public void ApplyRecommendedSettings(ArrayList configIssueIndexes)
        {
            // Check if PHP is not registered
            if (!IsPHPRegistered())
            {
                throw new InvalidOperationException("Cannot apply recommended settings because PHP is not registered properly");
            }

            ApplyRecommendedFastCgiSettings(configIssueIndexes);
            ApplyRecommendedPHPIniSettings(configIssueIndexes);
        }

        private bool ChangeDefaultDocument()
        {
            bool changeHappened = false;

            FileElement fileElement = _defaultDocumentCollection["index.php"];
            if (fileElement == null)
            {
                fileElement = _defaultDocumentCollection.CreateElement();
                fileElement.Value = "index.php";
                _defaultDocumentCollection.AddAt(0, fileElement);
                changeHappened = true;
            }
            else if (_defaultDocumentCollection.IndexOf(fileElement) > 0)
            {
                CopyIneritedDefaultDocs();
                MoveIndexPhpOnTop();
                changeHappened = true;
            }

            return changeHappened;
        }

        private bool ChangeMonitorChanges()
        {
            bool changeHappened = false;

            // If monitorChangesTo is supported then set it
            if (_currentFastCgiApplication.MonitorChangesToExists())
            {
                _currentFastCgiApplication.MonitorChangesTo = PHPIniFilePath;
                changeHappened = true;
            }

            return changeHappened;
        }

        private bool ChangePHPMaxRequests()
        {
            // Set PHP_FCGI_MAX_REQUESTS to be equal to instanceMaxRequests
            EnvironmentVariableElement envVariableElement = _currentFastCgiApplication.EnvironmentVariables["PHP_FCGI_MAX_REQUESTS"];
            if (envVariableElement == null)
            {
                _currentFastCgiApplication.EnvironmentVariables.Add("PHP_FCGI_MAX_REQUESTS", _currentFastCgiApplication.InstanceMaxRequests.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                envVariableElement.Value = _currentFastCgiApplication.InstanceMaxRequests.ToString(CultureInfo.InvariantCulture);
            }

            return true;
        }

        private bool ChangePHPRC()
        {
            bool changeHappened = false;

            // Set PHPRC
            EnvironmentVariableElement envVariableElement = _currentFastCgiApplication.EnvironmentVariables["PHPRC"];
            string expectedValue = EnsureTrailingBackslash(Path.GetDirectoryName(PHPIniFilePath));
            if (envVariableElement == null)
            {
                _currentFastCgiApplication.EnvironmentVariables.Add("PHPRC", expectedValue);
                changeHappened = true;
            }
            else
            {
                // If PHPRC does not point to a valid directory with php.ini or php-cgi-fcgi.ini ...
                string pathPhpIni = Path.Combine(envVariableElement.Value, "php.ini");
                string pathPhpSapiIni = Path.Combine(envVariableElement.Value, "php-cgi-fcgi.ini");
                if (!File.Exists(pathPhpIni) && !File.Exists(pathPhpSapiIni))
                {
                    // ... then set it to the directory where php.ini is loaded from
                    envVariableElement.Value = expectedValue;
                    changeHappened = true;
                }
            }

            return changeHappened;
        }

        private bool ChangeResourceType()
        {
            bool changeHappened = false;

            if (_currentPHPHandler.ResourceType != ResourceType.Either)
            {
                _currentPHPHandler.ResourceType = ResourceType.Either;
                changeHappened = true;
            }

            return changeHappened;
        }

        private void CopyIneritedDefaultDocs()
        {
            if (_managementUnit.ConfigurationPath.PathType == ConfigurationPathType.Server)
            {
                return;
            }

            FileElement[] list = new FileElement[_defaultDocumentCollection.Count];
            ((ICollection)_defaultDocumentCollection).CopyTo(list, 0);

            _defaultDocumentCollection.Clear();

            foreach (FileElement f in list)
            {
                _defaultDocumentCollection.AddCopy(f);
            }
        }

        private void CopyInheritedHandlers()
        {
            if (_managementUnit.ConfigurationPath.PathType == ConfigurationPathType.Server)
            {
                return;
            }

            HandlerElement[] list = new HandlerElement[_handlersCollection.Count];
            ((ICollection)_handlersCollection).CopyTo(list, 0);

            _handlersCollection.Clear();

            foreach (HandlerElement handler in list)
            {
                _handlersCollection.AddCopy(handler);
            }
        }

        private static string DoubleQuotesWrap(string value)
        {
            return '"' + value + '"';
        }

        private static string EnsureTrailingBackslash(string str)
        {            
            if (!str.EndsWith(@"\", StringComparison.Ordinal))
            {
                return str + @"\";
            }
            
            return str;
        }

        private static Version ExtractVersion(string version_string)
        {
            Version result = null;

            Regex r = new Regex(@"^(?<version>\d+\.\d+\.\d+(?:\.\d+)?).*");
            Match m = r.Match(version_string);
            if (m.Success)
            {
                string version = r.Match(version_string).Result("${version}");
                result = new Version(version);
            }

            return result;
        }

        private static string GenerateHandlerName(HandlersCollection collection, string phpVersion)
        {
            string prefix = "php-" + phpVersion;
            string name = prefix;

            for (int i = 1; true; i++)
            {
                if (collection[name] != null)
                {
                    name = prefix + "_" + i.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    break;
                }
            }
            return name;
        }

        public ArrayList GetAllPHPVersions()
        {
            ArrayList result = new ArrayList();
            
            foreach (HandlerElement handler in _handlersCollection)
            {
                if (String.Equals(handler.Path, "*.php", StringComparison.OrdinalIgnoreCase))
                {
                    if (String.Equals(handler.Modules, "FastCgiModule", StringComparison.OrdinalIgnoreCase) &&
                        File.Exists(handler.Executable))
                    {
                        result.Add(new string[] { handler.Name, handler.Executable, GetPHPExecutableVersion(handler.Executable) });
                    }
                }
            }

            return result;
        }

        public PHPConfigInfo GetPHPConfigInfo()
        {
            PHPConfigInfo configInfo = new PHPConfigInfo();

            // If PHP is not registered properly then just return information about
            // how it registered.
            if (!IsPHPRegistered())
            {
                configInfo.RegistrationType = _registrationType;
                return configInfo;
            }

            configInfo.RegistrationType = _registrationType;
            configInfo.HandlerName = _currentPHPHandler.Name;
            configInfo.Executable = _currentPHPHandler.Executable;
            configInfo.Version = GetPHPExecutableVersion(_currentPHPHandler.Executable);

            if (String.IsNullOrEmpty(PHPIniFilePath))
            {
                throw new FileNotFoundException("php.ini file does not exist");
            }

            configInfo.PHPIniFilePath = PHPIniFilePath;

            PHPIniFile file = new PHPIniFile(PHPIniFilePath);
            file.Parse();

            PHPIniSetting setting = file.GetSetting("error_log");
            if (setting != null)
            {
                configInfo.ErrorLog = setting.TrimmedValue;
            }
            else
            {
                configInfo.ErrorLog = String.Empty;
            }

            configInfo.EnabledExtCount = file.GetEnabledExtensionsCount();
            configInfo.InstalledExtCount = file.Extensions.Count;

            ICollection issues = ValidateConfiguration(file);
            configInfo.IsConfigOptimal = (issues.Count == 0);

            return configInfo;
        }

        private string GetPHPDirectory()
        {
            string phpDirectory = Path.GetDirectoryName(Environment.ExpandEnvironmentVariables(_currentPHPHandler.Executable));
            return EnsureTrailingBackslash(phpDirectory);
        }

        private static string GetPHPExecutableVersion(string phpexePath)
        {
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(phpexePath);
            return fileVersionInfo.ProductVersion;
        }

        private string GetPHPIniFilePath()
        {
            // If PHPRC environment variable is set then use the path specified there,
            // Otherwise try to get the directory path from registry,
            // Otherwise use the same path as where PHP executable is located.
            string directoryPath = String.Empty;
            EnvironmentVariableElement phpRcElement = _currentFastCgiApplication.EnvironmentVariables["PHPRC"];
            if (phpRcElement != null && !String.IsNullOrEmpty(phpRcElement.Value))
            {
                directoryPath = phpRcElement.Value;
            }
            else
            {
                directoryPath = GetPHPIniPathFromRegistry(_currentPHPHandler.Executable);
                if (String.IsNullOrEmpty(directoryPath))
                {
                    directoryPath = Path.GetDirectoryName(_currentPHPHandler.Executable);
                }
            }

            string phpIniPath = Path.Combine(directoryPath, "php-cgi-fcgi.ini");

            if (File.Exists(phpIniPath))
            {
                return phpIniPath;
            }
            else
            {
                phpIniPath = Path.Combine(directoryPath, "php.ini");
                if (File.Exists(phpIniPath))
                {
                    return phpIniPath;
                }
            }

            return String.Empty;
        }

        private static string GetPHPIniPathFromRegistry(string executable)
        {
            // Getting the path to php.ini from registry in accordance to the documentation at
            // http://www.php.net/manual/en/configuration.file.php

            // First try to access the version as it is
            string versionAsIs = GetPHPExecutableVersion(executable);
            string phpIniDirectory = TryToGetIniFilePath(versionAsIs);
            if (phpIniDirectory != null)
            {
                return phpIniDirectory;
            }

            // Try to get the properly formatted version 
            // If cannot then bail out
            Version version = ExtractVersion(versionAsIs);
            if (version == null)
            {
                return null;
            }

            // Try to get the IniFilePath from [HKEY_LOCAL_MACHINE\SOFTWARE\PHP\x.y.z]
            phpIniDirectory = TryToGetIniFilePath(version.ToString(3));
            if (phpIniDirectory != null)
            {
                return phpIniDirectory;
            }

            // Try to get the IniFilePath from [HKEY_LOCAL_MACHINE\SOFTWARE\PHP\x.y]
            phpIniDirectory = TryToGetIniFilePath(version.ToString(2));
            if (phpIniDirectory != null)
            {
                return phpIniDirectory;
            }

            // Try to get the IniFilePath from [HKEY_LOCAL_MACHINE\SOFTWARE\PHP\x]
            phpIniDirectory = TryToGetIniFilePath(version.ToString(1));
            if (phpIniDirectory != null)
            {
                return phpIniDirectory;
            }

            // Try to get the IniFilePath from [HKEY_LOCAL_MACHINE\SOFTWARE\PHP]
            phpIniDirectory = TryToGetIniFilePath(null);
            if (phpIniDirectory != null)
            {
                return phpIniDirectory;
            }

            return null;
        }

        private static PHPIniSetting GetToApplyCgiForceRedirect()
        {
            return new PHPIniSetting("cgi.force_redirect", "0", "PHP");
        }

        private static PHPIniSetting GetToApplyCgiPathInfo()
        {
            return new PHPIniSetting("cgi.fix_pathinfo", "1", "PHP");
        }

        private PHPIniSetting GetToApplyErrorLog(PHPIniFile file)
        {
            string handlerName = _currentPHPHandler.Name;
            PHPIniSetting setting = file.GetSetting("error_log");
            if (setting == null || !IsAbsoluteFilePath(setting.TrimmedValue, true))
            {
                string value = Path.Combine(Environment.ExpandEnvironmentVariables(@"%WINDIR%\Temp\"), handlerName + "_errors.log");
                setting = new PHPIniSetting("error_log", DoubleQuotesWrap(value), "PHP");
            }

            return setting;
        }

        private PHPIniSetting GetToApplyExtensionDir()
        {
            string value = EnsureTrailingBackslash(Path.Combine(PHPDirectory, "ext"));
            return new PHPIniSetting("extension_dir", DoubleQuotesWrap(value), "PHP");
        }

        private static PHPIniSetting GetToApplyFastCgiImpersonate()
        {
            return new PHPIniSetting("fastcgi.impersonate", "1", "PHP");
        }

        private static PHPIniSetting GetToApplyLogErrors()
        {
            return new PHPIniSetting("log_errors", "On", "PHP");
        }

        private static PHPIniSetting GetToApplySessionPath(PHPIniFile file)
        {
            PHPIniSetting setting = file.GetSetting("session.save_path");
            if (setting == null || !IsAbsoluteFilePath(setting.TrimmedValue, false))
            {
                string value = Environment.ExpandEnvironmentVariables(@"%WINDIR%\Temp\");
                setting = new PHPIniSetting("session.save_path", DoubleQuotesWrap(value), "Session");
            }
            
            return setting;
        }

        private static PHPIniSetting GetToApplyUploadTmpDir(PHPIniFile file)
        {
            PHPIniSetting setting = file.GetSetting("upload_tmp_dir");
            if (setting == null || !IsAbsoluteFilePath(setting.TrimmedValue, false))
            {
                string value = Environment.ExpandEnvironmentVariables(@"%WINDIR%\Temp\");
                setting = new PHPIniSetting("upload_tmp_dir", DoubleQuotesWrap(value), "PHP");
            }

            return setting;
        }

        private void Initialize()
        {
            // Get the handlers collection
            ManagementConfiguration config = _managementUnit.Configuration;
            HandlersSection handlersSection = (HandlersSection)config.GetSection("system.webServer/handlers", typeof(HandlersSection));
            _handlersCollection = handlersSection.Handlers;

            // Get the Default document collection
            DefaultDocumentSection defaultDocumentSection = (DefaultDocumentSection)config.GetSection("system.webServer/defaultDocument", typeof(DefaultDocumentSection));
            _defaultDocumentCollection = defaultDocumentSection.Files;

            // Get the FastCgi application collection
            Configuration appHostConfig = _managementUnit.ServerManager.GetApplicationHostConfiguration();
            FastCgiSection fastCgiSection = (FastCgiSection)appHostConfig.GetSection("system.webServer/fastCgi", typeof(FastCgiSection));
            _fastCgiApplicationCollection = fastCgiSection.Applications;

            // Assume by default that PHP is not registered
            _registrationType = PHPRegistrationType.None;

            // Find the currently active PHP handler and FastCGI application
            HandlerElement handler = _handlersCollection.GetActiveHandler("*.php");
            if (handler != null)
            {
                if (String.Equals(handler.Modules, "FastCgiModule", StringComparison.OrdinalIgnoreCase))
                {
                    _registrationType = PHPRegistrationType.FastCgi;
                }
                else if (String.Equals(handler.Modules, "CgiModule", StringComparison.OrdinalIgnoreCase))
                {
                    _registrationType = PHPRegistrationType.Cgi;
                }
                else if (String.Equals(handler.Modules, "IsapiModule", StringComparison.OrdinalIgnoreCase))
                {
                    _registrationType = PHPRegistrationType.Isapi;
                }

                if (_registrationType == PHPRegistrationType.FastCgi)
                {
                    ApplicationElement fastCgiApplication = _fastCgiApplicationCollection.GetApplication(handler.Executable, handler.Arguments);
                    if (fastCgiApplication != null)
                    {
                        _currentPHPHandler = handler;
                        _currentFastCgiApplication = fastCgiApplication;
                        _phpIniFilePath = GetPHPIniFilePath();
                        _phpDirectory = GetPHPDirectory();
                    }
                    else
                    {
                        _registrationType = PHPRegistrationType.None;
                    }
                }
            }
        }

        private static bool IsAbsoluteFilePath(string path, bool isFile)
        {
            string directory = Environment.ExpandEnvironmentVariables(path);
            if (Path.IsPathRooted(path))
            {
                if (isFile)
                {
                    directory = Path.GetDirectoryName(directory);
                }

                return Directory.Exists(directory);
            }

            return false;
        }

        private bool IsPHPRegistered()
        {
            return (_registrationType == PHPRegistrationType.FastCgi);
        }

        private HandlerElement MakeHandlerActive(string handlerName)
        {
            // We have to look up the handler elements by name because we may be working
            // on the copy of the handlers collection.
            HandlerElement handlerElement = _handlersCollection[handlerName];
            HandlerElement activeHandlerElement = _handlersCollection[_currentPHPHandler.Name];
            Debug.Assert(handlerElement != null && activeHandlerElement != null);

            int activeHandlerIndex = _handlersCollection.IndexOf(activeHandlerElement);
            _handlersCollection.Remove(handlerElement);
            return _handlersCollection.AddCopyAt(activeHandlerIndex, handlerElement);
        }

        private void MakeRecommendedFastCgiChanges()
        {
            bool iisChangeHappened = false;

            iisChangeHappened = ChangeDefaultDocument() || iisChangeHappened;
            iisChangeHappened = ChangeResourceType() || iisChangeHappened;
            iisChangeHappened = ChangePHPMaxRequests() || iisChangeHappened;
            iisChangeHappened = ChangePHPRC() || iisChangeHappened;
            iisChangeHappened = ChangeMonitorChanges() || iisChangeHappened;

            if (iisChangeHappened)
            {
                _managementUnit.Update();
            }
        }

        private void MakeRecommendedPHPIniChanges()
        {
            PHPIniFile file = new PHPIniFile(PHPIniFilePath);
            file.Parse();

            // Set the recommended php.ini settings
            List<PHPIniSetting> settings = new List<PHPIniSetting>();

            settings.Add(GetToApplyExtensionDir());
            settings.Add(GetToApplyLogErrors());
            settings.Add(GetToApplyErrorLog(file));
            settings.Add(GetToApplySessionPath(file));
            settings.Add(GetToApplyUploadTmpDir(file));
            settings.Add(GetToApplyCgiForceRedirect());
            settings.Add(GetToApplyCgiPathInfo());
            settings.Add(GetToApplyFastCgiImpersonate());
            
            settings.Add(new PHPIniSetting("fastcgi.logging", "0", "PHP"));
            settings.Add(new PHPIniSetting("max_execution_time", "300", "PHP"));
            settings.Add(new PHPIniSetting("display_errors", "Off", "PHP"));

            // Enable the most common PHP extensions
            List<PHPIniExtension> extensions = new List<PHPIniExtension>();
            extensions.Add(new PHPIniExtension("php_curl.dll", true));
            extensions.Add(new PHPIniExtension("php_gd2.dll", true));
            extensions.Add(new PHPIniExtension("php_gettext.dll", true));
            extensions.Add(new PHPIniExtension("php_mysql.dll", true));
            extensions.Add(new PHPIniExtension("php_mysqli.dll", true));
            extensions.Add(new PHPIniExtension("php_mbstring.dll", true));
            extensions.Add(new PHPIniExtension("php_openssl.dll", true));
            extensions.Add(new PHPIniExtension("php_soap.dll", true));
            extensions.Add(new PHPIniExtension("php_xmlrpc.dll", true));

            file.UpdateExtensions(extensions);
            file.AddOrUpdateSettings(settings);
            file.Save(PHPIniFilePath);
        }

        private FileElement MoveIndexPhpOnTop()
        {
            FileElement fileElement = _defaultDocumentCollection["index.php"];
            Debug.Assert(fileElement != null);

            _defaultDocumentCollection.Remove(fileElement);
            return _defaultDocumentCollection.AddCopyAt(0, fileElement);
        }

        private static string PreparePHPIniFile(string phpDir)
        {
            // Check for existence of php.ini file. If it does not exist then copy php.ini-recommended
            // or php.ini-production to it
            string phpIniFilePath = Path.Combine(phpDir, "php-cgi-fcgi.ini");
            if (!File.Exists(phpIniFilePath))
            {
                phpIniFilePath = Path.Combine(phpDir, "php.ini");

                if (!File.Exists(phpIniFilePath))
                {
                    string phpIniRecommendedPath = Path.Combine(phpDir, "php.ini-recommended");
                    string phpIniProductionPath = Path.Combine(phpDir, "php.ini-production");
                    if (File.Exists(phpIniRecommendedPath))
                    {
                        File.Copy(phpIniRecommendedPath, phpIniFilePath);
                    }
                    else if (File.Exists(phpIniProductionPath))
                    {
                        File.Copy(phpIniProductionPath, phpIniFilePath);
                    }
                    else
                    {
                        throw new FileNotFoundException("php.ini and php.ini recommended do not exist in " + phpDir);
                    }
                }
            }
            return phpIniFilePath;
        }

        public void RegisterPHPWithIIS(string path)
        {
            string phpexePath = Environment.ExpandEnvironmentVariables(path);
            
            if (!String.Equals(Path.GetFileName(phpexePath), "php-cgi.exe", StringComparison.OrdinalIgnoreCase) &&
                !String.Equals(Path.GetFileName(phpexePath), "php.exe", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("The provided php executable path is invalid", phpexePath);
            }

            // Check for existence of php executable in the specified directory
            if (!File.Exists(phpexePath))
            {
                throw new FileNotFoundException("php-cgi.exe and php.exe do not exist");
            }

            // Check for existence of php extensions directory
            string phpDir = EnsureTrailingBackslash(Path.GetDirectoryName(phpexePath));
            string extDir = Path.Combine(phpDir, "ext");
            if (!Directory.Exists(extDir))
            {
                throw new DirectoryNotFoundException("ext directory does not exist in " + phpDir);
            }

            string phpIniFilePath = PreparePHPIniFile(phpDir);

            bool iisUpdateHappened = false;
            ApplicationElement fastCgiApplication = _fastCgiApplicationCollection.GetApplication(phpexePath, "");
            // Create a FastCGI application if it does not exist
            bool isNewFastCgi = false;
            if (fastCgiApplication == null)
            {
                fastCgiApplication = _fastCgiApplicationCollection.CreateElement();
                fastCgiApplication.FullPath = phpexePath;
                // monitorChangesTo may not exist if FastCGI update is not installed
                if (fastCgiApplication.MonitorChangesToExists())
                {
                    fastCgiApplication.MonitorChangesTo = phpIniFilePath;
                }
                fastCgiApplication.InstanceMaxRequests = 10000;
                fastCgiApplication.ActivityTimeout = 300;
                fastCgiApplication.RequestTimeout = 300;

                fastCgiApplication.EnvironmentVariables.Add("PHPRC", phpDir);
                fastCgiApplication.EnvironmentVariables.Add("PHP_FCGI_MAX_REQUESTS", "10000");

                _fastCgiApplicationCollection.Add(fastCgiApplication);
                isNewFastCgi = true;
                iisUpdateHappened = true;
            }
                
            // Check if handler mapping with this executable already exists
            HandlerElement handlerElement = _handlersCollection.GetHandler("*.php", phpexePath);
            // Create a handler mapping if it does not exist
            bool isNewHandler = false;
            if (handlerElement == null)
            {
                // Create a PHP file handler if it does not exist
                handlerElement = _handlersCollection.CreateElement();
                handlerElement.Name = GenerateHandlerName(_handlersCollection, GetPHPExecutableVersion(phpexePath));
                handlerElement.Modules = "FastCgiModule";
                handlerElement.RequireAccess = RequireAccess.Script;
                handlerElement.Verb = "GET,HEAD,POST";
                handlerElement.Path = "*.php";
                handlerElement.ScriptProcessor = phpexePath;
                handlerElement.ResourceType = ResourceType.Either;
                _handlersCollection.AddAt(0, handlerElement);
                isNewHandler = true;
                iisUpdateHappened = true;
            }
            else if (_currentPHPHandler != null && handlerElement != _currentPHPHandler)
            {
                // Move the existing PHP file handler mapping on top
                CopyInheritedHandlers();
                MakeHandlerActive(handlerElement.Name);
                iisUpdateHappened = true;
            }

            // Check if index.php is set as a default document and move it to the top of the list
            iisUpdateHappened = ChangeDefaultDocument() || iisUpdateHappened;

            if (iisUpdateHappened)
            {
                _managementUnit.Update();
                // We need to call Initialize() again to set references to current handler and 
                // fastcgi application and to avoid the read-only exception from IIS config
                Initialize();
            }

            // Make the recommended changes to php.ini file
            MakeRecommendedPHPIniChanges();

            // Make recommended changes to existing iis configuration. This is the case
            // when either FastCGI application or a handler mapping or both existed for the
            // specified php-cgi.exe executable.
            if (!isNewFastCgi || !isNewHandler)
            {
                MakeRecommendedFastCgiChanges();
            }
        }

        public void SelectPHPHandler(string name)
        {
            // PHP is not registered properly so we don't attempt to do anything.
            if (!IsPHPRegistered())
            {
                return;
            }

            HandlerElement handler = _handlersCollection[name];
            // If the handler is already an active PHP handler then no need to do anything.
            if (handler != null && handler != _currentPHPHandler)
            {
                CopyInheritedHandlers();
                MakeHandlerActive(name);
                _managementUnit.Update();
            }
        }

        private static string TryToGetIniFilePath(string version)
        {
            string subkey = "SOFTWARE\\PHP";
            if (!String.IsNullOrEmpty(version))
            {
                subkey = subkey + "\\" + version;
            }

            RegistryKey key = Registry.LocalMachine.OpenSubKey(subkey);
            if (key != null)
            {
                object value = key.GetValue("IniFilePath");
                if (value != null)
                {
                    return (string)value;
                }
            }

            return null;
        }

        private static PHPConfigIssue ValidateCgiForceRedirect(PHPIniFile file)
        {
            PHPConfigIssue configIssue = null;
            
            // Check if cgi.force_redirect is set correctly
            PHPIniSetting setting = file.GetSetting("cgi.force_redirect");
            if (setting == null || String.IsNullOrEmpty(setting.TrimmedValue))
            {
                configIssue = new PHPConfigIssue("cgi.force_redirect",
                                                                String.Empty,
                                                                "0",
                                                                "ConfigIssueCgiForceRedirectNotSet",
                                                                "ConfigIssueCgiForceRedirectRecommend",
                                                                PHPConfigIssueIndex.CgiForceRedirect);
            }
            else if (!String.Equals(setting.TrimmedValue, "0", StringComparison.OrdinalIgnoreCase))
            {
                configIssue = new PHPConfigIssue("cgi.force_redirect",
                                                                setting.TrimmedValue,
                                                                "0",
                                                                "ConfigIssueCgiForceRedirectNotCorrect",
                                                                "ConfigIssueCgiForceRedirectRecommend",
                                                                PHPConfigIssueIndex.CgiForceRedirect);
            }

            return configIssue;
        }

        private static PHPConfigIssue ValidateCgiPathInfo(PHPIniFile file)
        {
            PHPConfigIssue configIssue = null;
            
            // Check if cgi.fix_pathinfo is set correctly
            PHPIniSetting setting = file.GetSetting("cgi.fix_pathinfo");
            if (setting == null || String.IsNullOrEmpty(setting.TrimmedValue))
            {
                configIssue = new PHPConfigIssue("cgi.fix_pathinfo",
                                                                String.Empty,
                                                                "1",
                                                                "ConfigIssueCgiPathInfoNotSet",
                                                                "ConfigIssueCgiPathInfoRecommend",
                                                                PHPConfigIssueIndex.CgiPathInfo);
            }
            else if (!String.Equals(setting.TrimmedValue, "1", StringComparison.OrdinalIgnoreCase))
            {
                configIssue = new PHPConfigIssue("cgi.fix_pathinfo",
                                                                setting.TrimmedValue,
                                                                "1",
                                                                "ConfigIssueCgiPathInfoNotCorrect",
                                                                "ConfigIssueCgiPathInfoRecommend",
                                                                PHPConfigIssueIndex.CgiPathInfo);
            }

            return configIssue;
        }

        public RemoteObjectCollection<PHPConfigIssue> ValidateConfiguration()
        {
            // Check if PHP is not registered
            if (!IsPHPRegistered())
            {
                return null;
            }

            PHPIniFile file = new PHPIniFile(PHPIniFilePath);
            file.Parse();

            return ValidateConfiguration(file);
        }

        private RemoteObjectCollection<PHPConfigIssue> ValidateConfiguration(PHPIniFile file)
        {

            RemoteObjectCollection<PHPConfigIssue> configIssues = new RemoteObjectCollection<PHPConfigIssue>();
          
            // IIS and FastCGI settings
            PHPConfigIssue configIssue = ValidateDefaultDocument();
            if (configIssue != null)
            {
                configIssues.Add(configIssue);
            }
            
            configIssue = ValidateResourceType();
            if (configIssue != null)
            {
                configIssues.Add(configIssue);
            }

            configIssue = ValidatePHPMaxRequests();
            if (configIssue != null)
            {
                configIssues.Add(configIssue);
            }

            configIssue = ValidatePHPRC();
            if (configIssue != null)
            {
                configIssues.Add(configIssue);
            }

            configIssue = ValidateMonitorChanges();
            if (configIssue != null)
            {
                configIssues.Add(configIssue);
            }

            // PHP Settings
            configIssue = ValidateExtensionDir(file);
            if (configIssue != null)
            {
                configIssues.Add(configIssue);
            }

            configIssue = ValidateLogErrors(file);
            if (configIssue != null)
            {
                configIssues.Add(configIssue);
            }

            configIssue = ValidateErrorLog(file);
            if (configIssue != null)
            {
                configIssues.Add(configIssue);
            }

            configIssue = ValidateSessionPath(file);
            if (configIssue != null)
            {
                configIssues.Add(configIssue);
            }

            configIssue = ValidateUploadTmpDir(file);
            if (configIssue != null)
            {
                configIssues.Add(configIssue);
            }

            configIssue = ValidateCgiForceRedirect(file);
            if (configIssue != null)
            {
                configIssues.Add(configIssue);
            }

            configIssue = ValidateCgiPathInfo(file);
            if (configIssue != null)
            {
                configIssues.Add(configIssue);
            }

            configIssue = ValidateFastCgiImpersonate(file);
            if (configIssue != null)
            {
                configIssues.Add(configIssue);
            }

            return configIssues;
        }

        private PHPConfigIssue ValidateDefaultDocument()
        {
            PHPConfigIssue configIssue = null;
            // Check if index.php is set as a default document
            FileElement fileElement = _defaultDocumentCollection["index.php"];
            if (fileElement == null)
            {
                configIssue = new PHPConfigIssue("Default document",
                                                                _defaultDocumentCollection[0].Value,
                                                                "index.php",
                                                                "ConfigIssueDefaultDocumentNotSet",
                                                                "ConfigIssueDefaultDocumentRecommend",
                                                                PHPConfigIssueIndex.DefaultDocument);
            }
            else if (_defaultDocumentCollection.IndexOf(fileElement) > 0)
            {
                configIssue = new PHPConfigIssue("Default document",
                                                                _defaultDocumentCollection[0].Value,
                                                                "index.php",
                                                                "ConfigIssueDefaultDocumentNotFirst",
                                                                "ConfigIssueDefaultDocumentRecommend",
                                                                PHPConfigIssueIndex.DefaultDocument);
            }
            
            return configIssue;
        }

        private PHPConfigIssue ValidateErrorLog(PHPIniFile file)
        {
            PHPConfigIssue configIssue = null;

            // Check if error_log is set to an absolute path and that path exists
            PHPIniSetting setting = file.GetSetting("error_log");
            string expectedValue = Path.Combine(Environment.ExpandEnvironmentVariables(@"%WINDIR%\Temp\"), _currentPHPHandler.Name + "_errors.log");
            if (setting == null || String.IsNullOrEmpty(setting.TrimmedValue))
            {
                configIssue = new PHPConfigIssue("error_log",
                                                                String.Empty,
                                                                expectedValue,
                                                                "ConfigIssueErrorLogNotSet",
                                                                "ConfigIssueErrorLogRecommend",
                                                                PHPConfigIssueIndex.ErrorLog);
            }
            else if (!IsAbsoluteFilePath(setting.TrimmedValue, true /* this is supposed to be a file */))
            {
                configIssue = new PHPConfigIssue("error_log",
                                                                setting.TrimmedValue,
                                                                expectedValue,
                                                                "ConfigIssueErrorLogNotCorrect",
                                                                "ConfigIssueErrorLogRecommend",
                                                                PHPConfigIssueIndex.ErrorLog);
            }

            return configIssue;
        }

        private PHPConfigIssue ValidateExtensionDir(PHPIniFile file)
        {
            PHPConfigIssue configIssue = null;
            
            PHPIniSetting setting = file.GetSetting("extension_dir");
            string expectedValue = EnsureTrailingBackslash(Path.Combine(PHPDirectory, "ext"));
            if (setting == null || String.IsNullOrEmpty(setting.TrimmedValue))
            {
                configIssue = new PHPConfigIssue("extension_dir",
                                                                String.Empty,
                                                                expectedValue,
                                                                "ConfigIssueExtensionDirNotSet",
                                                                "ConfigIssueExtensionDirRecommend",
                                                                PHPConfigIssueIndex.ExtensionDir);
            }
            else
            {
                string currentValue = EnsureTrailingBackslash(setting.TrimmedValue);
                if (!String.Equals(currentValue, expectedValue, StringComparison.OrdinalIgnoreCase))
                {
                    configIssue = new PHPConfigIssue("extension_dir",
                                                                    setting.TrimmedValue,
                                                                    expectedValue,
                                                                    "ConfigIssueExtensionDirIncorrect",
                                                                    "ConfigIssueExtensionDirRecommend",
                                                                    PHPConfigIssueIndex.ExtensionDir);
                }
            }

            return configIssue;
        }

        private static PHPConfigIssue ValidateFastCgiImpersonate(PHPIniFile file)
        {
            PHPConfigIssue configIssue = null;
            
            // Check if fastcgi impersonation is turned on
            PHPIniSetting setting = file.GetSetting("fastcgi.impersonate");
            if (setting == null || String.IsNullOrEmpty(setting.TrimmedValue))
            {
                configIssue = new PHPConfigIssue("fastcgi.impersonate",
                                                                String.Empty,
                                                                "1",
                                                                "ConfigIssueFastCgiImpersonateNotSet",
                                                                "ConfigIssueFastCgiImpersonateRecommend",
                                                                PHPConfigIssueIndex.FastCgiImpersonation);
            }
            else if (!String.Equals(setting.TrimmedValue, "1", StringComparison.OrdinalIgnoreCase))
            {
                configIssue = new PHPConfigIssue("fastcgi.impersonate",
                                                                setting.TrimmedValue,
                                                                "1",
                                                                "ConfigIssueFastCgiImpersonateNotCorrect",
                                                                "ConfigIssueFastCgiImpersonateRecommend",
                                                                PHPConfigIssueIndex.FastCgiImpersonation);
            }

            return configIssue;
        }

        private static PHPConfigIssue ValidateLogErrors(PHPIniFile file)
        {
            PHPConfigIssue configIssue = null;

            // Check if log_errors is set to On
            PHPIniSetting setting = file.GetSetting("log_errors");
            if (setting == null || String.IsNullOrEmpty(setting.TrimmedValue))
            {
                configIssue = new PHPConfigIssue("log_errors",
                                                                String.Empty,
                                                                "On",
                                                                "ConfigIssueLogErrorsNotSet",
                                                                "ConfigIssueLogErrorsRecommend",
                                                                PHPConfigIssueIndex.LogErrors);
            }
            else if (!String.Equals(setting.TrimmedValue, "On", StringComparison.OrdinalIgnoreCase))
            {
                configIssue = new PHPConfigIssue("log_errors",
                                                                setting.TrimmedValue,
                                                                "On",
                                                                "ConfigIssueLogErrorsNotCorrect",
                                                                "ConfigIssueLogErrorsRecommend",
                                                                PHPConfigIssueIndex.LogErrors);
            }

            return configIssue;
        }

        private PHPConfigIssue ValidateMonitorChanges()
        {
            PHPConfigIssue configIssue = null;
            // Check if monitorChangesTo setting is supported and is set correctly
            if (_currentFastCgiApplication.MonitorChangesToExists())
            {
                string path = _currentFastCgiApplication.MonitorChangesTo;
                if (String.IsNullOrEmpty(path))
                {
                    configIssue = new PHPConfigIssue("monitorChangesTo",
                                                                    String.Empty,
                                                                    PHPIniFilePath,
                                                                    "ConfigIssueMonitorChangesNotSet",
                                                                    "ConfigIssueMonitorChangesRecommend",
                                                                    PHPConfigIssueIndex.MonitorChangesTo);
                }
                else if (!String.Equals(PHPIniFilePath, path, StringComparison.OrdinalIgnoreCase))
                {
                    configIssue = new PHPConfigIssue("monitorChangesTo",
                                                                    path,
                                                                    PHPIniFilePath,
                                                                    "ConfigIssueMonitorChangesIncorrect",
                                                                    "ConfigIssueMonitorChangesRecommend",
                                                                    PHPConfigIssueIndex.MonitorChangesTo);
                }
            }

            return configIssue;
        }

        private PHPConfigIssue ValidatePHPMaxRequests()
        {
            PHPConfigIssue configIssue = null;
            // Check if PHP_FCGI_MAX_REQUESTS is set and is bigger than instanceMaxRequests
            EnvironmentVariableElement envVariableElement = _currentFastCgiApplication.EnvironmentVariables["PHP_FCGI_MAX_REQUESTS"];
            if (envVariableElement == null)
            {
                configIssue = new PHPConfigIssue("PHP_FCGI_MAX_REQUESTS",
                                                                String.Empty,
                                                                _currentFastCgiApplication.InstanceMaxRequests.ToString(CultureInfo.InvariantCulture),
                                                                "ConfigIssuePHPMaxRequestsNotSet",
                                                                "ConfigIssuePHPMaxRequestsRecommend",
                                                                PHPConfigIssueIndex.PHPMaxRequests);
            }
            else
            {
                long maxRequests;
                if (!Int64.TryParse(envVariableElement.Value, out maxRequests) ||
                    (maxRequests < _currentFastCgiApplication.InstanceMaxRequests))
                {
                    configIssue = new PHPConfigIssue("PHP_FCGI_MAX_REQUESTS",
                                                                    envVariableElement.Value,
                                                                    _currentFastCgiApplication.InstanceMaxRequests.ToString(CultureInfo.InvariantCulture),
                                                                    "ConfigIssuePHPMaxRequestsIncorrect",
                                                                    "ConfigIssuePHPMaxRequestsRecommend",
                                                                    PHPConfigIssueIndex.PHPMaxRequests);
                }
            }

            return configIssue;
        }

        private PHPConfigIssue ValidatePHPRC()
        {
            PHPConfigIssue configIssue = null;
            // Check if PHPRC is set and points to a directory that has php.ini file
            EnvironmentVariableElement envVariableElement = _currentFastCgiApplication.EnvironmentVariables["PHPRC"];
            string expectedValue = EnsureTrailingBackslash(Path.GetDirectoryName(PHPIniFilePath));
            if (envVariableElement == null)
            {
                configIssue = new PHPConfigIssue("PHPRC", 
                                                                String.Empty,
                                                                expectedValue,
                                                                "ConfigIssuePHPRCNotSet",
                                                                "ConfigIssuePHPRCRecommend",
                                                                PHPConfigIssueIndex.PHPRC);
            }
            else
            {
                string pathPhpIni = Path.Combine(envVariableElement.Value, "php.ini");
                string pathPhpSapiIni = Path.Combine(envVariableElement.Value, "php-cgi-fcgi.ini");
                if (!File.Exists(pathPhpIni) && !File.Exists(pathPhpSapiIni))
                {
                    configIssue = new PHPConfigIssue("PHPRC",
                                                                    envVariableElement.Value,
                                                                    expectedValue,
                                                                    "ConfigIssuePHPRCFileNotExists",
                                                                    "ConfigIssuePHPRCRecommend",
                                                                    PHPConfigIssueIndex.PHPRC);
                }
            }

            return configIssue;
        }

        private PHPConfigIssue ValidateResourceType()
        {
            PHPConfigIssue configIssue = null;
            // Check if handler mapping is configured for "File or Folder"
            if (_currentPHPHandler.ResourceType != ResourceType.Either)
            {
                configIssue = new PHPConfigIssue("resourceType",
                                                                _currentPHPHandler.ResourceType.ToString(),
                                                                ResourceType.Either.ToString(),
                                                                "ConfigIssueResourceTypeIncorrect",
                                                                "ConfigIssueResourceTypeRecommend",
                                                                PHPConfigIssueIndex.ResourceType);
            }

            return configIssue;
        }

        private static PHPConfigIssue ValidateSessionPath(PHPIniFile file)
        {
            PHPConfigIssue configIssue = null;
            // Check if session path is set to an absolute path and that path exists
            PHPIniSetting setting = file.GetSetting("session.save_path");
            string expectedValue = Environment.ExpandEnvironmentVariables(@"%WINDIR%\Temp\");
            if (setting == null || String.IsNullOrEmpty(setting.TrimmedValue))
            {
                configIssue = new PHPConfigIssue("session.save_path",
                                                                String.Empty,
                                                                expectedValue,
                                                                "ConfigIssueSessionPathNotSet",
                                                                "ConfigIssueSessionPathRecommend",
                                                                PHPConfigIssueIndex.SessionPath);
            }
            else if (!IsAbsoluteFilePath(setting.TrimmedValue, false /* this is supposed to be a directory */))
            {
                configIssue = new PHPConfigIssue("session.save_path",
                                                                setting.TrimmedValue,
                                                                expectedValue,
                                                                "ConfigIssueSessionPathNotCorrect",
                                                                "ConfigIssueSessionPathRecommend",
                                                                PHPConfigIssueIndex.SessionPath);
            }

            return configIssue;
        }

        private static PHPConfigIssue ValidateUploadTmpDir(PHPIniFile file)
        {
            PHPConfigIssue configIssue = null;
            // Check if Upload dir is set to an absolute path and that path exists
            PHPIniSetting setting = file.GetSetting("upload_tmp_dir");
            string expectedValue = Environment.ExpandEnvironmentVariables(@"%WINDIR%\Temp\");
            if (setting == null || String.IsNullOrEmpty(setting.TrimmedValue))
            {
                configIssue = new PHPConfigIssue("upload_tmp_dir",
                                                                String.Empty,
                                                                expectedValue,
                                                                "ConfigIssueUploadDirNotSet",
                                                                "ConfigIssueUploadDirRecommend",
                                                                PHPConfigIssueIndex.UploadDir);
            }
            else if (!IsAbsoluteFilePath(setting.TrimmedValue, false /* this is supposed to be a directory */))
            {
                configIssue = new PHPConfigIssue("upload_tmp_dir",
                                                                setting.TrimmedValue,
                                                                expectedValue,
                                                                "ConfigIssueUploadDirNotCorrect",
                                                                "ConfigIssueUploadDirRecommend",
                                                                PHPConfigIssueIndex.UploadDir);
            }

            return configIssue;
        }
    }
}