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

        bool LoadDocumentType(string typeOf, bool isInitialize);

        Document Read(DocumentQuery query, DocumentMeta meta = null);
        Document Read(string typeOf, object primaryOf);
       
        T Read<T>(DocumentQuery query, DocumentMeta meta = null);
        T GetCount<T>(DocumentQuery query, DocumentMeta meta = null);
        JArray ReadList(DocumentQuery query, DocumentMeta meta = null);
        List<T> ReadList<T>(DocumentQuery query);

        T GetCountFromQueryOf<T>(DocumentQuery query, DocumentMeta meta = null);
        QueryResult GetDocumentKeysFromQueryOf(DocumentQuery query, DocumentMeta meta = null);

        List<DocumentMeta> GetTypeMetaAll();
        DocumentMeta GetTypeMeta(string typeOf);

        void SubmitChanges(DocumentContext context, DocumentTransaction transaction);

        void BulkCreate(DocumentContext context, DocumentTransaction transaction);
        void BulkAppend(DocumentContext context, DocumentTransaction transaction);
        void BulkMerge(DocumentContext context, DocumentTransaction transaction);
        void BulkDelete(DocumentContext context, DocumentTransaction transaction);

        void TruncateTypeOf(string typeOf);
        void TruncateTypeOf(DocumentMeta meta);
        int Delete<T>(DocumentQuery query);
    }
}
