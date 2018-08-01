//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.Management.Client;
using Microsoft.Web.Management.Client.Win32;

namespace Web.Management.PHP.Setup
{

    [ModulePageIdentifier(Globals.PHPInfoPageIdentifier)]
    internal sealed class PHPInfoPage : ModulePage, IModuleChildPage
    {
        private WebBrowser _webBrowser;
        private Panel _panel;

        private string _baseUrl;
        private string _siteName;
        private string _configPath;
        private string _filepath;

        private bool _isLocalHandlersCollection;

        private PHPInfoTaskList _phpinfoTaskList;

        public PHPInfoPage()
        {
            InitializeComponent();
        }

        private bool IsLocalHandlersCollection
        {
            get
            {
                return _isLocalHandlersCollection;
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
                if (_phpinfoTaskList == null)
                {
                    _phpinfoTaskList = new PHPInfoTaskList(this);
                }

                tasks.Add(_phpinfoTaskList);

                return tasks;
            }
        }

        private void CheckForLocalHandlers()
        {
            StartAsyncTask(Resources.AllSettingsPageGettingSettings, OnCheckForLocalHandlers, OnCheckForLocalHandlersCompleted);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _webBrowser.Dispose();
            }
        }

        private MessageTaskItem GetLocalHandlersMessage()
        {
            if (!String.IsNullOrEmpty(_configPath))
            {
                var configPath = '/' + _configPath;
                return new MessageTaskItem(MessageTaskItemType.Information, String.Format(Resources.PHPInfoPageLocalHandlersFolder, configPath), String.Empty);
            }
            return new MessageTaskItem(MessageTaskItemType.Information, String.Format(Resources.PHPInfoPageLocalHandlersSite, _siteName), String.Empty);
        }

        private void GoBack()
        {
            Navigate(typeof(PHPPage));
        }

        protected override void Initialize(object navigationData)
        {
            base.Initialize(navigationData);
            var siteInfo = navigationData as string[];
            _baseUrl = siteInfo[0];
            _siteName = siteInfo[1];
            _configPath = siteInfo[2];
        }

        private void InitializeComponent()
        {
            _webBrowser = new WebBrowser();
            _panel = new Panel();
            _panel.SuspendLayout();
            SuspendLayout();
            // 
            // _webBrowser
            // 
            _webBrowser.AllowNavigation = false;
            _webBrowser.AllowWebBrowserDrop = false;
            _webBrowser.Dock = DockStyle.Fill;
            _webBrowser.Location = new System.Drawing.Point(0, 0);
            _webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            _webBrowser.Name = "_webBrowser";
            _webBrowser.ScriptErrorsSuppressed = true;
            _webBrowser.Size = new System.Drawing.Size(296, 284);
            _webBrowser.TabIndex = 0;
            _webBrowser.DocumentCompleted += OnWebBrowserDocumentCompleted;
            // 
            // _panel
            // 
            _panel.BorderStyle = BorderStyle.Fixed3D;
            _panel.Controls.Add(_webBrowser);
            _panel.Dock = DockStyle.Fill;
            _panel.Location = new System.Drawing.Point(0, 12);
            _panel.Name = "_panel";
            _panel.Size = new System.Drawing.Size(300, 288);
            _panel.TabIndex = 0;
            // 
            // PHPInfoPage
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            Controls.Add(_panel);
            Name = "PHPInfoPage";
            Padding = new Padding(0, 12, 0, 0);
            _panel.ResumeLayout(false);
            ResumeLayout(false);

        }

        protected override void OnActivated(bool initialActivation)
        {
            base.OnActivated(initialActivation);

            if (initialActivation)
            {
                ShowPHPInfo();
                CheckForLocalHandlers();
            }
        }

        private void OnCheckForLocalHandlers(object sender, DoWorkEventArgs e)
        {
            e.Result = Module.Proxy.CheckForLocalPHPHandler(_siteName, _configPath);
        }

        private void OnCheckForLocalHandlersCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                _isLocalHandlersCollection = (bool)e.Result;
            }
            catch(Exception ex)
            {
                DisplayErrorMessage(ex, Resources.ResourceManager);
            }
        }

        private void OnShowPHPInfo(object sender, DoWorkEventArgs e)
        {
            e.Result = Module.Proxy.CreatePHPInfo(_siteName);
        }

        private void OnShowPHPInfoCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                _filepath = (string)e.Result;
                var baseUri = new Uri(_baseUrl);
                var fullUri = new Uri(baseUri, Path.GetFileName(_filepath));
                _webBrowser.AllowNavigation = true;
                _webBrowser.Navigate(fullUri);
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ex, Resources.ResourceManager);
            }
        }

        private void OnWebBrowserDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (!String.IsNullOrEmpty(_filepath))
            {
                try
                {
                    Module.Proxy.RemovePHPInfo(_filepath);
                }
                catch (Exception ex)
                {
                    DisplayErrorMessage(ex, Resources.ResourceManager);
                }
                finally
                {
                    _filepath = String.Empty;
                    _webBrowser.AllowNavigation = false;
                }
            }
        }

        protected override bool ShowHelp()
        {
            return ShowOnlineHelp();
        }

        protected override bool ShowOnlineHelp()
        {
            return Helper.Browse(Globals.PHPInfoPageOnlineHelp);
        }

        private void ShowPHPInfo()
        {
            StartAsyncTask(Resources.AllSettingsPageGettingSettings, OnShowPHPInfo, OnShowPHPInfoCompleted);
        }


        private class PHPInfoTaskList : TaskList
        {

            private readonly PHPInfoPage _page;

            public PHPInfoTaskList(PHPInfoPage page)
            {
                _page = page;
            }

            public override ICollection GetTaskItems()
            {
                if (_page.InProgress)
                {
                    return new TaskItem[] { };
                }

                var tasks = new List<TaskItem>();
                tasks.Add(new MethodTaskItem("RefreshPHPInfo", Resources.PHPInfoRefreshPHPInfo, "Set"));
                tasks.Add(new MethodTaskItem("GoBack", Resources.AllPagesGoBackTask, "Tasks", null, Resources.GoBack16));

                if (_page.IsLocalHandlersCollection)
                {
                    tasks.Add(_page.GetLocalHandlersMessage());
                }

                return tasks;
            }

            public void GoBack()
            {
                _page.GoBack();
            }

            public void RefreshPHPInfo()
            {
                _page.ShowPHPInfo();
            }
            
        }
    }
}