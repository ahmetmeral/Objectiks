using Objectiks.Engine.Query;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Objectiks.Helper;

namespace Objectiks.Engine.Query
{
    public class QueryCompiler
    {
        public string TypeOf { get; set; }
        public string WhereBy { get; set; }
        public string OrderBy { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public QueryValues ValueBy { get; set; }

        public QueryCompiler(DocumentQuery queryBuilder)
        {
            ValueBy = new QueryValues();
            Compiler(queryBuilder);
        }

        private void Compiler(DocumentQuery queryBuilder)
        {
            TypeOf = queryBuilder.TypeOf;
            Skip = queryBuilder.Skip;
            Take = queryBuilder.Take;

            var builder = new List<string>();
            var types = EnumHelper.GetEnumValues<QueryParameterType>();

            var index = -1;
            foreach (var type in types)
            {
                var statement = new List<string>();
                var parameters = queryBuilder.Parameters.Where(p => p.Type == type).ToList();

                if (parameters == null || parameters.Count == 0)
                {
                    continue;
                }

                foreach (var parameter in parameters)
                {
                    index++;

                    if (parameter.Type == QueryParameterType.WorkOf ||
                        parameter.Type == QueryParameterType.UserOf ||
                        parameter.Type == QueryParameterType.PrimaryOf)
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
                    builder.Add("(" + string.Join(queryBuilder.IsAny ? " OR " : " AND ", statement) + ")");
                }
                else
                {
                    builder.Add(string.Join(queryBuilder.IsAny ? " OR " : " AND ", statement));
                }
            }

            WhereBy = string.Join(" AND ", builder);

            if (queryBuilder.OrderBy.Count > 0)
            {
                var direction = queryBuilder.OrderBy.Direction == OrderByDirection.Asc ? "Asc" : queryBuilder.OrderBy.Direction == OrderByDirection.Desc ? "Desc" : "Asc";
                OrderBy = string.Join(",", queryBuilder.OrderBy) + " " + direction;
            }
        }
    }
}
