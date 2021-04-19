using Objectiks.Engine;
using Objectiks.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Objectiks
{
    public class DocumentTransaction : IDisposable
    {
        private DocumentEngine Engine;
        internal int ThreadId { get; set; }
        internal DbTransaction DbTransaction { get; set; }
        internal ConcurrentDictionary<string, List<DocumentTransactionContext>> TypeOf { get; set; }
        internal List<DocumentStorage> Storages { get; set; }


        internal DocumentTransaction(DocumentEngine engine)
        {
            Engine = engine;
            ThreadId = Environment.CurrentManagedThreadId;
            TypeOf = new ConcurrentDictionary<string, List<DocumentTransactionContext>>();
            Storages = new List<DocumentStorage>();
        }

        internal DocumentStorage Ensure(string typeOf, int partition)
        {
            var storage = Storages.Where(s => s.TypeOfName == typeOf && s.Partition == partition)
                .FirstOrDefault();

            if (storage == null)
            {
                storage = new DocumentStorage(typeOf, Engine.Provider.BaseDirectory, partition);

                var isLocked = TransactionMonitor.Locked(ThreadId, storage.NameWithoutExtension);

                if (isLocked) { throw new Exception("Failed to lock document.."); }

                Storages.Add(storage);
            }

            return storage;
        }

        internal void AddOperation(string typeOf, DocumentStorage storage, List<Document> documents, OperationType operation)
        {
            if (!TypeOf.ContainsKey(typeOf.ToLowerInvariant()))
            {
                TypeOf.TryAdd(typeOf.ToLowerInvariant(), new List<DocumentTransactionContext>());
            }

            TypeOf[typeOf.ToLowerInvariant()].Add(new DocumentTransactionContext
            {
                TypeOf = typeOf,
                Storage = storage,
                Documents = documents,
                Operation = operation
            });
        }



        public void Commit()
        {
            foreach (var item in TypeOf)
            {
                var meta = Engine.GetTypeMeta(item.Key);

                //meta datayı güncelleyeceğiz..dokümanların kilitlerini kaldıracağız..

                foreach (var context in item.Value)
                {

                }

            }
        }

        public void Rollback()
        {

        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
