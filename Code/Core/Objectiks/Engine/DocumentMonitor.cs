using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Objectiks.Engine
{
    internal class DocumentMonitor
    {
        private readonly Dictionary<uint, DocumentTransaction> TransactionList = new Dictionary<uint, DocumentTransaction>();
        private readonly ThreadLocal<DocumentTransaction> Slot = new ThreadLocal<DocumentTransaction>();
        private readonly DocumentLock Locker;
        private int LastTransactionID = 0;

        public ICollection<DocumentTransaction> Transactions => TransactionList.Values;
        public bool IsInTransaction { get { return Locker.IsInTransaction; } }

        public DocumentMonitor()
        {
            Locker = new DocumentLock();
        }

        private uint GetNextTransactionID()
        {
            return (uint)Interlocked.Increment(ref LastTransactionID);
        }

        public DocumentTransaction GetTransaction(DocumentEngine engine, bool create, bool isInternal)
        {
            var transaction = Slot.Value;

            if (create && transaction == null)
            {
                bool alreadyLock;

                lock (TransactionList)
                {
                    alreadyLock = TransactionList.Values.Any(x => x.ThreadId == Environment.CurrentManagedThreadId);

                    transaction = new DocumentTransaction(GetNextTransactionID(), engine, this, isInternal);

                    TransactionList[transaction.TransactionId] = transaction;
                }

                if (!alreadyLock)
                {
                    Locker.EnterTransaction();
                }

                Slot.Value = transaction;
            }

            return transaction;
        }

        public void EnterLock(string typeOf)
        {
            Locker.EnterLock(typeOf);
        }

        public void ExitLock(string typeOf)
        {
            Locker.ExitLock(typeOf);
        }

        public void ReleaseTransaction(DocumentTransaction transaction)
        {
            bool isKeepLocked;

            lock (TransactionList)
            {
                TransactionList.Remove(transaction.TransactionId);

                isKeepLocked = TransactionList.Values.Any(x => x.ThreadId == Environment.CurrentManagedThreadId);
            }

            if (!isKeepLocked)
            {
                Locker.ExitTransaction();
            }

            Slot.Value = null;
        }

        public DocumentTransaction GetThreadTransaction()
        {
            lock (TransactionList)
            {
                return Slot.Value ?? TransactionList.Values.FirstOrDefault(x => x.ThreadId == Environment.CurrentManagedThreadId);
            }
        }

        public void Dispose()
        {
            if (TransactionList.Count > 0)
            {
                foreach (var transaction in TransactionList.Values)
                {
                    transaction.Dispose();
                }

                TransactionList.Clear();
            }
        }
    }
}
