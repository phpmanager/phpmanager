//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Ruslan Yakushev for the PHP Manager for IIS project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 


namespace Web.Management.PHP {
    partial class PHPPageItemControl {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this._pictureBox = new System.Windows.Forms.PictureBox();
            this._titleLabel = new System.Windows.Forms.LinkLabel();
            this._tasksLabel = new System.Windows.Forms.LinkLabel();
            this._infoTlp = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this._pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // _pictureBox
            // 
            this._pictureBox.Location = new System.Drawing.Point(24, 10);
            this._pictureBox.Name = "_pictureBox";
            this._pictureBox.Size = new System.Drawing.Size(32, 32);
            this._pictureBox.TabIndex = 0;
            this._pictureBox.TabStop = false;
            // 
            // _titleLabel
            // 
            this._titleLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this._titleLabel.LinkColor = System.Drawing.SystemColors.ControlText;
            this._titleLabel.Location = new System.Drawing.Point(62, 4);
            this._titleLabel.Name = "_titleLabel";
            this._titleLabel.Size = new System.Drawing.Size(442, 32);
            this._titleLabel.TabIndex = 0;
            this._titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _tasksLabel
            // 
            this._tasksLabel.AutoSize = true;
            this._tasksLabel.Location = new System.Drawing.Point(65, 90);
            this._tasksLabel.Name = "_tasksLabel";
            this._tasksLabel.Size = new System.Drawing.Size(0, 13);
            this._tasksLabel.TabIndex = 2;
            this._tasksLabel.TabStop = true;
            this._tasksLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnTasksLabelLinkClicked);
            // 
            // _infoTlp
            // 
            this._infoTlp.AutoSize = true;
            this._infoTlp.BackColor = System.Drawing.Color.Transparent;
            this._infoTlp.ColumnCount = 2;
            this._infoTlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this._infoTlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 336F));
            this._infoTlp.Location = new System.Drawing.Point(65, 43);
            this._infoTlp.Name = "_infoTlp";
            this._infoTlp.RowCount = 1;
            this._infoTlp.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._infoTlp.Size = new System.Drawing.Size(439, 47);
            this._infoTlp.TabIndex = 3;
            // 
            // PHPPageItemControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._infoTlp);
            this.Controls.Add(this._tasksLabel);
            this.Controls.Add(this._titleLabel);
            this.Controls.Add(this._pictureBox);
            this.MinimumSize = new System.Drawing.Size(60, 80);
            this.Name = "PHPPageItemControl";
            this.Size = new System.Drawing.Size(507, 118);
            ((System.ComponentModel.ISupportInitialize)(this._pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox _pictureBox;
        private System.Windows.Forms.LinkLabel _titleLabel;
        private System.Windows.Forms.LinkLabel _tasksLabel;
        private System.Windows.Forms.TableLayoutPanel _infoTlp;
    }
}
