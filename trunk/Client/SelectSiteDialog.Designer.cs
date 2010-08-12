namespace Web.Management.PHP
{
    partial class SelectSiteDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._contentPanel = new Microsoft.Web.Management.Client.Win32.ManagementPanel();
            this._siteLabel = new System.Windows.Forms.Label();
            this._titleLabel = new System.Windows.Forms.Label();
            this._sitesComboBox = new System.Windows.Forms.ComboBox();
            this._contentPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _contentPanel
            // 
            this._contentPanel.Controls.Add(this._siteLabel);
            this._contentPanel.Controls.Add(this._titleLabel);
            this._contentPanel.Controls.Add(this._sitesComboBox);
            this._contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._contentPanel.Location = new System.Drawing.Point(0, 0);
            this._contentPanel.Name = "_contentPanel";
            this._contentPanel.Size = new System.Drawing.Size(414, 132);
            this._contentPanel.TabIndex = 0;
            // 
            // _siteLabel
            // 
            this._siteLabel.AutoSize = true;
            this._siteLabel.Location = new System.Drawing.Point(12, 31);
            this._siteLabel.Name = "_siteLabel";
            this._siteLabel.Size = new System.Drawing.Size(28, 13);
            this._siteLabel.TabIndex = 1;
            this._siteLabel.Text = Resources.SelectSiteSiteLabel;
            // 
            // _titleLabel
            // 
            this._titleLabel.Location = new System.Drawing.Point(0, 0);
            this._titleLabel.Name = "_titleLabel";
            this._titleLabel.AutoSize = false;
            this._titleLabel.Size = new System.Drawing.Size(410, 33);
            this._titleLabel.TabIndex = 0;
            this._titleLabel.Text = Resources.SelectSiteSelectASiteFirst;
            // 
            // _sitesComboBox
            // 
            this._sitesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._sitesComboBox.FormattingEnabled = true;
            this._sitesComboBox.Location = new System.Drawing.Point(12, 51);
            this._sitesComboBox.Name = "_sitesComboBox";
            this._sitesComboBox.Size = new System.Drawing.Size(375, 21);
            this._sitesComboBox.TabIndex = 2;
            this._sitesComboBox.SelectedIndexChanged += new System.EventHandler(this.OnSitesComboBoxSelectedIndexChanged);
            // 
            // SelectSiteDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(414, 132);
            this.SetContent(this._contentPanel);
            this.Name = "SelectSiteDialog";
            this.ShowIcon = false;
            this.Text = Resources.SelectSiteTitle;
            this._contentPanel.ResumeLayout(false);
            this._contentPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private Microsoft.Web.Management.Client.Win32.ManagementPanel _contentPanel;
        private System.Windows.Forms.Label _titleLabel;
        private System.Windows.Forms.ComboBox _sitesComboBox;
        private System.Windows.Forms.Label _siteLabel;
    }
}