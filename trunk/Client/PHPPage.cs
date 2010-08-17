//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.Management.Client;
using Microsoft.Web.Management.Client.Win32;
using Microsoft.Web.Management.Server;
using Web.Management.PHP.Config;
using Web.Management.PHP.Extensions;
using Web.Management.PHP.Settings;
using Web.Management.PHP.Setup;

namespace Web.Management.PHP
{

    [ModulePageIdentifier(Globals.PHPPageIdentifier)]
    internal sealed class PHPPage : ModulePage
    {
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
            using (SelectSiteDomainDialog dlg = new SelectSiteDomainDialog(this.Module, this.Connection))
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
                                    Resources.PHPSettingsItemSessionTask,
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
            
            // Update the information summaries for each PHPPageItemControl
            UpdateInfo();

            Padding = new Padding(0, 12, 0, 0);
            Controls.Add(_phpExtensionItem);

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
            if (index == 0)
            {
                Navigate(typeof(PHPExtensionsPage));
            }
        }

        private void OnPHPExtensionItemTitleClick(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Navigate(typeof(PHPExtensionsPage));
        }

        private void OnPHPSettingsItemClick(int index)
        {
            if (index == 0)
            {
                Navigate(typeof(ErrorReportingPage));
            }
            if (index == 3)
            {
                Navigate(typeof(PHPSettingsPage));
            }
        }

        private void OnPHPSettingsItemTitleClick(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Navigate(typeof(PHPSettingsPage));
        }

        private void OnPHPSetupItemClick(int index)
        {
            if (index == 0)
            {
                RegisterPHPWithIIS();
            }
            else if (index == 1)
            {
                SelectPHPVersion();
            }
            else if (index == 2)
            {
                string siteName = null;
                string siteUrl = GetSiteUrlAndName(out siteName);
                if (!String.IsNullOrEmpty(siteUrl))
                {
                    Navigate(typeof(PHPInfoPage), new string[] { siteUrl, siteName });
                }
            }
        }

        private void OnPHPSetupItemTitleClick(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string siteName = null;
            string siteUrl = GetSiteUrlAndName(out siteName);
            if (!String.IsNullOrEmpty(siteUrl))
            {
                Navigate(typeof(PHPInfoPage), new string[] { siteUrl, siteName });
            }
        }

        protected override void Refresh()
        {
            UpdateInfo();
        }

        private void RegisterPHPWithIIS()
        {
            using (RegisterPHPDialog dlg = new RegisterPHPDialog(this.Module))
            {
                if (ShowDialog(dlg) == DialogResult.OK)
                {
                    UpdateInfo();
                    return;
                }
            }
        }

        private void SelectPHPVersion()
        {
            using (ChangeVersionDialog dlg = new ChangeVersionDialog(this.Module))
            {
                if (ShowDialog(dlg) == DialogResult.OK)
                {
                    UpdateInfo();
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

        private void UpdateInfo()
        {
            try
            {
                PHPConfigInfo configInfo = Module.Proxy.GetPHPConfigInfo();
                if (configInfo == null)
                {
                    _versionValueLabel.Text = Resources.PHPPageNone;
                    _executableValueLabel.Text = Resources.PHPPageNone;
                    _configPathValueLabel.Text = Resources.PHPPageNone;
                    _errorLogValueLabel.Text = Resources.PHPPageNone;
                    _enabledExtLabel.Text = Resources.PHPPageNone;
                    _installedExtLabel.Text = Resources.PHPPageNone;
                }
                else
                {
                    _versionValueLabel.Text = configInfo.Version;
                    _executableValueLabel.Text = configInfo.ScriptProcessor;
                    _configPathValueLabel.Text = configInfo.PHPIniFilePath;
                    _errorLogValueLabel.Text = configInfo.ErrorLog;
                    _enabledExtLabel.Text = String.Format(Resources.PHPPageEnabledExtensions, configInfo.EnabledExtCount);
                    _installedExtLabel.Text = String.Format(Resources.PHPPageInstalledExtensions, configInfo.InstalledExtCount);
                }
            }
            catch(Exception ex)
            {
                DisplayErrorMessage(ex, Resources.ResourceManager);
            }
        }

    }
}