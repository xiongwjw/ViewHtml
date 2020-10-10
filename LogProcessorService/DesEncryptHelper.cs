using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;

namespace LogProcessorService
{
    /// <summary>
    /// DES加密帮助类
    /// </summary>
    public static class DesEncryptHelper2
    {
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="unEncryptString">明文</param>
        /// <param name="key">密钥(长度必须8位以上)</param>
        /// <returns>密文</returns>
        public static string EncryptString(string unEncryptString,string key)
        {
            if (string.IsNullOrEmpty(key) || key.Length < 8)
            {
#if DEBUG
                throw new ArgumentException("min length is 8.", "key");
#else
                return string.Empty;
#endif
                
            }

            try
            {
                using (var cryptoServiceProvider = new DESCryptoServiceProvider())
                {
                    var inputByteArray = Encoding.UTF8.GetBytes(unEncryptString);
                    cryptoServiceProvider.Key = Encoding.UTF8.GetBytes(key);
                    cryptoServiceProvider.IV = Encoding.UTF8.GetBytes(key);
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, cryptoServiceProvider.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(inputByteArray, 0, inputByteArray.Length);
                            cryptoStream.FlushFinalBlock();
                            var stringBuilder = new StringBuilder();

                            byte[] arrBytes = memoryStream.ToArray();
                            foreach (var b in arrBytes)
                            {
                                stringBuilder.AppendFormat("{0:X2}", b);
                            }

                            return stringBuilder.ToString();
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Trace.TraceError(ex.Message);
                return string.Empty;
            }
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="encryptString">密文</param>
        /// <param name="key">//密钥(长度必须8位以上)</param>
        /// <returns>明文</returns>
        public static string DecryptString(string encryptString,string key)
        {
            if (string.IsNullOrEmpty(key) || key.Length < 8)
            {
#if DEBUG
                throw new ArgumentException("min length is 8.", "key");
#else
                return string.Empty;
#endif
            }

            try
            {
                using (var cryptoServiceProvider = new DESCryptoServiceProvider())
                {
                    var inputByteArray = new byte[encryptString.Length / 2];
                    int end = encryptString.Length / 2;
                    for (var x = 0; x < end; ++x)
                    {
                        var i = (Convert.ToInt32(encryptString.Substring(x * 2, 2), 16));
                        inputByteArray[x] = (byte)i;
                    }

                    cryptoServiceProvider.Key = Encoding.UTF8.GetBytes(key);
                    cryptoServiceProvider.IV = Encoding.UTF8.GetBytes(key);

                    using (var memoryStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, cryptoServiceProvider.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(inputByteArray, 0, inputByteArray.Length);
                            cryptoStream.FlushFinalBlock();

                            return Encoding.UTF8.GetString(memoryStream.ToArray());
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Trace.TraceError(ex.Message);
                return string.Empty;
            }
        }

        public static byte[] Decrypt(byte[] encryptBuffer, string key)
        {
            if (string.IsNullOrEmpty(key) || key.Length < 8)
            {
#if DEBUG
                throw new ArgumentException("min length is 8.", "key");
#else
                return null;
#endif
            }

            try
            {
                using (var cryptoServiceProvider = new DESCryptoServiceProvider())
                {
                    cryptoServiceProvider.Key = Encoding.UTF8.GetBytes(key);
                    cryptoServiceProvider.IV = Encoding.UTF8.GetBytes(key);

                    using (var memoryStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, cryptoServiceProvider.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(encryptBuffer, 0, encryptBuffer.Length);
                            cryptoStream.FlushFinalBlock();

                            return memoryStream.ToArray();
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Trace.TraceError(ex.Message);
                return null;
            }
        }

        public static byte[] Encrypt(byte[] unEncryptBuffer, string key)
        {
            if (string.IsNullOrEmpty(key) || key.Length < 8)
            {
#if DEBUG
                throw new ArgumentException("min length is 8.", "key");
#else
                return null;
#endif

            }

            try
            {
                using (var cryptoServiceProvider = new DESCryptoServiceProvider())
                {
                    cryptoServiceProvider.Key = Encoding.UTF8.GetBytes(key);
                    cryptoServiceProvider.IV = Encoding.UTF8.GetBytes(key);
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, cryptoServiceProvider.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(unEncryptBuffer, 0, unEncryptBuffer.Length);
                            cryptoStream.FlushFinalBlock();
                            return memoryStream.ToArray();
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Trace.TraceError(ex.Message);
                return null;
            }
        }
    }
}