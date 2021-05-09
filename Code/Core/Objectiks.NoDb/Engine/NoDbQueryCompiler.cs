using Objectiks.Engine.Query;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Objectiks.Helper;
using Objectiks.Engine;
using Objectiks.Services;

namespace Objectiks.NoDb.Engine
{
    public class NoDbQueryCompiler : IDocumentQueryCompiler
    {
        public string TypeOf { get; set; }
        public string WhereBy { get; set; }
        public string OrderBy { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public QueryValues ValueBy { get; set; }

        public NoDbQueryCompiler(DocumentQuery queryBuilder)
        {
            ValueBy = new QueryValues();
            Compiler(queryBuilder);
        }

        private void Compiler(DocumentQuery query)
        {
            TypeOf = query.TypeOf;
            Skip = query.Skip;
            Take = query.Take;

            var builder = new List<string>();
            var types = EnumHelper.GetEnumValues<ParameterType>();

            var index = -1;
            foreach (var type in types)
            {
                var statement = new List<string>();
                var parameters = query.Parameters.Where(p => p.Type == type).ToList();

                if (parameters == null || parameters.Count == 0)
                {
                    continue;
                }

                foreach (var parameter in parameters)
                {
                    index++;

                    if (parameter.Type == ParameterType.WorkOf ||
                        parameter.Type == ParameterType.UserOf ||
                        parameter.Type == ParameterType.PrimaryOf)
                    {
                        statement.Add($"{parameter.Field}=@{index}");
                    }
                    else
                    {
                        statement.Add($"{parameter.Field}.Contains(@{index})");
                    }

                    if (parameter.Value == null)
                    {
                        ValueBy.Add(string.Empty);
                    }
                    else
                    {
                        ValueBy.Add(parameter.Value.ToString().ToLowerInvariant());
                    }
                }

                if (parameters.Count > 1)
                {
                    builder.Add("(" + string.Join(query.IsAny ? " OR " : " AND ", statement) + ")");
                }
                else
                {
                    builder.Add(string.Join(query.IsAny ? " OR " : " AND ", statement));
                }
            }

            WhereBy = string.Join(" AND ", builder);

            if (query.OrderBy.Count > 0)
            {
                var direction = query.OrderBy.Direction == OrderByDirection.Asc ? "Asc" : query.OrderBy.Direction == OrderByDirection.Desc ? "Desc" : "Asc";
                OrderBy = string.Join(",", query.OrderBy) + " " + direction;
            }
        }

        //public string AsWhereStatement()
        //{

        //}

        //public string AsSelectStatement()
        //{

        //}
    }
}
