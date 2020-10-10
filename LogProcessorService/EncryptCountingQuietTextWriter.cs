﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using HelperService;
using log4net.Core;
using log4net.Util;

namespace LogProcessorService
{
    public class EncryptCountingQuietTextWriter : CountingQuietTextWriter
    {
        private readonly string _key;

        public EncryptCountingQuietTextWriter(string key, TextWriter writer, IErrorHandler errorHandler)
            : base(writer, errorHandler)
        {
            _key = key;
        }

        public override void Write(char value)
        {
            try
            {
                var encryptString = DesEncryptHelper.EncryptString(
                            string.Join("", value),
                            _key
                        );
                encryptString = string.Format("{0}{1}{0}\n", "VvvvV", encryptString);
                //将加密串首尾加上"VvvvV"以区分加密串和非加密串混合出现的情况
                base.Write(encryptString);
                Count += this.Encoding.GetByteCount(encryptString);
            }
            catch (Exception e)
            {
                this.ErrorHandler.Error("Failed to write [" + value + "].", e, ErrorCode.WriteFailure);
            }
        }

        public override void Write(char[] buffer, int index, int count)
        {
            if (count > 0)
            {
                try
                {
                    var encryptString = DesEncryptHelper.EncryptString(
                        string.Join("", buffer.Skip(index).Take(count)),
                        _key
                    );
                    encryptString = string.Format("{0}{1}{0}\n", "VvvvV", encryptString);
                    //将加密串首尾加上"VvvvV"以区分加密串和非加密串混合出现的情况
                    base.Write(encryptString);
                    Count += this.Encoding.GetByteCount(encryptString);
                }
                catch (Exception e)
                {
                    this.ErrorHandler.Error("Failed to write buffer.", e, ErrorCode.WriteFailure);
                }
            }
        }

        public override void Write(string str)
        {
            if (string.IsNullOrEmpty(str))
                return;

            try
            {
                var encryptString = DesEncryptHelper.EncryptString(
                           string.Join("", str),
                           _key
                       );
                encryptString = string.Format("{0}{1}{0}\n", "VvvvV", encryptString);
                //将加密串首尾加上"VvvvV"以区分加密串和非加密串混合出现的情况
                base.Write(encryptString);
                Count += this.Encoding.GetByteCount(encryptString);
            }
            catch (Exception e)
            {
                this.ErrorHandler.Error("Failed to write [" + str + "].", e, ErrorCode.WriteFailure);
            }
        }
    }
}
