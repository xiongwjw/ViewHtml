using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringFormatter
{
    public interface IValueFormat<T>
    {
        #region method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="argValue"></param>
        /// <param name="argFormatPattern"></param>
        /// <param name="argResult"></param>
        void Format(T argValue,
                     string argFormatPattern,
                     out string argResult);

        string FormatPAN(T argValue,
                    string argFormatPattern = "[****]#$|#### ");
        #endregion
    }
}
