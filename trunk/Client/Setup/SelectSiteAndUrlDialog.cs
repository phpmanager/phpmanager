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
        private Guid PreferenceServiceGuid = new Guid("68a2a947-eeb6-40d9-9e5a-977bf7753bce");
        private const string ServerSelectSitePreferenceKey = "ServerSitePreferenceKey";
        private const string ServerSelectUrlPreferenceKey = "ServerUrlPreferenceKey";

        private PHPModule _module;
        private Connection _connection;
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
        private System.ComponentModel.IContainer components = null;

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
                    IPreferencesService prefService = (IPreferencesService)GetService(typeof(IPreferencesService));
                    if (prefService != null)
                    {
                        _store = prefService.GetPreferencesStore(PreferenceServiceGuid);
                    }
                }
                return _store;
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
            this._urlsComboBox = new System.Windows.Forms.ComboBox();
            this._sitesComboBox = new System.Windows.Forms.ComboBox();
            this._urlsLabel = new System.Windows.Forms.Label();
            this._sitesLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _domainsComboBox
            // 
            this._urlsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._urlsComboBox.FormattingEnabled = true;
            this._urlsComboBox.Location = new System.Drawing.Point(0, 90);
            this._urlsComboBox.Name = "_domainsComboBox";
            this._urlsComboBox.Size = new System.Drawing.Size(385, 21);
            this._urlsComboBox.TabIndex = 3;
            this._urlsComboBox.SelectedIndexChanged += new System.EventHandler(this.OnUrlsComboBoxSelectedIndexChanged);
            // 
            // _sitesComboBox
            // 
            this._sitesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._sitesComboBox.FormattingEnabled = true;
            this._sitesComboBox.Location = new System.Drawing.Point(0, 25);
            this._sitesComboBox.Name = "_sitesComboBox";
            this._sitesComboBox.Size = new System.Drawing.Size(385, 21);
            this._sitesComboBox.TabIndex = 1;
            this._sitesComboBox.SelectedIndexChanged += new System.EventHandler(this.OnSitesComboBoxSelectedIndexChanged);
            // 
            // _domainsLabel
            // 
            this._urlsLabel.Location = new System.Drawing.Point(0, 65);
            this._urlsLabel.Name = "_domainsLabel";
            this._urlsLabel.Size = new System.Drawing.Size(385, 22);
            this._urlsLabel.TabIndex = 2;
            this._urlsLabel.Text = Resources.SelectSiteAndUrlDialogSelectAUrl;
            // 
            // _sitesLabel
            // 
            this._sitesLabel.Location = new System.Drawing.Point(0, 0);
            this._sitesLabel.Name = "_sitesLabel";
            this._sitesLabel.Size = new System.Drawing.Size(385, 22);
            this._sitesLabel.TabIndex = 0;
            this._sitesLabel.Text = Resources.SelectSiteAndUrlDialogSelectASite;
            // 
            // SelectSiteDomainDialog
            // 
            this.ClientSize = new System.Drawing.Size(414, 192);
            this.Controls.Add(this._sitesComboBox);
            this.Controls.Add(this._sitesLabel);
            this.Controls.Add(this._urlsLabel);
            this.Controls.Add(this._urlsComboBox);
            this.Name = "SelectSiteDomainDialog";
            this.ResumeLayout(false);
        }

        #endregion

        public void InitializeUI()
        {
            this._contentPanel = new ManagementPanel();
            _contentPanel.SuspendLayout();

            this._contentPanel.Location = new System.Drawing.Point(0, 0);
            this._contentPanel.Dock = DockStyle.Fill;
            this._contentPanel.Controls.Add(_urlsLabel);
            this._contentPanel.Controls.Add(_urlsComboBox);
            this._contentPanel.Controls.Add(_sitesLabel);
            this._contentPanel.Controls.Add(_sitesComboBox);

            this._contentPanel.ResumeLayout(false);
            this._contentPanel.PerformLayout();

            this.Text = Resources.SelectSiteAndUrlDialogTitle;

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

            this.DialogResult = DialogResult.OK;
            this.Close();
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
            StartAsyncTask(OnUrlsWorkerDoWork, OnUrlsWorkerDoWorkCompleted, null, (string)_sitesComboBox.SelectedItem);
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

                ArrayList sites = e.Result as ArrayList;
                foreach (string siteName in sites)
                {
                    _sitesComboBox.Items.Add(siteName);
                }

                if (!String.IsNullOrEmpty(_preferenceSite))
                {
                    int selectedIndex = _sitesComboBox.Items.IndexOf(_preferenceSite);
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

        private void OnUrlsComboBoxSelectedIndexChanged(object sender, System.EventArgs e)
        {
            Update();
        }

        private void OnUrlsWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            string siteName = (string)e.Argument;
            string relativePath = String.Empty;

            if (_connection.ConfigurationPath.PathType != ConfigurationPathType.Server)
            {
                relativePath = _connection.ConfigurationPath.GetEffectiveConfigurationPath(ManagementScope.Site);
            }
            e.Result = Helper.GetUrlListFromBindings(_connection.ScopePath.ServerName, _module.Proxy.GetSiteBindings(siteName), relativePath);
        }

        private void OnUrlsWorkerDoWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _urlsComboBox.BeginUpdate();
            _urlsComboBox.SuspendLayout();

            try
            {
                _urlsComboBox.Items.Clear();

                List<string> domains = e.Result as List<string>;
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