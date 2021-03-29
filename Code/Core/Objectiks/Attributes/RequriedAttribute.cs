using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RequriedAttribute : Attribute
    {
        internal string Name { get; set; }
        internal bool HasValue { get; set; }
    }
}
