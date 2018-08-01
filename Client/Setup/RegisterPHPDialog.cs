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

namespace Web.Management.PHP.Setup
{

    internal sealed class RegisterPHPDialog : 
#if VSDesigner
        Form
#else
        TaskForm
#endif
    {
        private readonly PHPModule _module;
        private readonly bool _isLocalConnection;

        private TextBox _dirPathTextBox;
        private Button _browseButton;
        private Label _dirPathLabel;
        private bool _canAccept;

        private ManagementPanel _contentPanel;
        private Label _exampleLabel;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private readonly System.ComponentModel.IContainer components = null;
    
        public RegisterPHPDialog(PHPModule module, bool isLocalConnection) : base(module)
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
            _dirPathLabel = new Label();
            _dirPathTextBox = new TextBox();
            _browseButton = new Button();
            _exampleLabel = new Label();
            SuspendLayout();
            // 
            // _dirPathLabel
            // 
            _dirPathLabel.AutoSize = true;
            _dirPathLabel.Location = new System.Drawing.Point(0, 13);
            _dirPathLabel.Name = "_dirPathLabel";
            _dirPathLabel.Size = new System.Drawing.Size(265, 13);
            _dirPathLabel.TabIndex = 0;
            _dirPathLabel.Text = Resources.RegisterPHPDialogSelectPath;
            // 
            // _dirPathTextBox
            // 
            _dirPathTextBox.Location = new System.Drawing.Point(0, 30);
            _dirPathTextBox.Name = "_dirPathTextBox";
            _dirPathTextBox.Size = new System.Drawing.Size(373, 20);
            _dirPathTextBox.TabIndex = 1;
            _dirPathTextBox.TextChanged += OnDirPathTextBoxTextChanged;
            // 
            // _browseButton
            // 
            _browseButton.Location = new System.Drawing.Point(379, 28);
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
            _exampleLabel.Location = new System.Drawing.Point(3, 57);
            _exampleLabel.Name = "_exampleLabel";
            _exampleLabel.Size = new System.Drawing.Size(35, 13);
            _exampleLabel.TabIndex = 3;
            _exampleLabel.Text = Resources.RegisterPHPDialogExample;
            // 
            // RegisterPHPDialog
            // 
            ClientSize = new System.Drawing.Size(434, 162);
            Controls.Add(_exampleLabel);
            Controls.Add(_browseButton);
            Controls.Add(_dirPathTextBox);
            Controls.Add(_dirPathLabel);
            Name = "RegisterPHPDialog";
            ResumeLayout(false);
#if VSDesigner
            PerformLayout();
#endif
        }

        public void InitializeUI()
        {
            _contentPanel = new ManagementPanel();
            _contentPanel.SuspendLayout();

            // Only show the auto suggest if it is a local connection.
            // Otherwise do not show auto suggest and also hide the browse button.
            if (_isLocalConnection)
            {
                _dirPathTextBox.AutoCompleteMode = AutoCompleteMode.Suggest;
                _dirPathTextBox.AutoCompleteSource = AutoCompleteSource.FileSystem;
            }
            else
            {
                _browseButton.Visible = false;
            }

            _contentPanel.Location = new System.Drawing.Point(0, 0);
            _contentPanel.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(_dirPathLabel);
            _contentPanel.Controls.Add(_dirPathTextBox);
            _contentPanel.Controls.Add(_browseButton);
            _contentPanel.Controls.Add(_exampleLabel);

            _contentPanel.ResumeLayout(false);
            _contentPanel.PerformLayout();

            Text = Resources.RegisterPHPDialogRegisterNew;

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
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = Resources.RegisterPHPDialogOpenFileTitle;
                dlg.Filter = Resources.RegisterPHPDialogOpenFileFilter;
                if (!String.IsNullOrEmpty(_dirPathTextBox.Text))
                {
                    dlg.InitialDirectory = System.IO.Path.GetDirectoryName(_dirPathTextBox.Text.Trim());
                }
                else
                {
                    dlg.InitialDirectory = Environment.ExpandEnvironmentVariables("%SystemDrive%");
                }
                
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _dirPathTextBox.Text = dlg.FileName;
                }
            }
        }

        private void OnDirPathTextBoxTextChanged(object sender, EventArgs e)
        {
            string path = _dirPathTextBox.Text.Trim();

            _canAccept = !String.IsNullOrEmpty(path);

            UpdateTaskForm();
        }

        protected override void ShowHelp()
        {
            Helper.Browse(Globals.RegisterPHPOnlineHelp);
        }

    }
}
