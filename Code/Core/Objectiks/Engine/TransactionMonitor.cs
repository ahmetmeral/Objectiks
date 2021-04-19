using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
            bool locked = true;

            if (!Threads.ContainsKey(name))
            {
                locked = !Threads.TryAdd(name, threadId);
            }
            else
            {
                lock (LockObject)
                {
                    while (true)
                    {
                        check++;

                        if (Threads.ContainsKey(name))
                        {
                            if (Threads[name] != threadId)
                            {
                                locked = true;
                                Thread.Sleep(3000);
                            }
                            else
                            {
                                locked = false;

                                break;
                            }
                        }

                        if (check > LockedTryCount)
                        {
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

        public static bool UnLocked(int threadId)
        {
            try
            {
                var keyValuePair = Threads.Where(t => t.Value == threadId).ToList();

                if (keyValuePair.Count > 0)
                {
                    foreach (var item in keyValuePair)
                    {
                        Threads.Remove(item.Key, out int tid);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
