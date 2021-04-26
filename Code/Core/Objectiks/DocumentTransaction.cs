using Objectiks.Engine;
using Objectiks.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;

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

        internal DocumentStorage GetTransactionStorage(string typeOf, int partition, bool isBeginOperation)
        {
            var storage = Storages.Where(s => s.TypeOf == typeOf && s.Partition == partition)
                .FirstOrDefault();

            if (storage == null)
            {
                storage = new DocumentStorage(typeOf, Engine.Provider.BaseDirectory, partition);

                //var isLocked = TransactionMonitor.LockedOld(ThreadId, storage.NameWithoutExtension);

                //if (isLocked) { throw new Exception("Failed to lock document.."); }

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

        internal void EnterTypeOfLock(string typeOf)
        {
            Monitor.Enter(typeOf.ToLowerInvariant());
        }

        internal void ExitTypeOfLock(string typeOf)
        {
            if (Monitor.IsEntered(typeOf))
            {
                Monitor.Exit(typeOf.ToLowerInvariant());
            }
        }

        internal void ExitAllTypeOfLock()
        {
            foreach (var item in TypeOf)
            {
                ExitTypeOfLock(item.Key);
            }
        }


        public void Commit()
        {
            try
            {
                if (DbTransaction == null)
                {
                    foreach (var storage in Storages)
                    {
                        storage.Commit();
                        storage.Dispose();
                    }
                }
                else
                {
                    DbTransaction.Commit();
                }

                foreach (var item in TypeOf)
                {
                    var meta = Engine.GetTypeMeta(item.Key);

                    foreach (var context in item.Value)
                    {
                        Engine.OnChangeDocuments(meta, context);
                    }

                    Engine.Cache.Set(meta, meta.Cache.Expire);
                }
            }
            catch (Exception ex)
            {
                Engine.Logger?.Error(ex);

                throw ex;
            }
        }

        public void Rollback()
        {
            try
            {
                if (DbTransaction == null)
                {
                    foreach (var storage in Storages)
                    {
                        storage.Rollback();
                        storage.Dispose();
                    }
                }
                else
                {
                    DbTransaction.Rollback();
                }
            }
            catch (Exception ex)
            {
                Engine.Logger?.Error(ex);

                throw ex;
            }
        }

        public void Rollback(Exception ex)
        {
            Rollback();

            Engine.Logger?.Error(ex);
        }

        public void Dispose()
        {
            ExitAllTypeOfLock();

            GC.SuppressFinalize(this);
        }
    }
}
