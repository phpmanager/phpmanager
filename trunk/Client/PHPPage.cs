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

        private const int IndexRegisterPHPTask = 0;
        private const int IndexChangeVersionTask = 1;
        private const int IndexCheckPHPInfoTask = 2;
        private const int IndexErrorReportingTask = 0;
        private const int IndexLimitsTask = 1;
        private const int IndexAllSettingsTask = 2;
        private const int IndexAllExtensionsTask = 0;

        // Summary labels
        private Label _enabledExtLabel;
        private Label _installedExtLabel;
        private Label _errorLogNameLabel;
        private LinkLabel _errorLogValueLabel;
        private LinkLabel _configPathValueLabel;
        private Label _configPathNameLabel;
        private Label _executableValueLabel;
        private Label _executableNameLabel;
        private Label _versionValueLabel;
        private Label _versionNameLabel;
        private LinkLabel _warningPHPConfiguration;

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

        private void GetSettings()
        {
            StartAsyncTask(Resources.AllSettingsPageGettingSettings, OnGetSettings, OnGetSettingsCompleted);
        }

        private string GetSiteUrlAndName(out string siteName)
        {
            using (Setup.SelectSiteAndUrlDialog dlg = new Setup.SelectSiteAndUrlDialog(this.Module, this.Connection))
            {
                if (ShowDialog(dlg) == DialogResult.OK)
                {
                    siteName = dlg.SiteName;
                    return dlg.SelectedUrl;
                }
            }

            siteName = string.Empty;
            return null;
        }

        private void InitializeUI()
        {
            SuspendLayout();

            IManagementUIService uiService = (IManagementUIService)GetService(typeof(IManagementUIService));

            Font titleFont = (Font)uiService.Styles["PageHeaderTitleFont"];

            //
            // Summary labels
            //
            _versionNameLabel = new Label();
            _versionNameLabel.Text = Resources.PHPPageVersion;
            _versionValueLabel = new Label();

            _executableNameLabel = new Label();
            _executableNameLabel.Text = Resources.PHPPageExecutable;
            _executableValueLabel = new Label();

            _configPathNameLabel = new Label();
            _configPathNameLabel.Text = Resources.PHPPageConfigurationFile;
            _configPathValueLabel = new LinkLabel();
            _configPathValueLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(OnPathLinkLabelLinkClicked);

            _errorLogNameLabel = new Label();
            _errorLogNameLabel.Text = Resources.PHPPageErrorLog;
            _errorLogValueLabel = new LinkLabel();
            _errorLogValueLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(OnPathLinkLabelLinkClicked);

            _warningPHPConfiguration = PreparePHPConfigWarning();

            _enabledExtLabel = new Label();
            _installedExtLabel = new Label();

            //
            // PHPSetup
            //
            _phpSetupItem = new PHPPageItemControl();
            _phpSetupItem.RightToLeftLayout = this.RightToLeftLayout;
            _phpSetupItem.RightToLeft = this.RightToLeft;
            _phpSetupItem.TitleClick += new LinkLabelLinkClickedEventHandler(OnPHPSetupItemTitleClick);
            _phpSetupItem.Title = Resources.PHPSetupItemTitle;
            _phpSetupItem.TitleFont = titleFont;
            _phpSetupItem.Image = Resources.PHPSetup32;

            _phpSetupItem.AddInfoRow(_versionNameLabel, _versionValueLabel);
            _phpSetupItem.AddInfoRow(_executableNameLabel, _executableValueLabel);
            _phpSetupItem.AddTask(OnPHPSetupItemClick,
                                    Resources.PHPSetupItemRegisterPHPTask,
                                    Resources.PHPSetupItemChangeVersionTask,
                                    Resources.PHPSetupItemCheckPHPInfoTask);
            _phpSetupItem.AddWarning(_warningPHPConfiguration);

            Controls.Add(_phpSetupItem);

            //
            // PHP Settings
            //
            _phpSettingsItem = new PHPPageItemControl();
            _phpSettingsItem.RightToLeftLayout = this.RightToLeftLayout;
            _phpSettingsItem.RightToLeft = this.RightToLeft;
            _phpSettingsItem.TitleClick += new LinkLabelLinkClickedEventHandler(OnPHPSettingsItemTitleClick);
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
            _phpExtensionItem = new PHPPageItemControl();
            _phpExtensionItem.RightToLeftLayout = this.RightToLeftLayout;
            _phpExtensionItem.RightToLeft = this.RightToLeft;
            _phpExtensionItem.TitleClick += new LinkLabelLinkClickedEventHandler(OnPHPExtensionItemTitleClick);
            _phpExtensionItem.Title = Resources.PHPExtensionsItemTitle;
            _phpExtensionItem.TitleFont = titleFont;
            _phpExtensionItem.Image = Resources.PHPExtensions32;

            _phpExtensionItem.AddSpanRow(_enabledExtLabel);
            _phpExtensionItem.AddSpanRow(_installedExtLabel);
            if (Connection.IsUserServerAdministrator)
            {
                _phpExtensionItem.AddTask(OnPHPExtensionItemClick,
                                        Resources.PHPExtensionItemEnableTask);
            }
            else
            {
                _phpExtensionItem.AddTask(OnPHPExtensionItemClick,
                                        Resources.PHPExtensionItemReadOnlyEnableTask);
            }

            Padding = new Padding(0, 12, 0, 0);
            Controls.Add(_phpExtensionItem);

            // Update the information summaries for each PHPPageItemControl
            GetSettings();

            ResumeLayout(true);
        }

        private void NavigateToPHPInfo()
        {
            string siteName = null;
            string siteUrl = GetSiteUrlAndName(out siteName);
            if (!String.IsNullOrEmpty(siteUrl))
            {
                Navigate(typeof(Setup.PHPInfoPage), new string[] { siteUrl, siteName });
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

        private void OnGetSettings(object sender, DoWorkEventArgs e)
        {
            e.Result = Module.Proxy.GetPHPConfigInfo();
        }

        private void OnGetSettingsCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bool isSuccess = false;

            try
            {
                PHPConfigInfo configInfo = (PHPConfigInfo)e.Result;
                if (configInfo != null)
                {
                    _versionValueLabel.Text = configInfo.Version;
                    _executableValueLabel.Text = configInfo.ScriptProcessor;
                    PrepareOpenFileLink(_configPathValueLabel, configInfo.PHPIniFilePath, Connection.IsLocalConnection);
                    PrepareOpenFileLink(_errorLogValueLabel, configInfo.ErrorLog, Connection.IsLocalConnection);
                    _enabledExtLabel.Text = String.Format(CultureInfo.CurrentCulture, Resources.PHPPageEnabledExtensions, configInfo.EnabledExtCount);
                    _installedExtLabel.Text = String.Format(CultureInfo.CurrentCulture, Resources.PHPPageInstalledExtensions, configInfo.InstalledExtCount);

                    _phpSetupItem.SetTitleState(true);
                    // Allow PHP registration only for server administrators
                    if (Connection.IsUserServerAdministrator)
                    {
                        _phpSetupItem.SetTaskState(IndexRegisterPHPTask, true);
                    }
                    else
                    {
                        _phpSetupItem.SetTaskState(IndexRegisterPHPTask, false);
                    }
                    // Site administrators can still change PHP version of check phpinfo()
                    _phpSetupItem.SetTaskState(IndexChangeVersionTask, true);
                    _phpSetupItem.SetTaskState(IndexCheckPHPInfoTask, true);
                    
                    // For site administrators these tasks are still available but the pages for those tasks will be read-only
                    _phpSettingsItem.SetTitleState(true);
                    _phpSettingsItem.SetTaskState(IndexErrorReportingTask, true);
                    _phpSettingsItem.SetTaskState(IndexLimitsTask, true);
                    _phpSettingsItem.SetTaskState(IndexAllSettingsTask, true);

                    // For site administrators the extension list is still available but is read only
                    _phpExtensionItem.SetTitleState(true);
                    _phpExtensionItem.SetTaskState(IndexAllExtensionsTask, true);

                    _phpSetupItem.ShowWarning(true);

                    isSuccess = true;
                }
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ex, Resources.ResourceManager);
            }

            if (!isSuccess)
            {
                _versionValueLabel.Text = Resources.PHPPagePHPNotRegistered;
                _executableValueLabel.Text = Resources.PHPPagePHPNotAvailable;
                PrepareOpenFileLink(_configPathValueLabel, Resources.PHPPagePHPNotAvailable, false);
                PrepareOpenFileLink(_errorLogValueLabel, Resources.PHPPagePHPNotAvailable, false);
                
                _enabledExtLabel.Text = Resources.PHPPageExtensionsNotAvailable;
                _installedExtLabel.Text = Resources.PHPPageExtensionsNotAvailable;

                // Disable all the tasks and the title link. Only the PHP registration task is still enabled
                _phpSetupItem.SetTitleState(false);
                _phpSetupItem.SetTaskState(IndexChangeVersionTask, false);
                _phpSetupItem.SetTaskState(IndexCheckPHPInfoTask, false);

                // Disable all the tasks and the title link
                _phpSettingsItem.SetTitleState(false);
                _phpSettingsItem.SetTaskState(IndexErrorReportingTask, false);
                _phpSettingsItem.SetTaskState(IndexLimitsTask, false);
                _phpSettingsItem.SetTaskState(IndexAllSettingsTask, false);

                // Disalbe all the tasks and the title link
                _phpExtensionItem.SetTitleState(false);
                _phpExtensionItem.SetTaskState(IndexAllExtensionsTask, false);
            }

            PerformLayout();
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            if (!this.Visible || this.Height == 0)
            {
                return;
            }

            Control.ControlCollection controls = this.Controls;

            Size clientSize = ClientSize;

            int width = clientSize.Width - Padding.Horizontal - 12;

            Size proposedSize = new Size(width, Int32.MaxValue);

            int top = Padding.Top + AutoScrollPosition.Y;
            for (int i = 0; i < controls.Count; i++)
            {
                Control ctl = controls[i];
                Size size = ctl.GetPreferredSize(proposedSize);
                ctl.SetBounds(Padding.Left,
                              top,
                              size.Width,
                              size.Height);

                top += ctl.Height;
            }

            if (top >= this.ClientSize.Height)
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
            if (index == IndexRegisterPHPTask)
            {
                RegisterPHPWithIIS();
            }
            else if (index == IndexChangeVersionTask)
            {
                SelectPHPVersion();
            }
            else if (index == IndexCheckPHPInfoTask)
            {
                NavigateToPHPInfo();
            }
        }

        private void OnPHPSetupItemTitleClick(object sender, LinkLabelLinkClickedEventArgs e)
        {
            NavigateToPHPInfo();
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

        private static void PrepareOpenFileLink(LinkLabel linkLabel, string path, bool showLink)
        {
            linkLabel.Text = path;

            if (showLink)
            {
                if (linkLabel.Links.Count == 0)
                {
                    LinkLabel.Link link = new LinkLabel.Link(0, path.Length, path);
                    linkLabel.Links.Add(link);
                }
                else
                {
                    LinkLabel.Link link = linkLabel.Links[0];
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
            LinkLabel result = new LinkLabel();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append(Resources.WarningPHPConfigNotOptimal);
            int fixItLinkStart = Resources.WarningPHPConfigNotOptimal.Length;
            sb.Append(Resources.WarningFixIt + Resources.WarningOr);
            int ignoreLinkStart = fixItLinkStart + Resources.WarningFixIt.Length + Resources.WarningOr.Length;
            sb.Append(Resources.WarningIgnore + Resources.WarningQuestionMark);
            
            result.Text = sb.ToString();
            
            LinkLabel.Link fixItLink = new LinkLabel.Link(fixItLinkStart, Resources.WarningFixIt.Length, 0);
            LinkLabel.Link ignoreLink = new LinkLabel.Link(ignoreLinkStart, Resources.WarningIgnore.Length, 1);
            result.Links.Add(fixItLink);
            result.Links.Add(ignoreLink);
            
            return result;
        }

        protected override void Refresh()
        {
            GetSettings();
        }

        private void RegisterPHPWithIIS()
        {
            using (Setup.RegisterPHPDialog dlg = new Setup.RegisterPHPDialog(this.Module, Connection.IsLocalConnection))
            {
                if (ShowDialog(dlg) == DialogResult.OK)
                {
                    GetSettings();
                    return;
                }
            }
        }

        private void SelectPHPVersion()
        {
            using (Setup.ChangeVersionDialog dlg = new Setup.ChangeVersionDialog(this.Module))
            {
                if (ShowDialog(dlg) == DialogResult.OK)
                {
                    GetSettings();
                    return;
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

    }
}