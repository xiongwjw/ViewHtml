using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eCATActivityTest
{
    public interface IDataCache
    {
        bool Get(string key,out object obj);
        bool Set(string key, object value, Type t);
    }
}
