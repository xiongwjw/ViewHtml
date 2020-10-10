using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Attribute4ECAT
{
    [AttributeUsage( AttributeTargets.Property, AllowMultiple=true, Inherited=true )]
    public class GrgRequiredAttribute : RequiredAttribute,
                                        IValidationGroupName
    {
        public string GroupName
        {
            get;
            set;
        }
    }

    public interface IValidationGroupName
    {
        string GroupName
        {
            get;
            set;
        }
    }
}
