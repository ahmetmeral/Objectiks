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
        internal ConcurrentDictionary<string, List<DocumentContext>> TypeOf { get; set; }
        internal List<DocumentStorage> Storages { get; set; }
        internal bool IsInternalTransaction { get; set; }

        internal DocumentTransaction(DocumentEngine engine, bool isInternalTransaction)
        {
            Engine = engine;
            ThreadId = Environment.CurrentManagedThreadId;
            TypeOf = new ConcurrentDictionary<string, List<DocumentContext>>();
            Storages = new List<DocumentStorage>();
            IsInternalTransaction = isInternalTransaction;
        }

        internal DocumentStorage Ensure(string typeOf, int partition, bool isBeginOperation)
        {
            var storage = Storages.Where(s => s.TypeOfName == typeOf && s.Partition == partition)
                .FirstOrDefault();

            if (storage == null)
            {
                storage = new DocumentStorage(typeOf, Engine.Provider.BaseDirectory, partition);

                var isLocked = TransactionMonitor.Locked(ThreadId, storage.NameWithoutExtension);

                if (isLocked) { throw new Exception("Failed to lock document.."); }

                if (isBeginOperation)
                {
                    storage.BeginOperation();
                }

                Storages.Add(storage);
            }

            return storage;
        }

        internal void AddOperation(DocumentContext documentContext)
        {
            if (!TypeOf.ContainsKey(documentContext.TypeOf.ToLowerInvariant()))
            {
                TypeOf.TryAdd(documentContext.TypeOf.ToLowerInvariant(), new List<DocumentContext>());
            }

            TypeOf[documentContext.TypeOf.ToLowerInvariant()].Add(documentContext);
        }

        public void Commit()
        {
            foreach (var storage in Storages)
            {
                try
                {
                    storage.Commit();
                    storage.Dispose();
                }
                catch (Exception ex)
                {
                    Engine.Logger?.Error(ex);

                    TransactionMonitor.UnLocked(ThreadId);

                    throw ex;
                }
            }

            foreach (var item in TypeOf)
            {
                var meta = Engine.GetTypeMeta(item.Key);

                foreach (var context in item.Value)
                {
                    try
                    {
                        Engine.OnChangeDocuments(meta, context);
                    }
                    catch (Exception ex)
                    {
                        Engine.Logger?.Error(ex);
                    }
                }
            }

            Engine.Transaction = null;
            TransactionMonitor.UnLocked(ThreadId);
        }

        public void Rollback()
        {
            foreach (var storage in Storages)
            {
                try
                {
                    storage.Rollback();
                    storage.Dispose();
                }
                catch (Exception ex)
                {
                    TransactionMonitor.UnLocked(ThreadId);

                    Engine.Logger?.Error(ex);
                    Engine.Logger?.Debug(DebugType.Transaction, $"TypeOf:{storage.TypeOfName} rollback failed..{ex.Message}");
                }
            }

            Engine.Transaction = null;
            TransactionMonitor.UnLocked(ThreadId);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
