using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringFormatter
{
    public static class FormatSymbol
    {
        public const char s_placeHolder = '#';

        public const char s_leftReplaceSymbol = '[';

        public const char s_rightReplaceSymbol = ']';

        public const char s_lineHeadSymbol = '^';

        public const char s_lineEndSymbol = '$';

        public const char s_linkSymbol = '|';

        public const char s_customLeftSymbol = '(';

        public const char s_customRightSymbol = ')';

        public const char s_systemLeftSymbol = '{';

        public const char s_systemRightSymbol = '}';

        public const char s_leftPadSymbol = '-';

        public const char s_padSymbol = ':';

        public const char s_digLeftSymbol = '<';

        public const char s_digRightSymbol = '>';
    }
}
