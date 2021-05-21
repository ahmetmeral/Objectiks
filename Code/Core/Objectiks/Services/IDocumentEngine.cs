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
        IDocumentLogger Logger { get; }
        DocumentOption Option { get; }
        List<IDocumentParser> ParseOf { get; }
        DocumentProvider Provider { get; }
        IDocumentWatcher Watcher { get; }


        DocumentEngine Initialize();
        DocumentTransaction BeginInternalTransaction();
        DocumentTransaction BeginTransaction();
        DocumentTransaction GetThreadTransaction();
        DocumentTransaction GetTransaction(bool create, bool isInternal);
        void ReleaseTransaction(DocumentTransaction transaction);

        bool LoadDocumentType(string typeOf, bool isInitialize = false);
        void CheckTypeOfSchema(string typeOf);

        Document GetDocumentFromSource(ref DocumentMeta meta, JObject data, int partition);
        

        DocumentType GetDocumentType(string typeOf);
        IDocumentParser GetDocumentParser(string typeOf, OperationType operation);


        DocumentMeta GetTypeMeta(string typeOf);
        List<DocumentMeta> GetTypeMetaAll();

        DocumentInfo GetTypeOfDocumentInfo(string typeOf, object primaryOf);
        DocumentInfo GetTypeOfDocumentInfo(string typeOf, object primaryOf, Type primaryOfDataType);

        T Read<T>(DocumentQuery query, DocumentMeta meta = null);
        List<T> ReadList<T>(DocumentQuery query);
        T GetCount<T>(DocumentQuery query, DocumentMeta meta = null);

        void RemoveAnyCacheOfFromQuery(DocumentQuery query);
        void SetAnyCacheOfDocument<T>(DocumentQuery query, T data);
        T ReadAnyCacheOfFromQuery<T>(DocumentQuery query);

        void SubmitChanges(DocumentContext context, DocumentTransaction transaction);
        void BulkAppend(DocumentContext context, DocumentTransaction transaction);
        void BulkCreate(DocumentContext context, DocumentTransaction transaction);
        void BulkDelete(DocumentContext context, DocumentTransaction transaction);
        void BulkMerge(DocumentContext context, DocumentTransaction transaction);
        int Delete<T>(DocumentQuery query);
        void TruncateTypeOf(DocumentMeta meta);
        void TruncateTypeOf(string typeOf);

        void OnChangeDocuments(DocumentMeta meta, DocumentContext context);
    }
}
