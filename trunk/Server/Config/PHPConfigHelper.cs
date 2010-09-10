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
using Microsoft.Web.Administration;
using Microsoft.Web.Management.Server;
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

        private bool ApplyRecommendedDefaultDocument(bool commitChanges)
        {
            bool updateHappened = false;

            FileElement fileElement = _defaultDocumentCollection["index.php"];
            if (fileElement == null)
            {
                fileElement = _defaultDocumentCollection.CreateElement();
                fileElement.Value = "index.php";
                _defaultDocumentCollection.AddAt(0, fileElement);
                updateHappened = true;
            }
            else if (_defaultDocumentCollection.IndexOf(fileElement) > 0)
            {
                CopyIneritedDefaultDocs();
                MoveIndexPhpOnTop();
                updateHappened = true;
            }

            if (commitChanges && updateHappened)
            {
                _managementUnit.Update();
            }

            return updateHappened;
        }

        private void ApplyRecommendedFastCgiSettings()
        {
            // Set the handler mapping resource type to "File or Folder"
            _currentPHPHandler.ResourceType = ResourceType.Either;

            // Set PHP_FCGI_MAX_REQUESTS to be equal to instanceMaxRequests
            EnvironmentVariableElement envVariableElement = _currentFastCgiApplication.EnvironmentVariables["PHP_FCGI_MAX_REQUESTS"];
            if (envVariableElement == null)
            {
                _currentFastCgiApplication.EnvironmentVariables.Add("PHP_FCGI_MAX_REQUESTS", _currentFastCgiApplication.InstanceMaxRequests.ToString());
            }
            else
            {
                envVariableElement.Value = _currentFastCgiApplication.InstanceMaxRequests.ToString();
            }

            // Set PHPRC
            envVariableElement = _currentFastCgiApplication.EnvironmentVariables["PHPRC"];
            string monitorChangesTo = PHPIniFilePath;
            if (envVariableElement == null)
            {
                _currentFastCgiApplication.EnvironmentVariables.Add("PHPRC", PHPDirectory);
            }
            else
            {
                // If PHPRC points to a valid directory with php.ini then use the same path for 
                // FastCgi monitorChangesTo setting
                string path = Path.Combine(envVariableElement.Value, "php.ini");
                if (File.Exists(path))
                {
                    monitorChangesTo = path;
                }
                else
                {
                    // Otherwise set PHPRC to point to current PHP directory
                    envVariableElement.Value = PHPDirectory;
                }
            }

            // If monitorChangesTo is supported then set it
            if (_currentFastCgiApplication.MonitorChangesToExists())
            {
                _currentFastCgiApplication.MonitorChangesTo = monitorChangesTo;
            }

            _managementUnit.Update();
        }

        private void ApplyRecommendedPHPIniSettings(bool isNewRegistration)
        {
            PHPIniFile file = new PHPIniFile(PHPIniFilePath);
            file.Parse();

            string handlerName = _currentPHPHandler.Name;

            // Set the recommended php.ini settings
            List<PHPIniSetting> settings = new List<PHPIniSetting>();

            // Set extension directory path
            string value = EnsureTrailingBackslash(Path.Combine(PHPDirectory, "ext"));
            settings.Add(new PHPIniSetting("extension_dir", value, "PHP"));

            // Set log_errors
            settings.Add(new PHPIniSetting("log_errors", "On", "PHP"));

            // Set error_log path if it is not set correctly
            PHPIniSetting currentSetting = file.GetSetting("error_log");
            if (currentSetting == null || !IsAbsoluteFilePath(currentSetting.Value, true))
            {
                value = Path.Combine(Environment.ExpandEnvironmentVariables(@"%WINDIR%\Temp\"), handlerName + "_errors.log");
                settings.Add(new PHPIniSetting("error_log", value, "PHP"));
            }

            // Set session path if it is not set correctly
            currentSetting = file.GetSetting("session.save_path");
            if (currentSetting == null || !IsAbsoluteFilePath(currentSetting.Value, false))
            {
                value = Environment.ExpandEnvironmentVariables(@"%WINDIR%\Temp\");
                settings.Add(new PHPIniSetting("session.save_path", value, "Session"));
            }

            // Set cgi.force_redirect
            settings.Add(new PHPIniSetting("cgi.force_redirect", "0", "PHP"));
            
            // Set cgi.fix_pathinfo
            settings.Add(new PHPIniSetting("cgi.fix_pathinfo", "1", "PHP"));

            // Enable fastcgi impersonation
            settings.Add(new PHPIniSetting("fastcgi.impersonate", "1", "PHP"));

            if (isNewRegistration)
            {
                // Disable fastcgi logging
                settings.Add(new PHPIniSetting("fastcgi.logging", "0", "PHP"));

                // Set maximum script execution time
                settings.Add(new PHPIniSetting("max_execution_time", "300", "PHP"));

                // Turn off display errors
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
            }

            file.AddOrUpdateSettings(settings);
            file.Save(PHPIniFilePath);
        }

        public void ApplyRecommendedSettings()
        {
            // Check if PHP is not registered
            if (_currentFastCgiApplication == null || _currentPHPHandler == null)
            {
                throw new InvalidOperationException("Cannot apply recommended settings because PHP is not registered properly");
            }

            ApplyRecommendedDefaultDocument(false /* Do not commit the changes yet. Next function will commit the changes anyway */);
            ApplyRecommendedFastCgiSettings();
            ApplyRecommendedPHPIniSettings(false /* This is an update to an existing PHP registration */);
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

        private static string EnsureTrailingBackslash(string str)
        {            
            if (!str.EndsWith(@"\", StringComparison.Ordinal))
            {
                return str + @"\";
            }
            
            return str;
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
                    if (File.Exists(handler.ScriptProcessor))
                    {
                        result.Add(new string[] { handler.Name, handler.ScriptProcessor, GetPHPExecutableVersion(handler.ScriptProcessor) });
                    }
                }
            }

            return result;
        }

        public PHPConfigInfo GetPHPConfigInfo()
        {
            // Check if PHP is not registered
            if (_currentFastCgiApplication == null || _currentPHPHandler == null)
            {
                return null;
            }

            PHPConfigInfo configInfo = new PHPConfigInfo();
            configInfo.HandlerName = _currentPHPHandler.Name;
            configInfo.ScriptProcessor = _currentPHPHandler.ScriptProcessor;
            configInfo.Version = GetPHPExecutableVersion(_currentPHPHandler.ScriptProcessor);

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
                configInfo.ErrorLog = setting.Value;
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
            string phpDirectory = Path.GetDirectoryName(Environment.ExpandEnvironmentVariables(_currentPHPHandler.ScriptProcessor));
            return EnsureTrailingBackslash(phpDirectory);
        }

        private static string GetPHPExecutableVersion(string phpexePath)
        {
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(phpexePath);
            return fileVersionInfo.ProductVersion;
        }

        private string GetPHPIniFilePath()
        {
            // If PHPRC environment variable is set then use the path specified there.
            // Otherwise use the same path as where PHP executable is located.
            string directoryPath = String.Empty;
            EnvironmentVariableElement phpRcElement = _currentFastCgiApplication.EnvironmentVariables["PHPRC"];
            if (phpRcElement != null && !String.IsNullOrEmpty(phpRcElement.Value))
            {
                directoryPath = phpRcElement.Value;
            }
            else
            {
                directoryPath = Path.GetDirectoryName(_currentPHPHandler.ScriptProcessor);
            }

            string phpIniPath = Path.Combine(directoryPath, "php.ini");

            if (File.Exists(phpIniPath))
            {
                return phpIniPath;
            }

            return String.Empty;
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



            // Find the currently active PHP handler and FastCGI application
            HandlerElement handler = _handlersCollection.GetActiveHandler("*.php");
            if (handler != null)
            {
                string executable = handler.ScriptProcessor;
                
                ApplicationElement fastCgiApplication = _fastCgiApplicationCollection.GetApplication(executable, "");
                if (fastCgiApplication != null)
                {
                    _currentPHPHandler = handler;
                    _currentFastCgiApplication = fastCgiApplication;
                    _phpIniFilePath = GetPHPIniFilePath();
                    _phpDirectory = GetPHPDirectory();
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

        private FileElement MoveIndexPhpOnTop()
        {
            FileElement fileElement = _defaultDocumentCollection["index.php"];
            Debug.Assert(fileElement != null);

            _defaultDocumentCollection.Remove(fileElement);
            return _defaultDocumentCollection.AddCopyAt(0, fileElement);
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
            
            // Check for existence of php.ini file. If it does not exist then copy php.ini-recommended
            // or php.ini-production to it
            string phpIniFilePath = Path.Combine(phpDir, "php.ini");
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
                handlerElement.Verb = "*";
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
            iisUpdateHappened = ApplyRecommendedDefaultDocument(false /* do not commit the changes yet */) || iisUpdateHappened;

            if (iisUpdateHappened)
            {
                _managementUnit.Update();
                // We need to call Initialize() again to set references to current handler and 
                // fastcgi application and to avoid the read-only exception from IIS config
                Initialize();
            }

            // Make recommended changes to existing iis configuration. This is the case
            // when either FastCGI application or a handler mapping or both existed for the
            // specified php-cgi.exe executable.
            if (!isNewFastCgi || !isNewHandler)
            {
                ApplyRecommendedFastCgiSettings();
            }

            // Make the recommended changes to php.ini file
            ApplyRecommendedPHPIniSettings(true /* this is a new registration of PHP */);
        }

        public void SelectPHPHandler(string name)
        {
            // PHP is not registered properly so we don't attempt to do anything.
            if (_currentFastCgiApplication == null || _currentPHPHandler == null)
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

        public RemoteObjectCollection<PHPConfigIssue> ValidateConfiguration()
        {
            // Check if PHP is not registered
            if (_currentFastCgiApplication == null || _currentPHPHandler == null)
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

            // Check if index.php is set as a default document
            FileElement fileElement = _defaultDocumentCollection["index.php"];
            if (fileElement == null)
            {
                PHPConfigIssue configIssue = new PHPConfigIssue("Default document",
                                                                _defaultDocumentCollection[0].Value,
                                                                "index.php",
                                                                "ConfigIssueDefaultDocumentNotSet",
                                                                "ConfigIssueDefaultDocumentRecommend");
                configIssues.Add(configIssue);
            }
            else if (_defaultDocumentCollection.IndexOf(fileElement) > 0)
            {
                PHPConfigIssue configIssue = new PHPConfigIssue("Default document",
                                                                _defaultDocumentCollection[0].Value,
                                                                "index.php",
                                                                "ConfigIssueDefaultDocumentNotFirst",
                                                                "ConfigIssueDefaultDocumentRecommend");
                configIssues.Add(configIssue);
            }

            #region FastCGI configuration

            // Check if handler mapping is configured for "File or Folder"
            if (_currentPHPHandler.ResourceType != ResourceType.Either)
            {
                PHPConfigIssue configIssue = new PHPConfigIssue("resourceType",
                                                                _currentPHPHandler.ResourceType.ToString(),
                                                                ResourceType.Either.ToString(),
                                                                "ConfigIssueResourceTypeIncorrect",
                                                                "ConfigIssueResourceTypeRecommend");
                configIssues.Add(configIssue);
            }

            // Check if PHP_FCGI_MAX_REQUESTS is set and is bigger than instanceMaxRequests
            EnvironmentVariableElement envVariableElement = _currentFastCgiApplication.EnvironmentVariables["PHP_FCGI_MAX_REQUESTS"];
            if (envVariableElement == null)
            {
                PHPConfigIssue configIssue = new PHPConfigIssue("PHP_FCGI_MAX_REQUESTS",
                                                                String.Empty,
                                                                _currentFastCgiApplication.InstanceMaxRequests.ToString(),
                                                                "ConfigIssuePHPMaxRequestsNotSet",
                                                                "ConfigIssuePHPMaxRequestsRecommend");
                configIssues.Add(configIssue);
            }
            else
            {
                long maxRequests;
                if (!Int64.TryParse(envVariableElement.Value, out maxRequests) ||
                    (maxRequests < _currentFastCgiApplication.InstanceMaxRequests))
                {
                    PHPConfigIssue configIssue = new PHPConfigIssue("PHP_FCGI_MAX_REQUESTS",
                                                                    envVariableElement.Value,
                                                                    _currentFastCgiApplication.InstanceMaxRequests.ToString(),
                                                                    "ConfigIssuePHPMaxRequestsIncorrect",
                                                                    "ConfigIssuePHPMaxRequestsRecommend");
                    configIssues.Add(configIssue);
                }
            }

            // Check if PHPRC is set and points to a directory that has php.ini file
            envVariableElement = _currentFastCgiApplication.EnvironmentVariables["PHPRC"];
            if (envVariableElement == null)
            {
                PHPConfigIssue configIssue = new PHPConfigIssue("PHPRC", 
                                                                String.Empty,
                                                                PHPDirectory,
                                                                "ConfigIssuePHPRCNotSet",
                                                                "ConfigIssuePHPRCRecommend");
                configIssues.Add(configIssue);
            }
            else
            {
                string path = Path.Combine(envVariableElement.Value, "php.ini");
                if (!File.Exists(path))
                {
                    PHPConfigIssue configIssue = new PHPConfigIssue("PHPRC",
                                                                    envVariableElement.Value,
                                                                    PHPDirectory,
                                                                    "ConfigIssuePHPRCFileNotExists",
                                                                    "ConfigIssuePHPRCRecommend");
                    configIssues.Add(configIssue);                   
                }
            }

            // Check if monitorChangesTo setting is supported and is set correctly
            if (_currentFastCgiApplication.MonitorChangesToExists())
            {
                string path = _currentFastCgiApplication.MonitorChangesTo;
                if (String.IsNullOrEmpty(path))
                {
                    PHPConfigIssue configIssue = new PHPConfigIssue("monitorChangesTo",
                                                                    String.Empty,
                                                                    PHPIniFilePath,
                                                                    "ConfigIssueMonitorChangesNotSet",
                                                                    "ConfigIssueMonitorChangesRecommend");
                    configIssues.Add(configIssue);
                }
                else if (!String.Equals(PHPIniFilePath, path, StringComparison.OrdinalIgnoreCase))
                {
                    PHPConfigIssue configIssue = new PHPConfigIssue("monitorChangesTo",
                                                                    path,
                                                                    PHPIniFilePath,
                                                                    "ConfigIssueMonitorChangesIncorrect",
                                                                    "ConfigIssueMonitorChangesRecommend");
                    configIssues.Add(configIssue);
                }
            }

            #endregion

            #region PHP configuration

            // Check if extention_dir is set to an absolute path
            PHPIniSetting setting = file.GetSetting("extension_dir");
            string expectedValue = EnsureTrailingBackslash(Path.Combine(PHPDirectory, "ext"));
            if (setting == null || String.IsNullOrEmpty(setting.Value))
            {
                PHPConfigIssue configIssue = new PHPConfigIssue("extension_dir",
                                                                String.Empty,
                                                                expectedValue,
                                                                "ConfigIssueExtensionDirNotSet",
                                                                "ConfigIssueExtensionDirRecommend");
                configIssues.Add(configIssue);
            }
            else
            {
                if (!String.Equals(setting.Value, expectedValue, StringComparison.OrdinalIgnoreCase))
                {
                    PHPConfigIssue configIssue = new PHPConfigIssue("extension_dir",
                                                                    setting.Value,
                                                                    expectedValue,
                                                                    "ConfigIssueExtensionDirIncorrect",
                                                                    "ConfigIssueExtensionDirRecommend");
                    configIssues.Add(configIssue);
                }
            }

            // Check if log_errors is set to On
            setting = file.GetSetting("log_errors");
            if (setting == null || String.IsNullOrEmpty(setting.Value))
            {
                PHPConfigIssue configIssue = new PHPConfigIssue("log_errors",
                                                                String.Empty,
                                                                "On",
                                                                "ConfigIssueLogErrorsNotSet",
                                                                "ConfigIssueLogErrorsRecommend");
                configIssues.Add(configIssue);
            }
            else if (!String.Equals(setting.Value, "On", StringComparison.OrdinalIgnoreCase))
            {
                PHPConfigIssue configIssue = new PHPConfigIssue("log_errors",
                                                                setting.Value,
                                                                "On",
                                                                "ConfigIssueLogErrorsNotCorrect",
                                                                "ConfigIssueLogErrorsRecommend");
                configIssues.Add(configIssue);
            }

            // Check if error_log is set to an absolute path and that path exists
            setting = file.GetSetting("error_log");
            expectedValue = Path.Combine(Environment.ExpandEnvironmentVariables(@"%WINDIR%\Temp\"), _currentPHPHandler.Name + "_errors.log");
            if (setting == null || String.IsNullOrEmpty(setting.Value))
            {
                PHPConfigIssue configIssue = new PHPConfigIssue("error_log",
                                                                String.Empty,
                                                                expectedValue,
                                                                "ConfigIssueErrorLogNotSet",
                                                                "ConfigIssueErrorLogRecommend");
                configIssues.Add(configIssue);
            }
            else if (!IsAbsoluteFilePath(setting.Value, true /* this is supposed to be a file */))
            {
                PHPConfigIssue configIssue = new PHPConfigIssue("error_log",
                                                                setting.Value,
                                                                expectedValue,
                                                                "ConfigIssueErrorLogNotCorrect",
                                                                "ConfigIssueErrorLogRecommend");
                configIssues.Add(configIssue);
            }

            // Check if session path is set to an absolute path and that path exists
            setting = file.GetSetting("session.save_path");
            expectedValue = Environment.ExpandEnvironmentVariables(@"%WINDIR%\Temp\");
            if (setting == null || String.IsNullOrEmpty(setting.Value))
            {
                PHPConfigIssue configIssue = new PHPConfigIssue("session.save_path",
                                                                String.Empty,
                                                                expectedValue,
                                                                "ConfigIssueSessionPathNotSet",
                                                                "ConfigIssueSessionPathRecommend");
                configIssues.Add(configIssue);
            }
            else if (!IsAbsoluteFilePath(setting.Value, false /* this is supposed to be a directory */))
            {
                PHPConfigIssue configIssue = new PHPConfigIssue("session.save_path",
                                                                setting.Value,
                                                                expectedValue,
                                                                "ConfigIssueSessionPathNotCorrect",
                                                                "ConfigIssueSessionPathRecommend");
                configIssues.Add(configIssue);
            }

            // Check if cgi.force_redirect is set correctly
            setting = file.GetSetting("cgi.force_redirect");
            if (setting == null || String.IsNullOrEmpty(setting.Value))
            {
                PHPConfigIssue configIssue = new PHPConfigIssue("cgi.force_redirect",
                                                                String.Empty,
                                                                "0",
                                                                "ConfigIssueCgiForceRedirectNotSet",
                                                                "ConfigIssueCgiForceRedirectRecommend");
                configIssues.Add(configIssue);
            }
            else if (!String.Equals(setting.Value, "0", StringComparison.OrdinalIgnoreCase))
            {
                PHPConfigIssue configIssue = new PHPConfigIssue("cgi.force_redirect",
                                                                setting.Value,
                                                                "0",
                                                                "ConfigIssueCgiForceRedirectNotCorrect",
                                                                "ConfigIssueCgiForceRedirectRecommend");
                configIssues.Add(configIssue);
            }

            // Check if cgi.fix_pathinfo is set correctly
            setting = file.GetSetting("cgi.fix_pathinfo");
            if (setting == null || String.IsNullOrEmpty(setting.Value))
            {
                PHPConfigIssue configIssue = new PHPConfigIssue("cgi.fix_pathinfo",
                                                                String.Empty,
                                                                "1",
                                                                "ConfigIssueCgiPathInfoNotSet",
                                                                "ConfigIssueCgiPathInfoRecommend");
                configIssues.Add(configIssue);
            }
            else if (!String.Equals(setting.Value, "1", StringComparison.OrdinalIgnoreCase))
            {
                PHPConfigIssue configIssue = new PHPConfigIssue("cgi.fix_pathinfo",
                                                                setting.Value,
                                                                "1",
                                                                "ConfigIssueCgiPathInfoNotCorrect",
                                                                "ConfigIssueCgiPathInfoRecommend");
                configIssues.Add(configIssue);
            }

            // Check if fastcgi impersonation is turned on
            setting = file.GetSetting("fastcgi.impersonate");
            if (setting == null || String.IsNullOrEmpty(setting.Value))
            {
                PHPConfigIssue configIssue = new PHPConfigIssue("fastcgi.impersonate",
                                                                String.Empty,
                                                                "1",
                                                                "ConfigIssueFastCgiImpersonateNotSet",
                                                                "ConfigIssueFastCgiImpersonateRecommend");
                configIssues.Add(configIssue);
            }
            else if (!String.Equals(setting.Value, "1", StringComparison.OrdinalIgnoreCase))
            {
                PHPConfigIssue configIssue = new PHPConfigIssue("fastcgi.impersonate",
                                                                setting.Value,
                                                                "1",
                                                                "ConfigIssueFastCgiImpersonateNotCorrect",
                                                                "ConfigIssueFastCgiImpersonateRecommend");
                configIssues.Add(configIssue);
            }

            #endregion

            return configIssues;
        }

    }
}