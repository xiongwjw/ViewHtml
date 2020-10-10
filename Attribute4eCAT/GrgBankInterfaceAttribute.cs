using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Attribute4ECAT
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class GrgBankInterfaceAttribute : System.Attribute
    {
#region constructor
        public GrgBankInterfaceAttribute()
        {

        }
#endregion

#region property
        /// <summary>
        /// 工程插件的名称
        /// </summary>
        public string Name
        {
            get;
            set;
        }
        /// <summary>
        /// 该工程插件的描述信息
        /// </summary>
        public string Description
        {
            get;
            set;
        }
        /// <summary>
        /// 工程插件被应用的银行
        /// </summary>
        public string Bank
        {
            get;
            set;
        }
        /// <summary>
        /// 工程插件依赖的最低核心版本
        /// </summary>
        public string DependentKernelVersion
        {
            get;
            set;
        }
        /// <summary>
        /// 工程插件的开发创建时间
        /// </summary>
        public string CreateTime
        {
            get;
            set;
        }
        /// <summary>
        /// 工程插件的开发人员
        /// </summary>
        public string Author
        {
            get;
            set;
        }
        /// <summary>
        /// 工程插件的修改记录
        /// </summary>
        public List<string> RevisedHistory
        {
            get;
            set;
        }
#endregion
    }
}
