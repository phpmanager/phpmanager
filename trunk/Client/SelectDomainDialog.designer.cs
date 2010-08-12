//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

namespace Web.Management.PHP
{
    partial class SelectDomainDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this._domainsLabel = new System.Windows.Forms.Label();
            this._titleLabel = new System.Windows.Forms.Label();
            this._domainsComboBox = new System.Windows.Forms.ComboBox();
            this._contentPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _contentPanel
            // 
            this._contentPanel.Controls.Add(this._domainsLabel);
            this._contentPanel.Controls.Add(this._titleLabel);
            this._contentPanel.Controls.Add(this._domainsComboBox);
            this._contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._contentPanel.Location = new System.Drawing.Point(0, 0);
            this._contentPanel.Name = "_contentPanel";
            this._contentPanel.Size = new System.Drawing.Size(414, 132);
            this._contentPanel.TabIndex = 0;
            // 
            // _siteLabel
            // 
            this._domainsLabel.AutoSize = true;
            this._domainsLabel.Location = new System.Drawing.Point(12, 31);
            this._domainsLabel.Name = "_bindingLabel";
            this._domainsLabel.Size = new System.Drawing.Size(28, 13);
            this._domainsLabel.TabIndex = 1;
            this._domainsLabel.Text = Resources.SelectDomainDomainLabel;
            // 
            // _titleLabel
            // 
            this._titleLabel.Location = new System.Drawing.Point(0, 0);
            this._titleLabel.Name = "_titleLabel";
            this._titleLabel.AutoSize = false;
            this._titleLabel.Size = new System.Drawing.Size(410, 33);
            this._titleLabel.TabIndex = 0;
            this._titleLabel.Text = Resources.SelectDomainSelectADomainFirst;
            // 
            // _urlsComboBox
            // 
            this._domainsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._domainsComboBox.FormattingEnabled = true;
            this._domainsComboBox.Location = new System.Drawing.Point(12, 51);
            this._domainsComboBox.Name = "_bindingsComboBox";
            this._domainsComboBox.Size = new System.Drawing.Size(375, 21);
            this._domainsComboBox.TabIndex = 2;
            this._domainsComboBox.SelectedIndexChanged += new System.EventHandler(this.OnDomainsComboBoxSelectedIndexChanged);
            // 
            // SelectSiteDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(414, 132);
            this.SetContent(this._contentPanel);
            this.Name = "SelectBindingDialog";
            this.ShowIcon = false;
            this.Text = Resources.SelectDomainTitle;
            this._contentPanel.ResumeLayout(false);
            this._contentPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private Microsoft.Web.Management.Client.Win32.ManagementPanel _contentPanel;
        private System.Windows.Forms.Label _titleLabel;
        private System.Windows.Forms.ComboBox _domainsComboBox;
        private System.Windows.Forms.Label _domainsLabel;
    }
}