//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Web.Management.Client;
using Microsoft.Web.Management.Client.Win32;
using Web.Management.PHP.Config;

namespace Web.Management.PHP.Settings
{

    [ModulePageIdentifier(Globals.ErrorReportingPageIdentifier)]
    internal sealed class ErrorReportingPage : ModuleDialogPage, IModuleChildPage
    {

        enum ErrorReportingPreset { Undefined = 0, Development = 1, Production = 2 };

        private string _errorLogFile = String.Empty;
        private ErrorReportingPreset _errorReportingPreset = ErrorReportingPreset.Undefined;
        private bool _hasChanges;

        private readonly string[] SettingNames = {
            "error_reporting",
            "display_errors",
            "track_errors",
            "html_errors",
            "log_errors",
            "fastcgi.logging"
        };
        private readonly string[] SettingsDevValues = {
            "E_ALL | E_STRICT",
            "On",
            "On",
            "On",
            "On",
            "1"
        };
        private readonly string[] SettingsProdValues = {
            "E_ALL & ~E_DEPRECATED",
            "Off",
            "Off",
            "Off",
            "On",
            "0"
        };

        private Label _devMachineLabel;
        private Label _selectServerTypeLabel;
        private RadioButton _devMachineRadioButton;
        private RadioButton _prodMachineRadioButton;
        private Label _prodMachineLabel;
        private Label _errorLogFileLabel;
        private TextBox _errorLogFileTextBox;
        private Button _errorLogBrowseButton;
        private GroupBox _serverTypeGroupBox;

        private PageTaskList _taskList;
        private IModulePage _parentPage;

        protected override bool CanApplyChanges
        {
            get 
            {
                return _hasChanges && (_devMachineRadioButton.Checked || _prodMachineRadioButton.Checked);
            }
        }

        protected override bool HasChanges
        {
            get
            {
                return _hasChanges;
            }
        }

        internal bool IsReadOnly
        {
            get
            {
                return Connection.ConfigurationPath.PathType == Microsoft.Web.Management.Server.ConfigurationPathType.Site &&
                    !Connection.IsUserServerAdministrator;
            }
        }

        private new PHPModule Module
        {
            get
            {
                return (PHPModule)base.Module;
            }
        }

        public IModulePage ParentPage
        {
            get
            {
                return _parentPage;
            }
            set
            {
                _parentPage = value;
            }
        }

        protected override TaskListCollection Tasks
        {
            get
            {
                TaskListCollection tasks = base.Tasks;
                if (_taskList == null)
                {
                    _taskList = new PageTaskList(this);
                }

                tasks.Add(_taskList);

                return tasks;
            }
        }

        protected override bool ApplyChanges()
        {
            bool appliedChanges = false;

            string[] settingValues = null;

            Debug.Assert(_devMachineRadioButton.Checked || _prodMachineRadioButton.Checked);
            if (_devMachineRadioButton.Checked)
            {
                settingValues = SettingsDevValues;
            }
            else if (_prodMachineRadioButton.Checked)
            {
                settingValues = SettingsProdValues;
            }

            RemoteObjectCollection<PHPIniSetting> settings = new RemoteObjectCollection<PHPIniSetting>();
            for (int i = 0; i < settingValues.Length; i++)
            {
                settings.Add(new PHPIniSetting(SettingNames[i], settingValues[i], "PHP"));
            }

            settings.Add(new PHPIniSetting("error_log", _errorLogFileTextBox.Text.Trim(), "PHP"));

            try
            {
                Module.Proxy.AddOrUpdateSettings(settings);
                appliedChanges = true;
                
                // Update the values used for determining if changes have been made
                _errorLogFile = _errorLogFileTextBox.Text;
                if (_devMachineRadioButton.Checked)
                {
                    _errorReportingPreset = ErrorReportingPreset.Development;
                }
                else if (_prodMachineRadioButton.Checked)
                {
                    _errorReportingPreset = ErrorReportingPreset.Production;
                }
                _hasChanges = false;
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ex, Resources.ResourceManager);
            }
            finally
            {
                Update();
            }

            return appliedChanges;
        }

