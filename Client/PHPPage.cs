//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.Management.Client;
using Microsoft.Web.Management.Client.Win32;
using Web.Management.PHP.Config;

namespace Web.Management.PHP
{

    [ModulePageIdentifier(Globals.PHPPageIdentifier)]
    internal sealed class PHPPage : ModulePage
    {

        private const int IndexRegisterPhpTask = 0;
        private const int IndexChangeVersionTask = 1;
        private const int IndexCheckPhpInfoTask = 2;
        private const int IndexErrorReportingTask = 0;
        private const int IndexLimitsTask = 1;
        private const int IndexAllSettingsTask = 2;
        private const int IndexAllExtensionsTask = 0;
        private const int IndexAddExtensionTask = 1;

        // Summary labels
        private Label _enabledExtLabel;
        private Label _installedExtLabel;
        private Label _handlerMappingNameLabel;
        private LinkLabel _handlerMappingValueLabel;
        private Label _errorLogNameLabel;
        private LinkLabel _errorLogValueLabel;
        private LinkLabel _configPathValueLabel;
        private Label _configPathNameLabel;
        private Label _executableValueLabel;
        private Label _executableNameLabel;
        private Label _versionValueLabel;
        private Label _versionNameLabel;

        private PHPPageItemControl _phpExtensionItem;
        private PHPPageItemControl _phpSettingsItem;
        private PHPPageItemControl _phpSetupItem;

        private new PHPModule Module
        {
            get
            {
                return (PHPModule)base.Module;
            }
        }

        protected override bool ShowTaskList
        {
            get
            {
                return false;
            }
        }

        private void AddExtension()
        {
            using (var dlg = new Extensions.AddExtensionDialog(Module, Connection.IsLocalConnection))
            {
                if (ShowDialog(dlg) == DialogResult.OK)
                {
                    Navigate(typeof(Extensions.AllExtensionsPage), dlg.AddedExtensionName);
                }
            }
        }

        private static string GetHandlerMappingLabelText(bool handlerIsLocal)
        {
            if (handlerIsLocal)
            {
                return Resources.PHPPageLocalHandler;
            }
            return Resources.PHPPageInheritedHandler;
        }

        private void GetSettings()
        {
            StartAsyncTask(Resources.AllSettingsPageGettingSettings, OnGetSettings, OnGetSettingsCompleted);
        }

        private string GetSiteUrlAndName(out string siteName, out string relativePath)
        {
            using (var dlg = new Setup.SelectSiteAndUrlDialog(Module, Connection))
            {
                if (ShowDialog(dlg) == DialogResult.OK)
                {
                    siteName = dlg.SiteName;
                    relativePath = dlg.RelativePath;
                    return dlg.SelectedUrl;
                }
            }

            siteName = string.Empty;
            relativePath = String.Empty;
            return null;
        }

