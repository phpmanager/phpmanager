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
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.Management.Client;
using Microsoft.Web.Management.Client.Win32;
using Web.Management.PHP.Config;

namespace Web.Management.PHP.Settings
{

    [ModulePageIdentifier(Globals.PHPSettingsPageIdentifier)]
    internal sealed class AllSettingsPage : ModuleListPage, IModuleChildPage
    {
        private ColumnHeader _nameColumn;
        private ColumnHeader _valueColumn;
        private ColumnHeader _sectionColumn;
        private ModuleListPageGrouping _sectionGrouping;
        private PageTaskList _taskList;
        private ModuleListPageSearchField[] _searchFields;
        private PHPIniFile _file;

        private const string NameString = "Name";
        private const string ValueString = "Value";
        private const string SectionString = "Section";
        private string _filterBy;
        private string _filterValue;

        // This is used to remember the name of added/updated setting so that 
        // it can be re-selected during refresh.
        private string _updatedSettingName;

        protected override bool CanRefresh
        {
            get
            {
                return true;
            }
        }

        protected override bool CanSearch
        {
            get
            {
                return true;
            }
        }

        protected override ModuleListPageGrouping DefaultGrouping
        {
            get
            {
                return Groupings[0];
            }
        }

        public override ModuleListPageGrouping[] Groupings
        {
            get
            {
                if (_sectionGrouping == null)
                {
                    _sectionGrouping = new ModuleListPageGrouping(SectionString, Resources.AllSettingsPageSectionField);
                }

                return new [] { _sectionGrouping };
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

        protected override ModuleListPageSearchField[] SearchFields
        {
            get
            {
                return _searchFields ?? (_searchFields = new[]
                    {
                        new ModuleListPageSearchField(NameString, Resources.AllSettingsPageNameField),
                        new ModuleListPageSearchField(ValueString, Resources.AllSettingsPageValueField),
                        new ModuleListPageSearchField(SectionString, Resources.AllSettingsPageSectionField)
                    });
            }
        }

        private PHPSettingItem SelectedItem
        {
            get
            {
                if (ListView.SelectedIndices.Count == 1)
                {
                    return ListView.SelectedItems[0] as PHPSettingItem;
                }

                return null;
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

        private void AddPHPSetting()
        {
            using (var dlg = new AddEditSettingDialog(Module, GetListOfSections()))
            {
                if (ShowDialog(dlg) == DialogResult.OK)
                {
                    // Save the name of added setting so that we can select it after refresh
                    _updatedSettingName = dlg.SettingName;
                    Refresh();
                }
            }
        }

        private void EditPHPSetting()
        {
            if (SelectedItem != null && !IsReadOnly)
            {
                using (var dlg = new AddEditSettingDialog(Module, SelectedItem.Setting))
                {
                    if (ShowDialog(dlg) == DialogResult.OK)
                    {
                        // Save the name of edited setting so that we can select it after refresh
                        _updatedSettingName = dlg.SettingName;
                        Refresh();
                    }
                }
            }
        }

        protected override ListViewGroup[] GetGroups(ModuleListPageGrouping grouping)
        {
            var groups = new Dictionary<string, ListViewGroup>();

            if (grouping == _sectionGrouping)
            {
                var items = ListView.Items;
                for (int i = 0; i < items.Count; i++)
                {
                    var item = (PHPSettingItem)items[i];
                    string sectionName = item.SectionName;
                    if (String.IsNullOrEmpty(sectionName)) {
                        continue;
                    }
                    if (!groups.ContainsKey(sectionName))
                    {
                        var sectionGroup = new ListViewGroup(sectionName, sectionName);
                        groups.Add(sectionName, sectionGroup);
                    }
                }
            }

            var result = new ListViewGroup[groups.Count];
            groups.Values.CopyTo(result, 0);
            return result;
        }

        private IEnumerable<string> GetListOfSections()
        {
            var sections = new SortedList<string, object>();

            foreach (PHPSettingItem item in ListView.Items)
            {
                var section = item.Setting.Section;
                if (String.IsNullOrEmpty(section))
                {
                    continue;
                }

                if (!sections.ContainsKey(section))
                {
                    sections.Add(item.Setting.Section, null);
                }
            }

            return sections.Keys;
        }

        private void GetSettings()
        {
            StartAsyncTask(Resources.AllSettingsPageGettingSettings, OnGetSettings, OnGetSettingsCompleted);
        }

        private void GoBack()
        {
            Navigate(typeof(PHPPage));
        }

        protected override void InitializeListPage()
        {
            _nameColumn = new ColumnHeader {Text = Resources.AllSettingsPageNameField, Width = 180};

            _valueColumn = new ColumnHeader {Text = Resources.AllSettingsPageValueField, Width = 180};

            _sectionColumn = new ColumnHeader {Text = Resources.AllSettingsPageSectionField, Width = 100};

            ListView.Columns.AddRange(new[] { _nameColumn, _valueColumn, _sectionColumn });

            ListView.MultiSelect = false;
            ListView.SelectedIndexChanged += OnListViewSelectedIndexChanged;
            ListView.KeyUp += OnListViewKeyUp;
            ListView.ItemActivate += OnListViewItemActivate;
        }

        private void LoadPHPIni(PHPIniFile file)
        {
            try
            {
                ListView.SuspendLayout();
                ListView.Items.Clear();

                foreach (var setting in file.Settings)
                {
                    if (_filterBy != null && _filterValue != null)
                    {
                        if (_filterBy == NameString &&
                            setting.Name.IndexOf(_filterValue, StringComparison.OrdinalIgnoreCase) == -1)
                        {
                            continue;
                        }
                        if (_filterBy == ValueString &&
                            setting.Value.IndexOf(_filterValue, StringComparison.OrdinalIgnoreCase) == -1)
                        {
                            continue;
                        }
                        if (_filterBy == SectionString &&
                            setting.Section.IndexOf(_filterValue, StringComparison.OrdinalIgnoreCase) == -1)
                        {
                            continue;
                        }
                    }

                    ListView.Items.Add(new PHPSettingItem(setting));
                }

                if (SelectedGrouping != null)
                {
                    Group(SelectedGrouping);
                }
            }
            finally
            {
                ListView.ResumeLayout();
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

        private void OnGetSettings(object sender, DoWorkEventArgs e)
        {
            e.Result = Module.Proxy.GetPHPIniSettings();
        }

        private void OnGetSettingsCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                object o = e.Result;

                _file = new PHPIniFile();
                _file.SetData(o);

                LoadPHPIni(_file);

                // If updated setting name was saved then use it to re-select it after refresh
                if (!String.IsNullOrEmpty(_updatedSettingName))
                {
                    SelectSettingByName(_updatedSettingName);
                    _updatedSettingName = null;
                }
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ex, Resources.ResourceManager);
            }
        }

        protected override void OnGroup(ModuleListPageGrouping grouping)
        {
            ListView.SuspendLayout();
            try
            {
                foreach (PHPSettingItem item in ListView.Items)
                {
                    if (grouping == _sectionGrouping)
                    {
                        item.Group = ListView.Groups[item.SectionName];
                    }
                }
            }
            finally
            {
                ListView.ResumeLayout();
            }
        }

        private void OnListViewItemActivate(object sender, EventArgs e)
        {
            EditPHPSetting();
        }

        private void OnListViewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete)
            {
                RemovePHPSetting();
                e.Handled = true;
            }
        }

        private void OnListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            Update();
        }

