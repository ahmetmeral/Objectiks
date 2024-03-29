﻿using Objectiks.Engine;
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
        internal ConcurrentDictionary<string, List<DocumentContext>> TypeOfContext { get; set; }
        internal List<string> TypeOfLock { get; set; }
        internal List<string> TypeOfTruncate { get; set; }
        internal List<DocumentStorage> Storages { get; set; }
        internal bool IsInternalTransaction { get; set; }
        internal bool IsTruncate { get; set; }
        internal bool IsException { get; set; }


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
            TypeOfContext = new ConcurrentDictionary<string, List<DocumentContext>>(StringComparer.OrdinalIgnoreCase);
            TypeOfLock = new List<string>();
            TypeOfTruncate = new List<string>();
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

        public void AddOperation(DocumentContext documentContext)
        {
            if (!TypeOfContext.ContainsKey(documentContext.TypeOf.ToLowerInvariant()))
            {
                TypeOfContext.TryAdd(documentContext.TypeOf.ToLowerInvariant(), new List<DocumentContext>());
            }

            TypeOfContext[documentContext.TypeOf.ToLowerInvariant()].Add(documentContext);
        }

        internal void EnterTypeOfLock(string typeOf)
        {
            Monitor.EnterLock(typeOf);

            if (!TypeOfLock.Contains(typeOf.ToLowerInvariant()))
            {
                TypeOfLock.Add(typeOf.ToLowerInvariant());
            }
        }

        internal void ExitTypeOfLock(string typeOf)
        {
            if (TypeOfLock.Contains(typeOf.ToLowerInvariant()))
            {
                TypeOfLock.Remove(typeOf.ToLowerInvariant());
            }

            Monitor.ExitLock(typeOf);
        }

        internal void ExitAllTypeOfLock()
        {
            var typeOfList = new List<string>(TypeOfLock);

            foreach (var typeOf in typeOfList)
            {
                ExitTypeOfLock(typeOf);
            }

            TypeOfLock.Clear();
        }

        internal void AddTruncateTypeOf(string typeOf)
        {
            if (!TypeOfTruncate.Contains(typeOf.ToLowerInvariant()))
            {
                TypeOfTruncate.Add(typeOf.ToLowerInvariant());
            }
        }

        internal void RemoveTruncateTypeOf(string typeOf)
        {
            if (TypeOfTruncate.Contains(typeOf.ToLowerInvariant()))
            {
                TypeOfTruncate.Remove(typeOf.ToLowerInvariant());
            }
        }

        internal bool IsTruncateTypeOf(string typeOf)
        {
            return TypeOfTruncate.Contains(typeOf.ToLowerInvariant());
        }

        public void Commit()
        {
            try
            {
                IsException = false;

                foreach (var item in TypeOfContext)
                {
                    var typeOf = item.Key;
                    var meta = Engine.GetTypeMeta(typeOf);

                    if (IsTruncateTypeOf(typeOf))
                    {
                        Engine.TruncateTypeOf(meta);

                        RemoveTruncateTypeOf(typeOf);

                        continue;
                    }

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
                IsException = true;

                throw ex;
            }
            finally
            {
                if (!IsException)
                {
                    ExitAllTypeOfLock();
                    Engine.ReleaseTransaction(this);
                }
            }
        }

        public void Rollback(bool isTypeOfReload)
        {
            try
            {
                if (!Monitor.IsInTransaction)
                {
                    throw new Exception("active transaction not found");
                }

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

                if (isTypeOfReload)
                {
                    foreach (var item in TypeOfContext)
                    {
                        var typeOf = item.Key;
                        Engine.LoadDocumentType(typeOf);
                    }
                }
            }
            catch (Exception ex)
            {
                Engine.Logger?.Error(ex);

                throw ex;
            }
            finally
            {
                if (Monitor.IsInTransaction)
                {
                    ExitAllTypeOfLock();
                    Engine.ReleaseTransaction(this);
                }
            }
        }

        public void Rollback()
        {
            Rollback(true);
        }

        public void Rollback(Exception ex)
        {
            Rollback(true);

            Engine.Logger?.Error(ex);
        }

        public void Rollback(Exception ex, bool isTypeOfReload)
        {
            Rollback(isTypeOfReload);

            Engine.Logger?.Error(ex);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
