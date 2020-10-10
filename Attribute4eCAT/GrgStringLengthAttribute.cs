using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Attribute4ECAT
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple=true, Inherited=true)]
    public class GrgStringLengthAttribute : StringLengthAttribute,
                                            IValidationGroupName
    {
        public GrgStringLengthAttribute(int argMaxNumber)
            : base(argMaxNumber)
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
            if ( AllowEmpty && null == value )
            {
                return true;
            }

            return base.IsValid(value);
        }
    }
}
