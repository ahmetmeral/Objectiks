using Objectiks.Engine;
using Objectiks.Engine.Query;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Extentions
{
    public static class QueryExtentions
    {
        public static QueryCompiler Compiler(this DocumentQuery builder)
        {
            return new QueryCompiler(builder);
        }
    }
}
