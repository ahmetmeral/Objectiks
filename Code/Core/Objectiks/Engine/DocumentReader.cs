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
        private readonly DocumentEngine Engine = null;
        private readonly QueryOf Query = null;


        public DocumentReader(DocumentEngine engine)
        {
            Engine = engine;
            Query = new QueryOf();
        }

        public DocumentReader(DocumentEngine engine, string typeOf)
        {
            Engine = engine;
            Query = new QueryOf(typeOf);
        }

        public DocumentReader(DocumentEngine engine, string typeOf, params object[] primaryOf)
        {
            Engine = engine;
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

        public IDocumentReader<T> CacheOf(int expireMinute = 1000)
        {
            Query.CacheOf(string.Empty, expireMinute);

            return this;
        }

        public IDocumentReader<T> CacheOf(string cacheOf, int expireMinute = 1000)
        {
            Query.CacheOf(cacheOf, expireMinute);

            return this;
        }

        public IDocumentReader<T> PrimaryOf(object primaryOf)
        {
            Query.PrimaryOf(primaryOf.ToString());

            return this;
        }

        public IDocumentReader<T> WorkOf(object workOf)
        {
            Query.WorkOf(workOf);

            return this;
        }

        public IDocumentReader<T> UserOf(object userOf)
        {
            Query.UserOf(userOf);

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

        public IDocumentReader<T> Refs(object refObjectOrClass)
        {
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
            Query.ResultType = ResultType.Count;

            return Engine.GetCount<long>(Query);
        }

        public List<T> ToList()
        {
            Query.ResultType = ResultType.List;

            return Engine.ReadList<T>(Query);
        }

        public T First()
        {
            Query.ResultType = ResultType.First;

            Ensure.Try(Query.PrimaryOfList.Count > 0 && Query.KeyOfList.Count > 0, "PrimarOf and KeyOf cannot be used together");
            Ensure.Try(Query.PrimaryOfList.Count > 1, "Use ToList() for multiple results");

            return Engine.Read<T>(Query);
        }

        public int Delete()
        {
            return Engine.Delete<T>(Query);
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
