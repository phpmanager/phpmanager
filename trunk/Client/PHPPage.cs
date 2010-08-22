//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.Web.Management.Client;
using Microsoft.Web.Management.Client.Win32;
using Web.Management.PHP.Config;
using System.ComponentModel;

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
        private Label _errorLogValueLabel;
        private Label _configPathValueLabel;
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

        private string GetSiteUrlAndName(out string siteName)
        {
            using (Setup.SelectSiteDomainDialog dlg = new Setup.SelectSiteDomainDialog(this.Module, this.Connection))
            {
                if (ShowDialog(dlg) == DialogResult.OK)
                {
                    siteName = dlg.SiteName;
                    return dlg.DomainName;
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
            _configPathValueLabel = new Label();

            _errorLogNameLabel = new Label();
            _errorLogNameLabel.Text = Resources.PHPPageErrorLog;
            _errorLogValueLabel = new Label();

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
            _phpSettingsItem.AddTask(OnPHPSettingsItemClick,
                                    Resources.PHPSettingsItemErrorReportingTask,
                                    Resources.PHPSettingsItemLimitsTask,
                                    Resources.PHPSettingsItemAllSettingsTask);

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
            _phpExtensionItem.AddTask(OnPHPExtensionItemClick,
                                    Resources.PHPExtensionItemEnableTask);

            Padding = new Padding(0, 12, 0, 0);
            Controls.Add(_phpExtensionItem);

            // Update the information summaries for each PHPPageItemControl
            GetSettings();

            ResumeLayout(true);
        }

        protected override void OnActivated(bool initialActivation)
        {
            base.OnActivated(initialActivation);
            
            if (initialActivation)
            {
                InitializeUI();
            }
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
                string siteName = null;
                string siteUrl = GetSiteUrlAndName(out siteName);
                if (!String.IsNullOrEmpty(siteUrl))
                {
                    Navigate(typeof(Setup.PHPInfoPage), new string[] { siteUrl, siteName });
                }
            }
        }

        private void OnPHPSetupItemTitleClick(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string siteName = null;
            string siteUrl = GetSiteUrlAndName(out siteName);
            if (!String.IsNullOrEmpty(siteUrl))
            {
                Navigate(typeof(Setup.PHPInfoPage), new string[] { siteUrl, siteName });
            }
        }

        protected override void Refresh()
        {
            GetSettings();
        }

        private void RegisterPHPWithIIS()
        {
            using (Setup.RegisterPHPDialog dlg = new Setup.RegisterPHPDialog(this.Module))
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void GetSettings()
        {
            StartAsyncTask(Resources.AllSettingsPageGettingSettings, OnGetSettings, OnGetSettingsCompleted);
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
                    _configPathValueLabel.Text = configInfo.PHPIniFilePath;
                    _errorLogValueLabel.Text = configInfo.ErrorLog;
                    _enabledExtLabel.Text = String.Format(CultureInfo.CurrentCulture, Resources.PHPPageEnabledExtensions, configInfo.EnabledExtCount);
                    _installedExtLabel.Text = String.Format(CultureInfo.CurrentCulture, Resources.PHPPageInstalledExtensions, configInfo.InstalledExtCount);

                    if (Connection.Scope == Microsoft.Web.Management.Server.ManagementScope.Server)
                    {
                        _phpSetupItem.SetTaskState(IndexRegisterPHPTask, true);
                    }
                    else
                    {
                        _phpSetupItem.SetTaskState(IndexRegisterPHPTask, false);
                    }
                    _phpSetupItem.SetTaskState(IndexChangeVersionTask, true);
                    _phpSetupItem.SetTaskState(IndexCheckPHPInfoTask, true);
                    _phpSettingsItem.SetTaskState(IndexErrorReportingTask, true);
                    _phpSettingsItem.SetTaskState(IndexLimitsTask, true);
                    _phpSettingsItem.SetTaskState(IndexAllSettingsTask, true);
                    _phpExtensionItem.SetTaskState(IndexAllExtensionsTask, true);

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
                _configPathValueLabel.Text = Resources.PHPPagePHPNotAvailable;
                _errorLogValueLabel.Text = Resources.PHPPagePHPNotAvailable;
                _enabledExtLabel.Text = Resources.PHPPageExtensionsNotAvailable;
                _installedExtLabel.Text = Resources.PHPPageExtensionsNotAvailable;
 
                _phpSetupItem.SetTaskState(IndexChangeVersionTask, false);
                _phpSetupItem.SetTaskState(IndexCheckPHPInfoTask, false);
                _phpSettingsItem.SetTaskState(IndexErrorReportingTask, false);
                _phpSettingsItem.SetTaskState(IndexLimitsTask, false);
                _phpSettingsItem.SetTaskState(IndexAllSettingsTask, false);
                _phpExtensionItem.SetTaskState(IndexAllExtensionsTask, false);
            }

        }

    }
}