using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


namespace ViewHtml
{
    public partial class FormEditor : Form
    {
        public FormEditor(string fileName)
        {
            InitializeComponent();
            editor.CurrentLanguage = wjw.editor.Editor.Language.XML;
            OpenFile(fileName);
        }

        private void OpenFile(string fileName)
        {
            try
            {
                string str = File.ReadAllText(fileName);
                editor.Text = str;
                this.Text = $"【{Path.GetFileName(fileName)}】 {fileName}";
            }
            catch (System.Exception ex)
            {
                Loger.WriteFile(ex.Message);
            }
        }
    }
}
