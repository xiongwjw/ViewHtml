using System.Diagnostics;
using System.IO;

namespace HelperService
{
    public static class LogFileDecryptHelper
    {
        /// <summary>
        /// 日志解密
        /// </summary>
        /// <param name="sourceFilePath">加密后的日志文件</param>
        /// <param name="targetFilePath">解密后的日志文件</param>
        /// <param name="key">//密钥(长度必须8位以上)</param>
        public static void DecryptFile(string sourceFilePath, string targetFilePath, string key)
        {
            using (var streamReader = File.OpenText(sourceFilePath))
            {
                if (File.Exists(targetFilePath))
                {
                    File.SetAttributes(targetFilePath, FileAttributes.Normal);
                    File.Delete(targetFilePath);
                }

                using (var streamWriter = File.CreateText(targetFilePath))
                {
                    while (!streamReader.EndOfStream)
                    {
                        var line = streamReader.ReadLine();
                        string decryptString;
                        //将加密串首尾加上"VvvvV"以区分加密串和非加密串混合出现的情况
                        if ((line.StartsWith("VvvvV") && line.EndsWith("VvvvV")))
                        {
                            decryptString = DesEncryptHelper.DecryptString(line.Trim("VvvvV".ToCharArray()), key);
                        }
                        else
                        {
                            decryptString = line + "\r\n";
                        }

                        streamWriter.Write(decryptString);
                    }

                    streamWriter.Flush();
                    //streamWriter.Close();
                }
            }
        }
    }
}
