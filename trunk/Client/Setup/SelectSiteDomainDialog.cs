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

    internal sealed class SelectSiteDomainDialog :
#if VSDesigner
        Form
#else
        TaskForm
#endif
    {
        private PHPModule _module;
        private Connection _connection;
        private string _selectedDomain;

        private ManagementPanel _contentPanel;
        private ComboBox _domainsComboBox;
        private ComboBox _sitesComboBox;
        private Label _domainsLabel;
        private Label _sitesLabel;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public SelectSiteDomainDialog(PHPModule module, Connection connection, string selectedDomain)
            : base(module)
        {
            _module = module;
            _connection = connection;
            _selectedDomain = selectedDomain;

            InitializeComponent();
            InitializeUI();

            Update();
        }

        protected override bool CanAccept
        {
            get
            {
                return _domainsComboBox.SelectedIndex >= 0;
            }
        }

        protected override bool CanShowHelp
        {
            get
            {
                return true;
            }
        }

        public string DomainName
        {
            get
            {
                return _domainsComboBox.SelectedItem as string;
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._domainsComboBox = new System.Windows.Forms.ComboBox();
            this._sitesComboBox = new System.Windows.Forms.ComboBox();
            this._domainsLabel = new System.Windows.Forms.Label();
            this._sitesLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _domainsComboBox
            // 
            this._domainsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._domainsComboBox.FormattingEnabled = true;
            this._domainsComboBox.Location = new System.Drawing.Point(0, 90);
            this._domainsComboBox.Name = "_domainsComboBox";
            this._domainsComboBox.Size = new System.Drawing.Size(385, 21);
            this._domainsComboBox.TabIndex = 3;
            this._domainsComboBox.SelectedIndexChanged += new System.EventHandler(this.OnDomainsComboBoxSelectedIndexChanged);
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
            this._domainsLabel.Location = new System.Drawing.Point(0, 65);
            this._domainsLabel.Name = "_domainsLabel";
            this._domainsLabel.Size = new System.Drawing.Size(385, 22);
            this._domainsLabel.TabIndex = 2;
            this._domainsLabel.Text = Resources.SelectSiteDomainDialogSelectADomain;
            // 
            // _sitesLabel
            // 
            this._sitesLabel.Location = new System.Drawing.Point(0, 0);
            this._sitesLabel.Name = "_sitesLabel";
            this._sitesLabel.Size = new System.Drawing.Size(385, 22);
            this._sitesLabel.TabIndex = 0;
            this._sitesLabel.Text = Resources.SelectSiteDomainDialogSelectASite;
            // 
            // SelectSiteDomainDialog
            // 
            this.ClientSize = new System.Drawing.Size(414, 192);
            this.Controls.Add(this._sitesComboBox);
            this.Controls.Add(this._sitesLabel);
            this.Controls.Add(this._domainsLabel);
            this.Controls.Add(this._domainsComboBox);
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
            this._contentPanel.Controls.Add(_domainsLabel);
            this._contentPanel.Controls.Add(_domainsComboBox);
            this._contentPanel.Controls.Add(_sitesLabel);
            this._contentPanel.Controls.Add(_sitesComboBox);

            this._contentPanel.ResumeLayout(false);
            this._contentPanel.PerformLayout();

            this.Text = Resources.SelectSiteDomainDialogTitle;

            SetContent(_contentPanel);
            UpdateTaskForm();
        }

        protected override void OnAccept()
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void OnDomainsComboBoxSelectedIndexChanged(object sender, System.EventArgs e)
        {
            Update();
        }

        private void OnDomainWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            string siteName = (string)e.Argument;
            e.Result = Helper.GetUrlListFromBindings(_connection.ScopePath.ServerName, _module.Proxy.GetSiteBindings(siteName));
        }

        private void OnDomainWorkerDoWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _domainsComboBox.BeginUpdate();
            _domainsComboBox.SuspendLayout();

            try
            {
                _domainsComboBox.Items.Clear();

                List<string> domains = e.Result as List<string>;
                foreach (string domain in domains)
                {
                    _domainsComboBox.Items.Add(domain);
                }

                int selectedIndex = _domainsComboBox.Items.IndexOf(_selectedDomain);
                _domainsComboBox.SelectedIndex = (selectedIndex >= 0) ? selectedIndex : 0;
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ex, Resources.ResourceManager);
            }
            finally
            {
                _domainsComboBox.ResumeLayout();
                _domainsComboBox.EndUpdate();
            }

            _domainsComboBox.Focus();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_connection.ConfigurationPath.PathType == ConfigurationPathType.Server)
            {
                StartAsyncTask(OnSiteWorkerDoWork, OnSiteWorkerDoWorkCompleted);
            }
            else
            {
                _sitesComboBox.Items.Add(_connection.ConfigurationPath.SiteName);
                _sitesComboBox.SelectedIndex = 0;
                _sitesComboBox.Enabled = false;
            }
        }

        private void OnSitesComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            StartAsyncTask(OnDomainWorkerDoWork, OnDomainWorkerDoWorkCompleted, null, (string)_sitesComboBox.SelectedItem);
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

        //protected override void ShowHelp() {
        //    PHPModule.Browse(Globals.SelectSiteDialogOnlineHelp);
        //}
    }
}