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
        DocumentOption Option { get; }
        IDocumentLogger Logger { get; }
        IDocumentCache Cache { get; }
        DocumentProvider Provider { get; }


        bool LoadDocumentType(string typeOf);

        Document Read(QueryOf query, DocumentMeta meta = null);
        Document Read(string typeOf, object primaryOf);
        JArray ReadList(QueryOf query, DocumentMeta meta = null);

        List<T> ReadList<T>(QueryOf query);
        T Read<T>(QueryOf query, DocumentMeta meta = null);
        T GetCount<T>(QueryOf query, DocumentMeta meta = null);

        List<DocumentMeta> GetTypeMetaAll();
        DocumentMeta GetTypeMeta(string typeOf);

        void SubmitChanges(DocumentMeta meta, DocumentInfo info, List<Document> docs, OperationType operation, Format format);

        void BulkCreate(DocumentMeta meta, DocumentInfo info, List<Document> docs, Format format = Format.None);
        void BulkAppend(DocumentMeta meta, DocumentInfo info, List<Document> docs, Format format = Format.None);
        void BulkMerge(DocumentMeta meta, DocumentInfo info, List<Document> docs, Format format = Format.None);
        void BulkDelete(DocumentMeta meta, DocumentInfo info, List<Document> docs, Format format = Format.None);
    }
}
