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

        private readonly string[] _settingNames = {
            "error_reporting",
            "display_errors",
            "track_errors",
            "html_errors",
            "log_errors",
            "fastcgi.logging"
        };
        private readonly string[] _settingsDevValues = {
            "E_ALL | E_STRICT",
            "On",
            "On",
            "On",
            "On",
            "1"
        };
        private readonly string[] _settingsProdValues = {
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

        public IModulePage ParentPage { get; set; }

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
            var appliedChanges = false;

            string[] settingValues = null;

            Debug.Assert(_devMachineRadioButton.Checked || _prodMachineRadioButton.Checked);
            if (_devMachineRadioButton.Checked)
            {
                settingValues = _settingsDevValues;
            }
            else if (_prodMachineRadioButton.Checked)
            {
                settingValues = _settingsProdValues;
            }

            var settings = new RemoteObjectCollection<PHPIniSetting>();
            for (int i = 0; i < settingValues.Length; i++)
            {
                settings.Add(new PHPIniSetting(_settingNames[i], settingValues[i], "PHP"));
            }

            string errorLogValue = '"' + _errorLogFileTextBox.Text.Trim(new[] {' ', '"'}) + '"';
            settings.Add(new PHPIniSetting("error_log", errorLogValue, "PHP"));

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
            _serverTypeGroupBox = new GroupBox();
            _prodMachineLabel = new Label();
            _prodMachineRadioButton = new RadioButton();
            _devMachineLabel = new Label();
            _selectServerTypeLabel = new Label();
            _devMachineRadioButton = new RadioButton();
            _errorLogFileLabel = new Label();
            _errorLogFileTextBox = new TextBox();
            _errorLogBrowseButton = new Button();
            _serverTypeGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // _serverTypeGroupBox
            // 
            _serverTypeGroupBox.Controls.Add(_prodMachineLabel);
            _serverTypeGroupBox.Controls.Add(_prodMachineRadioButton);
            _serverTypeGroupBox.Controls.Add(_devMachineLabel);
            _serverTypeGroupBox.Controls.Add(_selectServerTypeLabel);
            _serverTypeGroupBox.Controls.Add(_devMachineRadioButton);
            _serverTypeGroupBox.Location = new System.Drawing.Point(4, 12);
            _serverTypeGroupBox.Name = "_serverTypeGroupBox";
            _serverTypeGroupBox.Size = new System.Drawing.Size(472, 222);
            _serverTypeGroupBox.TabIndex = 0;
            _serverTypeGroupBox.TabStop = false;
            _serverTypeGroupBox.Text = Resources.ErrorReportingPageServerType;
            // 
            // _prodMachineLabel
            // 
            _prodMachineLabel.Location = new System.Drawing.Point(37, 150);
            _prodMachineLabel.Name = "_prodMachineLabel";
            _prodMachineLabel.Size = new System.Drawing.Size(413, 48);
            _prodMachineLabel.TabIndex = 4;
            _prodMachineLabel.Text = Resources.ErrorReportingPageProdMachineDesc;
            // 
            // _prodMachineRadioButton
            // 
            _prodMachineRadioButton.AutoSize = true;
            _prodMachineRadioButton.Location = new System.Drawing.Point(20, 130);
            _prodMachineRadioButton.Name = "_prodMachineRadioButton";
            _prodMachineRadioButton.Size = new System.Drawing.Size(119, 17);
            _prodMachineRadioButton.TabIndex = 3;
            _prodMachineRadioButton.TabStop = true;
            _prodMachineRadioButton.Text = Resources.ErrorReportingPageProdMachine;
            _prodMachineRadioButton.UseVisualStyleBackColor = true;
            _prodMachineRadioButton.CheckedChanged += OnProdMachineRadioButtonCheckedChanged;
            // 
            // _devMachineLabel
            // 
            _devMachineLabel.Location = new System.Drawing.Point(37, 75);
            _devMachineLabel.Name = "_devMachineLabel";
            _devMachineLabel.Size = new System.Drawing.Size(413, 46);
            _devMachineLabel.TabIndex = 2;
            _devMachineLabel.Text = Resources.ErrorReportingPageDevMachineDesc;
            // 
            // _selectServerTypeLabel
            // 
            _selectServerTypeLabel.Location = new System.Drawing.Point(6, 20);
            _selectServerTypeLabel.Name = "_selectServerTypeLabel";
            _selectServerTypeLabel.Size = new System.Drawing.Size(458, 23);
            _selectServerTypeLabel.TabIndex = 0;
            _selectServerTypeLabel.Text = Resources.ErrorReportingPageSelectServerType;
            // 
            // _devMachineRadioButton
            // 
            _devMachineRadioButton.AutoSize = true;
            _devMachineRadioButton.Location = new System.Drawing.Point(20, 55);
            _devMachineRadioButton.Name = "_devMachineRadioButton";
            _devMachineRadioButton.Size = new System.Drawing.Size(131, 17);
            _devMachineRadioButton.TabIndex = 1;
            _devMachineRadioButton.TabStop = true;
            _devMachineRadioButton.Text = Resources.ErrorReportingPageDevMachine;
            _devMachineRadioButton.UseVisualStyleBackColor = true;
            _devMachineRadioButton.CheckedChanged += OnDevMachineRadioButtonCheckedChanged;
            // 
            // _errorLogFileLabel
            // 
            _errorLogFileLabel.AutoSize = true;
            _errorLogFileLabel.Location = new System.Drawing.Point(3, 253);
            _errorLogFileLabel.Name = "_errorLogFileLabel";
            _errorLogFileLabel.Size = new System.Drawing.Size(65, 13);
            _errorLogFileLabel.TabIndex = 1;
            _errorLogFileLabel.Text = Resources.ErrorReportingErrorLogFile;
            // 
            // _errorLogFileTextBox
            // 
            _errorLogFileTextBox.Location = new System.Drawing.Point(7, 269);
            _errorLogFileTextBox.Name = "_errorLogFileTextBox";
            _errorLogFileTextBox.Size = new System.Drawing.Size(438, 20);
            _errorLogFileTextBox.TabIndex = 2;
            _errorLogFileTextBox.TextChanged += OnErrorLogFileTextBoxTextChanged;
            // 
            // _errorLogBrowseButton
            // 
            _errorLogBrowseButton.Location = new System.Drawing.Point(451, 267);
            _errorLogBrowseButton.Name = "_errorLogBrowseButton";
            _errorLogBrowseButton.Size = new System.Drawing.Size(25, 23);
            _errorLogBrowseButton.TabIndex = 3;
            _errorLogBrowseButton.Text = @"...";
            _errorLogBrowseButton.UseVisualStyleBackColor = true;
            _errorLogBrowseButton.Click += OnErrorLogBrowseButtonClick;
            // 
            // ErrorReportingPage
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScroll = true;
            Controls.Add(_errorLogBrowseButton);
            Controls.Add(_errorLogFileTextBox);
            Controls.Add(_errorLogFileLabel);
            Controls.Add(_serverTypeGroupBox);
            Name = "ErrorReportingPage";
            Size = new System.Drawing.Size(480, 360);
            _serverTypeGroupBox.ResumeLayout(false);
            _serverTypeGroupBox.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        private void InitializeUI()
        {
            if (IsReadOnly)
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
                _errorLogFileTextBox.AutoCompleteMode = AutoCompleteMode.Suggest;
                _errorLogFileTextBox.AutoCompleteSource = AutoCompleteSource.FileSystem;
            }
            else
            {
                _errorLogBrowseButton.Visible = false;
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
            _hasChanges = _hasChanges || oldHasChanges;

            if (_hasChanges)
            {
                Update();
            }
        }

        private void OnErrorLogBrowseButtonClick(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
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

                var file = new PHPIniFile();
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
            _hasChanges = _hasChanges || oldHasChanges;

            if (_hasChanges)
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
            PHPIniSetting setting = file.GetSetting(_settingNames[0]);
            string[] settingValues = null;

            if (setting != null)
            {
                if (String.Equals(setting.GetTrimmedValue(), _settingsDevValues[0]))
                {
                    _errorReportingPreset = ErrorReportingPreset.Development;
                    settingValues = _settingsDevValues;
                }
                else if (String.Equals(setting.GetTrimmedValue(), _settingsProdValues[0]))
                {
                    _errorReportingPreset = ErrorReportingPreset.Production;
                    settingValues = _settingsProdValues;
                }

                int i = 1;
                while (_errorReportingPreset != ErrorReportingPreset.Undefined && i < _settingNames.Length)
                {
                    setting = file.GetSetting(_settingNames[i]);
                    if (setting == null || !String.Equals(setting.GetTrimmedValue(), settingValues[i]))
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
                _errorLogFile = setting.GetTrimmedValue();
                _errorLogFileTextBox.Text = setting.GetTrimmedValue();
            }
        }

        private class PageTaskList : TaskList
        {
            private readonly ErrorReportingPage _page;

            public PageTaskList(ErrorReportingPage page)
            {
                _page = page;
            }

            public override System.Collections.ICollection GetTaskItems()
            {
                var tasks = new List<TaskItem>();

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
