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
        private Task ExecuteTask;
        private ConcurrentQueue<DocumentQueue> Queue = new ConcurrentQueue<DocumentQueue>();
        private ConcurrentQueue<DocumentPartition> Partitions = new ConcurrentQueue<DocumentPartition>();
        private TransactionMonitor TransactionMonitor = new TransactionMonitor();
        private Format Formatting = Format.None;
        private readonly DocumentEngine Engine;
        private DocumentTransaction Transaction;
        //meta : should not be read-only 
        private DocumentMeta Meta;
        private bool IsPartialStore = false;
        private int? PartialStoreLimit = 0;

        public string TypeOf { get; set; }

        public DocumentWriter() { }

        internal DocumentWriter(DocumentEngine engine, string typeOf, DocumentTransaction transaction = null)
        {
            TypeOf = typeOf;
            Engine = engine;
            Meta = engine.GetTypeMeta(typeOf);
            IsPartialStore = Engine.Option.SupportPartialStorage;

            if (IsPartialStore)
            {
                PartialStoreLimit = Engine.Option.SupportPartialStorageLimit;
            }

            if (transaction == null)
            {
                Transaction = new DocumentTransaction(engine, true);
            }
            else
            {
                Transaction = transaction;
            }
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
                CacheOf = attr.CacheOf,
                PrimaryOf = attr.Primary,
                WorkOf = attr.Account,
                UserOf = attr.User,
                Data = JObject.FromObject(model),
                Partition = attr.Partition,
                HasArray = false,
                Exists = attr.Exists
            };

            if (clearDocumentRefs)
            {
                RemoveIgnoredOrRefProperty(ref document, attr);
            }

            return document;
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

                    continue;
                }
                #endregion

                #region PrimaryOf set..
                var primary = property.GetAttribute<PrimaryAttribute>();

                if (primary != null)
                {
                    var primaryValue = property.GetValue(model, null);

                    var info = Engine.GetTypeOfDocumentInfo(TypeOf, primaryValue, property.PropertyType);

                    doc.Primary = info.PrimaryOf.ToString();
                    doc.CacheOf = Engine.Cache.CacheOfDocument(doc.TypeOf, doc.Primary);
                    doc.Partition = info.Partition;
                    doc.Exists = info.Exists;

                    property.SetValue(model, info.PrimaryOf);
                }
                #endregion

                #region Add keyOfList items..
                var keyOf = property.GetAttribute<KeyOfAttribute>();
                if (keyOf != null)
                {
                    var value = property.GetValue(model, null);
                    if (value != null)
                    {
                        doc.KeyOfValues.Add(value.ToString());
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

                #region AccountOf set
                var accountOf = property.GetAttribute<UserOfAttribute>();
                if (accountOf != null)
                {
                    var value = property.GetAttribute<UserOfAttribute>();
                    if (value != null)
                    {
                        doc.Account = value.ToString();
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
                        doc.User = value.ToString();
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
                    partition = Meta.GetAvailablePartition(Meta.Partitions.Current, PartialStoreLimit, temporyCount);
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

        public void Delete(T document)
        {
            if (document == null)
            {
                throw new Exception("Document is null");
            }

            var doc = GetDocument(document, false);

            if (doc.Exists)
            {
                Enqueue(doc, new DocumentPartition(doc.Partition, OperationType.Delete));
            }
            else
            {
                throw new Exception("Document not found..");
            }
        }

        public void Delete(List<T> documents)
        {
            foreach (var item in documents)
            {
                Delete(item);
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

        private void MonitorEnter()
        {
            Monitor.Enter(TypeOf.ToLowerInvariant());
        }

        private void MonitorExit()
        {
            if (Monitor.IsEntered(TypeOf.ToLowerInvariant()))
            {
                Monitor.Exit(TypeOf.ToLowerInvariant());
            }
        }

        private void Execute()
        {
            if (Queue.Count == 0) return;

            try
            {
                Engine.Watcher?.Lock();

                ReOrderPartitionByOperation();

                MonitorEnter();

                while (Partitions.TryDequeue(out var partOf))
                {
                    var context = new DocumentContext
                    {
                        TypeOf = Meta.TypeOf,
                        Primary = Meta.Primary,
                        Partition = partOf.Partition,
                        Operation = partOf.Operation,
                        Storage = Transaction.GetTransactionStorage(Meta.TypeOf, partOf.Partition, true),
                        Documents = Queue.Where(q => q.PartOf.Partition == partOf.Partition && q.PartOf.Operation == partOf.Operation).Select(s => s.Document).ToList(),
                        Formatting = Formatting
                    };

                    Engine.SubmitChanges(context, Transaction);
                }

                MonitorExit();

                Queue.Clear();
                Partitions.Clear();

                if (Transaction.IsInternalTransaction)
                {
                    Transaction.Commit();
                }
            }
            catch (IOException)
            {
                if (Transaction.IsInternalTransaction)
                {
                    Transaction.Rollback();
                }
            }
        }

        public void SubmitChanges()
        {
            lock (Queue)
            {
                if (Queue.Count > 0 && (ExecuteTask == null || ExecuteTask.IsCompleted))
                {
                    // https://blog.stephencleary.com/2013/08/startnew-is-dangerous.html
                    ExecuteTask = Task.Run(this.Execute);
                }
            }
        }

        public void Wait()
        {
            lock (Queue)
            {
                if (ExecuteTask != null)
                {
                    ExecuteTask.Wait();
                }

                if (Queue.Count > 0)
                {
                    this.Execute();
                }
            }
        }

        public void Dispose()
        {
            this.Wait();
            this.MonitorExit();
            GC.SuppressFinalize(this);
            Engine.Watcher?.UnLock();
        }
    }
}
