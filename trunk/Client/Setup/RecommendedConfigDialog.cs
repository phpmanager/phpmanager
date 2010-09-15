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
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.Web.Management.Client.Win32;
using Web.Management.PHP.Config;

namespace Web.Management.PHP.Setup
{

    internal sealed class RecommendedConfigDialog :
#if VSDesigner
        Form
#else
        BaseTaskForm
#endif
    {
        private const int TagIssueDescription = 0;
        private const int TagIssueRecommendation = 1;
        private const int TagIssueIndex = 2;
        private const int TagSize = 3;

        private PHPModule _module;
        private Panel _contentPanel;
        private Panel _buttonsPanel;

        private ListView _configIssuesListView;
        private ColumnHeader _nameHeader;
        private Label _configIssueLabel;
        private ColumnHeader _currentValueHeader;
        private ColumnHeader _recommendedValueHeader;
        private Label _recommendationLabel;
        private TextBox _recommendationTextBox;
        private Label _issueDescriptionLabel;
        private TextBox _issueDescriptionTextBox;
        private Button _cancelButton;
        private Button _applyButton;


        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public RecommendedConfigDialog(PHPModule module): base(module)
        {
            _module = module;
            InitializeComponent();
            InitializeUI();
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

        private static string GetResourceStringByName(string name)
        {
            string result = Resources.ResourceManager.GetString(name);
            if (result == null)
            {
                result = name;
            }
            return result;
        }

        private void InitializeComponent()
        {
            this._configIssuesListView = new System.Windows.Forms.ListView();
            this._nameHeader = new System.Windows.Forms.ColumnHeader();
            this._currentValueHeader = new System.Windows.Forms.ColumnHeader();
            this._recommendedValueHeader = new System.Windows.Forms.ColumnHeader();
            this._configIssueLabel = new System.Windows.Forms.Label();
            this._recommendationLabel = new System.Windows.Forms.Label();
            this._recommendationTextBox = new System.Windows.Forms.TextBox();
            this._issueDescriptionLabel = new System.Windows.Forms.Label();
            this._issueDescriptionTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // _configIssuesListView
            // 
            this._configIssuesListView.CheckBoxes = true;
            this._configIssuesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._nameHeader,
            this._currentValueHeader,
            this._recommendedValueHeader});
            this._configIssuesListView.FullRowSelect = true;
            this._configIssuesListView.Location = new System.Drawing.Point(0, 19);
            this._configIssuesListView.MultiSelect = false;
            this._configIssuesListView.Name = "_configIssuesListView";
            this._configIssuesListView.Size = new System.Drawing.Size(480, 130);
            this._configIssuesListView.TabIndex = 1;
            this._configIssuesListView.UseCompatibleStateImageBehavior = false;
            this._configIssuesListView.View = System.Windows.Forms.View.Details;
            this._configIssuesListView.SelectedIndexChanged += new System.EventHandler(this.OnConfigIssuesListViewSelectedIndexChanged);
            // 
            // _nameHeader
            // 
            this._nameHeader.Text = global::Web.Management.PHP.Resources.RecommendConfigDialogSettingName;
            this._nameHeader.Width = 170;
            // 
            // _currentValueHeader
            // 
            this._currentValueHeader.Text = global::Web.Management.PHP.Resources.RecommendConfigDialogCurrentValue;
            this._currentValueHeader.Width = 150;
            // 
            // _recommendedValueHeader
            // 
            this._recommendedValueHeader.Text = global::Web.Management.PHP.Resources.RecommendConfigDialogRecommendedValue;
            this._recommendedValueHeader.Width = 150;
            // 
            // _configIssueLabel
            // 
            this._configIssueLabel.AutoSize = true;
            this._configIssueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this._configIssueLabel.Location = new System.Drawing.Point(0, 0);
            this._configIssueLabel.Name = "_configIssueLabel";
            this._configIssueLabel.Size = new System.Drawing.Size(180, 13);
            this._configIssueLabel.TabIndex = 0;
            this._configIssueLabel.Text = global::Web.Management.PHP.Resources.RecommendConfigDialogDetectedIssues;
            // 
            // _recommendationLabel
            // 
            this._recommendationLabel.AutoSize = true;
            this._recommendationLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this._recommendationLabel.Location = new System.Drawing.Point(0, 234);
            this._recommendationLabel.Name = "_recommendationLabel";
            this._recommendationLabel.Size = new System.Drawing.Size(108, 13);
            this._recommendationLabel.TabIndex = 4;
            this._recommendationLabel.Text = global::Web.Management.PHP.Resources.RecommendConfigDialogRecommendation;
            // 
            // _recommendationTextBox
            // 
            this._recommendationTextBox.Location = new System.Drawing.Point(0, 253);
            this._recommendationTextBox.Multiline = true;
            this._recommendationTextBox.Name = "_recommendationTextBox";
            this._recommendationTextBox.ReadOnly = true;
            this._recommendationTextBox.Size = new System.Drawing.Size(480, 60);
            this._recommendationTextBox.TabIndex = 5;
            // 
            // _issueDescriptionLabel
            // 
            this._issueDescriptionLabel.AutoSize = true;
            this._issueDescriptionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this._issueDescriptionLabel.Location = new System.Drawing.Point(0, 161);
            this._issueDescriptionLabel.Name = "_issueDescriptionLabel";
            this._issueDescriptionLabel.Size = new System.Drawing.Size(107, 13);
            this._issueDescriptionLabel.TabIndex = 2;
            this._issueDescriptionLabel.Text = global::Web.Management.PHP.Resources.RecommendConfigDialogIssueDescription;
            // 
            // _issueDescriptionTextBox
            // 
            this._issueDescriptionTextBox.Location = new System.Drawing.Point(0, 180);
            this._issueDescriptionTextBox.Multiline = true;
            this._issueDescriptionTextBox.Name = "_issueDescriptionTextBox";
            this._issueDescriptionTextBox.ReadOnly = true;
            this._issueDescriptionTextBox.Size = new System.Drawing.Size(480, 40);
            this._issueDescriptionTextBox.TabIndex = 3;
            // 
            // RecommendedConfigDialog
            // 
            this.ClientSize = new System.Drawing.Size(504, 402);
            this.Controls.Add(this._issueDescriptionTextBox);
            this.Controls.Add(this._issueDescriptionLabel);
            this.Controls.Add(this._recommendationTextBox);
            this.Controls.Add(this._recommendationLabel);
            this.Controls.Add(this._configIssueLabel);
            this.Controls.Add(this._configIssuesListView);
            this.Name = "RecommendedConfigDialog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void InitializeUI()
        {
            _cancelButton = new System.Windows.Forms.Button();
            _applyButton = new System.Windows.Forms.Button();
            _contentPanel = new ManagementPanel();
            _buttonsPanel = new ManagementPanel();
            _contentPanel.SuspendLayout();
            _buttonsPanel.SuspendLayout();

            // 
            // _cancelButton
            // 
            this._cancelButton.Location = new System.Drawing.Point(417, 0);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 7;
            this._cancelButton.Text = Resources.RecommendConfigDialogCancelButton;
            this._cancelButton.UseVisualStyleBackColor = true;
            this._cancelButton.Click += new EventHandler(OnCancelButtonClick);
            // 
            // _applyButton
            // 
            this._applyButton.Location = new System.Drawing.Point(336, 0);
            this._applyButton.Name = "_applyButton";
            this._applyButton.Size = new System.Drawing.Size(75, 23);
            this._applyButton.TabIndex = 6;
            this._applyButton.Text = Resources.RecommendConfigDialogApplyButton;
            this._applyButton.UseVisualStyleBackColor = true;
            this._applyButton.Enabled = false;
            this._applyButton.Click += new EventHandler(OnApplyButtonClick);

            this._buttonsPanel.Controls.Add(this._applyButton);
            this._buttonsPanel.Controls.Add(this._cancelButton);
            this._buttonsPanel.Dock = DockStyle.Bottom;

            this._contentPanel.Location = new System.Drawing.Point(0, 0);
            this._contentPanel.Dock = DockStyle.Fill;
            this._contentPanel.Controls.Add(_configIssueLabel);
            this._contentPanel.Controls.Add(_configIssuesListView);
            this._contentPanel.Controls.Add(_issueDescriptionLabel);
            this._contentPanel.Controls.Add(_issueDescriptionTextBox);
            this._contentPanel.Controls.Add(_recommendationLabel);
            this._contentPanel.Controls.Add(_recommendationTextBox);

            SetContent(_contentPanel);
            SetButtonsPanel(_buttonsPanel);
            this._contentPanel.ResumeLayout(false);
            this._buttonsPanel.ResumeLayout(false);

            this.Text = Resources.RecommendConfigDialogTitle;
        }

        private void OnApplyButtonClick(object sender, EventArgs e)
        {
            try
            {
                ArrayList selectedIssues = new ArrayList();
                foreach (ListViewItem item in _configIssuesListView.Items)
                {
                    if (item.Checked)
                    {
                        object[] tag = item.Tag as object[];
                        selectedIssues.Add(tag[TagIssueIndex]);
                    }
                }

                _module.Proxy.ApplyRecommendedSettings(selectedIssues);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ex, Resources.ResourceManager);
            }
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void OnConfigIssuesDoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = _module.Proxy.GetConfigIssues();
        }

