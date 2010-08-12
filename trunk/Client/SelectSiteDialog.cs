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
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.Web.Management.Client.Win32;

namespace Web.Management.PHP
{

    internal partial class SelectSiteDialog : TaskForm
    {

        private PHPModule _module;

        public SelectSiteDialog(PHPModule module)
            : base(module)
        {

            _module = module;

            InitializeComponent();

            Update();
        }

        protected override bool CanAccept
        {
            get
            {
                return _sitesComboBox.SelectedIndex >= 0;
            }
        }

        protected override bool CanShowHelp
        {
            get
            {
                return true;
            }
        }

        public string SiteName
        {
            get
            {
                return _sitesComboBox.SelectedItem as string;
            }
        }

        protected override void OnAccept()
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            StartAsyncTask(OnWorkerDoWork, OnWorkerDoWorkCompleted);
        }

        private void OnSitesComboBoxSelectedIndexChanged(object sender, System.EventArgs e)
        {
            Update();
        }

        private void OnWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = _module.Proxy.GetSites();
        }

        private void OnWorkerDoWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _sitesComboBox.BeginUpdate();
            _sitesComboBox.SuspendLayout();
            try
            {
                _sitesComboBox.Items.Clear();

                ArrayList sites = e.Result as ArrayList;
                foreach (string siteName in sites)
                {
                    _sitesComboBox.Items.Add(siteName);
                }
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ex, Resources.ResourceManager);
            }
            finally
            {
                _sitesComboBox.ResumeLayout();
                _sitesComboBox.EndUpdate();
            }

            _sitesComboBox.Focus();
        }

        //protected override void ShowHelp() {
        //    PHPModule.Browse(Globals.SelectSiteDialogOnlineHelp);
        //}
    }
}