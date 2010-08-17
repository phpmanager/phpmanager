//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System;
using System.Windows.Forms;
using Microsoft.Web.Management.Client;
using Microsoft.Web.Management.Client.Win32;
using Microsoft.Web.Management.Server;

namespace Web.Management.PHP.Settings
{

    [ModulePageIdentifier(Globals.ErrorReportingPageIdentifier)]
    internal sealed class ErrorReportingPage : ModuleDialogPage
    {
        private Label _devMachineLabel;
        private Label _selectServerTypeLabel;
        private RadioButton _devMachineRadioButton;
        private RadioButton _prodMachineRadioButton;
        private Label _prodMachineLabel;
        private Label _errorLogFileLabel;
        private TextBox _errorLogFileTextBox;
        private Button _errorLogBrowseButton;
        private GroupBox _serverTypeGroupBox;
    
        protected override bool ApplyChanges()
        {
            throw new NotImplementedException();
        }

        protected override bool CanApplyChanges
        {
            get { throw new NotImplementedException(); }
        }

        protected override void CancelChanges()
        {
            throw new NotImplementedException();
        }

        protected override void Initialize(object navigationData)
        {
            base.Initialize(navigationData);

            InitializeComponent();
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
            this._serverTypeGroupBox.Size = new System.Drawing.Size(593, 222);
            this._serverTypeGroupBox.TabIndex = 0;
            this._serverTypeGroupBox.TabStop = false;
            this._serverTypeGroupBox.Text = Resources.ErrorReportingPageServerType;
            // 
            // _prodMachineLabel
            // 
            this._prodMachineLabel.Location = new System.Drawing.Point(37, 150);
            this._prodMachineLabel.Name = "_prodMachineLabel";
            this._prodMachineLabel.Size = new System.Drawing.Size(521, 48);
            this._prodMachineLabel.TabIndex = 5;
            this._prodMachineLabel.Text = Resources.ErrorReportingPageProdMachineDesc;
            // 
            // _prodMachineRadioButton
            // 
            this._prodMachineRadioButton.AutoSize = true;
            this._prodMachineRadioButton.Location = new System.Drawing.Point(20, 130);
            this._prodMachineRadioButton.Name = "_prodMachineRadioButton";
            this._prodMachineRadioButton.Size = new System.Drawing.Size(119, 17);
            this._prodMachineRadioButton.TabIndex = 4;
            this._prodMachineRadioButton.TabStop = true;
            this._prodMachineRadioButton.Text = Resources.ErrorReportingPageProdMachine;
            this._prodMachineRadioButton.UseVisualStyleBackColor = true;
            // 
            // _devMachineLabel
            // 
            this._devMachineLabel.Location = new System.Drawing.Point(37, 75);
            this._devMachineLabel.Name = "_devMachineLabel";
            this._devMachineLabel.Size = new System.Drawing.Size(521, 46);
            this._devMachineLabel.TabIndex = 3;
            this._devMachineLabel.Text = Resources.ErrorReportingPageDevMachineDesc;
            // 
            // _selectServerTypeLabel
            // 
            this._selectServerTypeLabel.Location = new System.Drawing.Point(6, 20);
            this._selectServerTypeLabel.Name = "_selectServerTypeLabel";
            this._selectServerTypeLabel.Size = new System.Drawing.Size(458, 23);
            this._selectServerTypeLabel.TabIndex = 1;
            this._selectServerTypeLabel.Text = Resources.ErrorReportingPageSelectServerType;
            // 
            // _devMachineRadioButton
            // 
            this._devMachineRadioButton.AutoSize = true;
            this._devMachineRadioButton.Location = new System.Drawing.Point(20, 55);
            this._devMachineRadioButton.Name = "_devMachineRadioButton";
            this._devMachineRadioButton.Size = new System.Drawing.Size(131, 17);
            this._devMachineRadioButton.TabIndex = 0;
            this._devMachineRadioButton.TabStop = true;
            this._devMachineRadioButton.Text = Resources.ErrorReportingPageDevMachine;
            this._devMachineRadioButton.UseVisualStyleBackColor = true;
            // 
            // _errorLogFileLabel
            // 
            this._errorLogFileLabel.AutoSize = true;
            this._errorLogFileLabel.Location = new System.Drawing.Point(3, 253);
            this._errorLogFileLabel.Name = "_errorLogFileLabel";
            this._errorLogFileLabel.Size = new System.Drawing.Size(65, 13);
            this._errorLogFileLabel.TabIndex = 1;
            this._errorLogFileLabel.Text = Resources.ErrorReportingPageErrorLogFile;
            // 
            // _errorLogFileTextBox
            // 
            this._errorLogFileTextBox.Location = new System.Drawing.Point(7, 269);
            this._errorLogFileTextBox.Name = "_errorLogFileTextBox";
            this._errorLogFileTextBox.Size = new System.Drawing.Size(501, 20);
            this._errorLogFileTextBox.TabIndex = 2;
            // 
            // _errorLogBrowseButton
            // 
            this._errorLogBrowseButton.Location = new System.Drawing.Point(514, 266);
            this._errorLogBrowseButton.Name = "_errorLogBrowseButton";
            this._errorLogBrowseButton.Size = new System.Drawing.Size(25, 23);
            this._errorLogBrowseButton.TabIndex = 3;
            this._errorLogBrowseButton.Text = "...";
            this._errorLogBrowseButton.UseVisualStyleBackColor = true;
            // 
            // ErrorReportingPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this._errorLogBrowseButton);
            this.Controls.Add(this._errorLogFileTextBox);
            this.Controls.Add(this._errorLogFileLabel);
            this.Controls.Add(this._serverTypeGroupBox);
            this.Name = "ErrorReportingPage";
            this.Size = new System.Drawing.Size(600, 338);
            this._serverTypeGroupBox.ResumeLayout(false);
            this._serverTypeGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void InitializeUI()
        {
 
        }
    }
}
