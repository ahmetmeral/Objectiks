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

namespace Objectiks.Engine
{
    public class DocumentWriter<T> : IDisposable
    {

        private Task ExecuteTask;
        private ConcurrentQueue<DocumentQueue> Queue = new ConcurrentQueue<DocumentQueue>();
        private ConcurrentQueue<DocumentPartition> Partitions = new ConcurrentQueue<DocumentPartition>();
        private Format Formatting = Format.None;
        private readonly string TypeOf = string.Empty;
        private readonly DocumentEngine Engine = null;
        //meta : should not be read-only 
        private DocumentMeta Meta = null;
        private bool IsPartialStore = false;
        private int? PartialStoreLimit = 0;

        public DocumentWriter() { }

        internal DocumentWriter(DocumentEngine engine, string typeOf)
        {
            TypeOf = typeOf;
            Engine = engine;
            Meta = engine.GetTypeMeta(typeOf);
            IsPartialStore = Engine.Manifest.Documents.StoragePartial;

            if (IsPartialStore)
            {
                PartialStoreLimit = Engine.Manifest.Documents.StoragePartialLimit;
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

        private Document GetDocument(T document, ref DocumentAttributes attr, bool clearDocumentRefs)
        {
            var cacheOf = Engine.Cache.CacheOfDocument(attr.TypeOf.Name, attr.Primary?.Value.ToString());

            DocumentKey? documentKey = Meta.GetDocumentKeyFromCacheOf(cacheOf);
            var exists = documentKey.HasValue && !String.IsNullOrEmpty(documentKey.Value.PrimaryOf);
            var partition = documentKey.HasValue && !String.IsNullOrEmpty(documentKey.Value.PrimaryOf) ? documentKey.Value.Partition : 0;

            var doc = new Document
            {
                TypeOf = attr.TypeOf.Name,
                Primary = attr.Primary.Value,
                CacheOf = cacheOf,
                Partition = partition,
                HasArray = false,
                HasLazy = Meta.HasLazy,
                Data = JObject.FromObject(document),
                Exists = exists
            };

            if (clearDocumentRefs)
            {
                RemoveIgnoredOrRefProperty(attr, ref doc);
            }

            return doc;
        }

        private DocumentAttributes GetDocumentAttributes(T model, bool keyOfProp = false)
        {
            var type = model.GetType();
            var properties = type.FindProperties();

            var typeOf = type.GetCustomAttribute<TypeOfAttribute>();

            if (typeOf == null)
            {
                typeOf = new TypeOfAttribute();
            }

            if (String.IsNullOrWhiteSpace(typeOf.Name))
            {
                typeOf.Name = type.Name;
            }

            var document = new DocumentAttributes(typeOf);

            foreach (var property in properties)
            {
                #region Primary
                var primary = property.GetAttribute<PrimaryAttribute>();

                if (primary != null)
                {
                    primary.Value = property.GetValue(model, null);

                    if (primary.Value == null)
                    {
                        primary.Value = Meta.GetNewSequenceId(property.PropertyType);

                        Ensure.NotNull(primary.Value, $"TypeOf:{typeOf.Name} Primary value is null");

                        property.SetValue(model, primary.Value);

                        document.IsNew = true;
                    }

                    document.Primary = primary;

                    continue;
                }
                #endregion

                #region KeyOf
                if (keyOfProp)
                {
                    var keyOf = property.GetAttribute<KeyOfAttribute>();
                    if (keyOf != null)
                    {
                        keyOf.Name = property.Name;

                        var value = property.GetValue(model, null);

                        if (value != null)
                        {
                            document.KeyOfValues.Add(value.ToString());
                        }
                        else
                        {
                            throw new ArgumentNullException(keyOf.Name);
                        }


                        document.Add(keyOf);

                        continue;
                    }
                }
                #endregion

                #region Requried
                var requried = property.GetAttribute<RequriedAttribute>();
                if (requried != null)
                {
                    requried.Name = property.Name;
                    requried.HasValue = property.GetValue(model, null) != null;

                    if (!requried.HasValue)
                    {
                        throw new ArgumentNullException(requried.Name);
                    }

                    document.Add(requried);

                    continue;
                }
                #endregion

                #region Ignore
                var ignore = property.GetAttribute<IgnoreAttribute>();
                if (ignore != null)
                {
                    ignore.Name = property.Name;
                    document.Add(ignore);
                }
                #endregion
            }

            return document;
        }

        private void RemoveIgnoredOrRefProperty(DocumentAttributes attr, ref Document document)
        {
            foreach (var prop in attr.Ignored)
            {
                document.Data.Remove(prop.Name);
            }

            if (Meta.Refs != null)
            {
                foreach (var item in Meta.Refs)
                {
                    string refPropertyName = item.GetTargetProperty();

                    if (attr.Ignored.Count(i => i.Name == refPropertyName) > 0)
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

        public void Add(T document, bool clearDocumentRefs = true)
        {
            if (document == null)
            {
                throw new Exception("Document is null");
            }

            var attr = GetDocumentAttributes(document, false);
            var doc = GetDocument(document, ref attr, clearDocumentRefs);

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

            var attr = GetDocumentAttributes(document, false);
            var doc = GetDocument(document, ref attr, false);

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

        private void Execute()
        {
            if (Queue.Count == 0) return;

            try
            {
                ReOrderPartitionByOperation();

                while (Partitions.TryDequeue(out var partOf))
                {
                    var info = new DocumentInfo(Meta.TypeOf, partOf.Partition);

                    var docs = Queue.Where(q => q.PartOf.Partition == partOf.Partition &&
                    q.PartOf.Operation == partOf.Operation).Select(s => s.Document).ToList();

                    Engine.Write(Meta, info, docs, partOf.Operation, Formatting);
                }

                Queue.Clear();
                Partitions.Clear();
            }
            catch (IOException)
            {
                //oluşur ise rollbackler çalışır tekrar yüklemek mantıklı olur.
            }
        }
    }
}
