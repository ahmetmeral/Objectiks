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

        DocumentTransaction BeginInternalTransaction();
        DocumentTransaction BeginTransaction();
        void BulkAppend(DocumentContext context, DocumentTransaction transaction);
        void BulkCreate(DocumentContext context, DocumentTransaction transaction);
        void BulkDelete(DocumentContext context, DocumentTransaction transaction);
        void BulkMerge(DocumentContext context, DocumentTransaction transaction);
        void CheckTypeOfSchema(string typeOf);
        int Delete<T>(DocumentQuery query);
        T GetCount<T>(DocumentQuery query, DocumentMeta meta = null);
        T GetCountFromQueryOf<T>(DocumentQuery query, DocumentMeta meta = null);
        Document GetDocumentFromSource(ref DocumentMeta meta, JObject data, int partition);
        QueryResult GetDocumentKeysFromQueryOf(DocumentQuery query, DocumentMeta meta = null);
        DocumentManifest GetDocumentManifest(string baseDirectory);
        IDocumentParser GetDocumentParser(string typeOf);
        DocumentSchema GetDocumentSchema(string typeOf);
        DocumentTransaction GetThreadTransaction();
        DocumentTransaction GetTransaction(bool create, bool isInternal);
        DocumentMeta GetTypeMeta(string typeOf);
        List<DocumentMeta> GetTypeMetaAll();
        DocumentInfo GetTypeOfDocumentInfo(string typeOf, object primaryOf, Type primaryOfDataType);
        DocumentEngine Initialize();
        bool LoadDocumentType(string typeOf, bool isInitialize = false);
        void OnChangeDocuments(DocumentMeta meta, DocumentContext context);
        void ParseDocumentData(ref DocumentMeta meta, ref Document document, DocumentStorage file, OperationType operation);
        Document Read(DocumentQuery query, DocumentMeta meta = null);
        Document Read(string typeOf, object primaryOf);
        T Read<T>(DocumentQuery query, DocumentMeta meta = null);
        T ReadAnyCacheOfFromQuery<T>(DocumentQuery query);
        List<T> ReadList<T>(DocumentQuery query);
        void ReleaseTransaction(DocumentTransaction transaction);
        void RemoveAnyCacheOfFromQuery(DocumentQuery query);
        void SetAnyCacheOfDocument<T>(DocumentQuery query, T data);
        void SubmitChanges(DocumentContext context, DocumentTransaction transaction);
        void TruncateTypeOf(DocumentMeta meta);
        void TruncateTypeOf(string typeOf);
    }
}
