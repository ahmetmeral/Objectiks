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
        private readonly DocumentQuery Query = null;


        public DocumentReader(DocumentEngine engine)
        {
            Engine = engine;
            Query = new DocumentQuery();
        }

        public DocumentReader(DocumentEngine engine, string typeOf)
        {
            Engine = engine;
            Query = new DocumentQuery(typeOf);
        }

        public DocumentReader(DocumentEngine engine, string typeOf, params object[] primaryOf)
        {
            Engine = engine;
            Query = new DocumentQuery(typeOf, primaryOf);
        }

        #region TypeOfBy
        public IDocumentReader<T> TypeOf(string typeOf)
        {
            Query.TypeOf = typeOf;

            return this;
        }

        public IDocumentReader<T> TypeOf(string typeOf, object primaryOf)
        {
            Query.TypeOf = typeOf;
            Query.AddParameter(new QueryParameter
            {
                Field = DocumentDefaults.DocumentMetaPrimaryOfProperty,
                Value = primaryOf,
                Type = QueryParameterType.PrimaryOf
            });

            return this;
        }

        public IDocumentReader<T> CacheOf(int expireMinute = 60)
        {
            Query.HasCacheOf = true;
            Query.CacheOf = new QueryCacheOf
            {
                Key = string.Empty,
                BeforeCallClear = false,
                Expire = expireMinute
            };

            return this;
        }

        public IDocumentReader<T> CacheOf(string cacheOf, int expireMinute = 60)
        {
            Query.HasCacheOf = true;
            Query.CacheOf = new QueryCacheOf
            {
                Key = cacheOf,
                BeforeCallClear = false,
                Expire = expireMinute
            };

            return this;
        }

        public IDocumentReader<T> CacheOf(string cacheOf, bool beforeCallRemove, int expireMinute = 60)
        {
            Query.HasCacheOf = true;
            Query.CacheOf = new QueryCacheOf
            {
                Key = cacheOf,
                BeforeCallClear = beforeCallRemove,
                Expire = expireMinute
            };

            return this;
        }

        public IDocumentReader<T> CacheOf(bool beforeCallRemove, int expireMinute = 60)
        {
            Query.HasCacheOf = true;
            Query.CacheOf = new QueryCacheOf
            {
                Key = string.Empty,
                BeforeCallClear = beforeCallRemove,
                Expire = expireMinute
            };

            return this;
        }

        public IDocumentReader<T> CacheOf(QueryCacheOf cacheOf)
        {
            Query.HasCacheOf = true;
            Query.CacheOf = cacheOf;

            return this;
        }

        public IDocumentReader<T> PrimaryOf(object primaryOf)
        {
            Query.HasPrimaryOf = true;
            Query.AddParameter(new QueryParameter
            {
                Type = QueryParameterType.PrimaryOf,
                Field = DocumentDefaults.DocumentMetaPrimaryOfProperty,
                Value = primaryOf
            });
            return this;
        }

        public IDocumentReader<T> WorkOf(object workOf)
        {
            Query.HasWorkOf = true;
            Query.AddParameter(new QueryParameter
            {
                Type = QueryParameterType.WorkOf,
                Field = DocumentDefaults.DocumentMetaWorkOfProperty,
                Value = workOf
            });

            return this;
        }

        public IDocumentReader<T> UserOf(object userOf)
        {
            Query.HasUserOf = true;
            Query.AddParameter(new QueryParameter
            {
                Type = QueryParameterType.UserOf,
                Field = DocumentDefaults.DocumentMetaUserOfProperty,
                Value = userOf
            });

            return this;
        }

        public IDocumentReader<T> KeyOf(object keyOf)
        {
            Query.HasKeyOf = true;
            Query.AddParameter(new QueryParameter
            {
                Type = QueryParameterType.KeyOf,
                Field = DocumentDefaults.DocumentMetaKeyOfProperty,
                Value = keyOf
            });

            return this;
        }

        public IDocumentReader<T> Any()
        {
            Query.Any();

            return this;
        }

        #endregion


        #region Where & Query By
        public IDocumentReader<T> OrderBy(string property)
        {
            Query.AddOrderBy(property);

            return this;
        }

        public IDocumentReader<T> Desc()
        {
            Query.OrderBy.Direction = OrderByDirection.Desc;

            return this;
        }

        public IDocumentReader<T> Asc()
        {
            Query.OrderBy.Direction = OrderByDirection.Asc;

            return this;
        }

        public IDocumentReader<T> OrderBy(Expression<Func<T, object>> expr)
        {
            Query.ParsePredicateExprOrderBy(expr.Body);
            Query.OrderBy.Direction = OrderByDirection.Asc;
            return this;
        }

        public IDocumentReader<T> OrderByDesc(Expression<Func<T, object>> expr)
        {
            Query.ParsePredicateExprOrderBy(expr.Body);
            Query.OrderBy.Direction = OrderByDirection.Desc;
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

            //Ensure.Try(Query.PrimaryOfList.Count > 0 && Query.KeyOfList.Count > 0, "PrimarOf and KeyOf cannot be used together");
            //Ensure.Try(Query.PrimaryOfList.Count > 1, "Use ToList() for multiple results");

            return Engine.Read<T>(Query);
        }

        public int Delete()
        {
            return Engine.Delete<T>(Query);
        }

        #endregion

        #region Utils
        public DocumentQuery GetDocumentQueryOf()
        {
            return Query;
        }


        #endregion
    }
}
