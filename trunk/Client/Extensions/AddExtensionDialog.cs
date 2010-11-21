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
using System.Windows.Forms;
using Microsoft.Web.Management.Client.Win32;

namespace Web.Management.PHP.Extensions
{

    internal sealed class AddExtensionDialog :
#if VSDesigner
        Form
#else
        TaskForm
#endif
    {
        private PHPModule _module;
        private bool _isLocalConnection;
        private bool _canAccept;

        private ManagementPanel _contentPanel;

        private TextBox _extensionPathTextBox;
        private Button _browseButton;
        private Label _exampleLabel;
        private Label _pathToExtenionLabel;
        private string _addedExtensionName;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public string AddedExtensionName
        {
            get
            {
                return _addedExtensionName;
            }
        }

        public AddExtensionDialog(PHPModule module, bool isLocalConnection) : base(module)
        {
            _module = module;
            _isLocalConnection = isLocalConnection;
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

        protected override bool CanShowHelp
        {
            get
            {
                return true;
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
            this._pathToExtenionLabel = new System.Windows.Forms.Label();
            this._extensionPathTextBox = new System.Windows.Forms.TextBox();
            this._browseButton = new System.Windows.Forms.Button();
            this._exampleLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _pathToExtenionLabel
            // 
            this._pathToExtenionLabel.AutoSize = true;
            this._pathToExtenionLabel.Location = new System.Drawing.Point(0, 13);
            this._pathToExtenionLabel.Name = "_pathToExtenionLabel";
            this._pathToExtenionLabel.Size = new System.Drawing.Size(193, 13);
            this._pathToExtenionLabel.TabIndex = 0;
            this._pathToExtenionLabel.Text = Resources.AddExtensionDialogProvidePath;
            // 
            // _extensionPathTextBox
            // 
            this._extensionPathTextBox.Location = new System.Drawing.Point(3, 30);
            this._extensionPathTextBox.Name = "_extensionPathTextBox";
            this._extensionPathTextBox.Size = new System.Drawing.Size(371, 20);
            this._extensionPathTextBox.TabIndex = 1;
            this._extensionPathTextBox.TextChanged += new System.EventHandler(this.OnExtensionPathTextBoxTextChanged);
            // 
            // _browseButton
            // 
            this._browseButton.Location = new System.Drawing.Point(380, 28);
            this._browseButton.Name = "_browseButton";
            this._browseButton.Size = new System.Drawing.Size(27, 23);
            this._browseButton.TabIndex = 2;
            this._browseButton.Text = "...";
            this._browseButton.UseVisualStyleBackColor = true;
            this._browseButton.Click += new System.EventHandler(this.OnBrowseButtonClick);
            // 
            // _exampleLabel
            // 
            this._exampleLabel.AutoSize = true;
            this._exampleLabel.Location = new System.Drawing.Point(0, 53);
            this._exampleLabel.Name = "_exampleLabel";
            this._exampleLabel.Size = new System.Drawing.Size(159, 13);
            this._exampleLabel.TabIndex = 3;
            this._exampleLabel.Text = Resources.AddExtensionDialogExample;
            // 
            // AddExtensionDialog
            // 
            this.ClientSize = new System.Drawing.Size(434, 142);
            this.Controls.Add(this._exampleLabel);
            this.Controls.Add(this._browseButton);
            this.Controls.Add(this._extensionPathTextBox);
            this.Controls.Add(this._pathToExtenionLabel);
            this.Name = "AddExtensionDialog";
            this.ResumeLayout(false);
            this.ResumeLayout(false);
#if VSDesigner
            this.PerformLayout();
#endif
        }

        private void InitializeUI()
        {
            _contentPanel = new ManagementPanel();
            _contentPanel.SuspendLayout();

            // Only show the auto suggest if it is a local connection.
            // Otherwise do not show auto suggest and also hide the browse button.
            if (_isLocalConnection)
            {
                this._extensionPathTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
                this._extensionPathTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            }
            else
            {
                this._browseButton.Visible = false;
            }

            this._contentPanel.Location = new System.Drawing.Point(0, 0);
            this._contentPanel.Dock = DockStyle.Fill;
            this._contentPanel.Controls.Add(_pathToExtenionLabel);
            this._contentPanel.Controls.Add(_extensionPathTextBox);
            this._contentPanel.Controls.Add(_browseButton);
            this._contentPanel.Controls.Add(_exampleLabel);

            this._contentPanel.ResumeLayout(false);
            this._contentPanel.PerformLayout();

            this.Text = Resources.AddExtensionDialogAddExtension;

            SetContent(_contentPanel);
            UpdateTaskForm();
        }

        protected override void OnAccept()
        {
            try
            {
                string path = _extensionPathTextBox.Text.Trim();
                _addedExtensionName = _module.Proxy.AddExtension(path);

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
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = Resources.AddExtensionDialogOpenFileTitle;
                dlg.Filter = Resources.AddExtensionDialogOpenFileFilter;
                if (!String.IsNullOrEmpty(_extensionPathTextBox.Text))
                {
                    dlg.InitialDirectory = System.IO.Path.GetDirectoryName(_extensionPathTextBox.Text.Trim());
                }
                else
                {
                    dlg.InitialDirectory = Environment.ExpandEnvironmentVariables("%SystemDrive%");
                }

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _extensionPathTextBox.Text = dlg.FileName;
                }
            }

        }

        private void OnExtensionPathTextBoxTextChanged(object sender, EventArgs e)
        {
            string path = _extensionPathTextBox .Text.Trim();

            _canAccept = !String.IsNullOrEmpty(path);

            UpdateTaskForm();
        }

        protected override void ShowHelp()
        {
            Helper.Browse(Globals.AddExtensionOnlineHelp);
        }

    }
}
