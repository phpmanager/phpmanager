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
using Web.Management.PHP.PHPExtensions;
using Web.Management.PHP.PHPSettings;
using Web.Management.PHP.PHPSetup;

namespace Web.Management.PHP
{

    [ModulePageIdentifier(Globals.PHPPageIdentifier)]
    internal sealed class PHPPage : ModulePage
    {
        // Summary labels
        private Label _enabledExtLabel;
        private Label _extPathValueLabel;
        private Label _extPathNameLabel;
        private Label _recommendedConfigLabel;
        private Label _configPathValueLabel;
        private Label _configPathNameLabel;
        private Label _executableValueLabel;
        private Label _executableNameLabel;
        private Label _versionValueLabel;        
        private Label _versionNameLabel;

        private PHPPageItemControl _phpExtensionItem;
        private PHPPageItemControl _phpSettingsItem;
        private PHPPageItemControl _phpSetupItem;

        private const int PHPSetupIndex = 1;
        private const int PHPSetupTitleIndex = 2;
        private const int PHPSettingsIndex = 3;
        private const int PHPSettingsTitleIndex = 4;
        private const int PHPExtensionsIndex = 5;
        private const int PHPExtensionsTitleIndex = 6;
        private int[] _parameters;

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

        private void ExecuteParameters()
        {
            int actionItem = _parameters[0];
            int index = _parameters[1];

            if (actionItem == PHPSetupIndex)
            {
                OnPHPSetupItemClick(index);
            }
            else if (actionItem == PHPSettingsIndex)
            {
                OnPHPSettingsItemClick(index);
            }
            else if (actionItem == PHPSetupTitleIndex)
            {
                OnPHPSetupItemTitleClick(this, null);
            }
            else if (actionItem == PHPSettingsTitleIndex)
            {
                OnPHPSettingsItemTitleClick(this, null);
            }
        }

        private void ExecuteSiteAction(int actionIndex, int index)
        {
            string siteName = GetSiteName();

            if (!String.IsNullOrEmpty(siteName))
            {
                ManagementConfigurationPath configPath = ManagementConfigurationPath.CreateSiteConfigurationPath(siteName);
                INavigationService ns = (INavigationService)GetService(typeof(INavigationService));
                ns.Navigate(Connection, configPath, typeof(PHPPage), new int[] { actionIndex, index });

            }
        }

        private string GetSiteName()
        {
            using (SelectSiteDialog dlg = new SelectSiteDialog(Module))
            {
                if (ShowDialog(dlg) == DialogResult.OK)
                {
                    return dlg.SiteName;
                }
            }

            return null;
        }

        private string GetSiteUrl()
        {
            using (SelectDomainDialog dlg = new SelectDomainDialog(this.Module, this.Connection, String.Empty))
            {
                if (ShowDialog(dlg) == DialogResult.OK)
                {
                    return dlg.SiteDomain;
                }
            }

            return null;
        }

