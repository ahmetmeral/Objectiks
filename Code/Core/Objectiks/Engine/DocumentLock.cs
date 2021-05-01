using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Objectiks.Engine
{
    internal class DocumentLock : IDisposable
    {
        private int Timeout = 1000 * 60;
        private readonly ReaderWriterLockSlim Transaction = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly ConcurrentDictionary<string, object> TypeOf = new ConcurrentDictionary<string, object>();

        public DocumentLock() { }

        public bool IsInTransaction => Transaction.IsReadLockHeld || Transaction.IsWriteLockHeld;

        public int TransactionsCount => Transaction.CurrentReadCount;

        public void EnterTransaction()
        {
            if (Transaction.IsWriteLockHeld) { return; }

            if (Transaction.TryEnterReadLock(Timeout) == false)
            {
                throw new Exception("Transaction read lock timeout");
            }
        }

        public void ExitTransaction()
        {
            if (Transaction.IsWriteLockHeld) { return; }

            Transaction.ExitReadLock();
        }

        public void EnterLock(string typeOfName)
        {
            var typeOf = TypeOf.GetOrAdd(typeOfName.ToLowerInvariant(), (s) => new object());

            if (!Monitor.TryEnter(typeOf, Timeout))
            {
                throw new Exception("Transaction write timeout");
            }
        }

        public void ExitLock(string typeOfName)
        {
            if (!TypeOf.TryGetValue(typeOfName.ToLowerInvariant(), out var typeOf))
            {
                throw new Exception("Transaction locker not found");
            }

            if (Monitor.IsEntered(typeOf))
            {
                Monitor.Exit(typeOf);
            }
        }

        public bool EnterExclusive()
        {
            if (Transaction.IsWriteLockHeld) { return false; }

            if (Transaction.TryEnterWriteLock(Timeout) == false)
            {
                throw new Exception("Transaction exclusive timeout");
            }

            return true;
        }

        public bool TryEnterExlusive(out bool mustExit)
        {
            if (Transaction.IsWriteLockHeld)
            {
                mustExit = false;
                return true;
            }

            if (Transaction.IsReadLockHeld || Transaction.CurrentReadCount > 0)
            {
                mustExit = false;
                return false;
            }


            if (Transaction.TryEnterWriteLock(10) == false)
            {
                mustExit = false;
                return false;
            }

            if (Transaction.RecursiveReadCount == 0)
            {
                throw new Exception("Must have no other transaction here");
            }

            mustExit = true;

            return true;
        }

        public void ExitExclusive()
        {
            Transaction.ExitWriteLock();
        }

        public ICollection<string> GetTypeOfList()
        {
            return TypeOf.Keys;
        }

        public void Dispose()
        {
            try
            {
                Transaction.Dispose();
            }
            catch (SynchronizationLockException) { }
        }
    }
}
