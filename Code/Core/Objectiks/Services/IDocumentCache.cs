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
        Document GetOrCreate(string typeOf, object primaryOf, Func<Document> func);
        DocumentMeta GetOrCreate(string typeOf, Func<DocumentMeta> func);
        Document Get(string typeOf, object primaryOf);
        DocumentMeta Get(string typeOf);
        DocumentTypeStatus GetStatus(string typeOf);
        void SetStatus(DocumentTypeStatus status);

        void Remove(string typeOf, object primaryOf);
        void Remove(string typeOf);
        void Remove(Document document);
        void Remove(DocumentMeta meta);
        void Reset();
        string CacheOf(Document doc);
        string CacheOfDocument(string typeOf, object primaryOf);
        string CacheOf(DocumentMeta meta);
        string CacheOfMeta(string typeOf);
    }
}