        protected override void CancelChanges()
        {
            if (_errorReportingPreset == ErrorReportingPreset.Development)
            {
                _devMachineRadioButton.Checked = true;
            }
            else if (_errorReportingPreset == ErrorReportingPreset.Production)
            {
                _prodMachineRadioButton.Checked = true;
            }
            else
            {
                _devMachineRadioButton.Checked = false;
                _prodMachineRadioButton.Checked = false;
            }

            _errorLogFileTextBox.Text = _errorLogFile;

            _hasChanges = false;
            Update();
        }

        private void GetSettings()
        {
            StartAsyncTask(Resources.AllSettingsPageGettingSettings, OnGetSettings, OnGetSettingsCompleted);
        }

        private void GoBack()
        {
            Navigate(typeof(PHPPage));
        }

        protected override void Initialize(object navigationData)
        {
            base.Initialize(navigationData);

            InitializeComponent();
            InitializeUI();

        }

        private void InitializeComponent()
        {
            this._serverTypeGroupBox = new System.Windows.Forms.GroupBox();
            this._prodMachineLabel = new System.Windows.Forms.Label();
            this._prodMachineRadioButton = new System.Windows.Forms.RadioButton();
            this._devMachineLabel = new System.Windows.Forms.Label();
            this._selectServerTypeLabel = new System.Windows.Forms.Label();
            this._devMachineRadioButton = new System.Windows.Forms.RadioButton();
            this._errorLogFileLabel = new System.Windows.Forms.Label();
            this._errorLogFileTextBox = new System.Windows.Forms.TextBox();
            this._errorLogBrowseButton = new System.Windows.Forms.Button();
            this._serverTypeGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // _serverTypeGroupBox
            // 
            this._serverTypeGroupBox.Controls.Add(this._prodMachineLabel);
            this._serverTypeGroupBox.Controls.Add(this._prodMachineRadioButton);
            this._serverTypeGroupBox.Controls.Add(this._devMachineLabel);
            this._serverTypeGroupBox.Controls.Add(this._selectServerTypeLabel);
            this._serverTypeGroupBox.Controls.Add(this._devMachineRadioButton);
            this._serverTypeGroupBox.Location = new System.Drawing.Point(4, 12);
            this._serverTypeGroupBox.Name = "_serverTypeGroupBox";
            this._serverTypeGroupBox.Size = new System.Drawing.Size(472, 222);
            this._serverTypeGroupBox.TabIndex = 0;
            this._serverTypeGroupBox.TabStop = false;
            this._serverTypeGroupBox.Text = Resources.ErrorReportingPageServerType;
            // 
            // _prodMachineLabel
            // 
            this._prodMachineLabel.Location = new System.Drawing.Point(37, 150);
            this._prodMachineLabel.Name = "_prodMachineLabel";
            this._prodMachineLabel.Size = new System.Drawing.Size(413, 48);
            this._prodMachineLabel.TabIndex = 4;
            this._prodMachineLabel.Text = Resources.ErrorReportingPageProdMachineDesc;
            // 
            // _prodMachineRadioButton
            // 
            this._prodMachineRadioButton.AutoSize = true;
            this._prodMachineRadioButton.Location = new System.Drawing.Point(20, 130);
            this._prodMachineRadioButton.Name = "_prodMachineRadioButton";
            this._prodMachineRadioButton.Size = new System.Drawing.Size(119, 17);
            this._prodMachineRadioButton.TabIndex = 3;
            this._prodMachineRadioButton.TabStop = true;
            this._prodMachineRadioButton.Text = Resources.ErrorReportingPageProdMachine;
            this._prodMachineRadioButton.UseVisualStyleBackColor = true;
            this._prodMachineRadioButton.CheckedChanged += new System.EventHandler(this.OnProdMachineRadioButtonCheckedChanged);
            // 
            // _devMachineLabel
            // 
            this._devMachineLabel.Location = new System.Drawing.Point(37, 75);
            this._devMachineLabel.Name = "_devMachineLabel";
            this._devMachineLabel.Size = new System.Drawing.Size(413, 46);
            this._devMachineLabel.TabIndex = 2;
            this._devMachineLabel.Text = Resources.ErrorReportingPageDevMachineDesc;
            // 
            // _selectServerTypeLabel
            // 
            this._selectServerTypeLabel.Location = new System.Drawing.Point(6, 20);
            this._selectServerTypeLabel.Name = "_selectServerTypeLabel";
            this._selectServerTypeLabel.Size = new System.Drawing.Size(458, 23);
            this._selectServerTypeLabel.TabIndex = 0;
            this._selectServerTypeLabel.Text = Resources.ErrorReportingPageSelectServerType;
            // 
            // _devMachineRadioButton
            // 
            this._devMachineRadioButton.AutoSize = true;
            this._devMachineRadioButton.Location = new System.Drawing.Point(20, 55);
            this._devMachineRadioButton.Name = "_devMachineRadioButton";
            this._devMachineRadioButton.Size = new System.Drawing.Size(131, 17);
            this._devMachineRadioButton.TabIndex = 1;
            this._devMachineRadioButton.TabStop = true;
            this._devMachineRadioButton.Text = Resources.ErrorReportingPageDevMachine;
            this._devMachineRadioButton.UseVisualStyleBackColor = true;
            this._devMachineRadioButton.CheckedChanged += new System.EventHandler(this.OnDevMachineRadioButtonCheckedChanged);
            // 
            // _errorLogFileLabel
            // 
            this._errorLogFileLabel.AutoSize = true;
            this._errorLogFileLabel.Location = new System.Drawing.Point(3, 253);
            this._errorLogFileLabel.Name = "_errorLogFileLabel";
            this._errorLogFileLabel.Size = new System.Drawing.Size(65, 13);
            this._errorLogFileLabel.TabIndex = 1;
            this._errorLogFileLabel.Text = Resources.ErrorReportingErrorLogFile;
            // 
            // _errorLogFileTextBox
            // 
            this._errorLogFileTextBox.Location = new System.Drawing.Point(7, 269);
            this._errorLogFileTextBox.Name = "_errorLogFileTextBox";
            this._errorLogFileTextBox.Size = new System.Drawing.Size(438, 20);
            this._errorLogFileTextBox.TabIndex = 2;
            this._errorLogFileTextBox.TextChanged += new System.EventHandler(this.OnErrorLogFileTextBoxTextChanged);
            // 
            // _errorLogBrowseButton
            // 
            this._errorLogBrowseButton.Location = new System.Drawing.Point(451, 267);
            this._errorLogBrowseButton.Name = "_errorLogBrowseButton";
            this._errorLogBrowseButton.Size = new System.Drawing.Size(25, 23);
            this._errorLogBrowseButton.TabIndex = 3;
            this._errorLogBrowseButton.Text = "...";
            this._errorLogBrowseButton.UseVisualStyleBackColor = true;
            this._errorLogBrowseButton.Click += new System.EventHandler(this.OnErrorLogBrowseButtonClick);
            // 
            // ErrorReportingPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScroll = true;
            this.Controls.Add(this._errorLogBrowseButton);
            this.Controls.Add(this._errorLogFileTextBox);
            this.Controls.Add(this._errorLogFileLabel);
            this.Controls.Add(this._serverTypeGroupBox);
            this.Name = "ErrorReportingPage";
            this.Size = new System.Drawing.Size(480, 360);
            this._serverTypeGroupBox.ResumeLayout(false);
            this._serverTypeGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void InitializeUI()
        {
            if (this.IsReadOnly)
            {
                _devMachineRadioButton.Enabled = false;
                _devMachineLabel.Enabled = false;
                _prodMachineRadioButton.Enabled = false;
                _prodMachineLabel.Enabled = false;
                _errorLogFileTextBox.Enabled = false;
                _errorLogBrowseButton.Enabled = false;
            }

            // Only show the auto suggest if it is a local connection.
            // Otherwise do not show auto suggest and also hide the browse button.
            if (Connection.IsLocalConnection)
            {
                this._errorLogFileTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
                this._errorLogFileTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            }
            else
            {
                this._errorLogBrowseButton.Visible = false;
            }
        }

        protected override void OnActivated(bool initialActivation)
        {
            base.OnActivated(initialActivation);

            if (initialActivation)
            {
                GetSettings();
            }
        }

        private void OnDevMachineRadioButtonCheckedChanged(object sender, EventArgs e)
        {
            bool oldHasChanges = _hasChanges;
            if (_errorReportingPreset == ErrorReportingPreset.Development)
            {
                _hasChanges = !_devMachineRadioButton.Checked;
            }
            else
            {
                _hasChanges = _devMachineRadioButton.Checked;
            }
            if (oldHasChanges != _hasChanges)
            {
                Update();
            }
        }

        private void OnErrorLogBrowseButtonClick(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = Resources.ErrorLogSaveDialogTitle;
                dlg.Filter = Resources.ErrorLogSaveDialogFilter;
                if (!String.IsNullOrEmpty(_errorLogFileTextBox.Text))
                {
                    dlg.InitialDirectory = System.IO.Path.GetDirectoryName(_errorLogFileTextBox.Text.Trim());
                }
                else
                {
                    dlg.InitialDirectory = Environment.ExpandEnvironmentVariables("%SystemDrive%");
                }
                
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _errorLogFileTextBox.Text = dlg.FileName;
                }
            }
        }

        private void OnErrorLogFileTextBoxTextChanged(object sender, EventArgs e)
        {
            if (!String.Equals(_errorLogFileTextBox.Text, _errorLogFile, StringComparison.OrdinalIgnoreCase))
            {
                _hasChanges = true;
                Update();
            }
        }

        private void OnGetSettings(object sender, DoWorkEventArgs e)
        {
            e.Result = Module.Proxy.GetPHPIniSettings();
        }

        private void OnGetSettingsCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                object o = e.Result;

                PHPIniFile file = new PHPIniFile();
                file.SetData(o);

                UpdateUI(file);
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ex, Resources.ResourceManager);
            }
        }

        private void OnProdMachineRadioButtonCheckedChanged(object sender, EventArgs e)
        {
            bool oldHasChanges = _hasChanges;
            if (_errorReportingPreset == ErrorReportingPreset.Production)
            {
                _hasChanges = !_prodMachineRadioButton.Checked;
            }
            else
            {
                _hasChanges = _prodMachineRadioButton.Checked;
            }
            if (oldHasChanges != _hasChanges)
            {
                Update();
            }
        }

        protected override bool ShowHelp()
        {
            return ShowOnlineHelp();
        }

        protected override bool ShowOnlineHelp()
        {
            return Helper.Browse(Globals.ErrorReportingOnlineHelp);
        }

        private void UpdateUI(PHPIniFile file)
        {
            PHPIniSetting setting = file.GetSetting(SettingNames[0]);
            string[] settingValues = null;

            if (setting != null)
            {
                if (String.Equals(setting.TrimmedValue, SettingsDevValues[0]))
                {
                    _errorReportingPreset = ErrorReportingPreset.Development;
                    settingValues = SettingsDevValues;
                }
                else if (String.Equals(setting.TrimmedValue, SettingsProdValues[0]))
                {
                    _errorReportingPreset = ErrorReportingPreset.Production;
                    settingValues = SettingsProdValues;
                }

                int i = 1;
                while (_errorReportingPreset != ErrorReportingPreset.Undefined && i < SettingNames.Length)
                {
                    setting = file.GetSetting(SettingNames[i]);
                    if (setting == null || !String.Equals(setting.TrimmedValue, settingValues[i]))
                    {
                        _errorReportingPreset = ErrorReportingPreset.Undefined;
                    }
                    i = i + 1;
                }
            }

            if (_errorReportingPreset == ErrorReportingPreset.Development)
            {
                _devMachineRadioButton.Checked = true;
            }
            else if (_errorReportingPreset == ErrorReportingPreset.Production)
            {
                _prodMachineRadioButton.Checked = true;
            }

            setting = file.GetSetting("error_log");
            if (setting != null)
            {
                _errorLogFile = setting.TrimmedValue;
                _errorLogFileTextBox.Text = setting.TrimmedValue;
            }
        }

        private class PageTaskList : TaskList
        {
            private ErrorReportingPage _page;

            public PageTaskList(ErrorReportingPage page)
            {
                _page = page;
            }

            public override System.Collections.ICollection GetTaskItems()
            {
                List<TaskItem> tasks = new List<TaskItem>();

                if (_page.IsReadOnly)
                {
                    tasks.Add(new MessageTaskItem(MessageTaskItemType.Information, Resources.AllPagesPageIsReadOnly, "Information"));
                }

                tasks.Add(new MethodTaskItem("GoBack", Resources.AllPagesGoBackTask, "Tasks", null, Resources.GoBack16));

                return tasks;
            }

            public void GoBack()
            {
                _page.GoBack();
            }
        }

    }

}
