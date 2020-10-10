using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Attribute4ECAT
{
    [AttributeUsage( AttributeTargets.Method, AllowMultiple=false, Inherited=false )]
    public class GrgCreateFunctionAttribute : Attribute
    {
        public GrgCreateFunctionAttribute( string strName )
        {
            m_strNameOfMethod = strName;
        }

        public string[] Parameters
        {
            get;
            set;
        }

        public string NameOfMethod
        {
            get
            {
                return m_strNameOfMethod;
            }
        }

        private string m_strNameOfMethod;
    }
}
