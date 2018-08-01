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
using Microsoft.Win32;
using Web.Management.PHP.DefaultDocument;
using Web.Management.PHP.FastCgi;
using Web.Management.PHP.Handlers;

namespace Web.Management.PHP.Config
{

    /// <summary>
    /// Provides functions to register PHP with IIS and to manage IIS and PHP configuration.
    /// </summary>
    public sealed class PHPConfigHelper
    {

        private readonly IConfigurationWrapper _configurationWrapper;

        private ApplicationElement _currentFastCgiApplication;
        private HandlerElement _currentPhpHandler;
        private HandlersCollection _handlersCollection;
        private FastCgiApplicationCollection _fastCgiApplicationCollection;
        private FilesCollection _defaultDocumentCollection;
        private string _phpIniFilePath;
        private string _phpDirectory;
        private PHPRegistrationType _registrationType;

        public PHPConfigHelper(IConfigurationWrapper configurationWrapper)
        {
            _configurationWrapper = configurationWrapper;
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

        public string AddExtension(string extensionPath)
        {
            if (String.IsNullOrEmpty(extensionPath))
            {
                throw new ArgumentException("The extension path is not specified.");
            }

            EnsurePHPIsRegistered();

            var filename = Path.GetFileName(extensionPath);
            if (String.IsNullOrEmpty(filename))
            {
                throw new ArgumentException(String.Format("Cannot extract file name from the extention path {0}.", extensionPath));   
            }
            var targetPath = Path.Combine(PHPDirectory, "ext");
            targetPath = Path.Combine(targetPath, filename);

            File.Copy(extensionPath, targetPath, false);
            var extension = new PHPIniExtension(filename, true);

            var extensions = new RemoteObjectCollection<PHPIniExtension> { extension };
            UpdateExtensions(extensions);
            
            return extension.Name;
        }

        public void AddOrUpdatePHPIniSettings(IEnumerable<PHPIniSetting> settings)
        {
            EnsurePHPIsRegistered();

            var file = new PHPIniFile(PHPIniFilePath);
            file.Parse();

            file.AddOrUpdateSettings(settings);
            file.Save(file.FileName);
        }

        private void ApplyRecommendedFastCgiSettings(ArrayList configIssueIndexes)
        {
            var iisChangeHappened = false;

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
                    case PHPConfigIssueIndex.PhpMaxRequests:
                        {
                            iisChangeHappened = ChangePHPMaxRequests() || iisChangeHappened;
                            break;
                        }
                    case PHPConfigIssueIndex.Phprc:
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
                _configurationWrapper.CommitChanges();
            }
        }

