using Objectiks.Engine.Query;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Objectiks.Extentions;
using Objectiks.Attributes;
using Newtonsoft.Json.Linq;
using System.Linq;
using Objectiks.Models;
using Newtonsoft.Json;
using System.Reflection;
using System.Data.Common;
using System.Data;
using Objectiks.Services;
using System.Threading;

namespace Objectiks.Engine
{
    public class DocumentWriter<T> : IDisposable, IDocumentWriter
    {
        private ConcurrentQueue<DocumentQueue> Queue = new ConcurrentQueue<DocumentQueue>();
        private ConcurrentQueue<DocumentPartition> Partitions = new ConcurrentQueue<DocumentPartition>();
        private Format Formatting = Format.None;

        private readonly DocumentEngine Engine;
        private DocumentTransaction Transaction;
        //meta : should not be read-only 
        private DocumentMeta Meta;
        private bool IsPartialStore = false;
        private int? PartialStoreLimit = 0;
        public string TypeOf { get; set; }

        public DocumentWriter() { }

        internal DocumentWriter(DocumentEngine engine, string typeOf)
        {
            Ensure.NotNullOrEmpty(typeOf, "TypeOf is empty");

            TypeOf = typeOf;
            Engine = engine;
            Meta = engine.GetTypeMeta(typeOf);
            IsPartialStore = Engine.Option.SupportPartialStorage;

            if (IsPartialStore)
            {
                PartialStoreLimit = Engine.Option.SupportPartialStorageSize;
            }

            Transaction = Engine.GetThreadTransaction();

            if (Transaction == null)
            {
                Transaction = Engine.BeginInternalTransaction();
            }

            Transaction.EnterTypeOfLock(typeOf);
        }

        private void ReOrderPartitionByOperation()
        {
            if (Partitions.Count > 1)
            {
                var ordered = Partitions.OrderBy(p => p.Operation).ToList();

                Partitions.Clear();

                foreach (var partOf in ordered)
                {
                    Partitions.Enqueue(partOf);
                }
            }
        }

        private Document GetDocument(T model, bool clearDocumentRefs)
        {
            var attr = GetDocumentAttributes(model);

            var document = new Document
            {
                TypeOf = attr.TypeOf,
                CacheOf = attr.CacheOf,
                PrimaryOf = attr.PrimaryOf,
                WorkOf = attr.WorkOf,
                UserOf = attr.UserOf,
                KeyOf = attr.KeyOfValues.ToArray(),
                Data = JObject.FromObject(model),
                Partition = attr.Partition,
                HasArray = false,
                Exists = attr.Exists,
                CreatedAt = DateTime.UtcNow
            };

            if (clearDocumentRefs)
            {
                RemoveIgnoredOrRefProperty(ref document, attr);
            }

            Ensure.NotNullOrEmpty(document.TypeOf, "Document typeOf is empty");

            return document;
        }

        private object GetDocumentPrimaryValue(T model)
        {
            object primaryValue = null;
            var type = model.GetType();
            var properties = type.FindProperties();

            foreach (PropertyInfo property in properties)
            {
                #region PrimaryOf set..
                var primary = property.GetAttribute<PrimaryAttribute>();

                if (primary != null)
                {
                    primaryValue = property.GetValue(model, null);

                    break;
                }
                #endregion
            }

            if (primaryValue == null)
            {
                throw new ArgumentNullException($"Primary value is null typeOf:{TypeOf}");
            }

            return primaryValue;
        }

