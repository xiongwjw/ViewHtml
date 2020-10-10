using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Collections;


namespace LogProcessorService
{
    class FileCreateTimeComparer : IComparer<string>
    {
        public int Compare( string argX,
                            string argY)
        {
            FileInfo xInfo = new FileInfo(argX);
            FileInfo yInfo = new FileInfo(argY);

            int result = xInfo.CreationTime.CompareTo( yInfo.CreationTime );

            xInfo = null;
            yInfo = null;

            return result;
        }
    }

    public class GrgLogManager
    {
#region constructor
        protected GrgLogManager()
        {

        }

        static GrgLogManager()
        {
            s_logManager = new GrgLogManager();
        }
#endregion

#region method
        public bool Initialize( string argCfgPath,
                                string argLogPath )
        {
            Debug.Assert(!string.IsNullOrEmpty(argCfgPath) && !string.IsNullOrEmpty(argLogPath));
            //load config
            LoadConfig(argCfgPath);

            m_logPath = argLogPath;
            m_driverName = Path.GetPathRoot(argLogPath);
            if ( string.IsNullOrEmpty(m_driverName) )
            {
                return false;
            }
            m_driverName = m_driverName.Substring(0, 1);
            //first check date 
            DeleteLogFileByDateRule();
            //second check size
            DeleteLogFileBySize();
            //Compress file
            CompressLogFiles();
            //rename file
            RenameLogFileExtra();

            //check if log files are exists.
            Log.Root.LogDebug("Check if log files are exists");
            Thread.Sleep(1000);
            if ( !Directory.Exists( argLogPath ) )
            {
                Log.Root.LogErrorFormat("Log path[{0}] isn't exist", argLogPath);
                return false;
            }
            bool exist = false;
            string now = string.Format("eCAT{0:D4}{1:D2}{2:D2}", DateTime.Now.Year,
                                        DateTime.Now.Month,
                                        DateTime.Now.Day);
            foreach ( var item in Directory.EnumerateFiles( argLogPath, 
                                                            "eCAT*.txt", 
                                                            SearchOption.TopDirectoryOnly ) )
            {
                if ( item.Contains(now) )
                {
                    exist = true;
                    break;
                }
            }

            if ( !exist )
            {
                Log.Root.LogError("The log file isn't exist");
                return false;
            }

            m_timer.Restart();
            m_monitorThread = new Thread( new ThreadStart( OnMonitorThread ) );
            m_monitorThread.Name = "LogManagerThread";
            m_monitorThread.Start();

            return true;
        }

        public void Exit()
        {
            if ( null != m_monitorThread )
            {
                m_arrEvts[0].Set();
                
                if ( m_monitorThread.Join( 3000 ) )
                {
                    m_monitorThread.Abort();
                    m_monitorThread.Join(3000);
                    m_monitorThread = null;
                }
            }

            foreach ( var evt in m_arrEvts )
            {
                evt.Dispose();
            }
            m_arrEvts[0] = null;
            m_arrEvts[1] = null;
        }

