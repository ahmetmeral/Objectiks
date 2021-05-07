using Objectiks.Engine;
using Objectiks.NoDb.Engine;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.NoDb.Extentions
{
    public static class NoDbExtentions
    {
        public static IDocumentQueryCompiler Compiler(this DocumentQuery builder)
        {
            return new NoDbQueryCompiler(builder);
        }
    }
}
