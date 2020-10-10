/********************************************************************
	FileName:   StringFormatterImp
    purpose:	

	author:		huang wei
	created:	2013/01/06

    revised history:
	2013/01/06  

================================================================
    Copyright (C) 2013, Grgbanking CO,. Ltd. All rights reserved.
================================================================
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Attribute4ECAT;
using System.Runtime.InteropServices;

namespace StringFormatter
{
    [GrgComponent("{482CE92E-E07D-4F66-9308-3342EDFDCCF4}",
                    Name="StringFormatter",
                    Author="huang wei",
                    Company = "Grgbanking co,. Ltd" )]
    public class StringFormatterImp : IValueFormat<string>
    {
#region constructor
        private StringFormatterImp()
        {

        }

        static StringFormatterImp()
        {
            s_Imp = new StringFormatterImp();
        }
#endregion

#region create
        [GrgCreateFunction("Create")]
        public static IValueFormat<string> Create()
        {
            return s_Imp as IValueFormat<string>;
        }
#endregion

#region method of the IValueFormat interface
        void IValueFormat<string>.Format( string argValue,
                                          string argFormatPattern,
                                          out string argResult )
        {
            argResult = null;
            if (string.IsNullOrEmpty(argValue) ||
                 string.IsNullOrEmpty(argFormatPattern))
            {
                throw new ArgumentNullException("argValue or argFormatPattern");
            }
            argValue = argValue.Trim();

            string[] arrPattern = argFormatPattern.Split(new char[] { FormatSymbol.s_linkSymbol }, StringSplitOptions.RemoveEmptyEntries);
            string argFormatValue = argValue;
            foreach ( string Pattern in arrPattern )
            {
                FormatString( argFormatValue, 
                              Pattern,
                              out argResult);
                argFormatValue = argResult;
            }
        }

        private void FormatString( string argValue,
                                   string argFormatPattern,
                                   out string argResult )
        {
            argResult = null;

            m_formatContent.Clear();

            bool headPoint = argFormatPattern[0] == FormatSymbol.s_lineHeadSymbol ? true : false;
            bool endPoint = argFormatPattern[argFormatPattern.Length - 1] == FormatSymbol.s_lineEndSymbol ? true : false;
            argFormatPattern = argFormatPattern.Trim(s_trimSymbol);
            int leftDigSymbolIndex = argFormatPattern.IndexOf(FormatSymbol.s_digLeftSymbol, 0);
            int rightDigSymbolIndex = argFormatPattern.IndexOf(FormatSymbol.s_digRightSymbol, argFormatPattern.Length - 1);
            string digital = null;
            if ( -1 == leftDigSymbolIndex ||
                 -1 == rightDigSymbolIndex ||
                 rightDigSymbolIndex < leftDigSymbolIndex ||
                 leftDigSymbolIndex == (rightDigSymbolIndex - 1) ||
                 rightDigSymbolIndex != argFormatPattern.Length - 1 )
            {
                leftDigSymbolIndex = rightDigSymbolIndex = -1;
            }
            else
            {
                leftDigSymbolIndex += 1;
                string digitalFormat = argFormatPattern.Substring( leftDigSymbolIndex, rightDigSymbolIndex - leftDigSymbolIndex );
                if ( argValue.Length <= digitalFormat.Length )
                {
                    return;
                }

                StringBuilder digBuidler = new StringBuilder();
                int valueIndex = argValue.Length - 1;
                for ( int formatIndex = digitalFormat.Length - 1; formatIndex >= 0; --formatIndex )
                {
                    if (digitalFormat[formatIndex] == FormatSymbol.s_placeHolder)
                    {
                        digBuidler.Append(argValue[valueIndex]);
                        --valueIndex;
                    }
                    else
                    {
                        digBuidler.Append(digitalFormat[formatIndex]);
                    }
                }

                Char[] arrContent = digBuidler.ToString().ToCharArray();
                Array.Reverse(arrContent);
                digital = new string(arrContent);
                digBuidler.Clear();
                digBuidler = null;

                argFormatPattern = argFormatPattern.Remove(leftDigSymbolIndex - 1);
                if ( (valueIndex + 1) != argValue.Length )
                {
                    argValue = argValue.Remove(valueIndex + 1);
                }            
            }

            int leftReplaceSymbolIndex = argFormatPattern.IndexOf(FormatSymbol.s_leftReplaceSymbol, 0);
            int rightReplaceSymbolIndex = argFormatPattern.LastIndexOf(FormatSymbol.s_rightReplaceSymbol, argFormatPattern.Length - 1);
            if (-1 == leftReplaceSymbolIndex ||
                 -1 == rightReplaceSymbolIndex ||
                 rightReplaceSymbolIndex < leftReplaceSymbolIndex ||
                 leftReplaceSymbolIndex == (rightReplaceSymbolIndex - 1))
            {
                leftReplaceSymbolIndex = -1;
                rightReplaceSymbolIndex = -1;
            }
            else
            {
                argFormatPattern = argFormatPattern.Remove(leftReplaceSymbolIndex, 1);
                rightReplaceSymbolIndex -= 1;
                argFormatPattern = argFormatPattern.Remove(rightReplaceSymbolIndex, 1);
                rightReplaceSymbolIndex -= 1;
            }

            int contentLength = argValue.Length; ;
            int PatternLength = argFormatPattern.Length;
            int contentIndex = 0;
            int patternIndex = 0;
            bool SequalOrder = true;
            if ( headPoint &&
                 endPoint )
            {
                //To do
                if (-1 != rightReplaceSymbolIndex &&
                     -1 != leftReplaceSymbolIndex)
                {
                    if ( argValue.Length > argFormatPattern.Length )
                    {
                        string replace = argFormatPattern.Substring(leftReplaceSymbolIndex, rightReplaceSymbolIndex - leftReplaceSymbolIndex + 1);
                        StringBuilder builder = new StringBuilder(argFormatPattern);
                        builder.Remove(leftReplaceSymbolIndex, replace.Length);
                        int deltra = argValue.Length - argFormatPattern.Length;
                        rightReplaceSymbolIndex += deltra;
                        replace = replace.PadRight(deltra + replace.Length, replace[0]);
                        builder.Insert(leftReplaceSymbolIndex, replace);
                        argFormatPattern = builder.ToString();
                        PatternLength = argFormatPattern.Length;
                    }
                }
            }
            else if (headPoint)
            {
                int Length = Math.Min(PatternLength, contentLength);
                if (Length < argValue.Length)
                {
                    contentLength = Length;
                }
            }
            else if (endPoint)
            {
                contentIndex = contentLength - 1;
                patternIndex = PatternLength - 1;
                SequalOrder = false;
            }
            else
            {
                if ( argFormatPattern[0] == FormatSymbol.s_placeHolder )
                {
                    SequalOrder = true;
                }
                else
                {
                    contentIndex = contentLength - 1;
                    patternIndex = PatternLength - 1;
                    SequalOrder = false;
                }
            }

            if (SequalOrder)
            {
                while (true)
                {
                    if (leftReplaceSymbolIndex <= patternIndex &&
                         patternIndex <= rightReplaceSymbolIndex)
                    {
                        m_formatContent.Append(argFormatPattern[patternIndex]);
                        ++contentIndex;
                    }
                    else if (argFormatPattern[patternIndex] != FormatSymbol.s_placeHolder)
                    {
                        m_formatContent.Append(argFormatPattern[patternIndex]);
                    }
                    else
                    {
                        if (contentIndex >= contentLength)
                        {
                            break;
                        }
                        m_formatContent.Append(argValue[contentIndex]);
                        ++contentIndex;
                    }

                    ++patternIndex;
                    if (patternIndex >= PatternLength)
                    {
                        if (headPoint)
                        {
                            break;
                        }
                        if ( contentIndex == 0 )
                        {
                            m_formatContent.Clear();
                            break;
                        }

                        patternIndex = 0;
                    }

                    if (contentIndex >= contentLength)
                    {
                        break;
                    }
                }

                if (headPoint &&
                     contentIndex < argValue.Length)
                {
                    int StartIndex = contentIndex;
                    int Count = argValue.Length - contentIndex;
                    m_formatContent.Append(argValue, StartIndex, Count);
                    argResult = m_formatContent.ToString();
                }
                else if ( contentIndex == 0 )
                {
                    argResult = argValue;
                }
                else
                {
                    argResult = m_formatContent.ToString();
                }

                if ( !string.IsNullOrEmpty(digital) )
                {
                    argResult += digital;
                }
            }
            else
            {
                while (true)
                {
                    if (leftReplaceSymbolIndex <= patternIndex &&
                         patternIndex <= rightReplaceSymbolIndex)
                    {
                        m_formatContent.Append(argFormatPattern[patternIndex]);
                        --contentIndex;
                    }
                    else if (argFormatPattern[patternIndex] != FormatSymbol.s_placeHolder)
                    {
                        m_formatContent.Append(argFormatPattern[patternIndex]);
                    }
                    else
                    {
                        m_formatContent.Append(argValue[contentIndex]);
                        --contentIndex;
                    }

                    --patternIndex;
                    if (patternIndex < 0)
                    {
                        if (endPoint)
                        {
                            break;
                        }
                        if ( contentIndex == (contentLength - 1) )
                        {
                            m_formatContent.Clear();
                            break;
                        }
                        patternIndex = PatternLength - 1;
                    }

                    if (contentIndex < 0)
                    {
                        break;
                    }
                }

                if ( endPoint &&
                     contentIndex >= 0)
                {
                    for (int i = contentIndex; i >= 0; --i)
                    {
                        m_formatContent.Append(argValue[i]);
                    }

                    Char[] arrContent = m_formatContent.ToString().ToCharArray();
                    Array.Reverse(arrContent);
                    argResult = new string(arrContent);
                }
                else if (contentIndex == (contentLength - 1))
                {
                    //for (int i = contentIndex; i >= 0; --i)
                    //{
                    //    m_formatContent.Append(argValue[i]);
                    //}
                    argResult = argValue;
                }
                else
                {
                    Char[] arrContent = m_formatContent.ToString().ToCharArray();
                    Array.Reverse(arrContent);
                    argResult = new string(arrContent);
                }

                if ( !string.IsNullOrEmpty(digital) )
                {
                    argResult += digital;
                }
            }
        }

        public string FormatPAN(string argValue, string argFormatPattern = "[****]#$|#### ")
        {
            string result = null;
            if (string.IsNullOrEmpty(argValue) ||
                 string.IsNullOrEmpty(argFormatPattern))
            {
                return string.Empty;
            }
            argValue = argValue.Trim();

            string[] arrPattern = argFormatPattern.Split(new char[] { FormatSymbol.s_linkSymbol }, StringSplitOptions.RemoveEmptyEntries);
            string argFormatValue = argValue;
            foreach (string Pattern in arrPattern)
            {
                FormatString(argFormatValue,
                              Pattern,
                              out result);
                argFormatValue = result;
            }
            return result;
        }
        #endregion

        #region field
        public StringBuilder m_formatContent = new StringBuilder();

        public static char[] s_trimSymbol = new char[]{ FormatSymbol.s_lineHeadSymbol, FormatSymbol.s_lineEndSymbol };

        private static StringFormatterImp s_Imp = null;
#endregion
    }

    /// <summary>
    ///系统时间类
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SystemTime
    {
        public ushort Year;
        public ushort Month;
        public ushort DayofWeek;
        public ushort Day;
        public ushort Hour;
        public ushort Minute;
        public ushort Second;
        public ushort MilliSeconds;
    }

    // add by ccyao,  应该在核心有一个这样的类，而不是由“maintenance common method”去维护
    public static class CommonFunctionHandler
    {
        /// <summary>
        /// 设置系统时间
        /// </summary>
        /// <param name="st">设置时间结构体</param>
        /// <returns>true成功，否则失败</returns>
        [DllImport("Kernel32.dll")]
        private static extern bool SetLocalTime(ref SystemTime st);


        /// <summary>
        /// 设置本地系统时间
        /// </summary>
        /// <param name="argNewDatetime">系统时间</param>
        /// <returns>true成功，否则失败</returns>
        public static bool SetSystemTime(DateTime argNewDatetime)
        {
            SystemTime st = new SystemTime();
            st.Year = Convert.ToUInt16(argNewDatetime.Year);
            st.Month = Convert.ToUInt16(argNewDatetime.Month);
            st.Day = Convert.ToUInt16(argNewDatetime.Day);
            st.DayofWeek = Convert.ToUInt16(argNewDatetime.DayOfWeek);
            st.Hour = Convert.ToUInt16(argNewDatetime.Hour);
            st.Minute = Convert.ToUInt16(argNewDatetime.Minute);
            st.Second = Convert.ToUInt16(argNewDatetime.Second);
            st.MilliSeconds = Convert.ToUInt16(argNewDatetime.Millisecond);

            bool bFlag = false;
            try
            {
                bFlag = SetLocalTime(ref st);
            }
            catch (Exception ex)
            {
                //Log.MaintenanceAction.LogError(ex.Message);
                bFlag = false;
            }

            return bFlag;
        }
    }
}
