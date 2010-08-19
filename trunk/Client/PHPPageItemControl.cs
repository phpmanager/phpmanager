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
        private const int WS_EX_NOINHERITLAYOUT = 0x100000;
        private const int WS_EX_LAYOUTRTL = 0x400000;
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
                CreateParams CP;
                CP = base.CreateParams;
                if (_rightToLeftLayout)
                {
                    CP.ExStyle = CP.ExStyle | WS_EX_LAYOUTRTL | WS_EX_NOINHERITLAYOUT;
                }

                return CP;
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
            StringBuilder sb = new StringBuilder();
            bool first = true;
            List<LinkLabel.Link> links = new List<LinkLabel.Link>();
            foreach (string s in actionTitles)
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
            foreach (LinkLabel.Link l in links)
            {
                _tasksLabel.Links.Add(l);
            }
        }

        private Size DoLayout(Size proposedSize, bool performLayout)
        {
            Size descriptionSize = _infoTlp.GetPreferredSize(new Size(proposedSize.Width - _infoTlp.Left, Int32.MaxValue));
            if (performLayout)
            {
                _titleLabel.Width = ClientRectangle.Width;
                _infoTlp.Size = descriptionSize;
            }

            int tasksTop = _infoTlp.Top + descriptionSize.Height + 10;

            Size tasksSize = _tasksLabel.GetPreferredSize(new Size(proposedSize.Width - _tasksLabel.Left, Int32.MaxValue));
            if (performLayout)
            {
                _tasksLabel.Top = tasksTop;
                _tasksLabel.Size = tasksSize;
            }

            int height = tasksSize.Height + tasksTop + 12;

            return new Size(proposedSize.Width, height);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            Size size = DoLayout(proposedSize, false);
            return size;
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            DoLayout(this.Size, true);
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

    }
}
