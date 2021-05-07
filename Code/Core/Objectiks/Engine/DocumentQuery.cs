using Objectiks.Engine.Query;
using Objectiks.Helper;
using Objectiks.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Objectiks.Engine
{
    public class DocumentQuery
    {
        public string TypeOf { get; internal set; }
        public QueryCacheOf CacheOf { get; internal set; }
        public QueryParameters Parameters { get; internal set; }
        public QueryOrderBy OrderBy { get; internal set; }
        public DocumentRefs Refs { get; internal set; }
        public ResultType ResultType { get; internal set; }
        public int Skip { get; internal set; }
        public int Take { get; internal set; }
        public bool HasWorkOf { get; internal set; }
        public bool HasUserOf { get; internal set; }
        public bool HasPrimaryOf { get; internal set; }
        public bool HasKeyOf { get; internal set; }
        public bool HasOrderBy { get; internal set; }
        public bool HasRefs { get; internal set; }
        public bool HasCacheOf { get; internal set; }
        public bool IsAny { get; internal set; } = false;

        public bool HasParameters
        {
            get
            {
                return Parameters.Count > 0;
            }
        }

        public DocumentQuery()
        {
            Initialize(null);
        }

        public DocumentQuery(string typeOf)
        {
            Initialize(typeOf);
        }

        public DocumentQuery(string typeOf, params object[] primaryOf)
        {
            Initialize(typeOf, primaryOf);
        }

        private void Initialize(string typeOf, params object[] primaryOf)
        {
            TypeOf = typeOf;
            CacheOf = new QueryCacheOf();
            Parameters = new QueryParameters();
            OrderBy = new QueryOrderBy();
            Refs = new DocumentRefs();
            HasPrimaryOf = primaryOf.Length > 0;
            OrderBy.Direction = OrderByDirection.Asc;

            foreach (var primaryOfValue in primaryOf)
            {
                AddParameter(new QueryParameter
                {
                    Type = QueryParameterType.PrimaryOf,
                    Field = DocumentDefaults.DocumentMetaPrimaryOfProperty,
                    Value = primaryOfValue
                });
            }
        }

        public void AddParameter(QueryParameter parameter)
        {
            Parameters.Add(parameter);
        }

        public void Any()
        {
            IsAny = true;
        }

        public void AddOrderBy(string property)
        {
            //https://dotnetfiddle.net/bs34gh
            this.OrderBy.Add(property);
        }

        public void OrderByTypeOf(OrderByDirection orderByType)
        {
            this.OrderBy.Direction = orderByType;
        }

        internal void ParsePredicateExprOrderBy(Expression expr)
        {
            string memberName = string.Empty;

            switch (expr)
            {
                case MemberExpression m:
                    memberName = m.Member.Name;
                    break;
                case UnaryExpression u when u.Operand is MemberExpression m:
                    memberName = m.Member.Name;
                    break;
                default:
                    throw new NotImplementedException(expr.GetType().ToString());
            }

            AddOrderBy(memberName);
        }

        internal void ParsePredicateExprSearchBy(Expression expr, object value)
        {
            string memberName = string.Empty;

            switch (expr)
            {
                case MemberExpression m:
                    memberName = m.Member.Name;
                    break;
                case UnaryExpression u when u.Operand is MemberExpression m:
                    memberName = m.Member.Name;
                    break;
                default:
                    throw new NotImplementedException(expr.GetType().ToString());
            }

            //ContainsBy(memberName, value);
        }

        public string GetCacheOfKey()
        {
            if (String.IsNullOrEmpty(CacheOf.Key))
            {
                var keys = new List<string>();

                foreach (var item in Parameters)
                {
                    keys.Add($"{item.Field}:{item.Value}");
                }

                foreach (var item in OrderBy)
                {
                    keys.Add(item);
                }

                keys.Add(OrderBy.Direction.ToString());

                return HashHelper.CreateMD5(string.Join(":", keys));
            }

            return CacheOf.Key;
        }
    }
}