        private DocumentAttributes GetDocumentAttributes(T model)
        {
            var doc = new DocumentAttributes();

            var type = model.GetType();
            var properties = type.FindProperties();
            var typeOf = type.GetCustomAttribute<TypeOfAttribute>();

            if (typeOf == null)
            {
                doc.TypeOf = type.Name;
            }
            else
            {
                doc.TypeOf = typeOf.Name;

                if (String.IsNullOrWhiteSpace(doc.TypeOf))
                {
                    doc.TypeOf = type.Name;
                }
            }

            foreach (PropertyInfo property in properties)
            {
                #region Requried - throw exception..
                var requried = property.GetAttribute<RequriedAttribute>();
                if (requried != null)
                {
                    requried.Name = property.Name;
                    requried.HasValue = property.GetValue(model, null) != null;

                    if (!requried.HasValue)
                    {
                        throw new ArgumentNullException(requried.Name);
                    }
                }
                #endregion

                #region PrimaryOf set..
                var primary = property.GetAttribute<PrimaryAttribute>();

                if (primary != null)
                {
                    var primaryValue = property.GetValue(model, null);

                    var info = Engine.GetTypeOfDocumentInfo(doc.TypeOf, primaryValue, property.PropertyType);

                    doc.PrimaryOf = info.PrimaryOf.ToString();
                    doc.CacheOf = Engine.Cache.CacheOfDoc(doc.TypeOf, doc.PrimaryOf);
                    doc.Partition = info.Partition;
                    doc.Exists = info.Exists;

                    if (!info.Exists)
                    {
                        property.SetValue(model, info.PrimaryOf);
                    }

                    continue;
                }
                #endregion

                #region Add keyOfList items..
                var keyOf = property.GetAttribute<KeyOfAttribute>();
                if (keyOf != null)
                {
                    var value = property.GetValue(model, null);
                    if (value != null)
                    {
                        doc.KeyOfValues.Add(value.ToString().ToLowerInvariant());
                    }
                }
                #endregion

                #region Add Ignore prop list
                var ignore = property.GetAttribute<IgnoreAttribute>();
                if (ignore != null)
                {
                    ignore.Name = property.Name;
                    doc.Ignored.Add(ignore);
                }
                #endregion

                #region WorkOf set
                var workOf = property.GetAttribute<WorkOfAttribute>();
                if (workOf != null)
                {
                    var value = property.GetValue(model, null);
                    if (value != null)
                    {
                        doc.WorkOf = value.ToString();
                    }
                    else
                    {
                        doc.WorkOf = "0";
                    }
                }
                #endregion

                #region UserOf set
                var userOf = property.GetAttribute<UserOfAttribute>();
                if (userOf != null)
                {
                    var value = property.GetValue(model, null);
                    if (value != null)
                    {
                        doc.UserOf = value.ToString();
                    }
                    else
                    {
                        doc.UserOf = "0";
                    }
                }
                #endregion
            }

            return doc;
        }

        private void RemoveIgnoredOrRefProperty(ref Document document, DocumentAttributes attrValues)
        {
            foreach (var prop in attrValues.Ignored)
            {
                document.Data.Remove(prop.Name);
            }

            if (Meta.Refs != null)
            {
                foreach (var item in Meta.Refs)
                {
                    string refPropertyName = item.GetTargetProperty();

                    if (attrValues.Ignored.Count(i => i.Name == refPropertyName) > 0)
                    {
                        document.Data.Remove(refPropertyName);
                    }
                }
            }
        }

        private void Enqueue(Document document, DocumentPartition partOf)
        {
            document.Partition = partOf.Partition;

            var queue = new DocumentQueue(document, partOf);

            Queue.Enqueue(queue);

            if (Partitions.Count(p => p.Partition == partOf.Partition && p.Operation == partOf.Operation) == 0)
            {
                Partitions.Enqueue(partOf);
            }
        }

        public void AddDocument(T document, bool clearDocumentRefs = true)
        {
            if (document == null)
            {
                throw new Exception("Document is null");
            }

            var doc = GetDocument(document, clearDocumentRefs);

            int temporyCount = 0;
            DocumentPartition partition;

            if (doc.Exists)
            {
                partition = new DocumentPartition(doc.Partition, doc.Exists);
            }
            else
            {
                if (IsPartialStore)
                {
                    temporyCount = Queue.Count(d => d.PartOf.Partition == Meta.Partitions.Current);
                    partition = Meta.GetPartition(Meta.Partitions.Current, PartialStoreLimit, temporyCount);
                }
                else
                {
                    partition = new DocumentPartition(Meta.Partitions.Current, doc.Exists);
                }
            }

            Enqueue(doc, partition);
        }

