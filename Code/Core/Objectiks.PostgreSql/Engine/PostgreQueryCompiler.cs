using Objectiks.Engine;
using Objectiks.Engine.Query;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.PostgreSql.Engine
{
    public class PostgreQueryCompiler : IDocumentQueryCompiler
    {
        public string TypeOf { get; set; }
        public string WhereBy { get; set; }
        public string OrderBy { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public QueryValues ValueBy { get; set; }

        public PostgreQueryCompiler(DocumentQuery queryBuilder)
        {
            ValueBy = new QueryValues();
            Compiler(queryBuilder);
        }

        private void Compiler(DocumentQuery query)
        {

        }
    }
}
