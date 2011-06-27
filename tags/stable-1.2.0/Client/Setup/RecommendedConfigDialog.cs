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
        TaskForm
#endif
    {
        private const int TagIssueDescription = 0;
        private const int TagIssueRecommendation = 1;
        private const int TagIssueIndex = 2;
        private const int TagSize = 3;

        private PHPModule _module;
        private ManagementPanel _contentPanel;

        private ListView _configIssuesListView;
        private ColumnHeader _nameHeader;
        private Label _configIssueLabel;
        private ColumnHeader _currentValueHeader;
        private ColumnHeader _recommendedValueHeader;
        private Label _recommendationLabel;
        private TextBox _recommendationTextBox;
        private Label _issueDescriptionLabel;
        private TextBox _issueDescriptionTextBox;
        private Label _formDescriptionLabel;
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

        protected override bool CanAccept
        {
            get
            {
                return (_configIssuesListView.CheckedItems.Count > 0);
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
            this._formDescriptionLabel = new System.Windows.Forms.Label();
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
            this._configIssuesListView.Location = new System.Drawing.Point(0, 74);
            this._configIssuesListView.MultiSelect = false;
            this._configIssuesListView.Name = "_configIssuesListView";
            this._configIssuesListView.Size = new System.Drawing.Size(480, 130);
            this._configIssuesListView.TabIndex = 2;
            this._configIssuesListView.UseCompatibleStateImageBehavior = false;
            this._configIssuesListView.View = System.Windows.Forms.View.Details;
            this._configIssuesListView.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.OnConfigIssuesListViewItemChecked);
            this._configIssuesListView.SelectedIndexChanged += new System.EventHandler(this.OnConfigIssuesListViewSelectedIndexChanged);
            // 
            // _nameHeader
            // 
            this._nameHeader.Text = Resources.RecommendConfigDialogSettingName;
            this._nameHeader.Width = 170;
            // 
            // _currentValueHeader
            // 
            this._currentValueHeader.Text = Resources.RecommendConfigDialogCurrentValue;
            this._currentValueHeader.Width = 150;
            // 
            // _recommendedValueHeader
            // 
            this._recommendedValueHeader.Text = Resources.RecommendConfigDialogRecommendedValue;
            this._recommendedValueHeader.Width = 150;
            // 
            // _configIssueLabel
            // 
            this._configIssueLabel.AutoSize = true;
            this._configIssueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this._configIssueLabel.Location = new System.Drawing.Point(0, 58);
            this._configIssueLabel.Name = "_configIssueLabel";
            this._configIssueLabel.Size = new System.Drawing.Size(180, 13);
            this._configIssueLabel.TabIndex = 1;
            this._configIssueLabel.Text = Resources.RecommendConfigDialogDetectedIssues;
            // 
            // _recommendationLabel
            // 
            this._recommendationLabel.AutoSize = true;
            this._recommendationLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this._recommendationLabel.Location = new System.Drawing.Point(0, 298);
            this._recommendationLabel.Name = "_recommendationLabel";
            this._recommendationLabel.Size = new System.Drawing.Size(108, 13);
            this._recommendationLabel.TabIndex = 5;
            this._recommendationLabel.Text = Resources.RecommendConfigDialogRecommendation;
            // 
            // _recommendationTextBox
            // 
            this._recommendationTextBox.Location = new System.Drawing.Point(0, 314);
            this._recommendationTextBox.Multiline = true;
            this._recommendationTextBox.Name = "_recommendationTextBox";
            this._recommendationTextBox.ReadOnly = true;
            this._recommendationTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._recommendationTextBox.Size = new System.Drawing.Size(480, 60);
            this._recommendationTextBox.TabIndex = 6;
            // 
            // _issueDescriptionLabel
            // 
            this._issueDescriptionLabel.AutoSize = true;
            this._issueDescriptionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this._issueDescriptionLabel.Location = new System.Drawing.Point(0, 225);
            this._issueDescriptionLabel.Name = "_issueDescriptionLabel";
            this._issueDescriptionLabel.Size = new System.Drawing.Size(107, 13);
            this._issueDescriptionLabel.TabIndex = 3;
            this._issueDescriptionLabel.Text = Resources.RecommendConfigDialogIssueDescription;
            // 
            // _issueDescriptionTextBox
            // 
            this._issueDescriptionTextBox.Location = new System.Drawing.Point(0, 242);
            this._issueDescriptionTextBox.Multiline = true;
            this._issueDescriptionTextBox.Name = "_issueDescriptionTextBox";
            this._issueDescriptionTextBox.ReadOnly = true;
            this._issueDescriptionTextBox.Size = new System.Drawing.Size(480, 41);
            this._issueDescriptionTextBox.TabIndex = 4;
            // 
            // _formDescriptionLabel
            // 
            this._formDescriptionLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this._formDescriptionLabel.Location = new System.Drawing.Point(0, 0);
            this._formDescriptionLabel.Name = "_formDescriptionLabel";
            this._formDescriptionLabel.Size = new System.Drawing.Size(504, 38);
            this._formDescriptionLabel.TabIndex = 0;
            this._formDescriptionLabel.Text = Resources.RecommendConfigDialogDescription;
            // 
            // RecommendedConfigDialog
            // 
            this.ClientSize = new System.Drawing.Size(504, 452);
            this.Controls.Add(this._formDescriptionLabel);
            this.Controls.Add(this._issueDescriptionTextBox);
            this.Controls.Add(this._issueDescriptionLabel);
            this.Controls.Add(this._recommendationTextBox);
            this.Controls.Add(this._recommendationLabel);
            this.Controls.Add(this._configIssueLabel);
            this.Controls.Add(this._configIssuesListView);
            this.Name = "RecommendedConfigDialog";
            this.ResumeLayout(false);
#if VSDesigner
            this.PerformLayout();
#endif
        }

        private void InitializeUI()
        {
            _contentPanel = new ManagementPanel();
            _contentPanel.SuspendLayout();

            this._contentPanel.Location = new System.Drawing.Point(0, 0);
            this._contentPanel.Dock = DockStyle.Fill;
            this._contentPanel.Controls.Add(_formDescriptionLabel);
            this._contentPanel.Controls.Add(_configIssueLabel);
            this._contentPanel.Controls.Add(_configIssuesListView);
            this._contentPanel.Controls.Add(_issueDescriptionLabel);
            this._contentPanel.Controls.Add(_issueDescriptionTextBox);
            this._contentPanel.Controls.Add(_recommendationLabel);
            this._contentPanel.Controls.Add(_recommendationTextBox);

            this._contentPanel.ResumeLayout(false);
            this._contentPanel.PerformLayout();

            this.Text = Resources.RecommendConfigDialogTitle;

            SetContent(_contentPanel);
            UpdateTaskForm();

        }

        protected override void OnAccept()
        {
            try
            {
                ArrayList selectedIssues = new ArrayList();
                foreach (ListViewItem item in _configIssuesListView.CheckedItems)
                {
                    object[] tag = item.Tag as object[];
                    selectedIssues.Add(tag[TagIssueIndex]);
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
                UpdateTaskForm();
            }
        }

        private void OnConfigIssuesListViewItemChecked(object sender, ItemCheckedEventArgs e)
        {
            UpdateTaskForm();
        }

        private void OnConfigIssuesListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            if (_configIssuesListView.SelectedItems.Count > 0)
            {
                object[] tag = _configIssuesListView.SelectedItems[0].Tag as object[];
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
