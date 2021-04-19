using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Objectiks.Engine
{
    internal class TransactionMonitor
    {
        static object LockObject = new object();
        private static int LockedTryCount = 25;
        private static ConcurrentDictionary<string, int> Threads = new ConcurrentDictionary<string, int>();

        public static bool Locked(int threadId, string name)
        {
            int check = 0;
            bool locked = false;

            if (!Threads.ContainsKey(name))
            {
                locked = Threads.TryAdd(name, threadId);
            }
            else
            {
                lock (LockObject)
                {
                    while (true)
                    {
                        if (Threads.ContainsKey(name))
                        {
                            if (Threads[name] != threadId)
                            {
                                locked = false;
                                Thread.Sleep(3000);
                            }
                            else
                            {
                                locked = true;

                                break;
                            }
                        }

                        if (check > LockedTryCount)
                        {
                            locked = false;

                            break;
                        }
                    }
                }
            }

            return locked;
        }

        public static bool UnLocked(string name)
        {
            Threads.TryRemove(name, out var locked);

            return locked > 0;
        }
    }
}
