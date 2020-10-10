using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace wjw.Editor
{
    public class PopUp
    {
        public static void Warning(string message)
        {
            MessageBox.Show(message, "Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        public static void Error(string message)
        {
            MessageBox.Show(message, "Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public static void Information(string message)
        {
            MessageBox.Show(message, "Editor", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
