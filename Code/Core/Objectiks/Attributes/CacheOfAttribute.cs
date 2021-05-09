using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public class CacheOfAttribute : Attribute
    {
        public int Expire { get; set; }
        public bool Lazy { get; set; }

        public CacheOfAttribute() { }

        public CacheOfAttribute(int expire, bool lazy = false)
        {
            Expire = expire;
            Lazy = lazy;
        }
    }
}
