using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public class TypeOfAttribute : Attribute
    {
        public string Name { get; set; }

        public TypeOfAttribute()
        {
        }

        public TypeOfAttribute(string name)
        {
            Name = name;
        }
    }
}
