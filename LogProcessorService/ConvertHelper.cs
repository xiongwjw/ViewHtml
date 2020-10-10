/********************************************************************
	FileName:   ConvertHelper
    purpose:	

	author:		huang wei
	created:	2013/04/24

    revised history:
	2013/04/24  

================================================================
    Copyright (C) 2013, Grgbanking CO,. Ltd. All rights reserved.
================================================================
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace LogProcessorService
{
    public class ConvertHelper2
    {
        static ConvertHelper2()
        {
            s_format = new StringBuilder(64);
            s_AscII = new StringBuilder(64);
        }

        /// <summary>
        /// 右补字符（unicode 字符占2位）
        /// </summary>
        /// <param name="argValue">需要填补的字符串</param>
        /// <param name="argTotalByteCount">目标长度</param>
        /// <param name="argPadChar">填补字符</param>
        /// <returns>右补至目标长度的字符串</returns>
        public static string PadRightEx(string argValue, int argTotalByteCount, char argPadChar)
        {
            Encoding coding = Encoding.GetEncoding("gb2312");
            int dcount = 0;
            foreach (char ch in argValue.ToCharArray())
            {
                if (coding.GetByteCount(ch.ToString()) == 2)
                    dcount++;
            }
            string w = argValue.PadRight(argTotalByteCount - dcount, argPadChar);
            return w;
        }

        public static int GetStringByteLen(string argValue)
        {
            if (string.IsNullOrEmpty(argValue))
                return 0;

            Encoding coding = Encoding.GetEncoding("gb2312");
            int dcount = 0;
            foreach (char ch in argValue.ToCharArray())
            {
                if (coding.GetByteCount(ch.ToString()) == 2)
                    dcount++;
            }
            return argValue.Length + dcount;
        }

        /// <summary>
        /// 左补字符（unicode 字符占2位）
        /// </summary>
        /// <param name="argValue">需要填补的字符串</param>
        /// <param name="argTotalByteCount">目标长度</param>
        /// <param name="argPadChar">填补字符</param>
        /// <returns>左补至目标长度的字符串</returns>
        public static string PadLeftEx(string argValue, int argTotalByteCount, char argPadChar)
        {
            Encoding coding = Encoding.GetEncoding("gb2312");
            int dcount = 0;
            foreach (char ch in argValue.ToCharArray())
            {
                if (coding.GetByteCount(ch.ToString()) == 2)
                    dcount++;
            }
            string w = argValue.PadLeft(argTotalByteCount - dcount, argPadChar);
            return w;
        }

        /// <summary>
        /// Convert a string to contain HEX code to a byte array (DEC).
        /// e.g. HEX string "2C1F035A" => byte array (DEC) : { 44, 15, 3, 90 }  (HEX) : { 0x2C, 0x1F, 0x03, 0x5A }
        /// </summary>
        /// <param name="argHexString">A string to contain hex code</param>
        /// <param name="argBytes">a byte array</param>
        /// <param name="argSize">size of result</param>
        /// <returns>If success, return value is true, otherwise, it is false</returns>
        /// <remarks>each 2 chars in the HEX string convert to a byte</remarks>
        public static bool ConvertHexStringToBytes(string argHexString,
                                            out byte[] argBytes,
                                            out int argSize)
        {
            argBytes = null;
            argSize = 0;

            if (string.IsNullOrEmpty(argHexString))
            {
                return false;
            }

            try
            {
                List<byte> listBuffer = new List<byte>();
                int total = argHexString.Length;
                byte bufferValue = 0;
                for (int i = 0; i < total; i += 2)
                {
                    bufferValue = (byte)(ConvertCharToByte(argHexString[i]) * 16 + ConvertCharToByte(argHexString[i + 1]));
                    listBuffer.Add(bufferValue);
                }

                argBytes = listBuffer.ToArray();
                argSize = listBuffer.Count;

                listBuffer.Clear();
                listBuffer = null;

                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        public static bool PadLeft4ByteArray(byte[] argInArray,
                                              int argSuggestSize,
                                              byte argPadValue,
                                              out byte[] argResult)
        {
            argResult = null;
            if (null == argInArray ||
                 argSuggestSize <= 0)
            {
                return false;
            }

            using (MemoryStream temp = new MemoryStream(argSuggestSize))
            {
                if (argSuggestSize < argInArray.Length)
                {
                    temp.Write(argInArray, 0, argSuggestSize);
                }
                else
                {
                    int padCount = argSuggestSize - argInArray.Length;
                    for (int i = 0; i < padCount; ++i)
                    {
                        temp.WriteByte(argPadValue);
                    }
                    temp.Write(argInArray, 0, argInArray.Length);
                }

                argResult = temp.ToArray();
            }

            return true;
        }

        public static bool PadRight4ByteArray(byte[] argInArray,
                                               int argSuggestSize,
                                               byte argPadValue,
                                               out byte[] argResult)
        {
            argResult = null;
            if (null == argInArray ||
                 argSuggestSize <= 0)
            {
                return false;
            }

            using (MemoryStream temp = new MemoryStream(argSuggestSize))
            {
                if (argSuggestSize < argInArray.Length)
                {
                    temp.Write(argInArray, 0, argSuggestSize);
                }
                else
                {
                    temp.Write(argInArray, 0, argInArray.Length);
                    int padCount = argSuggestSize - argInArray.Length;
                    for (int i = 0; i < padCount; ++i)
                    {
                        temp.WriteByte(argPadValue);
                    }
                }

                argResult = temp.ToArray();
            }

            return true;
        }

        /// <summary>
        /// Convert a acsii char to a byte.
        /// e,g. char '1' => 1  char 'A' => 10 char 'c' => 12
        /// </summary>
        /// <param name="argChar"></param>
        /// <returns></returns>
        public static byte ConvertCharToByte(char argChar)
        {
            byte value = 0;
            switch (argChar)
            {
                case '0':
                    {
                        value = 0;
                    }
                    break;

                case '1':
                    {
                        value = 1;
                    }
                    break;
                case '2':
                    {
                        value = 2;
                    }
                    break;
                case '3':
                    {
                        value = 3;
                    }
                    break;
                case '4':
                    {
                        value = 4;
                    }
                    break;
                case '5':
                    {
                        value = 5;
                    }
                    break;
                case '6':
                    {
                        value = 6;
                    }
                    break;
                case '7':
                    {
                        value = 7;
                    }
                    break;
                case '8':
                    {
                        value = 8;
                    }
                    break;
                case '9':
                    {
                        value = 9;
                    }
                    break;

                case 'A':
                case 'a':
                    {
                        value = 10;
                    }
                    break;

                case 'B':
                case 'b':
                    {
                        value = 11;
                    }
                    break;

                case 'C':
                case 'c':
                    {
                        value = 12;
                    }
                    break;

                case 'D':
                case 'd':
                    {
                        value = 13;
                    }
                    break;

                case 'E':
                case 'e':
                    {
                        value = 14;
                    }
                    break;

                case 'F':
                case 'f':
                    {
                        value = 15;
                    }
                    break;
            }

            return value;
        }

        /// <summary>
        /// convert a byte array to contain HEX code to a string ( for COM log )
        /// </summary>
        /// <param name="arrBuffer"></param>
        /// <param name="argSize"></param>
        /// <returns></returns>
        public static string HexToString(byte[] arrBuffer,
                                          int argSize)
        {
            Debug.Assert(null != arrBuffer && argSize > 0);

            s_format.Clear();

            int totalLine = (argSize + s_defLineWidth - 1) / s_defLineWidth;
            int half = s_defLineWidth / 2 - 1;
            int curAddress = 0;
            int bufferIndex = 0;
            byte temp = 0;
            char[] arrAsc = null;
            for (int i = 0; i < totalLine; ++i)
            {
                //修复缺陷7221：建议eCAT日志里打印通信报文时第一行换行对齐。 modify by liaoyuyu 2014-12-12
                if (i == 0)
                {
                    s_format.AppendLine();
                }

                s_format.AppendFormat("{0:D4}h: ", curAddress);
                for (int j = 0; j < s_defLineWidth; ++j)
                {
                    bufferIndex = i * s_defLineWidth + j;
                    if (bufferIndex >= argSize)
                    {
                        if (j < half)
                        {
                            s_format.Append(' ', 2);
                        }

                        s_format.Append(' ', ((s_defLineWidth - j) * 3));

                        break;
                    }
                    else
                    {
                        temp = arrBuffer[bufferIndex];
                        s_format.AppendFormat("{0:X2} ", temp);
                        if (j == half)
                        {
                            s_format.Append("| ");
                        }
                    }
                }

                s_AscII.Clear();
                arrAsc = Encoding.ASCII.GetChars(arrBuffer, curAddress, Math.Min(s_defLineWidth, argSize - curAddress));
                foreach (var item in arrAsc)
                {
                    if (item == '\u0000' ||
                        item == '\u000A')
                    {
                        s_AscII.Append('.');
                    }
                    else if (char.IsLetterOrDigit(item) ||
                              char.IsWhiteSpace(item))
                    {
                        s_AscII.Append(item);
                    }
                    else if ('*' == item || '=' == item)
                    {
                        s_AscII.Append(item);
                    }
                    else
                    {
                        s_AscII.Append('.');
                    }
                }
                s_format.AppendFormat(" [{0}]", s_AscII.ToString());
                s_format.AppendLine();
                curAddress += s_defLineWidth;
            }

            return s_format.ToString();
        }

        /// <summary>
        /// convert a byte array to a string to contain HEX code
        /// e.g byte array {56,78,90,12,50} => string "384E5A0C32";
        /// </summary> 
        /// <param name="arrBuffer"></param>
        /// <param name="argIndex"></param>
        /// <param name="argLength"></param>
        /// <returns></returns>
        public static string ByteToString(byte[] arrBuffer,
                                           int argIndex,
                                           int argLength)
        {
            Debug.Assert(null != arrBuffer && argIndex >= 0 && argLength > 0);
            if (null == arrBuffer ||
                 argIndex < 0 ||
                 argLength <= 0)
            {
                return string.Empty;
            }

            int total = arrBuffer.Length;
            if (total < (argIndex + argLength))
            {
                return string.Empty;
            }
            int end = argIndex + argLength;

            s_format.Clear();
            for (int i = argIndex; i < end; ++i)
            {
                s_format.AppendFormat("{0:X2}", arrBuffer[i]);
            }

            return s_format.ToString();
        }

        /// <summary>
        /// argSourceStr is "1234|456|45|4"
        /// argFindStr is "4"
        /// argSep is "|"
        /// then return true, 因为在argSourceStr中找到指定的字符串"4"
        /// </summary>
        /// <param name="argSourceStr"></param>
        /// <param name="argFindStr"></param>
        /// <param name="argSep"></param>
        /// <returns></returns>
        public static bool FindStringInString(string argSourceStr, string argFindStr, char [] argSep)
        {
            if (string.IsNullOrEmpty(argSourceStr))
            {
                return false;
            }

            if (string.IsNullOrEmpty(argFindStr))
            {
                return false;
            }

            string[] arrayString = argSourceStr.Split(argSep);
            foreach (string str in arrayString)
            {
                if (!string.IsNullOrEmpty(str) && argFindStr == str)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Convert a memory to contain a byte array to a string to contain HEX code
        /// </summary>
        /// <param name="argBuffer"></param>
        /// <param name="argIndex"></param>
        /// <param name="argLenght"></param>
        /// <returns></returns>
        public static string ByteToString(MemoryStream argBuffer,
                                           int argIndex,
                                           int argLenght)
        {
            return ByteToString(argBuffer.ToArray(), argIndex, argLenght);
        }

        /// <summary>
        /// Convert a byte to contain BCD code to a string 
        /// e.g. BCD array (compressed) {12, 34} => string "1234";
        /// </summary>
        /// <param name="argBCD"></param>
        /// <param name="argSize"></param>
        /// <param name="argIsCompress"></param>
        /// <returns></returns>
        public static string BCDToString(byte[] argBCD,
                                          int argSize,
                                          bool argIsCompress = true)
        {
            Debug.Assert(null != argBCD && argBCD.Length > 0 && argSize > argBCD.Length && argSize > 0);
            if (null == argBCD ||
                 argBCD.Length <= 0 ||
                 argSize <= 0 ||
                 argBCD.Length < argSize)
            {
                return null;
            }

            s_AscII.Clear();

            try
            {
                if (argIsCompress)
                {
                    byte highValue = 0;
                    byte lowValue = 0;
                    for (int i = 0; i < argSize; ++i)
                    {
                        highValue = HighBits(argBCD[i]);
                        lowValue = LowBits(argBCD[i]);
                        s_AscII.Append(BinaryToChar(highValue));
                        s_AscII.Append(BinaryToChar(lowValue));
                    }
                }
                else
                {
                    for (int i = 0; i < argSize; ++i)
                    {
                        s_AscII.Append(BinaryToChar(argBCD[i]));
                    }
                }
            }
            catch (System.Exception ex)
            {
                return null;
            }

            return s_AscII.ToString();
        }

        /// <summary>
        /// Convert to a string to a array to contain BCD code
        /// e.g. string "1234" => BCD array (compressed) {12,34}
        /// </summary>
        /// <param name="argBCDString"></param>
        /// <param name="argBCD"></param>
        /// <param name="argSize"></param>
        /// <param name="argIsCompress"></param>
        /// <returns></returns>
        public static bool StringToBCD(string argBCDString,
                                        out byte[] argBCD,
                                        out int argSize,
                                        bool argIsCompress = true)
        {
            Debug.Assert(!string.IsNullOrEmpty(argBCDString));
            argBCD = null;
            argSize = 0;
            if (string.IsNullOrEmpty(argBCDString))
            {
                return false;
            }

            if (argIsCompress)
            {
                return ConvertHexStringToBytes(argBCDString, out argBCD, out argSize);
            }
            else
            {
                List<byte> listBCD = new List<byte>();
                int total = argBCDString.Length;
                for (int i = 0; i < total; ++i)
                {
                    listBCD.Add(ConvertCharToByte(argBCDString[i]));
                }

                argBCD = listBCD.ToArray();
                argSize = listBCD.Count;
                listBCD.Clear();
                listBCD = null;
            }

            return true;
        }

        public static bool BCDToInt(byte[] argBCD,
                                     int argSize,
                                     ref int argResult,
                                     bool argHostOrder = true,
                                     bool argIsCompress = true)
        {
            Debug.Assert(null != argBCD && argSize > 0);

            argResult = 0;
            if (argIsCompress)
            {
                byte highValue = 0;
                byte lowValue = 0;
                int bit = 0;

                if (argHostOrder)
                {
                    for (int i = argSize - 1; i >= 0; --i, ++bit)
                    {
                        highValue = HighBits(argBCD[i]);
                        lowValue = LowBits(argBCD[i]);
                        argResult = argResult + (int)(highValue * Math.Pow(16, bit + 1)) + (int)(lowValue * Math.Pow(16, bit));
                    }
                }
                else
                {
                    for (int i = 0; i < argSize; ++i, ++bit)
                    {
                        highValue = HighBits(argBCD[i]);
                        lowValue = LowBits(argBCD[i]);
                        argResult = argResult + (int)(highValue * Math.Pow(16, bit + 1)) + (int)(lowValue * Math.Pow(16, bit));
                    }
                }
            }
            else
            {

            }

            return true;
        }
        /// <summary>
        /// convert a binary to a char
        /// e.g. binary 0001 => char '1'  binary 1000 => char '8'
        /// </summary>
        /// <param name="argValue"></param>
        /// <returns></returns>
        public static char BinaryToChar(byte argValue)
        {
            char result;
            switch (argValue)
            {
                case 0:
                    {
                        result = '0';
                    }
                    break;

                case 1:
                    {
                        result = '1';
                    }
                    break;

                case 2:
                    {
                        result = '2';
                    }
                    break;

                case 3:
                    {
                        result = '3';
                    }
                    break;

                case 4:
                    {
                        result = '4';
                    }
                    break;

                case 5:
                    {
                        result = '5';
                    }
                    break;

                case 6:
                    {
                        result = '6';
                    }
                    break;

                case 7:
                    {
                        result = '7';
                    }
                    break;

                case 8:
                    {
                        result = '8';
                    }
                    break;

                case 9:
                    {
                        result = '9';
                    }
                    break;

                default:
                    {
                        throw new Exception("illegal binary value");
                    }

            }

            return result;
        }
        /// <summary>
        /// Convert a ascii string to a HEX string
        /// e.g. ascii string "hello" => hex string "68656C6C6F"
        /// </summary>
        /// <param name="argAscII"></param>
        /// <param name="argSize"></param>
        /// <returns></returns>
        public static string AscIIToHexString(string argAscII,
                                               int argSize)
        {
            Debug.Assert(!string.IsNullOrEmpty(argAscII) && argSize > 0);
            if (string.IsNullOrEmpty(argAscII) ||
                 argSize <= 0)
            {
                return null;
            }

            int total = Math.Min(argSize, argAscII.Length);
            s_AscII.Clear();
            for (int i = 0; i < total; ++i)
            {
                s_AscII.AppendFormat("{0:X2}", Convert.ToByte(argAscII[i]));
            }

            return s_AscII.ToString();
        }
        /// <summary>
        /// Convert a hex string to a ascii string
        /// e.g. hex string "68656C6C6F" => ascii string "hello"
        /// </summary>
        /// <param name="argHexString"></param>
        /// <param name="argSize"></param>
        /// <returns></returns>
        public static string HexStringToAscII(string argHexString,
                                               int argSize)
        {
            Debug.Assert(!string.IsNullOrEmpty(argHexString) && argSize > 0);
            if (string.IsNullOrEmpty(argHexString) ||
                 argSize <= 0)
            {
                return null;
            }

            byte[] arrBuffer = null;
            int size = 0;
            ConvertHexStringToBytes(argHexString, out arrBuffer, out size);
            Debug.Assert(null != arrBuffer);

            return Encoding.ASCII.GetString(arrBuffer);
        }

        /// <summary>
        /// Get low bits of a byte
        /// e.g. byte 0x2C => low bits C
        /// </summary>
        /// <param name="argValue"></param>
        /// <returns></returns>
        public static byte LowBits(byte argValue)
        {
            return (byte)(argValue & s_lowBitMask);
        }
        /// <summary>
        /// get high bits of a byte
        /// e.g. byte 0x2C => high bits 2
        /// </summary>
        /// <param name="argValue"></param>
        /// <returns></returns>
        public static byte HighBits(byte argValue)
        {
            return (byte)((argValue & s_highBitMask) >> 4);
        }
        /// <summary>
        /// get low byte of a short
        /// e.g. short 0x2F5C => low byte 5C
        /// </summary>
        /// <param name="argValue"></param>
        /// <returns></returns>
        public static byte LowByte(short argValue)
        {
            return (byte)(argValue & s_lowbyteMask);
        }
        /// <summary>
        /// get high byte of a short
        /// e.g. short 0x2F5C => high byte 2F
        /// </summary>
        /// <param name="argValue"></param>
        /// <returns></returns>
        public static byte HighByte(short argValue)
        {
            return (byte)((argValue & s_highbyteMask) >> 8);
        }
        /// <summary>
        /// get low word of a int
        /// e.g int 0x8C7E0123 => low word 0123
        /// </summary>
        /// <param name="argValue"></param>
        /// <returns></returns>
        public static short LowWord(int argValue)
        {
            return (short)(argValue & s_lowWordMask);
        }
        /// <summary>
        /// get high word of a int
        /// e.g int 0x8C7E0123 => high word 8C7E
        /// </summary>
        /// <param name="argValue"></param>
        /// <returns></returns>
        public static short HighWord(int argValue)
        {
            return (short)((argValue & s_highWordMask) >> 16);
        }

        /// <summary>
        /// EncryptStringWithXOR
        /// e.g 123456 => 122961229912298123011230012303
        /// </summary>
        /// <param name="argString"></param>
        /// <param name="argRandomNumber"></param>
        /// <returns></returns>
        public static string EncryptStringWithXOR(string argString, int argRandomNumber = 0)
        {
            StringBuilder sb = new StringBuilder();
            if (argRandomNumber < 0 || argRandomNumber > 65535)
            {
                argRandomNumber = 12345;
            }
            foreach (char item in argString)
            {
                sb.AppendFormat("{0:D5}", item ^ argRandomNumber);
            }

            return sb.ToString();
        }

        /// <summary>
        /// DecryptStringWithXOR
        /// e.g 122961229912298123011230012303 => 123456
        /// </summary>
        /// <param name="argString"></param>
        /// <param name="argRandomNumber"></param>
        /// <returns></returns>
        public static string DecryptStringWithXOR(string argString, int argRandomNumber = 0)
        {
            StringBuilder sb = new StringBuilder();
            if (argRandomNumber < 0 || argRandomNumber > 65535)
            {
                argRandomNumber = 12345;
            }

            try
            {
                for (int i = 0; i < argString.Length; i += 5)
                {
                    int value = int.Parse(argString.Substring(i, 5)) ^ argRandomNumber;
                    sb.Append((char)value);
                }
            }
            catch (System.Exception ex)
            {
                sb.Clear();
            }

            return sb.ToString();
        }

        private static StringBuilder s_format = null;

        private static StringBuilder s_AscII = null;

        private const int s_defLineWidth = 20;

        private const bool s_showAddress = true;

        private const bool s_showAscII = true;

        private const byte s_lowBitMask = 0x0F;

        private const byte s_highBitMask = 0xF0;

        private const int s_lowbyteMask = 0x00FF;

        private const int s_highbyteMask = 0xFF00;

        private const int s_lowWordMask = 0x0000FFFF;

        private const uint s_highWordMask = 0xFFFF0000;
    }
}
