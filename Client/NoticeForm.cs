using System.Windows.Forms;

namespace Web.Management.PHP
{
    public partial class NoticeForm : Form
    {
        public NoticeForm()
        {
            InitializeComponent();
            label1.Text = Resources.WarningProcessBlocked;
            label1.MaximumSize = new System.Drawing.Size(panel1.Width, 0);
        }

        internal void SetLink(string url)
        {
            txtLink.Text = url;
            Clipboard.SetText(url);
        }
    }
}
