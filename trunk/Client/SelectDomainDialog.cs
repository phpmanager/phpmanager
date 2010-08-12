//------------------------------------------------------------------------------
// <copyright file="SelectDomainDialog.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.Web.Management.Client;
using Microsoft.Web.Management.Client.Win32;

namespace Web.Management.PHP
{

    internal partial class SelectDomainDialog : TaskForm
    {

        private PHPModule _module;
        private Connection _connection;
        private string _selectedDomain;

        public SelectDomainDialog(PHPModule module, Connection connection, string selectedDomain)
            : base(module)
        {
            _module = module;
            _connection = connection;
            _selectedDomain = selectedDomain;

            InitializeComponent();

            Update();
        }

        protected override bool CanAccept
        {
            get
            {
                return _domainsComboBox.SelectedIndex >= 0;
            }
        }

        protected override bool CanShowHelp
        {
            get
            {
                return true;
            }
        }

        public string SiteDomain
        {
            get
            {
                return _domainsComboBox.SelectedItem as string;
            }
        }

        protected override void OnAccept()
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void OnDomainsComboBoxSelectedIndexChanged(object sender, System.EventArgs e)
        {
            Update();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            StartAsyncTask(OnWorkerDoWork, OnWorkerDoWorkCompleted);
        }

        private void OnWorkerDoWork(object sender, DoWorkEventArgs e) 
        {
            e.Result = Helper.GetUrlListFromBindings(_connection.ScopePath.ServerName, _module.Proxy.GetSiteBindings());
        }

        private void OnWorkerDoWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _domainsComboBox.BeginUpdate();
            _domainsComboBox.SuspendLayout();
            try
            {
                _domainsComboBox.Items.Clear();

                List<string> domains = e.Result as List<string>;
                foreach (string domain in domains)
                {
                    _domainsComboBox.Items.Add(domain);
                }

                int selectedIndex = _domainsComboBox.Items.IndexOf(_selectedDomain);
                _domainsComboBox.SelectedIndex = (selectedIndex >= 0)? selectedIndex : 0;
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ex, Resources.ResourceManager);
            }
            finally
            {
                _domainsComboBox.ResumeLayout();
                _domainsComboBox.EndUpdate();
            }

            _domainsComboBox.Focus();
        }

        //protected override void ShowHelp() {
        //    PHPModule.Browse(Globals.SelectSiteDialogOnlineHelp);
        //}
    }
}