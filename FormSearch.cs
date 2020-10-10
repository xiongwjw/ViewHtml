using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ViewHtml
{
    public partial class FormSearch : Form
    {
        private List<SearchInfo> fileList;
        public FormSearch(List<SearchInfo> fileList)
        {
            InitializeComponent();
            this.fileList = fileList;
            InitListView();
            InitData();
        }

        private void InitListView()
        {
            lvMain.Items.Clear();
            lvMain.FullRowSelect = true;
            lvMain.View = View.Details;
            lvMain.GridLines = true;
        }

        private void InitData()
        {
            ShowID(this.fileList);
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                List<SearchInfo> list = this.fileList.Where(q => q.FileName.IndexOf(txtSearch.Text, StringComparison.OrdinalIgnoreCase) != -1).ToList();
                if (list != null)
                    ShowID(list);
            }
            else
                ShowID(this.fileList);
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (lvMain.SelectedItems.Count > 0)
                {
                    ListViewItem lv = lvMain.SelectedItems[0];
                    SelectNode = lv.Tag as TreeNode;
                    this.DialogResult = DialogResult.OK;
                    this.Hide();
                }
            }
            else if (e.KeyCode == Keys.Up || e.KeyCode==Keys.Down)
            {
                if (lvMain.SelectedItems.Count > 0)
                {
                    lvMain.Select();
                }
            }
        }

        private void lvMain_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (lvMain.SelectedItems.Count > 0)
                {
                    ListViewItem lv = lvMain.SelectedItems[0];
                    SelectNode = lv.Tag as TreeNode;
                    this.DialogResult = DialogResult.OK;
                    this.Hide();
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Hide();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ShowID(List<SearchInfo> fileList)
        {
            lvMain.Items.Clear();
            foreach (SearchInfo file in fileList)
            {
                ListViewItem lv = new ListViewItem();
                lv.Text = file.FileName;
                lv.SubItems.Add(file.FilePath);
                lv.Tag = file.Node;
                lvMain.Items.Add(lv);
            }
            if (lvMain.Items.Count > 0)
                lvMain.Items[0].Selected = true;
        }

        public TreeNode SelectNode = null;

        private void lvMain_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lvMain.SelectedItems.Count > 0)
            {
                ListViewItem lv = lvMain.SelectedItems[0];
                SelectNode = lv.Tag as TreeNode;
                this.DialogResult = DialogResult.OK;
                this.Hide();
            }
        }

        private void FormSearch_Shown(object sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
            txtSearch.Select();
        }



    }
}
