using System;
using System.Linq;

namespace HelperService
{
    internal enum Operation
    {
        Encrypt,
        Decrypt
    }

    /// <summary>
    /// 连接字符串帮助类
    /// </summary>
    public static class ConnectionStringHelper
    {
        /// <summary>
        ///  获取密码加密后的连接字符串
        /// </summary>
        /// <param name="unEncryptConnectionString">密码未加密后的连接字符串</param>
        /// <param name="key">//密钥(长度必须8位以上)</param>
        /// <param name="passwordPart">连接字符串中密码部分</param>
        /// <param name="separator">分隔符</param>
        /// <returns>密码加密后的连接字符串</returns>
        public static string GetEncryptConnectionString(string unEncryptConnectionString, string key = "1a2b3c4d", string passwordPart = "Password", string separator = ";")
        {
            return GetConnectionString(Operation.Encrypt, unEncryptConnectionString, key, passwordPart, separator);
        }

        /// <summary>
        ///  获取密码解密后的连接字符串
        /// </summary>
        /// <param name="encryptConnectionString">密码加密后的连接字符串</param>
        /// <param name="key">//密钥(长度必须8位以上)</param>
        /// <param name="passwordPart">连接字符串中密码部分</param>
        /// <param name="separator">分隔符</param>
        /// <returns>密码解密后的连接字符串</returns>
        public static string GetDecryptConnectionString(string encryptConnectionString, string key = "1a2b3c4d", string passwordPart = "Password", string separator = ";")
        {
            return GetConnectionString(Operation.Decrypt, encryptConnectionString, key, passwordPart, separator);
        }

        private static string GetConnectionString(Operation operation, string connectionString, string key, string passwordPart, string separator)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            var passwordUnit = connectionString
                .Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault(
                    t => t.ToUpper().StartsWith(passwordPart.ToUpper())
                );

            if (string.IsNullOrEmpty(passwordUnit))
            {
                return connectionString;
            }

            var passwordParts = passwordUnit.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
            if (passwordParts.Length < 2)
                return connectionString;

            switch (operation)
            {
                case Operation.Decrypt:
                    passwordParts[1] = DesEncryptHelper.DecryptString(passwordParts[1], key);
                    break;
                case Operation.Encrypt:
                    passwordParts[1] = DesEncryptHelper.EncryptString(passwordParts[1], key);
                    break;
            }

            return connectionString.Replace(passwordUnit, string.Join("=", passwordParts));
        }

        /// <summary>
        /// 移除加密连接字符串中的密码部分
        /// </summary>
        /// <param name="argEncryptConnectionString"></param>
        /// <returns></returns>
        public static string GetNotEncrytConnectionString(string argEncryptConnectionString)
        {
            string connString = argEncryptConnectionString;

            if(argEncryptConnectionString.Contains(";") && argEncryptConnectionString.Contains("Password"))
            {
                string[] connStringArr = argEncryptConnectionString.Split(new string[]{";"}, StringSplitOptions.RemoveEmptyEntries);

                if (connStringArr.Length > 0)
                {
                    connString = connStringArr[0];
                }
            }

            return connString;
        }
    }
}