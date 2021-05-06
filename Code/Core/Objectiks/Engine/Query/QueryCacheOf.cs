using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Engine.Query
{
    public class QueryCacheOf
    {
        public string Key { get; set; }
        public int Expire { get; set; }
        public bool BeforeCallClear { get; set; }
    }
}