        protected override void OnSearch(ModuleListPageSearchOptions options)
        {
            if (options.ShowAll)
            {
                _filterBy = null;
                _filterValue = null;
                LoadPHPIni(_file);
            }
            else
            {
                _filterBy = options.Field.Name;
                _filterValue = options.Text;
                LoadPHPIni(_file);
            }
        }

        internal void OpenPHPIniFile()
        {
            try
            {
                string physicalPath = Module.Proxy.GetPHPIniPhysicalPath();
                if (!String.IsNullOrEmpty(physicalPath) &&
                    String.Equals(Path.GetExtension(physicalPath), ".ini", StringComparison.OrdinalIgnoreCase) &&
                    File.Exists(physicalPath))
                {
                    Process.Start(physicalPath);
                }
                else
                {
                    ShowMessage(String.Format(CultureInfo.CurrentCulture, Resources.ErrorFileDoesNotExist, physicalPath), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ex, Resources.ResourceManager);
            }
        }

        protected override void Refresh()
        {
            GetSettings();
        }

        private void RemovePHPSetting()
        {
            PHPSettingItem item = SelectedItem;

            if (item != null)
            {
                if (ShowMessage(Resources.PHPIniSettingDeleteConfirmation, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        Module.Proxy.RemoveSetting(item.Setting);
                        ListView.Items.Remove(item);
                    }
                    catch(Exception ex)
                    {
                        DisplayErrorMessage(ex, Resources.ResourceManager);
                    }
                }
            }
        }

        // Used to select an item by name after refresh
        private void SelectSettingByName(string name)
        {
            foreach (PHPSettingItem item in ListView.Items)
            {
                if (String.Equals(item.Setting.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    item.Selected = true;
                    item.EnsureVisible();
                    break;
                }
            }
        }

        protected override bool ShowHelp()
        {
            return ShowOnlineHelp();
        }

        protected override bool ShowOnlineHelp()
        {
            return Helper.Browse(Globals.AllSettingsOnlineHelp);
        }


        private class PageTaskList : TaskList
        {
            private readonly AllSettingsPage _page;

            public PageTaskList(AllSettingsPage page)
            {
                _page = page;
            }

            public void AddSetting()
            {
                _page.AddPHPSetting();
            }

            public void EditSetting()
            {
                _page.EditPHPSetting();
            }

            public override System.Collections.ICollection GetTaskItems()
            {
                var tasks = new List<TaskItem>();

                if (_page.IsReadOnly)
                {
                    tasks.Add(new MessageTaskItem(MessageTaskItemType.Information, Resources.AllPagesPageIsReadOnly, "Information"));
                }
                else
                {
                    tasks.Add(new MethodTaskItem("AddSetting", Resources.AllPagesAddTask, "Edit"));

                    if (_page.SelectedItem != null)
                    {
                        tasks.Add(new MethodTaskItem("EditSetting", Resources.AllSettingsPageEditTask, "Edit", null));
                        tasks.Add(new MethodTaskItem("RemoveSetting", Resources.AllSettingsPageRemoveTask, "Edit", null, Resources.Delete16));
                    }

                    if (_page.Connection.IsLocalConnection)
                    {
                        tasks.Add(new MethodTaskItem("OpenPHPIniFile", Resources.AllPagesOpenPHPIniTask, "Tasks", null));
                    }
                }

                tasks.Add(new MethodTaskItem("GoBack", Resources.AllPagesGoBackTask, "Tasks", null, Resources.GoBack16));

                return tasks;
            }

            public void GoBack()
            {
                _page.GoBack();
            }

            public void OpenPHPIniFile()
            {
                _page.OpenPHPIniFile();
            }

            public void RemoveSetting()
            {
                _page.RemovePHPSetting();
            }

        }


        private class PHPSettingItem : ListViewItem
        {
            private readonly PHPIniSetting _setting;

            public PHPSettingItem(PHPIniSetting setting)
            {
                _setting = setting;
                Text = _setting.Name;
                SubItems.Add(_setting.Value);
                SubItems.Add(_setting.Section);
            }

            public string SectionName
            {
                get
                {
                    return _setting.Section;
                }
            }

            public PHPIniSetting Setting
            {
                get
                {
                    return _setting;
                }
            }
        }
    }
}
