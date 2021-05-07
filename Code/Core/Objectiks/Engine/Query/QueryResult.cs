using Objectiks.Models;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Engine.Query
{
    public class QueryResult
    {
        public List<DocumentKey> Keys { get; set; }
        public IDocumentQueryCompiler Query { get; set; }

        public QueryResult() { }

        public QueryResult(IDocumentQueryCompiler queryCompiler, List<DocumentKey> keys)
        {
            Query = queryCompiler;
            Keys = keys;
        }
    }
}
