using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ReadWriteLockSlimDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var pages = GetPages();
            var write = new List<Page>();
            var random = new Random();
            string[] typeOfs = new string[] { "Pages", "Categories" };

            Parallel.ForEach(pages, page =>
            {
                var typeOf = typeOfs[random.Next(0, typeOfs.Length)];
                var trans = new Transaction();
                trans.MonitorEnter(typeOf, page.Id);
                //trans.EnterLock("pages", page.Id);
                write.Add(page);
                //trans.ExitLock("pages", page.Id);
                trans.MonitorExit(typeOf, page.Id);
            });

            var typeOfTransactions = Transaction.TypeOf;
            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }

        static List<Page> GetPages()
        {
            var pages = new List<Page>();
            for (int i = 0; i < 10; i++)
            {
                pages.Add(new Page { Id = i });
            }
            return pages;
        }
    }

    public class Page
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }

        public override string ToString()
        {
            return Date.ToString();
        }
    }

    public class Transaction
    {
        private int Timeout = 10000;
        private static ReaderWriterLockSlim readWriteLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        //private static List<string> TypeOf = new List<string>();
        public static Dictionary<string, int> TypeOf = new Dictionary<string, int>();

        public Transaction() { }

        public void MonitorEnter(string typeOf, int index)
        {
            Monitor.Enter(typeOf);
            TypeOf.Add(typeOf, index);
            Console.WriteLine($"Enter typeOf : {typeOf} - {index} - {DateTime.Now.Ticks}");
        }

        public void MonitorExit(string typeOf, int index)
        {
            TypeOf.Remove(typeOf);
            Monitor.Exit(typeOf);
            Console.WriteLine($"Exit typeOf : {typeOf} - {index}");
        }

        public void EnterLock(string typeOf, int index)
        {
            readWriteLock.TryEnterWriteLock(Timeout);
            TypeOf.Add(typeOf, index);
            Console.WriteLine($"Enter typeOf : {typeOf} - {index} - {DateTime.Now.Ticks}");
        }

        public void ExitLock(string typeOf, int index)
        {
            TypeOf.Remove(typeOf);
            readWriteLock.ExitWriteLock();
            Console.WriteLine($"Exit typeOf : {typeOf} - {index}");
        }
    }
}
