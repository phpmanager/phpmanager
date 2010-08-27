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
        private PHPModule _module;
        private bool _canAccept;

        private Label _nameLabel;
        private TextBox _nameTextBox;
        private Label _valueLabel;
        private TextBox _valueTextBox;
        private Label _sectionLabel;
        private ManagementPanel _contentPanel;
        private TextBox _sectionTextBox;
        private LinkLabel _helpLinkLabel;

        private AutoCompleteStringCollection _autoCompleteSections;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            _sectionTextBox.Text = "PHP";

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
            this._nameLabel = new System.Windows.Forms.Label();
            this._nameTextBox = new System.Windows.Forms.TextBox();
            this._valueLabel = new System.Windows.Forms.Label();
            this._valueTextBox = new System.Windows.Forms.TextBox();
            this._sectionLabel = new System.Windows.Forms.Label();
            this._sectionTextBox = new System.Windows.Forms.TextBox();
            this._helpLinkLabel = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // _nameLabel
            // 
            this._nameLabel.AutoSize = true;
            this._nameLabel.Location = new System.Drawing.Point(0, 14);
            this._nameLabel.Name = "_nameLabel";
            this._nameLabel.Size = new System.Drawing.Size(38, 13);
            this._nameLabel.TabIndex = 0;
            this._nameLabel.Text = Resources.AddEditSettingDialogName;
            // 
            // _nameTextBox
            // 
            this._nameTextBox.Location = new System.Drawing.Point(0, 30);
            this._nameTextBox.Name = "_nameTextBox";
            this._nameTextBox.Size = new System.Drawing.Size(259, 20);
            this._nameTextBox.TabIndex = 1;
            this._nameTextBox.TextChanged += new System.EventHandler(this.OnTextBoxTextChanged);
            // 
            // _valueLabel
            // 
            this._valueLabel.AutoSize = true;
            this._valueLabel.Location = new System.Drawing.Point(0, 91);
            this._valueLabel.Name = "_valueLabel";
            this._valueLabel.Size = new System.Drawing.Size(37, 13);
            this._valueLabel.TabIndex = 2;
            this._valueLabel.Text = Resources.AddEditSettingDialogValue;
            // 
            // _valueTextBox
            // 
            this._valueTextBox.Location = new System.Drawing.Point(0, 107);
            this._valueTextBox.Name = "_valueTextBox";
            this._valueTextBox.Size = new System.Drawing.Size(259, 20);
            this._valueTextBox.TabIndex = 3;
            this._valueTextBox.TextChanged += new System.EventHandler(this.OnTextBoxTextChanged);
            // 
            // _sectionLabel
            // 
            this._sectionLabel.AutoSize = true;
            this._sectionLabel.Location = new System.Drawing.Point(0, 143);
            this._sectionLabel.Name = "_sectionLabel";
            this._sectionLabel.Size = new System.Drawing.Size(46, 13);
            this._sectionLabel.TabIndex = 4;
            this._sectionLabel.Text = Resources.AddEditSettingDialogSection;
            // 
            // _sectionTextBox
            // 
            this._sectionTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this._sectionTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this._sectionTextBox.Location = new System.Drawing.Point(0, 160);
            this._sectionTextBox.Name = "_sectionTextBox";
            this._sectionTextBox.Size = new System.Drawing.Size(259, 20);
            this._sectionTextBox.TabIndex = 5;
            // 
            // _helpLinkLabel
            // 
            this._helpLinkLabel.AutoSize = true;
            this._helpLinkLabel.Enabled = false;
            this._helpLinkLabel.Location = new System.Drawing.Point(0, 57);
            this._helpLinkLabel.Name = "_helpLinkLabel";
            this._helpLinkLabel.Size = new System.Drawing.Size(143, 13);
            this._helpLinkLabel.TabIndex = 6;
            this._helpLinkLabel.TabStop = true;
            this._helpLinkLabel.Text = Resources.AddEditSettingDialogLearnMore;
            this._helpLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnHelpLinkLabelLinkClicked);
            // 
            // AddEditSettingDialog
            // 
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this._helpLinkLabel);
            this.Controls.Add(this._sectionTextBox);
            this.Controls.Add(this._sectionLabel);
            this.Controls.Add(this._valueTextBox);
            this.Controls.Add(this._valueLabel);
            this.Controls.Add(this._nameTextBox);
            this.Controls.Add(this._nameLabel);
            this.Name = "AddEditSettingDialog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        public void InitializeUI()
        {

            this._contentPanel = new ManagementPanel();
            this._contentPanel.SuspendLayout();

            this._contentPanel.Location = new System.Drawing.Point(0, 0);
            this._contentPanel.Dock = DockStyle.Fill;
            this._contentPanel.Controls.Add(_nameLabel);
            this._contentPanel.Controls.Add(_nameTextBox);
            this._contentPanel.Controls.Add(_helpLinkLabel);
            this._contentPanel.Controls.Add(_valueLabel);
            this._contentPanel.Controls.Add(_valueTextBox);
            this._contentPanel.Controls.Add(_sectionLabel);
            this._contentPanel.Controls.Add(_sectionTextBox);

            this._contentPanel.ResumeLayout(false);
            this._contentPanel.PerformLayout();

            SetContent(_contentPanel);

            UpdateTaskForm();

        }

        protected override void OnAccept()
        {
            try
            {
                PHPIniSetting setting = new PHPIniSetting();
                setting.Name = _nameTextBox.Text.Trim();
                setting.Value = _valueTextBox.Text.Trim();
                setting.Section = _sectionTextBox.Text.Trim();

                RemoteObjectCollection<PHPIniSetting> settings = new RemoteObjectCollection<PHPIniSetting>();
                settings.Add(setting);
                _module.Proxy.AddOrUpdateSettings(settings);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ex, Resources.ResourceManager);
            }
        }

        private void OnHelpLinkLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.ShowHelp();
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