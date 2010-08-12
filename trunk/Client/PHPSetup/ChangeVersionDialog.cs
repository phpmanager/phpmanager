//#define VSDesigner

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.Management.Client.Win32;
using Web.Management.PHP.Config;

namespace Web.Management.PHP.PHPSetup
{

    internal partial class ChangeVersionDialog :
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
        private Label _executableLabel;
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
                    _versionComboBox.Items.Add(new PHPVersion( version[0], version[1], version[2]));
                }
                _versionComboBox.DisplayMember = "Version";
                _versionComboBox.SelectedIndex = 0;
                if (_versionComboBox.Items.Count > 0)
                {
                    _canAccept = true;
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

        private void _versionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_versionComboBox.Items.Count > 0)
            {
                _canAccept = true;
                PHPVersion selectedItem = (PHPVersion)_versionComboBox.SelectedItem;
                _executableLabel.Text = "Executable: " + selectedItem.ScriptProcessor;

                UpdateTaskForm();
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
            this._executableLabel = new System.Windows.Forms.Label();
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
            this._versionComboBox.SelectedIndexChanged += new System.EventHandler(this._versionComboBox_SelectedIndexChanged);
            // 
            // _executableLabel
            // 
            this._executableLabel.AutoSize = true;
            this._executableLabel.Location = new System.Drawing.Point(0, 54);
            this._executableLabel.Name = "_executableLabel";
            this._executableLabel.Size = new System.Drawing.Size(66, 13);
            this._executableLabel.TabIndex = 2;
            this._executableLabel.Text = "Executable: ";
            // 
            // SelectVersionDialog
            // 
            this.ClientSize = new System.Drawing.Size(364, 152);
            this.Controls.Add(this._executableLabel);
            this.Controls.Add(this._versionComboBox);
            this.Controls.Add(this._selectVersionLabel);
            this.Name = "SelectVersionDialog";
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
            this._contentPanel.Controls.Add(_executableLabel);

            this._contentPanel.ResumeLayout(false);
            this._contentPanel.PerformLayout();

            this.Text = "Change PHP version";

            SetContent(_contentPanel);
            UpdateTaskForm();
        }

        protected override void OnAccept()
        {
            PHPVersion selectedItem  = (PHPVersion)_versionComboBox.SelectedItem;
            _module.Proxy.SelectPHPVersion(selectedItem.Name);

            DialogResult = DialogResult.OK;
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
                    return _version;
                }
            }
        }
    }
}
