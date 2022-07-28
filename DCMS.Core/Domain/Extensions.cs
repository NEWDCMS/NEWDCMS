using System;
using System.Collections.Generic;
using System.Text;

namespace DCMS.Core
{

    [AttributeUsage(AttributeTargets.Property)]
    public class TagAttribute : Attribute
    {
        public TagAttribute(int month)
        {
            Month = month;
        }
        public int Month { get; set; }

    }

    public static class MExtensions
    {
        public static object Parse(this object self, string propertyName)
        {
            if (self == null)
            {
                return self;
            }
            Type t = self.GetType();
            var p = t.GetProperty(propertyName);
            var attrs = p?.GetCustomAttributes(typeof(TagAttribute), true);
            if (attrs?.Length > 0)
            {
                var attr = attrs[0] as TagAttribute;
                return attr?.Month;
            }
            return 0;
        }
    }
}
