using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ViewHtml
{
    public partial class FormSelectFolder : Form
    {
        public FormSelectFolder(string title = "")
        {
            InitializeComponent();
            if (!string.IsNullOrEmpty(title))
                lbTitle.Text = title;
        }
        public string SelectedPath = string.Empty;
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(txtPath.Text.Trim()))
                MessageBox.Show("Path not found!");
            else
            {
                this.SelectedPath = txtPath.Text.Trim();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fd = new FolderBrowserDialog();
            if (fd.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = fd.SelectedPath;
            }
        }

        private void FormSelectFolder_Load(object sender, EventArgs e)
        {
            this.Height = txtPath.Height + pnButton.Height + pnTitle.Height;
            txtPath.Select();
            txtPath.Focus();
        }
    }
}
