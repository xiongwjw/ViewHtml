
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
namespace ViewHtml
{
    public class Loger
    {
        public static void WriteFile(string message)
        {
            string folder_name = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
            string file_name = Path.Combine(folder_name, DateTime.Today.ToString("yyyyMMdd") + ".txt");
            if (!Directory.Exists(folder_name))
            {
                Directory.CreateDirectory(folder_name);
            }
            StreamWriter sw = null;
            FileStream fs = null;
            try
            {
                fs = new FileStream(file_name, FileMode.Append, FileAccess.Write, FileShare.Write);
                using (sw = new StreamWriter(fs))
                {
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " : " + message);
                    sw.Flush();
                    sw.Close();
                }
                fs.Close();
            }
            catch { }
            finally
            {
                if (sw != null) sw.Close();
                if (fs != null) fs.Close();
            }
        }
    }
}
