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
using System.Drawing;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;

namespace Web.Management.PHP
{

    internal partial class PHPPageItemControl : UserControl
    {
        private const int WsExNoinheritlayout = 0x100000;
        private const int WsExLayoutrtl = 0x400000;
        private const string WarningLabelName = "warningLabel";
        private bool _rightToLeftLayout;
        private int _tlpRowCount;

        private Action<int> _handler;

        public PHPPageItemControl()
        {
            InitializeComponent();
        }

        protected override CreateParams CreateParams
        {
            [
                SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode),
                SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)
            ]
            get
            {
                CreateParams cp = base.CreateParams;
                if (_rightToLeftLayout)
                {
                    cp.ExStyle = cp.ExStyle | WsExLayoutrtl | WsExNoinheritlayout;
                }

                return cp;
            }
        }

        public Image Image
        {
            get
            {
                return _pictureBox.Image;
            }
            set
            {
                _pictureBox.Image = value;
            }
        }

        public bool RightToLeftLayout
        {
            get
            {
                return _rightToLeftLayout;
            }
            set
            {
                if (_rightToLeftLayout != value)
                {
                    _rightToLeftLayout = value;
                    if (IsHandleCreated)
                    {
                        base.OnRightToLeftChanged(EventArgs.Empty);
                    }
                }
            }
        }

        public string Title
        {
            get
            {
                return _titleLabel.Text;
            }
            set
            {
                _titleLabel.Text = value;
            }
        }

        public Font TitleFont
        {
            get
            {
                return _titleLabel.Font;
            }
            set
            {
                _titleLabel.Font = value;
            }
        }

        public event LinkLabelLinkClickedEventHandler TitleClick
        {
            add
            {
                _titleLabel.LinkClicked += value;
            }
            remove
            {
                _titleLabel.LinkClicked -= value;
            }
        }

        public void AddInfoRow(Label labelName, Label labelValue)
        {
            labelName.Dock = labelValue.Dock = DockStyle.Fill;
            labelName.TextAlign = labelValue.TextAlign = ContentAlignment.MiddleLeft;
            _infoTlp.Controls.Add(labelName, 0, _tlpRowCount);
            _infoTlp.Controls.Add(labelValue, 1, _tlpRowCount);
            _tlpRowCount++;
        }

        public void AddSpanRow(Label labelSpan)
        {
            labelSpan.Dock = DockStyle.Fill;
            labelSpan.TextAlign = ContentAlignment.MiddleLeft;
            _infoTlp.Controls.Add(labelSpan, 0, _tlpRowCount);
            _infoTlp.SetColumnSpan(labelSpan, 2);
            _tlpRowCount++;
        }

        public void AddTask(Action<int> handler, params string[] actionTitles)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            if (_handler != null)
            {
                throw new InvalidOperationException();
            }

            _handler = handler;
            var sb = new StringBuilder();
            var first = true;
            var links = new List<LinkLabel.Link>();
            foreach (var s in actionTitles)
            {
                if (!first)
                {
                    sb.Append(Resources.PHPPageItemTaskSeparator);
                }
                first = false;
                links.Add(new LinkLabel.Link(sb.Length, s.Length, links.Count));

                sb.Append(s);
            }

            _tasksLabel.Text = sb.ToString();
            foreach (var l in links)
            {
                _tasksLabel.Links.Add(l);
            }
        }

        public void ClearWarning()
        {
            _warningPanel.Controls.RemoveByKey(WarningLabelName);
            _warningPanel.Visible = false;
        }

        private Size DoLayout(Size proposedSize, bool performLayout)
        {
            if (performLayout)
            {
                _titleLabel.Width = ClientRectangle.Width;
            }

            int nextTop = _titleLabel.Top + _titleLabel.Height + 8;

            if (_warningPanel.Visible)
            {
                Size warningSize = _warningPanel.GetPreferredSize(new Size(proposedSize.Width - _warningPanel.Left, Int32.MaxValue));
                if (performLayout)
                {
                    _warningPanel.Top = nextTop;
                    _warningPanel.Size = warningSize;
                }
                nextTop += _warningPanel.Height + 8;
            }

            Size infoSize = _infoTlp.GetPreferredSize(new Size(proposedSize.Width - _infoTlp.Left, Int32.MaxValue));
            if (performLayout)
            {
                _infoTlp.Top = nextTop;
                _infoTlp.Size = infoSize;
            }

            nextTop += _infoTlp.Height + 10;

            Size tasksSize = _tasksLabel.GetPreferredSize(new Size(proposedSize.Width - _tasksLabel.Left, Int32.MaxValue));
            if (performLayout)
            {
                _tasksLabel.Top = nextTop;
                _tasksLabel.Size = tasksSize;
            }

            int height = tasksSize.Height + nextTop + 12;

            return new Size(proposedSize.Width, height);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            Size size = DoLayout(proposedSize, false);
            return size;
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            DoLayout(Size, true);
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);

            RightToLeftLayout = RightToLeft == RightToLeft.Yes;
        }

        private void OnTasksLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _handler((int)e.Link.LinkData);
        }

        public void SetTaskState(int index, bool enabled)
        {
            LinkLabel.Link l = _tasksLabel.Links[index];
            if (l != null)
            {
                l.Enabled = enabled;
            }
        }

        public void SetTitleState(bool enabled)
        {
            _titleLabel.Enabled = enabled;
        }

        public void SetWarning(Label warningLabel)
        {
            warningLabel.AutoSize = true;
            warningLabel.Location = new Point(25, 6);
            warningLabel.Name = WarningLabelName;
            // Remove existing label from the warning panel in case it exists
            _warningPanel.Controls.RemoveByKey(WarningLabelName);
            // Add new label to the warning panel
            _warningPanel.Controls.Add(warningLabel);
            _warningPanel.Visible = true;
        }

    }
}
