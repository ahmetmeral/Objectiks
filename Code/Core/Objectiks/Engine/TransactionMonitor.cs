using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Objectiks.Engine
{
    //burası typeOf üzerinden çalışması gerekiyor.. ReadWriteSlim de bizim işimize yarayabilirz.
    public class TransactionMonitor
    {
        static object LockObject = new object();
        static int LockedTryCount = 25;

        private static ConcurrentDictionary<string, bool> Locked = new ConcurrentDictionary<string, bool>();

        public TransactionMonitor() { }

        public static bool EnterLock(string typeOf)
        {
            int check = 0;
            bool locked = true;

            if (!Locked.ContainsKey(typeOf))
            {
                locked = false;
                Locked.TryAdd(typeOf, true);

                return locked;
            }
            else
            {
                lock (LockObject)
                {
                    while (true)
                    {
                        check++;

                        if (Locked.ContainsKey(typeOf))
                        {
                            Thread.Sleep(3000);
                        }
                        else
                        {
                            Thread.Sleep(3000);
                            break;
                        }

                        if (check > LockedTryCount)
                        {
                            break;
                        }
                    }

                    Locked.TryAdd(typeOf, true);

                    locked = false;

                    return locked;
                }
            }
        }

        public static void ExitLock(string typeOf)
        {
            Locked.TryRemove(typeOf, out var locked);
        }
    }
}
