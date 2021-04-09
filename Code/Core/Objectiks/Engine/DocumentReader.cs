using Objectiks.Engine.Query;
using Objectiks.Models;
using Objectiks.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Objectiks.Engine
{
    public class DocumentReader<T> : IDocumentReader<T>
    {
        private readonly DocumentProvider Provider = null;
        private readonly QueryOf Query = null;

        public DocumentReader(DocumentProvider provider)
        {
            Provider = provider;
            Query = new QueryOf();
        }

        public DocumentReader(string typeOf)
        {
            Provider = ObjectiksOf.Core.GetTypeOfProvider(typeOf);
            Query = new QueryOf(typeOf);
        }

        public DocumentReader(string typeOf, params object[] primaryOf)
        {
            Provider = ObjectiksOf.Core.GetTypeOfProvider(typeOf);
            Query = new QueryOf(typeOf, primaryOf);
        }

        #region TypeOfBy
        public IDocumentReader<T> TypeOf(string typeOf)
        {
            Query.TypeOfBy(typeOf);

            return this;
        }

        public IDocumentReader<T> TypeOf(string typeOf, object primaryOf)
        {
            Query.TypeOfBy(typeOf);
            Query.PrimaryOf(primaryOf);

            return this;
        }

        public IDocumentReader<T> PrimaryOf(object primaryOf)
        {
            Query.PrimaryOf(primaryOf.ToString());

            return this;
        }

        public IDocumentReader<T> KeyOf(object keyOf)
        {
            Query.KeyOf(keyOf.ToString());

            return this;
        }

        public IDocumentReader<T> Any()
        {
            Query.Any();

            return this;
        }

        public IDocumentReader<T> Lazy(bool isLoadLazyRefs)
        {
            Query.Lazy = isLoadLazyRefs;

            return this;
        }

        public IDocumentReader<T> Refs(object refObjectOrClass)
        {
            Query.Lazy = true;

            if (refObjectOrClass is DocumentRef)
            {
                Query.RefList.Add((DocumentRef)refObjectOrClass);
            }
            else
            {
                Query.RefList.Add(DocumentRef.FromObject(refObjectOrClass));
            }

            return this;
        }
        #endregion


        #region Where & Query By
        public IDocumentReader<T> OrderBy(string property)
        {
            Query.OrderBy(property);

            return this;
        }

        public IDocumentReader<T> Desc()
        {
            Query.Direction = OrderDirection.Desc;

            return this;
        }

        public IDocumentReader<T> Asc()
        {
            Query.Direction = OrderDirection.Asc;

            return this;
        }

        public IDocumentReader<T> OrderBy(Expression<Func<T, object>> expr)
        {
            Query.ParsePredicateExprOrderBy(expr.Body);
            Query.Direction = OrderDirection.Asc;
            return this;
        }

        public IDocumentReader<T> OrderByDesc(Expression<Func<T, object>> expr)
        {
            Query.ParsePredicateExprOrderBy(expr.Body);
            Query.Direction = OrderDirection.Desc;
            return this;
        }

        public IDocumentReader<T> Skip(int skip)
        {
            Query.Skip = skip;

            return this;
        }

        public IDocumentReader<T> Take(int take)
        {
            Query.Take = take;

            return this;
        }
        #endregion

        #region Result

        public long Count()
        {
            return Provider.GetCount<long>(Query);
        }

        public List<T> ToList()
        {
            return Provider.ReadList<T>(Query);
        }

        public T First()
        {
            Ensure.Try(Query.PrimaryOfList.Count > 0 && Query.KeyOfList.Count > 0, "PrimarOf and KeyOf cannot be used together");
            Ensure.Try(Query.PrimaryOfList.Count > 1, "Use ToList() for multiple results");

            return Provider.Read<T>(Query);
        }

        #endregion

        #region Utils
        public QueryOf GetDocumentQueryOf()
        {
            return Query;
        }
        #endregion
    }
}
