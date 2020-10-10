using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Attribute4ECAT
{
    [AttributeUsage( AttributeTargets.Class, AllowMultiple=false, Inherited=false )]
    public class GrgWCFServiceAttribute : Attribute
    {
#region constructor
        public GrgWCFServiceAttribute( string argID )
        {
            m_id = argID;
        }
#endregion

#region property
        public string ID
        {
            get
            {
                return m_id;
            }
        }

        public string Name
        {
            get;
            set;
        }

        public string Author
        {
            get;
            set;
        }
#endregion

#region field
        protected string m_id;
#endregion
    }
}
