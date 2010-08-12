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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.Management.Client.Win32;

namespace Web.Management.PHP.PHPSetup
{

    internal sealed class RegisterPHPDialog : 
#if VSDesigner
        Form
#else
        TaskForm
#endif
    {
        private PHPModule _module;

        private TextBox _dirPathTextBox;
        private Button _browseButton;
        private Label _dirPathLabel;
        private bool _canAccept;

        private ManagementPanel _contentPanel;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
    
        public RegisterPHPDialog(PHPModule module) : base(module)
        {
            _module = module;
            InitializeComponent();
            InitializeUI();
        }

        protected override bool CanAccept
        {
            get
            {
                return _canAccept;
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

        private void InitializeComponent()
        {
            this._dirPathLabel = new System.Windows.Forms.Label();
            this._dirPathTextBox = new System.Windows.Forms.TextBox();
            this._browseButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _dirPathLabel
            // 
            this._dirPathLabel.AutoSize = true;
            this._dirPathLabel.Location = new System.Drawing.Point(0, 13);
            this._dirPathLabel.Name = "_dirPathLabel";
            this._dirPathLabel.Size = new System.Drawing.Size(99, 13);
            this._dirPathLabel.TabIndex = 0;
            this._dirPathLabel.Text = "PHP directory path:";
            // 
            // _dirPathTextBox
            // 
            this._dirPathTextBox.Location = new System.Drawing.Point(0, 30);
            this._dirPathTextBox.Name = "_dirPathTextBox";
            this._dirPathTextBox.Size = new System.Drawing.Size(373, 20);
            this._dirPathTextBox.TabIndex = 1;
            this._dirPathTextBox.TextChanged += new System.EventHandler(this.OnDirPathTextBoxTextChanged);
            // 
            // _browseButton
            // 
            this._browseButton.Location = new System.Drawing.Point(379, 28);
            this._browseButton.Name = "_browseButton";
            this._browseButton.Size = new System.Drawing.Size(27, 23);
            this._browseButton.TabIndex = 2;
            this._browseButton.Text = "...";
            this._browseButton.UseVisualStyleBackColor = true;
            this._browseButton.Click += new System.EventHandler(this.OnBrowseButtonClick);
            // 
            // AddPHPDialog
            // 
            this.ClientSize = new System.Drawing.Size(434, 162);
            this.Controls.Add(this._browseButton);
            this.Controls.Add(this._dirPathTextBox);
            this.Controls.Add(this._dirPathLabel);
            this.Name = "AddPHPDialog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        public void InitializeUI()
        {
            _contentPanel = new ManagementPanel();
            _contentPanel.SuspendLayout();

            this._contentPanel.Location = new System.Drawing.Point(0, 0);
            this._contentPanel.Dock = DockStyle.Fill;
            this._contentPanel.Controls.Add(_dirPathLabel);
            this._contentPanel.Controls.Add(_dirPathTextBox);
            this._contentPanel.Controls.Add(_browseButton);

            this._contentPanel.ResumeLayout(false);
            this._contentPanel.PerformLayout();

            this.Text = Resources.RegisterPHPDialogRegisterNew;

            SetContent(_contentPanel);
            UpdateTaskForm();
        }

        protected override void OnAccept()
        {
            try
            {
                string path = _dirPathTextBox.Text.Trim();
                _module.Proxy.RegisterPHPWithIIS(path);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ex, Resources.ResourceManager);
            }

        }

        private void OnBrowseButtonClick(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = Resources.RegisterPHPDialogSelectFolder;
                dlg.ShowNewFolderButton = false;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _dirPathTextBox.Text = dlg.SelectedPath;
                }
            }
        }

        private void OnDirPathTextBoxTextChanged(object sender, EventArgs e)
        {
            string path = _dirPathTextBox.Text.Trim();

            _canAccept = !String.IsNullOrEmpty(path);

            UpdateTaskForm();
        }
    }
}
