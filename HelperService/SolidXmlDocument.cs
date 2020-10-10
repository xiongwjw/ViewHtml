using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace HelperService
{
    public class SolidXmlDocument : XmlDocument
    {
#region constructor
        public SolidXmlDocument()
        {

        }
#endregion

#region method
        public override void Load(string filename)
        {
            lock ( m_locker )
            {
                try
                {
                    //base.Load(filename);
                    using ( FileStream Stream = new FileStream(filename, FileMode.Open, FileAccess.Read) )
                    {
                        base.Load(Stream);

                        Stream.Flush();
                        Stream.Close();
                    }

                    m_originFile = filename;

                    string folder = string.Format("{0}{1}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), s_appFolder);
                    if ( !Directory.Exists(folder) )
                    {
                        Directory.CreateDirectory(folder);
                    }
                    string xmlFileName = Path.GetFileName(filename);
                    string newFilePath = string.Format("{0}{1}", folder, xmlFileName);
                    File.Copy(filename, newFilePath, true);
                    Thread.Sleep(1000);
                }
                catch (System.Exception ex)
                {
                    Trace.TraceError(ex.Message);
                    Trace.TraceInformation("Prepare for rollback the xml file[{0}]", filename);
                    string backupFile = string.Format("{0}bak", filename);
                    if (File.Exists(backupFile))
                    {
                        Rollback(filename, backupFile); 
                        try
                        {
                            //base.Load(filename);
                            using ( FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read) )
                            {
                                base.Load(stream);
                                stream.Flush();
                                stream.Close();
                            }
                            return;
                        }
                        catch (System.Exception exp)
                        {
                            Trace.TraceError(exp.Message);
                        }
                    }

                    Resume(filename);
                }    
            }

        }

        private void Resume( string argFileName )
        {
            Debug.Assert(!string.IsNullOrEmpty(argFileName));

            string folder = string.Format("{0}{1}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), s_appFolder);
            string xmlFileName = Path.GetFileName(argFileName);
            string newFilePath = string.Format("{0}{1}", folder, xmlFileName);
            if (!File.Exists(newFilePath))
            {
                throw new Exception(string.Format("backup file{0} isn't exist", newFilePath));
            }

            if ( File.Exists(argFileName) )
            {
                FileInfo fInfo = new FileInfo(argFileName);
                fInfo.IsReadOnly = false;
            }
            File.Copy(newFilePath, argFileName, true);
            Thread.Sleep(1000);
            //base.Load(argFileName);
            using (FileStream stream = new FileStream(argFileName, FileMode.Open, FileAccess.Read))
            {
                base.Load(stream);
                stream.Flush();
                stream.Close();
            }
        }

        public override void Save(string filename)
        {
            lock ( m_locker )
            {
                if (string.IsNullOrEmpty(filename))
                {
                    throw new ArgumentNullException("filename");
                }
                m_originFile = filename;

                string backupFile = BackupFile;

                if (!BeginSave(filename, backupFile))
                {
                    throw new Exception(string.Format("Failed to save a xml file[{0}]", filename));
                }

                if (!SaveXml(filename))
                {
                    Rollback(filename, backupFile);
                    throw new Exception(string.Format("Failed to sava a xml file[{0}]", filename));
                }

                EndSave(backupFile);
            }
        }

        private bool BeginSave(string argXml,
                        string argBackup)
        {
            Debug.Assert(!string.IsNullOrEmpty(argXml) && !string.IsNullOrEmpty(argBackup) && File.Exists(argXml));

            try
            {
                if (File.Exists(argBackup))
                {
                    FileInfo backupInfo = new FileInfo(argBackup);
                    if (backupInfo.IsReadOnly)
                    {
                        backupInfo.IsReadOnly = false;
                    }
                    backupInfo = null;
                }

                FileInfo xmlInfo = new FileInfo(argXml);
                if (xmlInfo.IsReadOnly)
                {
                    xmlInfo.IsReadOnly = false;
                }
                xmlInfo = null;

                File.Copy(argXml, argBackup, true);
                Thread.Sleep(1000);
            }
            catch (System.Exception ex)
            {
                Trace.TraceError(ex.Message);
                return false;
            }

            return true;
        }

        private bool SaveXml(string argFile)
        {
            Debug.Assert(!string.IsNullOrEmpty(argFile));

            try
            {
                using ( FileStream stream = new FileStream( argFile, FileMode.Create, FileAccess.Write ) )
                {
                    base.Save(stream);
                    stream.Flush();
                    stream.Close();
                }
                Thread.Sleep(1000);
                //check xml file
                //XmlDocument doc = new XmlDocument();
                //using ( FileStream stream = new FileStream( argFile, FileMode.Open, FileAccess.Read ) )
                //{
                //    doc.Load(stream);
                //    stream.Flush();
                //    stream.Close();
                //}
                
                //doc.RemoveAll();
                //doc = null;
            }
            catch (System.Exception ex)
            {
                Trace.TraceError(ex.Message);
                return false;
            }

            return true;
        }

        private void EndSave(string argBackupFile)
        {
            Debug.Assert(!string.IsNullOrEmpty(argBackupFile));

            try
            {
                FileInfo info = new FileInfo(argBackupFile);
                if (info.IsReadOnly)
                {
                    info.IsReadOnly = false;
                }
                info.Delete();
                info = null;
                Thread.Sleep(1000);
            }
            catch (System.Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }

        private void Rollback(string argXmlFile,
                               string argBackupFile)
        {
            Debug.Assert(!string.IsNullOrEmpty(argXmlFile) && !string.IsNullOrEmpty(argBackupFile));

            try
            {
                if (!File.Exists(argBackupFile))
                {
                    Trace.TraceError("{0} isn't exist", argBackupFile);
                    return;
                }

                File.Copy(argBackupFile, argXmlFile, true);
                Thread.Sleep(1000);
                File.Delete(argBackupFile);
                Thread.Sleep(1000);
            }
            catch (System.Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }

        private string BackupFile
        {
            get
            {
                Debug.Assert(!string.IsNullOrEmpty(m_originFile));
                return string.Format(@"{0}bak", m_originFile);
            }
        }
#endregion

#region field
        protected string m_originFile = null;

        private object m_locker = new object();

        private const string s_appFolder = @"\Grgbanking\eCAT3\Grgbackup4Config\";
#endregion
    }
}