        private void ApplyRecommendedPHPIniSettings(ArrayList configIssueIndexes)
        {
            var file = new PHPIniFile(PHPIniFilePath);
            file.Parse();

            var settings = new List<PHPIniSetting>();

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
                    case PHPConfigIssueIndex.DateTimeZone:
                        {
                            settings.Add(GetToApplyDateTimeZone(file));
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
            EnsurePHPIsRegistered();

            ApplyRecommendedFastCgiSettings(configIssueIndexes);
            ApplyRecommendedPHPIniSettings(configIssueIndexes);
        }

        private bool ChangeDefaultDocument()
        {
            var changeHappened = false;

            var fileElement = _defaultDocumentCollection["index.php"];

            // We need to copy inherited default documents in order to prevent config errors
            // caused by adding the same document on the upper configuration level.
            CopyIneritedDefaultDocs();

            if (fileElement == null)
            {
                fileElement = _defaultDocumentCollection.CreateElement();
                fileElement.Value = "index.php";
                _defaultDocumentCollection.AddAt(0, fileElement);
                changeHappened = true;
            }
            else if (_defaultDocumentCollection.IndexOf(fileElement) > 0)
            {
                MoveIndexPhpOnTop();
                changeHappened = true;
            }

            return changeHappened;
        }

        private bool ChangeMonitorChanges()
        {
            var changeHappened = false;

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
            var envVariableElement = _currentFastCgiApplication.EnvironmentVariables["PHP_FCGI_MAX_REQUESTS"];
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
            var changeHappened = false;

            // Set PHPRC
            var envVariableElement = _currentFastCgiApplication.EnvironmentVariables["PHPRC"];
            var expectedValue = EnsureTrailingBackslash(Path.GetDirectoryName(PHPIniFilePath));
            if (envVariableElement == null)
            {
                _currentFastCgiApplication.EnvironmentVariables.Add("PHPRC", expectedValue);
                changeHappened = true;
            }
            else
            {
                if (!IsValidPhpDirectory(envVariableElement.Value))
                {
                    envVariableElement.Value = expectedValue;
                    changeHappened = true;
                }
            }

            return changeHappened;
        }

        private bool ChangeResourceType()
        {
            bool changeHappened = false;

            if (_currentPhpHandler.ResourceType != ResourceType.Either)
            {
                _currentPhpHandler.ResourceType = ResourceType.Either;
                changeHappened = true;
            }

            return changeHappened;
        }

        private void CopyIneritedDefaultDocs()
        {
            if (_configurationWrapper.IsServerConfigurationPath())
            {
                return;
            }

            var list = new FileElement[_defaultDocumentCollection.Count];
            ((ICollection)_defaultDocumentCollection).CopyTo(list, 0);

            _defaultDocumentCollection.Clear();

            foreach (FileElement f in list)
            {
                _defaultDocumentCollection.AddCopy(f);
            }
        }

        private void CopyInheritedHandlers()
        {
            if (_configurationWrapper.IsServerConfigurationPath())
            {
                return;
            }

            var list = new HandlerElement[_handlersCollection.Count];
            ((ICollection)_handlersCollection).CopyTo(list, 0);

            _handlersCollection.Clear();

            foreach (var handler in list)
            {
                _handlersCollection.AddCopy(handler);
            }
        }

        private static string DoubleQuotesWrap(string value)
        {
            return '"' + value + '"';
        }

        private static string EnsureBackslashes(string str)
        {
            return str.Replace('/', '\\');
        }

        private static string EnsureTrailingBackslash(string str)
        {            
            if (!str.EndsWith(@"\", StringComparison.Ordinal))
            {
                return str + @"\";
            }
            
            return str;
        }

        private static Version ExtractVersion(string versionAsIs)
        {
            Version result = null;

            var r = new Regex(@"^(?<version>\d+\.\d+\.\d+(?:\.\d+)?).*");
            var m = r.Match(versionAsIs);
            if (m.Success)
            {
                var version = r.Match(versionAsIs).Result("${version}");
                result = new Version(version);
            }

            return result;
        }

        private static string GenerateHandlerName(HandlersCollection collection, string phpVersion)
        {
            string prefix = String.Format("php-{0}",phpVersion);
            string name = prefix;

            for (var i = 1;; i++)
            {
                if (collection[name] != null)
                {
                    name = String.Format("{0}_{1}", prefix, i.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    break;
                }
            }
            return name;
        }

        public RemoteObjectCollection<PHPVersion> GetAllPHPVersions()
        {
            EnsurePHPIsRegistered();

            var result = new RemoteObjectCollection<PHPVersion>();
            
            foreach (var handler in _handlersCollection)
            {
                if (!String.Equals(handler.Path, "*.php", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                
                if (String.Equals(handler.Modules, "FastCgiModule", StringComparison.OrdinalIgnoreCase) &&
                    File.Exists(handler.Executable))
                {
                    result.Add(new PHPVersion(handler.Name, handler.Executable, GetPHPExecutableVersion(handler.Executable)));
                }
            }

            return result;
        }

        public PHPConfigInfo GetPHPConfigInfo()
        {
            var configInfo = new PHPConfigInfo();

            // If PHP is not registered properly then just return information about
            // how it registered.
            if (!IsPHPRegistered())
            {
                configInfo.RegistrationType = _registrationType;
                return configInfo;
            }

            configInfo.RegistrationType = _registrationType;
            configInfo.HandlerName = _currentPhpHandler.Name;
            configInfo.HandlerIsLocal = _currentPhpHandler.IsLocallyStored;
            configInfo.Executable = _currentPhpHandler.Executable;
            configInfo.Version = GetPHPExecutableVersion(_currentPhpHandler.Executable);
            configInfo.PHPIniFilePath = PHPIniFilePath;

            var file = new PHPIniFile(PHPIniFilePath);
            file.Parse();

            var setting = file.GetSetting("error_log");
            configInfo.ErrorLog = setting != null ? setting.GetTrimmedValue() : String.Empty;

            configInfo.EnabledExtCount = file.GetEnabledExtensionsCount();
            configInfo.InstalledExtCount = file.Extensions.Count;

            ICollection issues = ValidateConfiguration(file);
            configInfo.IsConfigOptimal = (issues.Count == 0);

            return configInfo;
        }

        private string GetPHPDirectory()
        {
            string phpDirectory = Path.GetDirectoryName(Environment.ExpandEnvironmentVariables(_currentPhpHandler.Executable));
            return EnsureTrailingBackslash(phpDirectory);
        }

        private static string GetPHPExecutableVersion(string phpexePath)
        {
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(phpexePath);
            return fileVersionInfo.ProductVersion;
        }

        public PHPIniFile GetPHPIniFile()
        {
            EnsurePHPIsRegistered();

            var file = new PHPIniFile(PHPIniFilePath);
            file.Parse();

            return file;
        }

        private string GetPHPIniFilePath()
        {
            var directoryPath = String.Empty;

            // If PHPRC environment variable is set per FastCGI process then use the path specified there,
            var phpRcElement = _currentFastCgiApplication.EnvironmentVariables["PHPRC"];
            if (phpRcElement != null && !String.IsNullOrEmpty(phpRcElement.Value))
            {
                directoryPath = phpRcElement.Value;
            }
            else
            {
                // If system-wide PHPRC environment variable is set then use it
                var envVar = Environment.GetEnvironmentVariable("PHPRC");
                if (!String.IsNullOrEmpty(envVar))
                {
                    directoryPath = envVar;
                }
            }

            // Otherwise try to get the directory path from registry,
            // Otherwise use the same path as where PHP executable is located.
            if (String.IsNullOrEmpty(directoryPath))
            {
                directoryPath = GetPHPIniPathFromRegistry(_currentPhpHandler.Executable);
                if (String.IsNullOrEmpty(directoryPath))
                {
                    directoryPath = Path.GetDirectoryName(_currentPhpHandler.Executable);

                    if (String.IsNullOrEmpty(directoryPath))
                    {
                        throw new ArgumentException("The directory path to php.ini file could not be determined from executable path.");
                    }
                }
            }

            string phpIniPath;
            if (IsDirectory(directoryPath))
            {
                phpIniPath = Path.Combine(directoryPath, "php-cgi-fcgi.ini");
            }
            else
            {
                phpIniPath = directoryPath;
                directoryPath = Path.GetDirectoryName(phpIniPath);
                if (String.IsNullOrEmpty(directoryPath))
                {
                    throw new ArgumentException("The directory path to php.ini file could not be determined.");
                }
            }

            if (File.Exists(phpIniPath))
            {
                return phpIniPath;
            }

            phpIniPath = Path.Combine(directoryPath, "php.ini");
            
            return File.Exists(phpIniPath) ? phpIniPath : String.Empty;
        }

        private static bool IsDirectory(string directoryPath)
        {
            return ((File.GetAttributes(directoryPath) & FileAttributes.Directory) == FileAttributes.Directory);
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

        private static string GetPHPTimeZone()
        {
            const long ticksPerHour = TimeSpan.TicksPerHour;
            const long ticksPerMinute = TimeSpan.TicksPerMinute;
            var timezones = new Dictionary<long, string>
                {
                    {-12*ticksPerHour, "Kwajalein"},
                    {-11*ticksPerHour, "Pacific/Midway"},
                    {-10*ticksPerHour, "Pacific/Honolulu"},
                    {-9*ticksPerHour, "America/Anchorage"},
                    {-8*ticksPerHour, "America/Los_Angeles"},
                    {-7*ticksPerHour, "America/Denver"},
                    {-6*ticksPerHour, "America/Tegucigalpa"},
                    {-5*ticksPerHour, "America/New_York"},
                    {-4*ticksPerHour - 30*ticksPerMinute, "America/Caracas"},
                    {-4*ticksPerHour, "America/Halifax"},
                    {-3*ticksPerHour - 30*ticksPerMinute, "America/St_Johns"},
                    {-3*ticksPerHour, "America/Sao_Paulo"},
                    {-2*ticksPerHour, "Atlantic/South_Georgia"},
                    {-1*ticksPerHour, "Atlantic/Azores"},
                    {0, "Europe/Dublin"},
                    {1*ticksPerHour, "Europe/Belgrade"},
                    {2*ticksPerHour, "Europe/Minsk"},
                    {3*ticksPerHour, "Asia/Kuwait"},
                    {3*ticksPerHour + 30*ticksPerMinute, "Asia/Tehran"},
                    {4*ticksPerHour, "Asia/Muscat"},
                    {5*ticksPerHour, "Asia/Yekaterinburg"},
                    {5*ticksPerHour + 30*ticksPerMinute, "Asia/Kolkata"},
                    {5*ticksPerHour + 45*ticksPerMinute, "Asia/Katmandu"},
                    {6*ticksPerHour, "Asia/Dhaka"},
                    {6*ticksPerHour + 30*ticksPerMinute, "Asia/Rangoon"},
                    {7*ticksPerHour, "Asia/Krasnoyarsk"},
                    {8*ticksPerHour, "Asia/Brunei"},
                    {9*ticksPerHour, "Asia/Seoul"},
                    {9*ticksPerHour + 30*ticksPerMinute, "Australia/Darwin"},
                    {10*ticksPerHour, "Australia/Canberra"},
                    {11*ticksPerHour, "Asia/Magadan"},
                    {12*ticksPerHour, "Pacific/Fiji"},
                    {13*ticksPerHour, "Pacific/Tongatapu"}
                };

            var currentTime = DateTime.Now;
            var localZone = TimeZone.CurrentTimeZone;
            var offset = localZone.GetUtcOffset(currentTime);

            // Some weird code to handle daylight savings time shifts
            if (localZone.IsDaylightSavingTime(currentTime))
            {
                var daylightTime = localZone.GetDaylightChanges(currentTime.Year);
                if (offset >= TimeSpan.Zero)
                {
                    offset += daylightTime.Delta;
                }
                else
                {
                    offset -= daylightTime.Delta;
                }
            }

            // Try to map the offset to one of the PHP time zones. If none found then use UTC time.
            string phpTimeZone;
            if (!timezones.TryGetValue(offset.Ticks, out phpTimeZone))
            {
                phpTimeZone = "Europe/Dublin";
            }

            return phpTimeZone;
        }

        private static PHPIniSetting GetToApplyCgiForceRedirect()
        {
            return new PHPIniSetting("cgi.force_redirect", "0", "PHP");
        }

        private static PHPIniSetting GetToApplyCgiPathInfo()
        {
            return new PHPIniSetting("cgi.fix_pathinfo", "1", "PHP");
        }

        private static PHPIniSetting GetToApplyDateTimeZone(PHPIniFile file)
        {
            var setting = file.GetSetting("date.timezone") ?? new PHPIniSetting("date.timezone", DoubleQuotesWrap(GetPHPTimeZone()), "Date");

            return setting;
        }

        private PHPIniSetting GetToApplyErrorLog(PHPIniFile file)
        {
            var handlerName = _currentPhpHandler.Name;
            var setting = file.GetSetting("error_log");
            if (setting == null || !IsAbsoluteFilePath(setting.GetTrimmedValue(), true))
            {
                var value = Path.Combine(Environment.ExpandEnvironmentVariables(@"%WINDIR%\Temp\"), handlerName + "_errors.log");
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
            if (setting == null || !IsAbsoluteFilePath(setting.GetTrimmedValue(), false))
            {
                string value = Environment.ExpandEnvironmentVariables(@"%WINDIR%\Temp\");
                setting = new PHPIniSetting("session.save_path", DoubleQuotesWrap(value), "Session");
            }
            
            return setting;
        }

        private static PHPIniSetting GetToApplyUploadTmpDir(PHPIniFile file)
        {
            PHPIniSetting setting = file.GetSetting("upload_tmp_dir");
            if (setting == null || !IsAbsoluteFilePath(setting.GetTrimmedValue(), false))
            {
                string value = Environment.ExpandEnvironmentVariables(@"%WINDIR%\Temp\");
                setting = new PHPIniSetting("upload_tmp_dir", DoubleQuotesWrap(value), "PHP");
            }

            return setting;
        }

        private void Initialize()
        {
            if (!IsFastCgiInstalled())
            {
                // If FastCGI is not installed on IIS then bail out as there is no point to continue
                _registrationType = PHPRegistrationType.NoneNoFastCgi;
                return;
            }

            // Get the handlers collection
            var handlersSection = _configurationWrapper.GetHandlersSection();
            _handlersCollection = handlersSection.Handlers;

            // Get the Default document collection
            var defaultDocumentSection = _configurationWrapper.GetDefaultDocumentSection();
            _defaultDocumentCollection = defaultDocumentSection.Files;

            // Get the FastCgi application collection
            var appHostConfig = _configurationWrapper.GetAppHostConfiguration();
            var fastCgiSection = (FastCgiSection)appHostConfig.GetSection("system.webServer/fastCgi", typeof(FastCgiSection));
            _fastCgiApplicationCollection = fastCgiSection.Applications;

            // Assume by default that PHP is not registered
            _registrationType = PHPRegistrationType.None;

            // Find the currently active PHP handler and FastCGI application
            var handler = _handlersCollection.GetActiveHandler("*.php");
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
                        _currentPhpHandler = handler;
                        _currentFastCgiApplication = fastCgiApplication;
                        _phpIniFilePath = GetPHPIniFilePath();
                        if (String.IsNullOrEmpty(_phpIniFilePath))
                        {
                            throw new FileNotFoundException(String.Format(Resources.CannotFindPhpIniForExecutableError, handler.Executable));
                        }
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
            var directory = Environment.ExpandEnvironmentVariables(path);
            if (Path.IsPathRooted(path))
            {
                if (isFile)
                {
                    directory = Path.GetDirectoryName(directory);

                    if (String.IsNullOrEmpty(directory))
                    {
                        throw new ArgumentException("Directory cannot be determined from the path.");
                    }
                }

                return Directory.Exists(directory);
            }

            return false;
        }

        private static bool IsFastCgiInstalled()
        {
            var result = false;

            const string subkey = "SOFTWARE\\Microsoft\\InetStp\\Components";
            var key = Registry.LocalMachine.OpenSubKey(subkey);
            if (key != null)
            {
                var value = key.GetValue("FastCgi");
                if (value != null)
                {
                    result = true;
                }
            }

            return result;
        }

        private bool IsPHPRegistered()
        {
            return (_registrationType == PHPRegistrationType.FastCgi);
        }

        private void MakeHandlerActive(string handlerName)
        {
            // We have to look up the handler elements by name because we may be working
            // on the copy of the handlers collection.
            var handlerElement = _handlersCollection[handlerName];
            var activeHandlerElement = _handlersCollection[_currentPhpHandler.Name];
            Debug.Assert(handlerElement != null && activeHandlerElement != null);

            var activeHandlerIndex = _handlersCollection.IndexOf(activeHandlerElement);
            _handlersCollection.Remove(handlerElement);
            _handlersCollection.AddCopyAt(activeHandlerIndex, handlerElement);
        }

        private void MakeRecommendedFastCgiChanges()
        {
            bool iisChangeHappened;

            iisChangeHappened = ChangeDefaultDocument();
            iisChangeHappened = ChangeResourceType() || iisChangeHappened;
            iisChangeHappened = ChangePHPMaxRequests() || iisChangeHappened;
            iisChangeHappened = ChangePHPRC() || iisChangeHappened;
            iisChangeHappened = ChangeMonitorChanges() || iisChangeHappened;

            if (iisChangeHappened)
            {
                _configurationWrapper.CommitChanges();
            }
        }

        private void MakeRecommendedPHPIniChanges()
        {
            var file = new PHPIniFile(PHPIniFilePath);
            file.Parse();

            // Set the recommended php.ini settings
            var settings = new List<PHPIniSetting>
                {
                    GetToApplyExtensionDir(),
                    GetToApplyLogErrors(),
                    GetToApplyErrorLog(file),
                    GetToApplySessionPath(file),
                    GetToApplyUploadTmpDir(file),
                    GetToApplyDateTimeZone(file),
                    GetToApplyCgiForceRedirect(),
                    GetToApplyCgiPathInfo(),
                    GetToApplyFastCgiImpersonate(),
                    new PHPIniSetting("fastcgi.logging", "0", "PHP"),
                    new PHPIniSetting("max_execution_time", "300", "PHP"),
                    new PHPIniSetting("display_errors", "Off", "PHP")
                };

            // Enable the most common PHP extensions
            var extensions = new List<PHPIniExtension>
                {
                    new PHPIniExtension("php_curl.dll", true),
                    new PHPIniExtension("php_gd2.dll", true),
                    new PHPIniExtension("php_gettext.dll", true),
                    new PHPIniExtension("php_mysql.dll", true),
                    new PHPIniExtension("php_mysqli.dll", true),
                    new PHPIniExtension("php_mbstring.dll", true),
                    new PHPIniExtension("php_openssl.dll", true),
                    new PHPIniExtension("php_soap.dll", true),
                    new PHPIniExtension("php_xmlrpc.dll", true)
                };

            file.UpdateExtensions(extensions);
            file.AddOrUpdateSettings(settings);
            file.Save(PHPIniFilePath);
        }

        private void MoveIndexPhpOnTop()
        {
            var fileElement = _defaultDocumentCollection["index.php"];
            Debug.Assert(fileElement != null);

            _defaultDocumentCollection.Remove(fileElement);
            _defaultDocumentCollection.AddCopyAt(0, fileElement);
        }

        private static string PreparePHPIniFile(string phpDir)
        {
            // Check for existence of php.ini file. If it does not exist then copy php.ini-recommended
            // or php.ini-production to it
            var phpIniFilePath = Path.Combine(phpDir, "php-cgi-fcgi.ini");
            if (!File.Exists(phpIniFilePath))
            {
                phpIniFilePath = Path.Combine(phpDir, "php.ini");

                if (!File.Exists(phpIniFilePath))
                {
                    var phpIniRecommendedPath = Path.Combine(phpDir, "php.ini-recommended");
                    var phpIniProductionPath = Path.Combine(phpDir, "php.ini-production");
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
                        throw new FileNotFoundException(String.Format(Resources.PhpIniFilesDoNotExistError, phpDir));
                    }
                }
            }
            return phpIniFilePath;
        }

        public void RegisterPHPWithIIS(string path)
        {
            if (_registrationType == PHPRegistrationType.NoneNoFastCgi)
            {
                throw new InvalidOperationException(Resources.FastCgiNotEnabledError);
            }

            string phpexePath = Environment.ExpandEnvironmentVariables(path);
            
            if (!String.Equals(Path.GetFileName(phpexePath), "php-cgi.exe", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(String.Format(Resources.PhpCgiExePathInvalidError, phpexePath)); 
            }

            // Check for existence of php executable in the specified directory
            if (!File.Exists(phpexePath))
            {
                throw new FileNotFoundException(String.Format(Resources.PhpCgiExeDoesNotExistError, Path.GetDirectoryName(phpexePath)));
            }

            // Check for existence of php extensions directory
            var phpDir = EnsureTrailingBackslash(Path.GetDirectoryName(phpexePath));
            var extDir = Path.Combine(phpDir, "ext");
            if (!Directory.Exists(extDir))
            {
                throw new DirectoryNotFoundException(String.Format(Resources.FolderDoesNotHaveExtDirError, phpDir));
            }

            var phpIniFilePath = PreparePHPIniFile(phpDir);

            var iisUpdateHappened = false;
            var fastCgiApplication = _fastCgiApplicationCollection.GetApplication(phpexePath, "");
            // Create a FastCGI application if it does not exist
            var isNewFastCgi = false;
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
            var handlerElement = _handlersCollection.GetHandler("*.php", phpexePath);
            // Create a handler mapping if it does not exist
            var isNewHandler = false;

            // We need to copy inherited handlers in order to prevent config errors
            // caused by adding the same PHP version on the upper configuration level.
            CopyInheritedHandlers();

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
            else if (_currentPhpHandler != null && handlerElement != _currentPhpHandler)
            {
                // Move the existing PHP file handler mapping on top
                MakeHandlerActive(handlerElement.Name);
                iisUpdateHappened = true;
            }

            // Check if index.php is set as a default document and move it to the top of the list
            iisUpdateHappened = ChangeDefaultDocument() || iisUpdateHappened;

            if (iisUpdateHappened)
            {
                _configurationWrapper.CommitChanges();
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

        public void RemovePHPIniSetting(PHPIniSetting setting)
        {
            EnsurePHPIsRegistered();

            var file = new PHPIniFile(PHPIniFilePath);
            file.Parse();

            if (file.Remove(setting))
            {
                file.Save(file.FileName);
            }
        }

        public HandlerElement GetPHPHandlerByName(string name)
        {
            EnsurePHPIsRegistered();

            return _handlersCollection.GetHandlerByNameAndPath(name, "*.php");
        }

        private void EnsurePHPIsRegistered()
        {
            if (!IsPHPRegistered())
            {
                throw new InvalidOperationException(Resources.PhpNotRegisteredError);
            }
        }

        public void SelectPHPHandler(string name)
        {
            EnsurePHPIsRegistered();

            var handler = _handlersCollection[name];
            // If the handler is already an active PHP handler then no need to do anything.
            if (handler != null && handler != _currentPhpHandler)
            {
                CopyInheritedHandlers();
                MakeHandlerActive(name);
                _configurationWrapper.CommitChanges();
            }
        }

        private static string TryToGetIniFilePath(string version)
        {
            var subkey = "SOFTWARE\\PHP";
            if (!String.IsNullOrEmpty(version))
            {
                subkey = subkey + "\\" + version;
            }

            var key = Registry.LocalMachine.OpenSubKey(subkey);
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

        public void UpdateExtensions(IEnumerable<PHPIniExtension> extensions)
        {
            EnsurePHPIsRegistered();

            var file = new PHPIniFile(PHPIniFilePath);
            file.Parse();

            file.UpdateExtensions(extensions);
            file.Save(file.FileName);
        }

        private static PHPConfigIssue ValidateCgiForceRedirect(PHPIniFile file)
        {
            PHPConfigIssue configIssue = null;
            
            // Check if cgi.force_redirect is set correctly
            var setting = file.GetSetting("cgi.force_redirect");
            if (setting == null || String.IsNullOrEmpty(setting.GetTrimmedValue()))
            {
                configIssue = new PHPConfigIssue("cgi.force_redirect",
                                                                String.Empty,
                                                                "0",
                                                                "ConfigIssueCgiForceRedirectNotSet",
                                                                "ConfigIssueCgiForceRedirectRecommend",
                                                                PHPConfigIssueIndex.CgiForceRedirect);
            }
            else if (!String.Equals(setting.GetTrimmedValue(), "0", StringComparison.OrdinalIgnoreCase))
            {
                configIssue = new PHPConfigIssue("cgi.force_redirect",
                                                                setting.GetTrimmedValue(),
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
            var setting = file.GetSetting("cgi.fix_pathinfo");
            if (setting == null || String.IsNullOrEmpty(setting.GetTrimmedValue()))
            {
                configIssue = new PHPConfigIssue("cgi.fix_pathinfo",
                                                                String.Empty,
                                                                "1",
                                                                "ConfigIssueCgiPathInfoNotSet",
                                                                "ConfigIssueCgiPathInfoRecommend",
                                                                PHPConfigIssueIndex.CgiPathInfo);
            }
            else if (!String.Equals(setting.GetTrimmedValue(), "1", StringComparison.OrdinalIgnoreCase))
            {
                configIssue = new PHPConfigIssue("cgi.fix_pathinfo",
                                                                setting.GetTrimmedValue(),
                                                                "1",
                                                                "ConfigIssueCgiPathInfoNotCorrect",
                                                                "ConfigIssueCgiPathInfoRecommend",
                                                                PHPConfigIssueIndex.CgiPathInfo);
            }

            return configIssue;
        }

        public RemoteObjectCollection<PHPConfigIssue> ValidateConfiguration()
        {
            EnsurePHPIsRegistered();

            var file = new PHPIniFile(PHPIniFilePath);
            file.Parse();

            return ValidateConfiguration(file);
        }

        private RemoteObjectCollection<PHPConfigIssue> ValidateConfiguration(PHPIniFile file)
        {

            var configIssues = new RemoteObjectCollection<PHPConfigIssue>();
          
            // IIS and FastCGI settings
            var configIssue = ValidateDefaultDocument();
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

            configIssue = ValidateDateTimeZone(file);
            if (configIssue != null)
            {
                configIssues.Add(configIssue);
            }

            return configIssues;
        }

        private static PHPConfigIssue ValidateDateTimeZone(PHPIniFile file)
        {
            PHPConfigIssue configIssue = null;

            var setting = file.GetSetting("date.timezone");
            if (setting == null)
            {
                configIssue = new PHPConfigIssue("date.timezone",
                                                               String.Empty,
                                                               GetPHPTimeZone(),
                                                               "ConfigIssueDateTimeZoneNotSet",
                                                               "ConfigIssueDateTimeZoneRecommend",
                                                               PHPConfigIssueIndex.DateTimeZone);
            }

            return configIssue;
        }

        private PHPConfigIssue ValidateDefaultDocument()
        {
            PHPConfigIssue configIssue = null;
            // Check if index.php is set as a default document
            var fileElement = _defaultDocumentCollection["index.php"];
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
            var setting = file.GetSetting("error_log");
            string expectedValue = Path.Combine(Environment.ExpandEnvironmentVariables(@"%WINDIR%\Temp\"), _currentPhpHandler.Name + "_errors.log");
            if (setting == null || String.IsNullOrEmpty(setting.GetTrimmedValue()))
            {
                configIssue = new PHPConfigIssue("error_log",
                                                                String.Empty,
                                                                expectedValue,
                                                                "ConfigIssueErrorLogNotSet",
                                                                "ConfigIssueErrorLogRecommend",
                                                                PHPConfigIssueIndex.ErrorLog);
            }
            else if (!IsAbsoluteFilePath(setting.GetTrimmedValue(), true /* this is supposed to be a file */))
            {
                configIssue = new PHPConfigIssue("error_log",
                                                                setting.GetTrimmedValue(),
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
            
            var setting = file.GetSetting("extension_dir");
            string expectedValue = EnsureTrailingBackslash(Path.Combine(PHPDirectory, "ext"));
            if (setting == null || String.IsNullOrEmpty(setting.GetTrimmedValue()))
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
                string currentValue = EnsureTrailingBackslash(setting.GetTrimmedValue());
                currentValue = EnsureBackslashes(currentValue);
                if (!String.Equals(currentValue, expectedValue, StringComparison.OrdinalIgnoreCase))
                {
                    configIssue = new PHPConfigIssue("extension_dir",
                                                                    setting.GetTrimmedValue(),
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
            var setting = file.GetSetting("fastcgi.impersonate");
            if (setting == null || String.IsNullOrEmpty(setting.GetTrimmedValue()))
            {
                configIssue = new PHPConfigIssue("fastcgi.impersonate",
                                                                String.Empty,
                                                                "1",
                                                                "ConfigIssueFastCgiImpersonateNotSet",
                                                                "ConfigIssueFastCgiImpersonateRecommend",
                                                                PHPConfigIssueIndex.FastCgiImpersonation);
            }
            else if (!String.Equals(setting.GetTrimmedValue(), "1", StringComparison.OrdinalIgnoreCase))
            {
                configIssue = new PHPConfigIssue("fastcgi.impersonate",
                                                                setting.GetTrimmedValue(),
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
            var setting = file.GetSetting("log_errors");
            if (setting == null || String.IsNullOrEmpty(setting.GetTrimmedValue()))
            {
                configIssue = new PHPConfigIssue("log_errors",
                                                                String.Empty,
                                                                "On",
                                                                "ConfigIssueLogErrorsNotSet",
                                                                "ConfigIssueLogErrorsRecommend",
                                                                PHPConfigIssueIndex.LogErrors);
            }
            else if (!String.Equals(setting.GetTrimmedValue(), "On", StringComparison.OrdinalIgnoreCase))
            {
                configIssue = new PHPConfigIssue("log_errors",
                                                                setting.GetTrimmedValue(),
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
                var path = _currentFastCgiApplication.MonitorChangesTo;
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
            var envVariableElement = _currentFastCgiApplication.EnvironmentVariables["PHP_FCGI_MAX_REQUESTS"];
            if (envVariableElement == null)
            {
                configIssue = new PHPConfigIssue("PHP_FCGI_MAX_REQUESTS",
                                                                String.Empty,
                                                                _currentFastCgiApplication.InstanceMaxRequests.ToString(CultureInfo.InvariantCulture),
                                                                "ConfigIssuePHPMaxRequestsNotSet",
                                                                "ConfigIssuePHPMaxRequestsRecommend",
                                                                PHPConfigIssueIndex.PhpMaxRequests);
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
                                                                    PHPConfigIssueIndex.PhpMaxRequests);
                }
            }

            return configIssue;
        }

        private PHPConfigIssue ValidatePHPRC()
        {
            PHPConfigIssue configIssue = null;
            // Check if PHPRC is set and points to a directory that has php.ini file
            var envVariableElement = _currentFastCgiApplication.EnvironmentVariables["PHPRC"];
            var expectedValue = EnsureTrailingBackslash(Path.GetDirectoryName(PHPIniFilePath));
            if (envVariableElement == null)
            {
                configIssue = new PHPConfigIssue("PHPRC", 
                                                                String.Empty,
                                                                expectedValue,
                                                                "ConfigIssuePHPRCNotSet",
                                                                "ConfigIssuePHPRCRecommend",
                                                                PHPConfigIssueIndex.Phprc);
            }
            else
            {
                if (!IsValidPhpDirectory(envVariableElement.Value))
                {
                    configIssue = new PHPConfigIssue("PHPRC",
                                                                    envVariableElement.Value,
                                                                    expectedValue,
                                                                    "ConfigIssuePHPRCFileNotExists",
                                                                    "ConfigIssuePHPRCRecommend",
                                                                    PHPConfigIssueIndex.Phprc);
                }
            }

            return configIssue;
        }

        private static bool IsValidPhpDirectory(string path)
        {
            var directoryName = path;
            if (!IsDirectory(path))
            {
                directoryName = Path.GetDirectoryName(path);
                if (String.IsNullOrEmpty(directoryName))
                {
                    throw new ArgumentException("Directory name cannot be determined from the path.");
                }
            }

            var pathPhpIni = Path.Combine(directoryName, "php.ini");
            var pathPhpSapiIni = Path.Combine(directoryName, "php-cgi-fcgi.ini");

            return (File.Exists(pathPhpIni) || File.Exists(pathPhpSapiIni));
        }

        private PHPConfigIssue ValidateResourceType()
        {
            PHPConfigIssue configIssue = null;
            // Check if handler mapping is configured for "File or Folder"
            if (_currentPhpHandler.ResourceType != ResourceType.Either)
            {
                configIssue = new PHPConfigIssue("resourceType",
                                                                _currentPhpHandler.ResourceType.ToString(),
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
            var setting = file.GetSetting("session.save_path");
            var expectedValue = Environment.ExpandEnvironmentVariables(@"%WINDIR%\Temp\");
            if (setting == null || String.IsNullOrEmpty(setting.GetTrimmedValue()))
            {
                configIssue = new PHPConfigIssue("session.save_path",
                                                                String.Empty,
                                                                expectedValue,
                                                                "ConfigIssueSessionPathNotSet",
                                                                "ConfigIssueSessionPathRecommend",
                                                                PHPConfigIssueIndex.SessionPath);
            }
            else if (!IsAbsoluteFilePath(setting.GetTrimmedValue(), false /* this is supposed to be a directory */))
            {
                configIssue = new PHPConfigIssue("session.save_path",
                                                                setting.GetTrimmedValue(),
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
            var setting = file.GetSetting("upload_tmp_dir");
            var expectedValue = Environment.ExpandEnvironmentVariables(@"%WINDIR%\Temp\");
            if (setting == null || String.IsNullOrEmpty(setting.GetTrimmedValue()))
            {
                configIssue = new PHPConfigIssue("upload_tmp_dir",
                                                                String.Empty,
                                                                expectedValue,
                                                                "ConfigIssueUploadDirNotSet",
                                                                "ConfigIssueUploadDirRecommend",
                                                                PHPConfigIssueIndex.UploadDir);
            }
            else if (!IsAbsoluteFilePath(setting.GetTrimmedValue(), false /* this is supposed to be a directory */))
            {
                configIssue = new PHPConfigIssue("upload_tmp_dir",
                                                                setting.GetTrimmedValue(),
                                                                expectedValue,
                                                                "ConfigIssueUploadDirNotCorrect",
                                                                "ConfigIssueUploadDirRecommend",
                                                                PHPConfigIssueIndex.UploadDir);
            }

            return configIssue;
        }
    }
}