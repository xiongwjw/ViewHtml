using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Attribute4ECAT
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple=true, Inherited=true)]
    public class GrgDataTypeAttribute : DataTypeAttribute,
                                        IValidationGroupName
    {
                // Summary:
        //     Initializes a new instance of the System.ComponentModel.DataAnnotations.DataTypeTypeAttribute
        //     class by using the specified type name.
        //
        // Parameters:
        //   dataType:
        //     The name of the type to associate with the data field.
        public GrgDataTypeAttribute(DataType dataType) : base(dataType)
        {

        }
        //
        // Summary:
        //     Initializes a new instance of the System.ComponentModel.DataAnnotations.DataTypeTypeAttribute
        //     class by using the specified field template name.
        //
        // Parameters:
        //   customDataType:
        //     The name of the custom field template to associate with the data field.
        //
        // Exceptions:
        //   System.ArgumentException:
        //     customDataType is null or an empty string ("").
        public GrgDataTypeAttribute(string customDataType)
            : base(customDataType)
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

    [AttributeUsage(AttributeTargets.Property, AllowMultiple=true, Inherited=true)]
    public class GrgEnumDataTypeAttribute : GrgDataTypeAttribute
    {
                        // Summary:
        //     Initializes a new instance of the System.ComponentModel.DataAnnotations.DataTypeTypeAttribute
        //     class by using the specified type name.
        //
        // Parameters:
        //   dataType:
        //     The name of the type to associate with the data field.
        public GrgEnumDataTypeAttribute(DataType dataType) : base(dataType)
        {

        }
        //
        // Summary:
        //     Initializes a new instance of the System.ComponentModel.DataAnnotations.DataTypeTypeAttribute
        //     class by using the specified field template name.
        //
        // Parameters:
        //   customDataType:
        //     The name of the custom field template to associate with the data field.
        //
        // Exceptions:
        //   System.ArgumentException:
        //     customDataType is null or an empty string ("").
        public GrgEnumDataTypeAttribute(string customDataType)
            : base(customDataType)
        {

        }
    }
}
