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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.Management.Client;
using Microsoft.Web.Management.Client.Win32;
using Web.Management.PHP.Config;

namespace Web.Management.PHP.Extensions
{

    [ModulePageIdentifier(Globals.PHPExtensionsPageIdentifier)]
    internal sealed class AllExtensionsPage : ModuleListPage, IModuleChildPage
    {
        private ColumnHeader _nameColumn;
        private ColumnHeader _stateColumn;
        private ModuleListPageGrouping _stateGrouping;
        private PageTaskList _taskList;
        private ModuleListPageSearchField[] _searchFields;
        private PHPIniFile _file;

        private const string NameString = "Name";
        private const string StateString = "State";
        private string _filterBy;
        private string _filterValue;
        private IModulePage _parentPage;

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
                if (_stateGrouping == null)
                {
                    _stateGrouping = new ModuleListPageGrouping(StateString, Resources.AllExtensionsPageStateField);
                }

                return new ModuleListPageGrouping[] { _stateGrouping };
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

        protected override ModuleListPageSearchField[] SearchFields
        {
            get
            {
                if (_searchFields == null)
                {
                    _searchFields = new ModuleListPageSearchField[]{
                        new ModuleListPageSearchField(NameString, Resources.AllSettingsPageNameField)};
                }

                return _searchFields;
            }
        }

        private PHPExtensionItem SelectedItem
        {
            get
            {
                if (ListView.SelectedIndices.Count == 1)
                {
                    return ListView.SelectedItems[0] as PHPExtensionItem;
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

        private void GetExtensions()
        {
            StartAsyncTask(Resources.AllExtensionsPageGettingExtensions, OnGetExtensions, OnGetExtensionsCompleted);
        }

        protected override ListViewGroup[] GetGroups(ModuleListPageGrouping grouping)
        {
            ListViewGroup[] result = new ListViewGroup[2];
            result[0] = new ListViewGroup("Enabled", Resources.AllExtensionsPageEnabledGroup);
            result[1] = new ListViewGroup("Disabled", Resources.AllExtensionsPageDisabledGroup);

            return result;
        }

        private void GoBack()
        {
            Navigate(typeof(PHPPage));
        }

        protected override void InitializeListPage()
        {
            _nameColumn = new ColumnHeader();
            _nameColumn.Text = Resources.AllExtensionsPageNameField;
            _nameColumn.Width = 160;

            _stateColumn = new ColumnHeader();
            _stateColumn.Text = Resources.AllExtensionsPageStateField;
            _stateColumn.Width = 60;

            ListView.Columns.AddRange(new ColumnHeader[] { _nameColumn, _stateColumn });

            ListView.MultiSelect = false;
            ListView.SelectedIndexChanged += new EventHandler(OnListViewSelectedIndexChanged);
            //ListView.DoubleClick += new EventHandler(OnListViewDoubleClick);
        }

        private void LoadExtensions(PHPIniFile file)
        {
            try
            {
                ListView.SuspendLayout();
                ListView.Items.Clear();

                foreach (PHPIniExtension extension in file.Extensions)
                {
                    if (_filterBy != null && _filterValue != null) {
                        if (_filterBy == NameString &&
                            extension.Name.IndexOf(_filterValue, StringComparison.OrdinalIgnoreCase) == -1)
                        {
                            continue;
                        }
                    }
                    ListView.Items.Add(new PHPExtensionItem(extension));
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
                GetExtensions();
            }
        }

        private void OnGetExtensions(object sender, DoWorkEventArgs e)
        {
            e.Result = Module.Proxy.GetPHPIniSettings();
        }

        private void OnGetExtensionsCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                object o = e.Result;

                _file = new PHPIniFile();
                _file.SetData(o);

                LoadExtensions(_file);
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
                foreach (PHPExtensionItem item in ListView.Items)
                {
                    if (grouping == _stateGrouping)
                    {
                        item.Group = ListView.Groups[item.State];
                    }
                }
            }
            finally
            {
                ListView.ResumeLayout();
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
                LoadExtensions(_file);
            }
            else
            {
                _filterBy = options.Field.Name;
                _filterValue = options.Text;
                LoadExtensions(_file);
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
                    ShowMessage(String.Format(CultureInfo.CurrentCulture, Resources.ErrorPHPIniFileDoesNotExist, physicalPath), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ex, Resources.ResourceManager);
            }
        }

        protected override void Refresh()
        {
            GetExtensions();
        }

        private void SetExtensionState(bool enabled)
        {
            PHPExtensionItem item = SelectedItem;
            item.Extension.Enabled = enabled;
            RemoteObjectCollection<PHPIniExtension> extensions = new RemoteObjectCollection<PHPIniExtension>();
            extensions.Add(item.Extension);
            Module.Proxy.UpdatePHPExtensions(extensions);

            Refresh();
        }


        private class PageTaskList : TaskList
        {
            private AllExtensionsPage _page;

            public PageTaskList(AllExtensionsPage page)
            {
                _page = page;
            }

            public void DisableExtension()
            {
                _page.SetExtensionState(false);
            }

            public void EnableExtension()
            {
                _page.SetExtensionState(true);
            }

            public override System.Collections.ICollection GetTaskItems()
            {
                List<TaskItem> tasks = new List<TaskItem>();

                if (_page.IsReadOnly)
                {
                    tasks.Add(new MessageTaskItem(MessageTaskItemType.Information, Resources.AllPagesPageIsReadOnly, "Information"));
                }
                else
                {
                    if (_page.SelectedItem != null)
                    {
                        if (_page.SelectedItem.Extension.Enabled)
                        {
                            tasks.Add(new MethodTaskItem("DisableExtension", Resources.AllExtensionsPageDisableTask, "Edit", null));
                        }
                        else
                        {
                            tasks.Add(new MethodTaskItem("EnableExtension", Resources.AllExtensionsPageEnableTask, "Edit", null));
                        }
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

        }


        private class PHPExtensionItem : ListViewItem
        {
            private PHPIniExtension _extension;

            public PHPExtensionItem(PHPIniExtension extension)
            {
                _extension = extension;
                Text = _extension.Name;
                SubItems.Add(this.State);

                if (!extension.Enabled) {
                    this.ForeColor = SystemColors.ControlDark;
                }
            }

            public PHPIniExtension Extension
            {
                get
                {
                    return _extension;
                }
            }

            public string State
            {
                get
                {
                    return _extension.Enabled ? "Enabled" : "Disabled";
                }
            }

        }
    }
}
