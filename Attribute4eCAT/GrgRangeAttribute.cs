using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Attribute4ECAT
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple=true, Inherited=true)]
    public class GrgRangeAttribute : RangeAttribute,
                                     IValidationGroupName
    {
                // Summary:
        //     Initializes a new instance of the System.ComponentModel.DataAnnotations.RangeAttribute
        //     class by using the specified minimum and maximum values.
        //
        // Parameters:
        //   minimum:
        //     Specifies the minimum value allowed for the data field value.
        //
        //   maximum:
        //     Specifies the maximum value allowed for the data field value.
        public GrgRangeAttribute(double minimum, double maximum) : base(minimum, maximum)
        {

        }
        //
        // Summary:
        //     Initializes a new instance of the System.ComponentModel.DataAnnotations.RangeAttribute
        //     class by using the specified minimum and maximum values.
        //
        // Parameters:
        //   minimum:
        //     Specifies the minimum value allowed for the data field value.
        //
        //   maximum:
        //     Specifies the maximum value allowed for the data field value.
        public GrgRangeAttribute(int minimum, int maximum)
            : base(minimum, maximum)
        {

        }
        //
        // Summary:
        //     Initializes a new instance of the System.ComponentModel.DataAnnotations.RangeAttribute
        //     class by using the specified minimum and maximum values and the specific
        //     type.
        //
        // Parameters:
        //   type:
        //     Specifies the type of the object to test.
        //
        //   minimum:
        //     Specifies the minimum value allowed for the data field value.
        //
        //   maximum:
        //     Specifies the maximum value allowed for the data field value.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     type is null.
        public GrgRangeAttribute(Type type, string minimum, string maximum)
            : base(type, minimum, maximum)
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
