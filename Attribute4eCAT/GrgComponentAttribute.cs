using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Attribute4ECAT
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class GrgComponentAttribute : System.Attribute
    {
        public GrgComponentAttribute(string strID)
        {
            ID = strID;
        }

        public string ID
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        [DefaultValue("Generic")]
        public string Catalog
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public string Description
        {
            get;
            set;
        }

        [DefaultValue("unknown")]
        public string Author
        {
            get;
            set;
        }

        [DefaultValue("Grgbanking.ltd.com")]
        public string Company
        {
            get;
            set;
        }

        [DefaultValue("www.Grgbanking.com/support/activity/")]
        public string SupportUrl
        {
            get;
            set;
        }
    }
}
