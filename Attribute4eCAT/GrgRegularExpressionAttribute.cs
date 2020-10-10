using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Attribute4ECAT
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple=true, Inherited=true)]
    public class GrgRegularExpressionAttribute : RegularExpressionAttribute,
                                                 IValidationGroupName
    {
        public GrgRegularExpressionAttribute(string argPattern)
            : base(argPattern)
        {

        }

        public string GroupName
        {
            get;
            set;
        }

        public bool AllowEmpty
        {
            get;
            set;
        }

        public override bool IsValid(object value)
        {
            if ( AllowEmpty )
            {
                if ( value == null ||
                     (value is string && string.IsNullOrEmpty((string)value)) )
                {
                    return true;
                }
            }

            return base.IsValid(value);
        }
    }
}
