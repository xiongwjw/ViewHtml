using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eCATActivityTest
{
    public class eCatTransactionDataCache : IDataCache
    {

        private Dictionary<string, object> datacache = new Dictionary<string, object>();

        public bool Get(string key, out object argObj)
        {
            object obj = null;
            datacache.TryGetValue(key, out obj);
            argObj = obj;
            return true;
        }

        public bool Set(string key, object value, Type t)
        {
            if (!datacache.ContainsKey(key))
                datacache.Add(key, value);
            else
            {
                datacache[key] = value;
            }
            return true;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"type is {this.GetType().Name}").AppendLine();
            foreach (var item in datacache)
            {
                sb.Append($"{item.Key}:{item.Value}").AppendLine();
            }
            return sb.ToString();
        }
    }
}
