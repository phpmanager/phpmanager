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
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.Web.Management.Client.Win32;
using Web.Management.PHP.Config;

namespace Web.Management.PHP.Setup
{

    internal sealed class ChangeVersionDialog :
#if VSDesigner
        Form
#else
        TaskForm
#endif
    {
        private readonly PHPModule _module;

        private ManagementPanel _contentPanel;
        private Label _selectVersionLabel;
        private ComboBox _versionComboBox;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private readonly IContainer components = null;

        public ChangeVersionDialog(PHPModule module) : base(module)
        {
            _module = module;
            InitializeComponent();
            InitializeUI();
        }

        protected override bool CanAccept
        {
            get
            {
                return (_versionComboBox.Items.Count > 0);
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
            _selectVersionLabel = new Label();
            _versionComboBox = new ComboBox();
            SuspendLayout();
            // 
            // _selectVersionLabel
            // 
            _selectVersionLabel.AutoSize = true;
            _selectVersionLabel.Location = new System.Drawing.Point(0, 13);
            _selectVersionLabel.Name = "_selectVersionLabel";
            _selectVersionLabel.Size = new System.Drawing.Size(102, 13);
            _selectVersionLabel.TabIndex = 0;
            _selectVersionLabel.Text = Resources.ChangeVersionDialogSelectVersion;
            // 
            // _versionComboBox
            // 
            _versionComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _versionComboBox.FormattingEnabled = true;
            _versionComboBox.Location = new System.Drawing.Point(3, 30);
            _versionComboBox.Name = "_versionComboBox";
            _versionComboBox.Size = new System.Drawing.Size(326, 21);
            _versionComboBox.TabIndex = 1;
            // 
            // ChangeVersionDialog
            // 
            ClientSize = new System.Drawing.Size(364, 142);
            Controls.Add(_versionComboBox);
            Controls.Add(_selectVersionLabel);
            Name = "ChangeVersionDialog";
            ResumeLayout(false);
#if VSDesigner
            PerformLayout();
#endif
        }

        private void InitializeUI()
        {
            _contentPanel = new ManagementPanel();
            _contentPanel.SuspendLayout();

            _contentPanel.Location = new System.Drawing.Point(0, 0);
            _contentPanel.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(_selectVersionLabel);
            _contentPanel.Controls.Add(_versionComboBox);

            _contentPanel.ResumeLayout(false);
            _contentPanel.PerformLayout();

            Text = Resources.ChangeVersionDialogTitle;

            SetContent(_contentPanel);
            UpdateTaskForm();
        }

        protected override void OnAccept()
        {
            var selectedItem  = (PHPVersion)_versionComboBox.SelectedItem;

            try
            {
                _module.Proxy.SelectPHPVersion(selectedItem.HandlerName);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ex, Resources.ResourceManager);
            }
            Close();
        }

        private void OnGetVersionsDoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = _module.Proxy.GetAllPHPVersions();
        }

        private void OnGetVersionsDoWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _versionComboBox.BeginUpdate();
            _versionComboBox.SuspendLayout();

            try
            {
                var phpVersions = e.Result as RemoteObjectCollection<PHPVersion>;
                foreach (var phpVersion in phpVersions)
                {
                    phpVersion.Version = String.Format("{0} ({1})", phpVersion.Version, phpVersion.ScriptProcessor);
                    _versionComboBox.Items.Add(phpVersion);
                }
                _versionComboBox.DisplayMember = "Version";
                _versionComboBox.SelectedIndex = 0;
                if (_versionComboBox.Items.Count > 0)
                {
                    UpdateTaskForm();
                }
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ex, Resources.ResourceManager);
            }
            finally
            {
                _versionComboBox.ResumeLayout();
                _versionComboBox.EndUpdate();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            StartAsyncTask(OnGetVersionsDoWork, OnGetVersionsDoWorkCompleted);
        }

        protected override void ShowHelp()
        {
            Helper.Browse(Globals.ChangeVersionOnlineHelp);
        }
    }
}
