using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public class ParseOfAttribute : Attribute
    {
        public string Name { get; set; }

        public ParseOfAttribute() { }

        public ParseOfAttribute(string parseOf)
        {
            Name = parseOf;
        }
    }
}
