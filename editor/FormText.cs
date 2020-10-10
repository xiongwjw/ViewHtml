using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace wjw.Editor
{
    public partial class FormText : Form
    {
        public FormText(string text)
        {
            InitializeComponent();
            rtText.Text = text;
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
           

            return base.ProcessCmdKey(ref msg, keyData);
        }

    }
}
