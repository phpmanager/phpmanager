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
using System.Windows.Forms;
using Microsoft.Web.Management.Client.Win32;

namespace Web.Management.PHP.Setup
{

    internal sealed class ChangeVersionDialog :
#if VSDesigner
        Form
#else
        TaskForm
#endif
    {
        private PHPModule _module;
        private bool _canAccept;

        private ManagementPanel _contentPanel;
        private Label _selectVersionLabel;
        private ComboBox _versionComboBox;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public ChangeVersionDialog(PHPModule module) : base(module)
        {
            _module = module;
            InitializeComponent();
            InitializeUI();

            try
            {
                ArrayList versions = _module.Proxy.GetAllPHPVersions();
                foreach (string[] version in versions)
                {
                    _versionComboBox.Items.Add(new PHPVersion(version[0], version[1], version[2]));
                }
                _versionComboBox.DisplayMember = "Version";
                _versionComboBox.SelectedIndex = 0;
                if (_versionComboBox.Items.Count > 0)
                {
                    _canAccept = true;
                    UpdateTaskForm();
                }
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ex, Resources.ResourceManager);
            }

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
            this._selectVersionLabel = new System.Windows.Forms.Label();
            this._versionComboBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // _selectVersionLabel
            // 
            this._selectVersionLabel.AutoSize = true;
            this._selectVersionLabel.Location = new System.Drawing.Point(0, 13);
            this._selectVersionLabel.Name = "_selectVersionLabel";
            this._selectVersionLabel.Size = new System.Drawing.Size(102, 13);
            this._selectVersionLabel.TabIndex = 0;
            this._selectVersionLabel.Text = "Select PHP version:";
            // 
            // _versionComboBox
            // 
            this._versionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._versionComboBox.FormattingEnabled = true;
            this._versionComboBox.Location = new System.Drawing.Point(3, 30);
            this._versionComboBox.Name = "_versionComboBox";
            this._versionComboBox.Size = new System.Drawing.Size(326, 21);
            this._versionComboBox.TabIndex = 1;
            // 
            // ChangeVersionDialog
            // 
            this.ClientSize = new System.Drawing.Size(364, 142);
            this.Controls.Add(this._versionComboBox);
            this.Controls.Add(this._selectVersionLabel);
            this.Name = "ChangeVersionDialog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void InitializeUI()
        {
            _contentPanel = new ManagementPanel();
            _contentPanel.SuspendLayout();

            this._contentPanel.Location = new System.Drawing.Point(0, 0);
            this._contentPanel.Dock = DockStyle.Fill;
            this._contentPanel.Controls.Add(_selectVersionLabel);
            this._contentPanel.Controls.Add(_versionComboBox);

            this._contentPanel.ResumeLayout(false);
            this._contentPanel.PerformLayout();

            this.Text = Resources.ChangeVersionDialogTitle;

            SetContent(_contentPanel);
            UpdateTaskForm();
        }

        protected override void OnAccept()
        {
            PHPVersion selectedItem  = (PHPVersion)_versionComboBox.SelectedItem;

            try
            {
                _module.Proxy.SelectPHPVersion(selectedItem.Name);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ex, Resources.ResourceManager);
            }
            Close();
        }


        // Used internally for the select version combo box
        private class PHPVersion
        {
            private string _name;
            private string _scriptProcessor;
            private string _version;

            public PHPVersion(string name, string scriptProcessor, string version)
            {
                _name = name;
                _scriptProcessor = scriptProcessor;
                _version = version;
            }

            public string Name
            {
                get
                {
                    return _name;
                }
            }

            public string ScriptProcessor
            {
                get
                {
                    return _scriptProcessor;
                }
            }

            public string Version
            {
                get
                {
                    return _version + " (" + _scriptProcessor + ")";
                }
            }
        }
    }
}