        protected override void Initialize(object navigationData)
        {
            base.Initialize(navigationData);

            int[] parameters = navigationData as int[];
            if (parameters != null)
            {
                _parameters = parameters;
            }
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
            _versionNameLabel.Text = "PHP version:";
            _versionValueLabel = new Label();

            _executableNameLabel = new Label();
            _executableNameLabel.Text = "PHP executable:";
            _executableValueLabel = new Label();

            _configPathNameLabel = new Label();
            _configPathNameLabel.Text = "Configuration file:";
            _configPathValueLabel = new Label();
            _recommendedConfigLabel = new Label();

            _extPathNameLabel = new Label();
            _extPathNameLabel.Text = "Extensions path:";
            _extPathValueLabel = new Label();
            _enabledExtLabel = new Label();

            //
            // PHPSetup
            //
            _phpSetupItem = new PHPPageItemControl();
            _phpSetupItem.RightToLeftLayout = this.RightToLeftLayout;
            _phpSetupItem.RightToLeft = this.RightToLeft;
            _phpSetupItem.TitleClick += new LinkLabelLinkClickedEventHandler(OnPHPSetupItemTitleClick);
            _phpSetupItem.Title = Resources.PHPSetupPageTitle;
            _phpSetupItem.TitleFont = titleFont;
            _phpSetupItem.Image = Resources.PHPSetup32;

            _phpSetupItem.AddInfoRow(_versionNameLabel, _versionValueLabel);
            _phpSetupItem.AddInfoRow(_executableNameLabel, _executableValueLabel);
            _phpSetupItem.AddTask(OnPHPSetupItemClick,
                                    Resources.PHPSetupRegisterPHPTask,
                                    Resources.PHPSetupChangeVersion,
                                    Resources.PHPSetupCheckPHPInfoTask);

            Controls.Add(_phpSetupItem);

            //
            // PHP Settings
            //
            _phpSettingsItem = new PHPPageItemControl();
            _phpSettingsItem.RightToLeftLayout = this.RightToLeftLayout;
            _phpSettingsItem.RightToLeft = this.RightToLeft;
            _phpSettingsItem.TitleClick += new LinkLabelLinkClickedEventHandler(OnPHPSettingsItemTitleClick);
            _phpSettingsItem.Title = Resources.PHPSettingsTitle;
            _phpSettingsItem.TitleFont = titleFont;
            _phpSettingsItem.Image = Resources.PHPSettings32;

            _phpSettingsItem.AddInfoRow(_configPathNameLabel, _configPathValueLabel);
            _phpSettingsItem.AddSpanRow(_recommendedConfigLabel);
            _phpSettingsItem.AddTask(OnPHPSettingsItemClick,
                                    Resources.PHPSettingsApplyRecommendedTask,
                                    Resources.PHPSettingsErrorReportingTask,
                                    Resources.PHPSettingsAllSettingsTask);

            Controls.Add(_phpSettingsItem);

            //
            // PHP Extensions
            //
            _phpExtensionItem = new PHPPageItemControl();
            _phpExtensionItem.RightToLeftLayout = this.RightToLeftLayout;
            _phpExtensionItem.RightToLeft = this.RightToLeft;
            _phpExtensionItem.TitleClick += new LinkLabelLinkClickedEventHandler(OnPHPExtensionItemTitleClick);
            _phpExtensionItem.Title = Resources.PHPExtensionsPageTitle;
            _phpExtensionItem.TitleFont = titleFont;
            _phpExtensionItem.Image = Resources.PHPExtensions32;

            _phpExtensionItem.AddInfoRow(_extPathNameLabel, _extPathValueLabel);
            _phpExtensionItem.AddSpanRow(_enabledExtLabel);
            _phpExtensionItem.AddTask(OnPHPExtensionItemClick,
                                    Resources.PHPExtensionEnableDisable,
                                    Resources.PHPExtensionConfigure);
            
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

                if (_parameters != null)
                {
                    BeginInvoke(new MethodInvoker(ExecuteParameters));
                }
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
                Navigate(typeof(PHPPage), new object());
            }
            else if (index == 1)
            {
                Navigate(typeof(PHPPage));
            }
        }

        private void OnPHPExtensionItemTitleClick(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Navigate(typeof(PHPExtensionsPage));
        }

        // TODO: Change the following methods:

        private void OnPHPSettingsItemClick(int index)
        {
            if (index == 2)
            {
                Navigate(typeof(PHPSettingsPage), new object());
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
                if (Connection.ConfigurationPath.PathType == ConfigurationPathType.Server)
                {
                    ExecuteSiteAction(PHPSetupIndex, index);
                    return;
                }

                string siteBinding = GetSiteUrl();
                if (!String.IsNullOrEmpty(siteBinding)) {
                    Navigate(typeof(PHPInfoPage), siteBinding);
                }
            }
        }

        private void OnPHPSetupItemTitleClick(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Connection.ConfigurationPath.PathType == ConfigurationPathType.Server)
            {
                ExecuteSiteAction(PHPSetupTitleIndex, 0);

                return;
            }

            string siteBinding = GetSiteUrl();

            if (!String.IsNullOrEmpty(siteBinding)) {
                Navigate(typeof(PHPInfoPage), siteBinding);
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
            PHPConfigInfo configInfo = Module.Proxy.GetPHPConfigInfo();

            if (configInfo == null)
            {
                _versionValueLabel.Text = "None";
                _executableValueLabel.Text = "None";
                _configPathValueLabel.Text = "None";
                _extPathValueLabel.Text = "None";
            }
            else
            {
                _versionValueLabel.Text = configInfo.Version;
                _executableValueLabel.Text = configInfo.ScriptProcessor;
                _configPathValueLabel.Text = @"C:\PHP\5211NTS\php.ini";
                _recommendedConfigLabel.Text = "Recommended configuration is set";
                _extPathValueLabel.Text = @"C:\PHP\5211NTS\ext\";
                _enabledExtLabel.Text = "There are 6 extensions enabled";
            }

        }

    }
}