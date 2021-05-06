using Objectiks.Engine;
using Objectiks.Engine.Query;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Objectiks.Services
{
    public interface IDocumentReader<T>
    {
        DocumentQuery GetDocumentQueryOf();
        IDocumentReader<T> TypeOf(string typeOf);
        IDocumentReader<T> TypeOf(string typeOf, object primaryOf);
        IDocumentReader<T> PrimaryOf(object primaryOf);
        IDocumentReader<T> WorkOf(object workOf);
        IDocumentReader<T> UserOf(object userOf);
        IDocumentReader<T> KeyOf(object keyOf);
        IDocumentReader<T> CacheOf(QueryCacheOf cacheOf);
        IDocumentReader<T> CacheOf(int expireMinute = 60);
        IDocumentReader<T> CacheOf(string cacheOf, int expireMinute = 60);
        IDocumentReader<T> CacheOf(string cacheOf, bool beforeCallRemove, int expireMinute = 60);
        IDocumentReader<T> CacheOf(bool beforeCallRemove, int expireMinute = 60);
        IDocumentReader<T> Any();

        IDocumentReader<T> Refs(object refObjectOrClass);

        IDocumentReader<T> OrderBy(string property);
        IDocumentReader<T> Desc();
        IDocumentReader<T> Asc();
        IDocumentReader<T> OrderBy(Expression<Func<T, object>> expr);
        IDocumentReader<T> OrderByDesc(Expression<Func<T, object>> expr);

        IDocumentReader<T> Skip(int skip);
        IDocumentReader<T> Take(int take);

        long Count();
        T First();
        List<T> ToList();

        int Delete();
    }
}