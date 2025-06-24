using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageViewer.Infrastructure.Attributes
{
    public class DisplayNameAttribute : Attribute
    {
        public static readonly DisplayNameAttribute Default = new DisplayNameAttribute();

        private string _displayName;

        public virtual string DisplayName => DisplayNameValue;

        protected string DisplayNameValue
        {
            get
            {
                return _displayName;
            }
            set
            {
                _displayName = value;
            }
        }

        public DisplayNameAttribute()
            : this(string.Empty)
        {
        }

        public DisplayNameAttribute(string displayName)
        {
            _displayName = displayName;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            if (obj is DisplayNameAttribute displayNameAttribute)
            {
                return displayNameAttribute.DisplayName == DisplayName;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return DisplayName.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return Equals(Default);
        }
    }
}
