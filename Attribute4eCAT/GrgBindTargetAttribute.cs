using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Attribute4ECAT
{
    [Flags]
    public enum TargetType : byte
    {
        Unknown = 0,
        Int,
        UInt,
        Long,
        ULong,
        Short,
        UShort,
        Byte,
        Char,
        Float,
        Double,
        String,
        Enum,
        Bool
    }

    [Flags]
    public enum AccessRight : byte
    {
        OnlyRead = 0,
        OnlyWrite,
        ReadAndWrite
    }

    [AttributeUsage( AttributeTargets.Property, AllowMultiple = false, Inherited = true )]
    public class GrgBindTargetAttribute : Attribute
    {
#region constructor
        public GrgBindTargetAttribute( string name )
        {
            Debug.Assert(!string.IsNullOrEmpty(name));
            m_targetName = name;
            Type = TargetType.Unknown;
            Access = AccessRight.ReadAndWrite;
        }
#endregion

#region property
        public string Name
        {
            get
            {
                return m_targetName;
            }
        }

        public TargetType Type
        {
            set;
            get;
        }

        public AccessRight Access
        {
            get;
            set;
        }
#endregion

#region field
        public string m_targetName = null;
#endregion
    }
}
