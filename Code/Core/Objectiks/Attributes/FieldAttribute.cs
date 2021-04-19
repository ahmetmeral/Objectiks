using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class FieldAttribute : Attribute
    {
        public string Name { get; set; }

        public FieldAttribute() { }

        public FieldAttribute(string name)
        {
            Name = name;
        }
    }
}
