using System;
using System.Security.Cryptography;
using System.Text;

namespace HelperService
{

    public class AESHelper
    {

        /// <summary>
        ///  AES 加密
        /// </summary>
        /// <param name="argbytes">明文（待加密）</param>
        /// <param name="key">密文</param>
        /// <returns></returns>
        public static string AesEncrypt(byte[] argbytes, string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            if (argbytes.Length==0) return null;
            Byte[] toEncryptArray = argbytes;

            RijndaelManaged rm = new RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(key),
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7,
                KeySize = 128,
                BlockSize = 128

            };

            ICryptoTransform cTransform = rm.CreateEncryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }
        /// <summary>
        ///  AES 解密
        /// </summary>
        /// <param name="argData">明文（待解密）</param>
        /// <param name="key">密文</param>
        /// <returns></returns>
        public static string AesDecrypt(string argData, string key)
        {
            if (string.IsNullOrEmpty(key)) return argData;
            if (string.IsNullOrEmpty(argData)) return null;
            Byte[] toEncryptArray = Encoding.UTF8.GetBytes(argData); ;

            RijndaelManaged rm = new RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(key),
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7,
                KeySize = 128,
                BlockSize = 128
            };

            ICryptoTransform cTransform = rm.CreateDecryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Encoding.UTF8.GetString(resultArray);
        }
    }

}


