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
        private readonly DocumentMonitor Monitor;
        private readonly int _ThreadId = Environment.CurrentManagedThreadId;

        internal DbTransaction DbTransaction { get; set; }
        internal ConcurrentDictionary<string, List<DocumentContext>> TypeOf { get; set; }
        internal List<DocumentStorage> Storages { get; set; }
        internal bool IsInternalTransaction { get; set; }

        public uint TransactionId
        {
            get; set;
        }

        public int ThreadId
        {
            get { return _ThreadId; }
        }

        internal DocumentTransaction(uint transactionId, DocumentEngine engine, DocumentMonitor monitor, bool isInternal)
        {
            TransactionId = transactionId;
            Engine = engine;
            Monitor = monitor;
            Storages = new List<DocumentStorage>();
            TypeOf = new ConcurrentDictionary<string, List<DocumentContext>>();
            IsInternalTransaction = isInternal;
        }

        //internal DocumentTransaction(DocumentEngine engine, bool isInternalTransaction)
        //{
        //    Engine = engine;
        //    TypeOf = new ConcurrentDictionary<string, List<DocumentContext>>();
        //    Storages = new List<DocumentStorage>();
        //    IsInternalTransaction = isInternalTransaction;
        //}

        internal DocumentStorage GetTransactionalStorage(string typeOf, int partition, bool isBeginOperation)
        {
            var storage = Storages.Where(s => s.TypeOf == typeOf && s.Partition == partition)
                .FirstOrDefault();

            if (storage == null)
            {
                storage = new DocumentStorage(typeOf, Engine.Provider.BaseDirectory, partition);

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
            Monitor.EnterLock(typeOf);
        }

        internal void ExitTypeOfLock(string typeOf)
        {
            Monitor.ExitLock(typeOf);
        }

        internal void ExitAllTypeOfLock()
        {
            foreach (var item in TypeOf.Keys)
            {
                ExitTypeOfLock(item);
            }
        }

        public void Commit()
        {
            try
            {
                foreach (var item in TypeOf)
                {
                    var meta = Engine.GetTypeMeta(item.Key);

                    foreach (var context in item.Value)
                    {
                        Engine.OnChangeDocuments(meta, context);
                    }

                    Engine.Cache.Set(meta, meta.Cache.Expire);
                }

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

               
            }
            catch (Exception ex)
            {
                Engine.Logger?.Error(ex);

                throw ex;
            }
            finally
            {
                ExitAllTypeOfLock();
                ObjectiksOf.Core.ReleaseTransaction(this);
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
            finally
            {
                ExitAllTypeOfLock();
                ObjectiksOf.Core.ReleaseTransaction(this);
            }
        }

        public void Rollback(Exception ex)
        {
            Rollback();

            Engine.Logger?.Error(ex);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