        private void OnConfigIssuesDoWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _configIssuesListView.BeginUpdate();
            _configIssuesListView.SuspendLayout();

            try
            {
                RemoteObjectCollection<PHPConfigIssue> configIssues = e.Result as RemoteObjectCollection<PHPConfigIssue>;
                if (configIssues != null)
                {
                    foreach (PHPConfigIssue configIssue in configIssues)
                    {
                        ListViewItem listViewItem = new ListViewItem(configIssue.SettingName);
                        if (String.IsNullOrEmpty(configIssue.CurrentValue))
                        {
                            listViewItem.SubItems.Add(Resources.ConfigIssueNone);
                        }
                        else
                        {
                            listViewItem.SubItems.Add(configIssue.CurrentValue);
                        }
                        listViewItem.SubItems.Add(configIssue.RecommendedValue);
                        listViewItem.Tag = new object[TagSize] { GetResourceStringByName(configIssue.IssueDescription), 
                                                           GetResourceStringByName(configIssue.Recommendation),
                                                           configIssue.IssueIndex
                                                         };
                        _configIssuesListView.Items.Add(listViewItem);
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ex, Resources.ResourceManager);
            }
            finally
            {
                _configIssuesListView.ResumeLayout();
                _configIssuesListView.EndUpdate();
            }

            if (_configIssuesListView.Items.Count > 0)
            {
                _configIssuesListView.Focus();
                _configIssuesListView.Items[0].Selected = true;
                _applyButton.Enabled = true;
            }
        }

        private void OnConfigIssuesListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            if (_configIssuesListView.SelectedItems.Count > 0)
            {
                object [] tag = _configIssuesListView.SelectedItems[0].Tag as object[];
                _issueDescriptionTextBox.Text = (string)tag[TagIssueDescription];
                _recommendationTextBox.Text = (string)tag[TagIssueRecommendation];
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            StartAsyncTask(OnConfigIssuesDoWork, OnConfigIssuesDoWorkCompleted);
        }

        protected override void ShowHelp()
        {
            Helper.Browse(Globals.RecommendedConfigOnlineHelp);
        }

    }
}
