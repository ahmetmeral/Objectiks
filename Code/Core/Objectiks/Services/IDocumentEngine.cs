using Newtonsoft.Json.Linq;
using Objectiks.Engine;
using Objectiks.Engine.Query;
using Objectiks.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objectiks.Services
{
    public interface IDocumentEngine
    {
        IDocumentCache Cache { get; }
        IDocumentConnection Connection { get; }
        DocumentManifest Manifest { get; }

        bool LoadDocumentType(string typeOf);

        Document Read(QueryOf query, DocumentMeta meta = null);
        Document Read(string typeOf, object primaryOf);
        JArray ReadList(QueryOf query, DocumentMeta meta = null);

        List<T> ReadList<T>(QueryOf query);
        T Read<T>(QueryOf query, DocumentMeta meta = null);
        T GetCount<T>(QueryOf query, DocumentMeta meta = null);

        List<DocumentMeta> GetTypeMetaAll();
        DocumentMeta GetTypeMeta(string typeOf);

        void Write(DocumentMeta meta, DocumentInfo info, List<Document> docs, OperationType operation, Format format);
    }
}
