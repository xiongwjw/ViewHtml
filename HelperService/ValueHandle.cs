using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HelperService
{   
    public static class ValueHandle
    {
        public static bool DoubleEqual( double argX,
                                        double argY )
        {
            if (Math.Abs(argX - argY) <= 0.0000001)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>   
        /// 判断输入的字符串是否只包含数字和英文字母,add by liaoyuyu 2014-10-30
        /// </summary>   
        /// <param name="input"></param>   
        /// <returns></returns>   
        public static bool IsNumAndLetter(string input)
        {
            string pattern = @"^[A-Za-z0-9]+$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(input);
        }

    }
}
