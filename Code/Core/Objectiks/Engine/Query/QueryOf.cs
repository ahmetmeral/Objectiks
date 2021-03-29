using Objectiks.Helper;
using Objectiks.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Objectiks.Engine.Query
{
    public class QueryOfCompiler : IDisposable
    {
        public string TypeOf { get; set; }
        public string Query { get; set; }
        public string OrderBy { get; set; }
        public string[] Parameters { get; set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

    public class QueryOf
    {
        public string TypeOf { get; internal set; }
        public PrimaryOfValues PrimaryOfList { get; internal set; }
        public KeyOfValues KeyOfList { get; internal set; }
        public DocumentRefs RefList { get; internal set; }
        public int Skip { get; internal set; }
        public int Take { get; internal set; }
        public bool Lazy { get; internal set; } = true;
        public bool IsAny { get; internal set; } = false;
        public OrderDirection Direction { get; internal set; } = OrderDirection.None;

        private WhereBy WhereByAndList = null;
        private ValueBy ValueByList = null;
        private OrderBy OrderByList = null;

        private int ValueByIndex = 0;

        public bool HasRefs
        {
            get { return RefList.Count > 0; }
        }

        public bool HasKeyOf
        {
            get { return KeyOfList.Count > 0; }
        }

        public bool HasOrderBy
        {
            get { return OrderByList.Count > 0; }
        }

        public bool HasPrimaryOf
        {
            get { return PrimaryOfList.Count > 0; }
        }

        public QueryOf()
        {
            Initialize(null);
        }

        public QueryOf(string typeOf)
        {
            Initialize(typeOf);
        }

        public QueryOf(string typeOf, params object[] primaryOf)
        {
            Initialize(typeOf, primaryOf);
        }

        private void Initialize(string typeOf, params object[] primaryOf)
        {
            PrimaryOfList = new PrimaryOfValues();
            KeyOfList = new KeyOfValues();
            RefList = new DocumentRefs();
            WhereByAndList = new WhereBy();
            ValueByList = new ValueBy();
            OrderByList = new OrderBy();

            TypeOfBy(typeOf);
            PrimaryOf(primaryOf);
        }

        public void TypeOfBy(string typeOf)
        {
            TypeOf = typeOf;
        }

        public void PrimaryOf(List<string> primaryOf)
        {
            PrimaryOf(primaryOf.ToArray());
        }

        public void PrimaryOf(params object[] primaryOf)
        {
            foreach (var key in primaryOf)
            {
                if (key == null)
                    continue;

                if (!PrimaryOfList.Contains(key.ToString()))
                {
                    PrimaryOfList.Add(key.ToString());
                    ContainsBy(DocumentDefaults.DocumentMetaPrimaryOfProperty, key);
                }
            }
        }

        public void KeyOf(List<object> keyOf)
        {
            KeyOf(keyOf.ToArray());
        }

        public void KeyOf(params object[] keyOf)
        {
            foreach (var key in keyOf)
            {
                if (key == null)
                    continue;

                if (!KeyOfList.Contains(key.ToString()))
                {
                    KeyOfList.Add(key.ToString());
                    ContainsBy(DocumentDefaults.DocumentMetaKeyOfProperty, key);
                }
            }
        }

        public int ValueOf(object value)
        {
            var index = ValueByIndex;
            ValueByList.Add(value.ToString().ToLowerInvariant());
            this.ValueByIndex++;
            return index;
        }

        public void WhereBy(string property, object value)
        {
            this.WhereByAndList.Add($"{property}=@{ValueByIndex}");
            this.ValueByList.Add(value.ToString());
            this.ValueByIndex++;
        }

        public void ContainsBy(string property, object value)
        {
            //https://dynamic-linq.net/knowledge-base/4599989/how-to-use-dynamic-linq--system-linq-dynamic--for-like-operation-
            this.WhereByAndList.Add($"{property}.Contains(@{ValueByIndex})");
            this.ValueByList.Add(value.ToString().ToLowerInvariant());
            this.ValueByIndex++;
        }

        public void KeyOfStatement(string statement)
        {
            if (this.KeyOfList.Count == 0)
            {
                this.KeyOfList.Add("ByPassHasKeyOf");
            }

            this.WhereByAndList.Add(statement);
        }

        public void Any()
        {
            IsAny = true;
        }

        public void OrderBy(string property)
        {
            //https://dotnetfiddle.net/bs34gh
            this.OrderByList.Add(property);
        }

        public void OrderByTypeOf(OrderDirection orderByType)
        {
            Direction = orderByType;
        }

        public string AsWhere()
        {
            return String.Join(IsAny ? " OR " : " AND ", WhereByAndList);
        }

        public string[] AsWhereParameters()
        {
            return ValueByList.ToArray();
        }

        public string AsOrderBy()
        {
            string orderBy = string.Empty;

            if (Direction != OrderDirection.None)
            {
                orderBy = " " + Direction.ToString().ToLowerInvariant();
            }

            return string.Join(",", OrderByList) + orderBy;
        }

        public QueryOfCompiler AsCompiler()
        {
            var compiler = new QueryOfCompiler
            {
                TypeOf = this.TypeOf,
                Query = AsWhere(),
                OrderBy = AsOrderBy(),
                Parameters = AsWhereParameters(),
            };

            return compiler;
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

            OrderBy(memberName);
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

            ContainsBy(memberName, value);
        }
    }
}
