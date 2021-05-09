using Objectiks.Engine.Query;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Objectiks.Helper;
using Objectiks.Engine;
using Objectiks.Services;

namespace Objectiks.PostgreSql.Engine
{
    public class PostgreQueryCompiler : IDocumentQueryCompiler
    {
        private readonly DocumentOption Option;
        private readonly DocumentQuery Query;

        public string TypeOf { get; set; }
        public string SelectBy { get; set; }
        public string FromBy { get; set; }
        public string WhereBy { get; set; }
        public string OrderBy { get; set; }
        public string InsertBy { get; set; }
        public string UpdateBy { get; set; }
        public string DeleteBy { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }

        public string CommandText { get; set; }
        public QueryValues ValueBy { get; set; }


        public PostgreQueryCompiler(DocumentOption option, DocumentQuery query)
        {
            ValueBy = new QueryValues();
            Option = option;
            Query = query;
            TypeOf = query.TypeOf;
            Take = query.Take;
            Skip = query.Skip;
        }

        public string Select()
        {
            var parameterIndex = -1;
            SelectBy = "*";
            FromBy = GetFromBy();

            #region WhereBy

            var whereBy = new List<string>();

            var types = new ParameterType[] {
                ParameterType.WorkOf,
                ParameterType.PrimaryOf,
                ParameterType.UserOf,
                ParameterType.KeyOf,
                ParameterType.Where
            };

            foreach (var type in types)
            {
                var builder = new List<string>();

                var parameters = Query.Parameters.Where(p => p.Type == type).ToList();

                if (parameters == null || parameters.Count == 0)
                {
                    continue;
                }

                foreach (var parameter in parameters)
                {
                    parameterIndex++;

                    if (parameter.Type == ParameterType.KeyOf)
                    {
                        builder.Add($"{parameter.Field} Like %@{parameterIndex}%");
                    }
                    else
                    {
                        builder.Add($"{parameter.Field}=@{parameterIndex}");
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
                    whereBy.Add("(" + string.Join(Query.IsAny ? " OR " : " AND ", builder) + ")");
                }
                else
                {
                    whereBy.Add(string.Join(Query.IsAny ? " OR " : " AND ", builder));
                }
            }
            #endregion

            WhereBy = string.Join(" AND ", whereBy);

            if (Query.OrderBy.Count > 0)
            {
                var direction = Query.OrderBy.Direction == OrderByDirection.Asc ? "Asc" : Query.OrderBy.Direction == OrderByDirection.Desc ? "Desc" : "Asc";
                OrderBy = string.Join(",", Query.OrderBy) + " " + direction;
            }

            var queryBy = new QueryBy();
            queryBy.Add($"SELECT {SelectBy}");
            queryBy.Add($"FROM {FromBy}");

            if (Query.HasParameters)
            {
                queryBy.Add($"WHERE {whereBy}");
            }

            if (Query.HasOrderBy)
            {
                queryBy.Add($"ORDER BY {OrderBy}");
            }

            if (Query.HasPager)
            {
                queryBy.Add($"LIMIT {Take} OFFSET {Skip}");
            }

            return string.Join(" ", queryBy);
        }

        private string GetFromBy()
        {
            var fromBy = TypeOf;

            if (String.IsNullOrEmpty(Option.SqlProviderSchema))
            {
                fromBy = $"{Option.SqlProviderSchema}{Option.SqlProviderSchemaSeperator}{TypeOf}";
            }

            return fromBy;
        }
    }
}
