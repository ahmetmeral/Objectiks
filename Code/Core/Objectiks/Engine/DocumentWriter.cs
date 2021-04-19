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

namespace Objectiks.Engine
{
    public class DocumentWriter<T> : IDisposable, IDocumentWriter
    {
        private Task ExecuteTask;
        private ConcurrentQueue<DocumentQueue> Queue = new ConcurrentQueue<DocumentQueue>();
        private ConcurrentQueue<DocumentPartition> Partitions = new ConcurrentQueue<DocumentPartition>();
        private Format Formatting = Format.None;
        private readonly DocumentEngine Engine = null;
        //meta : should not be read-only 
        private DocumentMeta Meta = null;
        private bool IsPartialStore = false;
        private int? PartialStoreLimit = 0;

        public string TypeOf { get; set; }

        public DocumentWriter() { }

        internal DocumentWriter(DocumentEngine engine, string typeOf)
        {
            TypeOf = typeOf;
            Engine = engine;
            Meta = engine.GetTypeMeta(typeOf);
            IsPartialStore = Engine.Option.SupportPartialStorage;

            if (IsPartialStore)
            {
                PartialStoreLimit = Engine.Option.SupportPartialStorageLimit;
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
            var attrValues = GetDocumentAttributes(model);
            var cacheOf = Engine.Cache.CacheOfDocument(attrValues.TypeOf, attrValues.Primary);

            DocumentKey? documentKey = Meta.GetDocumentKeyFromCacheOf(cacheOf);
            var exists = documentKey.HasValue && !String.IsNullOrEmpty(documentKey.Value.PrimaryOf);

            var document = new Document
            {
                CacheOf = cacheOf,
                PrimaryOf = attrValues.Primary,
                AccountOf = attrValues.Account,
                UserOf = attrValues.User,
                Data = JObject.FromObject(model),
                Partition = exists ? documentKey.Value.Partition : 0,
                HasArray = false,
                Exists = exists
            };

            if (clearDocumentRefs)
            {
                RemoveIgnoredOrRefProperty(ref document, attrValues);
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
                    primary.Value = property.GetValue(model, null);

                    if (primary.Value == null)
                    {
                        primary.Value = Meta.GetNewSequenceId(property.PropertyType);
                    }

                    doc.Primary = primary.Value.ToString();

                    property.SetValue(model, primary.Value);
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

        public void WatcherLock()
        {
            Engine.Watcher?.Lock();
        }

        public void WatcherUnLock()
        {
            Engine.Watcher?.UnLock();
        }

        public void Add(T document, bool clearDocumentRefs = true)
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

        public void Add(List<T> documents, bool clearDocumentRefs = true)
        {
            foreach (var item in documents)
            {
                Add(item, clearDocumentRefs);
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
            if (Queue.Count > 0)
            {
                throw new Exception("Called before the Add function");
            }

            IsPartialStore = true;
            PartialStoreLimit = limit;
        }

        private void Execute()
        {
            if (Queue.Count == 0) return;

            try
            {
                Engine.Watcher?.Lock();

                ReOrderPartitionByOperation();

                while (Partitions.TryDequeue(out var partOf))
                {
                    var storage = Engine.Transaction.Ensure(Meta.TypeOf, partOf.Partition);

                    var docs = Queue.Where(q => q.PartOf.Partition == partOf.Partition &&
                    q.PartOf.Operation == partOf.Operation).Select(s => s.Document).ToList();

                    Engine.SubmitChanges(Meta, storage, docs, partOf.Operation, Formatting);
                }

                Queue.Clear();
                Partitions.Clear();

                Engine.Watcher?.UnLock();
            }
            catch (IOException)
            {
                //oluşur ise rollbackler çalışır tekrar yüklemek mantıklı olur.
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
            GC.SuppressFinalize(this);

        }
    }
}
