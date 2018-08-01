//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

//#define VSDesigner

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.Web.Management.Client;
using Microsoft.Web.Management.Client.Win32;
using Microsoft.Web.Management.Server;

namespace Web.Management.PHP.Setup
{

    internal sealed class SelectSiteAndUrlDialog :
#if VSDesigner
        Form
#else
        TaskForm
#endif
    {
        private readonly Guid _preferenceServiceGuid = new Guid("68a2a947-eeb6-40d9-9e5a-977bf7753bce");
        private const string ServerSelectSitePreferenceKey = "ServerSitePreferenceKey";
        private const string ServerSelectUrlPreferenceKey = "ServerUrlPreferenceKey";

        private readonly PHPModule _module;
        private readonly Connection _connection;
        private string _preferenceUrl;
        private string _preferenceSite;
        private PreferencesStore _store;

        private ManagementPanel _contentPanel;
        private ComboBox _urlsComboBox;
        private ComboBox _sitesComboBox;
        private Label _urlsLabel;
        private Label _sitesLabel;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private readonly IContainer components = null;

        public SelectSiteAndUrlDialog(PHPModule module, Connection connection)
            : base(module)
        {
            _module = module;
            _connection = connection;

            InitializeComponent();
            InitializeUI();

            Update();
        }

        protected override bool CanAccept
        {
            get
            {
                return _urlsComboBox.SelectedIndex >= 0;
            }
        }

        protected override bool CanShowHelp
        {
            get
            {
                return true;
            }
        }

        private PreferencesStore PreferencesStore
        {
            get
            {
                if (_store == null)
                {
                    var prefService = (IPreferencesService)GetService(typeof(IPreferencesService));
                    if (prefService != null)
                    {
                        _store = prefService.GetPreferencesStore(_preferenceServiceGuid);
                    }
                }
                return _store;
            }
        }

        public string RelativePath
        {
            get
            {
                if (_connection.ConfigurationPath.PathType != ConfigurationPathType.Server)
                {
                    return _connection.ConfigurationPath.GetEffectiveConfigurationPath(ManagementScope.Site);
                }
                return String.Empty;
            }
        }

        public string SelectedUrl
        {
            get
            {
                return _urlsComboBox.SelectedItem as string;
            }
        }

        public string SiteName
        {
            get
            {
                return _sitesComboBox.SelectedItem as string;
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private static string GetSitePreferenceKey(Connection connection)
        {
            return connection.ConfigurationPath.SiteName + "/" + connection.ConfigurationPath.GetEffectiveConfigurationPath(ManagementScope.Site);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            _urlsComboBox = new System.Windows.Forms.ComboBox();
            _sitesComboBox = new System.Windows.Forms.ComboBox();
            _urlsLabel = new System.Windows.Forms.Label();
            _sitesLabel = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // _domainsComboBox
            // 
            _urlsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _urlsComboBox.FormattingEnabled = true;
            _urlsComboBox.Location = new System.Drawing.Point(0, 90);
            _urlsComboBox.Name = "_domainsComboBox";
            _urlsComboBox.Size = new System.Drawing.Size(385, 21);
            _urlsComboBox.TabIndex = 3;
            _urlsComboBox.SelectedIndexChanged += new System.EventHandler(OnUrlsComboBoxSelectedIndexChanged);
            // 
            // _sitesComboBox
            // 
            _sitesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _sitesComboBox.FormattingEnabled = true;
            _sitesComboBox.Location = new System.Drawing.Point(0, 25);
            _sitesComboBox.Name = "_sitesComboBox";
            _sitesComboBox.Size = new System.Drawing.Size(385, 21);
            _sitesComboBox.TabIndex = 1;
            _sitesComboBox.SelectedIndexChanged += new System.EventHandler(OnSitesComboBoxSelectedIndexChanged);
            // 
            // _domainsLabel
            // 
            _urlsLabel.Location = new System.Drawing.Point(0, 65);
            _urlsLabel.Name = "_domainsLabel";
            _urlsLabel.Size = new System.Drawing.Size(385, 22);
            _urlsLabel.TabIndex = 2;
            _urlsLabel.Text = Resources.SelectSiteAndUrlDialogSelectAUrl;
            // 
            // _sitesLabel
            // 
            _sitesLabel.Location = new System.Drawing.Point(0, 0);
            _sitesLabel.Name = "_sitesLabel";
            _sitesLabel.Size = new System.Drawing.Size(385, 22);
            _sitesLabel.TabIndex = 0;
            _sitesLabel.Text = Resources.SelectSiteAndUrlDialogSelectASite;
            // 
            // SelectSiteDomainDialog
            // 
            ClientSize = new System.Drawing.Size(414, 192);
            Controls.Add(_sitesComboBox);
            Controls.Add(_sitesLabel);
            Controls.Add(_urlsLabel);
            Controls.Add(_urlsComboBox);
            Name = "SelectSiteDomainDialog";
            ResumeLayout(false);
#if VSDesigner
            PerformLayout();
#endif
        }

        #endregion

        public void InitializeUI()
        {
            _contentPanel = new ManagementPanel();
            _contentPanel.SuspendLayout();

            _contentPanel.Location = new System.Drawing.Point(0, 0);
            _contentPanel.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(_urlsLabel);
            _contentPanel.Controls.Add(_urlsComboBox);
            _contentPanel.Controls.Add(_sitesLabel);
            _contentPanel.Controls.Add(_sitesComboBox);

            _contentPanel.ResumeLayout(false);
            _contentPanel.PerformLayout();

            Text = Resources.SelectSiteAndUrlDialogTitle;

            SetContent(_contentPanel);
            UpdateTaskForm();
        }

        private void LoadServerPreferences(PreferencesStore store)
        {
            _preferenceSite = store.GetValue(ServerSelectSitePreferenceKey, String.Empty);
            _preferenceUrl = store.GetValue(ServerSelectUrlPreferenceKey, String.Empty);
        }

        private void LoadSitePreferences(PreferencesStore store)
        {
            _preferenceUrl = store.GetValue(GetSitePreferenceKey(_connection), String.Empty);
        }

        protected override void OnAccept()
        {
            if (_connection.ConfigurationPath.PathType == ConfigurationPathType.Server)
            {
                SaveServerPreferences(PreferencesStore);
            }
            else
            {
                SaveSitePreferences(PreferencesStore);
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_connection.ConfigurationPath.PathType == ConfigurationPathType.Server)
            {
                LoadServerPreferences(PreferencesStore);
                StartAsyncTask(OnSiteWorkerDoWork, OnSiteWorkerDoWorkCompleted);
            }
            else
            {
                LoadSitePreferences(PreferencesStore);
                _sitesComboBox.Items.Add(_connection.ConfigurationPath.SiteName);
                _sitesComboBox.SelectedIndex = 0;
                _sitesComboBox.Enabled = false;
            }
        }

        private void OnSitesComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            StartAsyncTask(OnUrlsWorkerDoWork, OnUrlsWorkerDoWorkCompleted, null, _sitesComboBox.SelectedItem);
        }

        private void OnSiteWorkerDoWork(object sender, DoWorkEventArgs e) 
        {
            e.Result = _module.Proxy.GetSites();
        }

        private void OnSiteWorkerDoWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _sitesComboBox.BeginUpdate();
            _sitesComboBox.SuspendLayout();
            
            try
            {
                _sitesComboBox.Items.Clear();

                var sites = e.Result as ArrayList;
                sites.Sort();
                foreach (string siteName in sites)
                {
                    _sitesComboBox.Items.Add(siteName);
                }

                if (!String.IsNullOrEmpty(_preferenceSite))
                {
                    var selectedIndex = _sitesComboBox.Items.IndexOf(_preferenceSite);
                    if (selectedIndex >= 0)
                    {
                        _sitesComboBox.SelectedIndex = selectedIndex;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ex, Resources.ResourceManager);
            }
            finally
            {
                _sitesComboBox.ResumeLayout();
                _sitesComboBox.EndUpdate();
            }

            _sitesComboBox.Focus();
        }

        private void OnUrlsComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            Update();
        }

        private void OnUrlsWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var siteName = (string)e.Argument;

            e.Result = Helper.GetUrlListFromBindings(_connection.ScopePath.ServerName, _module.Proxy.GetSiteBindings(siteName), RelativePath);
        }

        private void OnUrlsWorkerDoWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _urlsComboBox.BeginUpdate();
            _urlsComboBox.SuspendLayout();

            try
            {
                _urlsComboBox.Items.Clear();

                var domains = e.Result as List<string>;
                foreach (string domain in domains)
                {
                    _urlsComboBox.Items.Add(domain);
                }

                _urlsComboBox.SelectedIndex = 0;
                if (!String.IsNullOrEmpty(_preferenceUrl))
                {
                    int selectedIndex = _urlsComboBox.Items.IndexOf(_preferenceUrl);
                    if (selectedIndex > 0)
                    {
                        _urlsComboBox.SelectedIndex = selectedIndex;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ex, Resources.ResourceManager);
            }
            finally
            {
                _urlsComboBox.ResumeLayout();
                _urlsComboBox.EndUpdate();
            }

            _urlsComboBox.Focus();
        }

        private void SaveServerPreferences(PreferencesStore store)
        {
            store.SetValue(ServerSelectSitePreferenceKey, SiteName, String.Empty);
            store.SetValue(ServerSelectUrlPreferenceKey, SelectedUrl, String.Empty);
        }

        private void SaveSitePreferences(PreferencesStore store)
        {
            store.SetValue(GetSitePreferenceKey(_connection), SelectedUrl, String.Empty);
        }

        protected override void ShowHelp()
        {
            Helper.Browse(Globals.SelectSiteDomainOnlineHelp);
        }

    }
}