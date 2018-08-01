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
using System.Windows.Forms;
using Microsoft.Web.Management.Client.Win32;
using Web.Management.PHP.Config;

namespace Web.Management.PHP.Settings
{

    internal sealed class AddEditSettingDialog : 
#if VSDesigner
        Form
#else
        TaskForm
#endif
    {
        private readonly PHPModule _module;
        private bool _canAccept;

        private Label _nameLabel;
        private TextBox _nameTextBox;
        private Label _valueLabel;
        private TextBox _valueTextBox;
        private Label _sectionLabel;
        private ManagementPanel _contentPanel;
        private TextBox _sectionTextBox;
        private LinkLabel _helpLinkLabel;

        private readonly AutoCompleteStringCollection _autoCompleteSections;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private readonly System.ComponentModel.IContainer components = null;

        // Constructor for Add Setting Dialog
        public AddEditSettingDialog(PHPModule module, IEnumerable<string> sections)
            : base(module)
        {
            _module = module;

            InitializeComponent();
            InitializeUI();

            // The dialog is used in the "Add setting" mode
            Text = Resources.AddEditSettingDialogAddSetting;

            _autoCompleteSections = new AutoCompleteStringCollection();
            foreach (string section in sections)
            {
                _autoCompleteSections.Add(section);
            }
            _sectionTextBox.AutoCompleteCustomSource = _autoCompleteSections;
            _sectionTextBox.Text = @"PHP";

            UpdateUI();
        }

        // Constructor for Edit Setting Dialog
        public AddEditSettingDialog(PHPModule module, PHPIniSetting setting)
            : base(module)
        {
            _module = module;

            InitializeComponent();
            InitializeUI();

            // The dialog is used in the "Edit setting" mode
            Text = Resources.AddEditSettingDialogEditSetting;

            _nameTextBox.Enabled = false;
            _sectionTextBox.Enabled = false;
            _nameTextBox.Text = setting.Name;
            _valueTextBox.Text = setting.Value;
            _sectionTextBox.Text = setting.Section;

            UpdateUI();
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

        internal string SettingName { get; set; }

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
            _nameLabel = new Label();
            _nameTextBox = new  TextBox();
            _valueLabel = new Label();
            _valueTextBox = new TextBox();
            _sectionLabel = new Label();
            _sectionTextBox = new TextBox();
            _helpLinkLabel = new LinkLabel();
            SuspendLayout();
            // 
            // _nameLabel
            // 
            _nameLabel.AutoSize = true;
            _nameLabel.Location = new System.Drawing.Point(0, 14);
            _nameLabel.Name = "_nameLabel";
            _nameLabel.Size = new System.Drawing.Size(38, 13);
            _nameLabel.TabIndex = 0;
            _nameLabel.Text = Resources.AddEditSettingDialogName;
            // 
            // _nameTextBox
            // 
            _nameTextBox.Location = new System.Drawing.Point(0, 30);
            _nameTextBox.Name = "_nameTextBox";
            _nameTextBox.Size = new System.Drawing.Size(259, 20);
            _nameTextBox.TabIndex = 1;
            _nameTextBox.TextChanged += OnTextBoxTextChanged;
            // 
            // _valueLabel
            // 
            _valueLabel.AutoSize = true;
            _valueLabel.Location = new System.Drawing.Point(0, 91);
            _valueLabel.Name = "_valueLabel";
            _valueLabel.Size = new System.Drawing.Size(37, 13);
            _valueLabel.TabIndex = 2;
            _valueLabel.Text = Resources.AddEditSettingDialogValue;
            // 
            // _valueTextBox
            // 
            _valueTextBox.Location = new System.Drawing.Point(0, 107);
            _valueTextBox.Name = "_valueTextBox";
            _valueTextBox.Size = new System.Drawing.Size(259, 20);
            _valueTextBox.TabIndex = 3;
            _valueTextBox.TextChanged += OnTextBoxTextChanged;
            // 
            // _sectionLabel
            // 
            _sectionLabel.AutoSize = true;
            _sectionLabel.Location = new System.Drawing.Point(0, 143);
            _sectionLabel.Name = "_sectionLabel";
            _sectionLabel.Size = new System.Drawing.Size(46, 13);
            _sectionLabel.TabIndex = 4;
            _sectionLabel.Text = Resources.AddEditSettingDialogSection;
            // 
            // _sectionTextBox
            // 
            _sectionTextBox.AutoCompleteMode = AutoCompleteMode.Suggest;
            _sectionTextBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
            _sectionTextBox.Location = new System.Drawing.Point(0, 160);
            _sectionTextBox.Name = "_sectionTextBox";
            _sectionTextBox.Size = new System.Drawing.Size(259, 20);
            _sectionTextBox.TabIndex = 5;
            // 
            // _helpLinkLabel
            // 
            _helpLinkLabel.AutoSize = true;
            _helpLinkLabel.Enabled = false;
            _helpLinkLabel.Location = new System.Drawing.Point(0, 57);
            _helpLinkLabel.Name = "_helpLinkLabel";
            _helpLinkLabel.Size = new System.Drawing.Size(143, 13);
            _helpLinkLabel.TabIndex = 6;
            _helpLinkLabel.TabStop = true;
            _helpLinkLabel.Text = Resources.AddEditSettingDialogLearnMore;
            _helpLinkLabel.LinkClicked += OnHelpLinkLabelLinkClicked;
            // 
            // AddEditSettingDialog
            // 
            ClientSize = new System.Drawing.Size(284, 262);
            Controls.Add(_helpLinkLabel);
            Controls.Add(_sectionTextBox);
            Controls.Add(_sectionLabel);
            Controls.Add(_valueTextBox);
            Controls.Add(_valueLabel);
            Controls.Add(_nameTextBox);
            Controls.Add(_nameLabel);
            Name = "AddEditSettingDialog";
            ResumeLayout(false);
#if VSDesigner
            PerformLayout();
#endif
        }

        public void InitializeUI()
        {

            _contentPanel = new ManagementPanel();
            _contentPanel.SuspendLayout();

            _contentPanel.Location = new System.Drawing.Point(0, 0);
            _contentPanel.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(_nameLabel);
            _contentPanel.Controls.Add(_nameTextBox);
            _contentPanel.Controls.Add(_helpLinkLabel);
            _contentPanel.Controls.Add(_valueLabel);
            _contentPanel.Controls.Add(_valueTextBox);
            _contentPanel.Controls.Add(_sectionLabel);
            _contentPanel.Controls.Add(_sectionTextBox);

            _contentPanel.ResumeLayout(false);
            _contentPanel.PerformLayout();

            SetContent(_contentPanel);

            UpdateTaskForm();
        }

        protected override void OnAccept()
        {
            try
            {
                var setting = new PHPIniSetting
                    {
                        Name = _nameTextBox.Text.Trim(),
                        Value = _valueTextBox.Text.Trim(),
                        Section = _sectionTextBox.Text.Trim()
                    };

                var settings = new RemoteObjectCollection<PHPIniSetting> {setting};
                _module.Proxy.AddOrUpdateSettings(settings);

                DialogResult = DialogResult.OK;
                SettingName = setting.Name;
                Close();
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ex, Resources.ResourceManager);
            }
        }

        private void OnHelpLinkLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ShowHelp();
        }

        private void OnTextBoxTextChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        protected override void ShowHelp()
        {
            Helper.Browse("http://www.php.net/" + _nameTextBox.Text.Trim());
        }

        private void UpdateUI()
        {
            string name = _nameTextBox.Text.Trim();
            string value = _valueTextBox.Text.Trim();
            string section = _sectionTextBox.Text.Trim();
            _canAccept = !String.IsNullOrEmpty(name) && !String.IsNullOrEmpty(value) && !String.IsNullOrEmpty(section);
            _helpLinkLabel.Enabled = !String.IsNullOrEmpty(name);

            UpdateTaskForm();
        }

    }
}