        public void AddDocuments(List<T> documents, bool clearDocumentRefs = true)
        {
            foreach (var item in documents)
            {
                AddDocument(item, clearDocumentRefs);
            }
        }

        public void UpdateDocument(T document, bool clearDocumentRefs = true)
        {
            AddDocument(document, clearDocumentRefs);
        }

        public void UpdateDocuments(List<T> documents, bool clearDocumentRefs = true)
        {
            AddDocuments(documents, clearDocumentRefs);
        }

        internal void DeleteDocument(Document Document)
        {
            if (Document.Exists)
            {
                Enqueue(Document, new DocumentPartition(Document.Partition, OperationType.Delete));
            }
            else
            {
                throw new Exception("Document not found..");
            }
        }

        public void DeleteDocument(Guid primaryOf)
        {
            if (primaryOf == Guid.Empty)
            {
                throw new ArgumentNullException("primaryOf is null");
            }

            var doc = Engine.Read(TypeOf, primaryOf);

            DeleteDocument(doc);
        }

        public void DeleteDocument(long primaryOf)
        {
            if (primaryOf == 0)
            {
                throw new ArgumentNullException("primaryOf is null");
            }

            var doc = Engine.Read(TypeOf, primaryOf);

            DeleteDocument(doc);
        }

        public void DeleteDocument(int primaryOf)
        {
            if (primaryOf == 0)
            {
                throw new ArgumentNullException("primaryOf is null");
            }

            var doc = Engine.Read(TypeOf, primaryOf);

            DeleteDocument(doc);
        }

        public void DeleteDocument(T document)
        {
            if (document == null)
            {
                throw new Exception("Document is null");
            }

            var primaryOf = GetDocumentPrimaryValue(document);
            var doc = Engine.Read(TypeOf, primaryOf);

            DeleteDocument(doc);
        }

        public void DeleteDocuments(List<T> documents)
        {
            foreach (var item in documents)
            {
                DeleteDocument(item);
            }
        }

        public void UseFormatting(bool isIndented = true)
        {
            Formatting = isIndented ? Format.Indented : Format.None;
        }

        public void UsePartialStore(int limit = 1)
        {
            if (Engine.Option.SupportPartialStorage)
            {
                if (Queue.Count > 0)
                {
                    throw new Exception("Called before the Add function");
                }

                IsPartialStore = true;
                PartialStoreLimit = limit;
            }
        }

        public void SubmitChanges()
        {
            if (Queue.Count == 0) return;

            try
            {
                ReOrderPartitionByOperation();

                while (Partitions.TryDequeue(out var partOf))
                {
                    var context = new DocumentContext
                    {
                        TypeOf = Meta.TypeOf,
                        Primary = Meta.Primary,
                        Partition = partOf.Partition,
                        Operation = partOf.Operation,
                        Storage = Transaction.GetTransactionalStorage(Meta.TypeOf, partOf.Partition, true),
                        Documents = Queue.Where(q => q.PartOf.Partition == partOf.Partition && q.PartOf.Operation == partOf.Operation).Select(s => s.Document).ToList(),
                        Formatting = Formatting
                    };

                    Engine.SubmitChanges(context, Transaction);
                }

                Queue.Clear();
                Partitions.Clear();

                Commit();
            }
            catch (IOException ex)
            {
                Rollback(ex);
            }
        }

        public void Truncate()
        {
            try
            {
                Transaction.AddTruncateTypeOf(TypeOf);
                Transaction.AddOperation(new DocumentContext
                {
                    TypeOf = TypeOf,
                    Operation = OperationType.Truncate,
                    Documents = new List<Document>(),
                });

                Commit();
            }
            catch (Exception ex)
            {
                Rollback(ex);
            }
        }

        private void Commit()
        {
            if (Transaction.IsInternalTransaction)
            {
                Transaction.Commit();
            }
        }

        private void Rollback(Exception exception = null)
        {
            if (Transaction.IsInternalTransaction)
            {
                Transaction.Rollback(exception);
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Engine.Watcher?.UnLock();
        }
    }
}
