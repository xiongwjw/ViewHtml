using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace HelperService
{
    /// <summary>
    /// 操作系统操作帮助类
    /// </summary>
    public static class OsOperationHelper
    {
        private static string _currentWorkDirectory = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        ///  重启操作系统
        /// </summary>
        public static void Reboot()
        {
            try
            {
                Reboot1();
                Reboot2();
                Reboot3();
            }
            catch (Exception ex)
            {
                throw new Exception("Reboot Fail", ex);
            }
        }

        /// <summary>
        /// 关闭操作系统
        /// </summary>
        public static void Shutdown()
        {
            try
            {
                Shutdown1();
                Shutdown2();
            }
            catch (Exception ex)
            {
                throw new Exception("Shutdown Fail", ex);
            }
        }

        private static void Shutdown1()
        {
            RebootOrShutdown(false);
        }

        public static void Shutdown2()
        {
            try
            {
                var filePath = Path.Combine(_currentWorkDirectory, "ShutdownSystem.bat");

                GenerateShutdownBatchFile(filePath);
                StartExecuteBatchFile(filePath);
                Thread.Sleep(60000);//暂停主线程，等待操作系统关闭，避免应操作系统关闭过程中，主线程还在继续运行因操作系统关闭部分组件而导致主线程抛出错误
            }
            catch(Exception ex)
			{
                throw new Exception("Shutdown Fail", ex);
			}
        }

        private static void Reboot1()
        {
            var grgShutDownCmd = Path.Combine(_currentWorkDirectory, "GrgShutDown.exe");
            if (!File.Exists(grgShutDownCmd))
            {
                string spPath = Environment.GetEnvironmentVariable("GrgXfsSP");
                grgShutDownCmd = spPath + @"\GrgShutDown.exe";
                if (!File.Exists(grgShutDownCmd))
                {
                    return;
                }
            }

            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = grgShutDownCmd,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                var process = new Process { StartInfo = processStartInfo };
                process.Start();
                process.WaitForExit();
                Thread.Sleep(60000);//暂停主线程，等待操作系统关闭，避免应操作系统关闭过程中，主线程还在继续运行因操作系统关闭部分组件而导致主线程抛出错误
            }
            catch { }
        }

        private static void Reboot2()
        {
            RebootOrShutdown();
        }

        private static void Reboot3()
        {
            try
            {
                var filePath = Path.Combine(_currentWorkDirectory, "RebootSystem.bat");

                GenerateRebootBatchFile(filePath);
                StartExecuteBatchFile(filePath);
                Thread.Sleep(60000);//暂停主线程，等待操作系统关闭，避免应操作系统关闭过程中，主线程还在继续运行因操作系统关闭部分组件而导致主线程抛出错误
            }
            catch (Exception ex)
            {
                throw new Exception("Reboot Fail", ex);
            }
        }

        private static void RebootOrShutdown(bool argIsReboot = true)
        {
            try
            {
                var proc = new Process();
                var processStartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Environment.SystemDirectory,
                    FileName = "cmd.exe",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                };
                proc.StartInfo = processStartInfo;
                proc.Start();
                proc.StandardInput.WriteLine(
                    "shutdown.exe /{0} /f /t 0",
                    argIsReboot ? "r" : "s"
                );

                Thread.Sleep(60000);//暂停主线程，等待操作系统关闭，避免应操作系统关闭过程中，主线程还在继续运行因操作系统关闭部分组件而导致主线程抛出错误
            }
            catch { }
        }

        /// <summary>
        /// 开始执行批处理文件
        /// </summary>
        /// <param name="filePath">批处理文件路径</param>
        public static void StartExecuteBatchFile(string argFilePath)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", string.Format("/C start {0} &", argFilePath))
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true
            };

            Process.Start(processInfo);
        }

        private static void GenerateRebootBatchFile(string argFilePath)
        {
            GenerateBatchFile(
                new[] {
                    "call KillATMC.bat",
                    string.Format(@"{0}\shutdown.exe /r /f /t 0", Environment.SystemDirectory),
                    string.Format("del /S /Q {0}",argFilePath)
                },
                argFilePath
            );
        }

        private static void GenerateShutdownBatchFile(string argFilePath)
        {
            GenerateBatchFile(
                new[]{
                    "call KillATMC.bat",
                    string.Format(@"{0}\shutdown.exe /s /f /t 0", Environment.SystemDirectory),
                    string.Format("del /S /Q {0}",argFilePath)
                },
                argFilePath
            );
        }

        private static void GenerateBatchFile(IEnumerable<string> argCommands, string argFilePath)
        {
            if (File.Exists(argFilePath))
            {
                File.SetAttributes(argFilePath, FileAttributes.Normal);
                File.Delete(argFilePath);
            }
            var streamWriter = File.CreateText(argFilePath);
            foreach (var comand in argCommands)
            {
                streamWriter.WriteLine(comand);
            }

            streamWriter.Flush();
            streamWriter.Close();
        }
    }
}