        private void InitializeUI()
        {
            SuspendLayout();

            var uiService = (IManagementUIService)GetService(typeof(IManagementUIService));

            var titleFont = (Font)uiService.Styles["PageHeaderTitleFont"];
            Padding = new Padding(0, 12, 0, 0);

            //
            // All page item labels
            //
            _versionNameLabel = new Label {Text = Resources.PHPPageVersion};
            _versionValueLabel = new Label();

            _executableNameLabel = new Label {Text = Resources.PHPPageExecutable};
            _executableValueLabel = new Label();

            _handlerMappingNameLabel = new Label {Text = Resources.PHPPageHandlerMapping};
            _handlerMappingValueLabel = new LinkLabel();
            _handlerMappingValueLabel.LinkClicked += OnHandlerMappingValueLabelLinkClicked;

            _configPathNameLabel = new Label {Text = Resources.PHPPageConfigurationFile};
            _configPathValueLabel = new LinkLabel();
            _configPathValueLabel.LinkClicked += OnPathLinkLabelLinkClicked;

            _errorLogNameLabel = new Label {Text = Resources.PHPPageErrorLog};
            _errorLogValueLabel = new LinkLabel();
            _errorLogValueLabel.LinkClicked += OnPathLinkLabelLinkClicked;

            _enabledExtLabel = new Label();
            _installedExtLabel = new Label();

            //
            // PHPSetup
            //
            _phpSetupItem = new PHPPageItemControl
                {
                    RightToLeftLayout = RightToLeftLayout,
                    RightToLeft = RightToLeft
                };
            _phpSetupItem.TitleClick += OnPHPSetupItemTitleClick;
            _phpSetupItem.Title = Resources.PHPSetupItemTitle;
            _phpSetupItem.TitleFont = titleFont;
            _phpSetupItem.Image = Resources.PHPSetup32;

            _phpSetupItem.AddInfoRow(_versionNameLabel, _versionValueLabel);
            _phpSetupItem.AddInfoRow(_executableNameLabel, _executableValueLabel);
            _phpSetupItem.AddInfoRow(_handlerMappingNameLabel, _handlerMappingValueLabel);
            _phpSetupItem.AddTask(OnPHPSetupItemClick,
                                    Resources.PHPSetupItemRegisterPHPTask,
                                    Resources.PHPSetupItemChangeVersionTask,
                                    Resources.PHPSetupItemCheckPHPInfoTask);

            Controls.Add(_phpSetupItem);
            //
            // PHP Settings
            //
            _phpSettingsItem = new PHPPageItemControl
                {
                    RightToLeftLayout = RightToLeftLayout,
                    RightToLeft = RightToLeft
                };
            _phpSettingsItem.TitleClick += OnPHPSettingsItemTitleClick;
            _phpSettingsItem.Title = Resources.PHPSettingsItemTitle;
            _phpSettingsItem.TitleFont = titleFont;
            _phpSettingsItem.Image = Resources.PHPSettings32;

            _phpSettingsItem.AddInfoRow(_configPathNameLabel, _configPathValueLabel);
            _phpSettingsItem.AddInfoRow(_errorLogNameLabel, _errorLogValueLabel);
            if (Connection.IsUserServerAdministrator)
            {
                _phpSettingsItem.AddTask(OnPHPSettingsItemClick,
                                        Resources.PHPSettingsItemErrorReportingTask,
                                        Resources.PHPSettingsItemLimitsTask,
                                        Resources.PHPSettingsItemAllSettingsTask);
            }
            else
            {
                _phpSettingsItem.AddTask(OnPHPSettingsItemClick,
                                        Resources.PHPSettingsItemReadOnlyErrorReportingTask,
                                        Resources.PHPSettingsItemReadOnlyLimitsTask,
                                        Resources.PHPSettingsItemReadOnlyAllSettingsTask);
            }

            Controls.Add(_phpSettingsItem);
            //
            // PHP Extensions
            //
            _phpExtensionItem = new PHPPageItemControl
                {
                    RightToLeftLayout = RightToLeftLayout,
                    RightToLeft = RightToLeft
                };
            _phpExtensionItem.TitleClick += OnPHPExtensionItemTitleClick;
            _phpExtensionItem.Title = Resources.PHPExtensionsItemTitle;
            _phpExtensionItem.TitleFont = titleFont;
            _phpExtensionItem.Image = Resources.PHPExtensions32;

            _phpExtensionItem.AddSpanRow(_enabledExtLabel);
            _phpExtensionItem.AddSpanRow(_installedExtLabel);
            if (Connection.IsUserServerAdministrator)
            {
                _phpExtensionItem.AddTask(OnPHPExtensionItemClick,
                                        Resources.PHPExtensionItemEnableTask, Resources.PHPExtensionItemAddTask);
            }
            else
            {
                _phpExtensionItem.AddTask(OnPHPExtensionItemClick,
                                        Resources.PHPExtensionItemReadOnlyEnableTask);
            }

            Controls.Add(_phpExtensionItem);

            // Update the information summaries for each PHPPageItemControl
            Refresh();

            ResumeLayout(true);
        }

        private void NavigateToPHPInfo()
        {
            string siteName;
            string relativePath;
            var siteUrl = GetSiteUrlAndName(out siteName, out relativePath);
            if (!String.IsNullOrEmpty(siteUrl))
            {
                Navigate(typeof(Setup.PHPInfoPage), new[] { siteUrl, siteName, relativePath });
            }
        }

