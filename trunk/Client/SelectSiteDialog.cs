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
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.Web.Management.Client.Win32;

namespace Web.Management.PHP
{

    internal sealed class SelectSiteDialog : TaskForm
    {

        private PHPModule _module;

        private Microsoft.Web.Management.Client.Win32.ManagementPanel _contentPanel;
        private System.Windows.Forms.Label _titleLabel;
        private System.Windows.Forms.ComboBox _sitesComboBox;
        private System.Windows.Forms.Label _siteLabel;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public SelectSiteDialog(PHPModule module)
            : base(module)
        {

            _module = module;

            InitializeComponent();

            Update();
        }

        protected override bool CanAccept
        {
            get
            {
                return _sitesComboBox.SelectedIndex >= 0;
            }
        }

        protected override bool CanShowHelp
        {
            get
            {
                return true;
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
            this._contentPanel = new Microsoft.Web.Management.Client.Win32.ManagementPanel();
            this._siteLabel = new System.Windows.Forms.Label();
            this._titleLabel = new System.Windows.Forms.Label();
            this._sitesComboBox = new System.Windows.Forms.ComboBox();
            this._contentPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _contentPanel
            // 
            this._contentPanel.Controls.Add(this._siteLabel);
            this._contentPanel.Controls.Add(this._titleLabel);
            this._contentPanel.Controls.Add(this._sitesComboBox);
            this._contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._contentPanel.Location = new System.Drawing.Point(0, 0);
            this._contentPanel.Name = "_contentPanel";
            this._contentPanel.Size = new System.Drawing.Size(414, 132);
            this._contentPanel.TabIndex = 0;
            // 
            // _siteLabel
            // 
            this._siteLabel.AutoSize = true;
            this._siteLabel.Location = new System.Drawing.Point(12, 31);
            this._siteLabel.Name = "_siteLabel";
            this._siteLabel.Size = new System.Drawing.Size(28, 13);
            this._siteLabel.TabIndex = 1;
            this._siteLabel.Text = Resources.SelectSiteDialogSiteLabel;
            // 
            // _titleLabel
            // 
            this._titleLabel.Location = new System.Drawing.Point(0, 0);
            this._titleLabel.Name = "_titleLabel";
            this._titleLabel.AutoSize = false;
            this._titleLabel.Size = new System.Drawing.Size(410, 33);
            this._titleLabel.TabIndex = 0;
            this._titleLabel.Text = Resources.SelectSiteDialogSelectASiteFirst;
            // 
            // _sitesComboBox
            // 
            this._sitesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._sitesComboBox.FormattingEnabled = true;
            this._sitesComboBox.Location = new System.Drawing.Point(12, 51);
            this._sitesComboBox.Name = "_sitesComboBox";
            this._sitesComboBox.Size = new System.Drawing.Size(375, 21);
            this._sitesComboBox.TabIndex = 2;
            this._sitesComboBox.SelectedIndexChanged += new System.EventHandler(this.OnSitesComboBoxSelectedIndexChanged);
            // 
            // SelectSiteDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(414, 132);
            this.SetContent(this._contentPanel);
            this.Name = "SelectSiteDialog";
            this.ShowIcon = false;
            this.Text = Resources.SelectSiteDialogTitle;
            this._contentPanel.ResumeLayout(false);
            this._contentPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        protected override void OnAccept()
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            StartAsyncTask(OnWorkerDoWork, OnWorkerDoWorkCompleted);
        }

        private void OnSitesComboBoxSelectedIndexChanged(object sender, System.EventArgs e)
        {
            Update();
        }

        private void OnWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = _module.Proxy.GetSites();
        }

        private void OnWorkerDoWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
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