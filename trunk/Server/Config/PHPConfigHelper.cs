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
using Web.Management.PHP.Config;
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

        public PHPConfigHelper(ManagementUnit mgmtUnit)
        {
            _managementUnit = mgmtUnit;
        }

        private static void ApplyRecommendedPHPIniSettings(string phpiniPath)
        {
            // TODO: Add real stuff here
            PHPIniFile file = new PHPIniFile(phpiniPath);
            file.Parse();

            List<PHPIniSetting> settings = new List<PHPIniSetting>();
            // Set FastCGI Impersonation
            settings.Add(new PHPIniSetting("fastcgi.impersonate", "1", "PHP"));
            
            // Disable FastCGI logging
            settings.Add(new PHPIniSetting("fastcgi.logging", "0", "PHP"));

            // Set error log file path
            settings.Add(new PHPIniSetting("error_log", @"C:\php", "PHP"));

            file.AddOrUpdateSettings(settings);
            file.Save(phpiniPath);
        }

        private void CopyInheritedHandlers(HandlersCollection handlersCollection)
        {
            if (_managementUnit.ConfigurationPath.PathType == ConfigurationPathType.Server)
            {
                return;
            }

            HandlerElement[] list = new HandlerElement[handlersCollection.Count];
            ((ICollection)handlersCollection).CopyTo(list, 0);

            handlersCollection.Clear();

            foreach (HandlerElement handler in list)
            {
                handlersCollection.AddCopy(handler);
            }
        }

        /// <summary>
        /// Gets the information about all the PHP versions registered on the current 
        /// configuration scope.
        /// </summary>
        /// <returns>List of string arrays that contain handler name, script processor and version</returns>
        public ArrayList GetAllPHPVersions()
        {
            HandlersSection handlersSection = GetHandlersSection();
            ArrayList result = new ArrayList();

            foreach (HandlerElement handler in handlersSection.Handlers)
            {
                if (String.Equals(handler.Path, "*.php", StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(new string[] { handler.Name, handler.ScriptProcessor, GetPHPExecutableVersion(handler.ScriptProcessor) });
                }
            }

            return result;
        }

        private FastCgiSection GetFastCgiSection()
        {
            Configuration appHostConfig = _managementUnit.ServerManager.GetApplicationHostConfiguration();
            FastCgiSection fastCgiSection = (FastCgiSection)appHostConfig.GetSection("system.webServer/fastCgi", typeof(FastCgiSection));
            return fastCgiSection;
        }

        private HandlersSection GetHandlersSection()
        {
            ManagementConfiguration config = _managementUnit.Configuration;
            HandlersSection handlersSection = (HandlersSection)config.GetSection("system.webServer/handlers", typeof(HandlersSection));
            return handlersSection;
        }

        /// <summary>
        /// Returns the summary of the configuration information for the currently active PHP version
        /// </summary>
        /// <returns>The summary of the configuration information</returns>
        public PHPConfigInfo GetPHPConfigInfo()
        {
            HandlersCollection handlersCollection = GetHandlersSection().Handlers;
            HandlerElement handler = handlersCollection.GetActiveHandler("*.php");
            if (handler != null)
            {
                return new PHPConfigInfo(handler.Name, handler.ScriptProcessor, GetPHPExecutableVersion(handler.ScriptProcessor));
            }

            return null;
        }

        private static string GetPHPExecutableVersion(string phpexePath)
        {
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(phpexePath);
            return fileVersionInfo.ProductVersion;
        }

        public string GetPHPIniDirectory()
        {
            HandlersSection handlersSection = GetHandlersSection();
            HandlerElement phpHandler = handlersSection.Handlers.GetActiveHandler("*.php");
            if (phpHandler == null)
            {
                return String.Empty;
            }

            return GetPHPIniDirectory(phpHandler);
        }

        public string GetPHPIniDirectory(HandlerElement phpHandler)
        {
            string executable = phpHandler.ScriptProcessor;

            // TODO: handle the case with arguments
            FastCgiSection fastCgiSection = GetFastCgiSection();
            ApplicationElement fastCgiApplication = fastCgiSection.Applications.GetApplication(executable, "");
            if (fastCgiApplication == null)
            {
                return String.Empty;
            }

            string directoryPath = Path.GetDirectoryName(fastCgiApplication.MonitorChangesTo);

            if (!String.IsNullOrEmpty(directoryPath))
            {
                return directoryPath;
            }

            directoryPath = fastCgiApplication.EnvironmentVariables["PHPRC"].Value;
            if (!String.IsNullOrEmpty(directoryPath))
            {
                return directoryPath;
            }

            return String.Empty;
        }

        private static void MoveHandlerOnTop(HandlersCollection handlersCollection, string handlerName)
        {
            HandlerElement handlerElement = handlersCollection[handlerName];

            int index = handlersCollection.IndexOf(handlerElement);
            if (index != 0)
            {
                handlersCollection.Remove(handlerElement);
                handlersCollection.AddCopyAt(0, handlerElement);
            }
        }

        /// <summary>
        /// Registers PHP with IIS
        /// </summary>
        /// <param name="phpDirectory">The directory where PHP runtime binaries are located</param>
        public void RegisterPHPWithIIS(string phpDirectory)
        {
            string expandedDir = Environment.ExpandEnvironmentVariables(phpDirectory);

            // Check for existence of php executable in the specified directory
            string phpexePath = Path.Combine(phpDirectory, "php-cgi.exe");
            if (!File.Exists(phpexePath))
            {
                // If php-cgi.exe does not exist then it may be PHP4 installation
                phpexePath = Path.Combine(phpDirectory, "php.exe");
                if (!File.Exists(phpexePath))
                {
                    throw new ArgumentException("php-cgi.exe and php.exe do not exist in " + phpDirectory);
                }
            }
            
            // Check for existence of php.ini file. If it does not exist then copy php.ini-recommended to it
            string phpiniPath = Path.Combine(phpDirectory, "php.ini");
            if (!File.Exists(phpiniPath))
            {
                string phpiniRecommendedPath = Path.Combine(phpDirectory, "php.ini-recommended");
                if (File.Exists(phpiniRecommendedPath))
                {
                    File.Copy(phpiniRecommendedPath, phpiniPath);
                }
                else
                {
                    throw new ArgumentException("php.ini and php.ini recommended do not exist in " + phpDirectory);
                }
            }

            // Check if the FastCGI applicaiton for this executable already exists
            FastCgiSection fastCgiSection = GetFastCgiSection();
            
            FastCgiApplicationCollection applicationCollection = fastCgiSection.Applications;
            ApplicationElement fastCgiApplication = applicationCollection.GetApplication(phpexePath, "");
            
            // Create a FastCGI application if it does not exist
            if (fastCgiApplication == null)
            {
                ApplicationElement applicationElement = applicationCollection.CreateElement();
                applicationElement.FullPath = phpexePath;
                applicationElement.MonitorChangesTo = phpiniPath;
                applicationElement.InstanceMaxRequests = 10000;

                applicationElement.EnvironmentVariables.Add("PHPRC", expandedDir);
                applicationElement.EnvironmentVariables.Add("PHP_FCGI_MAX_REQUESTS", "10000");

                applicationCollection.Add(applicationElement);
            }

            // Check if file mapping with this executable already exists
            HandlersSection handlersSection = GetHandlersSection();
            HandlersCollection handlersCollection = handlersSection.Handlers;
            HandlerElement handlerElement = handlersCollection.GetHandler("*.php", phpexePath);
            
            if (handlerElement == null)
            {
                // Create a PHP file handler is it does not exist
                handlerElement = handlersCollection.CreateElement();
                handlerElement.Name = "php-" + GetPHPExecutableVersion(phpexePath);
                handlerElement.Modules = "FastCgiModule";
                handlerElement.RequireAccess = RequireAccess.Script;
                handlerElement.Verb = "*";
                handlerElement.Path = "*.php";
                handlerElement.ScriptProcessor = phpexePath;
                handlerElement.ResourceType = ResourceType.Either;
                handlersCollection.AddAt(0, handlerElement);
            }
            else
            {
                // Move the existing PHP file handler mapping on top
                string handlerName = handlerElement.Name;
                CopyInheritedHandlers(handlersCollection);
                MoveHandlerOnTop(handlersCollection, handlerName);
            }

            _managementUnit.Update();
        }

        /// <summary>
        /// Moves the specified PHP handler to the top of the handlers list, thus making it
        /// an active handler for the current configuration scope. If the current configuration
        /// scope is not server, then the handlers collection is copied locally.
        /// </summary>
        /// <param name="name">Name of the PHP file handler in IIS handlers collection</param>
        public void SelectPHPHandler(string name)
        {
            HandlersCollection handlersCollection = GetHandlersSection().Handlers;
            HandlerElement handler = handlersCollection[name];
            if (handler != null)
            {
                CopyInheritedHandlers(handlersCollection);
                MoveHandlerOnTop(handlersCollection, name);
                _managementUnit.Update();
            }
        }

    }
}