        public void LoadConfig( string argCfgPath )
        {
            Debug.Assert(!string.IsNullOrEmpty(argCfgPath));

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(argCfgPath);

                //backup day
                XmlAttribute attri = doc.DocumentElement.Attributes[s_backupDayAttri];
                if ( null != attri &&
                     !string.IsNullOrEmpty(attri.Value) )
                {
                    int backup = 0;
                    if ( int.TryParse(attri.Value, out backup) )
                    {
                        if ( backup > 0 )
                        {
                            m_backupDay = backup;
                        }
                    }
                }
                //reserved size
                attri = doc.DocumentElement.Attributes[s_reservedSizeAttri];
                if ( null != attri &&
                     !string.IsNullOrEmpty(attri.Value) )
                {
                    int size = 0;
                    if ( int.TryParse(attri.Value, out size) )
                    {
                        if ( size > 0 )
                        {
                            m_reservedSize = (size + 50) * s_OneMBSize;
                        }
                    }
                }

                XmlNode compressNode = doc.DocumentElement.SelectSingleNode(s_compressStrategyNode);
                if ( null != compressNode )
                {
                    XmlNodeList listNode = compressNode.SelectNodes(s_itemNode);
                    string folder = null;
                    List<string> listPattern = null;
                    foreach ( XmlNode itemNode in listNode )
                    {
                        if ( itemNode.NodeType != XmlNodeType.Element )
                        {
                            continue;
                        }

                        attri = itemNode.Attributes[s_compressFolderAttri];
                        if (null != attri &&
                             !string.IsNullOrEmpty(attri.Value))
                        {
                            folder = AppDomain.CurrentDomain.BaseDirectory + attri.Value;
                        }
                        else
                        {
                            continue;
                        }
                        if ( !Directory.Exists( folder ) )
                        {
                            continue;
                        }

                        attri = itemNode.Attributes[s_compressFilePatternAttri];
                        if (null != attri &&
                             !string.IsNullOrEmpty(attri.Value))
                        {
                            listPattern = new List<string>();
                            string[] arrPattern = attri.Value.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var item in arrPattern)
                            {
                                if ( !listPattern.Contains( item ) )
                                {
                                    listPattern.Add(item);
                                }
                            }
                        }
                        else
                        {
                            continue;
                        }
                        if ( listPattern.Count == 0 )
                        {
                            continue;
                        }

                        m_listCompressStrategy.Add(new CompressStrategyParam()
                            {
                                Folder = folder,
                                FilePatternCollection = listPattern
                            });
                    }
                }

