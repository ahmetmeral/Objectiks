using Objectiks.Engine;
using Objectiks.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Services
{
    public interface IDocumentCache
    {
        void Set(Document document, int expire);
        void Set(DocumentMeta meta, int expire);
        void Set(DocumentSequence sequence);
       
        Document GetOrCreateDocument(string typeOf, object primaryOf, Func<Document> func);
        DocumentMeta GetOrCreateMeta(string typeOf, Func<DocumentMeta> func);
        DocumentSequence GetOrCreateSequence(string typeOf, Func<DocumentSequence> func);
       
        Document Get(string typeOf, object primaryOf);
        DocumentMeta Get(string typeOf);
        DocumentSequence GetSequence(string typeOf);

        void Set(DocumentInfo documentInfo);
        DocumentInfo GetDocumentInfo(string typeOf, object primaryOf);

        void Set<T>(DocumentQuery query, T data);
        T Get<T>(DocumentQuery query);

        void Remove(string typeOf, object primaryOf);
        void Remove(string typeOf);
        void Remove(Document document);
        void Remove(DocumentMeta meta);
        void Remove(DocumentQuery query);

        string CacheOf(DocumentQuery query);
        string CacheOf(Document document);
        string CacheOfDocument(string typeOf, object primaryOf);
        string CacheOf(DocumentMeta meta);
        string CacheOfMeta(string typeOf);
        string CacheOf(DocumentSequence sequence);
        string CacheOfSequence(string typeOf);
        string CacheOf(DocumentInfo info);
        string CacheOfDocumentInfo(string typeOf, object primary);

        void Flush();
    }
}
