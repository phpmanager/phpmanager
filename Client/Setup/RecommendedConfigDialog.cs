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

        private readonly PHPModule _module;
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
        private readonly IContainer components = null;

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
            string result = Resources.ResourceManager.GetString(name) ?? name;
            return result;
        }

        private void InitializeComponent()
        {
            _configIssuesListView = new ListView();
            _nameHeader = new ColumnHeader();
            _currentValueHeader = new ColumnHeader();
            _recommendedValueHeader = new ColumnHeader();
            _configIssueLabel = new Label();
            _recommendationLabel = new Label();
            _recommendationTextBox = new TextBox();
            _issueDescriptionLabel = new Label();
            _issueDescriptionTextBox = new TextBox();
            _formDescriptionLabel = new Label();
            SuspendLayout();
            // 
            // _configIssuesListView
            // 
            _configIssuesListView.CheckBoxes = true;
            _configIssuesListView.Columns.AddRange(new[] {
            _nameHeader,
            _currentValueHeader,
            _recommendedValueHeader});
            _configIssuesListView.FullRowSelect = true;
            _configIssuesListView.Location = new System.Drawing.Point(0, 74);
            _configIssuesListView.MultiSelect = false;
            _configIssuesListView.Name = "_configIssuesListView";
            _configIssuesListView.Size = new System.Drawing.Size(480, 130);
            _configIssuesListView.TabIndex = 2;
            _configIssuesListView.UseCompatibleStateImageBehavior = false;
            _configIssuesListView.View = View.Details;
            _configIssuesListView.ItemChecked += OnConfigIssuesListViewItemChecked;
            _configIssuesListView.SelectedIndexChanged += OnConfigIssuesListViewSelectedIndexChanged;
            // 
            // _nameHeader
            // 
            _nameHeader.Text = Resources.RecommendConfigDialogSettingName;
            _nameHeader.Width = 170;
            // 
            // _currentValueHeader
            // 
            _currentValueHeader.Text = Resources.RecommendConfigDialogCurrentValue;
            _currentValueHeader.Width = 150;
            // 
            // _recommendedValueHeader
            // 
            _recommendedValueHeader.Text = Resources.RecommendConfigDialogRecommendedValue;
            _recommendedValueHeader.Width = 150;
            // 
            // _configIssueLabel
            // 
            _configIssueLabel.AutoSize = true;
            _configIssueLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 204);
            _configIssueLabel.Location = new System.Drawing.Point(0, 58);
            _configIssueLabel.Name = "_configIssueLabel";
            _configIssueLabel.Size = new System.Drawing.Size(180, 13);
            _configIssueLabel.TabIndex = 1;
            _configIssueLabel.Text = Resources.RecommendConfigDialogDetectedIssues;
            // 
            // _recommendationLabel
            // 
            _recommendationLabel.AutoSize = true;
            _recommendationLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 204);
            _recommendationLabel.Location = new System.Drawing.Point(0, 298);
            _recommendationLabel.Name = "_recommendationLabel";
            _recommendationLabel.Size = new System.Drawing.Size(108, 13);
            _recommendationLabel.TabIndex = 5;
            _recommendationLabel.Text = Resources.RecommendConfigDialogRecommendation;
            // 
            // _recommendationTextBox
            // 
            _recommendationTextBox.Location = new System.Drawing.Point(0, 314);
            _recommendationTextBox.Multiline = true;
            _recommendationTextBox.Name = "_recommendationTextBox";
            _recommendationTextBox.ReadOnly = true;
            _recommendationTextBox.ScrollBars = ScrollBars.Vertical;
            _recommendationTextBox.Size = new System.Drawing.Size(480, 60);
            _recommendationTextBox.TabIndex = 6;
            // 
            // _issueDescriptionLabel
            // 
            _issueDescriptionLabel.AutoSize = true;
            _issueDescriptionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 204);
            _issueDescriptionLabel.Location = new System.Drawing.Point(0, 225);
            _issueDescriptionLabel.Name = "_issueDescriptionLabel";
            _issueDescriptionLabel.Size = new System.Drawing.Size(107, 13);
            _issueDescriptionLabel.TabIndex = 3;
            _issueDescriptionLabel.Text = Resources.RecommendConfigDialogIssueDescription;
            // 
            // _issueDescriptionTextBox
            // 
            _issueDescriptionTextBox.Location = new System.Drawing.Point(0, 242);
            _issueDescriptionTextBox.Multiline = true;
            _issueDescriptionTextBox.Name = "_issueDescriptionTextBox";
            _issueDescriptionTextBox.ReadOnly = true;
            _issueDescriptionTextBox.Size = new System.Drawing.Size(480, 41);
            _issueDescriptionTextBox.TabIndex = 4;
            // 
            // _formDescriptionLabel
            // 
            _formDescriptionLabel.Dock = DockStyle.Top;
            _formDescriptionLabel.Location = new System.Drawing.Point(0, 0);
            _formDescriptionLabel.Name = "_formDescriptionLabel";
            _formDescriptionLabel.Size = new System.Drawing.Size(504, 38);
            _formDescriptionLabel.TabIndex = 0;
            _formDescriptionLabel.Text = Resources.RecommendConfigDialogDescription;
            // 
            // RecommendedConfigDialog
            // 
            ClientSize = new System.Drawing.Size(504, 452);
            Controls.Add(_formDescriptionLabel);
            Controls.Add(_issueDescriptionTextBox);
            Controls.Add(_issueDescriptionLabel);
            Controls.Add(_recommendationTextBox);
            Controls.Add(_recommendationLabel);
            Controls.Add(_configIssueLabel);
            Controls.Add(_configIssuesListView);
            Name = "RecommendedConfigDialog";
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
            _contentPanel.Controls.Add(_formDescriptionLabel);
            _contentPanel.Controls.Add(_configIssueLabel);
            _contentPanel.Controls.Add(_configIssuesListView);
            _contentPanel.Controls.Add(_issueDescriptionLabel);
            _contentPanel.Controls.Add(_issueDescriptionTextBox);
            _contentPanel.Controls.Add(_recommendationLabel);
            _contentPanel.Controls.Add(_recommendationTextBox);

            _contentPanel.ResumeLayout(false);
            _contentPanel.PerformLayout();

            Text = Resources.RecommendConfigDialogTitle;

            SetContent(_contentPanel);
            UpdateTaskForm();

        }

        protected override void OnAccept()
        {
            try
            {
                var selectedIssues = new ArrayList();
                foreach (ListViewItem item in _configIssuesListView.CheckedItems)
                {
                    var tag = item.Tag as object[];
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
                var configIssues = e.Result as RemoteObjectCollection<PHPConfigIssue>;
                if (configIssues != null)
                {
                    foreach (var configIssue in configIssues)
                    {
                        var listViewItem = new ListViewItem(configIssue.SettingName);
                        if (String.IsNullOrEmpty(configIssue.CurrentValue))
                        {
                            listViewItem.SubItems.Add(Resources.ConfigIssueNone);
                        }
                        else
                        {
                            listViewItem.SubItems.Add(configIssue.CurrentValue);
                        }
                        listViewItem.SubItems.Add(configIssue.RecommendedValue);
                        listViewItem.Tag = new object[] { GetResourceStringByName(configIssue.IssueDescription), 
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
                var tag = _configIssuesListView.SelectedItems[0].Tag as object[];
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