        protected override void OnActivated(bool initialActivation)
        {
            base.OnActivated(initialActivation);
            
            if (initialActivation)
            {
                InitializeUI();
            }
        }

        private static void OnFastCgiLearnMoreLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string learnMoreUrl = String.Format(Globals.EnableFastCgiLearnMore, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
            Helper.Browse(learnMoreUrl);
        }

        private void OnGetSettings(object sender, DoWorkEventArgs e)
        {
            e.Result = Module.Proxy.GetPHPConfigInfo();
        }

        private void OnGetSettingsCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                var configInfo = (PHPConfigInfo)e.Result;
                UpdatePageItemsState(configInfo);
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ex, Resources.ResourceManager);
                UpdatePageItemsState(null);
            }
        }

        private void OnHandlerMappingValueLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Type handlerMappingsPage = GetPageType("Handlers");
            if (handlerMappingsPage != null)
            {
                Navigate(handlerMappingsPage);
            }
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            if (!Visible || Height == 0)
            {
                return;
            }

            var controls = Controls;

            var clientSize = ClientSize;

            var width = clientSize.Width - Padding.Horizontal - 12;

            var proposedSize = new Size(width, Int32.MaxValue);

            var top = Padding.Top + AutoScrollPosition.Y;
            for (var i = 0; i < controls.Count; i++)
            {
                var ctl = controls[i];
                var size = ctl.GetPreferredSize(proposedSize);
                ctl.SetBounds(Padding.Left,
                              top,
                              size.Width,
                              size.Height);

                top += ctl.Height;
            }

            if (top >= ClientSize.Height)
            {
                AdjustFormScrollbars(true);
                AutoScrollMinSize = new Size(ClientSize.Width, top);
            }
            else
            {
                AutoScrollMinSize = Size.Empty;
                AdjustFormScrollbars(false);
            }
        }

        private void OnPathLinkLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenPhysicalFile((string)e.Link.LinkData);
        }

        private void OnPHPExtensionItemClick(int index)
        {
            if (index == IndexAllExtensionsTask)
            {
                Navigate(typeof(Extensions.AllExtensionsPage));
            }
            if (index == IndexAddExtensionTask)
            {
                AddExtension();
            }
        }

        private void OnPHPExtensionItemTitleClick(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Navigate(typeof(Extensions.AllExtensionsPage));
        }

        private void OnPHPSettingsItemClick(int index)
        {
            if (index == IndexErrorReportingTask)
            {
                Navigate(typeof(Settings.ErrorReportingPage));
            }
            if (index == IndexLimitsTask)
            {
                Navigate(typeof(Settings.RuntimeLimitsPage));
            }
            if (index == IndexAllSettingsTask)
            {
                Navigate(typeof(Settings.AllSettingsPage));
            }
        }

        private void OnPHPSettingsItemTitleClick(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Navigate(typeof(Settings.AllSettingsPage));
        }

        private void OnPHPSetupItemClick(int index)
        {
            if (index == IndexRegisterPhpTask)
            {
                RegisterPHPWithIIS();
            }
            else if (index == IndexChangeVersionTask)
            {
                SelectPHPVersion();
            }
            else if (index == IndexCheckPhpInfoTask)
            {
                NavigateToPHPInfo();
            }
        }

        private void OnPHPSetupItemTitleClick(object sender, LinkLabelLinkClickedEventArgs e)
        {
            NavigateToPHPInfo();
        }

        private void OnViewRecommendationsLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (var dlg = new Setup.RecommendedConfigDialog(Module))
            {
                if (ShowDialog(dlg) == DialogResult.OK)
                {
                    Refresh();
                }
            }
        }

        internal void OpenPhysicalFile(string physicalPath)
        {
            try
            {
                if (!String.IsNullOrEmpty(physicalPath) &&
                    File.Exists(physicalPath))
                {
                    System.Diagnostics.Process.Start(physicalPath);
                }
                else
                {
                    ShowMessage(String.Format(CultureInfo.CurrentCulture, Resources.ErrorFileDoesNotExist, physicalPath), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ex, Resources.ResourceManager);
            }
        }

        private static LinkLabel PrepareNoFastCgiWarning()
        {
            var result = new LinkLabel();
            var sb = new System.Text.StringBuilder();

            sb.Append(Resources.WarningPHPConfigNoFastCgi);
            var fastCgiLearnMoreLinkStart = Resources.WarningPHPConfigNoFastCgi.Length;
            sb.Append(Resources.WarningFastCgiLearnMore);

            result.Text = sb.ToString();
            var fastCgiLearnMoreLink = new LinkLabel.Link(fastCgiLearnMoreLinkStart, Resources.WarningFastCgiLearnMore.Length, 0);
            result.Links.Add(fastCgiLearnMoreLink);

            result.LinkClicked += OnFastCgiLearnMoreLinkClicked;

            return result;
        }

        private static void PrepareOpenFileLink(LinkLabel linkLabel, string path, bool showLink)
        {
            linkLabel.Text = path;

            if (showLink && !String.IsNullOrEmpty(path))
            {
                if (linkLabel.Links.Count == 0)
                {
                    var link = new LinkLabel.Link(0, path.Length, path);
                    linkLabel.Links.Add(link);
                }
                else
                {
                    var link = linkLabel.Links[0];
                    link.Length = path.Length;
                    link.LinkData = path;
                }
            }
            else
            {
                if (linkLabel.Links.Count > 0)
                {
                    linkLabel.Links.Clear();
                }
            }
        }

        private LinkLabel PreparePHPConfigWarning()
        {
            var result = new LinkLabel();
            var sb = new System.Text.StringBuilder();

            sb.Append(Resources.WarningPHPConfigNotOptimal);
            var viewRecommendationsLinkStart = Resources.WarningPHPConfigNotOptimal.Length;
            sb.Append(Resources.WarningViewRecommendations);
            
            result.Text = sb.ToString();
            
            var fixItLink = new LinkLabel.Link(viewRecommendationsLinkStart, Resources.WarningViewRecommendations.Length, 0);
            result.Links.Add(fixItLink);

            result.LinkClicked += OnViewRecommendationsLinkClicked;
            
            return result;
        }

        private static Label PreparePHPRegistrationWarning(PHPRegistrationType registrationType)
        {
            Label result;

            if (registrationType == PHPRegistrationType.Cgi)
            {
                result = new Label {Text = Resources.WarningPHPConfigCgi};
            }
            else if (registrationType == PHPRegistrationType.Isapi)
            {
                result = new Label {Text = Resources.WarningPHPConfigIsapi};
            }
            else if (registrationType == PHPRegistrationType.None)
            {
                result = new Label {Text = Resources.WarningPHPConfigNotRegistered};
            }
            else if (registrationType == PHPRegistrationType.NoneNoFastCgi)
            {
                result = PrepareNoFastCgiWarning();
            }
            else
            {
                // Just in case
                result = new Label {Text = String.Empty};
            }

            return result;
        }

        protected override void Refresh()
        {
            GetSettings();
        }

        private void RegisterPHPWithIIS()
        {
            using (var dlg = new Setup.RegisterPHPDialog(Module, Connection.IsLocalConnection))
            {
                if (ShowDialog(dlg) == DialogResult.OK)
                {
                    Refresh();
                }
            }
        }

        private void SelectPHPVersion()
        {
            using (var dlg = new Setup.ChangeVersionDialog(Module))
            {
                if (ShowDialog(dlg) == DialogResult.OK)
                {
                    Refresh();
                }
            }
        }

        protected override bool ShowHelp()
        {
            return ShowOnlineHelp();
        }

        protected override bool ShowOnlineHelp()
        {
            return Helper.Browse(Globals.PHPPageOnlineHelp);
        }

        private void UpdatePageItemsState(PHPConfigInfo configInfo)
        {
            UpdatePHPSetupItem(configInfo);
            UpdatePHPSettingsItem(configInfo);
            UpdatePHPExtensionsItem(configInfo);

            PerformLayout();
        }

        private void UpdatePHPExtensionsItem(PHPConfigInfo configInfo)
        {
            var isPhpSetup = (configInfo != null && configInfo.RegistrationType == PHPRegistrationType.FastCgi);

            _phpExtensionItem.SetTitleState(isPhpSetup);
            if (isPhpSetup)
            {
                _enabledExtLabel.Text = String.Format(CultureInfo.CurrentCulture, Resources.PHPPageEnabledExtensions, configInfo.EnabledExtCount);
                _installedExtLabel.Text = String.Format(CultureInfo.CurrentCulture, Resources.PHPPageInstalledExtensions, configInfo.InstalledExtCount);
            }
            else
            {
                _enabledExtLabel.Text = Resources.PHPPageExtensionsNotAvailable;
                _installedExtLabel.Text = Resources.PHPPageExtensionsNotAvailable;
            }
            _phpExtensionItem.SetTaskState(IndexAllExtensionsTask, isPhpSetup);

            if (Connection.IsUserServerAdministrator)
            {
                _phpExtensionItem.SetTaskState(IndexAddExtensionTask, isPhpSetup);
            }
        }

        private void UpdatePHPSettingsItem(PHPConfigInfo configInfo)
        {
            var isPhpSetup = (configInfo != null && configInfo.RegistrationType == PHPRegistrationType.FastCgi);

            _phpSettingsItem.SetTitleState(isPhpSetup);
            if (isPhpSetup)
            {
                PrepareOpenFileLink(_configPathValueLabel, configInfo.PHPIniFilePath, Connection.IsLocalConnection);
                PrepareOpenFileLink(_errorLogValueLabel, configInfo.ErrorLog, Connection.IsLocalConnection);
            }
            else
            {
                PrepareOpenFileLink(_configPathValueLabel, Resources.PHPPagePHPNotAvailable, false);
                PrepareOpenFileLink(_errorLogValueLabel, Resources.PHPPagePHPNotAvailable, false);
            }
            _phpSettingsItem.SetTaskState(IndexErrorReportingTask, isPhpSetup);
            _phpSettingsItem.SetTaskState(IndexLimitsTask, isPhpSetup);
            _phpSettingsItem.SetTaskState(IndexAllSettingsTask, isPhpSetup);
        }

        private void UpdatePHPSetupItem(PHPConfigInfo configInfo)
        {
            bool isPhpSetup = (configInfo != null && configInfo.RegistrationType == PHPRegistrationType.FastCgi);

            _phpSetupItem.SetTitleState(isPhpSetup);
            _phpSetupItem.ClearWarning();

            if (isPhpSetup)
            {
                // Show warning about non optimal configuration if
                // PHP configuration is not optimal and
                // user is a server administrator.
                if (!configInfo.IsConfigOptimal && Connection.IsUserServerAdministrator)
                {
                    _phpSetupItem.SetWarning(PreparePHPConfigWarning());
                }
            }
            else if (configInfo != null)
            {
                // Show warning about PHP not being setup or setup incorrectly
                _phpSetupItem.SetWarning(PreparePHPRegistrationWarning(configInfo.RegistrationType));
            }
            else
            {
                // Show warning about failed IIS configuration
                var errorLabel = new Label {Text = Resources.ErrorFailedToGetConfiguration};
                _phpSetupItem.SetWarning(errorLabel);
            }
            _versionValueLabel.Text = isPhpSetup ? configInfo.Version : Resources.PHPPagePHPNotAvailable;
            _executableValueLabel.Text = isPhpSetup ? configInfo.Executable : Resources.PHPPagePHPNotAvailable;
            _handlerMappingValueLabel.Text = isPhpSetup ? GetHandlerMappingLabelText(configInfo.HandlerIsLocal) : Resources.PHPPagePHPNotAvailable;

            // Allow PHP registration only for server administrators
            if (configInfo != null && configInfo.RegistrationType != PHPRegistrationType.NoneNoFastCgi)
            {
                _phpSetupItem.SetTaskState(IndexRegisterPhpTask, Connection.IsUserServerAdministrator);
            }
            else
            {
                // If there is an error in IIS configuration then do not allow new registrations
                _phpSetupItem.SetTaskState(IndexRegisterPhpTask, false);
            }

            _phpSetupItem.SetTaskState(IndexChangeVersionTask, isPhpSetup);
            _phpSetupItem.SetTaskState(IndexCheckPhpInfoTask, isPhpSetup);
        }

    }
}