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
        private IModulePage _parentPage;
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

        private static string GetConfigurationPath(string baseUrl)
        {
            Uri baseUri = new Uri(baseUrl);
            string path = baseUri.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped);
            if (!String.IsNullOrEmpty(path))
            {
                return '/' + path.TrimEnd('/');
            }
            else
            {
                return String.Empty;
            }
        }

        private MessageTaskItem GetLocalHandlersMessage()
        {
            if (!String.IsNullOrEmpty(_configPath))
            {
                return new MessageTaskItem(MessageTaskItemType.Information, String.Format(Resources.PHPInfoPageLocalHandlersFolder, _configPath), String.Empty);
            }
            else
            {
                return new MessageTaskItem(MessageTaskItemType.Information, String.Format(Resources.PHPInfoPageLocalHandlersSite, _siteName), String.Empty);
            }
        }

        private void GoBack()
        {
            Navigate(typeof(PHPPage));
        }

        protected override void Initialize(object navigationData)
        {
            base.Initialize(navigationData);
            string[] siteInfo = navigationData as string[];
            this._baseUrl = siteInfo[0];
            this._siteName = siteInfo[1];
            this._configPath = GetConfigurationPath(this._baseUrl);
        }

        private void InitializeComponent()
        {
            this._webBrowser = new System.Windows.Forms.WebBrowser();
            this._panel = new System.Windows.Forms.Panel();
            this._panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _webBrowser
            // 
            this._webBrowser.AllowNavigation = false;
            this._webBrowser.AllowWebBrowserDrop = false;
            this._webBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this._webBrowser.Location = new System.Drawing.Point(0, 0);
            this._webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this._webBrowser.Name = "_webBrowser";
            this._webBrowser.ScriptErrorsSuppressed = true;
            this._webBrowser.Size = new System.Drawing.Size(296, 284);
            this._webBrowser.TabIndex = 0;
            this._webBrowser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.OnWebBrowserDocumentCompleted);
            // 
            // _panel
            // 
            this._panel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this._panel.Controls.Add(this._webBrowser);
            this._panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._panel.Location = new System.Drawing.Point(0, 12);
            this._panel.Name = "_panel";
            this._panel.Size = new System.Drawing.Size(300, 288);
            this._panel.TabIndex = 0;
            // 
            // PHPInfoPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this._panel);
            this.Name = "PHPInfoPage";
            this.Padding = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this._panel.ResumeLayout(false);
            this.ResumeLayout(false);

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
            e.Result = Module.Proxy.CheckForLocalPHPHandler(this._siteName, this._configPath);
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
            e.Result = Module.Proxy.CreatePHPInfo(this._siteName);
        }

        private void OnShowPHPInfoCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                _filepath = (string)e.Result;
                Uri baseUri = new Uri(this._baseUrl);
                Uri fullUri = new Uri(baseUri, Path.GetFileName(_filepath));
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

            private PHPInfoPage _page;

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

                List<TaskItem> tasks = new List<TaskItem>();
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