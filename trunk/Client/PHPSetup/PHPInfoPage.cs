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
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.Management.Client;
using Microsoft.Web.Management.Client.Win32;
using Microsoft.Web.Management.Server;

namespace Web.Management.PHP.PHPSetup
{

    [ModulePageIdentifier(Globals.PHPInfoPageIdentifier)]
    internal sealed class PHPInfoPage : ModulePage, IModuleChildPage
    {
        private IModulePage _parentPage;
        private WebBrowser _webBrowser;
        private Panel _panel;

        private string _domain;
        private string _filepath;

        private PHPInfoTaskList _phpinfoTaskList;

        public PHPInfoPage()
        {
            InitializeComponent();
        }

        public string Domain 
        {
            get
            {
                return _domain;
            }
            set
            {
                _domain = value;
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

        private void GoBack()
        {
            Navigate(typeof(PHPPage));
        }

        protected override void Initialize(object navigationData)
        {
            base.Initialize(navigationData);
            this.Domain = navigationData as string;
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

        private void SelectDomain()
        {
            using(SelectDomainDialog dlg = new SelectDomainDialog(this.Module, this.Connection, this.Domain))
            {
                if (ShowDialog(dlg) == DialogResult.OK)
                {
                    this.Domain = dlg.SiteDomain;
                    ShowPHPInfo();
                }
            }
        }

        private void ShowPHPInfo()
        {
            try
            {
                if (!String.IsNullOrEmpty(_domain))
                {
                    _filepath = Module.Proxy.CreatePHPInfo();
                    string url = this.Domain + Path.GetFileName(_filepath);
                    _webBrowser.AllowNavigation = true;
                    _webBrowser.Navigate(url);
                }
            }
            catch(Exception ex)
            {
                DisplayErrorMessage(ex, Resources.ResourceManager);
            }
        }

        #region IModuleChildPage Members

        #endregion


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

                return new TaskItem[] {
                    new MethodTaskItem("RefreshPHPInfo", Resources.PHPInfoRefreshPHPInfo, "Set"),
                    new MethodTaskItem("SelectDomain", Resources.PHPInfoSelectDomain, "Set"),
                    new MethodTaskItem("GoBack", Resources.GoBackTask, "Tasks", null, Resources.GoBack16)
                };
            }

            public void GoBack()
            {
                _page.GoBack();
            }

            public void RefreshPHPInfo()
            {
                _page.ShowPHPInfo();
            }
            
            public void SelectDomain()
            {
                _page.SelectDomain();
            }

        }
    }
}