                doc.RemoveAll();
                doc = null;
            }
            catch (System.Exception ex)
            {
                Log.Root.LogError("Failed to load config of log manager", ex);
                return;
            }
        }

        private void DeleteLogFileByDateRule()
        {
            //Search all files
            FileInfo info = null;
            DateTime nowTime = DateTime.Now;
            TimeSpan backupTimespan = new TimeSpan( m_backupDay, 0, 0, 0 );
            try
            {
                m_listNeedDelFiles.Clear();
                foreach (var filePath in Directory.EnumerateFiles(m_logPath,
                                                                  "*.*",
                                                                  SearchOption.AllDirectories ))
                {
                    info = new FileInfo(filePath);
                    if (info.CreationTime.Add(backupTimespan).CompareTo(nowTime) < 0)
                    {
                        m_listNeedDelFiles.Add(filePath);
                    }
                    info = null;
                }
                if ( m_listNeedDelFiles.Count == 0 )
                {
                    return;
                }
                Log.Kernel.LogDebug("Prepare for delete out-of-date files");
                //delete files
                FileInfo fInfo = null;
                foreach ( var filePath in m_listNeedDelFiles )
                {
                    try
                    {
                        fInfo = new FileInfo(filePath);
                        if ( fInfo.IsReadOnly )
                        {
                            fInfo.IsReadOnly = false;
                        }
                        fInfo = null;
                        File.Delete(filePath);
                    }
                    catch (System.Exception ex)
                    {
                        Log.Kernel.LogWarn(string.Format("Failed to delete a file", filePath), ex);
                    }
                }
                m_listNeedDelFiles.Clear();
            }
            catch (System.Exception ex)
            {
                Log.Root.LogDebug("Failed to delete log file", ex);	
            }
        }

        private void DeleteLogFileBySize()
        {
            //Search all files
            DateTime nowTime = DateTime.Now;
            try
            {
                string[] arrFiles = Directory.GetFiles(m_logPath, "*.*", SearchOption.AllDirectories);
                Array.Sort( arrFiles, new FileCreateTimeComparer() );
                DriveInfo dInfo = new DriveInfo(m_driverName);
                if (dInfo.TotalFreeSpace > m_reservedSize)
                {
                    dInfo = null;
                    arrFiles = null;
                    return;
                }
                Log.Kernel.LogDebug("Prepare for delete some files to get more space");
                //delete files
                FileInfo fInfo = null;
                DateTime now = DateTime.Now;
                DateTime today = new DateTime( now.Year, now.Month, now.Day, 0, 0, 0 );
                foreach (var filePath in arrFiles)
                {
                    try
                    {
                        fInfo = new FileInfo(filePath);
                        if ( fInfo.CreationTime < today )
                        {
                            if ( fInfo.IsReadOnly )
                            {
                                fInfo.IsReadOnly = false;
                            }
                            File.Delete(filePath);
                            if (dInfo.TotalFreeSpace > m_reservedSize)
                            {
                                break;
                            }
                        }
                        fInfo = null;
                    }
                    catch (System.Exception ex)
                    {
                        //Log.Kernel.LogDebug(string.Format("Failed to delete a log file",filePath), ex);
                    }
                }

                dInfo = null;
                arrFiles = null;
            }
            catch (System.Exception ex)
            {
                Log.Root.LogDebug("Failed to delete log file", ex);
            }
        }

        private void CompressLogFiles()
        {
            DateTime now = DateTime.Now;
            try
            {
                if (m_listCompressStrategy.Count == 0)
                {
                    Log.Kernel.LogWarn("The compress file pattern is empty");
                    return;
                }

                string compressProg = AppDomain.CurrentDomain.BaseDirectory + "7z.exe";
                if ( !File.Exists(compressProg) )
                {
                    Log.Kernel.LogWarn("The 7z.exe isn't exist");
                    return;
                }
                //string logFilePath = AppDomain.CurrentDomain.BaseDirectory + m_compressFolder;
                //if ( !Directory.Exists( logFilePath ) )
                //{
                //    Log.Kernel.LogWarnFormat(@"The {0} isn't exist", m_compressFolder);
                //    return;
                //}

                //m_listNeedDelFiles.Clear();
                if ( m_dicNeedDelFiles.Count > 0 )
                {
                    foreach (var item in m_dicNeedDelFiles)
                    {
                        item.Value.Clear();
                    }
                    m_dicNeedDelFiles.Clear();
                }

                DateTime level = new DateTime(now.Year, now.Month, now.Day, 23, 59, 0);
                DateTime baseLevel = level.Subtract(new TimeSpan(5, 0, 0, 0, 0));
                DateTime createTime;
                List<string> listDelFiles = null;
                foreach ( var strategyItem in m_listCompressStrategy )
                {
                    DirectoryInfo logDInfo = new DirectoryInfo(strategyItem.Folder);
                    //List<string> listLogFile = new List<string>();
                    foreach (var item in strategyItem.FilePatternCollection)
                    {
                        foreach (var fitem in logDInfo.EnumerateFiles(item, SearchOption.TopDirectoryOnly))
                        {
                            try
                            {
                                if (fitem.CreationTime < baseLevel)
                                {
                                    //listLogFile.Add(item.FullName);
                                    //m_listNeedDelFiles.Add(item.FullName);
                                    createTime = new DateTime(fitem.CreationTime.Year, fitem.CreationTime.Month, fitem.CreationTime.Day);
                                    if (m_dicNeedDelFiles.ContainsKey(createTime))
                                    {
                                        listDelFiles = m_dicNeedDelFiles[createTime];
                                        if (!listDelFiles.Contains( fitem.FullName ))
                                        {
                                            listDelFiles.Add(fitem.FullName);
                                        }
                                    }
                                    else
                                    {
                                        List<string> listLogFile = new List<string>();
                                        listLogFile.Add(fitem.FullName);
                                        m_dicNeedDelFiles.Add(createTime, listLogFile);
                                    }
                                    if (fitem.IsReadOnly)
                                    {
                                        fitem.IsReadOnly = false;
                                    }
                                }
                            }
                            catch (System.Exception ex)
                            {
                                Log.Kernel.LogWarn("Failed to get file information", ex);
                            }
                        }
                    }
                    logDInfo = null;                   
                }
                if (m_dicNeedDelFiles.Count == 0)
                {
                    return;
                } 

                Log.Kernel.LogDebug("Prepare to compress log files");
                string compressFile = AppDomain.CurrentDomain.BaseDirectory + s_logCompressFile;
                List<string> delListFile = null;
                string compressedFile = null;
                foreach (var item in m_dicNeedDelFiles)
                {
                    delListFile = item.Value;
                    Debug.Assert(delListFile.Count > 0);
                    using (FileStream fs = new FileStream(compressFile, FileMode.Create, FileAccess.Write))
                    {
                        using (StreamWriter writer = new StreamWriter(fs, Encoding.UTF8))
                        {
                            foreach (var delitem in delListFile)
                            {
                                writer.WriteLine(delitem);
                            }
                        }
                    }

                    string outputFile = string.Format(@"log\eCAT{0:D4}{1:D2}{2:D2}.zip", item.Key.Year, item.Key.Month, item.Key.Day);
                    string cmdline = string.Format(s_7zCompressFormat,outputFile,s_logCompressFile);
                    bool result = false;
                    try
                    {
                        using (Process proc = new Process())
                        {
                            proc.StartInfo.FileName = compressProg;
                            proc.StartInfo.Arguments = cmdline;
                            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            proc.StartInfo.UseShellExecute = true;
                            proc.StartInfo.CreateNoWindow = true;
                            proc.StartInfo.ErrorDialog = false;
                            proc.Start();
                            proc.WaitForExit();
                            result = true;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Log.Kernel.LogError("Failed to compress log files", ex);
                    }
                    File.Delete(compressFile);
                    compressedFile = AppDomain.CurrentDomain.BaseDirectory + outputFile;
                    if (result &&
                         File.Exists(compressedFile))
                    {
                        FileInfo fInfo = new FileInfo(compressedFile);
                        fInfo.CreationTime = item.Key;
                        fInfo.LastAccessTime = item.Key;
                        fInfo.LastWriteTime = item.Key;
                        fInfo = null;
                    }

                    if (result)
                    {
                        foreach (var delItem in delListFile)
                        {
                            try
                            {
                                File.Delete(delItem);
                            }
                            catch (System.Exception ex)
                            {
                                //Log.Kernel.LogWarn(string.Format("Failed to delete a file[{0}]", item), ex);
                            }
                        }
                    }
                    //m_listNeedDelFiles.Clear();
                }

                foreach (var item in m_dicNeedDelFiles)
                {
                    item.Value.Clear();
                }
                m_dicNeedDelFiles.Clear();
            }
            catch (System.Exception ex)
            {
                Log.Kernel.LogError("Failed to compress log files", ex);
            }
        }

        private void RenameLogFileExtra()
        {
            //Search all files
            FileInfo fInfo = null;
            DateTime nowTime = DateTime.Now;
            try
            {
                m_listNeedDelFiles.Clear();
                foreach (var itemFile in Directory.EnumerateFiles(m_logPath,
                                                                  "*.*.txt",
                                                                  SearchOption.AllDirectories))
                {
                    m_listNeedDelFiles.Add(itemFile);
                }

                if (m_listNeedDelFiles.Count == 0)
                {
                    return;
                }

                Log.Kernel.LogDebug("Prepare for rename files");

                m_listNeedDelFiles.Sort();
                m_listNeedDelFiles.Reverse();

                int i = 1;
                foreach(var filePath in m_listNeedDelFiles)
                {
                    fInfo = new FileInfo(filePath);

                    if (fInfo.CreationTime.ToString("yyyyMMdd") == nowTime.ToString("yyyyMMdd"))
                    {
                        string noExtensionName = fInfo.Name.Replace(fInfo.Extension, "");
                        string[] arrItem = noExtensionName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                        if (arrItem.Length == 2 && IsAllNumber(arrItem[1]))
                        {
                            try
                            {
                                for (; i < 1000; i++)
                                {
                                    string destFileName = fInfo.DirectoryName + "\\" + arrItem[0] + "_" + i.ToString() + fInfo.Extension;
                                    if (!File.Exists(destFileName))
                                    {
                                        if (fInfo.IsReadOnly)
                                        {
                                            fInfo.IsReadOnly = false;
                                        }
                                        fInfo.MoveTo(destFileName);
                                        break;
                                    }
                                }
                            }
                            catch (System.Exception ex)
                            {
                                Log.Root.LogDebug("rename fail", ex);
                            }
                        }
                    }
                    fInfo = null;
                }
            }
            catch (System.Exception ex)
            {
                Log.Root.LogDebug("Failed to RenameLogFileExtra", ex);
            }
            m_listNeedDelFiles.Clear();
        }

        private bool IsAllNumber(string strNubmer)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("^[0-9]{1,}$");
            bool Valid = false;
            if (!string.IsNullOrWhiteSpace(strNubmer) && reg.IsMatch(strNubmer))
            {
                Valid = true;
            }
            return Valid;
        }

        private void OnMonitorThread()
        {
            int ret = 0;
            try
            {
                TimeSpan day = new TimeSpan(1, 0, 0, 0);
                while (true)
                {
                    ret = WaitHandle.WaitAny(m_arrEvts, 1200000); //20 minutes
                    if ( 0 == ret )
                    {
                        m_arrEvts[0].Reset();
                        break;
                    }
                    else if ( 1 == ret )
                    {
                        m_arrEvts[1].Reset();
                    }
                    else if ( WaitHandle.WaitTimeout == ret )
                    {
                        //a day
                        if ( m_timer.Elapsed >= day )
                        {
                            m_timer.Restart();
                            DeleteLogFileByDateRule();
                        }
                        //for size
                        if ( !string.IsNullOrEmpty(m_driverName) )
                        {
                            DriveInfo dInfo = new DriveInfo(m_driverName);
                            if ( dInfo.TotalFreeSpace < m_reservedSize )
                            {
                                DeleteLogFileBySize();
                            }
                            dInfo = null;
                        }
                        //compress
                        DateTime now = DateTime.Now;
                        if ( now.Hour >= 3 &&
                             now.Hour <= 5 )
                        {
                            CompressLogFiles();
                        }
                    }
                }
            }
            catch ( ThreadAbortException exp )
            {
                Log.Root.LogWarn("The monitor thread has been aborted", exp);
                Thread.ResetAbort();
            }
            catch (System.Exception ex)
            {
                Log.Root.LogError("Failed to start monitor thread",ex);
            }
        }
