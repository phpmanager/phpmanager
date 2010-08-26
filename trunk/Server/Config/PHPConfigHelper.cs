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
using System.IO;
using Microsoft.Web.Administration;
using Microsoft.Web.Management.Server;
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

        public PHPConfigHelper(ManagementUnit mgmtUnit)
        {
            _managementUnit = mgmtUnit;
            Initialize();
        }

        private void ApplyRecommendedPHPIniSettings()
        {
            string phpDirectory = Path.GetDirectoryName(_currentPHPHandler.ScriptProcessor);
            string handlerName = _currentPHPHandler.Name;
            string phpIniPath = GetPHPIniPath();

            PHPIniFile file = new PHPIniFile(phpIniPath);
            file.Parse();

            // Set the recommended php.ini settings
            List<PHPIniSetting> settings = new List<PHPIniSetting>();

            // Set extension directory path
            string value = Path.Combine(phpDirectory, "ext");
            settings.Add(new PHPIniSetting("extension_dir", value, "PHP"));

            // Set log_errors
            settings.Add(new PHPIniSetting("log_errors", "On", "PHP"));

            // Set error_log path
            value = Path.Combine(Environment.ExpandEnvironmentVariables(@"%WINDIR%\Temp\"), handlerName + "_errors.log");
            settings.Add(new PHPIniSetting("error_log", value, "PHP"));

            // Set session path
            value = Environment.ExpandEnvironmentVariables(@"%WINDIR%\Temp\");
            settings.Add(new PHPIniSetting("session.save_path", value, "Session"));

            // Set cgi.force_redirect
            settings.Add(new PHPIniSetting("cgi.force_redirect", "0", "PHP"));
            
            // Set cgi.fix_pathinfo
            settings.Add(new PHPIniSetting("cgi.fix_pathinfo", "1", "PHP"));

            // Enable fastcgi impersonation
            settings.Add(new PHPIniSetting("fastcgi.impersonate ", "1", "PHP"));
            
            // Disable fastcgi logging
            settings.Add(new PHPIniSetting("fastcgi.logging", "0", "PHP"));

            // Set maximum script execution time
            settings.Add(new PHPIniSetting("max_execution_time", "300", "PHP"));

            // Turn off display errors
            settings.Add(new PHPIniSetting("display_errors", "Off", "PHP"));
            file.AddOrUpdateSettings(settings);

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

            file.Save(phpIniPath);
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

        private static string GenerateHandlerName(HandlersCollection collection, string phpVersion)
        {
            string prefix = "php-" + phpVersion;
            string name = prefix;

            for (int i = 1; true; i++)
            {
                if (collection[name] != null)
                {
                    name = prefix + "_" + i.ToString();
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
                    result.Add(new string[] { handler.Name, handler.ScriptProcessor, GetPHPExecutableVersion(handler.ScriptProcessor) });
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
            string phpIniPath = GetPHPIniPath();

            if (String.IsNullOrEmpty(phpIniPath))
            {
                throw new FileNotFoundException("php.ini file does not exist");
            }

            configInfo.PHPIniFilePath = phpIniPath;

            PHPIniFile file = new PHPIniFile(phpIniPath);
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

            return configInfo;
        }

        private static string GetPHPExecutableVersion(string phpexePath)
        {
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(phpexePath);
            return fileVersionInfo.ProductVersion;
        }

        public string GetPHPIniPath()
        {
            // PHP is not registered so we do not know the path to php.ini file
            if (_currentFastCgiApplication == null || _currentPHPHandler == null)
            {
                return String.Empty;
            }

            string directoryPath = _currentFastCgiApplication.EnvironmentVariables["PHPRC"].Value;
            // If PHPRC is not set then use the same path where php-cgi.exe is located
            if (String.IsNullOrEmpty(directoryPath))
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
                }
            }
        }

        private static void MoveHandlerOnTop(HandlersCollection handlersCollection, string handlerName)
        {
            HandlerElement handlerElement = handlersCollection[handlerName];
            if (handlerElement != null)
            {
                handlersCollection.Remove(handlerElement);
                handlersCollection.AddCopyAt(0, handlerElement);
            }
        }

        public void RegisterPHPWithIIS(string path)
        {
            string phpexePath = Environment.ExpandEnvironmentVariables(path);
            bool iisUpdateHappened = false;
            
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

            // Check for existense of php extensions directory
            string phpDir = Path.GetDirectoryName(phpexePath);
            string extDir = Path.Combine(phpDir, "ext");
            if (!Directory.Exists(extDir))
            {
                throw new DirectoryNotFoundException("ext directory does not exist in " + phpDir);
            }
            
            // Check for existence of php.ini file. If it does not exist then copy php.ini-recommended
            // or php.ini-production to it
            string phpiniPath = Path.Combine(phpDir, "php.ini");
            if (!File.Exists(phpiniPath))
            {
                string phpiniRecommendedPath = Path.Combine(phpDir, "php.ini-recommended");
                string phpiniProductionPath = Path.Combine(phpDir, "php.ini-production");
                if (File.Exists(phpiniRecommendedPath))
                {
                    File.Copy(phpiniRecommendedPath, phpiniPath);
                }
                else if (File.Exists(phpiniProductionPath))
                {
                    File.Copy(phpiniProductionPath, phpiniPath);
                }
                else
                {
                    throw new FileNotFoundException("php.ini and php.ini recommended do not exist in " + phpDir);
                }
            }

            ApplicationElement fastCgiApplication = _fastCgiApplicationCollection.GetApplication(phpexePath, "");
            
            // Create a FastCGI application if it does not exist
            if (fastCgiApplication == null)
            {
                fastCgiApplication = _fastCgiApplicationCollection.CreateElement();
                fastCgiApplication.FullPath = phpexePath;
                fastCgiApplication.MonitorChangesTo = phpiniPath;
                fastCgiApplication.InstanceMaxRequests = 10000;
                fastCgiApplication.ActivityTimeout = 300;
                fastCgiApplication.RequestTimeout = 300;

                fastCgiApplication.EnvironmentVariables.Add("PHPRC", phpDir);
                fastCgiApplication.EnvironmentVariables.Add("PHP_FCGI_MAX_REQUESTS", "10000");

                _fastCgiApplicationCollection.Add(fastCgiApplication);
                iisUpdateHappened = true;
            }

            // Check if file mapping with this executable already exists
            HandlerElement handlerElement = _handlersCollection.GetHandler("*.php", phpexePath);
            
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
                iisUpdateHappened = true;
            }
            else if (_handlersCollection.IndexOf(handlerElement) > 0)
            {
                // Move the existing PHP file handler mapping on top
                CopyInheritedHandlers();
                MoveHandlerOnTop(_handlersCollection, handlerElement.Name);
                iisUpdateHappened = true;
            }

            if (iisUpdateHappened)
            {
                _managementUnit.Update();
            }

            // Update the references to current php handler and application
            _currentPHPHandler = handlerElement;
            _currentFastCgiApplication = fastCgiApplication;

            // Make the recommended changes to php.ini file
            ApplyRecommendedPHPIniSettings();
        }

        public void SelectPHPHandler(string name)
        {
            HandlerElement handler = _handlersCollection[name];
            if (handler != null && _handlersCollection.IndexOf(handler) > 0)
            {
                CopyInheritedHandlers();
                MoveHandlerOnTop(_handlersCollection, name);

                _managementUnit.Update();

                // Update the references to current php handler and application
                ApplicationElement fastCgiApplication = _fastCgiApplicationCollection.GetApplication(handler.ScriptProcessor, "");
                _currentFastCgiApplication = fastCgiApplication;
                _currentPHPHandler = handler;
            }
        }

    }
}