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
        private readonly PHPModule _module;
        private readonly bool _isLocalConnection;
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
        private readonly System.ComponentModel.IContainer components = null;

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
            _pathToExtenionLabel = new Label();
            _extensionPathTextBox = new TextBox();
            _browseButton = new Button();
            _exampleLabel = new Label();
            SuspendLayout();
            // 
            // _pathToExtenionLabel
            // 
            _pathToExtenionLabel.AutoSize = true;
            _pathToExtenionLabel.Location = new System.Drawing.Point(0, 13);
            _pathToExtenionLabel.Name = "_pathToExtenionLabel";
            _pathToExtenionLabel.Size = new System.Drawing.Size(193, 13);
            _pathToExtenionLabel.TabIndex = 0;
            _pathToExtenionLabel.Text = Resources.AddExtensionDialogProvidePath;
            // 
            // _extensionPathTextBox
            // 
            _extensionPathTextBox.Location = new System.Drawing.Point(3, 30);
            _extensionPathTextBox.Name = "_extensionPathTextBox";
            _extensionPathTextBox.Size = new System.Drawing.Size(371, 20);
            _extensionPathTextBox.TabIndex = 1;
            _extensionPathTextBox.TextChanged += OnExtensionPathTextBoxTextChanged;
            // 
            // _browseButton
            // 
            _browseButton.Location = new System.Drawing.Point(380, 28);
            _browseButton.Name = "_browseButton";
            _browseButton.Size = new System.Drawing.Size(27, 23);
            _browseButton.TabIndex = 2;
            _browseButton.Text = @"...";
            _browseButton.UseVisualStyleBackColor = true;
            _browseButton.Click += OnBrowseButtonClick;
            // 
            // _exampleLabel
            // 
            _exampleLabel.AutoSize = true;
            _exampleLabel.Location = new System.Drawing.Point(0, 53);
            _exampleLabel.Name = "_exampleLabel";
            _exampleLabel.Size = new System.Drawing.Size(159, 13);
            _exampleLabel.TabIndex = 3;
            _exampleLabel.Text = Resources.AddExtensionDialogExample;
            // 
            // AddExtensionDialog
            // 
            ClientSize = new System.Drawing.Size(434, 142);
            Controls.Add(_exampleLabel);
            Controls.Add(_browseButton);
            Controls.Add(_extensionPathTextBox);
            Controls.Add(_pathToExtenionLabel);
            Name = "AddExtensionDialog";
            ResumeLayout(false);
            ResumeLayout(false);
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
                _extensionPathTextBox.AutoCompleteMode = AutoCompleteMode.Suggest;
                _extensionPathTextBox.AutoCompleteSource = AutoCompleteSource.FileSystem;
            }
            else
            {
                _browseButton.Visible = false;
            }

            _contentPanel.Location = new System.Drawing.Point(0, 0);
            _contentPanel.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(_pathToExtenionLabel);
            _contentPanel.Controls.Add(_extensionPathTextBox);
            _contentPanel.Controls.Add(_browseButton);
            _contentPanel.Controls.Add(_exampleLabel);

            _contentPanel.ResumeLayout(false);
            _contentPanel.PerformLayout();

            Text = Resources.AddExtensionDialogAddExtension;

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
            using (var dlg = new OpenFileDialog())
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