#endregion

#region property
        public static GrgLogManager Instance
        {
            get
            {
                return s_logManager;
            }
        }
#endregion

#region field
        protected static GrgLogManager s_logManager;

        protected int m_backupDay = 90;

        protected long m_reservedSize = s_defaultReservedSize;

        protected string m_logPath = null;

        protected string m_driverName = null;

        protected List<string> m_listNeedDelFiles = new List<string>();

        //protected List<string> m_listCompressFilePatterns = new List<string>();

//        protected string m_compressFolder = @"log\";

        private List<CompressStrategyParam> m_listCompressStrategy = new List<CompressStrategyParam>();

        protected Dictionary<DateTime, List<string>> m_dicNeedDelFiles = new Dictionary<DateTime, List<string>>();

        private const long s_defaultReservedSize = 367001600; //350MB

        private const string s_backupDayAttri = "backupDay";

        private const string s_reservedSizeAttri = "reservedSize";

        private const string s_compressFilePatternAttri = "filePattern";

        private const string s_compressFolderAttri = "folder";

        private const string s_itemNode = "Item";

        private const string s_compressStrategyNode = "CompressStrategy";

        public const string s_logCompressFile = "logCompressFiles.txt";

        public const string s_7zCompressFormat = "a -tzip -mx7 -ssw {0} @{1}";

        private const int s_OneMBSize = 1048576;

        private Thread m_monitorThread = null;

        private ManualResetEvent[] m_arrEvts = new ManualResetEvent[]
        {
            new ManualResetEvent(false),
            new ManualResetEvent(false)
        };

        private Stopwatch m_timer = new Stopwatch();
#endregion
    }

    class CompressStrategyParam
    {
        public string Folder
        {
            get
            {
                return m_folder;
            }
            set
            {
                m_folder = value;
            }
        }

        public List<string> FilePatternCollection
        {
            get
            {
                return m_listFilePattern;
            }
            set
            {
                m_listFilePattern = value;
            }
        }

        private string m_folder = null;

        private List<string> m_listFilePattern = null;
    }
}
