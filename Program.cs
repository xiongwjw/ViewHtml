using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ViewHtml
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] commands)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            RunApplication(commands);
            //Application.Run(new FormViewHtml());
        }

        private static void RunApplication(string[] commands)
        {
            if (commands.Length > 0)
            {
                string fileName = string.Empty;
                foreach (string commond in commands)
                {
                    fileName += commond;
                    fileName += " ";
                }
                fileName.Trim();
                Application.Run(new FormViewHtml(fileName));
            }
            else
            {
                Application.Run(new FormViewHtml());
            }
        }

    }
}
