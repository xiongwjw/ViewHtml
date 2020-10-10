using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Attribute4ECAT
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class GrgActivityAttribute : GrgComponentAttribute
    {
        public GrgActivityAttribute(string strID)
            : base(strID)
        {

        }

        public string NodeNameOfConfiguration
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public string[] ForwardTargets
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public string[] ExtendParams
        {
            get;
            set;
        }

        /// <summary>
        /// States to map a state of action to a screen.
        /// </summary>
        public string[] UIStates
        {
            get;
            set;
        }

        [DefaultValue(false)]
        public bool Published
        {
            get;
            set;
        }

		[DefaultValue(null)]
		public string Path4UI
		{
			get;
			set;
		}
    }
}
