using Objectiks.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Engine.Query
{
    public class QueryResult
    {
        public List<DocumentKey> Keys { get; set; }
        public QueryCompiler Query { get; set; }

        public QueryResult() { }

        public QueryResult(QueryCompiler queryCompiler, List<DocumentKey> keys)
        {
            Query = queryCompiler;
            Keys = keys;
        }
    }
}
